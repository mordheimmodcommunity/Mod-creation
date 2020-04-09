using System;

public class KGFDataColumn
{
	private string itsName;

	private Type itsType;

	public string ColumnName
	{
		get
		{
			return itsName;
		}
		set
		{
			itsName = value;
		}
	}

	public Type ColumnType
	{
		get
		{
			return itsType;
		}
		set
		{
			itsType = value;
		}
	}

	public KGFDataColumn(string theName, Type theType)
	{
		itsName = theName;
		itsType = theType;
	}

	public void Add(string theName, Type theType)
	{
		itsName = theName;
		itsType = theType;
	}
}
