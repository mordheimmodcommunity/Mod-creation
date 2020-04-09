using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PandoraDebug
{
	private const string msgWithObject = "{0} ({1}) - [{2}] {3} {4} for Object: {5}";

	private const string msgWithoutObject = "{0} ({1}) - [{2}] {3} {4} for Object: NULL";

	private const string defaultSeparator = "\n";

	[Conditional("MCOTD_DEBUG")]
	public static void Assert(bool condition, string message, bool fatal)
	{
	}

	public static void LogDebug(object msg, string category = "uncategorised", MonoBehaviour monoObject = null)
	{
		if (Pandora.fullLog)
		{
			string message = (!monoObject) ? string.Format("{0} ({1}) - [{2}] {3} {4} for Object: NULL", GetTimeString(), GetTyche(), "DEBUG", category, msg) : string.Format("{0} ({1}) - [{2}] {3} {4} for Object: {5}", GetTimeString(), GetTyche(), "DEBUG", category, msg, monoObject.name);
			UnityEngine.Debug.Log(message);
		}
	}

	public static void LogInfo(object msg, string category = "uncategorised", MonoBehaviour monoObject = null)
	{
		if (Pandora.fullLog)
		{
			string message = (!monoObject) ? string.Format("{0} ({1}) - [{2}] {3} {4} for Object: NULL", GetTimeString(), GetTyche(), "INFO", category, msg) : string.Format("{0} ({1}) - [{2}] {3} {4} for Object: {5}", GetTimeString(), GetTyche(), "INFO", category, msg, monoObject.name);
			UnityEngine.Debug.Log(message);
		}
	}

	public static void LogWarning(object msg, string category = "uncategorised", MonoBehaviour monoObject = null)
	{
		string message = (!monoObject) ? string.Format("{0} ({1}) - [{2}] {3} {4} for Object: NULL", GetTimeString(), GetTyche(), "WARNING", category, msg) : string.Format("{0} ({1}) - [{2}] {3} {4} for Object: {5}", GetTimeString(), GetTyche(), "WARNING", category, msg, monoObject.name);
		UnityEngine.Debug.LogWarning(message);
	}

	public static void LogError(object msg, string category = "uncategorised", MonoBehaviour monoObject = null)
	{
		string message = (!monoObject) ? string.Format("{0} ({1}) - [{2}] {3} {4} for Object: NULL", GetTimeString(), GetTyche(), "ERROR", category, msg) : string.Format("{0} ({1}) - [{2}] {3} {4} for Object: {5}", GetTimeString(), GetTyche(), "ERROR", category, msg, monoObject.name);
		UnityEngine.Debug.LogError(message);
	}

	public static void LogFatal(object msg, string category = "uncategorised", MonoBehaviour monoObject = null)
	{
		string message = (!monoObject) ? string.Format("{0} ({1}) - [{2}] {3} {4} for Object: NULL", GetTimeString(), GetTyche(), "FATAL", category, msg) : string.Format("{0} ({1}) - [{2}] {3} {4} for Object: {5}", GetTimeString(), GetTyche(), "FATAL", category, msg, monoObject.name);
		UnityEngine.Debug.LogError(message);
	}

	public static void LogException(Exception e, bool fatal = true)
	{
		string empty = string.Empty;
		string text = empty;
		empty = text + TimeSpan.FromSeconds(Time.time) + ": " + e;
		empty = empty + "\nInnerException: " + e.InnerException;
		empty = empty + "\nMessage: " + e.Message;
		empty = empty + "\nSource: " + e.Source;
		empty = empty + "\nStackTrace: " + e.StackTrace;
		empty = empty + "\nTargetSite: " + e.TargetSite;
		UnityEngine.Debug.LogException(e);
	}

	public static void LogStrings(string separator, params string[] strings)
	{
		string text = null;
		for (int i = 0; i < strings.Length; i++)
		{
			text = ((separator == null) ? (text + string.Format(strings[i] + "{0}", (i >= strings.Length - 1) ? string.Empty : "\n")) : (text + string.Format(strings[i] + "{0}", (i >= strings.Length - 1) ? string.Empty : separator)));
		}
		LogInfo("Length = " + strings.Length + text);
	}

	public static void LogVector(Vector3 vec)
	{
		LogInfo("Vector x = " + vec.x + ", y = " + vec.y + ", z = " + vec.z);
	}

	public static void LogArray<T>(T[] array)
	{
		LogArray(null, array);
	}

	public static void LogArray<T>(string separator, T[] array)
	{
		string text = null;
		for (int i = 0; i < array.Length; i++)
		{
			text = ((separator == null) ? (text + string.Format(array[i].ToString() + "{0}", (i >= array.Length - 1) ? string.Empty : "\n")) : (text + string.Format(array[i].ToString() + "{0}", (i >= array.Length - 1) ? string.Empty : separator)));
		}
		LogInfo("Length = " + array.Length + text);
	}

	public static void LogArray2D<T>(T[,] array)
	{
		LogArray2D(null, array);
	}

	public static void LogArray2D<T>(string separator, T[,] array)
	{
		string text = null;
		for (int i = 0; i < array.GetUpperBound(0); i++)
		{
			for (int j = 0; j < array.GetUpperBound(1); j++)
			{
				text = ((separator == null) ? (text + string.Format(array[i, 0].ToString() + ", " + array[i, 1].ToString() + "{0}", (i * j >= array.Length - 1) ? string.Empty : "\n")) : (text + string.Format(array[i, 0].ToString() + ", " + array[i, 1].ToString() + "{0}", (i * j >= array.Length - 1) ? string.Empty : separator)));
			}
		}
		LogInfo("Length = " + array.Length + text);
	}

	public static void LogArray3D<T>(T[,,] array)
	{
		LogArray3D(null, array);
	}

	public static void LogArray3D<T>(string separator, T[,,] array)
	{
		string text = null;
		int num = 1;
		for (int i = 0; i < array.GetLength(2); i++)
		{
			for (int j = 0; j < array.GetLength(1); j++)
			{
				for (int k = 0; k < array.GetLength(0); k++)
				{
					text = ((separator == null) ? (text + string.Format(num.ToString() + ". [" + k.ToString() + ", " + j.ToString() + ", " + i.ToString() + "] =" + array[k, j, i] + "{0}", (i * j * k >= array.Length - 1) ? string.Empty : "\n")) : (text + string.Format(num.ToString() + ". [" + k.ToString() + ", " + j.ToString() + ", " + i.ToString() + "] =" + array[k, j, i] + "{0}", (i * j * k >= array.Length - 1) ? string.Empty : separator)));
					num++;
				}
			}
		}
		LogInfo("Length = " + array.Length + text);
	}

	public static void LogList<T>(List<T> list)
	{
		LogList(null, list);
	}

	public static void LogList<T>(string separator, List<T> list)
	{
		string text = null;
		for (int i = 0; i < list.Count; i++)
		{
			text = ((separator == null) ? (text + string.Format(list[i].ToString() + "{0}", (i >= list.Count - 1) ? string.Empty : "\n")) : (text + string.Format(list[i].ToString() + "{0}", (i >= list.Count - 1) ? string.Empty : separator)));
		}
		LogInfo("Length = " + list.Count + text);
	}

	public static void LogDictionary<T, K>(Dictionary<T, K> dictionary)
	{
		LogDictionary(null, dictionary);
	}

	public static void LogDictionary<T, K>(string separator, Dictionary<T, K> dictionary)
	{
		string text = null;
		int num = 0;
		foreach (KeyValuePair<T, K> item in dictionary)
		{
			text = ((separator == null) ? (text + string.Format(item.Key.ToString() + ", " + item.Value.ToString() + "{0}", (num >= dictionary.Count - 1) ? string.Empty : "\n")) : (text + string.Format(item.Key.ToString() + ", " + item.Value.ToString() + "{0}", (num >= dictionary.Count - 1) ? string.Empty : separator)));
			num++;
		}
		LogInfo("Length = " + dictionary.Count + text);
	}

	private static string GetTimeString()
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time);
		return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}:{timeSpan.Milliseconds:000}";
	}

	private static string GetTyche()
	{
		if (PandoraSingleton<MissionManager>.Exists())
		{
			return PandoraSingleton<MissionManager>.Instance.NetworkTyche.Count.ToString();
		}
		return "0";
	}
}
