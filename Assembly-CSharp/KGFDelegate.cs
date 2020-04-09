using System;
using System.Collections.Generic;
using UnityEngine;

public class KGFDelegate
{
	private List<Action<object, EventArgs>> itsDelegateList = new List<Action<object, EventArgs>>();

	public void Trigger(object theSender)
	{
		Trigger(theSender, null);
	}

	public void Trigger(object theSender, EventArgs theArgs)
	{
		for (int num = itsDelegateList.Count - 1; num >= 0; num--)
		{
			Action<object, EventArgs> action = itsDelegateList[num];
			if (action == null)
			{
				itsDelegateList.RemoveAt(num);
			}
			else if (action.Target == null)
			{
				itsDelegateList.RemoveAt(num);
			}
			else if (action.Target is MonoBehaviour && (MonoBehaviour)action.Target == null)
			{
				itsDelegateList.RemoveAt(num);
			}
			else
			{
				action(theSender, theArgs);
			}
		}
	}

	public void Clear()
	{
		itsDelegateList.Clear();
	}

	public static KGFDelegate operator +(KGFDelegate theMyDelegate, Action<object, EventArgs> theDelegate)
	{
		theMyDelegate.itsDelegateList.Add(theDelegate);
		return theMyDelegate;
	}

	public static KGFDelegate operator -(KGFDelegate theMyDelegate, Action<object, EventArgs> theDelegate)
	{
		theMyDelegate.itsDelegateList.Remove(theDelegate);
		return theMyDelegate;
	}
}
