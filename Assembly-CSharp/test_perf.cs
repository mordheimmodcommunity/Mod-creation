using System.Collections.Generic;
using UnityEngine;

public class test_perf : MonoBehaviour
{
    public List<GameObject> prefabs;

    public GameObject anchor;

    public float lag = 0.05f;

    public int limit = 1000;

    public bool markStatic;

    private List<GameObject> generatedObj;

    private float timer;

    private Rect countLabelRect;

    private Rect modelLabelRect;

    private int currentPrefabidx;

    private RaycastHit hitInfo;

    private LayerMask layerMask;

    private GameObject autoGenerate;

    private int autoCount;

    private void Start()
    {
        currentPrefabidx = 0;
        countLabelRect = new Rect(0f, 0f, 200f, 50f);
        modelLabelRect = new Rect(200f, 0f, 200f, 50f);
        layerMask = 512;
        generatedObj = new List<GameObject>();
    }

    private void Update()
    {
        if (generatedObj.Count < limit && Input.GetMouseButton(0) && timer >= 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Ray ray = base.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo, 100000f, layerMask.value) && currentPrefabidx < prefabs.Count)
                {
                    GameObject gameObject = Object.Instantiate(prefabs[currentPrefabidx], hitInfo.point, Quaternion.identity) as GameObject;
                    gameObject.transform.parent = anchor.transform;
                    gameObject.isStatic = markStatic;
                    generatedObj.Add(gameObject);
                    Debug.DrawRay(ray.origin, hitInfo.point - ray.origin, Color.red);
                }
                timer = lag;
            }
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            currentPrefabidx = ((currentPrefabidx - 1 < 0) ? (prefabs.Count - 1) : (currentPrefabidx - 1));
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            currentPrefabidx = ((currentPrefabidx + 1 < prefabs.Count) ? (currentPrefabidx + 1) : 0);
        }
        if (autoGenerate != null)
        {
            if (generatedObj.Count < limit)
            {
                int num = (int)Mathf.Ceil(Mathf.Sqrt(limit));
                int num2 = generatedObj.Count % num;
                int num3 = generatedObj.Count / num;
                GameObject gameObject2 = Object.Instantiate(autoGenerate, new Vector3((num2 - num / 2) * 3, 0f, (num3 - num / 2) * 3), Quaternion.identity) as GameObject;
                gameObject2.transform.parent = anchor.transform;
                gameObject2.isStatic = markStatic;
                generatedObj.Add(gameObject2);
            }
            else
            {
                autoGenerate = null;
            }
        }
    }

    private void OnGUI()
    {
        if (prefabs.Count > 0)
        {
            GUI.Label(countLabelRect, "Prefab Count  : " + generatedObj.Count + " / " + limit);
            GUI.Label(modelLabelRect, "Current Model : " + prefabs[currentPrefabidx].name);
        }
        if (GUI.Button(new Rect(0f, 50f, 200f, 50f), "Combine"))
        {
            StaticBatchingUtility.Combine(anchor);
        }
        if (GUI.Button(new Rect(0f, 100f, 200f, 50f), "AutoGenerate"))
        {
            Clear();
            autoGenerate = prefabs[currentPrefabidx];
        }
        if (GUI.Button(new Rect(0f, 150f, 200f, 50f), "Clear"))
        {
            Clear();
        }
    }

    private void Clear()
    {
        foreach (GameObject item in generatedObj)
        {
            Object.Destroy(item);
        }
        generatedObj.Clear();
    }
}
