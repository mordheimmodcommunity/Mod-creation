using UnityEngine;

public class KGFConsoleTutorial : MonoBehaviour
{
	public void Awake()
	{
		KGFConsole.AddCommand("tutorial", this, "WriteMessage");
	}

	private void WriteMessage()
	{
		Debug.Log("this is a message from WriteMessage()");
	}

	private void WriteMessage(int theNumber)
	{
		Debug.Log("this is a message from WriteMessage(int) with the Parameter " + theNumber);
	}

	private void WriteMessage(string theText)
	{
		Debug.Log("this is a message from WriteMessage(string) with the Parameter " + theText);
	}
}
