public class KGFDataCell
{
    private KGFDataColumn itsColumn;

    private KGFDataRow itsRow;

    private object itsValue;

    public KGFDataColumn Column => itsColumn;

    public KGFDataRow Row => itsRow;

    public object Value
    {
        get
        {
            return itsValue;
        }
        set
        {
            itsValue = value;
        }
    }

    public KGFDataCell(KGFDataColumn theColumn, KGFDataRow theRow)
    {
        itsColumn = theColumn;
        itsRow = theRow;
        itsValue = null;
    }

    public override string ToString()
    {
        return itsValue.ToString();
    }
}
