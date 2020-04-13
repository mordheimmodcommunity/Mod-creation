using System.Collections.Generic;

public class EventLogger
{
    public enum LogEvent
    {
        NONE,
        HIRE,
        FIRE,
        DEATH,
        RETIREMENT,
        LEFT,
        INJURY,
        MUTATION,
        RECOVERY,
        NO_TREATMENT,
        SKILL,
        SPELL,
        SHIPMENT_REQUEST,
        SHIPMENT_DELIVERY,
        SHIPMENT_LATE,
        FACTION0_DELIVERY,
        FACTION1_DELIVERY,
        FACTION2_DELIVERY,
        NEW_MISSION,
        CONTACT_ITEM,
        MARKET_ROTATION,
        OUTSIDER_ROTATION,
        WARBAND_CREATED,
        RANK_ACHIEVED,
        MEMORABLE_CAMPAIGN_VICTORY,
        VICTORY_STREAK,
        MEMORABLE_KILL,
        MISSION_REWARDS,
        RESPEC_POINT,
        COUNT
    }

    public List<Tuple<int, LogEvent, int>> history;

    public EventLogger(List<Tuple<int, LogEvent, int>> log)
    {
        history = log;
    }

    public void RemoveLastHistory(LogEvent logEvent)
    {
        for (int num = history.Count - 1; num >= 0; num--)
        {
            if (history[num].Item2 == logEvent)
            {
                history.RemoveAt(num);
            }
        }
    }

    public void RemoveHistory(Tuple<int, LogEvent, int> log)
    {
        history.Remove(log);
    }

    public void AddHistory(int date, LogEvent logEvent, int data)
    {
        int num = history.Count;
        while (num > 0 && history[num - 1].Item1 > date)
        {
            num--;
        }
        history.Insert(num, new Tuple<int, LogEvent, int>(date, logEvent, data));
    }

    public Tuple<int, LogEvent, int> LastHistoryEvent()
    {
        if (history.Count > 0)
        {
            return history[history.Count - 1];
        }
        return null;
    }

    public bool HasEventAtDay(LogEvent logEvent, int date)
    {
        List<Tuple<int, LogEvent, int>> list = new List<Tuple<int, LogEvent, int>>();
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].Item1 == date && history[i].Item2 == logEvent)
            {
                return true;
            }
        }
        return false;
    }

    public Tuple<int, LogEvent, int> FindLastEvent(LogEvent logEvent)
    {
        for (int num = history.Count - 1; num >= 0; num--)
        {
            if (history[num].Item2 == logEvent)
            {
                return history[num];
            }
        }
        return null;
    }

    public Tuple<int, LogEvent, int> FindEventBefore(LogEvent logEvent, int date)
    {
        for (int num = history.Count - 1; num >= 0; num--)
        {
            if (history[num].Item1 <= date && history[num].Item2 == logEvent)
            {
                return history[num];
            }
        }
        return null;
    }

    public Tuple<int, LogEvent, int> FindEventAfter(LogEvent logEvent, int date)
    {
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].Item1 >= date && history[i].Item2 == logEvent)
            {
                return history[i];
            }
        }
        return null;
    }

    public List<Tuple<int, LogEvent, int>> FindEventsAfter(LogEvent logEvent, int date)
    {
        List<Tuple<int, LogEvent, int>> list = new List<Tuple<int, LogEvent, int>>();
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].Item1 >= date && history[i].Item2 == logEvent)
            {
                list.Add(history[i]);
            }
        }
        return list;
    }

    public List<Tuple<int, LogEvent, int>> FindEventsAtDay(LogEvent logEvent, int date)
    {
        List<Tuple<int, LogEvent, int>> list = new List<Tuple<int, LogEvent, int>>();
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].Item1 == date && history[i].Item2 == logEvent)
            {
                list.Add(history[i]);
            }
        }
        return list;
    }

    public Tuple<int, LogEvent, int> FindEventBetween(LogEvent logEvent, int from, int to)
    {
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].Item1 >= from && history[i].Item1 <= to && history[i].Item2 == logEvent)
            {
                return history[i];
            }
        }
        return null;
    }

    public List<Tuple<int, LogEvent, int>> GetEventsBetween(int from, int to)
    {
        List<Tuple<int, LogEvent, int>> list = new List<Tuple<int, LogEvent, int>>();
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].Item1 >= from && history[i].Item1 <= to)
            {
                list.Add(history[i]);
            }
        }
        return list;
    }

    public List<Tuple<int, LogEvent, int>> GetEventsAtDay(int day)
    {
        List<Tuple<int, LogEvent, int>> list = new List<Tuple<int, LogEvent, int>>();
        for (int num = history.Count - 1; num >= 0; num--)
        {
            Tuple<int, LogEvent, int> tuple = history[num];
            if (tuple.Item1 < day)
            {
                break;
            }
            if (tuple.Item1 == day)
            {
                list.Add(tuple);
            }
        }
        return list;
    }

    public List<Tuple<int, LogEvent, int>> GetEventsFromDay(int day)
    {
        List<Tuple<int, LogEvent, int>> list = new List<Tuple<int, LogEvent, int>>();
        if (history.Count > 0)
        {
            int i;
            for (i = history.Count - 1; i >= 0; i--)
            {
                if (history[i].Item1 <= day)
                {
                    i++;
                    break;
                }
            }
            if (i < 0)
            {
                i++;
            }
            for (; i < history.Count; i++)
            {
                list.Add(history[i]);
            }
        }
        return list;
    }
}
