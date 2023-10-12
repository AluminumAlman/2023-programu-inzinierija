namespace AALKisShared;

public record class FolderRecord<T> where T : IJsonSerializable, new()
{
    public int Id { get; set; } = -1;

    public string Name { get; set; } = "";

    public List<T> Records { get; set; } = new List<T>();

    public FolderRecord() { }

    public void SetFromDirectory(string path, bool previewOnly = false)
    {
        static string GetLowestDirectoryName(string path)
        {
            return path.Split(new char[] {'\\', '/'})
                    .Last(str => str.Length > 0);
        }

        Name = GetLowestDirectoryName(path);

        Records = Directory.GetFiles(path)
            .Select<string, T>(filePath => {
                    T t = new T();
                    t.SetFromJsonFile(filePath, previewOnly);
                    return t; })
            .ToList();

        return;
    }
}

