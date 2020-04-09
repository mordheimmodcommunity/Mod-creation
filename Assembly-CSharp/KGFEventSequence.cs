using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KGFEventSequence : KGFEventBase, KGFIValidator
{
	[Serializable]
	public class KGFEventSequenceEntry
	{
		public float itsWaitBefore;

		public KGFEventBase itsEvent;

		public float itsWaitAfter;
	}

	private const string itsEventCategory = "KGFEventSystem";

	public List<KGFEventSequenceEntry> itsEntries = new List<KGFEventSequenceEntry>();

	private static List<KGFEventSequence> itsListOfRunningSequences = new List<KGFEventSequence>();

	private bool itsEventSequenceRunning;

	private static List<KGFEventSequence> itsListOfSequencesAll = new List<KGFEventSequence>();

	private static bool itsStepMode = false;

	private int itsStayBeforeStepID;

	private int? itsEventDoneCounter;

	protected override void KGFAwake()
	{
		itsListOfSequencesAll.Add(this);
	}

	public void Step()
	{
		itsStayBeforeStepID++;
	}

	public void Finish()
	{
		itsStayBeforeStepID = itsEntries.Count + 1;
	}

	public static bool GetSingleStepMode()
	{
		return itsStepMode;
	}

	public bool IsWaitingForDebugInput()
	{
		return itsStayBeforeStepID == itsEventDoneCounter;
	}

	public int GetCurrentStepNumber()
	{
		return itsEventDoneCounter.GetValueOrDefault(0);
	}

	public int GetStepCount()
	{
		return itsEntries.Count;
	}

	public static void SetSingleStepMode(bool theActivateStepMode)
	{
		itsStepMode = theActivateStepMode;
	}

	public static KGFEventSequence[] GetAllSequences()
	{
		return itsListOfSequencesAll.ToArray();
	}

	public static IEnumerable<KGFEventSequence> GetQueuedSequences()
	{
		for (int i = itsListOfSequencesAll.Count - 1; i >= 0; i--)
		{
			KGFEventSequence aSequence = itsListOfSequencesAll[i];
			if (aSequence == null)
			{
				itsListOfSequencesAll.RemoveAt(i);
			}
			else if (aSequence.gameObject == null)
			{
				itsListOfSequencesAll.RemoveAt(i);
			}
			else if (aSequence.IsQueued())
			{
				yield return aSequence;
			}
		}
	}

	public static int GetNumberOfRunningSequences()
	{
		return itsListOfRunningSequences.Count;
	}

	public static KGFEventSequence[] GetRunningEventSequences()
	{
		return itsListOfRunningSequences.ToArray();
	}

	public void InitList()
	{
		if (itsEntries.Count == 0)
		{
			itsEntries.Add(new KGFEventSequenceEntry());
		}
	}

	public void Insert(KGFEventSequenceEntry theElementAfterToInsert, KGFEventSequenceEntry theElementToInsert)
	{
		int num = itsEntries.IndexOf(theElementAfterToInsert);
		itsEntries.Insert(num + 1, theElementToInsert);
	}

	public void Delete(KGFEventSequenceEntry theElementToDelete)
	{
		if (itsEntries.Count > 1)
		{
			itsEntries.Remove(theElementToDelete);
		}
	}

	public void MoveUp(KGFEventSequenceEntry theElementToMoveUp)
	{
		int num = itsEntries.IndexOf(theElementToMoveUp);
		if (num <= 0)
		{
			KGFEvent.LogWarning("cannot move up element at 0 index", "KGFEventSystem", this);
			return;
		}
		Delete(theElementToMoveUp);
		itsEntries.Insert(num - 1, theElementToMoveUp);
	}

	public void MoveDown(KGFEventSequenceEntry theElementToMoveDown)
	{
		int num = itsEntries.IndexOf(theElementToMoveDown);
		if (num >= itsEntries.Count - 1)
		{
			KGFEvent.LogWarning("cannot move down element at end index", "KGFEventSystem", this);
			return;
		}
		Delete(theElementToMoveDown);
		itsEntries.Insert(num + 1, theElementToMoveDown);
	}

	public bool IsRunning()
	{
		return itsEventSequenceRunning;
	}

	public bool IsQueued()
	{
		int? num = itsEventDoneCounter;
		return num.HasValue && !itsEventSequenceRunning;
	}

	public string GetNextExecutedJobItem()
	{
		int? num = itsEventDoneCounter;
		if (num.HasValue)
		{
			if (itsEventDoneCounter.GetValueOrDefault() < itsEntries.Count)
			{
				return itsEntries[itsEventDoneCounter.GetValueOrDefault()].itsEvent.name;
			}
			return "finished";
		}
		return "not running";
	}

	[KGFEventExpose]
	public override void Trigger()
	{
		itsEventDoneCounter = 0;
		if (base.gameObject.active)
		{
			itsEventSequenceRunning = true;
			KGFEvent.LogDebug("Start: " + base.gameObject.name, "KGFEventSystem", this);
			StartCoroutine("StartSequence");
		}
		else
		{
			KGFEvent.LogDebug("Queued: " + base.gameObject.name, "KGFEventSystem", this);
		}
	}

	[KGFEventExpose]
	public void StopSequence()
	{
		StopCoroutine("StartSequence");
		itsEventSequenceRunning = false;
		itsEventDoneCounter = null;
		if (itsListOfRunningSequences.Contains(this))
		{
			itsListOfRunningSequences.Remove(this);
		}
	}

	private IEnumerator StartSequence()
	{
		itsStayBeforeStepID = 0;
		if (!itsListOfRunningSequences.Contains(this))
		{
			itsListOfRunningSequences.Add(this);
		}
		int? num = itsEventDoneCounter;
		if (!num.HasValue)
		{
			yield break;
		}
		for (int i = itsEventDoneCounter.GetValueOrDefault(0); i < itsEntries.Count; i++)
		{
			KGFEventSequenceEntry anEntry = itsEntries[i];
			if (anEntry.itsWaitBefore > 0f)
			{
				yield return new WaitForSeconds(anEntry.itsWaitBefore);
			}
			try
			{
				if (anEntry.itsEvent != null)
				{
					anEntry.itsEvent.Trigger();
				}
				else
				{
					KGFEvent.LogError("events have null entries", "KGFEventSystem", this);
				}
			}
			catch (Exception e)
			{
				KGFEvent.LogError("Exception in event_sequence:" + e, "KGFEventSystem", this);
			}
			itsEventDoneCounter = i + 1;
			if (anEntry.itsWaitAfter > 0f)
			{
				yield return new WaitForSeconds(anEntry.itsWaitAfter);
			}
		}
		itsEventDoneCounter = null;
		itsEventSequenceRunning = false;
		if (itsListOfRunningSequences.Contains(this))
		{
			itsListOfRunningSequences.Remove(this);
		}
	}

	private void OnDestruct()
	{
		StopSequence();
	}

	public override KGFMessageList Validate()
	{
		KGFMessageList kGFMessageList = new KGFMessageList();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (itsEntries != null)
		{
			for (int i = 0; i < itsEntries.Count; i++)
			{
				KGFEventSequenceEntry kGFEventSequenceEntry = itsEntries[i];
				if (kGFEventSequenceEntry.itsEvent == null)
				{
					flag = true;
				}
				if (kGFEventSequenceEntry.itsWaitBefore < 0f)
				{
					flag2 = true;
				}
				if (kGFEventSequenceEntry.itsWaitAfter < 0f)
				{
					flag3 = true;
				}
			}
		}
		if (flag)
		{
			kGFMessageList.AddError("sequence entry has null event");
		}
		if (flag2)
		{
			kGFMessageList.AddError("sequence entry itsWaitBefore <= 0");
		}
		if (flag3)
		{
			kGFMessageList.AddError("sequence entry itsWaitAfter <= 0");
		}
		return kGFMessageList;
	}
}
