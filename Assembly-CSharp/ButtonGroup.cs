using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonGroup : ImageGroup
{
    public ToggleEffects effects;

    public Text label;

    private string locTag;

    private bool justEnabled;

    protected override void Awake()
    {
        base.Awake();
        if (effects == null)
        {
            effects = GetComponentInChildren<ToggleEffects>();
        }
        if ((Object)(object)label == null)
        {
            label = GetComponentInChildren<Text>();
        }
        SetDisabled(disabled: false);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void Localize()
    {
        if ((Object)(object)label != null && locTag != null)
        {
            label.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(locTag));
        }
    }

    public override void SetAction(string pandoraAction, string locTag, int inputLayer = 0, bool negative = false, Sprite keyboardOverload = null, Sprite consoleOverload = null)
    {
        base.SetAction(pandoraAction, locTag, inputLayer, negative, keyboardOverload, consoleOverload);
        SetInteractable(inter: true);
        this.locTag = locTag;
        Localize();
    }

    public bool IsInteractable()
    {
        return ((Selectable)effects.toggle).get_interactable();
    }

    public void SetInteractable(bool inter)
    {
        ((Selectable)effects.toggle).set_interactable(inter);
    }

    public void SetDisabled(bool disabled = true)
    {
        effects.overrideColor = true;
        effects.enabled = !disabled;
        effects.toggle.set_isOn(effects.toggle.get_isOn() && !disabled);
        SetInteractable(!disabled);
    }

    private new void OnEnable()
    {
        justEnabled = true;
    }

    private void Update()
    {
        if (justEnabled)
        {
            justEnabled = false;
        }
        else if (action != null && callback != null && IsInteractable() && ((!negative && PandoraSingleton<PandoraInput>.Instance.GetKeyUp(action, inputLayer)) || (negative && PandoraSingleton<PandoraInput>.Instance.GetNegKeyUp(action, inputLayer))))
        {
            PandoraDebug.LogDebug("ButtonGroup Action = " + action + "Layer " + inputLayer);
            callback();
        }
    }

    public virtual void OnAction(UnityAction func, bool mouseOnly, bool clear = true)
    {
        justEnabled = true;
        if (clear)
        {
            effects.onAction.RemoveAllListeners();
        }
        if (!mouseOnly)
        {
            callback = func;
        }
        else
        {
            callback = null;
        }
        if (func != null)
        {
            effects.onAction.AddListener(func);
        }
    }
}
