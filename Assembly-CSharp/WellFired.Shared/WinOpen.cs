namespace WellFired.Shared
{
    public class WinOpen : IOpen
    {
        public void OpenFolderToDisplayFile(string filePath)
        {
            filePath = "\"filePath\"";
            RuntimeProcessRunner runtimeProcessRunner = new RuntimeProcessRunner("explorer.exe", $"/select,{filePath}");
            runtimeProcessRunner.Execute();
        }
    }
}
