using Rewired;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PandoraInput : PandoraSingleton<PandoraInput>
{
    public enum InputLayer
    {
        NOTHING = -9999,
        NORMAL = 0,
        FLY_BY_CAM = 200,
        TRANSITION = 1000,
        POP_UP = 1,
        CHAT = 2,
        END_GAME = 3,
        LOOTING = 4,
        WHEEL = 5,
        MENU = 6,
        LOG = 7
    }

    public enum States
    {
        MENU = 0,
        MISSION = 1,
        NB_STATE = 2,
        NONE = 2
    }

    public enum InputMode
    {
        NONE,
        KEYBOARD,
        MOUSE,
        JOYSTICK
    }

    public float firstRepeatDelay = 0.4f;

    public float subsequentRepeatRate = 0.4f;

    public float mousePointerInactiveHideDelay = 5f;

    private List<InputLayer> popLayers;

    private List<InputLayer> inputLayers;

    private States currentStateId;

    private bool showCursor = true;

    private float lastMouseInputTime;

    public bool leftHandedMouse;

    public bool leftHandedController;

    private Dictionary<string, float> keyRepeatData;

    public bool initialized;

    public int CurrentInputLayer
    {
        get;
        private set;
    }

    public InputMode lastInputMode
    {
        get;
        private set;
    }

    public bool IsActive
    {
        get;
        private set;
    }

    public Player player
    {
        get;
        private set;
    }

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        lastInputMode = InputMode.NONE;
        popLayers = new List<InputLayer>();
        inputLayers = new List<InputLayer>();
        PushInputLayer(InputLayer.NORMAL);
        IsActive = false;
        showCursor = true;
        Cursor.lockState = CursorLockMode.Confined;
        player = ReInput.get_players().GetPlayer(0);
        player.controllers.AddController((ControllerType)2, 0, true);
        ReInput.add_ControllerConnectedEvent((Action<ControllerStatusChangedEventArgs>)OnControllerConnected);
        ReInput.add_ControllerDisconnectedEvent((Action<ControllerStatusChangedEventArgs>)OnControllerDisconnected);
        keyRepeatData = new Dictionary<string, float>();
        initialized = true;
    }

    public void SetLastInputMode(InputMode mode)
    {
        if (mode != lastInputMode)
        {
            lastInputMode = mode;
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.INPUT_TYPE_CHANGED);
        }
    }

    public void Rumble(float leftVibration, float rightVibration)
    {
        for (int i = 0; i < player.controllers.get_Joysticks().Count; i++)
        {
            if (player.controllers.get_Joysticks()[i].get_supportsVibration())
            {
                player.controllers.get_Joysticks()[i].SetVibration(leftVibration, rightVibration);
            }
        }
    }

    public void StopRumble()
    {
        for (int i = 0; i < player.controllers.get_Joysticks().Count; i++)
        {
            if (player.controllers.get_Joysticks()[i].get_supportsVibration())
            {
                player.controllers.get_Joysticks()[i].StopVibration();
            }
        }
    }

    private void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        //IL_0038: Unknown result type (might be due to invalid IL or missing references)
        //IL_0064: Unknown result type (might be due to invalid IL or missing references)
        PandoraDebug.LogDebug("A controller was connected! Name = " + args.get_name() + " Id = " + args.get_controllerId() + " Type = " + args.get_controllerType(), "INPUT");
        player.controllers.AddController(ReInput.get_controllers().GetController(args.get_controllerType(), args.get_controllerId()), true);
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CONTROLLER_CONNECTED);
    }

    private void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
    {
        //IL_0038: Unknown result type (might be due to invalid IL or missing references)
        PandoraDebug.LogDebug("A controller was disconnected! Name = " + args.get_name() + " Id = " + args.get_controllerId() + " Type = " + args.get_controllerType(), "INPUT");
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.CONTROLLER_DISCONNECTED);
    }

    public void LoadMappingFromXml(ControllerType controller, List<string> mappings)
    {
        //IL_0022: Unknown result type (might be due to invalid IL or missing references)
        //IL_0029: Unknown result type (might be due to invalid IL or missing references)
        //IL_002b: Invalid comparison between Unknown and I4
        //IL_005f: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a0: Unknown result type (might be due to invalid IL or missing references)
        if (mappings == null || mappings.Count <= 0)
        {
            return;
        }
        player.controllers.maps.ClearMaps(controller, true);
        if ((int)controller == 2)
        {
            for (int i = 0; i < player.controllers.get_joystickCount(); i++)
            {
                Joystick val = player.controllers.get_Joysticks()[i];
                player.controllers.maps.AddMapsFromXml(((Controller)val).get_type(), ((Controller)val).id, mappings);
            }
        }
        else
        {
            player.controllers.maps.AddMapsFromXml(controller, 0, mappings);
        }
        foreach (ControllerMap allMap in player.controllers.maps.GetAllMaps())
        {
            allMap.set_enabled(allMap.get_categoryId() == (int)currentStateId);
        }
    }

    public void RestoreDefaultMappings()
    {
        player.controllers.maps.LoadDefaultMaps((ControllerType)0);
        player.controllers.maps.LoadDefaultMaps((ControllerType)2);
        player.controllers.maps.LoadDefaultMaps((ControllerType)1);
        if (leftHandedController)
        {
            leftHandedController = false;
            SetLeftHandedController(leftHanded: true, includeUserAssignables: true);
        }
        if (leftHandedMouse)
        {
            leftHandedMouse = false;
            SetLeftHandedMouse(leftHanded: true, includeUserAssignables: true);
        }
    }

    public ActionElementMap GetInputForAction(string actionName, ControllerType controller, Pole inputPole = 0)
    {
        //IL_0016: Unknown result type (might be due to invalid IL or missing references)
        //IL_0046: Unknown result type (might be due to invalid IL or missing references)
        //IL_004b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0054: Unknown result type (might be due to invalid IL or missing references)
        //IL_0061: Unknown result type (might be due to invalid IL or missing references)
        foreach (ControllerMap item in player.controllers.maps.GetAllMapsInCategory((int)currentStateId, controller))
        {
            ActionElementMap[] elementMapsWithAction = item.GetElementMapsWithAction(actionName);
            if (elementMapsWithAction != null)
            {
                for (int i = 0; i < elementMapsWithAction.Length; i++)
                {
                    if (elementMapsWithAction[i].get_axisContribution() == inputPole || ((int)elementMapsWithAction[i].get_elementType() == 0 && (int)elementMapsWithAction[i].get_axisRange() == 0))
                    {
                        return elementMapsWithAction[i];
                    }
                }
            }
        }
        return null;
    }

    public void ActivateController(ControllerType controllerType)
    {
        //IL_0016: Unknown result type (might be due to invalid IL or missing references)
        if (player != null)
        {
            player.controllers.AddController(controllerType, 0, true);
        }
    }

    public void DeactivateController(ControllerType controllerType)
    {
        //IL_0016: Unknown result type (might be due to invalid IL or missing references)
        if (player != null)
        {
            player.controllers.RemoveController(controllerType, 0);
        }
    }

    public void SetActionInverted(string action, bool inverted = true)
    {
        int actionId = ReInput.get_mapping().GetActionId(action);
        foreach (ControllerMap allMap in player.controllers.maps.GetAllMaps())
        {
            foreach (ActionElementMap item in allMap.ElementMapsWithAction(actionId))
            {
                item.set_invert(inverted);
            }
        }
    }

    public void SetLeftHandedMouse(bool leftHanded = true, bool includeUserAssignables = false)
    {
        //IL_0196: Unknown result type (might be due to invalid IL or missing references)
        //IL_01b6: Unknown result type (might be due to invalid IL or missing references)
        //IL_01ea: Unknown result type (might be due to invalid IL or missing references)
        //IL_020a: Unknown result type (might be due to invalid IL or missing references)
        if (leftHanded == leftHandedMouse)
        {
            return;
        }
        leftHandedMouse = leftHanded;
        if (leftHandedMouse)
        {
            PandoraInputModule.ActionInput = (InputButton)1;
        }
        else
        {
            PandoraInputModule.ActionInput = (InputButton)0;
        }
        int num = -1;
        int num2 = -1;
        for (int i = 0; i < ((ControllerWithMap)ReInput.get_controllers().get_Mouse()).get_ButtonElementIdentifiers().Count; i++)
        {
            if (((ControllerWithMap)ReInput.get_controllers().get_Mouse()).get_ButtonElementIdentifiers()[i].get_name().Equals("Left Mouse Button"))
            {
                num = ((ControllerWithMap)ReInput.get_controllers().get_Mouse()).get_ButtonElementIdentifiers()[i].get_id();
                break;
            }
        }
        for (int j = 0; j < ((ControllerWithMap)ReInput.get_controllers().get_Mouse()).get_ButtonElementIdentifiers().Count; j++)
        {
            if (((ControllerWithMap)ReInput.get_controllers().get_Mouse()).get_ButtonElementIdentifiers()[j].get_name().Equals("Right Mouse Button"))
            {
                num2 = ((ControllerWithMap)ReInput.get_controllers().get_Mouse()).get_ButtonElementIdentifiers()[j].get_id();
                break;
            }
        }
        foreach (ControllerMap allMap in player.controllers.maps.GetAllMaps((ControllerType)1))
        {
            ActionElementMap[] buttonMaps = allMap.GetButtonMaps();
            ElementAssignment val = default(ElementAssignment);
            ElementAssignment val2 = default(ElementAssignment);
            for (int k = 0; k < buttonMaps.Length; k++)
            {
                if (includeUserAssignables || !ReInput.get_mapping().GetAction(buttonMaps[k].get_actionId()).get_userAssignable())
                {
                    if (buttonMaps[k].get_elementIdentifierId() == num)
                    {
                        ((ElementAssignment)(ref val))._002Ector((ControllerType)1, (ControllerElementType)1, num2, (AxisRange)0, KeyCode.None, (ModifierKeyFlags)0, buttonMaps[k].get_actionId(), buttonMaps[k].get_axisContribution(), buttonMaps[k].get_invert(), buttonMaps[k].get_id());
                        allMap.ReplaceElementMap(val);
                    }
                    else if (buttonMaps[k].get_elementIdentifierId() == num2)
                    {
                        ((ElementAssignment)(ref val2))._002Ector((ControllerType)1, (ControllerElementType)1, num, (AxisRange)0, KeyCode.None, (ModifierKeyFlags)0, buttonMaps[k].get_actionId(), buttonMaps[k].get_axisContribution(), buttonMaps[k].get_invert(), buttonMaps[k].get_id());
                        allMap.ReplaceElementMap(val2);
                    }
                }
            }
        }
    }

    public void SetLeftHandedController(bool leftHanded = true, bool includeUserAssignables = false)
    {
        //IL_0243: Unknown result type (might be due to invalid IL or missing references)
        //IL_0263: Unknown result type (might be due to invalid IL or missing references)
        //IL_0297: Unknown result type (might be due to invalid IL or missing references)
        //IL_02b7: Unknown result type (might be due to invalid IL or missing references)
        //IL_02eb: Unknown result type (might be due to invalid IL or missing references)
        //IL_030b: Unknown result type (might be due to invalid IL or missing references)
        //IL_033f: Unknown result type (might be due to invalid IL or missing references)
        //IL_035f: Unknown result type (might be due to invalid IL or missing references)
        if (player.controllers.get_Joysticks().Count <= 0 || leftHanded == leftHandedController)
        {
            return;
        }
        leftHandedController = leftHanded;
        int num = -1;
        int num2 = -1;
        int num3 = -1;
        int num4 = -1;
        for (int i = 0; i < ((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers().Count; i++)
        {
            if (((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers()[i].get_name().Equals("Left Stick X"))
            {
                num = ((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers()[i].get_id();
            }
            else if (((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers()[i].get_name().Equals("Left Stick Y"))
            {
                num2 = ((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers()[i].get_id();
            }
            else if (((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers()[i].get_name().Equals("Right Stick X"))
            {
                num3 = ((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers()[i].get_id();
            }
            else if (((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers()[i].get_name().Equals("Right Stick Y"))
            {
                num4 = ((ControllerWithAxes)ReInput.get_controllers().get_Joysticks()[0]).get_AxisElementIdentifiers()[i].get_id();
            }
        }
        foreach (ControllerMap allMap in player.controllers.maps.GetAllMaps((ControllerType)2))
        {
            ActionElementMap[] elementMaps = allMap.GetElementMaps();
            ElementAssignment val = default(ElementAssignment);
            ElementAssignment val2 = default(ElementAssignment);
            ElementAssignment val3 = default(ElementAssignment);
            ElementAssignment val4 = default(ElementAssignment);
            for (int j = 0; j < elementMaps.Length; j++)
            {
                if (includeUserAssignables || !ReInput.get_mapping().GetAction(elementMaps[j].get_actionId()).get_userAssignable())
                {
                    if (elementMaps[j].get_elementIdentifierId() == num)
                    {
                        ((ElementAssignment)(ref val))._002Ector((ControllerType)2, (ControllerElementType)0, num3, (AxisRange)0, KeyCode.None, (ModifierKeyFlags)0, elementMaps[j].get_actionId(), elementMaps[j].get_axisContribution(), elementMaps[j].get_invert(), elementMaps[j].get_id());
                        allMap.ReplaceElementMap(val);
                    }
                    else if (elementMaps[j].get_elementIdentifierId() == num2)
                    {
                        ((ElementAssignment)(ref val2))._002Ector((ControllerType)2, (ControllerElementType)0, num4, (AxisRange)0, KeyCode.None, (ModifierKeyFlags)0, elementMaps[j].get_actionId(), elementMaps[j].get_axisContribution(), elementMaps[j].get_invert(), elementMaps[j].get_id());
                        allMap.ReplaceElementMap(val2);
                    }
                    else if (elementMaps[j].get_elementIdentifierId() == num3)
                    {
                        ((ElementAssignment)(ref val3))._002Ector((ControllerType)2, (ControllerElementType)0, num, (AxisRange)0, KeyCode.None, (ModifierKeyFlags)0, elementMaps[j].get_actionId(), elementMaps[j].get_axisContribution(), elementMaps[j].get_invert(), elementMaps[j].get_id());
                        allMap.ReplaceElementMap(val3);
                    }
                    else if (elementMaps[j].get_elementIdentifierId() == num4)
                    {
                        ((ElementAssignment)(ref val4))._002Ector((ControllerType)2, (ControllerElementType)0, num2, (AxisRange)0, KeyCode.None, (ModifierKeyFlags)0, elementMaps[j].get_actionId(), elementMaps[j].get_axisContribution(), elementMaps[j].get_invert(), elementMaps[j].get_id());
                        allMap.ReplaceElementMap(val4);
                    }
                }
            }
        }
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        IList<InputBehavior> inputBehaviors = player.controllers.maps.get_InputBehaviors();
        inputBehaviors[1].set_mouseXYAxisSensitivity(sensitivity);
    }

    public void SetJoystickSensitivity(float sensitivity)
    {
        IList<InputBehavior> inputBehaviors = player.controllers.maps.get_InputBehaviors();
        inputBehaviors[1].set_joystickAxisSensitivity(sensitivity * 5f);
    }

    public void MapKeyboardKey(string inputCategory, KeyCode key, int actionId, bool isPositive = true)
    {
        player.controllers.maps.GetFirstMapInCategory((ControllerType)0, 0, inputCategory).CreateElementMap(actionId, (Pole)((!isPositive) ? 1 : 0), key, (ModifierKeyFlags)0);
    }

    public ActionElementMap GetFirstConflictingActionMap(int actionId, string inputCategory, ControllerType controller, int keyIdentifier)
    {
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_0045: Unknown result type (might be due to invalid IL or missing references)
        ControllerMap firstMapInCategory = player.controllers.maps.GetFirstMapInCategory(controller, 0, inputCategory);
        ActionElementMap[] elementMaps = firstMapInCategory.GetElementMaps();
        foreach (ActionElementMap val in elementMaps)
        {
            if (ReInput.get_mapping().GetAction(val.get_actionId()).get_userAssignable())
            {
                int num = ((int)controller != 0) ? val.get_elementIdentifierId() : ((int)val.get_keyCode());
                if (num == keyIdentifier)
                {
                    return val;
                }
            }
        }
        return null;
    }

    public void PushInputLayer(InputLayer layer)
    {
        inputLayers.Add(layer);
        CurrentInputLayer = (int)layer;
        PandoraDebug.LogInfo("PUSH InputLayers.Count = " + inputLayers.Count + " current = " + (InputLayer)CurrentInputLayer, "PandoraInput");
    }

    public void PopInputLayer(InputLayer layer)
    {
        PandoraDebug.LogInfo("Input Layer to be POPPED! = " + inputLayers.Count + " current = " + (InputLayer)CurrentInputLayer + " To Be Popped " + layer, "PandoraInput");
        popLayers.Add(layer);
    }

    public void ClearInputLayer()
    {
        popLayers.Clear();
        inputLayers.Clear();
        PushInputLayer(InputLayer.NORMAL);
    }

    public States GetCurrentState()
    {
        return currentStateId;
    }

    public void SetCurrentState(States state, bool showMouse)
    {
        showCursor = showMouse;
        if (state < States.NB_STATE)
        {
            currentStateId = state;
            IsActive = true;
        }
        else
        {
            IsActive = false;
        }
        if (showCursor)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        Cursor.visible = showCursor;
        foreach (ControllerMap allMap in player.controllers.maps.GetAllMaps())
        {
            allMap.set_enabled(allMap.get_categoryId() == (int)state);
        }
    }

    private void OnApplicationFocus(bool focusStatus)
    {
        if (focusStatus)
        {
            if (showCursor)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            Cursor.visible = showCursor;
        }
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public float GetAxis(string name, int layer = 0)
    {
        if (IsActive && (CurrentInputLayer == layer || layer == -1))
        {
            UpdateLastInputType();
            return player.GetAxis(name);
        }
        return 0f;
    }

    public float GetAxisRaw(string name, int layer = 0)
    {
        if (IsActive && (CurrentInputLayer == layer || layer == -1))
        {
            UpdateLastInputType();
            return player.GetAxisRaw(name);
        }
        return 0f;
    }

    public bool GetKeyDown(string name, int layer = 0)
    {
        if (IsActive && (CurrentInputLayer == layer || layer == -1))
        {
            UpdateLastInputType();
            bool flag = player.GetButtonDown(name);
            if (flag)
            {
                keyRepeatData[name] = firstRepeatDelay;
            }
            else if (keyRepeatData.ContainsKey(name))
            {
                float buttonTimePressed = player.GetButtonTimePressed(name);
                if (buttonTimePressed > keyRepeatData[name])
                {
                    flag = true;
                    keyRepeatData[name] = buttonTimePressed + subsequentRepeatRate - Mathf.Repeat(buttonTimePressed - keyRepeatData[name], subsequentRepeatRate);
                }
            }
            return flag;
        }
        return false;
    }

    public bool GetNegKeyDown(string name, int layer = 0)
    {
        if (IsActive && (CurrentInputLayer == layer || layer == -1))
        {
            UpdateLastInputType();
            bool flag = player.GetNegativeButtonDown(name);
            string key = "neg" + name;
            if (flag)
            {
                keyRepeatData[key] = firstRepeatDelay;
            }
            else if (keyRepeatData.ContainsKey(key))
            {
                float negativeButtonTimePressed = player.GetNegativeButtonTimePressed(name);
                if (negativeButtonTimePressed > keyRepeatData[key])
                {
                    flag = true;
                    keyRepeatData[key] = negativeButtonTimePressed + subsequentRepeatRate - Mathf.Repeat(negativeButtonTimePressed - keyRepeatData[key], subsequentRepeatRate);
                }
            }
            return flag;
        }
        return false;
    }

    public bool GetKeyUp(string name, int layer = 0)
    {
        if (IsActive && (CurrentInputLayer == layer || layer == -1))
        {
            UpdateLastInputType();
            return player.GetButtonUp(name);
        }
        return false;
    }

    public bool GetNegKeyUp(string name, int layer = 0)
    {
        if (IsActive && (CurrentInputLayer == layer || layer == -1))
        {
            UpdateLastInputType();
            return player.GetNegativeButtonUp(name);
        }
        return false;
    }

    public bool GetKey(string name, int layer = 0)
    {
        if (IsActive && (CurrentInputLayer == layer || layer == -1))
        {
            UpdateLastInputType();
            return player.GetButton(name);
        }
        return false;
    }

    public bool GetNegKey(string name, int layer = 0)
    {
        if (IsActive && (CurrentInputLayer == layer || layer == -1))
        {
            UpdateLastInputType();
            return player.GetNegativeButton(name);
        }
        return false;
    }

    private void UpdateLastInputType()
    {
        //IL_0025: Unknown result type (might be due to invalid IL or missing references)
        //IL_002a: Unknown result type (might be due to invalid IL or missing references)
        //IL_002b: Unknown result type (might be due to invalid IL or missing references)
        //IL_003d: Expected I4, but got Unknown
        Controller val = null;
        if (player != null)
        {
            val = player.controllers.GetLastActiveController();
        }
        if (val != null)
        {
            ControllerType type = val.get_type();
            switch ((int)type)
            {
                case 0:
                    SetLastInputMode(InputMode.KEYBOARD);
                    break;
                case 1:
                    SetLastInputMode(InputMode.MOUSE);
                    break;
                case 2:
                    SetLastInputMode(InputMode.JOYSTICK);
                    break;
            }
        }
    }

    public string GetInputString()
    {
        return Input.inputString;
    }

    public Vector3 GetMousePosition()
    {
        return Input.mousePosition;
    }

    public void LockPlayerPollKey(string actionName)
    {
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            return;
        }
        if (((Controller)ReInput.get_controllers().get_Mouse()).get_isConnected())
        {
            bool flag = true;
            for (int i = 0; i < ((ControllerWithAxes)ReInput.get_controllers().get_Mouse()).get_axisCount(); i++)
            {
                if (((ControllerWithAxes)ReInput.get_controllers().get_Mouse()).GetAxisTimeInactive(i) < mousePointerInactiveHideDelay)
                {
                    flag = false;
                }
            }
            if (flag)
            {
                if (Cursor.visible)
                {
                    Cursor.visible = false;
                }
            }
            else if (showCursor)
            {
                Cursor.visible = true;
            }
        }
        if (popLayers.Count <= 0)
        {
            return;
        }
        for (int j = 0; j < popLayers.Count; j++)
        {
            for (int num = inputLayers.Count - 1; num >= 0; num--)
            {
                if (inputLayers[num] == popLayers[j])
                {
                    inputLayers.RemoveAt(num);
                    break;
                }
            }
            CurrentInputLayer = (int)inputLayers[inputLayers.Count - 1];
        }
        popLayers.Clear();
    }
}
