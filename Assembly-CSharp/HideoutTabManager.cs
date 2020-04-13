using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideoutTabManager : PandoraSingleton<HideoutTabManager>
{
    public UITabLoader tabLoaderLeft;

    public UITabLoader tabLoaderRight;

    public UITabLoader tabLoaderCenter;

    public GameObject tabLeftBg;

    public GameObject tabRightBg;

    public GameObject popupAnchor;

    public ButtonGroup button1;

    public ButtonGroup button2;

    public ButtonGroup button3;

    public ButtonGroup button4;

    public ButtonGroup button5;

    public Sprite icnBack;

    public Sprite icnOptions;

    public AudioSource audioSource;

    private Dictionary<ModuleId, UIModule> tabLeftModules;

    private Dictionary<ModuleId, UIModule> tabRightModules;

    private Dictionary<ModuleId, UIModule> tabCenterModules;

    private Dictionary<PopupModuleId, UIPopupModule> popUpModules;

    private Dictionary<PopupBgSize, GameObject> popUpBgs;

    private CanvasGroupDisabler guiCanvas;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        guiCanvas = GetComponent<CanvasGroupDisabler>();
    }

    public IEnumerator Load()
    {
        tabRightModules = new Dictionary<ModuleId, UIModule>();
        tabLoaderRight.Load(tabRightModules);
        yield return null;
        tabLeftModules = new Dictionary<ModuleId, UIModule>();
        tabLoaderLeft.Load(tabLeftModules);
        yield return null;
        tabCenterModules = new Dictionary<ModuleId, UIModule>();
        tabLoaderCenter.Load(tabCenterModules);
        yield return null;
        RegisterModules(popupAnchor, out popUpModules);
        RegisterBgs(popupAnchor, out popUpBgs);
    }

    public IEnumerator ParentModules()
    {
        tabLoaderRight.ParentModules();
        yield return null;
        tabLoaderLeft.ParentModules();
        yield return null;
        tabLoaderCenter.ParentModules();
    }

    private void Update()
    {
        if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("hide_gui", -1))
        {
            guiCanvas.enabled = !guiCanvas.enabled;
        }
    }

    public void DeactivateAllButtons()
    {
        button1.gameObject.SetActive(value: false);
        button2.gameObject.SetActive(value: false);
        button3.gameObject.SetActive(value: false);
        button4.gameObject.SetActive(value: false);
        button5.gameObject.SetActive(value: false);
    }

    private void RegisterModules(GameObject anchor, out Dictionary<ModuleId, UIModule> tabModules)
    {
        tabModules = new Dictionary<ModuleId, UIModule>();
        UIModule[] componentsInChildren = anchor.GetComponentsInChildren<UIModule>(includeInactive: true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            tabModules[componentsInChildren[i].moduleId] = componentsInChildren[i];
        }
    }

    private void RegisterModules(GameObject anchor, out Dictionary<PopupModuleId, UIPopupModule> tabModules)
    {
        tabModules = new Dictionary<PopupModuleId, UIPopupModule>();
        UIPopupModule[] componentsInChildren = anchor.GetComponentsInChildren<UIPopupModule>(includeInactive: true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            tabModules[componentsInChildren[i].popupModuleId] = componentsInChildren[i];
        }
    }

    private void RegisterBgs(GameObject anchor, out Dictionary<PopupBgSize, GameObject> tabModules)
    {
        tabModules = new Dictionary<PopupBgSize, GameObject>();
        Image[] componentsInChildren = anchor.GetComponentsInChildren<Image>(includeInactive: true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (((Component)(object)componentsInChildren[i]).gameObject.name.Contains(((PopupBgSize)j).ToLowerString()))
                {
                    tabModules[(PopupBgSize)j] = ((Component)(object)componentsInChildren[i]).gameObject;
                }
            }
        }
    }

    public void ActivateLeftTabModule(bool activate, params ModuleId[] modules)
    {
        ActivateTabModule(tabLeftModules, activate, modules);
    }

    public void ActivateRightTabModule(bool activate, params ModuleId[] modules)
    {
        ActivateTabModule(tabRightModules, activate, modules);
    }

    public void ActivateCenterTabModule(bool activate, params ModuleId[] modules)
    {
        ActivateTabModule(tabCenterModules, activate, modules);
    }

    public void ActivateLeftTabModules(bool displayBg, params ModuleId[] modules)
    {
        ActivateTabModules(tabLeftModules, modules);
        tabLeftBg.SetActive(value: false);
    }

    public void ActivateRightTabModules(bool displayBg, params ModuleId[] modules)
    {
        ActivateTabModules(tabRightModules, modules);
        tabRightBg.SetActive(value: false);
    }

    public void ActivateCenterTabModules(params ModuleId[] modules)
    {
        bool flag = false;
        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i] == ModuleId.NOTIFICATION)
            {
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            modules.AppendItem(ModuleId.NOTIFICATION);
        }
        ActivateTabModules(tabCenterModules, modules);
    }

    public void ActivatePopupModules(PopupBgSize popupSize, params PopupModuleId[] modules)
    {
        for (int i = 0; i < popUpBgs.Count; i++)
        {
            foreach (KeyValuePair<PopupBgSize, GameObject> popUpBg in popUpBgs)
            {
                if (popUpBg.Key == popupSize)
                {
                    popUpBg.Value.SetActive(value: true);
                }
                else
                {
                    popUpBg.Value.SetActive(value: false);
                }
            }
        }
        ActivateTabModules(popUpModules, modules);
    }

    private void ActivateTabModules<T, U>(Dictionary<T, U> tab, T[] modules) where U : UIPopupModule
    {
        foreach (KeyValuePair<T, U> item in tab)
        {
            SetModule(item.Value, Array.IndexOf(modules, item.Key) >= 0);
        }
    }

    private void ActivateTabModule(Dictionary<ModuleId, UIModule> tab, bool activate, ModuleId[] modules)
    {
        for (int i = 0; i < modules.Length; i++)
        {
            SetModule(tab[modules[i]], activate);
        }
    }

    private void SetModule<T>(T module, bool activate) where T : UIPopupModule
    {
        if (module.gameObject.activeSelf != activate)
        {
            module.gameObject.SetActive(activate);
        }
        module.SetInteractable(activate);
        if (activate && !module.initialized)
        {
            module.Init();
        }
    }

    public List<T> GetModulesPopup<T>(params PopupModuleId[] modules) where T : UIPopupModule
    {
        List<T> list = new List<T>();
        for (int i = 0; i < modules.Length; i++)
        {
            list.Add((T)popUpModules[modules[i]]);
        }
        return list;
    }

    public T GetModulePopup<T>(PopupModuleId moduleKey) where T : UIPopupModule
    {
        return (T)popUpModules[moduleKey];
    }

    public T GetModuleLeft<T>(ModuleId moduleKey) where T : UIModule
    {
        return (T)tabLeftModules[moduleKey];
    }

    public T GetModuleRight<T>(ModuleId moduleKey) where T : UIModule
    {
        return (T)tabRightModules[moduleKey];
    }

    public T GetModuleCenter<T>(ModuleId moduleKey) where T : UIModule
    {
        return (T)tabCenterModules[moduleKey];
    }
}
