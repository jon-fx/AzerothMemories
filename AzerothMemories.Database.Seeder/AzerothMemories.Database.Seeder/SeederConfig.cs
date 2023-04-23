namespace AzerothMemories.Database.Seeder;

public static class SeederConfig
{
    public static string RootPath = "C:\\Users\\John\\Desktop\\Stuff\\BlizzardData";

    public static string ToolsPath = Path.Combine(RootPath, "AzerothMemories.Tools");

    public static string JsonDataPath = Path.Combine(RootPath, "AzerothMemories.Data");

    public static string MediaDataPath = Path.Combine(RootPath, "AzerothMemories.Media");

    public static FileInfo GetLocalMediaFileInfo(string mediaPath)
    {
        var localPath = Path.Combine(MediaDataPath, $"{mediaPath}.jpg");
        return new FileInfo(localPath);
    }

    public static async Task<Dictionary<string, BlizzardData>> LoadAllJsonRecords()
    {
        var serverSideResources = new Dictionary<string, BlizzardData>();
        var files = Directory.EnumerateFiles(JsonDataPath, "*-*.json");
        foreach (var file in files)
        {
            await using var stream = File.OpenRead(file);
            var blizzardDataSet = JsonSerializer.Deserialize<BlizzardData[]>(stream);

            foreach (var blizzardData in blizzardDataSet)
            {
                serverSideResources.Add(blizzardData.Key, blizzardData);
            }
        }

        return serverSideResources;
    }
}