using UnityEngine;

public class KGFString : MonoBehaviour
{
	public string itsString;

	public string GetString()
	{
		return itsString;
	}

	public override string ToString()
	{
		return itsString;
	}
}
