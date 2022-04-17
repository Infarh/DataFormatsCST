using System.IO.Compression;
using DataFormats.CST.BeamPattern;

namespace DataFormats.CST.ConsoleTest;

public static class CSVTest
{
    public static async Task RunAsync()
    {
        const string data_file_path = "Patterns.csv.zip";

        var data_file = new FileInfo(data_file_path);
        using var zip = new ZipArchive(data_file.OpenRead(), ZipArchiveMode.Read);

        const string reflector_pattern_csv = "ReflectorPattern2D.txt";
        var pattern_item = zip.GetEntry(reflector_pattern_csv)
            ?? throw new InvalidOperationException("Не найден файл в архиве");

        await using var reflector_pattern_stream = pattern_item.Open();
        var pattern = await CSV.LoadAsync(reflector_pattern_stream);

        var ph90 = pattern.Pattern.Where(p => p.Angle.Phi == 90).ToArray();
    }
}
