using Rewired;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RemapButtonPopupView : MonoBehaviour
{
    private const string INSTRUCTIONS_KEYB_STRING_ID = "menu_remap_action_instructions_keyboard";

    private const string INSTRUCTIONS_CTRL_STRING_ID = "menu_remap_action_instructions_controller";

    private bool isShown;

    private Action<Pole, ControllerPollingInfo> callback;

    private ControllerType controller;

    private bool closeOnUp;

    private bool keyFromMouse;

    private ControllerPollingInfo pollInfo;

    public Text instructionsField;

    private void Awake()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CONTROLLER_CONNECTED, Hide);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.CONTROLLER_DISCONNECTED, Hide);
    }

    private void OnDestroy()
    {
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.CONTROLLER_CONNECTED, Hide);
        PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.CONTROLLER_DISCONNECTED, Hide);
    }

    public void Show(Action<Pole, ControllerPollingInfo> callback, ControllerType controller, string actionName)
    {
        //IL_0000: Unknown result type (might be due to invalid IL or missing references)
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0002: Unknown result type (might be due to invalid IL or missing references)
        //IL_0014: Expected I4, but got Unknown
        //IL_007f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0080: Unknown result type (might be due to invalid IL or missing references)
        switch ((int)controller)
        {
            case 0:
                instructionsField.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_remap_action_instructions_keyboard", actionName));
                break;
            case 2:
                instructionsField.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("menu_remap_action_instructions_controller", actionName));
                break;
        }
        isShown = true;
        closeOnUp = false;
        this.controller = controller;
        this.callback = callback;
        base.gameObject.SetActive(value: true);
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.POP_UP);
        base.gameObject.SetSelected(force: true);
    }

    public void Hide()
    {
        if (isShown)
        {
            isShown = false;
            base.gameObject.SetActive(value: false);
            PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.POP_UP);
            if (EventSystem.get_current().get_currentSelectedGameObject() == base.gameObject)
            {
                EventSystem.get_current().SetSelectedGameObject((GameObject)null);
            }
        }
    }

    private void Update()
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_0007: Unknown result type (might be due to invalid IL or missing references)
        //IL_0019: Expected I4, but got Unknown
        //IL_0085: Unknown result type (might be due to invalid IL or missing references)
        //IL_00e7: Unknown result type (might be due to invalid IL or missing references)
        ControllerType val = controller;
        switch ((int)val)
        {
            case 1:
                break;
            case 0:
                if ((closeOnUp && !keyFromMouse && ReInput.get_controllers().get_Keyboard().GetKeyUp(((ControllerPollingInfo)(ref pollInfo)).get_keyboardKey())) || (keyFromMouse && ((ControllerWithMap)ReInput.get_controllers().get_Mouse()).GetButtonUpById(((ControllerPollingInfo)(ref pollInfo)).get_elementIdentifierId())))
                {
                    callback((Pole)0, pollInfo);
                    Hide();
                }
                else
                {
                    PollKeyboardForAssignment();
                }
                break;
            case 2:
                if (closeOnUp && ((ControllerWithMap)PandoraSingleton<PandoraInput>.Instance.player.controllers.get_Joysticks()[0]).GetButtonUpById(((ControllerPollingInfo)(ref pollInfo)).get_elementIdentifierId()))
                {
                    callback((Pole)0, pollInfo);
                    Hide();
                }
                else
                {
                    PollJoystickForAssignment();
                }
                break;
        }
    }

    private void PollKeyboardForAssignment()
    {
        //IL_000a: Unknown result type (might be due to invalid IL or missing references)
        //IL_000f: Unknown result type (might be due to invalid IL or missing references)
        //IL_007a: Unknown result type (might be due to invalid IL or missing references)
        //IL_007b: Unknown result type (might be due to invalid IL or missing references)
        //IL_009d: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a2: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c6: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c7: Unknown result type (might be due to invalid IL or missing references)
        ControllerPollingInfo val = ReInput.get_controllers().get_Keyboard().PollForFirstKey();
        if (((ControllerPollingInfo)(ref val)).get_success())
        {
            KeyCode keyboardKey = ((ControllerPollingInfo)(ref val)).get_keyboardKey();
            if (keyboardKey != KeyCode.LeftWindows && keyboardKey != KeyCode.RightWindows && keyboardKey != KeyCode.Menu && keyboardKey != KeyCode.LeftCommand && keyboardKey != KeyCode.RightCommand && keyboardKey != KeyCode.Escape && InputImageTable.butTable.ContainsKey(((ControllerPollingInfo)(ref val)).get_elementIdentifierName()))
            {
                pollInfo = val;
                closeOnUp = true;
                keyFromMouse = false;
            }
        }
        else
        {
            ControllerPollingInfo val2 = ((ControllerWithMap)ReInput.get_controllers().get_Mouse()).PollForFirstButtonDown();
            if (((ControllerPollingInfo)(ref val2)).get_success() && InputImageTable.butTable.ContainsKey(((ControllerPollingInfo)(ref val2)).get_elementIdentifierName()))
            {
                pollInfo = val2;
                closeOnUp = true;
                keyFromMouse = true;
            }
        }
    }

    private void PollJoystickForAssignment()
    {
        //IL_0015: Unknown result type (might be due to invalid IL or missing references)
        //IL_001b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0020: Unknown result type (might be due to invalid IL or missing references)
        //IL_004f: Unknown result type (might be due to invalid IL or missing references)
        //IL_005a: Unknown result type (might be due to invalid IL or missing references)
        //IL_005b: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d5: Unknown result type (might be due to invalid IL or missing references)
        ControllerPollingInfo arg = PandoraSingleton<PandoraInput>.Instance.player.controllers.polling.PollControllerForFirstElementDown(controller, 0);
        if (!((ControllerPollingInfo)(ref arg)).get_success() || !InputImageTable.butTable.ContainsKey("joy_" + ((ControllerPollingInfo)(ref arg)).get_elementIdentifierName()))
        {
            return;
        }
        if ((int)((ControllerPollingInfo)(ref arg)).get_elementType() != 0)
        {
            pollInfo = arg;
            closeOnUp = true;
            return;
        }
        string text = ((ControllerPollingInfo)(ref arg)).get_elementIdentifierName().ToLowerInvariant().Replace(" ", "_");
        if (!text.Equals("left_stick_x") && !text.Equals("left_stick_y") && !text.Equals("right_stick_x") && !text.Equals("right_stick_y"))
        {
            callback(((ControllerPollingInfo)(ref arg)).get_axisPole(), arg);
            Hide();
        }
    }
}
