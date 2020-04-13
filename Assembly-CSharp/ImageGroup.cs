using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageGroup : MonoBehaviour
{
    public Image button;

    public bool gamepadOnly;

    private Sprite keyboardImageOverload;

    private Sprite consoleImageOverload;

    public bool hideIconInKeyboard;

    protected string action;

    protected bool negative;

    protected int inputLayer;

    protected UnityAction callback;

    protected virtual void Awake()
    {
        if (!((Object)(object)button == null))
        {
            return;
        }
        Image[] componentsInChildren = GetComponentsInChildren<Image>(includeInactive: true);
        if (componentsInChildren.Length < 1)
        {
            return;
        }
        if (componentsInChildren.Length == 1)
        {
            button = componentsInChildren[0];
            return;
        }
        Image[] array = componentsInChildren;
        int num = 0;
        Image val;
        while (true)
        {
            if (num < array.Length)
            {
                val = array[num];
                LayoutElement component = ((Component)(object)val).GetComponent<LayoutElement>();
                if ((Object)(object)component == null || !component.get_ignoreLayout())
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        button = val;
    }

    protected virtual void Start()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.INPUT_TYPE_CHANGED, OnInputTypeChanged);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.OPTIONS_CLOSED, RefreshImage);
    }

    protected virtual void OnDestroy()
    {
        if (PandoraSingleton<NoticeManager>.Instance != null)
        {
            PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.INPUT_TYPE_CHANGED, OnInputTypeChanged);
            PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.OPTIONS_CLOSED, RefreshImage);
        }
    }

    protected virtual void OnInputTypeChanged()
    {
        RefreshImage();
    }

    protected virtual void OnEnable()
    {
        RefreshImage();
    }

    public virtual void SetAction(string pandoraAction, string locTag, int inputLayer = 0, bool negative = false, Sprite keyboardOverload = null, Sprite consoleOverload = null)
    {
        this.negative = negative;
        this.inputLayer = inputLayer;
        keyboardImageOverload = keyboardOverload;
        consoleImageOverload = consoleOverload;
        base.gameObject.SetActive(value: true);
        if (!string.IsNullOrEmpty(pandoraAction))
        {
            action = pandoraAction;
            RefreshImage();
            ((Component)(object)button).gameObject.SetActive(value: true);
        }
        else
        {
            ((Component)(object)button).gameObject.SetActive(value: false);
            action = null;
        }
    }

    public void RefreshImage()
    {
        //IL_00eb: Unknown result type (might be due to invalid IL or missing references)
        //IL_012b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0155: Unknown result type (might be due to invalid IL or missing references)
        //IL_017f: Unknown result type (might be due to invalid IL or missing references)
        //IL_01cf: Unknown result type (might be due to invalid IL or missing references)
        //IL_01db: Unknown result type (might be due to invalid IL or missing references)
        //IL_0230: Unknown result type (might be due to invalid IL or missing references)
        //IL_025a: Unknown result type (might be due to invalid IL or missing references)
        if (action == null || !((Object)(object)button != null))
        {
            return;
        }
        if (gamepadOnly)
        {
            if (PandoraSingleton<PandoraInput>.Instance.lastInputMode != PandoraInput.InputMode.JOYSTICK)
            {
                MaskableGraphic[] componentsInChildren = GetComponentsInChildren<MaskableGraphic>(includeInactive: true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    ((Behaviour)(object)componentsInChildren[i]).enabled = false;
                }
            }
            else
            {
                MaskableGraphic[] componentsInChildren2 = GetComponentsInChildren<MaskableGraphic>(includeInactive: true);
                for (int j = 0; j < componentsInChildren2.Length; j++)
                {
                    ((Behaviour)(object)componentsInChildren2[j]).enabled = true;
                }
            }
        }
        ((Component)(object)button).gameObject.SetActive(PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK || !hideIconInKeyboard);
        if (keyboardImageOverload != null)
        {
            button.set_sprite(keyboardImageOverload);
            return;
        }
        Pole inputPole = (Pole)(negative ? 1 : 0);
        string text = string.Empty;
        ActionElementMap val = null;
        switch (PandoraSingleton<PandoraInput>.Instance.lastInputMode)
        {
            case PandoraInput.InputMode.NONE:
            case PandoraInput.InputMode.KEYBOARD:
            case PandoraInput.InputMode.MOUSE:
                val = PandoraSingleton<PandoraInput>.Instance.GetInputForAction(action, (ControllerType)1, inputPole);
                if (val != null)
                {
                    text = val.get_elementIdentifierName();
                    break;
                }
                val = PandoraSingleton<PandoraInput>.Instance.GetInputForAction(action, (ControllerType)0, inputPole);
                if (val != null)
                {
                    text = val.get_elementIdentifierName();
                }
                break;
            case PandoraInput.InputMode.JOYSTICK:
                val = PandoraSingleton<PandoraInput>.Instance.GetInputForAction(action, (ControllerType)2, inputPole);
                if (val != null)
                {
                    string text2 = val.get_elementIdentifierName();
                    if (text2.EndsWith("-") || text2.EndsWith("+"))
                    {
                        text2 = text2.Substring(0, text2.Length - 2);
                    }
                    if ((int)val.get_elementType() == 0 && (int)val.get_axisRange() == 0)
                    {
                        text2 = ((!negative) ? (text2 + " +") : (text2 + " -"));
                    }
                    text = "joy_" + text2;
                    break;
                }
                val = PandoraSingleton<PandoraInput>.Instance.GetInputForAction(action, (ControllerType)1, inputPole);
                if (val != null)
                {
                    text = val.get_elementIdentifierName();
                    break;
                }
                val = PandoraSingleton<PandoraInput>.Instance.GetInputForAction(action, (ControllerType)0, inputPole);
                if (val != null)
                {
                    text = val.get_elementIdentifierName();
                }
                break;
        }
        if (!string.IsNullOrEmpty(text))
        {
            if (!InputImageTable.butTable.ContainsKey(text))
            {
                text = "???";
            }
            string str = InputImageTable.butTable[text];
            button.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("input/" + str, cached: true));
        }
    }
}
