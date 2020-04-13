using UnityEngine;
using UnityEngine.UI;

public class UIMissionLadderGroup : MonoBehaviour
{
    public Text initiative;

    public Image background;

    public Image backgroundBig;

    public Image unitIcon;

    public RectTransform unitIconTransform;

    public Image unitStarIcon;

    public Image deadOverlay;

    public Image outOfSightOverlay;

    public Sprite backgroundAlly;

    public Sprite backgroundAllyCurrent;

    public Sprite backgroundEnemy;

    public Sprite backgroundEnemyCurrent;

    public Sprite backgroundNeutral;

    public Sprite backgroundNeutralCurrent;

    public Sprite backgroundUnknown;

    public Sprite backgroundUnknownCurrent;

    public Image overviewSelected;

    public Image overviewCurrent;

    public Color allyColor = Color.blue;

    public Color enemyColor = Color.red;

    public Color neutralColor = Color.magenta;

    private void Awake()
    {
        ((Behaviour)(object)overviewSelected).enabled = false;
        ((Behaviour)(object)overviewCurrent).enabled = false;
        unitIconTransform = (((Component)(object)unitIcon).transform as RectTransform);
    }

    public void Set(UnitController unitController, bool isCurrent)
    {
        Unit unit = unitController.unit;
        string text = unit.Initiative.ToConstantString();
        if (initiative.get_text() != text)
        {
            initiative.set_text(text);
        }
        if (isCurrent)
        {
            unitIconTransform.sizeDelta = new Vector2(64f, 64f);
            if (!((Behaviour)(object)backgroundBig).enabled)
            {
                ((Behaviour)(object)backgroundBig).enabled = true;
            }
            if (((Behaviour)(object)background).enabled)
            {
                ((Behaviour)(object)background).enabled = false;
            }
        }
        else
        {
            unitIconTransform.sizeDelta = new Vector2(36f, 36f);
            if (((Behaviour)(object)backgroundBig).enabled)
            {
                ((Behaviour)(object)backgroundBig).enabled = false;
            }
            if (!((Behaviour)(object)background).enabled)
            {
                ((Behaviour)(object)background).enabled = true;
            }
        }
        if (!unitController.ladderVisible)
        {
            if (base.gameObject.activeSelf)
            {
                base.gameObject.SetActive(value: false);
            }
        }
        else if (unitController.HasBeenSpotted || unit.Status == UnitStateId.OUT_OF_ACTION)
        {
            if (unitController.IsPlayed())
            {
                if (backgroundBig.get_overrideSprite() != backgroundAllyCurrent)
                {
                    backgroundBig.set_overrideSprite(backgroundAllyCurrent);
                }
                if (background.get_overrideSprite() != backgroundAlly)
                {
                    background.set_overrideSprite(backgroundAlly);
                }
                if (((Graphic)unitIcon).get_color() != allyColor)
                {
                    ((Graphic)unitIcon).set_color(allyColor);
                }
            }
            else if (unit.IsMonster)
            {
                if (backgroundBig.get_overrideSprite() != backgroundNeutralCurrent)
                {
                    backgroundBig.set_overrideSprite(backgroundNeutralCurrent);
                }
                if (background.get_overrideSprite() != backgroundNeutral)
                {
                    background.set_overrideSprite(backgroundNeutral);
                }
                if (((Graphic)unitIcon).get_color() != neutralColor)
                {
                    ((Graphic)unitIcon).set_color(neutralColor);
                }
            }
            else
            {
                if (backgroundBig.get_overrideSprite() != backgroundEnemyCurrent)
                {
                    backgroundBig.set_overrideSprite(backgroundEnemyCurrent);
                }
                if (background.get_overrideSprite() != backgroundEnemy)
                {
                    background.set_overrideSprite(backgroundEnemy);
                }
                if (((Graphic)unitIcon).get_color() != enemyColor)
                {
                    ((Graphic)unitIcon).set_color(enemyColor);
                }
            }
            Sprite icon = unit.GetIcon();
            if (unitIcon.get_overrideSprite() != icon)
            {
                unitIcon.set_overrideSprite(icon);
            }
            switch (unit.GetUnitTypeId())
            {
                case UnitTypeId.LEADER:
                    {
                        if (!((Behaviour)(object)unitStarIcon).enabled)
                        {
                            ((Behaviour)(object)unitStarIcon).enabled = true;
                        }
                        Sprite sprite = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_leader", cached: true);
                        if (unitStarIcon.get_sprite() != sprite)
                        {
                            unitStarIcon.set_sprite(sprite);
                        }
                        break;
                    }
                case UnitTypeId.HERO_1:
                case UnitTypeId.HERO_2:
                case UnitTypeId.HERO_3:
                    {
                        if (!((Behaviour)(object)unitStarIcon).enabled)
                        {
                            ((Behaviour)(object)unitStarIcon).enabled = true;
                        }
                        Sprite sprite3 = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_heroes", cached: true);
                        if (unitStarIcon.get_sprite() != sprite3)
                        {
                            unitStarIcon.set_sprite(sprite3);
                        }
                        break;
                    }
                case UnitTypeId.IMPRESSIVE:
                    {
                        if (!((Behaviour)(object)unitStarIcon).enabled)
                        {
                            ((Behaviour)(object)unitStarIcon).enabled = true;
                        }
                        Sprite sprite2 = PandoraSingleton<AssetBundleLoader>.Instance.LoadResource<Sprite>("icn_impressive", cached: true);
                        if (unitStarIcon.get_sprite() != sprite2)
                        {
                            unitStarIcon.set_sprite(sprite2);
                        }
                        break;
                    }
                default:
                    if (((Behaviour)(object)unitStarIcon).enabled)
                    {
                        ((Behaviour)(object)unitStarIcon).enabled = false;
                    }
                    break;
            }
            if (((Behaviour)(object)deadOverlay).enabled != (unit.Status == UnitStateId.OUT_OF_ACTION))
            {
                ((Behaviour)(object)deadOverlay).enabled = (unit.Status == UnitStateId.OUT_OF_ACTION);
            }
            if (((Behaviour)(object)outOfSightOverlay).enabled != (!unitController.IsImprintVisible() && unit.Status != UnitStateId.OUT_OF_ACTION))
            {
                ((Behaviour)(object)outOfSightOverlay).enabled = (!unitController.IsImprintVisible() && unit.Status != UnitStateId.OUT_OF_ACTION);
            }
        }
        else
        {
            backgroundBig.set_overrideSprite(backgroundUnknownCurrent);
            background.set_overrideSprite(backgroundUnknown);
            backgroundBig.set_overrideSprite((Sprite)null);
            background.set_overrideSprite((Sprite)null);
            unitIcon.set_overrideSprite((Sprite)null);
            ((Behaviour)(object)unitStarIcon).enabled = false;
            ((Behaviour)(object)deadOverlay).enabled = false;
            ((Behaviour)(object)outOfSightOverlay).enabled = false;
            ((Graphic)unitIcon).set_color(Color.white);
        }
    }

    public void SetCurrent(bool isCurrent, bool force, bool realCurrent)
    {
        if (isCurrent)
        {
            unitIconTransform.sizeDelta = new Vector2(64f, 64f);
            if (!((Behaviour)(object)backgroundBig).enabled)
            {
                ((Behaviour)(object)backgroundBig).enabled = true;
            }
            if ((bool)(Object)(object)background)
            {
                ((Behaviour)(object)background).enabled = false;
            }
            if (((Behaviour)(object)overviewCurrent).enabled != (force && realCurrent))
            {
                ((Behaviour)(object)overviewCurrent).enabled = (force && realCurrent);
            }
            if (((Behaviour)(object)overviewSelected).enabled != force && !realCurrent)
            {
                ((Behaviour)(object)overviewSelected).enabled = (force && !realCurrent);
            }
        }
        else
        {
            unitIconTransform.sizeDelta = new Vector2(36f, 36f);
            if (((Behaviour)(object)backgroundBig).enabled)
            {
                ((Behaviour)(object)backgroundBig).enabled = false;
            }
            if (!((Behaviour)(object)background).enabled)
            {
                ((Behaviour)(object)background).enabled = true;
            }
            if (((Behaviour)(object)overviewSelected).enabled)
            {
                ((Behaviour)(object)overviewSelected).enabled = false;
            }
            if (((Behaviour)(object)overviewCurrent).enabled)
            {
                ((Behaviour)(object)overviewCurrent).enabled = false;
            }
        }
    }
}
