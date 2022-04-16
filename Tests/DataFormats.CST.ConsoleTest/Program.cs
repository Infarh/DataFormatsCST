using System.IO.Compression;
using DataFormats.CST.BeamPattern;

const string data_file_path = "Patterns.zip";

var data_file = new FileInfo(data_file_path);
using var zip = new ZipArchive(data_file.OpenRead(), ZipArchiveMode.Read);

const string horn_3ghz_ffs = "Horn3GHz.ffs";
const string horn_braodband_ffs = "HornBroadBand.ffs";
var pattern_item = zip.GetEntry(horn_braodband_ffs) 
    ?? throw new InvalidOperationException("Не найден файл в архиве");

await using var horn_3ghz_stream = pattern_item.Open();
var ffs = await FFS.LoadAsync(horn_3ghz_stream);

Console.WriteLine("---");
