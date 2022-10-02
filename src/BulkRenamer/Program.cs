using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Toenda.Foundation;
using ExifLib;

namespace BulkRenamer;

public static class Extensions
{
	public static T? GetExifValue<T>(this ExifReader exif, ExifTags tag)
	{
		T value;
		if (exif.GetTagValue(tag, out value))
		{
			return value;
		}

		return default;
	}
}

public class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Bulk Renamer");

		for (int i = 0; i < Console.WindowWidth; i++)
		{
			Console.Write("-");
		}

		Console.WriteLine();
		Console.WriteLine();

		string sourceFolder = @"D:\Temp\Fotobuch";
		string targetFolder = @"D:\Temp\Fotobuch2";

		if (!Directory.Exists(targetFolder))
		{
			Directory.CreateDirectory(targetFolder);
		}

		DirectoryInfo info = new DirectoryInfo(sourceFolder);

		int filesFoundForCopy = 0;

		foreach (DirectoryInfo dir in info.GetDirectories())
		{
			Console.WriteLine($"Entering folder {dir.Name}");

			string targetSubFolder = Path.GetFullPath(dir.Name, targetFolder);

			if (!Directory.Exists(targetSubFolder))
			{
				Directory.CreateDirectory(targetSubFolder);
			}

			FileInfo[] files = dir.GetFiles();

			foreach (FileInfo file in files)
			{
				if (!file.Name.StartsWith("."))
				{
					filesFoundForCopy++;
					bool hasExifData = true;

					try
					{
						var exif = new ExifReader(file.FullName);

						var dateTime = exif.GetExifValue<DateTime>(ExifTags.DateTime);

						if (dateTime != default)
						{
							var dateTimeDigitized = exif.GetExifValue<DateTime>(ExifTags.DateTimeDigitized);
							var dateTimeOriginal = exif.GetExifValue<DateTime>(ExifTags.DateTimeOriginal);

							Console.WriteLine($"   Found file: {file.Name} [{dateTime}] - [{dateTimeDigitized}] - [{dateTimeOriginal}]");

							if (dateTime != dateTimeDigitized)
							{
								Console.WriteLine($"      ---> Difference digi");
							}
							if (dateTime != dateTimeOriginal)
							{
								Console.WriteLine($"      ---> Difference org");
							}
							if (dateTimeDigitized != dateTimeOriginal)
							{
								Console.WriteLine($"      ---> Difference digi & org");
							}

							string newFilename = $"{dateTime.Year}" +
								$".{dateTime.Month.ToString().PadLeft(2, '0')}" +
								$".{dateTime.Day.ToString().PadLeft(2, '0')}" +
								$" {dateTime.Hour.ToString().PadLeft(2, '0')}" +
								$"-{file.CreationTime.Minute.ToString().PadLeft(2, '0')}" +
								$"-{file.CreationTime.Second.ToString().PadLeft(2, '0')}" +
								$" - Alt [{Path.GetFileNameWithoutExtension(file.Name)}]" +
								$"{Path.GetExtension(file.Name)}";
							Console.WriteLine($"      ---> [ {Path.GetFullPath(newFilename, targetSubFolder)} ]");

							File.Copy(
								file.FullName,
								Path.GetFullPath(newFilename, targetSubFolder)
							);
						}
						else
						{
							hasExifData = false;
						}
					}
					catch
					{
						hasExifData = false;
					}

					if (!hasExifData)
					{
						string tryFindDate = Path.GetFileNameWithoutExtension(file.Name.Substring(9));
						DateTime dateTime = file.CreationTime;

						if (tryFindDate.ToLower() != "jpg"
							&& tryFindDate.ToLower() != "jpeg"
							&& tryFindDate.Length > 0)
						{
							dateTime = DateTime.Parse(tryFindDate);
						}

						Console.WriteLine($"   Found file: {file.Name} [{file.CreationTime}] - [{file.LastWriteTime}] - [---KEIN EXIF---]");
						string newFilename = $"{dateTime.Year}" +
								$".{dateTime.Month.ToString().PadLeft(2, '0')}" +
								$".{dateTime.Day.ToString().PadLeft(2, '0')}" +
								$" {dateTime.Hour.ToString().PadLeft(2, '0')}" +
								$"-{dateTime.Minute.ToString().PadLeft(2, '0')}" +
								$"-{dateTime.Second.ToString().PadLeft(2, '0')}" +
								$" - Alt [{Path.GetFileNameWithoutExtension(file.Name)}]" +
								$"{Path.GetExtension(file.Name)}";
						Console.WriteLine($"      ---> [ {Path.GetFullPath(newFilename, targetSubFolder)} ]");

						File.Copy(
							file.FullName,
							Path.GetFullPath(newFilename, targetSubFolder)
						);
					}
				}
			}
		}

		int filesCopied = 0;
		DirectoryInfo infoCheck = new DirectoryInfo(targetFolder);

		foreach (DirectoryInfo dir in infoCheck.GetDirectories())
		{
			FileInfo[] files = dir.GetFiles();

			foreach (FileInfo file in files)
			{
				if (!file.Name.StartsWith("."))
				{
					filesCopied++;
				}
			}
		}

		Console.WriteLine($"Files found for copy: {filesFoundForCopy}");
		Console.WriteLine($"Files copied: {filesCopied}");

		Console.WriteLine();
		Console.Read();
	}
}
