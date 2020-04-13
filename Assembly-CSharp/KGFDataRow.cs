using System;
using System.Collections.Generic;

public class KGFDataRow
{
    private KGFDataTable itsTable;

    private List<KGFDataCell> itsCells = new List<KGFDataCell>();

    public KGFDataCell this[int theIndex]
    {
        get
        {
            if (theIndex >= 0 && theIndex < itsTable.Columns.Count)
            {
                return itsCells[theIndex];
            }
            throw new ArgumentOutOfRangeException();
        }
        set
        {
            if (theIndex >= 0 && theIndex < itsTable.Columns.Count)
            {
                itsCells[theIndex] = value;
                return;
            }
            throw new ArgumentOutOfRangeException();
        }
    }

    public KGFDataCell this[string theName]
    {
        get
        {
            foreach (KGFDataCell itsCell in itsCells)
            {
                if (itsCell.Column.ColumnName.Equals(theName))
                {
                    return itsCell;
                }
            }
            throw new ArgumentOutOfRangeException();
        }
        set
        {
            bool flag = false;
            for (int i = 0; i < itsCells.Count; i++)
            {
                if (itsCells[i].Column.ColumnName.Equals(theName))
                {
                    itsCells[i] = value;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    public KGFDataCell this[KGFDataColumn theColumn]
    {
        get
        {
            for (int i = 0; i < itsTable.Columns.Count; i++)
            {
                if (itsCells[i].Column.Equals(theColumn))
                {
                    return itsCells[i];
                }
            }
            throw new ArgumentOutOfRangeException();
        }
        set
        {
            for (int i = 0; i < itsTable.Columns.Count; i++)
            {
                if (itsCells[i].Column.Equals(theColumn))
                {
                    itsCells[i] = value;
                }
            }
            throw new ArgumentOutOfRangeException();
        }
    }

    public KGFDataRow()
    {
        itsTable = null;
    }

    public KGFDataRow(KGFDataTable theTable)
    {
        itsTable = theTable;
        foreach (KGFDataColumn column in theTable.Columns)
        {
            itsCells.Add(new KGFDataCell(column, this));
        }
    }

    public bool IsNull(KGFDataColumn theColumn)
    {
        return IsNull(theColumn.ColumnName);
    }

    public bool IsNull(string theColumn)
    {
        foreach (KGFDataCell itsCell in itsCells)
        {
            if (itsCell.Column.ColumnName.Equals(theColumn) && itsCell.Value != null)
            {
                return false;
            }
        }
        return true;
    }
}
