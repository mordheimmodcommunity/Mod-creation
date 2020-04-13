namespace WellFired.Shared
{
    public class OSXOpen : IOpen
    {
        public void OpenFolderToDisplayFile(string filePath)
        {
            filePath = "\"" + filePath + "\"";
            RuntimeProcessRunner runtimeProcessRunner = new RuntimeProcessRunner("open", $"-n -R {filePath}");
            runtimeProcessRunner.Execute();
        }
    }
}
