using System.Collections.Generic;

public class MessageManager
{
    private Dictionary<int, List<string>> messages;

    private int lastMessageTurn;

    public int currentTurn;

    public int currentPos;

    private MessageDelegate del;

    private CampaignMissionId currentMissionId;

    public void Init(CampaignMissionId missionId)
    {
        currentMissionId = missionId;
        messages = new Dictionary<int, List<string>>();
        if (missionId != 0)
        {
            lastMessageTurn = 0;
            DataFactory instance = PandoraSingleton<DataFactory>.Instance;
            int num = (int)missionId;
            List<CampaignMissionMessageData> list = instance.InitData<CampaignMissionMessageData>("fk_campaign_mission_id", num.ToString());
            using (List<CampaignMissionMessageData>.Enumerator enumerator = list.GetEnumerator())
            {
                CampaignMissionMessageData mesData;
                while (enumerator.MoveNext())
                {
                    mesData = enumerator.Current;
                    if (!messages.ContainsKey(mesData.UnitTurn))
                    {
                        List<CampaignMissionMessageData> list2 = list.FindAll((CampaignMissionMessageData x) => x.UnitTurn == mesData.UnitTurn);
                        list2.Sort(new MessageSorter());
                        messages[mesData.UnitTurn] = list2.ConvertAll((CampaignMissionMessageData x) => x.Label);
                        if (mesData.UnitTurn >= lastMessageTurn)
                        {
                            lastMessageTurn = mesData.UnitTurn;
                        }
                    }
                }
            }
            currentTurn = 0;
        }
    }

    public bool DisplayNewTurn(MessageDelegate displayedDel = null)
    {
        currentTurn++;
        currentPos = 0;
        return DisplayMessage(displayedDel);
    }

    public bool DisplayNextPos(MessageDelegate displayedDel = null)
    {
        currentPos++;
        return DisplayMessage(displayedDel);
    }

    private bool DisplayMessage(MessageDelegate displayedDel = null)
    {
        del = displayedDel;
        if (messages.ContainsKey(currentTurn) && currentPos < messages[currentTurn].Count)
        {
            bool v = currentTurn == lastMessageTurn && messages[currentTurn].Count - 1 == currentPos;
            string text = messages[currentTurn][currentPos];
            if (PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK)
            {
                switch (text)
                {
                    case "tuto_01_message_02":
                    case "tuto_03_message_07":
                    case "tuto_03_message_11":
                        text += "_console";
                        break;
                }
            }
            PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.GAME_TUTO_MESSAGE, v1: true, text, v);
            PandoraDebug.LogInfo(messages[currentTurn][currentPos], "MESSAGE");
            if (del != null)
            {
                del();
                del = null;
            }
            return true;
        }
        return false;
    }

    private void OnMessageDisplayed()
    {
        if (del != null)
        {
            del();
            del = null;
        }
    }
}
