using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Serializable]
public class KGFEvent : KGFEventBase, KGFIValidator
{
    [Serializable]
    public class KGFEventData
    {
        public bool itsRuntimeObjectSearch;

        public string itsRuntimeObjectSearchType = string.Empty;

        public string itsRuntimeObjectSearchFilter = string.Empty;

        public GameObject itsObject;

        public string itsComponentName = string.Empty;

        public string itsMethodName = string.Empty;

        public string itsMethodNameShort = string.Empty;

        public EventParameter[] itsParameters = new EventParameter[0];

        public EventParameterType[] itsParameterTypes = new EventParameterType[0];

        public bool itsPassthroughMode;

        private KGFEventFilterMethod itsFilterMethod;

        public KGFEventData()
        {
        }

        public KGFEventData(bool thePassThroughMode, params EventParameterType[] theParameterTypes)
        {
            itsParameterTypes = theParameterTypes;
            itsPassthroughMode = thePassThroughMode;
        }

        public Type GetRuntimeType()
        {
            return Type.GetType(itsRuntimeObjectSearchType);
        }

        public bool GetDirectPassThroughMode()
        {
            return itsPassthroughMode;
        }

        public void SetDirectPassThroughMode(bool thePassThroughMode)
        {
            itsPassthroughMode = thePassThroughMode;
        }

        public void SetRuntimeParameterInfos(params EventParameterType[] theParameterTypes)
        {
            if (theParameterTypes == null)
            {
                itsParameterTypes = new EventParameterType[0];
            }
            else
            {
                itsParameterTypes = theParameterTypes;
            }
        }

        public EventParameterType[] GetParameterLinkTypes()
        {
            return itsParameterTypes;
        }

        public bool GetSupportsRuntimeParameterInfos()
        {
            return itsParameterTypes.Length > 0;
        }

        public bool GetIsParameterLinked(int theParameterIndex)
        {
            if (!GetSupportsRuntimeParameterInfos())
            {
                return false;
            }
            if (theParameterIndex >= itsParameters.Length)
            {
                return false;
            }
            return itsParameters[theParameterIndex].itsLinked;
        }

        public void SetIsParameterLinked(int theParameterIndex, bool theLinkState)
        {
            if (theParameterIndex < itsParameters.Length)
            {
                itsParameters[theParameterIndex].itsLinked = theLinkState;
            }
        }

        public int GetParameterLink(int theParameterIndex)
        {
            if (theParameterIndex >= itsParameters.Length)
            {
                return 0;
            }
            return itsParameters[theParameterIndex].itsLink;
        }

        public void SetParameterLink(int theParameterIndex, int theLink)
        {
            if (theParameterIndex < itsParameters.Length)
            {
                itsParameters[theParameterIndex].itsLink = theLink;
            }
        }

        public EventParameter[] GetParameters()
        {
            return itsParameters;
        }

        public void SetParameters(EventParameter[] theParameters)
        {
            itsParameters = theParameters;
        }

        public GameObject GetGameObject()
        {
            return itsObject;
        }

        private object GetFieldValueByReflection(MonoBehaviour theCaller, string theMemberName)
        {
            Type type = theCaller.GetType();
            return type.GetField(theMemberName)?.GetValue(theCaller);
        }

        public void Trigger(MonoBehaviour theCaller, params object[] theParameters)
        {
            List<object> list = new List<object>(theParameters);
            EventParameterType[] array = itsParameterTypes;
            foreach (EventParameterType eventParameterType in array)
            {
                if (eventParameterType.GetCopyFromSourceObject())
                {
                    list.Add(GetFieldValueByReflection(theCaller, eventParameterType.itsName));
                }
            }
            if (itsRuntimeObjectSearch)
            {
                TriggerRuntimeSearch(theCaller, list.ToArray());
            }
            else
            {
                TriggerDefault(theCaller, list.ToArray());
            }
        }

        private int GetParameterIndexWithType(int theIndex, string theType)
        {
            int num = 0;
            for (int i = 0; i < itsParameterTypes.Length; i++)
            {
                EventParameterType eventParameterType = itsParameterTypes[i];
                if (eventParameterType.itsTypeName == theType)
                {
                    if (num == theIndex)
                    {
                        return i;
                    }
                    num++;
                }
            }
            return 0;
        }

        private bool CheckRuntimeObjectName(MonoBehaviour theMonobehaviour)
        {
            if (itsRuntimeObjectSearchFilter.Trim() == string.Empty)
            {
                return true;
            }
            if (itsRuntimeObjectSearchFilter == theMonobehaviour.name)
            {
                return true;
            }
            return false;
        }

        private void TriggerRuntimeSearch(MonoBehaviour theCaller, object[] theRuntimeParameters)
        {
            Type runtimeType = GetRuntimeType();
            if ((object)runtimeType == null)
            {
                LogError("could not find type", "KGFEventSystem", theCaller);
                return;
            }
            if (itsMethodName == null)
            {
                LogError("event has no selected method", "KGFEventSystem", theCaller);
                return;
            }
            if (!FindMethod(this, out MethodInfo theMethod, out MonoBehaviour _))
            {
                LogError("Could not find method on object.", "KGFEventSystem", theCaller);
                return;
            }
            object[] array = null;
            if (GetDirectPassThroughMode())
            {
                array = theRuntimeParameters;
            }
            else
            {
                ParameterInfo[] parameters = theMethod.GetParameters();
                array = ConvertParameters(parameters, itsParameters);
                for (int i = 0; i < itsParameters.Length; i++)
                {
                    if (GetIsParameterLinked(i))
                    {
                        int parameterIndexWithType = GetParameterIndexWithType(GetParameterLink(i), parameters[i].ParameterType.FullName);
                        if (parameterIndexWithType < theRuntimeParameters.Length)
                        {
                            array[i] = theRuntimeParameters[parameterIndexWithType];
                        }
                        else
                        {
                            Debug.LogError("you did not give enough parameters");
                        }
                    }
                }
            }
            List<MonoBehaviour> list = new List<MonoBehaviour>();
            try
            {
                if (runtimeType.IsInterface || typeof(KGFObject).IsAssignableFrom(runtimeType))
                {
                    foreach (object @object in KGFAccessor.GetObjects(runtimeType))
                    {
                        MonoBehaviour monoBehaviour = @object as MonoBehaviour;
                        if (monoBehaviour != null && CheckRuntimeObjectName(monoBehaviour))
                        {
                            theMethod.Invoke(@object, array);
                            list.Add(monoBehaviour);
                        }
                    }
                }
                else if (!runtimeType.IsInterface)
                {
                    UnityEngine.Object[] array2 = UnityEngine.Object.FindObjectsOfType(runtimeType);
                    foreach (object obj in array2)
                    {
                        MonoBehaviour monoBehaviour2 = obj as MonoBehaviour;
                        if (monoBehaviour2 != null && CheckRuntimeObjectName(monoBehaviour2))
                        {
                            theMethod.Invoke(obj, array);
                            list.Add(monoBehaviour2);
                        }
                    }
                }
            }
            catch (Exception arg)
            {
                LogError("invoked method caused exception in event_generic:" + arg, "KGFEventSystem", theCaller);
            }
            List<string> list2 = new List<string>();
            if (array != null)
            {
                object[] array3 = array;
                foreach (object arg2 in array3)
                {
                    list2.Add(string.Empty + arg2);
                }
            }
            foreach (MonoBehaviour item in list)
            {
                string theMessage = string.Format("{0}({1}): {2} ({3})", item.name, itsRuntimeObjectSearchType, theMethod.Name, string.Join(",", list2.ToArray()));
                LogDebug(theMessage, "KGFEventSystem", theCaller);
            }
        }

        private void TriggerDefault(MonoBehaviour theCaller, params object[] theRuntimeParameters)
        {
            if (itsObject == null)
            {
                LogError("event has null object", "KGFEventSystem", theCaller);
                return;
            }
            if (itsComponentName == null)
            {
                LogError("event has no selected component", "KGFEventSystem", theCaller);
                return;
            }
            if (itsMethodName == null)
            {
                LogError("event has no selected method", "KGFEventSystem", theCaller);
                return;
            }
            if (!FindMethod(this, out MethodInfo theMethod, out MonoBehaviour theComponent))
            {
                LogError("Could not find method on object.", "KGFEventSystem", theCaller);
                return;
            }
            object[] array = null;
            if (GetDirectPassThroughMode())
            {
                array = theRuntimeParameters;
            }
            else
            {
                ParameterInfo[] parameters = theMethod.GetParameters();
                array = ConvertParameters(parameters, itsParameters);
                for (int i = 0; i < itsParameters.Length; i++)
                {
                    if (GetIsParameterLinked(i))
                    {
                        int parameterIndexWithType = GetParameterIndexWithType(GetParameterLink(i), parameters[i].ParameterType.FullName);
                        if (parameterIndexWithType < theRuntimeParameters.Length)
                        {
                            array[i] = theRuntimeParameters[parameterIndexWithType];
                        }
                        else
                        {
                            Debug.LogError("you did not give enough parameters");
                        }
                    }
                }
            }
            try
            {
                theMethod.Invoke(theComponent, array);
            }
            catch (Exception arg)
            {
                LogError("invoked method caused exception in event_generic:" + arg, "KGFEventSystem", theCaller);
            }
            List<string> list = new List<string>();
            if (array != null)
            {
                object[] array2 = array;
                foreach (object arg2 in array2)
                {
                    list.Add(string.Empty + arg2);
                }
            }
            string theMessage = string.Format("{0}({1}): {2} ({3})", itsObject.name, theComponent.GetType().Name, theMethod.Name, string.Join(",", list.ToArray()));
            LogDebug(theMessage, "KGFEventSystem", theCaller);
        }

        public void SetMethodFilter(KGFEventFilterMethod theFilter)
        {
            itsFilterMethod = theFilter;
        }

        public void ClearMethodFilter()
        {
            itsFilterMethod = null;
        }

        private KGFEventFilterMethod GetFilterMethod()
        {
            return itsFilterMethod;
        }

        public bool CheckMethod(MethodInfo theMethod)
        {
            if (itsFilterMethod != null && !GetFilterMethod()(theMethod))
            {
                return false;
            }
            if (GetSupportsRuntimeParameterInfos() && GetDirectPassThroughMode())
            {
                ParameterInfo[] parameters = theMethod.GetParameters();
                if (parameters.Length != itsParameterTypes.Length)
                {
                    return false;
                }
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!itsParameterTypes[i].GetIsMatchingType(parameters[i].ParameterType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public KGFMessageList GetErrors()
        {
            KGFMessageList kGFMessageList = new KGFMessageList();
            if (string.IsNullOrEmpty(itsMethodName))
            {
                kGFMessageList.AddError("Empty method name");
            }
            if (itsRuntimeObjectSearch && string.IsNullOrEmpty(itsRuntimeObjectSearchType))
            {
                kGFMessageList.AddError("Empty type field");
            }
            if (!FindMethod(this, out MethodInfo theMethod, out MonoBehaviour _))
            {
                kGFMessageList.AddError("Could not find method on object.");
            }
            else
            {
                ParameterInfo[] parameters = theMethod.GetParameters();
                for (int i = 0; i < itsParameters.Length; i++)
                {
                    if (!GetIsParameterLinked(i) && typeof(UnityEngine.Object).IsAssignableFrom(parameters[i].ParameterType) && itsParameters[i].itsValueUnityObject == null)
                    {
                        kGFMessageList.AddError("Empty unity object in parameters");
                    }
                }
            }
            return kGFMessageList;
        }
    }

    [Serializable]
    public class EventParameterType
    {
        public string itsName;

        public string itsTypeName;

        public bool itsCopyFromSourceObject;

        public EventParameterType()
        {
        }

        public EventParameterType(string theName, Type theType)
        {
            itsName = theName;
            itsTypeName = theType.FullName;
        }

        public void SetCopyFromSourceObject(bool theCopy)
        {
            itsCopyFromSourceObject = theCopy;
        }

        public bool GetCopyFromSourceObject()
        {
            return itsCopyFromSourceObject;
        }

        public bool GetIsMatchingType(Type theOtherParameterType)
        {
            return itsTypeName == theOtherParameterType.FullName;
        }
    }

    [Serializable]
    public class EventParameter
    {
        public int itsValueInt32;

        public string itsValueString;

        public float itsValueSingle;

        public double itsValueDouble;

        public Color itsValueColor;

        public Rect itsValueRect;

        public Vector2 itsValueVector2;

        public Vector3 itsValueVector3;

        public Vector4 itsValueVector4;

        public bool itsValueBoolean;

        public UnityEngine.Object itsValueUnityObject;

        public bool itsLinked;

        public int itsLink;

        public EventParameter()
        {
            itsValueUnityObject = null;
        }
    }

    public delegate bool KGFEventFilterMethod(MethodInfo theMethod);

    private const string itsEventCategory = "KGFEventSystem";

    public KGFEventData itsEventData = new KGFEventData();

    public void SetDestination(GameObject theGameObject, string theComponentName, string theMethodString)
    {
        itsEventData.itsObject = theGameObject;
        itsEventData.itsComponentName = theComponentName;
        itsEventData.itsMethodName = theMethodString;
    }

    private static bool FindMethod(KGFEventData theEventData, out MethodInfo theMethod, out MonoBehaviour theComponent)
    {
        theMethod = null;
        theComponent = null;
        if (theEventData.itsRuntimeObjectSearch)
        {
            MethodInfo[] methods = GetMethods(theEventData.GetRuntimeType(), theEventData);
            foreach (MethodInfo methodInfo in methods)
            {
                string methodString = GetMethodString(methodInfo);
                if (methodString == theEventData.itsMethodName)
                {
                    theMethod = methodInfo;
                    return true;
                }
            }
        }
        else if (theEventData.itsObject != null)
        {
            MonoBehaviour[] components = theEventData.itsObject.GetComponents<MonoBehaviour>();
            MonoBehaviour[] array = components;
            foreach (MonoBehaviour monoBehaviour in array)
            {
                if (!(monoBehaviour.GetType().Name == theEventData.itsComponentName))
                {
                    continue;
                }
                theComponent = monoBehaviour;
                MethodInfo[] methods2 = GetMethods(monoBehaviour.GetType(), theEventData);
                foreach (MethodInfo methodInfo2 in methods2)
                {
                    string methodString2 = GetMethodString(methodInfo2);
                    if (methodString2 == theEventData.itsMethodName)
                    {
                        theMethod = methodInfo2;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public override void Trigger()
    {
        itsEventData.Trigger(this);
    }

    private static bool SearchInstanceForVariable(Type theType, object theInstance, string theName, ref object theValue)
    {
        FieldInfo field = theType.GetField(theName);
        if ((object)field != null)
        {
            theValue = field.GetValue(theInstance);
            return true;
        }
        return false;
    }

    private static object[] ConvertParameters(ParameterInfo[] theMethodParametersList, EventParameter[] theParametersList)
    {
        object[] array = new object[theMethodParametersList.Length];
        for (int i = 0; i < theMethodParametersList.Length; i++)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(theMethodParametersList[i].ParameterType))
            {
                array[i] = theParametersList[i].itsValueUnityObject;
            }
            else if (!SearchInstanceForVariable(typeof(EventParameter), theParametersList[i], "itsValue" + theMethodParametersList[i].ParameterType.Name, ref array[i]))
            {
                Debug.LogError("could not find variable for type:" + theMethodParametersList[i].ParameterType.Name);
            }
        }
        return array;
    }

    public static MethodInfo[] GetMethods(Type theType, KGFEventData theData)
    {
        List<MethodInfo> list = new List<MethodInfo>();
        Type type = theType;
        while ((object)type != null)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            MethodInfo[] array = methods;
            foreach (MethodInfo methodInfo in array)
            {
                if (methodInfo.GetCustomAttributes(typeof(KGFEventExpose), inherit: true).Length > 0 && theData.CheckMethod(methodInfo))
                {
                    list.Add(methodInfo);
                }
            }
            type = type.BaseType;
        }
        return list.ToArray();
    }

    public static string GetMethodString(MethodInfo theMethod)
    {
        return theMethod.ToString();
    }

    public static void LogError(string theMessage, string theCategory, MonoBehaviour theCaller)
    {
        Debug.LogError(theMessage);
    }

    public static void LogDebug(string theMessage, string theCategory, MonoBehaviour theCaller)
    {
        Debug.Log(theMessage);
    }

    public static void LogWarning(string theMessage, string theCategory, MonoBehaviour theCaller)
    {
        Debug.LogWarning(theMessage);
    }

    public override KGFMessageList Validate()
    {
        KGFMessageList kGFMessageList = new KGFMessageList();
        if ((string.Empty + itsEventData.itsMethodName).Trim() == string.Empty)
        {
            kGFMessageList.AddError("itsMethod is empty");
        }
        if (!itsEventData.itsRuntimeObjectSearch)
        {
            if (itsEventData.itsObject == null)
            {
                kGFMessageList.AddError("itsObject == null");
            }
            if ((string.Empty + itsEventData.itsComponentName).Trim() == string.Empty)
            {
                kGFMessageList.AddError("itsScript is empty");
            }
            if (itsEventData.itsObject != null && !FindMethod(itsEventData, out MethodInfo _, out MonoBehaviour _))
            {
                kGFMessageList.AddError("method could not be found");
            }
        }
        if (itsEventData.itsRuntimeObjectSearch)
        {
            Type runtimeType = itsEventData.GetRuntimeType();
            if ((object)runtimeType == null)
            {
                kGFMessageList.AddError("could not find type");
            }
            else if (runtimeType.IsInterface)
            {
                kGFMessageList.AddWarning("you used an interface, please ensure that the objects you want to call the method on are derrived from KGFObject");
            }
            else
            {
                if (!typeof(MonoBehaviour).IsAssignableFrom(runtimeType))
                {
                    kGFMessageList.AddError("type must be derrived from Monobehaviour");
                }
                if (!typeof(KGFObject).IsAssignableFrom(runtimeType))
                {
                    kGFMessageList.AddWarning("please derrive from KGFObject because it will be faster to search");
                }
            }
        }
        return kGFMessageList;
    }
}
