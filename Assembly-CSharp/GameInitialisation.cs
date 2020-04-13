using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitialisation : MonoBehaviour
{
    private bool launched;

    private bool pandoraLoaded;

    private void Awake()
    {
    }

    private void Start()
    {
        StartCoroutine(LoadPandora());
    }

    private IEnumerator LoadPandora()
    {
        ResourceRequest req = Resources.LoadAsync("prefabs/pandora");
        yield return req;
        Object.Instantiate(req.asset);
        pandoraLoaded = true;
    }

    private void Update()
    {
        if (pandoraLoaded && PandoraSingleton<GameManager>.Instance.graphicOptionsSet && !launched)
        {
            launched = true;
            LaunchCopyrights();
        }
    }

    private void LaunchCopyrights()
    {
        PandoraDebug.LogDebug("Launching copyright scene");
        SceneManager.LoadScene("copyright", LoadSceneMode.Single);
    }
}
