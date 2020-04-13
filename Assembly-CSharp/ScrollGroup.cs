using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollGroup : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
    public RectTransform contentRoot;

    public Scrollbar scrollbar;

    public ScrollRect scrollRect;

    public ScrollRectEnsureVisible ensureVisible;

    [Tooltip("Maximum number of items in the list. Adding more will remove the older ones. 0 is unlimited")]
    public int maxItems;

    private GameObject item;

    public List<GameObject> items = new List<GameObject>();

    public List<GameObject> itemsToBeDestroyed = new List<GameObject>();

    public bool hideBarIfEmpty;

    [Tooltip("Will not show the scrollbar if content fits in the content rect.")]
    public bool hideBarIfUnnecessary;

    public bool fixedSizeHandle;

    private float midPoint;

    private bool hideScrollbar;

    public bool useNavigation = true;

    [Tooltip("Determines if the scroll group will select focus the latest added item rather than the first.")]
    public bool preferLatestAdded;

    private Coroutine realignListCoroutine;

    public int CurrentIndex
    {
        get;
        private set;
    }

    private void Awake()
    {
        CurrentIndex = -1;
        if ((Object)(object)scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>();
        }
        if (contentRoot == null)
        {
            contentRoot = (RectTransform)((Component)(object)scrollRect).transform.GetChild(0);
        }
        if ((Object)(object)scrollbar == null)
        {
            scrollbar = GetComponentInChildren<Scrollbar>();
        }
        ensureVisible = ((Component)(object)scrollRect).GetComponent<ScrollRectEnsureVisible>();
        if (hideBarIfEmpty)
        {
            ((Component)(object)scrollbar).gameObject.SetActive(value: false);
        }
        if (fixedSizeHandle)
        {
            ((UnityEvent<float>)(object)scrollbar.get_onValueChanged()).AddListener((UnityAction<float>)ResizeHandle);
            ResizeHandle(0f);
        }
    }

    private void ResizeHandle(float value)
    {
        scrollbar.set_size(0f);
    }

    public void Setup(GameObject item, bool hideBarIfEmpty)
    {
        this.item = item;
        this.hideBarIfEmpty = hideBarIfEmpty;
    }

    public void ClearList()
    {
        CurrentIndex = -1;
        for (int i = 0; i < items.Count; i++)
        {
            items[i].transform.SetParent(null);
            items[i].SetActive(value: false);
            itemsToBeDestroyed.Add(items[i]);
        }
        items.Clear();
        if (hideBarIfEmpty && (Object)(object)scrollbar != null)
        {
            ((Component)(object)scrollbar).gameObject.SetActive(value: false);
        }
        scrollRect.set_normalizedPosition(Vector2.zero);
        HightlightAnimate component = GetComponent<HightlightAnimate>();
        if (component != null)
        {
            component.Deactivate();
        }
    }

    public void RemoveItemAt(int index)
    {
        //IL_0024: Unknown result type (might be due to invalid IL or missing references)
        //IL_0029: Unknown result type (might be due to invalid IL or missing references)
        //IL_0059: Unknown result type (might be due to invalid IL or missing references)
        //IL_005e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0092: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cd: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d2: Unknown result type (might be due to invalid IL or missing references)
        //IL_0107: Unknown result type (might be due to invalid IL or missing references)
        if (items.Count > 1)
        {
            Toggle component = items[index].GetComponent<Toggle>();
            Navigation navigation = ((Selectable)component).get_navigation();
            bool flag = component.get_isOn();
            if (index < items.Count - 2)
            {
                Toggle component2 = items[index + 1].GetComponent<Toggle>();
                Navigation navigation2 = ((Selectable)component2).get_navigation();
                if (scrollRect.get_horizontal())
                {
                    ((Navigation)(ref navigation2)).set_selectOnRight(((Navigation)(ref navigation)).get_selectOnRight());
                }
                else
                {
                    ((Navigation)(ref navigation2)).set_selectOnUp(((Navigation)(ref navigation)).get_selectOnUp());
                }
                ((Selectable)component2).set_navigation(navigation2);
                if (flag)
                {
                    StartCoroutine(SelectOnNextFrame(component2));
                    flag = false;
                }
            }
            if (index > 0)
            {
                Toggle component3 = items[index - 1].GetComponent<Toggle>();
                Navigation navigation3 = ((Selectable)component3).get_navigation();
                if (scrollRect.get_horizontal())
                {
                    ((Navigation)(ref navigation3)).set_selectOnLeft(((Navigation)(ref navigation)).get_selectOnLeft());
                }
                else
                {
                    ((Navigation)(ref navigation3)).set_selectOnDown(((Navigation)(ref navigation)).get_selectOnDown());
                }
                ((Selectable)component3).set_navigation(navigation3);
                if (flag)
                {
                    StartCoroutine(SelectOnNextFrame(component3));
                    flag = false;
                }
            }
        }
        itemsToBeDestroyed.Add(items[index]);
        items.RemoveAt(index);
    }

    public IEnumerator SelectOnNextFrame(Toggle toggle)
    {
        yield return false;
        toggle.set_isOn(true);
        ((MonoBehaviour)(object)toggle).SetSelected(force: true);
    }

    public void OnEnable()
    {
        if (CurrentIndex != -1)
        {
            RealignList(isOn: true, CurrentIndex, force: true);
        }
        if (hideBarIfUnnecessary)
        {
            CheckUnnecessaryBars();
        }
    }

    public GameObject AddToList(Selectable up, Selectable down)
    {
        //IL_016a: Unknown result type (might be due to invalid IL or missing references)
        //IL_016f: Unknown result type (might be due to invalid IL or missing references)
        //IL_01f9: Unknown result type (might be due to invalid IL or missing references)
        //IL_01fe: Unknown result type (might be due to invalid IL or missing references)
        //IL_022b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0242: Unknown result type (might be due to invalid IL or missing references)
        if (CurrentIndex == -1)
        {
            CurrentIndex = 0;
        }
        GameObject go = Object.Instantiate(item);
        go.transform.SetParent(contentRoot, worldPositionStays: false);
        if ((Object)(object)scrollbar != null && !hideScrollbar)
        {
            ((Component)(object)scrollbar).gameObject.SetActive(value: true);
        }
        if (maxItems != 0 && maxItems <= items.Count)
        {
            items[0].transform.SetParent(null);
            Object.Destroy(items[0]);
            items.RemoveAt(0);
        }
        items.Add(go);
        int num = items.Count - 1;
        Toggle[] componentsInChildren = go.GetComponentsInChildren<Toggle>(includeInactive: true);
        if (hideBarIfUnnecessary && (Object)(object)scrollbar != null)
        {
            ((Component)(object)scrollbar).gameObject.SetActive(value: false);
            if (base.gameObject.activeInHierarchy)
            {
                StartCoroutine(CheckUnnecessaryBars());
            }
        }
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            ((UnityEvent<bool>)(object)componentsInChildren[i].onValueChanged).AddListener((UnityAction<bool>)delegate (bool isOn)
            {
                RealignList(isOn, go);
            });
            Navigation navigation = ((Selectable)componentsInChildren[i]).get_navigation();
            if (useNavigation)
            {
                ((Navigation)(ref navigation)).set_mode((Mode)4);
                ((Navigation)(ref navigation)).set_selectOnUp(up);
                ((Navigation)(ref navigation)).set_selectOnDown(down);
                if (items.Count > 1)
                {
                    Selectable component = items[items.Count - 2].GetComponent<Selectable>();
                    if ((Object)(object)component != null)
                    {
                        if (scrollRect.get_horizontal())
                        {
                            ((Navigation)(ref navigation)).set_selectOnLeft(component);
                        }
                        else
                        {
                            ((Navigation)(ref navigation)).set_selectOnUp(component);
                        }
                        Navigation navigation2 = component.get_navigation();
                        if (scrollRect.get_horizontal())
                        {
                            ((Navigation)(ref navigation2)).set_selectOnRight((Selectable)(object)componentsInChildren[i]);
                        }
                        else
                        {
                            ((Navigation)(ref navigation2)).set_selectOnDown((Selectable)(object)componentsInChildren[i]);
                        }
                        component.set_navigation(navigation2);
                    }
                }
            }
            else
            {
                ((Navigation)(ref navigation)).set_mode((Mode)0);
            }
            ((Selectable)componentsInChildren[i]).set_navigation(navigation);
        }
        if (base.gameObject.activeInHierarchy)
        {
            if (realignListCoroutine != null)
            {
                StopCoroutine(realignListCoroutine);
            }
            realignListCoroutine = StartCoroutine(RepositionScrollListOnNextFrameCoroutine());
        }
        return go;
    }

    public void RealignList(bool isOn, int index, bool force = false)
    {
        if (isOn && base.isActiveAndEnabled && items.Count > index)
        {
            CurrentIndex = index;
            if (ensureVisible != null && (force || PandoraSingleton<PandoraInput>.Instance.lastInputMode != PandoraInput.InputMode.MOUSE))
            {
                ensureVisible.CenterOnItem((RectTransform)items[index].transform);
            }
        }
    }

    public void RealignList(bool isOn, GameObject go, bool force = false)
    {
        if (!isOn || !base.isActiveAndEnabled)
        {
            return;
        }
        int num = 0;
        while (true)
        {
            if (num < items.Count)
            {
                if (go == items[num])
                {
                    break;
                }
                num++;
                continue;
            }
            return;
        }
        CurrentIndex = num;
        if (ensureVisible != null && (force || PandoraSingleton<PandoraInput>.Instance.lastInputMode != PandoraInput.InputMode.MOUSE))
        {
            ensureVisible.CenterOnItem((RectTransform)items[num].transform);
        }
    }

    public void ResetSelection()
    {
        //IL_002f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        //IL_0035: Unknown result type (might be due to invalid IL or missing references)
        //IL_004b: Expected I4, but got Unknown
        if (items.Count <= 0)
        {
            return;
        }
        CurrentIndex = 0;
        if ((Object)(object)scrollbar != null)
        {
            Direction direction = scrollbar.get_direction();
            switch ((int)direction)
            {
                case 2:
                    scrollRect.set_verticalNormalizedPosition((!preferLatestAdded) ? 1f : 0f);
                    break;
                case 3:
                    scrollRect.set_verticalNormalizedPosition((!preferLatestAdded) ? 0f : 1f);
                    break;
                case 0:
                    scrollRect.set_verticalNormalizedPosition((!preferLatestAdded) ? 0f : 1f);
                    break;
                case 1:
                    scrollRect.set_verticalNormalizedPosition((!preferLatestAdded) ? 1f : 0f);
                    break;
            }
        }
        else
        {
            RealignList(isOn: true, 0, force: true);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (CurrentIndex == -1)
        {
            CurrentIndex = 0;
        }
        if (CurrentIndex >= 0 && items.Count > 0)
        {
            items[CurrentIndex].SetSelected(force: true);
        }
    }

    public void HideScrollbar()
    {
        hideScrollbar = true;
        if ((Object)(object)scrollbar != null)
        {
            ((Component)(object)scrollbar).gameObject.SetActive(value: false);
        }
    }

    public void ShowScrollbar(bool forceShow = true)
    {
        hideScrollbar = false;
        if ((Object)(object)scrollbar != null)
        {
            if (forceShow || !hideBarIfUnnecessary)
            {
                ((Component)(object)scrollbar).gameObject.SetActive(value: true);
            }
            else if (hideBarIfUnnecessary && base.gameObject.activeInHierarchy)
            {
                StartCoroutine(CheckUnnecessaryBars());
            }
        }
    }

    private IEnumerator CheckUnnecessaryBars()
    {
        yield return null;
        yield return null;
        yield return null;
        if ((Object)(object)scrollbar != null)
        {
            ((Component)(object)scrollbar).gameObject.SetActive(scrollbar.get_size() < 0.999f && !hideScrollbar && items.Count > 0);
        }
    }

    public void RepositionScrollListOnNextFrame()
    {
        StartCoroutine(RepositionScrollListOnNextFrameCoroutine());
    }

    private IEnumerator RepositionScrollListOnNextFrameCoroutine()
    {
        yield return null;
        ResetSelection();
    }

    public void ForceScroll(bool isNegative, bool setSelected = true)
    {
        //IL_0016: Unknown result type (might be due to invalid IL or missing references)
        //IL_001c: Expected O, but got Unknown
        if ((Object)(object)scrollbar != null)
        {
            PointerEventData val = (PointerEventData)(object)new PointerEventData(EventSystem.get_current());
            if (setSelected)
            {
                ((BaseEventData)val).set_selectedObject(((Component)(object)scrollbar).gameObject);
            }
            val.set_scrollDelta((!isNegative) ? Vector2.one : (-Vector2.one));
            scrollRect.OnScroll(val);
        }
    }

    private void LateUpdate()
    {
        DestroyItems();
    }

    public void DestroyItems()
    {
        if (itemsToBeDestroyed.Count > 0)
        {
            for (int i = 0; i < itemsToBeDestroyed.Count; i++)
            {
                Object.Destroy(itemsToBeDestroyed[i]);
            }
            itemsToBeDestroyed.Clear();
        }
    }
}
