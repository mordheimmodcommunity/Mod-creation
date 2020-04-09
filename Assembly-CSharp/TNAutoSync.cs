using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TNet;
using UnityEngine;

[ExecuteInEditMode]
public class TNAutoSync : TNBehaviour
{
	[Serializable]
	public class SavedEntry
	{
		public Component target;

		public string propertyName;
	}

	private class ExtendedEntry : SavedEntry
	{
		public FieldInfo field;

		public PropertyInfo property;

		public object lastValue;
	}

	public System.Collections.Generic.List<SavedEntry> entries = new System.Collections.Generic.List<SavedEntry>();

	public int updatesPerSecond = 10;

	public bool isSavedOnServer = true;

	public bool onlyOwnerCanSync = true;

	public bool isImportant;

	private TNet.List<ExtendedEntry> mList = new TNet.List<ExtendedEntry>();

	private object[] mCached;

	private void Awake()
	{
		int i = 0;
		for (int count = entries.Count; i < count; i++)
		{
			SavedEntry savedEntry = entries[i];
			if (!(savedEntry.target != null) || string.IsNullOrEmpty(savedEntry.propertyName))
			{
				continue;
			}
			FieldInfo field = savedEntry.target.GetType().GetField(savedEntry.propertyName, BindingFlags.Instance | BindingFlags.Public);
			if ((object)field != null)
			{
				ExtendedEntry extendedEntry = new ExtendedEntry();
				extendedEntry.target = savedEntry.target;
				extendedEntry.field = field;
				extendedEntry.lastValue = field.GetValue(savedEntry.target);
				mList.Add(extendedEntry);
				continue;
			}
			PropertyInfo property = savedEntry.target.GetType().GetProperty(savedEntry.propertyName, BindingFlags.Instance | BindingFlags.Public);
			if ((object)property != null)
			{
				ExtendedEntry extendedEntry2 = new ExtendedEntry();
				extendedEntry2.target = savedEntry.target;
				extendedEntry2.property = property;
				extendedEntry2.lastValue = property.GetValue(savedEntry.target, null);
				mList.Add(extendedEntry2);
			}
			else
			{
				Debug.LogError("Unable to find property: '" + savedEntry.propertyName + "' on " + savedEntry.target.GetType());
			}
		}
		if (mList.size > 0)
		{
			if (updatesPerSecond > 0)
			{
				StartCoroutine(PeriodicSync());
			}
		}
		else
		{
			Debug.LogWarning("Nothing to sync", this);
			base.enabled = false;
		}
	}

	private IEnumerator PeriodicSync()
	{
		while (true)
		{
			if (TNManager.isInChannel && updatesPerSecond > 0)
			{
				if (mList.size != 0 && (!onlyOwnerCanSync || base.tno.isMine) && Cache())
				{
					Sync();
				}
				yield return new WaitForSeconds(1f / (float)updatesPerSecond);
			}
			else
			{
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

	private void OnNetworkPlayerJoin(Player p)
	{
		if (mList.size != 0 && !isSavedOnServer && TNManager.isHosting)
		{
			if (Cache())
			{
				Sync();
			}
			else
			{
				base.tno.Send(byte.MaxValue, p, mCached);
			}
		}
	}

	private bool Cache()
	{
		bool flag = false;
		bool flag2 = false;
		if (mCached == null)
		{
			flag = true;
			mCached = new object[mList.size];
		}
		for (int i = 0; i < mList.size; i++)
		{
			ExtendedEntry extendedEntry = mList[i];
			object value = ((object)extendedEntry.field != null) ? (value = extendedEntry.field.GetValue(extendedEntry.target)) : (value = extendedEntry.property.GetValue(extendedEntry.target, null));
			if (!value.Equals(extendedEntry.lastValue))
			{
				flag2 = true;
			}
			if (flag || flag2)
			{
				extendedEntry.lastValue = value;
				mCached[i] = value;
			}
		}
		return flag2;
	}

	public void Sync()
	{
		if (TNManager.isInChannel && mList.size != 0)
		{
			if (isImportant)
			{
				base.tno.Send(byte.MaxValue, (!isSavedOnServer) ? Target.Others : Target.OthersSaved, mCached);
			}
			else
			{
				base.tno.SendQuickly(byte.MaxValue, (!isSavedOnServer) ? Target.Others : Target.OthersSaved, mCached);
			}
		}
	}

	[RFC(byte.MaxValue)]
	private void OnSync(object[] val)
	{
		if (!base.enabled)
		{
			return;
		}
		for (int i = 0; i < mList.size; i++)
		{
			ExtendedEntry extendedEntry = mList[i];
			extendedEntry.lastValue = val[i];
			if ((object)extendedEntry.field != null)
			{
				extendedEntry.field.SetValue(extendedEntry.target, extendedEntry.lastValue);
			}
			else
			{
				extendedEntry.property.SetValue(extendedEntry.target, extendedEntry.lastValue, null);
			}
		}
	}
}
