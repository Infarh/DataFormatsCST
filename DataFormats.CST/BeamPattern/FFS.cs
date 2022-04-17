using System.Globalization;
using System.Runtime.CompilerServices;

using DataFormats.CST.Infrastructure;

using MathCore;
using MathCore.Vectors;

namespace DataFormats.CST.BeamPattern;

public class FFS
{
    /// <summary> Информация о частотной составляющей</summary>
    /// <param name="Frequency">Частота Гц</param>
    /// <param name="Radiated">Излучённая мощность</param>
    /// <param name="Accepted">Поглощённая мощность</param>
    /// <param name="StimulatedPower">Мощность источника (генератора)</param>
    public readonly record struct FrequencyInfo(double Frequency, double Radiated, double Accepted, double StimulatedPower);

    public readonly record struct PatternValue(SpaceAngle Angle, (Complex Th, Complex Ph) E)
    {
        public Complex EValue => new(Math.Sqrt(E.Th.Re * E.Th.Re + E.Ph.Re * E.Ph.Re), Math.Sqrt(E.Th.Im * E.Th.Im + E.Ph.Im * E.Ph.Im));
    }

    public readonly record struct Pattern(FrequencyInfo Frequency, IReadOnlyList<PatternValue> Values);

    public static IEnumerable<(FrequencyInfo PatternInfo, PatternValue Value)> EnumValues(string FilePath) => EnumValues(new FileInfo(FilePath));
    public static IEnumerable<(FrequencyInfo PatternInfo, PatternValue Value)> EnumValues(FileInfo File)
    {
        using var reader = File.OpenText();
        foreach (var value in EnumValues(reader))
            yield return value;
    }

    public static IEnumerable<(FrequencyInfo PatternInfo, PatternValue Value)> EnumValues(Stream DataStream) => EnumValues(new StreamReader(DataStream));

    public static IEnumerable<(FrequencyInfo PatternInfo, PatternValue Value)> EnumValues(StreamReader Reader)
    {
        var line = Reader.ReadLine();

        const string start_line = "// CST Farfield Source File";
        if (line != start_line)
            throw new InvalidOperationException("Неверный формат файла - некорректная первая строка файла");

        //string? version = null;
        //string? data_type = null;
        var frequency_infos_list = new List<FrequencyInfo>();
        //Vector3D position = default;
        //Vector3D x_axis = default;
        //Vector3D y_axis = default;
        //Vector3D z_axis = default;
        var current_frequency_index = 0;
        FrequencyInfo current_frequency_info = default;
        while (!Reader.EndOfStream)
        {
            line = Reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (Reader.EndOfStream) throw new InvalidOperationException("Неожиданный конец файла");
            switch (line.Trim())
            {
                case "// Version:":
                    //version = Reader.ReadLine();
                    _ = Reader.ReadLine();

                    line = Reader.ReadLine();
                    if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                    break;

                case "// Data Type":
                    //data_type = Reader.ReadLine();
                    _ = Reader.ReadLine();

                    line = Reader.ReadLine();
                    if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                    break;

                case "// #Frequencies":
                    {
                        line = Reader.ReadLine()
                            ?? throw new FormatException("Отсутствует строка с числом частотных составляющих");
                        var frequencies_count = int.Parse(line.Trim());
                        frequency_infos_list.Capacity = frequencies_count;

                        line = Reader.ReadLine();
                        if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                    }
                    break;

                case "// Position":
                    //line = Reader.ReadLine()
                    _ = Reader.ReadLine()
                        ?? throw new FormatException("Отсутствует строка с положением фазового центра");
                    //position = line.AsSpan().ParseVector3D();

                    line = Reader.ReadLine();
                    if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                    break;

                case "// zAxis":
                    //line = Reader.ReadLine()
                    _ = Reader.ReadLine()
                        ?? throw new FormatException("Отсутствует строка zAxis");
                    //z_axis = line.AsSpan().ParseVector3D();

                    line = Reader.ReadLine();
                    if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                    break;

                case "// yAxis":
                    //line = Reader.ReadLine()
                    _ = Reader.ReadLine()
                        ?? throw new FormatException("Отсутствует строка yAxis");
                    //y_axis = line.AsSpan().ParseVector3D();

                    line = Reader.ReadLine();
                    if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                    break;

                case "// xAxis":
                    //line = Reader.ReadLine()
                    _ = Reader.ReadLine()
                        ?? throw new FormatException("Отсутствует строка xAxis");
                    //x_axis = line.AsSpan().ParseVector3D();

                    line = Reader.ReadLine();
                    if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                    break;

                case "// Radiated/Accepted/Stimulated Power , Frequency":
                    for (line = Reader.ReadLine(); !string.IsNullOrWhiteSpace(line); line = Reader.ReadLine())
                    {
                        var radiated = double.Parse(line, CultureInfo.InvariantCulture);

                        line = Reader.ReadLine()
                            ?? throw new FormatException("Не определена строка с поглощённой мощностью");
                        var accepted = double.Parse(line, CultureInfo.InvariantCulture);

                        line = Reader.ReadLine()
                            ?? throw new FormatException("Не определена строка с мощностью генератора");
                        var stimulated = double.Parse(line, CultureInfo.InvariantCulture);

                        line = Reader.ReadLine()
                            ?? throw new FormatException("Не определена строка со значением частоты");
                        var frequency = double.Parse(line, CultureInfo.InvariantCulture);

                        line = Reader.ReadLine();
                        if (line is not { Length: 0 })
                            throw new FormatException("После блока параметров частотной составляющей отсутствует строка-разделитель");

                        frequency_infos_list.Add(new(frequency, radiated, accepted, stimulated));
                    }
                    frequency_infos_list.TrimExcess();
                    current_frequency_info = frequency_infos_list[0];
                    break;

                case "// >> Total #phi samples, total #theta samples":
                    {
                        //line = Reader.ReadLine()
                        _ = Reader.ReadLine()
                            ?? throw new FormatException("Отсутствует строка с количеством отсчётов ДН");
                        //var (phi_count, theta_count) = line.AsSpan().ParseInt32Tuple2();

                        line = Reader.ReadLine();
                        if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                    }
                    break;

                case "// >> Phi, Theta, Re(E_Theta), Im(E_Theta), Re(E_Phi), Im(E_Phi):":
                    {
                        for (line = Reader.ReadLine(); !string.IsNullOrWhiteSpace(line); line = Reader.ReadLine())
                        {
                            var str_reader = line.EnumStrings();
                            str_reader.MoveNext();

                            var ph = str_reader.Current.ToDouble();
                            str_reader.MoveNext();
                            var th = str_reader.Current.ToDouble();
                            str_reader.MoveNext();
                            var e_re_th = str_reader.Current.ToDouble();
                            str_reader.MoveNext();
                            var e_im_th = str_reader.Current.ToDouble();
                            str_reader.MoveNext();
                            var e_re_ph = str_reader.Current.ToDouble();
                            str_reader.MoveNext();
                            var e_im_ph = str_reader.Current.ToDouble();

                            var pattern_value = new PatternValue(new(ph, th, AngleType.Deg), (new(e_re_th, e_im_th), new(e_re_ph, e_im_ph)));
                            yield return (current_frequency_info, pattern_value);
                        }

                        current_frequency_index++;
                        if (current_frequency_index >= frequency_infos_list.Count)
                            yield break;

                        current_frequency_info = frequency_infos_list[current_frequency_index];
                    }
                    break;
            }
        }
    }

    public static Task<FFS> LoadAsync(string FileName, CancellationToken Cancel = default) =>
        LoadAsync(new FileInfo(FileName), Cancel);

    public static async Task<FFS> LoadAsync(FileInfo DataFile, CancellationToken Cancel = default)
    {
        using var reader = DataFile.OpenText();
        return await LoadAsync(reader, Cancel).ConfigureAwait(false);
    }

    public static Task<FFS> LoadAsync(Stream DataStream, CancellationToken Cancel = default) =>
        LoadAsync(new StreamReader(DataStream), Cancel);

    public static async Task<FFS> LoadAsync(StreamReader Reader, CancellationToken Cancel = default)
    {
        try
        {
            await using var cancellation_registration = Cancel.Register(o => ((StreamReader)o!).Dispose(), Reader);
            var line = await Reader.ReadLineAsync().ConfigureAwait(false);

            const string start_line = "// CST Farfield Source File";
            if (line != start_line)
                throw new InvalidOperationException("Неверный формат файла - некорректная первая строка файла");

            string? version = null;
            string? data_type = null;
            var frequency_infos_list = new List<FrequencyInfo>();
            Vector3D position = default;
            Vector3D x_axis = default;
            Vector3D y_axis = default;
            Vector3D z_axis = default;
            var patterns = new List<Pattern>();
            List<PatternValue> values = new();
            while (!Reader.EndOfStream)
            {
                Cancel.ThrowIfCancellationRequested();

                line = await Reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (Reader.EndOfStream) throw new InvalidOperationException("Неожиданный конец файла");
                switch (line.Trim())
                {
                    case "// Version:":
                        version = await Reader.ReadLineAsync();

                        line = await Reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                        break;

                    case "// Data Type":
                        data_type = await Reader.ReadLineAsync();

                        line = await Reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                        break;

                    case "// #Frequencies":
                        {
                            line = await Reader.ReadLineAsync()
                                ?? throw new FormatException("Отсутствует строка с числом частотных составляющих");
                            var frequencies_count = int.Parse(line.Trim());
                            frequency_infos_list.Capacity = frequencies_count;

                            line = await Reader.ReadLineAsync();
                            if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                        }
                        break;

                    case "// Position":
                        line = await Reader.ReadLineAsync()
                            ?? throw new FormatException("Отсутствует строка с положением фазового центра");
                        position = line.AsSpan().ParseVector3D();

                        line = await Reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                        break;

                    case "// zAxis":
                        line = await Reader.ReadLineAsync()
                            ?? throw new FormatException("Отсутствует строка zAxis");
                        z_axis = line.AsSpan().ParseVector3D();

                        line = await Reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                        break;

                    case "// yAxis":
                        line = await Reader.ReadLineAsync()
                            ?? throw new FormatException("Отсутствует строка yAxis");
                        y_axis = line.AsSpan().ParseVector3D();

                        line = await Reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                        break;

                    case "// xAxis":
                        line = await Reader.ReadLineAsync()
                            ?? throw new FormatException("Отсутствует строка xAxis");
                        x_axis = line.AsSpan().ParseVector3D();

                        line = await Reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                        break;

                    case "// Radiated/Accepted/Stimulated Power , Frequency":
                        for (line = await Reader.ReadLineAsync(); !string.IsNullOrWhiteSpace(line); line = await Reader.ReadLineAsync())
                        {
                            var radiated = double.Parse(line, CultureInfo.InvariantCulture);

                            line = await Reader.ReadLineAsync()
                                ?? throw new FormatException("Не определена строка с поглощённой мощностью");
                            var accepted = double.Parse(line, CultureInfo.InvariantCulture);

                            line = await Reader.ReadLineAsync()
                                ?? throw new FormatException("Не определена строка с мощностью генератора");
                            var stimulated = double.Parse(line, CultureInfo.InvariantCulture);

                            line = await Reader.ReadLineAsync()
                                ?? throw new FormatException("Не определена строка со значением частоты");
                            var frequency = double.Parse(line, CultureInfo.InvariantCulture);

                            line = await Reader.ReadLineAsync();
                            if (line is not { Length: 0 })
                                throw new FormatException("После блока параметров частотной составляющей отсутствует строка-разделитель");

                            frequency_infos_list.Add(new(frequency, radiated, accepted, stimulated));
                        }
                        frequency_infos_list.TrimExcess();
                        patterns.Capacity = frequency_infos_list.Count;
                        break;

                    case "// >> Total #phi samples, total #theta samples":
                        {
                            line = await Reader.ReadLineAsync()
                                ?? throw new FormatException("Отсутствует строка с количеством отсчётов ДН");
                            var (phi_count, theta_count) = line.AsSpan().ParseInt32Tuple2();
                            values.Capacity = phi_count * theta_count;

                            line = await Reader.ReadLineAsync();
                            if (!string.IsNullOrEmpty(line)) throw new FormatException("Отсутствует строка-разделитель");
                        }
                        break;

                    case "// >> Phi, Theta, Re(E_Theta), Im(E_Theta), Re(E_Phi), Im(E_Phi):":
                        {
                            values.Clear();
                            for (line = await Reader.ReadLineAsync(); !string.IsNullOrWhiteSpace(line); line = await Reader.ReadLineAsync())
                            {
                                var str_reader = line.EnumStrings();
                                str_reader.MoveNext();

                                var ph = str_reader.Current.ToDouble();
                                str_reader.MoveNext();
                                var th = str_reader.Current.ToDouble();
                                str_reader.MoveNext();
                                var e_re_th = str_reader.Current.ToDouble();
                                str_reader.MoveNext();
                                var e_im_th = str_reader.Current.ToDouble();
                                str_reader.MoveNext();
                                var e_re_ph = str_reader.Current.ToDouble();
                                str_reader.MoveNext();
                                var e_im_ph = str_reader.Current.ToDouble();

                                values.Add(new(new(ph, th, AngleType.Deg), (new(e_re_th, e_im_th), new(e_re_ph, e_im_ph))));
                            }
                            patterns.Add(new(frequency_infos_list[patterns.Count], values.ToArray()));
                        }
                        break;
                }
            }

            frequency_infos_list.TrimExcess();
            patterns.TrimExcess();

            return new()
            {
                Version = version ?? throw new FormatException("Не определена версия"),
                DataType = data_type ?? throw new FormatException("Не определён тип данных"),
                Position = position,
                xAxes = x_axis,
                yAxes = y_axis,
                zAxes = z_axis,
                Patterns = patterns,
            };
        }
        catch (FormatException e)
        {
            throw new InvalidOperationException("Ошибка формата файла данных", e);
        }
        catch (ObjectDisposedException e) when (Cancel.IsCancellationRequested)
        {
            throw new OperationCanceledException("Операция была прервана", e, Cancel);
        }
    }

    public string Version { get; private init; } = null!;
    public string DataType { get; private init; } = null!;
    public Vector3D Position { get; private init; }
    public Vector3D xAxes { get; private init; }
    public Vector3D yAxes { get; private init; }
    public Vector3D zAxes { get; private init; }
    public IReadOnlyList<Pattern> Patterns { get; private init; } = null!;

    private FFS() { }
}
