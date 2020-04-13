using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

public class DBConnector : PandoraSingleton<DBConnector>
{
    private const string SELECT = "SELECT ";

    private const string FROM = " FROM ";

    private const string WHERE = " WHERE ";

    private const string ORDER_BY = " ORDER BY ";

    private const string LIMIT = " LIMIT ";

    public string databasePath;

    private Dictionary<uint, object> cachedListResults = new Dictionary<uint, object>();

    private List<object> emptyList = new List<object>(0);

    public SqliteConnection Connection
    {
        get;
        private set;
    }

    private void Destroy()
    {
        if (Connection != null)
        {
            Connection.Close();
        }
    }

    public void Connect(string path)
    {
        databasePath = path;
        CreateConnection();
    }

    private void CreateConnection()
    {
        //Discarded unreachable code: IL_0116
        //IL_0044: Unknown result type (might be due to invalid IL or missing references)
        //IL_004e: Expected O, but got Unknown
        if (Connection != null)
        {
            Connection.Close();
        }
        string text = $"URI=file:{Application.streamingAssetsPath}{databasePath}";
        try
        {
            PandoraDebug.LogInfo("Trying to connec to : " + text, "DB Connector");
            Connection = (SqliteConnection)(object)new SqliteConnection(text);
            TextAsset textAsset = Resources.Load<TextAsset>("database/connection1");
            TextAsset textAsset2 = Resources.Load<TextAsset>("database/connection2");
            TextAsset textAsset3 = Resources.Load<TextAsset>("database/connection3");
            if (textAsset != null && textAsset2 != null && textAsset3 != null)
            {
                Connection.SetPassword(textAsset.text.Trim() + textAsset2.text.Trim() + textAsset3.text.Trim());
            }
            Connection.Open();
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            PandoraDebug.LogWarning(ex);
            Debug.Log(ex.Message + " [" + text + "]");
            Debug.Log(ex.StackTrace);
            return;
        }
        PandoraDebug.LogInfo("Connection successful", "DB Connector");
    }

    public void CloseConnection()
    {
        Connection.Close();
        Connection = null;
    }

    public List<List<object>> GetTablesName()
    {
        string request = "SELECT name  FROM sqlite_master WHERE type='table' ORDER BY name";
        return ExecuteRequest(request);
    }

    public List<string> GetTableFieldsName(string table)
    {
        //IL_001e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0024: Expected O, but got Unknown
        List<string> list = new List<string>();
        string text = "SELECT * FROM " + table + " LIMIT 1";
        SqliteCommand val = (SqliteCommand)(object)new SqliteCommand(text, Connection);
        try
        {
            SqliteDataReader val2 = val.ExecuteReader();
            try
            {
                while (val2.Read())
                {
                    for (int i = 0; i < val2.get_FieldCount(); i++)
                    {
                        list.Add(val2.GetName(i));
                    }
                }
                val2.Close();
                return list;
            }
            finally
            {
                ((IDisposable)val2)?.Dispose();
            }
        }
        finally
        {
            ((IDisposable)val)?.Dispose();
        }
    }

    public List<List<string>> GetTableFieldsTypeName(string table)
    {
        //IL_0022: Unknown result type (might be due to invalid IL or missing references)
        //IL_0028: Expected O, but got Unknown
        List<List<string>> list = new List<List<string>>();
        string text = "SELECT * FROM " + table + " LIMIT 1";
        SqliteCommand val = (SqliteCommand)(object)new SqliteCommand(text, PandoraSingleton<DBConnector>.Instance.Connection);
        try
        {
            SqliteDataReader val2 = val.ExecuteReader();
            try
            {
                while (val2.Read())
                {
                    for (int i = 0; i < val2.get_FieldCount(); i++)
                    {
                        List<string> list2 = new List<string>();
                        list2.Add(val2.GetFieldType(i).Name);
                        list2.Add(val2.GetName(i));
                        list.Add(list2);
                    }
                }
                return list;
            }
            finally
            {
                ((IDisposable)val2)?.Dispose();
            }
        }
        finally
        {
            ((IDisposable)val)?.Dispose();
        }
    }

    public List<List<object>> GetTableIdName(string table)
    {
        string request = "SELECT id, name FROM " + table + " ORDER BY id;";
        return ExecuteRequest(request);
    }

    public List<T> Read<T>(string table, string fields, StringBuilder condition, StringBuilder sort, int limit = 0) where T : DataCore, new()
    {
        StringBuilder stringBuilder = PandoraUtils.StringBuilder;
        stringBuilder.Append("SELECT ").Append(fields);
        stringBuilder.Append(" FROM ").Append(table);
        if (condition.Length > 0)
        {
            stringBuilder.Append(" WHERE ");
            for (int i = 0; i < condition.Length; i++)
            {
                stringBuilder.Append(condition[i]);
            }
        }
        if (sort.Length > 0)
        {
            stringBuilder.Append(" ORDER BY ");
            for (int j = 0; j < sort.Length; j++)
            {
                stringBuilder.Append(sort[j]);
            }
        }
        if (limit > 0)
        {
            stringBuilder.Append(" LIMIT ").Append(limit.ToConstantString());
        }
        uint key = FNV1a.ComputeHash(stringBuilder);
        if (!cachedListResults.TryGetValue(key, out object value))
        {
            List<T> list = ExecuteRequest<T>(stringBuilder.ToString());
            if (list.Count > 0)
            {
                cachedListResults[key] = list;
            }
            else
            {
                cachedListResults[key] = list;
            }
            return list;
        }
        return (List<T>)value;
    }

    private List<T> ExecuteRequest<T>(string request) where T : DataCore, new()
    {
        //Discarded unreachable code: IL_009d
        //IL_000d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0013: Expected O, but got Unknown
        List<T> list = new List<T>();
        try
        {
            SqliteCommand val = (SqliteCommand)(object)new SqliteCommand(request, Connection);
            try
            {
                SqliteDataReader val2 = val.ExecuteReader();
                try
                {
                    while (val2.Read())
                    {
                        T item = new T();
                        item.Populate(val2);
                        list.Add(item);
                    }
                    val2.Close();
                }
                finally
                {
                    ((IDisposable)val2)?.Dispose();
                }
                ((System.ComponentModel.Component)(object)val).Dispose();
                return list;
            }
            finally
            {
                ((IDisposable)val)?.Dispose();
            }
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            PandoraDebug.LogException(ex, fatal: false);
            Debug.Log(ex.Message);
            return null;
        }
    }

    private List<List<object>> ExecuteRequest(string request)
    {
        //Discarded unreachable code: IL_00b3
        //IL_000d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0013: Expected O, but got Unknown
        List<List<object>> list = new List<List<object>>();
        try
        {
            SqliteCommand val = (SqliteCommand)(object)new SqliteCommand(request, Connection);
            try
            {
                SqliteDataReader val2 = val.ExecuteReader();
                try
                {
                    while (val2.Read())
                    {
                        List<object> list2 = new List<object>();
                        for (int i = 0; i < val2.get_FieldCount(); i++)
                        {
                            list2.Add(val2.GetValue(i));
                        }
                        list.Add(list2);
                    }
                    val2.Close();
                }
                finally
                {
                    ((IDisposable)val2)?.Dispose();
                }
                ((System.ComponentModel.Component)(object)val).Dispose();
                return list;
            }
            finally
            {
                ((IDisposable)val)?.Dispose();
            }
        }
        catch (Exception ex)
        {
            PandoraDebug.LogException(ex, fatal: false);
            Debug.Log(ex.Message);
            return null;
        }
    }

    public static string TableNameToClassName(string tableName)
    {
        return PandoraUtils.UnderToCamel(tableName) + "Data";
    }

    public static string ClassNameToTableName(string className)
    {
        return PandoraUtils.CamelToUnder(className.Replace("Data", string.Empty));
    }
}
