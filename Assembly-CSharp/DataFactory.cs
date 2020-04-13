using System;
using System.Collections.Generic;
using System.Text;

public class DataFactory : PandoraSingleton<DataFactory>
{
    private const string WHERE_FORMAT = "{0} = \"{1}\"";

    private const string LIKE_STATEMENT = "{0} LIKE \"%{1}%\"";

    private const string SORT_ASC = "ASC";

    private const string SORT_DESC = "DESC";

    private const string AND_STATEMENT = " AND ";

    private const char SPACE = ' ';

    private const char QUOTE = '"';

    private const char EQUAL = '=';

    private Dictionary<Type, string> cachedTypeName;

    private readonly StringBuilder whereBuilder = new StringBuilder(1024);

    private readonly StringBuilder sortBuilder = new StringBuilder(1024);

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        cachedTypeName = new Dictionary<Type, string>();
        PandoraSingleton<DBConnector>.Instance.Connect(PandoraSingleton<DBConnector>.Instance.databasePath);
        Constant.Init();
    }

    private void Clear()
    {
        whereBuilder.Remove(0, whereBuilder.Length);
        sortBuilder.Remove(0, sortBuilder.Length);
    }

    public T InitData<T>(int id) where T : DataCore, new()
    {
        Clear();
        AppendWhere(whereBuilder, "id", Constant.ToString(id));
        return InitDBObject<T>();
    }

    private string GetTableName(Type type)
    {
        if (!cachedTypeName.TryGetValue(type, out string value))
        {
            value = DBConnector.ClassNameToTableName(type.Name);
            cachedTypeName[type] = value;
        }
        return value;
    }

    public List<T> InitData<T>() where T : DataCore, new()
    {
        Clear();
        return InitDBObjects<T>();
    }

    public List<T> InitData<T>(string field1, string id1) where T : DataCore, new()
    {
        Clear();
        AppendWhere(whereBuilder, field1, id1);
        return InitDBObjects<T>();
    }

    public List<T> InitData<T>(string field1, string id1, string field2, string id2) where T : DataCore, new()
    {
        Clear();
        AppendWhere(whereBuilder, field1, id1);
        whereBuilder.Append(" AND ");
        AppendWhere(whereBuilder, field2, id2);
        return InitDBObjects<T>();
    }

    public List<T> InitData<T>(string[] fields, string[] ids) where T : DataCore, new()
    {
        Clear();
        for (int i = 0; i < fields.Length; i++)
        {
            if (i > 0)
            {
                whereBuilder.Append(" AND ");
            }
            AppendWhere(whereBuilder, fields[i], ids[i]);
        }
        return InitDBObjects<T>();
    }

    public T InitDataClosest<T>(string field, int id, bool lower) where T : DataCore, new()
    {
        return InitDataClosest<T>(field, id, new string[0], new string[0], lower);
    }

    public T InitDataClosest<T>(string field, int id, string[] fields, string[] ids, bool lower) where T : DataCore, new()
    {
        Clear();
        whereBuilder.Append(field);
        whereBuilder.Append((!lower) ? " >= " : " <= ");
        whereBuilder.Append(id.ToConstantString());
        for (int i = 0; i < fields.Length; i++)
        {
            whereBuilder.Append(" AND ");
            AppendWhere(whereBuilder, fields[i], ids[i]);
        }
        if (lower)
        {
            AppendSortDesc(sortBuilder, field);
        }
        else
        {
            AppendSortAsc(sortBuilder, field);
        }
        return InitDBObject<T>();
    }

    public List<T> InitDataRange<T>(string field, int low, int high, string[] fields, string[] ids) where T : DataCore, new()
    {
        Clear();
        whereBuilder.Append(field).Append(" >= ").Append(low);
        whereBuilder.Append(" AND ");
        whereBuilder.Append(field).Append(" <= ").Append(high);
        for (int i = 0; i < fields.Length; i++)
        {
            whereBuilder.Append(" AND ");
            AppendWhere(whereBuilder, fields[i], ids[i]);
        }
        AppendSortAsc(sortBuilder, field);
        return InitDBObjects<T>();
    }

    private T InitDBObject<T>() where T : DataCore, new()
    {
        List<T> list = PandoraSingleton<DBConnector>.Instance.Read<T>(GetTableName(typeof(T)), "*", whereBuilder, sortBuilder, 1);
        if (list.Count == 0)
        {
            return (T)null;
        }
        return list[0];
    }

    private List<T> InitDBObjects<T>() where T : DataCore, new()
    {
        return PandoraSingleton<DBConnector>.Instance.Read<T>(GetTableName(typeof(T)), "*", whereBuilder, sortBuilder);
    }

    private static void AppendWhere(StringBuilder stringBuilder, string field, string id)
    {
        stringBuilder.Append(field).Append('=').Append('"')
            .Append(id)
            .Append('"');
    }

    private static void AppendSortAsc(StringBuilder stringBuilder, string field)
    {
        stringBuilder.Append(field).Append(' ').Append("ASC");
    }

    private static void AppendSortDesc(StringBuilder stringBuilder, string field)
    {
        stringBuilder.Append(field).Append(' ').Append("DESC");
    }

    private static void AppendLike(StringBuilder stringBuilder, string field, string like)
    {
        stringBuilder.Append(field).Append(" LIKE \"%").Append(like)
            .Append("%\"");
    }
}
