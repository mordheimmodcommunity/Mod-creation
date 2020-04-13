using System;
using System.Collections.Generic;
using UnityEngine;

public class NoticeManager : PandoraSingleton<NoticeManager>
{
    private Dictionary<Notices, List<DelReceiveNotice>> registry;

    public List<object> Parameters
    {
        get;
        private set;
    }

    public Notices NoticeId
    {
        get;
        private set;
    }

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        registry = new Dictionary<Notices, List<DelReceiveNotice>>(NoticesComparer.Instance);
        Parameters = new List<object>(8);
    }

    public void RegisterListener(Notices noticeId, DelReceiveNotice del)
    {
        if (!registry.ContainsKey(noticeId))
        {
            registry[noticeId] = new List<DelReceiveNotice>();
        }
        if (registry[noticeId].IndexOf(del) == -1)
        {
            registry[noticeId].Add(del);
        }
    }

    public void RemoveListener(Notices noticeId, DelReceiveNotice del)
    {
        if (registry.ContainsKey(noticeId))
        {
            int num = registry[noticeId].IndexOf(del);
            if (num != -1)
            {
                registry[noticeId].RemoveAt(num);
            }
        }
    }

    public void SendNotice(Notices noticeId)
    {
        if (registry != null && registry.ContainsKey(noticeId))
        {
            List<DelReceiveNotice> list = registry[noticeId];
            NoticeId = noticeId;
            Parameters.Clear();
            for (int num = list.Count - 1; num >= 0; num--)
            {
                try
                {
                    list[num]();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }

    public void SendNotice<T1>(Notices noticeId, T1 v1)
    {
        if (registry.ContainsKey(noticeId))
        {
            List<DelReceiveNotice> list = registry[noticeId];
            NoticeId = noticeId;
            for (int num = list.Count - 1; num >= 0; num--)
            {
                Parameters.Clear();
                Parameters.Add(v1);
                try
                {
                    list[num]();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            Parameters.Clear();
        }
    }

    public void SendNotice<T1, T2>(Notices noticeId, T1 v1, T2 v2)
    {
        if (registry.ContainsKey(noticeId))
        {
            List<DelReceiveNotice> list = registry[noticeId];
            NoticeId = noticeId;
            for (int num = list.Count - 1; num >= 0; num--)
            {
                Parameters.Clear();
                Parameters.Add(v1);
                Parameters.Add(v2);
                try
                {
                    list[num]();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            Parameters.Clear();
        }
    }

    public void SendNotice<T1, T2, T3>(Notices noticeId, T1 v1, T2 v2, T3 v3)
    {
        if (registry.ContainsKey(noticeId))
        {
            List<DelReceiveNotice> list = registry[noticeId];
            NoticeId = noticeId;
            for (int num = list.Count - 1; num >= 0; num--)
            {
                Parameters.Clear();
                Parameters.Add(v1);
                Parameters.Add(v2);
                Parameters.Add(v3);
                try
                {
                    list[num]();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            Parameters.Clear();
        }
    }

    public void SendNotice<T1, T2, T3, T4>(Notices noticeId, T1 v1, T2 v2, T3 v3, T4 v4)
    {
        if (registry.ContainsKey(noticeId))
        {
            List<DelReceiveNotice> list = registry[noticeId];
            NoticeId = noticeId;
            for (int num = list.Count - 1; num >= 0; num--)
            {
                Parameters.Clear();
                Parameters.Add(v1);
                Parameters.Add(v2);
                Parameters.Add(v3);
                Parameters.Add(v4);
                try
                {
                    list[num]();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            Parameters.Clear();
        }
    }

    public void SendNotice<T1, T2, T3, T4, T5>(Notices noticeId, T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
    {
        if (registry.ContainsKey(noticeId))
        {
            List<DelReceiveNotice> list = registry[noticeId];
            NoticeId = noticeId;
            for (int num = list.Count - 1; num >= 0; num--)
            {
                Parameters.Clear();
                Parameters.Add(v1);
                Parameters.Add(v2);
                Parameters.Add(v3);
                Parameters.Add(v4);
                Parameters.Add(v5);
                try
                {
                    list[num]();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            Parameters.Clear();
        }
    }

    public void SendNotice<T1, T2, T3, T4, T5, T6>(Notices noticeId, T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
    {
        if (registry.ContainsKey(noticeId))
        {
            List<DelReceiveNotice> list = registry[noticeId];
            NoticeId = noticeId;
            for (int num = list.Count - 1; num >= 0; num--)
            {
                Parameters.Clear();
                Parameters.Add(v1);
                Parameters.Add(v2);
                Parameters.Add(v3);
                Parameters.Add(v4);
                Parameters.Add(v5);
                Parameters.Add(v6);
                try
                {
                    list[num]();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            Parameters.Clear();
        }
    }

    public void Clear()
    {
        Parameters.Clear();
        registry.Clear();
    }
}
