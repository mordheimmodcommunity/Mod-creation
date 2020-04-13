using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class PandoraUtils
{
    private class RectCastData
    {
        public Vector3 startPos;

        public Vector3 dir;

        public float dist;

        public float radius;

        public int layerMask;

        public List<Collider> traversableColliders;
    }

    private const char under = '_';

    public static RaycastHit[] hits = new RaycastHit[1];

    private static readonly StringBuilder stringBuilder = new StringBuilder(1024);

    private static RectCastData castData = new RectCastData();

    private static List<Vector2> tempPoints = new List<Vector2>();

    public static StringBuilder StringBuilder
    {
        get
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            return stringBuilder;
        }
    }

    public static string UnderToCamel(string underscore, bool Upper = true)
    {
        StringBuilder stringBuilder = StringBuilder;
        for (int i = 0; i <= underscore.Length - 1; i++)
        {
            if (underscore[i] != '_')
            {
                if ((i == 0 && Upper) || (i != 0 && underscore[i - 1] == '_'))
                {
                    stringBuilder.Append(char.ToUpper(underscore[i]));
                }
                else
                {
                    stringBuilder.Append(char.ToLower(underscore[i]));
                }
            }
        }
        return stringBuilder.ToString();
    }

    public static string CamelToUnder(string camel)
    {
        StringBuilder stringBuilder = StringBuilder;
        for (int i = 0; i <= camel.Length - 1; i++)
        {
            if (char.IsUpper(camel[i]))
            {
                if (i != 0)
                {
                    stringBuilder.Append('_');
                }
                stringBuilder.Append(char.ToLowerInvariant(camel[i]));
            }
            else
            {
                stringBuilder.Append(camel[i]);
            }
        }
        return stringBuilder.ToString();
    }

    public static int GetCharIdxPos(char c, int idx, string str)
    {
        int num = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == c)
            {
                num++;
                if (num == idx)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public static float ManhattanDistance(Vector3 src, Vector3 dest)
    {
        return Mathf.Abs(src.x - dest.x) + Mathf.Abs(src.y - dest.y) + Mathf.Abs(src.z - dest.z);
    }

    public static float FlatSqrDistance(Vector3 src, Vector3 dest)
    {
        return Vector2.SqrMagnitude(new Vector2(src.x, src.z) - new Vector2(dest.x, dest.z));
    }

    public static void DrawFacingGizmoCube(Transform transform, float height, float width, float length, float offsetX = 0f, float offsetZ = 0f, bool drawTriangle = true)
    {
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length * -1f);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length * -1f, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length * -1f);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length * -1f, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length + transform.up * height, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length * -1f + transform.up * height);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length + transform.up * height, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length + transform.up * height);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length * -1f + transform.up * height, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length * -1f + transform.up * height);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length * -1f + transform.up * height, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length + transform.up * height);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length + transform.up * height);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length * -1f, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width + transform.right * length * -1f + transform.up * height);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length + transform.up * height);
        Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length * -1f, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width * -1f + transform.right * length * -1f + transform.up * height);
        if (drawTriangle)
        {
            Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.right * length * -1f);
            Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.forward * width, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.right * length);
            Gizmos.DrawLine(transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.right * length * -1f, transform.position + transform.forward * offsetZ + transform.right * offsetX + transform.right * length);
        }
    }

    public static int Round(float val)
    {
        return (int)val + ((val - (float)(int)val >= 0.5f) ? 1 : 0);
    }

    public static bool IsBetween(float val, float min, float max)
    {
        if (min > max)
        {
            return val >= max && val <= min;
        }
        return val >= min && val <= max;
    }

    public static bool IsBetween(int val, int min, int max)
    {
        if (min > max)
        {
            return val >= max && val <= min;
        }
        return val >= min && val <= max;
    }

    public static bool RectCast(Vector3 startPos, Vector3 dir, float dist, float height, float width, int layerMask, List<Collider> traversableColliders, out RaycastHit raycastHit, bool useSphere = true)
    {
        castData.startPos = startPos;
        castData.dir = dir.normalized;
        castData.dist = dist;
        castData.layerMask = layerMask;
        castData.traversableColliders = traversableColliders;
        Vector3 normalized = Vector3.Cross(Vector3.up, new Vector3(dir.x, 0f, dir.z)).normalized;
        raycastHit = default(RaycastHit);
        if (castData.dist <= 0f)
        {
            return true;
        }
        bool flag = true;
        flag &= !Physics.Raycast(startPos, normalized, width / 2f, layerMask);
        if (flag)
        {
            flag &= !Physics.Raycast(startPos, normalized * -1f, width / 2f, layerMask);
        }
        if (flag && useSphere)
        {
            castData.radius = Mathf.Min(width / 2f, height / 2f);
            castData.startPos = startPos;
            flag &= LaunchRectSphere(castData, out raycastHit);
        }
        if (flag)
        {
            castData.startPos = startPos;
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        if (flag)
        {
            castData.startPos = startPos + Vector3.up * height / 2f;
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        if (flag)
        {
            castData.startPos = startPos + Vector3.up * height / 2f * -1f;
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        castData.dist -= width / 2f;
        if (castData.dist <= 0f)
        {
            return true;
        }
        if (flag)
        {
            castData.startPos = startPos + normalized * width / 2f;
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        if (flag)
        {
            castData.startPos = startPos + normalized * (width / 2f * -1f);
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        if (flag)
        {
            castData.startPos = startPos + Vector3.up * height / 2f + normalized * width / 2f;
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        if (flag)
        {
            castData.startPos = startPos + Vector3.up * height / 2f + normalized * width / 2f * -1f;
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        if (flag)
        {
            castData.startPos = startPos + Vector3.up * height / 2f * -1f + normalized * width / 2f;
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        if (flag)
        {
            castData.startPos = startPos + Vector3.up * height / 2f * -1f + normalized * width / 2f * -1f;
            flag &= LaunchRectRay(castData, out raycastHit);
        }
        return flag;
    }

    private static bool LaunchRectRay(RectCastData castData, out RaycastHit raycastHit)
    {
        Physics.Raycast(castData.startPos, castData.dir, out raycastHit, castData.dist, castData.layerMask);
        return raycastHit.collider == null || (castData.traversableColliders != null && castData.traversableColliders.IndexOf(raycastHit.collider) != -1);
    }

    private static bool LaunchRectSphere(RectCastData castData, out RaycastHit raycastHit)
    {
        Physics.SphereCast(castData.startPos, castData.radius, castData.dir, out raycastHit, castData.dist, castData.layerMask);
        return raycastHit.collider == null || (castData.traversableColliders != null && castData.traversableColliders.IndexOf(raycastHit.collider) != -1);
    }

    public static void InsertDistinct<T>(ref List<T> list, T obj)
    {
        if (list.IndexOf(obj) == -1)
        {
            list.Add(obj);
        }
    }

    public static List<T> GetPercList<T>(ref List<T> list, float perc)
    {
        Shuffle(list);
        perc = Mathf.Clamp01(perc);
        int count = Mathf.CeilToInt(perc * (float)list.Count);
        return list.GetRange(0, count);
    }

    public static void Shuffle<T>(List<T> list)
    {
        int count = list.Count;
        while (count > 1)
        {
            int index = PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, count--);
            T value = list[index];
            list[index] = list[count];
            list[count] = value;
        }
    }

    public static Vector2 GetPointInsideMeshEdges(List<Tuple<Vector2, Vector2>> edges, Vector2 center, Vector2 src, Vector2 target)
    {
        IsPointInsideEdges(edges, src, target, out Vector2 point);
        IsPointInsideEdges(edges, (center - target) * 1000f, target, out Vector2 point2);
        return (!(Vector2.SqrMagnitude(target - point) < Vector2.SqrMagnitude(target - point2))) ? point2 : point;
    }

    public static bool IsPointInsideEdges(List<Tuple<Vector2, Vector2>> edges, Vector2 point, Vector2 checkDestPoint, float minEdgeDistance = -1f)
    {
        int num = 0;
        tempPoints.Clear();
        for (int i = 0; i < edges.Count; i++)
        {
            if (!LineIntersect(edges[i].Item1, edges[i].Item2, point, checkDestPoint, out Vector2 intersect))
            {
                continue;
            }
            if (minEdgeDistance >= 0f && (intersect - point).sqrMagnitude < minEdgeDistance * minEdgeDistance)
            {
                PandoraDebug.LogDebug("Discarding edge collision due to proximity.", "ENGAGE");
                continue;
            }
            bool flag = true;
            for (int j = 0; j < tempPoints.Count; j++)
            {
                Vector2 vector = tempPoints[j];
                if (Mathf.Approximately(vector.x, intersect.x))
                {
                    Vector2 vector2 = tempPoints[j];
                    if (Mathf.Approximately(vector2.y, intersect.y))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag)
            {
                tempPoints.Add(intersect);
                num++;
            }
        }
        return num % 2 == 1;
    }

    public static bool IsPointInsideEdges(List<Tuple<Vector2, Vector2>> edges, Vector2 src, Vector2 target, out Vector2 point)
    {
        Vector2 vector = Vector2.zero;
        float num = float.MaxValue;
        int num2 = 0;
        for (int i = 0; i < edges.Count; i++)
        {
            if (LineIntersect(edges[i].Item1, edges[i].Item2, src, target, out Vector2 intersect))
            {
                num2++;
                float num3 = Vector2.SqrMagnitude(intersect - target);
                if (num3 < num)
                {
                    vector = intersect;
                    num = num3;
                }
            }
        }
        if (num2 % 2 == 1)
        {
            point = target;
            return true;
        }
        point = vector;
        return false;
    }

    private static bool LineIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, out Vector2 intersect)
    {
        Vector2 vector = new Vector2(p1.x - p0.x, p1.y - p0.y);
        Vector2 vector2 = new Vector2(p3.x - p2.x, p3.y - p2.y);
        float num = ((0f - vector.y) * (p0.x - p2.x) + vector.x * (p0.y - p2.y)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
        float num2 = (vector2.x * (p0.y - p2.y) - vector2.y * (p0.x - p2.x)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
        if (num >= 0f && num <= 1f && num2 >= 0f && num2 <= 1f)
        {
            intersect = new Vector2(p0.x + num2 * vector.x, p0.y + num2 * vector.y);
            return true;
        }
        intersect = Vector2.zero;
        return false;
    }

    public static bool SendCapsule(Vector3 pos, Vector3 dir, float minHeight, float maxHeight, float maxDist, float radius, out RaycastHit rayHitData)
    {
        return Physics.CapsuleCast(pos + Vector3.up * minHeight, pos + Vector3.up * maxHeight, radius, dir, out rayHitData, maxDist, LayerMaskManager.decisionMask);
    }

    public static bool SendCapsule(Vector3 pos, Vector3 dir, float minHeight, float maxHeight, float maxDist, float radius)
    {
        return Physics.CapsuleCast(pos + Vector3.up * minHeight, pos + Vector3.up * maxHeight, radius, dir, maxDist, LayerMaskManager.decisionMask);
    }

    public static float SqrDistPointLineDist(Vector3 v1, Vector3 v2, Vector3 v0, bool isSegment)
    {
        float num = Vector3.SqrMagnitude(v1 - v2);
        if (num == 0f)
        {
            return Vector3.SqrMagnitude(v1 - v0);
        }
        float num2 = Vector3.Dot(v0 - v1, v2 - v1) / num;
        if (num2 < 0f)
        {
            return Vector3.SqrMagnitude(v0 - v1);
        }
        if (num2 > 1f)
        {
            return Vector3.SqrMagnitude(v0 - v2);
        }
        Vector3 a = v1 + num2 * (v2 - v1);
        return Vector3.SqrMagnitude(a - v0);
    }

    public static bool Approximately(Vector3 v1, Vector3 v2)
    {
        return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z);
    }

    public static Color StringToColor(string colorStr)
    {
        string[] array = colorStr.Split(new char[1]
        {
            ','
        });
        if (array.Length == 3)
        {
            return new Color(StringToColorChannel(array[0]), StringToColorChannel(array[1]), StringToColorChannel(array[2]));
        }
        if (array.Length == 4)
        {
            return new Color(StringToColorChannel(array[0]), StringToColorChannel(array[1]), StringToColorChannel(array[2]), StringToColorChannel(array[3]));
        }
        return default(Color);
    }

    private static float StringToColorChannel(string colorChannel)
    {
        return float.Parse(colorChannel, NumberFormatInfo.InvariantInfo) / 255f;
    }

    public static void RemoveBySwap<T>(List<T> list, int index)
    {
        list[index] = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
    }
}
