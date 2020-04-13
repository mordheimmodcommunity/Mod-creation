using System.Collections;
using UnityEngine;

public class UISound : MonoBehaviour
{
    private static UISound _instance;

    private AudioClip click;

    private AudioClip select;

    private AudioSource source;

    private bool playSelect;

    private bool playClick;

    private float time;

    public static UISound Instance => _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Object.Destroy(this);
            return;
        }
        _instance = this;
        Object.DontDestroyOnLoad(base.gameObject);
        source = GetComponent<AudioSource>();
        LoadSounds();
    }

    private IEnumerator WaitForPan()
    {
        while (!PandoraSingleton<Pan>.Instance.Initialized)
        {
            yield return null;
        }
        LoadSounds();
    }

    private void LoadSounds()
    {
        PandoraSingleton<Pan>.Instance.GetSound("interface/", "click", cache: true, delegate (AudioClip clip)
        {
            click = clip;
        });
        PandoraSingleton<Pan>.Instance.GetSound("interface/", "mouseover", cache: true, delegate (AudioClip clip)
        {
            select = clip;
        });
    }

    public void OnSelect()
    {
        playSelect = true;
    }

    public void OnClick()
    {
        playClick = true;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time > 0.1f)
        {
            if (playClick)
            {
                playSelect = false;
                playClick = false;
                source.PlayOneShot(click);
                time = 0f;
            }
            else if (playSelect)
            {
                playSelect = false;
                source.PlayOneShot(select);
                time = 0f;
            }
        }
    }
}
