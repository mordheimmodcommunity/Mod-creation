using System;
using System.Diagnostics;
using UnityEngine;

public class RuntimeProcessRunner
{
	private static readonly int DEFAULT_TIMEOUT = 40000;

	private static readonly string DEFAULT_WORKING_DIRECTORY = string.Empty;

	private string mCommand;

	private string mExecutable;

	private string mWorkingDirectory;

	private int mTimeoutMs;

	private Process mProcess;

	public Action OnProcessSuccesful
	{
		get;
		set;
	}

	public Action OnProcessFailed
	{
		get;
		set;
	}

	public bool IsComplete
	{
		get;
		private set;
	}

	public RuntimeProcessRunner(string executable, string args, string workingDirectory, int timeoutMs)
	{
		OnProcessSuccesful = delegate
		{
		};
		OnProcessFailed = delegate
		{
		};
		mExecutable = executable;
		mCommand = args;
		mWorkingDirectory = workingDirectory;
		mTimeoutMs = timeoutMs;
	}

	public RuntimeProcessRunner(string executable, string args)
	{
		OnProcessSuccesful = delegate
		{
		};
		OnProcessFailed = delegate
		{
		};
		mExecutable = executable;
		mCommand = args;
		mWorkingDirectory = DEFAULT_WORKING_DIRECTORY;
		mTimeoutMs = DEFAULT_TIMEOUT;
	}

	public void Execute()
	{
		try
		{
			mProcess = new Process
			{
				StartInfo = 
				{
					UseShellExecute = false,
					FileName = mExecutable,
					Arguments = mCommand,
					WorkingDirectory = mWorkingDirectory,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};
			mProcess.OutputDataReceived += delegate(object p, DataReceivedEventArgs o)
			{
				if (!string.IsNullOrEmpty(o.Data))
				{
					OutputLineReceived(o.Data.TrimEnd(new char[0]));
				}
			};
			mProcess.Start();
			mProcess.BeginOutputReadLine();
			string text = mProcess.StandardError.ReadToEnd();
			if (!mProcess.WaitForExit(mTimeoutMs))
			{
				ProcessFailed(timedOut: true, string.Empty, -1);
			}
			else if (mProcess.ExitCode != 0 || text.Length != 0)
			{
				ProcessFailed(timedOut: false, text, mProcess.ExitCode);
			}
			else
			{
				ProcessSuccesful();
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Exception on process queue Thread: " + ex.Message + "\n" + ex.StackTrace);
		}
		finally
		{
			IsComplete = true;
		}
	}

	public void Abort()
	{
		mProcess.Kill();
	}

	protected virtual void OutputLineReceived(string line)
	{
		UnityEngine.Debug.Log(line);
	}

	protected virtual void ProcessSuccesful()
	{
		OnProcessSuccesful();
		UnityEngine.Debug.Log("Process Complete");
	}

	protected virtual void ProcessFailed(bool timedOut, string errorMessage, int errorCode)
	{
		OnProcessFailed();
		UnityEngine.Debug.Log($"Process Failed : {errorMessage} with code : {errorCode}");
	}
}
