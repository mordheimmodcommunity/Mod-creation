using System;

[Serializable]
public class IDReference
{
	public string itsID = string.Empty;

	public bool itsEmpty = true;

	public bool itsCanBeDeleted;

	public string GetID()
	{
		return itsID;
	}

	public void SetID(string theID)
	{
		itsID = theID;
		itsEmpty = false;
	}

	public bool GetHasValue()
	{
		return !itsEmpty;
	}

	public void SetEmpty()
	{
		itsEmpty = true;
	}

	public override string ToString()
	{
		return GetID();
	}

	public bool GetCanBeDeleted()
	{
		return itsCanBeDeleted;
	}

	public void SetCanBeDeleted(bool theCanBeDeleted)
	{
		itsCanBeDeleted = theCanBeDeleted;
	}
}
