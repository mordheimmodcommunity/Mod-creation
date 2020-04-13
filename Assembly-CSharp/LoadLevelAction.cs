using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LoadLevelAction : MonoBehaviour
{
    public bool loadOnStart;

    public string levelName;

    public bool isAsync;

    public bool isAdditive;

    public UnityEvent loaded;

    private void Awake()
    {
    }

    public void LoadLevel()
    {
        Debug.Log("LoadLevel : async = " + isAsync + ", additive = " + isAdditive + ", levelname = " + levelName);
        if (isAsync)
        {
            if (isAdditive)
            {
                StartCoroutine(WaitLoading(Application.LoadLevelAdditiveAsync(levelName)));
            }
            else
            {
                StartCoroutine(WaitLoading(Application.LoadLevelAsync(levelName)));
            }
        }
        else if (isAdditive)
        {
            Application.LoadLevelAdditive(levelName);
            loaded.Invoke();
        }
        else
        {
            Application.LoadLevel(levelName);
            loaded.Invoke();
        }
    }

    private IEnumerator WaitLoading(AsyncOperation loading)
    {
        while (!loading.isDone)
        {
            yield return null;
        }
        loaded.Invoke();
    }
}
