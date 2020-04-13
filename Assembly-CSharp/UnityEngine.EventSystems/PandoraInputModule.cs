namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event/Pandora Input Module")]
    public class PandoraInputModule : PointerInputModule
    {
        private const InputButton DEFAULT_ACTION_INPUT = 0;

        [SerializeField]
        private string m_HorizontalAxis = "h";

        [SerializeField]
        private string m_VerticalAxis = "v";

        [SerializeField]
        private string m_SubmitButton = "action";

        [SerializeField]
        private string m_CancelButton = "cancel";

        [SerializeField]
        private float m_InputActionsPerSecond = 10f;

        private float m_NextAction;

        private Vector2 m_LastMousePosition;

        private Vector2 m_MousePosition;

        [SerializeField]
        private bool m_AllowActivationOnMobileDevice;

        private static InputButton actionInput;

        public static InputButton ActionInput
        {
            get
            {
                //IL_0000: Unknown result type (might be due to invalid IL or missing references)
                return actionInput;
            }
            set
            {
                //IL_0000: Unknown result type (might be due to invalid IL or missing references)
                //IL_0001: Unknown result type (might be due to invalid IL or missing references)
                actionInput = value;
            }
        }

        public float InputActionsPerSecond
        {
            get
            {
                return m_InputActionsPerSecond;
            }
            set
            {
                m_InputActionsPerSecond = value;
            }
        }

        public string horizontalAxis
        {
            get
            {
                return m_HorizontalAxis;
            }
            set
            {
                m_HorizontalAxis = value;
            }
        }

        public string verticalAxis
        {
            get
            {
                return m_VerticalAxis;
            }
            set
            {
                m_VerticalAxis = value;
            }
        }

        public string submitButton
        {
            get
            {
                return m_SubmitButton;
            }
            set
            {
                m_SubmitButton = value;
            }
        }

        public string cancelButton
        {
            get
            {
                return m_CancelButton;
            }
            set
            {
                m_CancelButton = value;
            }
        }

        public PandoraInputModule()
            : this()
        {
        }

        public override void UpdateModule()
        {
            m_LastMousePosition = m_MousePosition;
            m_MousePosition = PandoraSingleton<PandoraInput>.Instance.GetMousePosition();
        }

        public override bool IsModuleSupported()
        {
            return true;
        }

        public override bool ShouldActivateModule()
        {
            if (!((BaseInputModule)this).ShouldActivateModule() || !PandoraSingleton<PandoraInput>.Instance.IsActive)
            {
                return false;
            }
            return PandoraSingleton<PandoraInput>.Instance.GetKeyUp(m_SubmitButton, -1) | PandoraSingleton<PandoraInput>.Instance.GetKeyUp(m_CancelButton, -1) | ((double)(m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0) | Input.GetMouseButtonDown(0);
        }

        public override void ActivateModule()
        {
            if (PandoraSingleton<PandoraInput>.Instance.IsActive)
            {
                ((BaseInputModule)this).ActivateModule();
                m_MousePosition = Input.mousePosition;
                m_LastMousePosition = Input.mousePosition;
                GameObject gameObject = ((BaseInputModule)this).get_eventSystem().get_currentSelectedGameObject();
                if (gameObject == null)
                {
                    gameObject = ((BaseInputModule)this).get_eventSystem().get_lastSelectedGameObject();
                }
                if (gameObject == null)
                {
                    gameObject = ((BaseInputModule)this).get_eventSystem().get_firstSelectedGameObject();
                }
                ((BaseInputModule)this).get_eventSystem().SetSelectedGameObject((GameObject)null, ((BaseInputModule)this).GetBaseEventData());
                ((BaseInputModule)this).get_eventSystem().SetSelectedGameObject(gameObject, ((BaseInputModule)this).GetBaseEventData());
            }
        }

        public override void DeactivateModule()
        {
            ((BaseInputModule)this).DeactivateModule();
            ((PointerInputModule)this).ClearSelection();
        }

        public override void Process()
        {
            if (!PandoraSingleton<PandoraInput>.Instance.IsActive || !base.enabled)
            {
                return;
            }
            bool flag = SendUpdateEventToSelectedObject();
            if (((BaseInputModule)this).get_eventSystem().get_sendNavigationEvents())
            {
                if (!flag)
                {
                    flag |= SendMoveEventToSelectedObject();
                }
                if (!flag)
                {
                    SendSubmitEventToSelectedObject();
                }
            }
            ProcessMouseEvent();
        }

        public bool IsMouseMoving()
        {
            return m_LastMousePosition != m_MousePosition;
        }

        private bool SendSubmitEventToSelectedObject()
        {
            if (((BaseInputModule)this).get_eventSystem().get_currentSelectedGameObject() == null)
            {
                return false;
            }
            BaseEventData baseEventData = ((BaseInputModule)this).GetBaseEventData();
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp(m_SubmitButton, -1))
            {
                ExecuteEvents.Execute<ISubmitHandler>(((BaseInputModule)this).get_eventSystem().get_currentSelectedGameObject(), baseEventData, ExecuteEvents.get_submitHandler());
            }
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp(m_CancelButton, -1))
            {
                ExecuteEvents.Execute<ICancelHandler>(((BaseInputModule)this).get_eventSystem().get_currentSelectedGameObject(), baseEventData, ExecuteEvents.get_cancelHandler());
            }
            return ((AbstractEventData)baseEventData).get_used();
        }

        private bool AllowMoveEventProcessing(float time)
        {
            return PandoraSingleton<PandoraInput>.Instance.GetKeyDown(m_HorizontalAxis, -1) || PandoraSingleton<PandoraInput>.Instance.GetKeyDown(m_VerticalAxis, -1) || PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown(m_HorizontalAxis, -1) || PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown(m_VerticalAxis, -1);
        }

        private Vector2 GetRawMoveVector()
        {
            Vector2 zero = Vector2.zero;
            zero.x = PandoraSingleton<PandoraInput>.Instance.GetAxisRaw(m_HorizontalAxis, -1);
            zero.y = PandoraSingleton<PandoraInput>.Instance.GetAxisRaw(m_VerticalAxis, -1);
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown(m_HorizontalAxis, -1))
            {
                zero.x = 1f;
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown(m_HorizontalAxis, -1))
            {
                zero.x = -1f;
            }
            if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown(m_VerticalAxis, -1))
            {
                zero.y = 1f;
            }
            else if (PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown(m_VerticalAxis, -1))
            {
                zero.y = -1f;
            }
            return zero;
        }

        private bool SendMoveEventToSelectedObject()
        {
            float unscaledTime = Time.unscaledTime;
            if (!AllowMoveEventProcessing(unscaledTime))
            {
                return false;
            }
            Vector2 rawMoveVector = GetRawMoveVector();
            AxisEventData axisEventData = ((BaseInputModule)this).GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.9f);
            Vector2 moveVector = axisEventData.get_moveVector();
            if (Mathf.Approximately(moveVector.x, 0f))
            {
                Vector2 moveVector2 = axisEventData.get_moveVector();
                if (Mathf.Approximately(moveVector2.y, 0f))
                {
                    goto IL_0087;
                }
            }
            ExecuteEvents.Execute<IMoveHandler>(((BaseInputModule)this).get_eventSystem().get_currentSelectedGameObject(), (BaseEventData)(object)axisEventData, ExecuteEvents.get_moveHandler());
            goto IL_0087;
        IL_0087:
            return ((AbstractEventData)axisEventData).get_used();
        }

        private void ProcessMouseEvent()
        {
            //IL_00d1: Unknown result type (might be due to invalid IL or missing references)
            //IL_00d6: Unknown result type (might be due to invalid IL or missing references)
            MouseState mousePointerEventData = ((PointerInputModule)this).GetMousePointerEventData();
            bool pressed = mousePointerEventData.AnyPressesThisFrame();
            bool released = mousePointerEventData.AnyReleasesThisFrame();
            MouseButtonEventData eventData = mousePointerEventData.GetButtonState((InputButton)0).get_eventData();
            if (UseMouse(pressed, released, eventData.buttonData))
            {
                ProcessMousePress(eventData);
                ((PointerInputModule)this).ProcessMove(eventData.buttonData);
                ((PointerInputModule)this).ProcessDrag(eventData.buttonData);
                ProcessMousePress(mousePointerEventData.GetButtonState((InputButton)1).get_eventData());
                ((PointerInputModule)this).ProcessDrag(mousePointerEventData.GetButtonState((InputButton)1).get_eventData().buttonData);
                ProcessMousePress(mousePointerEventData.GetButtonState((InputButton)2).get_eventData());
                ((PointerInputModule)this).ProcessDrag(mousePointerEventData.GetButtonState((InputButton)2).get_eventData().buttonData);
                if (!Mathf.Approximately(eventData.buttonData.get_scrollDelta().sqrMagnitude, 0f))
                {
                    RaycastResult pointerCurrentRaycast = eventData.buttonData.get_pointerCurrentRaycast();
                    ExecuteEvents.ExecuteHierarchy<IScrollHandler>(ExecuteEvents.GetEventHandler<IScrollHandler>(((RaycastResult)(ref pointerCurrentRaycast)).get_gameObject()), (BaseEventData)(object)eventData.buttonData, ExecuteEvents.get_scrollHandler());
                }
            }
        }

        private bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
        {
            return pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling();
        }

        private bool SendUpdateEventToSelectedObject()
        {
            if (((BaseInputModule)this).get_eventSystem().get_currentSelectedGameObject() == null)
            {
                return false;
            }
            BaseEventData baseEventData = ((BaseInputModule)this).GetBaseEventData();
            ExecuteEvents.Execute<IUpdateSelectedHandler>(((BaseInputModule)this).get_eventSystem().get_currentSelectedGameObject(), baseEventData, ExecuteEvents.get_updateSelectedHandler());
            return ((AbstractEventData)baseEventData).get_used();
        }

        private void ProcessMousePress(MouseButtonEventData data)
        {
            //IL_0008: Unknown result type (might be due to invalid IL or missing references)
            //IL_000d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0024: Unknown result type (might be due to invalid IL or missing references)
            //IL_002f: Unknown result type (might be due to invalid IL or missing references)
            //IL_003a: Unknown result type (might be due to invalid IL or missing references)
            //IL_003f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0082: Unknown result type (might be due to invalid IL or missing references)
            PointerEventData buttonData = data.buttonData;
            if (buttonData.get_button() == actionInput)
            {
                buttonData.set_button((InputButton)0);
            }
            else if ((int)buttonData.get_button() == 0)
            {
                buttonData.set_button(actionInput);
            }
            RaycastResult pointerCurrentRaycast = buttonData.get_pointerCurrentRaycast();
            GameObject gameObject = ((RaycastResult)(ref pointerCurrentRaycast)).get_gameObject();
            if (data.PressedThisFrame())
            {
                buttonData.set_eligibleForClick(true);
                buttonData.set_delta(Vector2.zero);
                buttonData.set_dragging(false);
                buttonData.set_useDragThreshold(true);
                buttonData.set_pressPosition(buttonData.get_position());
                buttonData.set_pointerPressRaycast(buttonData.get_pointerCurrentRaycast());
                ((PointerInputModule)this).DeselectIfSelectionChanged(gameObject, (BaseEventData)(object)buttonData);
                GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, (BaseEventData)(object)buttonData, ExecuteEvents.get_pointerDownHandler());
                if (gameObject2 == null)
                {
                    gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
                }
                float unscaledTime = Time.unscaledTime;
                if (gameObject2 == buttonData.get_lastPress())
                {
                    if ((double)(unscaledTime - buttonData.get_clickTime()) < 0.300000011920929)
                    {
                        buttonData.set_clickCount(buttonData.get_clickCount() + 1);
                    }
                    else
                    {
                        buttonData.set_clickCount(1);
                    }
                    buttonData.set_clickTime(unscaledTime);
                }
                else
                {
                    buttonData.set_clickCount(1);
                }
                buttonData.set_pointerPress(gameObject2);
                buttonData.set_rawPointerPress(gameObject);
                buttonData.set_clickTime(unscaledTime);
                buttonData.set_pointerDrag(ExecuteEvents.GetEventHandler<IDragHandler>(gameObject));
                if (buttonData.get_pointerDrag() != null)
                {
                    ExecuteEvents.Execute<IInitializePotentialDragHandler>(buttonData.get_pointerDrag(), (BaseEventData)(object)buttonData, ExecuteEvents.get_initializePotentialDrag());
                }
            }
            if (data.ReleasedThisFrame())
            {
                ExecuteEvents.Execute<IPointerUpHandler>(buttonData.get_pointerPress(), (BaseEventData)(object)buttonData, ExecuteEvents.get_pointerUpHandler());
                GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
                if (buttonData.get_pointerPress() == eventHandler && buttonData.get_eligibleForClick())
                {
                    ExecuteEvents.Execute<IPointerClickHandler>(buttonData.get_pointerPress(), (BaseEventData)(object)buttonData, ExecuteEvents.get_pointerClickHandler());
                }
                else if (buttonData.get_pointerDrag() != null)
                {
                    ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, (BaseEventData)(object)buttonData, ExecuteEvents.get_dropHandler());
                }
                buttonData.set_eligibleForClick(false);
                buttonData.set_pointerPress((GameObject)null);
                buttonData.set_rawPointerPress((GameObject)null);
                buttonData.set_dragging(false);
                if (buttonData.get_pointerDrag() != null)
                {
                    ExecuteEvents.Execute<IEndDragHandler>(buttonData.get_pointerDrag(), (BaseEventData)(object)buttonData, ExecuteEvents.get_endDragHandler());
                }
                buttonData.set_pointerDrag((GameObject)null);
                if (gameObject != buttonData.get_pointerEnter())
                {
                    ((BaseInputModule)this).HandlePointerExitAndEnter(buttonData, (GameObject)null);
                    ((BaseInputModule)this).HandlePointerExitAndEnter(buttonData, gameObject);
                }
            }
        }
    }
}
