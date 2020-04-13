using System.Collections.Generic;

public class KGFMessageList
{
    public abstract class Message
    {
        private string itsMessage;

        public string Description => itsMessage;

        public Message(string theMessage)
        {
            itsMessage = theMessage;
        }
    }

    public class Error : Message
    {
        public Error(string theMessage)
            : base(theMessage)
        {
        }
    }

    public class Info : Message
    {
        public Info(string theMessage)
            : base(theMessage)
        {
        }
    }

    public class Warning : Message
    {
        public Warning(string theMessage)
            : base(theMessage)
        {
        }
    }

    public bool itsHasErrors;

    public bool itsHasWarnings;

    public bool itsHasInfos;

    private List<Message> itsMessageList = new List<Message>();

    public void AddError(string theMessage)
    {
        itsHasErrors = true;
        itsMessageList.Add(new Error(theMessage));
    }

    public void AddInfo(string theMessage)
    {
        itsHasInfos = true;
        itsMessageList.Add(new Info(theMessage));
    }

    public void AddWarning(string theMessage)
    {
        itsHasWarnings = true;
        itsMessageList.Add(new Warning(theMessage));
    }

    public string[] GetErrorArray()
    {
        List<string> list = new List<string>();
        foreach (Message itsMessage in itsMessageList)
        {
            if (itsMessage is Error)
            {
                list.Add(itsMessage.Description);
            }
        }
        return list.ToArray();
    }

    public string[] GetInfoArray()
    {
        List<string> list = new List<string>();
        foreach (Message itsMessage in itsMessageList)
        {
            if (itsMessage is Info)
            {
                list.Add(itsMessage.Description);
            }
        }
        return list.ToArray();
    }

    public string[] GetWarningArray()
    {
        List<string> list = new List<string>();
        foreach (Message itsMessage in itsMessageList)
        {
            if (itsMessage is Warning)
            {
                list.Add(itsMessage.Description);
            }
        }
        return list.ToArray();
    }

    public Message[] GetAllMessagesArray()
    {
        return itsMessageList.ToArray();
    }

    public void AddMessages(Message[] theMessages)
    {
        foreach (Message message in theMessages)
        {
            itsMessageList.Add(message);
            if (message is Error)
            {
                itsHasErrors = true;
            }
            if (message is Warning)
            {
                itsHasWarnings = true;
            }
            if (message is Info)
            {
                itsHasInfos = true;
            }
        }
    }
}
