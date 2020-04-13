using System;
using System.Reflection;
using UnityEngine;

namespace TNet
{
    public static class UnityTools
    {
        public static void Clear(object[] objs)
        {
            int i = 0;
            for (int num = objs.Length; i < num; i++)
            {
                objs[i] = null;
            }
        }

        private static void PrintException(Exception ex, CachedFunc ent, int funcID, string funcName, params object[] parameters)
        {
            string text = string.Empty;
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i != 0)
                    {
                        text += ", ";
                    }
                    text += parameters[i].GetType().ToString();
                }
            }
            string text2 = string.Empty;
            if (ent.parameters != null)
            {
                for (int j = 0; j < ent.parameters.Length; j++)
                {
                    if (j != 0)
                    {
                        text2 += ", ";
                    }
                    text2 += ent.parameters[j].ParameterType.ToString();
                }
            }
            string text3 = "Failed to call RFC ";
            if (string.IsNullOrEmpty(funcName))
            {
                string text4 = text3;
                text3 = text4 + "#" + funcID + " on " + ((ent.obj == null) ? "<null>" : ent.obj.GetType().ToString());
            }
            else
            {
                string text4 = text3;
                text3 = text4 + ent.obj.GetType() + "." + funcName;
            }
            text3 = ((ex.InnerException == null) ? (text3 + ": " + ex.Message + "\n") : (text3 + ": " + ex.InnerException.Message + "\n"));
            if (text != text2)
            {
                text3 = text3 + "  Expected args: " + text2 + "\n";
                text3 = text3 + "  Received args: " + text + "\n\n";
            }
            text3 = ((ex.InnerException == null) ? (text3 + ex.StackTrace + "\n") : (text3 + ex.InnerException.StackTrace + "\n"));
            Debug.LogError(text3);
        }

        public static bool ExecuteFirst(List<CachedFunc> rfcs, byte funcID, out object retVal, params object[] parameters)
        {
            //Discarded unreachable code: IL_00b0, IL_00cb
            retVal = null;
            for (int i = 0; i < rfcs.size; i++)
            {
                CachedFunc ent = rfcs[i];
                if (ent.id == funcID)
                {
                    if (ent.parameters == null)
                    {
                        ent.parameters = ent.func.GetParameters();
                    }
                    try
                    {
                        retVal = ((ent.parameters.Length != 1 || (object)ent.parameters[0].ParameterType != typeof(object[])) ? ent.func.Invoke(ent.obj, parameters) : ent.func.Invoke(ent.obj, new object[1]
                        {
                            parameters
                        }));
                        return retVal != null;
                    }
                    catch (Exception ex)
                    {
                        PrintException(ex, ent, funcID, string.Empty, parameters);
                        return false;
                    }
                }
            }
            Debug.LogError("[TNet] Unable to find an function with ID of " + funcID);
            return false;
        }

        public static bool ExecuteAll(List<CachedFunc> rfcs, byte funcID, params object[] parameters)
        {
            //Discarded unreachable code: IL_00a6, IL_00c1
            for (int i = 0; i < rfcs.size; i++)
            {
                CachedFunc ent = rfcs[i];
                if (ent.id == funcID)
                {
                    if (ent.parameters == null)
                    {
                        ent.parameters = ent.func.GetParameters();
                    }
                    try
                    {
                        if (ent.parameters.Length == 1 && (object)ent.parameters[0].ParameterType == typeof(object[]))
                        {
                            ent.func.Invoke(ent.obj, new object[1]
                            {
                                parameters
                            });
                        }
                        else
                        {
                            ent.func.Invoke(ent.obj, parameters);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        PrintException(ex, ent, funcID, string.Empty, parameters);
                        return false;
                    }
                }
            }
            Debug.LogError("[TNet] Unable to find an function with ID of " + funcID);
            return false;
        }

        public static bool ExecuteAll(List<CachedFunc> rfcs, string funcName, params object[] parameters)
        {
            //Discarded unreachable code: IL_00b5
            bool result = false;
            for (int i = 0; i < rfcs.size; i++)
            {
                CachedFunc ent = rfcs[i];
                if (ent.func.Name == funcName)
                {
                    result = true;
                    if (ent.parameters == null)
                    {
                        ent.parameters = ent.func.GetParameters();
                    }
                    try
                    {
                        if (ent.parameters.Length == 1 && (object)ent.parameters[0].ParameterType == typeof(object[]))
                        {
                            ent.func.Invoke(ent.obj, new object[1]
                            {
                                parameters
                            });
                        }
                        else
                        {
                            ent.func.Invoke(ent.obj, parameters);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        PrintException(ex, ent, 0, funcName, parameters);
                    }
                }
            }
            Debug.LogError("[TNet] Unable to find a function called '" + funcName + "'");
            return result;
        }

        public static void Broadcast(string methodName, params object[] parameters)
        {
            MonoBehaviour[] array = UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
            int i = 0;
            for (int num = array.Length; i < num; i++)
            {
                MonoBehaviour monoBehaviour = array[i];
                MethodInfo method = monoBehaviour.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if ((object)method != null)
                {
                    try
                    {
                        method.Invoke(monoBehaviour, parameters);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message + " (" + monoBehaviour.GetType() + "." + methodName + ")", monoBehaviour);
                    }
                }
            }
        }

        public static float SpringLerp(float from, float to, float strength, float deltaTime)
        {
            if (deltaTime > 1f)
            {
                deltaTime = 1f;
            }
            int num = Mathf.RoundToInt(deltaTime * 1000f);
            deltaTime = 0.001f * strength;
            for (int i = 0; i < num; i++)
            {
                from = Mathf.Lerp(from, to, deltaTime);
            }
            return from;
        }

        public static Rect PadRect(Rect rect, float padding)
        {
            Rect result = rect;
            result.xMin -= padding;
            result.xMax += padding;
            result.yMin -= padding;
            result.yMax += padding;
            return result;
        }

        public static bool IsParentChild(GameObject parent, GameObject child)
        {
            if (parent == null || child == null)
            {
                return false;
            }
            return IsParentChild(parent.transform, child.transform);
        }

        public static bool IsParentChild(Transform parent, Transform child)
        {
            if (parent == null || child == null)
            {
                return false;
            }
            while (child != null)
            {
                if (parent == child)
                {
                    return true;
                }
                child = child.parent;
            }
            return false;
        }

        public static GameObject Instantiate(GameObject go, Vector3 pos, Quaternion rot, Vector3 velocity, Vector3 angularVelocity)
        {
            if (go != null)
            {
                go = (UnityEngine.Object.Instantiate(go, pos, rot) as GameObject);
                Rigidbody component = go.GetComponent<Rigidbody>();
                if (component != null)
                {
                    if (component.isKinematic)
                    {
                        component.isKinematic = false;
                        component.velocity = velocity;
                        component.angularVelocity = angularVelocity;
                        component.isKinematic = true;
                    }
                    else
                    {
                        component.velocity = velocity;
                        component.angularVelocity = angularVelocity;
                    }
                }
            }
            return go;
        }
    }
}
