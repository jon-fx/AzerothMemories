using System.Net;

namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowToolsInternal
{
    private readonly string _buildFilePath;
    private readonly string _buildString;
    private readonly bool _throwIfNotFound;
    private readonly MoaResourceCache _resourceCache;
    private Dictionary<int, string> _listFile;

    public WowToolsInternal(string buildString, bool throwIfNotFound)
    {
        _buildString = buildString;
        _throwIfNotFound = throwIfNotFound;

        _buildFilePath = Path.Combine(SeederConfig.ToolsPath, _buildString);
        if (!Directory.Exists(_buildFilePath))
        {
            Directory.CreateDirectory(_buildFilePath);
        }

        _resourceCache = new MoaResourceCache(this, _buildFilePath);
    }

    public string BuildString => _buildString;

    public Version BuildVersion => Version.Parse(_buildString);

    public MoaResourceCache ResourceCache => _resourceCache;

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

    private Dictionary<int, string> GetListFile()
    {
        var dictionary = new Dictionary<int, string>();
        var fileInfo = DownloadIfNotExists("_list-file.csv", "https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile.csv");
        if (fileInfo == null)
        {
            return dictionary;
        }

        var allLines = File.ReadAllLines(fileInfo.FullName).ToArray();
        foreach (var line in allLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var split = line.Split(';');
            var result = int.TryParse(split[0], out var id);
            if (!result)
            {
                throw new NotImplementedException();
            }

            dictionary.Add(id, split[1]);
        }

        if (dictionary.Count < 1_500_000)
        {
            throw new NotImplementedException();
        }

        return dictionary;
    }

    private FileInfo DownloadIfNotExists(string fileName, string remotePath)
    {
        var filePath = Path.Combine(_buildFilePath, fileName);
        var fileInfo = new FileInfo(filePath);

        if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
        {
            fileInfo.Directory.Create();
        }

        if (!fileInfo.Exists || fileInfo.Length == 0)
        {
            using var webClient = new WebClient();
            webClient.Headers.Add("User-Agent: Other");
            try
            {
                webClient.DownloadFile(remotePath, fileInfo.FullName);
            }
            catch (WebException)
            {
                if (_throwIfNotFound)
                {
                    throw;
                }

                return null;
            }
        }

        return fileInfo;
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
        var fileInfo = DownloadIfNotExists($"{fileName}-{locale}.csv", $"http://localhost:5080/dbc/export/?name={fileName}&build={_buildString}&locale={locale}");
        if (fileInfo == null)
        {
            return;
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