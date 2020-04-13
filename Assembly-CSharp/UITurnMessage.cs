using DG.Tweening;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class UITurnMessage : CanvasGroupDisabler
{
    public Text text;

    public Image icon;

    public Image overlay;

    public Sprite overlayEnemy;

    public Sprite overlayPlayer;

    public Sprite overlayNeutral;

    private Tweener currentTween;

    private Sprite noneIcon;

    private UIMissionLadderGroup ladderTemplate;

    private long currentIndex;

    private long index;

    private void Awake()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_ROUND_START, OnNewRound);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_ROUT_TEST, OnRoutTest);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_PLAYER_CHANGE, OnPlayerChange);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_REINFORCEMENTS, OnReinforcement);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_DEAD_UNIT_FLEE, OnDeadUnitLeave);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_UNIT_SPAWN, OnUnitSpawn);
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.DEPLOY_UNIT, OnDeployUnit);
        noneIcon = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("unit/none", cached: true);
        currentIndex = 0L;
        index = 0L;
    }

    private void Start()
    {
        ladderTemplate = PandoraSingleton<UIMissionManager>.Instance.ladder.template.GetComponent<UIMissionLadderGroup>();
    }

    private void OnRoutTest()
    {
        WarbandController warbandController = (WarbandController)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        bool flag = (bool)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
        string msg = PandoraSingleton<LocalizationManager>.Instance.GetStringById((!warbandController.IsPlayed()) ? "combat_enemy_rout" : "combat_self_rout") + "\n \n" + PandoraSingleton<LocalizationManager>.Instance.GetStringById((!flag) ? "com_failure" : "com_success");
        StartCoroutine(Show(index++, msg, (!warbandController.IsPlayed()) ? overlayEnemy : overlayPlayer, Warband.GetIcon(warbandController.WarData.Id)));
    }

    private void OnPlayerChange()
    {
        Color white = Color.white;
        UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
        Sprite iconSprite;
        string stringById;
        Sprite overlaySprite;
        if (currentUnit.IsPlayed())
        {
            iconSprite = currentUnit.unit.GetIcon();
            stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("combat_unit_turn", currentUnit.unit.Name);
            overlaySprite = overlayPlayer;
            white = ladderTemplate.allyColor;
        }
        else
        {
            if (currentUnit.unit.IsMonster)
            {
                white = ladderTemplate.neutralColor;
                overlaySprite = overlayNeutral;
            }
            else
            {
                white = ladderTemplate.enemyColor;
                overlaySprite = overlayEnemy;
            }
            if (currentUnit.IsImprintVisible())
            {
                stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("combat_unit_turn", currentUnit.unit.Name);
                iconSprite = currentUnit.unit.GetIcon();
            }
            else if (currentUnit.HasBeenSpotted)
            {
                stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("combat_enemy_turn");
                iconSprite = currentUnit.unit.GetIcon();
            }
            else
            {
                stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("combat_enemy_turn");
                iconSprite = noneIcon;
                white = Color.white;
            }
        }
        StartCoroutine(Show(index++, stringById, overlaySprite, iconSprite, white));
    }

    private void OnDeployUnit()
    {
        UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
        Sprite iconSprite;
        string stringById;
        Sprite overlaySprite;
        Color iconColor;
        if (currentUnit.IsPlayed())
        {
            iconSprite = currentUnit.unit.GetIcon();
            stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("combat_unit_deploy", currentUnit.unit.Name);
            overlaySprite = overlayPlayer;
            iconColor = ladderTemplate.allyColor;
        }
        else
        {
            iconSprite = noneIcon;
            stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("combat_enemy_deploy");
            overlaySprite = overlayEnemy;
            iconColor = ladderTemplate.enemyColor;
        }
        TweenExtensions.Complete((Tween)(object)currentTween);
        StopAllCoroutines();
        currentIndex = index;
        StartCoroutine(Show(index++, stringById, overlaySprite, iconSprite, iconColor));
    }

    private void OnNewRound()
    {
        PandoraSingleton<Pan>.Instance.Narrate("new_round");
        DisplayNewTurn(PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_round_no", ((int)PandoraSingleton<NoticeManager>.Instance.Parameters[0] + 1).ToConstantString()));
    }

    private void OnReinforcement()
    {
        string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_round_no", ((int)PandoraSingleton<NoticeManager>.Instance.Parameters[0] + 1).ToConstantString());
        stringById = stringById + "\n \n" + PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_reinforcements");
        DisplayNewTurn(stringById);
    }

    private void OnDeadUnitLeave()
    {
        string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_round_no", ((int)PandoraSingleton<NoticeManager>.Instance.Parameters[0] + 1).ToConstantString());
        stringById = stringById + "\n \n" + PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_dead_unit_flee");
        DisplayNewTurn(stringById);
    }

    private void OnUnitSpawn()
    {
        DisplayNewTurn(PandoraSingleton<LocalizationManager>.Instance.GetStringById((string)PandoraSingleton<NoticeManager>.Instance.Parameters[0], (string)PandoraSingleton<NoticeManager>.Instance.Parameters[1]));
    }

    private void DisplayNewTurn(string msg)
    {
        StartCoroutine(Show(index++, msg));
    }

    private IEnumerator Show(long coroutineIndex, string msg, Sprite overlaySprite = null, Sprite iconSprite = null, [Optional] Color iconColor)
    {
        while (coroutineIndex != currentIndex)
        {
            yield return null;
        }
        OnEnable();
        ((Behaviour)(object)overlay).enabled = (overlaySprite != null);
        overlay.set_overrideSprite(overlaySprite);
        ((Behaviour)(object)icon).enabled = (iconSprite != null);
        icon.set_overrideSprite(iconSprite);
        ((Graphic)icon).set_color(iconColor);
        text.set_text(msg);
        currentTween = TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetDelay<Tweener>(ShortcutExtensions.DOFade(base.CanvasGroup, 0f, 1f), 2f), (TweenCallback)(object)new TweenCallback(OnDisable));
    }

    public override void OnDisable()
    {
        bool interactable = base.CanvasGroup.interactable;
        base.OnDisable();
        if (interactable && index != 0L)
        {
            currentIndex++;
        }
    }
}
