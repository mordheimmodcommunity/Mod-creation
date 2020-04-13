using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class KGFUtility
{
    public struct Point
    {
        public int X;

        public int Y;
    }

    public struct RECT
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    private static bool itsMouseRectActive = false;

    private static RECT itsOriginalClippingRect;

    private static Rect itsCachedRect = default(Rect);

    private static Vector3 itsCachedVector3 = default(Vector3);

    private static Vector2 itsCachedVector2 = default(Vector2);

    public static T[] GetComponentsInterface<T>(this MonoBehaviour theMonobehaviour) where T : class
    {
        List<T> list = new List<T>();
        MonoBehaviour[] components = theMonobehaviour.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour monoBehaviour in components)
        {
            T val = monoBehaviour as T;
            if (val != null)
            {
                list.Add(val);
            }
        }
        return list.ToArray();
    }

    public static T GetComponentInterface<T>(this MonoBehaviour theMonobehaviour) where T : class
    {
        T[] componentsInterface = theMonobehaviour.GetComponentsInterface<T>();
        if (componentsInterface.Length > 0)
        {
            return componentsInterface[0];
        }
        return (T)null;
    }

    public static List<T> Sorted<T>(this List<T> theList)
    {
        List<T> list = new List<T>(theList);
        list.Sort();
        return list;
    }

    public static bool ContainsItem<T>(this IEnumerable<T> theList, T theNeedle) where T : class
    {
        foreach (T the in theList)
        {
            if (theNeedle.Equals(the))
            {
                return true;
            }
        }
        return false;
    }

    public static string JoinToString<T>(this IEnumerable<T> theList, string theSeparator)
    {
        if (theList == null)
        {
            return string.Empty;
        }
        List<string> list = new List<string>();
        foreach (T the in theList)
        {
            list.Add(the.ToString());
        }
        return string.Join(theSeparator, list.ToArray());
    }

    public static IEnumerable<T> InsertItem<T>(this IEnumerable<T> theList, T theItem, int thePosition)
    {
        int i = 0;
        bool anInserted = false;
        foreach (T anElement in theList)
        {
            if (i == thePosition)
            {
                yield return theItem;
                anInserted = true;
            }
            yield return anElement;
            i++;
        }
        if (!anInserted)
        {
            yield return theItem;
        }
    }

    public static IEnumerable<T> AppendItem<T>(this IEnumerable<T> theList, T theItem)
    {
        foreach (T the in theList)
        {
            yield return the;
        }
        yield return theItem;
    }

    public static IEnumerable<T> Distinct<T>(this IEnumerable<T> theList)
    {
        List<T> aDistinctList = new List<T>();
        foreach (T anElement in theList)
        {
            if (!aDistinctList.Contains(anElement))
            {
                aDistinctList.Add(anElement);
                yield return anElement;
            }
        }
    }

    public static IEnumerable<T> Remove<T>(this IEnumerable<T> theMainList, T[] theListToRemove)
    {
        List<T> aListToRemove = new List<T>(theListToRemove);
        foreach (T anElement in theMainList)
        {
            if (!aListToRemove.Contains(anElement))
            {
                yield return anElement;
            }
        }
    }

    public static IEnumerable<T> Sorted<T>(this IEnumerable<T> theList)
    {
        List<T> aList = new List<T>(theList);
        aList.Sort();
        foreach (T item in aList)
        {
            yield return item;
        }
    }

    public static IEnumerable<T> Sorted<T>(this IEnumerable<T> theList, Comparison<T> theComparison)
    {
        List<T> aList = new List<T>(theList);
        aList.Sort(theComparison);
        foreach (T item in aList)
        {
            yield return item;
        }
    }

    public static List<T> ToDynList<T>(this IEnumerable<T> theList)
    {
        return new List<T>(theList);
    }

    public static void SetScaleRecursively(this Transform theTransform, Vector3 theScale)
    {
        foreach (Transform item in theTransform)
        {
            item.SetScaleRecursively(theScale);
        }
        theTransform.localScale = theScale;
    }

    public static void SetChildrenActiveRecursively(this GameObject theGameObject, bool theActive)
    {
        foreach (Transform item in theGameObject.transform)
        {
            item.gameObject.SetActiveRecursively(theActive);
        }
    }

    public static void SetLayerRecursively(this GameObject theGameObject, int theLayer)
    {
        theGameObject.layer = theLayer;
        foreach (Transform item in theGameObject.transform)
        {
            GameObject gameObject = item.gameObject;
            gameObject.SetLayerRecursively(theLayer);
        }
    }

    public static long DateToUnix(this DateTime theDate)
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static string Shortened(this string theString, int theMaxLength)
    {
        if (theString.Length > theMaxLength)
        {
            return theString.Substring(0, theMaxLength - 2) + "..";
        }
        return theString;
    }

    public static string Join(this string theSeparator, params string[] theItems)
    {
        return string.Join(theSeparator, theItems);
    }

    public static string Join(this string theSeparator, IEnumerable<string> theItems)
    {
        return string.Join(theSeparator, new List<string>(theItems).ToArray());
    }

    public static string RemoveRight(this string theString, char theSeparator)
    {
        string text = string.Empty + theString;
        while (text.Length > 0 && text[text.Length - 1] != theSeparator)
        {
            text = text.Remove(text.Length - 1);
        }
        return text;
    }

    public static string GetLastPart(this string theString, char theSeparator)
    {
        string[] array = theString.Split(new char[1]
        {
            theSeparator
        });
        return array[array.Length - 1];
    }

    public static string ConvertPathToUnity(string thePlatformPath)
    {
        return thePlatformPath.Replace(Path.DirectorySeparatorChar, '/');
    }

    public static string ConvertPathToPlatformSpecific(string theUnityPath)
    {
        return theUnityPath.Replace('/', Path.DirectorySeparatorChar);
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out Point _point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetCursorPos(int _x, int _y);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ClipCursor(ref RECT rcClip);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClipCursor(out RECT rcClip);

    [DllImport("user32.dll")]
    private static extern int GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(int hWnd, ref RECT lpRect);

    public static void SetMouseRect(Rect theRect)
    {
        RECT rcClip = new RECT((int)theRect.x, (int)theRect.y, (int)theRect.xMax, (int)theRect.yMax);
        if (itsMouseRectActive)
        {
            ClearMouseRect();
        }
        itsOriginalClippingRect = default(RECT);
        GetClipCursor(out itsOriginalClippingRect);
        ClipCursor(ref rcClip);
        itsMouseRectActive = true;
    }

    public static void ClearMouseRect()
    {
        if (itsMouseRectActive)
        {
            ClipCursor(ref itsOriginalClippingRect);
            itsMouseRectActive = false;
        }
    }

    public static Rect GetWindowRect()
    {
        int foregroundWindow = GetForegroundWindow();
        RECT lpRect = default(RECT);
        GetWindowRect(foregroundWindow, ref lpRect);
        return new Rect(lpRect.Left, lpRect.Top, lpRect.Right, lpRect.Bottom);
    }

    public static float PingPong(float theTime, float theMaxValue, float thePingStayTime, float thePongStayTime, float theTransitionTime)
    {
        float num = thePingStayTime + thePongStayTime + 2f * theTransitionTime;
        float num2 = theTime % num;
        if (num2 < thePingStayTime)
        {
            return 0f;
        }
        if (num2 < thePingStayTime + theTransitionTime)
        {
            return (num2 - thePingStayTime) * theMaxValue / theTransitionTime;
        }
        if (num2 < thePingStayTime + theTransitionTime + thePongStayTime)
        {
            return theMaxValue;
        }
        return theMaxValue - (num2 - (thePingStayTime + theTransitionTime + thePongStayTime)) * theMaxValue / theTransitionTime;
    }

    private static Color32[] BlockBlur1D(Color32[] thePixels, int theWidth, int theHeight, int theBlurRadius)
    {
        Color32[] array = new Color32[thePixels.Length];
        for (int i = 0; i < theHeight; i++)
        {
            for (int j = 0; j < theWidth; j++)
            {
                int num2;
                int num;
                int num3 = num2 = (num = 0);
                int num4 = 0;
                for (int k = j - theBlurRadius; k <= j + theBlurRadius; k++)
                {
                    Color32 color = thePixels[Mathf.Clamp(k, 0, theWidth - 1) + i * theWidth];
                    num3 += color.r;
                    num2 += color.g;
                    num += color.b;
                    num4++;
                }
                Color32 color2 = thePixels[j + i * theWidth];
                color2.r = (byte)(num3 / num4);
                color2.g = (byte)(num2 / num4);
                color2.b = (byte)(num / num4);
                array[j + i * theWidth] = color2;
            }
        }
        return array;
    }

    private static Color32[] BlockBlur2D(Color32[] thePixels, int theWidth, int theHeight, int theBlurRadiusX, int theBlurRadiusY)
    {
        Color32[] array = new Color32[thePixels.Length];
        for (int i = 0; i < theHeight; i++)
        {
            for (int j = 0; j < theWidth; j++)
            {
                int num2;
                int num;
                int num3 = num2 = (num = 0);
                int num4 = (j - theBlurRadiusX >= 0) ? (j - theBlurRadiusX) : 0;
                int num5 = (i - theBlurRadiusY >= 0) ? (i - theBlurRadiusY) : 0;
                int num6 = 0;
                for (int k = num5; k < theHeight && k <= i + theBlurRadiusY; k++)
                {
                    for (int l = num4; l < theWidth && l <= j + theBlurRadiusX; l++)
                    {
                        Color32 color = thePixels[l + k * theWidth];
                        num3 += color.r;
                        num2 += color.g;
                        num += color.b;
                        num6++;
                    }
                }
                Color32 color2 = thePixels[j + i * theWidth];
                color2.r = (byte)(num3 / num6);
                color2.g = (byte)(num2 / num6);
                color2.b = (byte)(num / num6);
                array[j + i * theWidth] = color2;
            }
        }
        return array;
    }

    public static Rect GetCachedRect(float theX, float theY, float theWidth, float theHeight)
    {
        itsCachedRect.x = theX;
        itsCachedRect.y = theY;
        itsCachedRect.width = theWidth;
        itsCachedRect.height = theHeight;
        return itsCachedRect;
    }

    public static Rect GetCachedRect(Rect theRect)
    {
        itsCachedRect.x = theRect.x;
        itsCachedRect.y = theRect.y;
        itsCachedRect.width = theRect.width;
        itsCachedRect.height = theRect.height;
        return itsCachedRect;
    }

    public static Vector3 GetCachedVector3(float theX, float theY, float theZ)
    {
        itsCachedVector3.x = theX;
        itsCachedVector3.y = theY;
        itsCachedVector3.z = theZ;
        return itsCachedVector3;
    }

    public static Vector2 GetCachedVector2(float theX, float theY)
    {
        itsCachedVector2.x = theX;
        itsCachedVector2.y = theY;
        return itsCachedVector2;
    }

    public static DateTime DateFromUnix(long theSeconds)
    {
        return new DateTime(1970, 1, 1).AddSeconds(theSeconds);
    }

    public static string ToHexString(byte[] buffer)
    {
        string text = string.Empty;
        foreach (byte b in buffer)
        {
            text += $"{b:x02}";
        }
        return text;
    }

    public static Texture2D GetBestAspectMatchingTexture(float theAspectRatio, params Texture2D[] theTextures)
    {
        Texture2D texture2D = null;
        if (theTextures.Length > 0)
        {
            texture2D = theTextures[0];
            for (int i = 1; i < theTextures.Length; i++)
            {
                Texture2D texture2D2 = theTextures[i];
                if (!(texture2D2 == null))
                {
                    float num = Mathf.Abs(theAspectRatio - (float)texture2D.width / (float)texture2D.height);
                    float num2 = Mathf.Abs(theAspectRatio - (float)texture2D2.width / (float)texture2D2.height);
                    if (num2 < num)
                    {
                        texture2D = texture2D2;
                    }
                }
            }
        }
        return texture2D;
    }

    public static Quaternion SetLookRotationSafe(Quaternion theQuaternion, Vector3 theUpVector, Vector3 theLookRotation, Vector3 theAlternativeLookDirection)
    {
        if (theAlternativeLookDirection.magnitude == 0f)
        {
            throw new Exception("Alternative look vector can never be 0!");
        }
        if (theLookRotation.magnitude != 0f)
        {
            theQuaternion.SetLookRotation(theLookRotation, theUpVector);
            return theQuaternion;
        }
        theQuaternion.SetLookRotation(theAlternativeLookDirection, theUpVector);
        return theQuaternion;
    }
}
