using System.Collections.Generic;
using UnityEngine;
using WellFired;

public class SequenceManager : PandoraSingleton<SequenceManager>
{
	private Dictionary<string, USSequencer> seqs;

	[HideInInspector]
	public USSequencer currentSeq;

	private bool relative;

	private DelSequenceDone finishDel;

	[HideInInspector]
	public bool isPlaying;

	[HideInInspector]
	public bool destroyAfterPlay;

	private void Start()
	{
		seqs = new Dictionary<string, USSequencer>();
		isPlaying = false;
		destroyAfterPlay = true;
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.SEQUENCE_ENDED, EndSequence);
	}

	public void PlaySequence(USSequencer seq)
	{
		seqs[((Object)(object)seq).name] = seq;
		PlaySequence(((Object)(object)seq).name);
	}

	public void PlaySequence(string name, DelSequenceDone onFinishDel = null)
	{
		isPlaying = true;
		if ((Object)(object)currentSeq != null && currentSeq.get_IsPlaying())
		{
			EndSequence();
		}
		finishDel = onFinishDel;
		if (!seqs.ContainsKey(name))
		{
			PandoraSingleton<AssetBundleLoader>.Instance.LoadResourceAsync<GameObject>("prefabs/sequences/" + name + "/" + name, delegate(Object prefab)
			{
				GameObject gameObject = (GameObject)Object.Instantiate(prefab);
				USSequencer component = gameObject.GetComponent<USSequencer>();
				seqs[name] = component;
				LaunchSequence(name);
			});
		}
		else
		{
			LaunchSequence(name);
		}
	}

	private void LaunchSequence(string name)
	{
		currentSeq = seqs[name];
		currentSeq.Stop();
		currentSeq.Play();
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.SEQUENCE_STARTED);
	}

	public void EndSequence()
	{
		if ((Object)(object)currentSeq != null)
		{
			currentSeq.SkipTimelineTo(currentSeq.get_Duration());
			if (destroyAfterPlay)
			{
				Object.Destroy(((Component)(object)currentSeq).gameObject);
				seqs.Clear();
			}
			currentSeq = null;
			isPlaying = false;
			if (finishDel != null)
			{
				DelSequenceDone delSequenceDone = finishDel;
				finishDel = null;
				delSequenceDone();
			}
		}
	}
}
