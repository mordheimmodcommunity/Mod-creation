using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveView : MonoBehaviour
{
    public Toggle toggleObjective;

    public Text counter;

    public Text objectiveText;

    public Text complete;

    public CanvasGroup completeCanvasGroup;

    public float startPositionX;

    public float endPosition;

    public ObjectiveInfo lastObjectiveInfo;

    public Objective mainObjective;

    public int subIndex;

    private bool sequencePlaying;

    public void Set(Objective obj, int objIndex, string desc, bool loading)
    {
        mainObjective = obj;
        subIndex = objIndex;
        objectiveText.set_text(desc);
    }

    public void UpdateObjective(bool loading, int counter1 = -1, int counter2 = -1)
    {
        if (mainObjective != null && mainObjective.Locked)
        {
            if (lastObjectiveInfo == null)
            {
                lastObjectiveInfo = new ObjectiveInfo();
                lastObjectiveInfo.locked = true;
            }
            base.gameObject.SetActive(value: false);
            return;
        }
        base.gameObject.SetActive(value: true);
        bool flag = false;
        if (mainObjective != null)
        {
            if (subIndex == -1)
            {
                counter1 = (int)mainObjective.counter.x;
                counter2 = (int)mainObjective.counter.y;
                flag = mainObjective.done;
            }
            else
            {
                flag = mainObjective.dones[subIndex];
            }
        }
        else if (counter1 != -1 && counter2 != -1)
        {
            flag = (counter1 == counter2);
        }
        if (lastObjectiveInfo == null)
        {
            lastObjectiveInfo = new ObjectiveInfo();
            lastObjectiveInfo.locked = false;
            lastObjectiveInfo.counter1 = counter1;
            lastObjectiveInfo.counter2 = counter2;
            lastObjectiveInfo.completed = flag;
        }
        toggleObjective.set_isOn(flag);
        counter.set_text((counter2 == -1) ? counter1.ToString() : $"{counter1} / {counter2}");
        ((Component)(object)counter).gameObject.SetActive(!loading && (counter1 != -1 || counter2 != -1));
        ((Component)(object)complete).gameObject.SetActive(!loading);
        if (loading)
        {
            Hide();
        }
        else if (lastObjectiveInfo.locked)
        {
            OnNew();
        }
        else if (!lastObjectiveInfo.completed && flag)
        {
            OnComplete();
        }
        else if ((lastObjectiveInfo.completed && !flag) || (lastObjectiveInfo.counter1 != counter1 && counter2 != -1))
        {
            OnUpdate();
        }
        else
        {
            Hide();
        }
        lastObjectiveInfo.locked = false;
        lastObjectiveInfo.completed = flag;
        lastObjectiveInfo.counter1 = counter1;
        lastObjectiveInfo.counter2 = counter2;
    }

    public void OnNew()
    {
        completeCanvasGroup.alpha = 1f;
        complete.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("objective_new"));
        Move();
        FadeOut();
        PandoraSingleton<Pan>.Instance.Narrate("new_objective");
    }

    public void OnUpdate()
    {
        DOTween.Kill((object)this, true);
        completeCanvasGroup.alpha = 1f;
        complete.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("objective_updated"));
        Move();
        FadeOut();
        if (mainObjective != null)
        {
            PandoraSingleton<Pan>.Instance.Narrate("objective_updated");
        }
    }

    public void OnComplete()
    {
        DOTween.Kill((object)this, true);
        completeCanvasGroup.alpha = 1f;
        complete.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("objective_completed"));
        Move();
        FadeOut();
        if (mainObjective != null)
        {
            PandoraSingleton<Pan>.Instance.Narrate("objective_completed");
        }
    }

    public void FadeOut()
    {
        TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => completeCanvasGroup.alpha), (DOSetter<float>)delegate (float alpha)
        {
            completeCanvasGroup.alpha = alpha;
        }, 0f, 1f), 5f), (object)this);
    }

    public void Move()
    {
        RectTransform textTransform = (RectTransform)completeCanvasGroup.transform;
        Vector2 anchoredPosition = textTransform.anchoredPosition;
        anchoredPosition.x = startPositionX;
        textTransform.anchoredPosition = anchoredPosition;
        TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)delegate
        {
            Vector2 anchoredPosition3 = textTransform.anchoredPosition;
            return anchoredPosition3.x;
        }, (DOSetter<float>)delegate (float x)
        {
            Vector2 anchoredPosition2 = textTransform.anchoredPosition;
            anchoredPosition2.x = x;
            textTransform.anchoredPosition = anchoredPosition2;
        }, endPosition, 0.5f), (object)this);
    }

    public void Hide()
    {
        if (!DOTween.IsTweening((object)this))
        {
            completeCanvasGroup.alpha = 0f;
        }
    }
}
