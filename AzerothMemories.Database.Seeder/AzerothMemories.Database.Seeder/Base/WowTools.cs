﻿using System.Net;

namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowTools
{
    private const string Build = "9.1.5.41323";

    private Dictionary<int, string> _listFile;

    public bool TryGetIconName(int iconId, out string iconName)
    {
        _listFile ??= GetListFile();

        if (_listFile.TryGetValue(iconId, out iconName))
        {
            var split = iconName.Split('/');
            iconName = split[^1].Split('.')[0];

            return true;
        }

        return false;
    }

    private static Dictionary<int, string> GetListFile()
    {
        var filePath = @"C:\Users\John\Desktop\Stuff\BlizzardData\listfile.csv";
        var dictionary = new Dictionary<int, string>();
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            throw new NotImplementedException();
        }

        var lines = File.ReadAllLines(fileInfo.FullName);
        foreach (var line in lines)
        {
            var split = line.Split(';');
            var result = int.TryParse(split[0], out var id);
            if (!result)
            {
                throw new NotImplementedException();
            }

            dictionary.Add(id, split[1]);
        }

        return dictionary;
    }

    public void LoadDataFromWowTools(string fileName, string primaryKeyName, ref Dictionary<int, WowToolsData> dictionary, string[] fieldsToLoad = null)
    {
        foreach (var locale in WowToolsData.AllLocales)
        {
            LoadDataFromWowTools(fileName, primaryKeyName, ref dictionary, locale, fieldsToLoad);
        }
    }

    public void LoadDataFromWowTools(string fileName, string primaryKeyName, ref Dictionary<int, WowToolsData> dictionary, string locale, string[] fieldsToLoad = null)
    {
        var filePath = @$"C:\Users\John\Desktop\Stuff\BlizzardData\Tools\{fileName}-{locale}.csv";
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists || fileInfo.Length == 0)
        {
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            using var webClient = new WebClient();
            webClient.Headers.Add("User-Agent: Other");
            webClient.DownloadFile($"https://wow.tools/dbc/api/export/?name={fileName}&build={Build}&locale={locale}", filePath);
        }

        using var stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var streamReader = new StreamReader(stream);
        using var csvReader = CsvHelpers.GetReader(streamReader);

        csvReader.Read();
        csvReader.ReadHeader();

        while (csvReader.Read())
        {
            var result = csvReader.TryGetField<int>(primaryKeyName, out var id);
            if (!result)
            {
                throw new NotImplementedException();
            }

            if (!dictionary.TryGetValue(id, out var value))
            {
                dictionary.Add(id, value = new WowToolsData(id));
            }

            if (fieldsToLoad == null)
            {
                foreach (var headerStr in csvReader.HeaderRecord)
                {
                    var header = headerStr;
                    if (!csvReader.TryGetField<string>(header, out var fieldValue))
                    {
                        throw new NotImplementedException();
                    }

                    value.TryAdd(header, locale, fieldValue);
                }
            }
            else
            {
                foreach (var headerStr in fieldsToLoad)
                {
                    var header = headerStr;
                    if (!csvReader.TryGetField<string>(header, out var fieldValue))
                    {
                        throw new NotImplementedException();
                    }

                    value.TryAdd(header, locale, fieldValue);
                }
            }
        }
    }
}