using HighlightingSystem;
using UnityEngine;

public class TriggerPoint : MonoBehaviour
{
    public GameObject trigger;

    public string soundName;

    protected GameObject fx;

    private Animation anim;

    private AudioSource audioSource;

    [HideInInspector]
    public uint guid;

    public Highlighter Highlight
    {
        get;
        private set;
    }

    public void Init()
    {
        anim = GetComponent<Animation>();
        audioSource = GetComponent<AudioSource>();
        Renderer component = trigger.GetComponent<Renderer>();
        if (component != null)
        {
            component.enabled = false;
        }
        Highlight = base.gameObject.GetComponent<Highlighter>();
        if ((Object)(object)Highlight == null)
        {
            Highlight = base.gameObject.AddComponent<Highlighter>();
        }
        Highlight.seeThrough = false;
        if (soundName != string.Empty && audioSource != null)
        {
            PanFlute panFlute = base.gameObject.AddComponent<PanFlute>();
            panFlute.fluteType = Pan.Type.FX;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.enabled = false;
        }
    }

    public virtual void Trigger(UnitController currentUnit)
    {
        if (anim != null)
        {
            anim.Play();
        }
        if (fx != null)
        {
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localRotation = Quaternion.identity;
        }
        if (soundName != string.Empty && audioSource != null)
        {
            PandoraSingleton<Pan>.Instance.GetSound(soundName, cache: true, delegate (AudioClip clip)
            {
                audioSource.enabled = true;
                audioSource.PlayOneShot(clip);
            });
        }
    }

    public virtual void ActionOnUnit(UnitController currentUnit)
    {
    }

    public virtual bool IsActive()
    {
        return true;
    }
}
