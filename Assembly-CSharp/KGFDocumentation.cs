using System.IO;
using UnityEngine;

public class KGFDocumentation : MonoBehaviour
{
    public void OpenDocumentation()
    {
        string dataPath = Application.dataPath;
        dataPath = Path.Combine(dataPath, "kolmich");
        dataPath = Path.Combine(dataPath, "documentation");
        dataPath = Path.Combine(dataPath, "files");
        dataPath += Path.DirectorySeparatorChar;
        dataPath += "documentation.html";
        dataPath.Replace('/', Path.DirectorySeparatorChar);
        Application.OpenURL("file://" + dataPath);
    }
}
