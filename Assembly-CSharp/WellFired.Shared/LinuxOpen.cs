namespace WellFired.Shared
{
	public class LinuxOpen : IOpen
	{
		public void OpenFolderToDisplayFile(string filePath)
		{
			filePath = "\"filePath\"";
			RuntimeProcessRunner runtimeProcessRunner = new RuntimeProcessRunner("nautilus", $"{filePath}");
			runtimeProcessRunner.Execute();
		}
	}
}
