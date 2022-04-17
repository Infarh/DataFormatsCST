using System.Collections.Generic;
using System.Globalization;

using MathCore.Vectors;

namespace DataFormats.CST.Infrastructure;

internal static class ParseEx
{
    public static Vector3D ParseVector3D(this ReadOnlySpan<char> Str, char Separator = ' ')
    {
        var str = Str;

        var index = str.IndexOf(Separator);
        var x = double.Parse(str[..index], NumberStyles.Any, CultureInfo.InvariantCulture);

        str = str[(index + 1)..];
        index = str.IndexOf(Separator);
        var y = double.Parse(str[..index], NumberStyles.Any, CultureInfo.InvariantCulture);

        var z = double.Parse(str[(index + 1)..], NumberStyles.Any, CultureInfo.InvariantCulture);

        return new(x, y, z);
    }

    public static (double x, double y) ParseDoubleTuple2(this ReadOnlySpan<char> str, char Separator = ' ')
    {
        var index = str.IndexOf(Separator);
        var s = str[..index];
        var x = double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);

        var index2 = str[(index + 1)..].IndexOf(Separator);
        s = str[(index + 1)..index2];
        var y = double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);

        return (x, y);
    }

    public static (int x, int y) ParseInt32Tuple2(this ReadOnlySpan<char> str, char Separator = ' ')
    {
        var index = str.IndexOf(Separator);
        var s = str[..index];
        var x = int.Parse(s);

        var index2 = str[(index + 1)..].IndexOf(Separator);
        s = index2 < 0 ? str[(index + 1)..] : str[(index + 1)..index2];
        var y = int.Parse(s);

        return (x, y);
    }

    //public static IEnumerable<ReadOnlyMemory<char>> EnumStrings(this string Str, char Separator = ' ')
    //{
    //    var length = Str.Length;
    //    if (length == 0)
    //        yield break;

    //    var start = 0;
    //    while (start < length && Str[start] == Separator) start++;
    //    if (start == length)
    //        yield break;

    //    do
    //    {
    //        var end = start + 1;
    //        while (end < length && Str[end] != Separator) end++;

    //        var result = Str.AsMemory(start..end);
    //        yield return result;

    //        if (end == length)
    //            yield break;

    //        start = end;

    //        while (start < length && Str[start] == Separator) start++;
    //        if (start == length)
    //            yield break;
    //    }
    //    while (true);
    //}

    public static SubstringEnumerator EnumStrings(this string Str, char Separator = ' ') => new(Str, Separator);

    public struct SubstringEnumerator
    {
        private readonly string _Str;
        private readonly char _Separator;
        private int _Start;

        public SubstringEnumerator(string Str, char Separator)
        {
            _Str = Str;
            _Separator = Separator;
            Current = null!;
            _Start = 0;
        }

        public ReadOnlyMemory<char> Current { get; private set; }

        public bool MoveNext()
        {
            var length = _Str.Length;
            if (length == 0)
                return false;

            if (_Start == length)
                return false;

            if (_Start == 0)
            {
                while (_Start < length && _Str[_Start] == _Separator) _Start++;
                if (_Start == length)
                    return false;
            }
            else
            {
                while (_Start < length && _Str[_Start] == _Separator) _Start++;
                if (_Start == length)
                    return false;
            }

            var end = _Start + 1;
            while (end < length && _Str[end] != _Separator) end++;
            Current = _Str.AsMemory(_Start..end);
            _Start = end;
            return true;
        }
    }

    public static int ToInt32(this ReadOnlySpan<char> str) => int.Parse(str);
    public static int ToInt32(this Span<char> str) => int.Parse(str);

    public static double ToDouble(this ReadOnlySpan<char> str)
    {
        //var s = str.ToString();
        return double.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    public static double ToDouble(this Span<char> str) => double.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

    public static int ToInt32(this ReadOnlyMemory<char> str) => str.Span.ToInt32();
    public static int ToInt32(this Memory<char> str) => str.Span.ToInt32();
    public static double ToDouble(this ReadOnlyMemory<char> str) => str.Span.ToDouble();
    public static double ToDouble(this Memory<char> str) => str.Span.ToDouble();
}
