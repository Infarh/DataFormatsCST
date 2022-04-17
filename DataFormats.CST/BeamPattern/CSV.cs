using DataFormats.CST.Infrastructure;

using MathCore;
using MathCore.Values;
using MathCore.Vectors;

namespace DataFormats.CST.BeamPattern;

public class CSV
{
    public readonly record struct PatternValue(SpaceAngle Angle, double Dir, (Complex Th, Complex Ph) E, double AxisRatio)
    {
        public double DirDb => Dir.In_dB_byPower();

        public double EThAbs => E.Th.Abs;
        /// <summary>10log10(<see cref="EThAbs"/>)</summary>
        public double EThAbsDb => EThAbs.In_dB_byPower();
        public double EThPhaseDeg => E.Th.Arg * Consts.ToDeg;

        public double EPhAbs => E.Ph.Abs;
        /// <summary>10log10(<see cref="EPhAbs"/>)</summary>
        public double EPhAbsDb => EPhAbs.In_dB_byPower();
        public double EPhPhaseDeg => E.Ph.Arg * Consts.ToDeg;

        public double Abs => Math.Sqrt(EThAbs.Pow2() + EPhAbs.Pow2());

        /// <summary>10log10(<see cref="Abs"/>)</summary>
        public double AbsDb => Abs.In_dB_byPower();

        /// <summary>Коэффициент эллиптичности в дБ 20log10(<see cref="AxisRatio"/>)</summary>
        public double AxisRatioDb => AxisRatio.In_dB();
    }

    private static ReadOnlyMemory<char> GetHeaderUnit(string HeaderLine, string ValueName)
    {
        var header_span = HeaderLine.AsMemory();

        var value_name_length = ValueName.Length;
        var index = header_span.Span.IndexOf(ValueName, StringComparison.OrdinalIgnoreCase);
        if (index < 0) throw new InvalidOperationException($"В строке заголовка не найден параметр {ValueName}")
        {
            Data =
            {
                { nameof(HeaderLine), HeaderLine },
                { nameof(ValueName), ValueName },
            }
        };

        header_span = header_span[index..];

        var open_bracket_index = header_span.Span.IndexOf('[');
        var result = header_span[(open_bracket_index + 1)..];

        var close_bracket_index = result.Span.IndexOf(']');
        result = result[..close_bracket_index];

        return result.Trim();
    }

    public static Task<CSV> LoadAsync(string FileName, CancellationToken Cancel = default) =>
        LoadAsync(new FileInfo(FileName), Cancel);

    public static async Task<CSV> LoadAsync(FileInfo DataFile, CancellationToken Cancel = default)
    {
        using var reader = DataFile.OpenText();
        return await LoadAsync(reader, Cancel).ConfigureAwait(false);
    }

    public static Task<CSV> LoadAsync(Stream DataStream, CancellationToken Cancel = default) =>
        LoadAsync(new StreamReader(DataStream), Cancel);

    public static async Task<CSV> LoadAsync(StreamReader Reader, CancellationToken Cancel = default)
    {
        await using var cancel_registration = Cancel.Register(o => ((StreamReader)o!).Dispose(), Reader);
        try
        {
            var line = await Reader.ReadLineAsync().ConfigureAwait(false)
                ?? throw new InvalidOperationException("Не удалось прочитать строку заголовка");

            var is_angle_in_deg = GetHeaderUnit(line, "Theta").Span.SequenceEqual("deg.");
            var is_amplitude_in_db = GetHeaderUnit(line, "Abs(Dir.)").Span.SequenceEqual("dBi");

            await Reader.ReadLineAsync();

            var values = new List<PatternValue>(100_000);
            var th_min_max = new MinMaxValue();
            var ph_min_max = new MinMaxValue();
            while (!Reader.EndOfStream)
            {
                line = await Reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;

                var str_reader = line.EnumStrings();
                str_reader.MoveNext();

                var read_only_memory = str_reader.Current;
                var th = read_only_memory.ToDouble().CorrectFromDeg(is_angle_in_deg);
                str_reader.MoveNext();

                var ph = str_reader.Current.ToDouble().CorrectFromDeg(is_angle_in_deg);
                str_reader.MoveNext();

                th_min_max.AddValue(th);
                ph_min_max.AddValue(ph);
                var angle = new SpaceAngle(th, ph, AngleType.Deg);

                var abs = str_reader.Current.ToDouble().CorrectFromDbP(is_amplitude_in_db);
                str_reader.MoveNext();

                var abs_th = str_reader.Current.ToDouble().CorrectFromDbP(is_amplitude_in_db);

                var arg_th = str_reader.Current.ToDouble().CorrectFromDeg(is_angle_in_deg);
                str_reader.MoveNext();
                var abs_ph = str_reader.Current.ToDouble().CorrectFromDbP(is_amplitude_in_db);
                str_reader.MoveNext();
                var arg_ph = str_reader.Current.ToDouble().CorrectFromDeg(is_angle_in_deg);
                str_reader.MoveNext();

                var ax_ratio = str_reader.Current.ToDouble().CorrectFromDb(is_amplitude_in_db);

                var e_th = Complex.Exp(abs_th, arg_th * Consts.ToRad);
                var e_ph = Complex.Exp(abs_ph, arg_ph * Consts.ToRad);
                values.Add(new(angle, abs, (e_th, e_ph), ax_ratio));
            }

            return new()
            {
                Pattern = values.ToArray(),
                ThetaInterval = th_min_max,
                PhiInterval = ph_min_max,
            };
        }
        catch (FormatException e)
        {
            throw new InvalidOperationException("Ошибка формата файла данных", e);
        }
        catch (ObjectDisposedException e) when (Cancel.IsCancellationRequested)
        {
            throw new OperationCanceledException("Операция чтения отменена", e);
        }
    }

    public IReadOnlyList<PatternValue> Pattern { get; private init; }

    public Interval ThetaInterval { get; private init; }
    public Interval PhiInterval { get; private init; }

    private CSV() { }
}
