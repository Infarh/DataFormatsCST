namespace DataFormats.CST.Infrastructure;

internal struct SubstringEnumerator
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
