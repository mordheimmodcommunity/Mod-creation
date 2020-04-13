using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoad
{
    public static bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }

    public static void Delete(string filePath)
    {
        if (Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static void Save(string filePath, object data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        Stream stream = File.Open(filePath, FileMode.Create);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Binder = new VersionDeserializationBinderEdge();
        binaryFormatter.Serialize(stream, data);
        stream.Close();
    }

    public static T Load<T>(string filePath)
    {
        Stream stream = File.Open(filePath, FileMode.Open);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Binder = new VersionDeserializationBinderEdge();
        T result = (T)binaryFormatter.Deserialize(stream);
        stream.Close();
        return result;
    }
}
