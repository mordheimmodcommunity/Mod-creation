using System;
using System.IO;
using System.Text;
using UnityEngine;

public class KGFDebugFile : KGFModule, KGFIDebug
{
	public KGFDataDebugFile itsDataDebugFile = new KGFDataDebugFile();

	public KGFDebugFile()
		: base(new Version(1, 0, 0, 1), new Version(1, 1, 0, 0))
	{
		if (itsDataDebugFile.itsFilePath == string.Empty)
		{
			itsDataDebugFile.itsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "KGFLog.txt");
		}
	}

	protected override void KGFAwake()
	{
		base.KGFAwake();
		if (itsDataDebugFile.itsFilePath == string.Empty)
		{
			itsDataDebugFile.itsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "KGFLog.txt");
		}
		using (StreamWriter streamWriter = new StreamWriter(itsDataDebugFile.itsFilePath, append: false, Encoding.ASCII))
		{
			streamWriter.WriteLine(string.Empty.PadLeft("FileLogger started: ".Length + DateTime.Now.ToString().Length, '='));
		}
		KGFDebug.AddLogger(this);
	}

	public override string GetName()
	{
		return "KGFDebugFile";
	}

	public override string GetDocumentationPath()
	{
		return "KGFDebugFile_Manual.html";
	}

	public override string GetForumPath()
	{
		return "index.php?qa=kgfdebug";
	}

	public override Texture2D GetIcon()
	{
		return null;
	}

	public void SetLogFilePath(string thePath)
	{
		itsDataDebugFile.itsFilePath = thePath;
	}

	public void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage)
	{
		Log(theLevel, theCategory, theMessage, string.Empty, null);
	}

	public void Log(KGFDebug.KGFDebugLog aLog)
	{
		Log(aLog.GetLevel(), aLog.GetCategory(), aLog.GetMessage(), aLog.GetStackTrace(), aLog.GetObject() as MonoBehaviour);
	}

	public void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage, string theStackTrace)
	{
		Log(theLevel, theCategory, theMessage, theStackTrace, null);
	}

	public void Log(KGFeDebugLevel theLevel, string theCategory, string theMessage, string theStackTrace, MonoBehaviour theObject)
	{
		try
		{
			using (StreamWriter streamWriter = new StreamWriter(itsDataDebugFile.itsFilePath, append: true, Encoding.ASCII))
			{
				if (theObject != null)
				{
					streamWriter.WriteLine("{0}{6}{1}{6}{2}{6}{3}{6}{4}{6}{5}", DateTime.Now.ToString(), theLevel, theCategory, theMessage, theObject.name, theStackTrace, itsDataDebugFile.itsSeparator);
				}
				else
				{
					streamWriter.WriteLine("{0}{6}{1}{6}{2}{6}{3}{6}{4}{6}{5}", DateTime.Now.ToString(), theLevel, theCategory, theMessage, string.Empty, theStackTrace, itsDataDebugFile.itsSeparator);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("couldn't write to file " + itsDataDebugFile.itsFilePath + ". " + ex.Message);
		}
	}

	public void SetMinimumLogLevel(KGFeDebugLevel theLevel)
	{
		itsDataDebugFile.itsMinimumLogLevel = theLevel;
	}

	public KGFeDebugLevel GetMinimumLogLevel()
	{
		return itsDataDebugFile.itsMinimumLogLevel;
	}

	public override KGFMessageList Validate()
	{
		KGFMessageList kGFMessageList = new KGFMessageList();
		if (itsDataDebugFile.itsSeparator.Length == 0)
		{
			kGFMessageList.AddInfo("no seperator is set");
		}
		if (itsDataDebugFile.itsFilePath == string.Empty)
		{
			kGFMessageList.AddInfo("no file path set. path will be set to desktop.");
		}
		else if (!Directory.Exists(Path.GetDirectoryName(itsDataDebugFile.itsFilePath)))
		{
			kGFMessageList.AddError("the current directory doesn`t exist");
		}
		return kGFMessageList;
	}
}
