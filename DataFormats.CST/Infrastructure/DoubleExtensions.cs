using MathCore;

namespace DataFormats.CST.Infrastructure;

internal static class DoubleExtensions
{
    public static double CorrectFromDbP(this double value, bool IsInDb) => IsInDb ? value.From_dB_byPower() : value;
    public static double CorrectFromDb(this double value, bool IsInDb) => IsInDb ? value.From_dB() : value;

    public static double CorrectFromDeg(this double value, bool IsInDeg) => IsInDeg ? value : value * Consts.ToDeg;
}
