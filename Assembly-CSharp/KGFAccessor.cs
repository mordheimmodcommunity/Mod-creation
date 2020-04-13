using System;
using System.Collections.Generic;
using UnityEngine;

public static class KGFAccessor
{
    public class KGFAccessorEventargs : EventArgs
    {
        private object itsObject;

        public KGFAccessorEventargs(object theObject)
        {
            itsObject = theObject;
        }

        public object GetObject()
        {
            return itsObject;
        }
    }

    private static Dictionary<Type, List<KGFObject>> itsListSorted = new Dictionary<Type, List<KGFObject>>();

    private static Dictionary<Type, KGFDelegate> itsListEventsAdd = new Dictionary<Type, KGFDelegate>();

    private static Dictionary<Type, KGFDelegate> itsListEventsAddOnce = new Dictionary<Type, KGFDelegate>();

    private static Dictionary<Type, KGFDelegate> itsListEventsRemove = new Dictionary<Type, KGFDelegate>();

    public static void AddKGFObject(KGFObject theObjectScript)
    {
        Type type = theObjectScript.GetType();
        if (!itsListSorted.ContainsKey(type))
        {
            itsListSorted[type] = new List<KGFObject>();
        }
        itsListSorted[type].Add(theObjectScript);
        foreach (Type key in itsListEventsAdd.Keys)
        {
            if (key.IsAssignableFrom(type))
            {
                itsListEventsAdd[key].Trigger(null, new KGFAccessorEventargs(theObjectScript));
            }
        }
        if (itsListEventsAddOnce.Count > 0)
        {
            List<Type> list = new List<Type>();
            foreach (Type key2 in itsListEventsAddOnce.Keys)
            {
                if (key2.IsAssignableFrom(type))
                {
                    list.Add(key2);
                }
            }
            foreach (Type item in list)
            {
                itsListEventsAddOnce[item].Trigger(null, new KGFAccessorEventargs(theObjectScript));
                itsListEventsAddOnce.Remove(item);
            }
        }
    }

    public static void RemoveKGFObject(KGFObject theObjectScript)
    {
        Type type = theObjectScript.GetType();
        try
        {
            itsListSorted[type].Remove(theObjectScript);
        }
        catch
        {
        }
        foreach (Type key in itsListEventsRemove.Keys)
        {
            if (key.IsAssignableFrom(type))
            {
                itsListEventsRemove[key].Trigger(null, new KGFAccessorEventargs(theObjectScript));
            }
        }
    }

    public static void GetExternal<T>(Action<object, EventArgs> theRegisterCallback)
    {
        T @object = GetObject<T>();
        if (@object != null)
        {
            theRegisterCallback(null, new KGFAccessorEventargs(@object));
        }
        else
        {
            RegisterAddOnceEvent<T>(theRegisterCallback);
        }
    }

    public static void RegisterAddEvent<T>(Action<object, EventArgs> theCallback)
    {
        if (theCallback != null)
        {
            Type typeFromHandle = typeof(T);
            if (!itsListEventsAdd.ContainsKey(typeFromHandle))
            {
                itsListEventsAdd[typeFromHandle] = new KGFDelegate();
            }
            Dictionary<Type, KGFDelegate> dictionary;
            Dictionary<Type, KGFDelegate> dictionary2 = dictionary = itsListEventsAdd;
            Type key;
            Type key2 = key = typeFromHandle;
            KGFDelegate theMyDelegate = dictionary[key];
            dictionary2[key2] = theMyDelegate + theCallback;
        }
    }

    public static void RegisterAddOnceEvent<T>(Action<object, EventArgs> theCallback)
    {
        if (theCallback != null)
        {
            Type typeFromHandle = typeof(T);
            if (!itsListEventsAddOnce.ContainsKey(typeFromHandle))
            {
                itsListEventsAddOnce[typeFromHandle] = new KGFDelegate();
            }
            Dictionary<Type, KGFDelegate> dictionary;
            Dictionary<Type, KGFDelegate> dictionary2 = dictionary = itsListEventsAddOnce;
            Type key;
            Type key2 = key = typeFromHandle;
            KGFDelegate theMyDelegate = dictionary[key];
            dictionary2[key2] = theMyDelegate + theCallback;
        }
    }

    public static void UnregisterAddEvent<T>(Action<object, EventArgs> theCallback)
    {
        Type typeFromHandle = typeof(T);
        if (itsListEventsAdd.ContainsKey(typeFromHandle))
        {
            Dictionary<Type, KGFDelegate> dictionary;
            Dictionary<Type, KGFDelegate> dictionary2 = dictionary = itsListEventsAdd;
            Type key;
            Type key2 = key = typeFromHandle;
            KGFDelegate theMyDelegate = dictionary[key];
            dictionary2[key2] = theMyDelegate - theCallback;
        }
    }

    public static void RegisterRemoveEvent<T>(Action<object, EventArgs> theCallback)
    {
        if (theCallback != null)
        {
            Type typeFromHandle = typeof(T);
            if (!itsListEventsRemove.ContainsKey(typeFromHandle))
            {
                itsListEventsRemove[typeFromHandle] = new KGFDelegate();
            }
            Dictionary<Type, KGFDelegate> dictionary;
            Dictionary<Type, KGFDelegate> dictionary2 = dictionary = itsListEventsRemove;
            Type key;
            Type key2 = key = typeFromHandle;
            KGFDelegate theMyDelegate = dictionary[key];
            dictionary2[key2] = theMyDelegate + theCallback;
        }
    }

    public static void UnregisterRemoveEvent<T>(Action<object, EventArgs> theCallback)
    {
        Type typeFromHandle = typeof(T);
        if (itsListEventsRemove.ContainsKey(typeFromHandle))
        {
            Dictionary<Type, KGFDelegate> dictionary;
            Dictionary<Type, KGFDelegate> dictionary2 = dictionary = itsListEventsRemove;
            Type key;
            Type key2 = key = typeFromHandle;
            KGFDelegate theMyDelegate = dictionary[key];
            dictionary2[key2] = theMyDelegate - theCallback;
        }
    }

    public static IEnumerable<T> GetObjectsEnumerable<T>()
    {
        foreach (object anObject in GetObjectsEnumerable(typeof(T)))
        {
            yield return (T)anObject;
        }
    }

    public static IEnumerable<object> GetObjectsEnumerable(Type theType)
    {
        foreach (Type aType in itsListSorted.Keys)
        {
            if (theType.IsAssignableFrom(aType))
            {
                List<KGFObject> aListObjectScripts = itsListSorted[aType];
                for (int i = aListObjectScripts.Count - 1; i >= 0; i--)
                {
                    object anObject = aListObjectScripts[i];
                    MonoBehaviour aMonobehaviour = aListObjectScripts[i];
                    if (aMonobehaviour == null)
                    {
                        aListObjectScripts.RemoveAt(i);
                    }
                    else if (aMonobehaviour.gameObject == null)
                    {
                        aListObjectScripts.RemoveAt(i);
                    }
                    else
                    {
                        yield return anObject;
                    }
                }
            }
        }
    }

    public static List<T> GetObjects<T>()
    {
        return new List<T>(GetObjectsEnumerable<T>());
    }

    public static List<object> GetObjects(Type theType)
    {
        return new List<object>(GetObjectsEnumerable(theType));
    }

    public static IEnumerable<string> GetObjectsNames<T>() where T : KGFObject
    {
        foreach (T anObject in GetObjects<T>())
        {
            yield return anObject.name;
        }
    }

    public static T GetObject<T>()
    {
        using (IEnumerator<T> enumerator = GetObjectsEnumerable<T>().GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
        }
        return default(T);
    }

    public static object GetObject(Type theType)
    {
        using (IEnumerator<object> enumerator = GetObjectsEnumerable(theType).GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
        }
        return null;
    }

    public static int GetAddHandlerCount()
    {
        return itsListEventsAdd.Count;
    }

    public static int GetAddOnceHandlerCount()
    {
        return itsListEventsAddOnce.Count;
    }

    public static IEnumerable<Type> GetObjectCacheListTypes()
    {
        foreach (Type key in itsListSorted.Keys)
        {
            yield return key;
        }
    }

    public static int GetObjectCacheListCountByType(Type theType)
    {
        if (itsListSorted.ContainsKey(theType))
        {
            return itsListSorted[theType].Count;
        }
        return 0;
    }
}
