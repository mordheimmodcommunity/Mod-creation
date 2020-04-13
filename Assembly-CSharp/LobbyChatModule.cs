using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyChatModule : UIModule
{
    public ScrollGroup messages;

    public GameObject item;

    public InputField text;

    private GameObject prevSelected;

    public void Setup()
    {
        messages.Setup(item, hideBarIfEmpty: true);
        messages.ClearList();
        ((UnityEventBase)(object)text.get_onEndEdit()).RemoveAllListeners();
        ((UnityEvent<string>)(object)text.get_onEndEdit()).AddListener((UnityAction<string>)SendChat);
        ((KeyUpInputField)(object)text).OnSelectCallBack = Select;
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.HERMES_CHAT, GetChat);
        prevSelected = null;
    }

    public void Select()
    {
        prevSelected = EventSystem.get_current().get_currentSelectedGameObject();
        text.ActivateInputField();
        PandoraSingleton<PandoraInput>.Instance.PushInputLayer(PandoraInput.InputLayer.CHAT);
    }

    private void GetChat()
    {
        ulong num = (ulong)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
        string text = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
        GameObject gameObject = messages.AddToList(null, null);
        Text component = gameObject.GetComponent<Text>();
        if (num != 0L)
        {
            ((Graphic)((Component)(object)component).GetComponent<Text>()).set_color(new Color(127f, 127f, 0f));
        }
        component.set_text(text);
    }

    public void SendChat(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            PandoraSingleton<Hermes>.Instance.SendChat(message);
            text.set_text(string.Empty);
        }
        text.DeactivateInputField();
        if (prevSelected != null)
        {
            prevSelected.SetSelected();
        }
        prevSelected = null;
        PandoraSingleton<PandoraInput>.Instance.PopInputLayer(PandoraInput.InputLayer.CHAT);
    }
}
