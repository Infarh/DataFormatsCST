namespace DataFormats.CST.ConsoleTest;

public static class ProgressControl
{
    public static IProgress<double> Create(Action<double> Progress) => new Progress<double>(Progress);

    public static (CancellationTokenSource Cancellation, IProgress<double> Progress) CreateWithCancellation(Action<double> Progress) =>
        (new(), new Progress<double>(Progress));
}
