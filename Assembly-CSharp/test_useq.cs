using System.Collections.Generic;
using UnityEngine;
using WellFired;

public class test_useq : MonoBehaviour
{
	public List<Transform> anchors;

	public string sequenceName;

	private USSequencer sequence;

	private bool loaded;

	private int anchorIdx;

	private void Start()
	{
	}

	private void Update()
	{
		if (loaded && Input.GetKeyDown(KeyCode.Space) && (Object)(object)sequence != null)
		{
			Debug.Log("PLAY SEQUENCE");
			Camera.main.transform.parent = anchors[anchorIdx];
			if (anchors[anchorIdx] != null)
			{
				Camera.main.transform.localPosition = Vector3.zero;
				Camera.main.transform.localRotation = Quaternion.identity;
			}
			anchorIdx = (anchorIdx + 1) % anchors.Count;
			sequence.Stop();
			sequence.Play();
		}
		if (!loaded && Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("LOADING SEQUENCE");
			GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("prefabs/sequences/" + sequenceName + "/" + sequenceName));
			sequence = gameObject.GetComponent<USSequencer>();
			if ((Object)(object)sequence == null)
			{
				Debug.Log("SEQUENCE NULL!!!!");
				return;
			}
			loaded = true;
			Debug.Log("SEQUENCE LOADED");
		}
	}
}
