using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapContour : MonoBehaviour
{
    public Vector2 center;

    private List<MapEdge> edges;

    public bool refresh;

    public List<Tuple<Vector2, Vector2>> FlatEdges
    {
        get;
        private set;
    }

    private void Awake()
    {
        Refresh();
    }

    private void Update()
    {
        if (refresh)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        refresh = false;
        edges = new List<MapEdge>();
        edges.AddRange(GetComponentsInChildren<MapEdge>());
        edges.Sort(new EdgeSorter());
        FlatEdges = new List<Tuple<Vector2, Vector2>>();
        Vector2 vector = default(Vector2);
        Vector2 item = default(Vector2);
        for (int i = 0; i < edges.Count; i++)
        {
            int index = (i + 1 < edges.Count) ? (i + 1) : 0;
            Vector3 position = edges[i].transform.position;
            float x = position.x;
            Vector3 position2 = edges[i].transform.position;
            vector = new Vector2(x, position2.z);
            Vector3 position3 = edges[index].transform.position;
            float x2 = position3.x;
            Vector3 position4 = edges[index].transform.position;
            item = new Vector2(x2, position4.z);
            FlatEdges.Add(new Tuple<Vector2, Vector2>(vector, item));
            center += vector;
        }
        center /= (float)FlatEdges.Count;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < edges.Count; i++)
        {
            int index = (i + 1 < edges.Count) ? (i + 1) : 0;
            Debug.DrawLine(edges[i].transform.position, edges[index].transform.position, Color.white);
            Debug.DrawLine(edges[i].transform.position + Vector3.up * 25f, edges[index].transform.position + Vector3.up * 25f, Color.white);
            Debug.DrawLine(edges[i].transform.position + Vector3.up * 50f, edges[index].transform.position + Vector3.up * 50f, Color.white);
        }
    }
}
