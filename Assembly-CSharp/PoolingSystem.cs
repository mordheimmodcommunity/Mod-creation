using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem<T> where T : class
{
    private static Vector3 outOfTheWay = new Vector3(20000f, 20000f, 20000f);

    private List<GameObject> availableList = new List<GameObject>();

    private List<GameObject> inUseList = new List<GameObject>();

    private GameObject original;

    private bool isGameObject;

    public int InUse => inUseList.Count;

    public int Available => availableList.Count;

    public PoolingSystem(GameObject prefab, int initialSize)
    {
        GameObject gameObject = null;
        original = prefab;
        if ((object)typeof(T) == typeof(GameObject))
        {
            isGameObject = true;
        }
        for (int i = 0; i < initialSize; i++)
        {
            gameObject = (Object.Instantiate(original, outOfTheWay, Quaternion.identity) as GameObject);
            gameObject.SetActive(value: false);
            availableList.Add(gameObject);
        }
    }

    public void CleanUp()
    {
        inUseList.TrimExcess();
        availableList.TrimExcess();
    }

    public void ReleaseElement(T element, bool SetOutOfTheWay)
    {
        GameObject gameObject;
        if (isGameObject)
        {
            gameObject = (element as GameObject);
        }
        else
        {
            Component component = element as Component;
            gameObject = component.gameObject;
        }
        if (SetOutOfTheWay)
        {
            gameObject.transform.position = outOfTheWay;
        }
        gameObject.SetActive(value: false);
        inUseList.Remove(gameObject);
        availableList.Add(gameObject);
    }

    public T GetElement()
    {
        GameObject gameObject = null;
        if (availableList.Count == 0)
        {
            gameObject = (Object.Instantiate(original, outOfTheWay, Quaternion.identity) as GameObject);
            gameObject.SetActive(value: false);
            inUseList.Add(gameObject);
        }
        else
        {
            gameObject = availableList[0];
            availableList.RemoveAt(0);
            inUseList.Add(gameObject);
        }
        gameObject.SetActive(value: true);
        if (isGameObject)
        {
            return gameObject as T;
        }
        return gameObject.GetComponent(typeof(T)) as T;
    }
}
