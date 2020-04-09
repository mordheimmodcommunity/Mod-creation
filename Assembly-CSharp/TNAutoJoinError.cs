using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class TNAutoJoinError : MonoBehaviour
{
	private void AutoJoinError(string message)
	{
		TextMesh component = GetComponent<TextMesh>();
		component.text = message;
	}
}
