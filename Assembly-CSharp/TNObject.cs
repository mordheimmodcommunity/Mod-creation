using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TNet;
using UnityEngine;

[AddComponentMenu("TNet/Network Object")]
[ExecuteInEditMode]
public sealed class TNObject : MonoBehaviour
{
	private class DelayedCall
	{
		public uint objID;

		public byte funcID;

		public string funcName;

		public object[] parameters;
	}

	private static int mDummyID = 0;

	private static TNet.List<TNObject> mList = new TNet.List<TNObject>();

	private static Dictionary<uint, TNObject> mDictionary = new Dictionary<uint, TNObject>();

	private static TNet.List<DelayedCall> mDelayed = new TNet.List<DelayedCall>();

	[SerializeField]
	private int id;

	[NonSerialized]
	[HideInInspector]
	public bool rebuildMethodList = true;

	[NonSerialized]
	private TNet.List<CachedFunc> mRFCs = new TNet.List<CachedFunc>();

	[NonSerialized]
	private bool mIsRegistered;

	[NonSerialized]
	private int mOwner = -1;

	[NonSerialized]
	private TNObject mParent;

	public uint uid
	{
		get
		{
			return (uint)((!(mParent != null)) ? id : ((int)mParent.uid));
		}
		set
		{
			if (mParent != null)
			{
				mParent.uid = value;
			}
			else
			{
				id = (int)(value & 0xFFFFFF);
			}
		}
	}

	public bool hasParent => mParent != null;

	public bool isMine => (mOwner != -1) ? (mOwner == TNManager.playerID) : TNManager.isThisMyObject;

	public int ownerID
	{
		get
		{
			return (!(mParent != null)) ? mOwner : mParent.ownerID;
		}
		set
		{
			if (mParent != null)
			{
				mParent.ownerID = value;
			}
			else if (mOwner != value)
			{
				Send("SetOwner", Target.All, value);
			}
		}
	}

	[RFC]
	private void SetOwner(int val)
	{
		mOwner = val;
	}

	private void Awake()
	{
		if (id == 0 && !TNManager.isConnected)
		{
			id = ++mDummyID;
		}
		mOwner = TNManager.objectOwnerID;
		if (TNManager.GetPlayer(mOwner) == null)
		{
			mOwner = TNManager.hostID;
		}
	}

	private void OnNetworkPlayerLeave(Player p)
	{
		if (p != null && mOwner == p.id)
		{
			mOwner = TNManager.hostID;
		}
	}

	public static TNObject Find(uint tnID)
	{
		if (mDictionary == null)
		{
			return null;
		}
		TNObject value = null;
		mDictionary.TryGetValue(tnID, out value);
		return value;
	}

	private static TNObject FindParent(Transform t)
	{
		while (t != null)
		{
			TNObject component = t.gameObject.GetComponent<TNObject>();
			if (component != null)
			{
				return component;
			}
			t = t.parent;
		}
		return null;
	}

	private void Start()
	{
		if (id == 0)
		{
			mParent = FindParent(base.transform.parent);
			if (TNManager.isConnected && mParent == null && Application.isPlaying)
			{
				Debug.LogError("Objects that are not instantiated via TNManager.Create must have a non-zero ID.", this);
			}
			return;
		}
		Register();
		int num = 0;
		while (num < mDelayed.size)
		{
			DelayedCall delayedCall = mDelayed[num];
			if (delayedCall.objID == uid)
			{
				if (!string.IsNullOrEmpty(delayedCall.funcName))
				{
					Execute(delayedCall.funcName, delayedCall.parameters);
				}
				else
				{
					Execute(delayedCall.funcID, delayedCall.parameters);
				}
				mDelayed.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private void OnDestroy()
	{
		Unregister();
	}

	public void Register()
	{
		if (!mIsRegistered && uid != 0 && mParent == null)
		{
			mDictionary[uid] = this;
			mList.Add(this);
			mIsRegistered = true;
		}
	}

	private void Unregister()
	{
		if (mIsRegistered)
		{
			if (mDictionary != null)
			{
				mDictionary.Remove(uid);
			}
			if (mList != null)
			{
				mList.Remove(this);
			}
			mIsRegistered = false;
		}
	}

	public bool Execute(byte funcID, params object[] parameters)
	{
		if (mParent != null)
		{
			return mParent.Execute(funcID, parameters);
		}
		if (rebuildMethodList)
		{
			RebuildMethodList();
		}
		return UnityTools.ExecuteAll(mRFCs, funcID, parameters);
	}

	public bool Execute(string funcName, params object[] parameters)
	{
		if (mParent != null)
		{
			return mParent.Execute(funcName, parameters);
		}
		if (rebuildMethodList)
		{
			RebuildMethodList();
		}
		return UnityTools.ExecuteAll(mRFCs, funcName, parameters);
	}

	public static void FindAndExecute(uint objID, byte funcID, params object[] parameters)
	{
		TNObject tNObject = Find(objID);
		if (tNObject != null)
		{
			if (tNObject.Execute(funcID, parameters))
			{
			}
		}
		else if (TNManager.isInChannel)
		{
			DelayedCall delayedCall = new DelayedCall();
			delayedCall.objID = objID;
			delayedCall.funcID = funcID;
			delayedCall.parameters = parameters;
			mDelayed.Add(delayedCall);
		}
	}

	public static void FindAndExecute(uint objID, string funcName, params object[] parameters)
	{
		TNObject tNObject = Find(objID);
		if (tNObject != null)
		{
			if (tNObject.Execute(funcName, parameters))
			{
			}
		}
		else if (TNManager.isInChannel)
		{
			DelayedCall delayedCall = new DelayedCall();
			delayedCall.objID = objID;
			delayedCall.funcName = funcName;
			delayedCall.parameters = parameters;
			mDelayed.Add(delayedCall);
		}
	}

	private void RebuildMethodList()
	{
		rebuildMethodList = false;
		mRFCs.Clear();
		MonoBehaviour[] components = GetComponents<MonoBehaviour>();
		int i = 0;
		for (int num = components.Length; i < num; i++)
		{
			MonoBehaviour monoBehaviour = components[i];
			Type type = monoBehaviour.GetType();
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int j = 0; j < methods.Length; j++)
			{
				if (methods[j].IsDefined(typeof(RFC), inherit: true))
				{
					CachedFunc item = default(CachedFunc);
					item.obj = monoBehaviour;
					item.func = methods[j];
					RFC rFC = (RFC)item.func.GetCustomAttributes(typeof(RFC), inherit: true)[0];
					item.id = rFC.id;
					mRFCs.Add(item);
				}
			}
		}
	}

	public void Send(byte rfcID, Target target, params object[] objs)
	{
		SendRFC(rfcID, null, target, reliable: true, objs);
	}

	public void Send(string rfcName, Target target, params object[] objs)
	{
		SendRFC(0, rfcName, target, reliable: true, objs);
	}

	public void Send(byte rfcID, Player target, params object[] objs)
	{
		if (target != null)
		{
			SendRFC(rfcID, null, target, reliable: true, objs);
		}
		else
		{
			SendRFC(rfcID, null, Target.All, reliable: true, objs);
		}
	}

	public void Send(string rfcName, Player target, params object[] objs)
	{
		if (target != null)
		{
			SendRFC(0, rfcName, target, reliable: true, objs);
		}
		else
		{
			SendRFC(0, rfcName, Target.All, reliable: true, objs);
		}
	}

	public void SendQuickly(byte rfcID, Target target, params object[] objs)
	{
		SendRFC(rfcID, null, target, reliable: false, objs);
	}

	public void SendQuickly(string rfcName, Target target, params object[] objs)
	{
		SendRFC(0, rfcName, target, reliable: false, objs);
	}

	public void SendQuickly(byte rfcID, Player target, params object[] objs)
	{
		if (target != null)
		{
			SendRFC(rfcID, null, target, reliable: false, objs);
		}
		else
		{
			SendRFC(rfcID, null, Target.All, reliable: false, objs);
		}
	}

	public void SendQuickly(string rfcName, Player target, params object[] objs)
	{
		SendRFC(0, rfcName, target, reliable: false, objs);
	}

	public void BroadcastToLAN(int port, byte rfcID, params object[] objs)
	{
		BroadcastToLAN(port, rfcID, null, objs);
	}

	public void BroadcastToLAN(int port, string rfcName, params object[] objs)
	{
		BroadcastToLAN(port, 0, rfcName, objs);
	}

	public void Remove(string rfcName)
	{
		RemoveSavedRFC(uid, 0, rfcName);
	}

	public void Remove(byte rfcID)
	{
		RemoveSavedRFC(uid, rfcID, null);
	}

	private static uint GetUID(uint objID, byte rfcID)
	{
		return (objID << 8) | rfcID;
	}

	public static void DecodeUID(uint uid, out uint objID, out byte rfcID)
	{
		rfcID = (byte)(uid & 0xFF);
		objID = uid >> 8;
	}

	private void SendRFC(byte rfcID, string rfcName, Target target, bool reliable, params object[] objs)
	{
		bool flag = false;
		if (target == Target.Broadcast)
		{
			if (TNManager.isConnected)
			{
				BinaryWriter binaryWriter = TNManager.BeginSend(Packet.Broadcast);
				binaryWriter.Write(GetUID(uid, rfcID));
				if (rfcID == 0)
				{
					binaryWriter.Write(rfcName);
				}
				binaryWriter.WriteArray(objs);
				TNManager.EndSend(reliable);
			}
			else
			{
				flag = true;
			}
		}
		else if (target == Target.Host && TNManager.isHosting)
		{
			flag = true;
		}
		else if (TNManager.isInChannel)
		{
			if (!reliable)
			{
				switch (target)
				{
				case Target.All:
					target = Target.Others;
					flag = true;
					break;
				case Target.AllSaved:
					target = Target.OthersSaved;
					flag = true;
					break;
				}
			}
			byte packetID = (byte)(38 + target);
			BinaryWriter binaryWriter2 = TNManager.BeginSend(packetID);
			binaryWriter2.Write(GetUID(uid, rfcID));
			if (rfcID == 0)
			{
				binaryWriter2.Write(rfcName);
			}
			binaryWriter2.WriteArray(objs);
			TNManager.EndSend(reliable);
		}
		else if (!TNManager.isConnected && (target == Target.All || target == Target.AllSaved))
		{
			flag = true;
		}
		if (flag)
		{
			if (rfcID != 0)
			{
				Execute(rfcID, objs);
			}
			else
			{
				Execute(rfcName, objs);
			}
		}
	}

	private void SendRFC(byte rfcID, string rfcName, Player target, bool reliable, params object[] objs)
	{
		if (TNManager.isConnected)
		{
			BinaryWriter binaryWriter = TNManager.BeginSend(Packet.ForwardToPlayer);
			binaryWriter.Write(target.id);
			binaryWriter.Write(GetUID(uid, rfcID));
			if (rfcID == 0)
			{
				binaryWriter.Write(rfcName);
			}
			binaryWriter.WriteArray(objs);
			TNManager.EndSend(reliable);
		}
		else if (target == TNManager.player)
		{
			if (rfcID != 0)
			{
				Execute(rfcID, objs);
			}
			else
			{
				Execute(rfcName, objs);
			}
		}
	}

	private void BroadcastToLAN(int port, byte rfcID, string rfcName, params object[] objs)
	{
		BinaryWriter binaryWriter = TNManager.BeginSend(Packet.ForwardToAll);
		binaryWriter.Write(GetUID(uid, rfcID));
		if (rfcID == 0)
		{
			binaryWriter.Write(rfcName);
		}
		binaryWriter.WriteArray(objs);
		TNManager.EndSend(port);
	}

	private static void RemoveSavedRFC(uint objID, byte rfcID, string funcName)
	{
		if (TNManager.isInChannel)
		{
			BinaryWriter binaryWriter = TNManager.BeginSend(Packet.RequestRemoveRFC);
			binaryWriter.Write(GetUID(objID, rfcID));
			if (rfcID == 0)
			{
				binaryWriter.Write(funcName);
			}
			TNManager.EndSend();
		}
	}
}
