using System;

public class KGFObjectListItemDisplayAttribute : Attribute
{
	private string itsHeader;

	private bool itsSearchable;

	private bool itsDisplay;

	public string Header => itsHeader;

	public bool Searchable => itsSearchable;

	public bool Display => itsDisplay;

	public KGFObjectListItemDisplayAttribute(string theHeader)
	{
		itsHeader = theHeader;
		itsSearchable = false;
		itsDisplay = true;
	}

	public KGFObjectListItemDisplayAttribute(string theHeader, bool theSearchable)
	{
		itsHeader = theHeader;
		itsSearchable = theSearchable;
		itsDisplay = true;
	}

	public KGFObjectListItemDisplayAttribute(string theHeader, bool theSearchable, bool theDisplay)
	{
		itsHeader = theHeader;
		itsSearchable = theSearchable;
		itsDisplay = theDisplay;
	}
}
