using System;

[Serializable]
public class KGFDataDebugFile
{
	public KGFeDebugLevel itsMinimumLogLevel = KGFeDebugLevel.eError;

	public string itsSeparator = ";";

	public string itsFilePath = string.Empty;
}
