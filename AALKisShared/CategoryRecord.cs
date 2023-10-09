namespace AALKisShared;

public record class CategoryRecord<T> where T : IJsonSerializable, new()
{
    public string Name { get; set; }

    public List<T> Records { get; set; }

    public CategoryRecord()
    {
        Name = "";
        Records = new List<T>();
    }

    public void SetFromDirectory(string path, bool previewOnly = false)
    {
        string GetLowestDirectoryName(string path)
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

