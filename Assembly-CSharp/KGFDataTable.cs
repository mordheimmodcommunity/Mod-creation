using System.Collections.Generic;

public class KGFDataTable
{
	private List<KGFDataColumn> itsColumns = new List<KGFDataColumn>();

	private List<KGFDataRow> itsRows = new List<KGFDataRow>();

	public List<KGFDataColumn> Columns => itsColumns;

	public List<KGFDataRow> Rows => itsRows;

	public KGFDataRow NewRow()
	{
		return new KGFDataRow(this);
	}
}
