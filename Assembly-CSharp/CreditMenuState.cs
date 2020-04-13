using DG.Tweening;
using Prometheus;
using UnityEngine;

public class CreditMenuState : UIStateMonoBehaviour<MainMenuController>
{
    public ButtonGroup butExit;

    public Sprite icnBack;

    public Transform creditText;

    private RectTransform canvasRect;

    public CanvasGroup thanksForPlaying;

    private Translate translatingScript;

    public Vector3 oPos;

    private bool firstTime = true;

    public GameObject camPos;

    private Canvas canvas;

    private float canvasTop;

    private Vector3[] rectCorners = new Vector3[4];

    public override int StateId => 4;

    public override void Awake()
    {
        base.Awake();
        canvas = GetComponentInParent<Canvas>();
        canvasRect = (RectTransform)canvas.transform;
    }

    public override void StateEnter()
    {
        butExit.SetAction("cancel", "menu_back", 0, negative: false, icnBack);
        butExit.OnAction(OnInputCancel, mouseOnly: false);
        thanksForPlaying.alpha = 0f;
        Show(visible: true);
        base.StateMachine.camManager.dummyCam.transform.position = camPos.transform.position;
        base.StateMachine.camManager.dummyCam.transform.rotation = camPos.transform.rotation;
        base.StateMachine.camManager.Transition();
        if (firstTime)
        {
            firstTime = false;
            translatingScript = creditText.gameObject.GetComponentsInChildren<Translate>(includeInactive: true)[0];
            translatingScript.smooth = false;
            oPos = creditText.localPosition;
        }
        else
        {
            translatingScript.enabled = true;
            creditText.localPosition = oPos;
        }
        canvasRect.GetWorldCorners(rectCorners);
        canvasTop = rectCorners[1].y;
    }

    public override void OnInputCancel()
    {
        base.StateMachine.GoToPrev();
    }

    public override void StateExit()
    {
        butExit.gameObject.SetActive(value: false);
        Show(visible: false);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        ((RectTransform)creditText).GetWorldCorners(rectCorners);
        float y = rectCorners[0].y;
        if (translatingScript.enabled && y > canvasTop)
        {
            translatingScript.enabled = false;
            ShortcutExtensions.DOFade(thanksForPlaying, 1f, 1f);
        }
    }
}
