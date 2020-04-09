using System;
using System.IO;
using System.Net;
using System.Reflection;
using TNet;
using UnityEngine;

[AddComponentMenu("TNet/Network Manager")]
public class TNManager : MonoBehaviour
{
	public static bool rebuildMethodList = true;

	private static List<CachedFunc> mRCCs = new List<CachedFunc>();

	private static Player mPlayer = new Player("Guest");

	private static List<Player> mPlayers;

	private static TNManager mInstance;

	private static int mObjectOwner = 1;

	public GameObject[] objects;

	private GameClient mClient = new GameClient();

	public static GameClient client => (!(mInstance != null)) ? null : mInstance.mClient;

	public static bool isConnected => mInstance != null && mInstance.mClient.isConnected;

	public static bool isTryingToConnect => mInstance != null && mInstance.mClient.isTryingToConnect;

	public static bool isHosting => mInstance == null || mInstance.mClient.isHosting;

	public static bool isInChannel => mInstance != null && mInstance.mClient.isConnected && mInstance.mClient.isInChannel;

	public static bool isActive
	{
		get
		{
			return mInstance != null && mInstance.mClient.isActive;
		}
		set
		{
			if (mInstance != null)
			{
				mInstance.mClient.isActive = value;
			}
		}
	}

	public static bool noDelay
	{
		get
		{
			return mInstance != null && mInstance.mClient.noDelay;
		}
		set
		{
			if (mInstance != null)
			{
				mInstance.mClient.noDelay = value;
			}
		}
	}

	public static int ping => (mInstance != null) ? mInstance.mClient.ping : 0;

	public static bool canUseUDP => mInstance != null && mInstance.mClient.canUseUDP;

	public static int listeningPort => (mInstance != null) ? mInstance.mClient.listeningPort : 0;

	public static int objectOwnerID => mObjectOwner;

	public static bool isThisMyObject => mObjectOwner == playerID;

	public static IPEndPoint packetSource => (!(mInstance != null)) ? null : mInstance.mClient.packetSource;

	public static string channelData
	{
		get
		{
			return (!(mInstance != null)) ? string.Empty : mInstance.mClient.channelData;
		}
		set
		{
			if (mInstance != null)
			{
				mInstance.mClient.channelData = value;
			}
		}
	}

	public static int channelID => isConnected ? mInstance.mClient.channelID : 0;

	public static int hostID => (!isConnected) ? mPlayer.id : mInstance.mClient.hostID;

	public static int playerID => (!isConnected) ? mPlayer.id : mInstance.mClient.playerID;

	public static string playerName
	{
		get
		{
			return (!isConnected) ? mPlayer.name : mInstance.mClient.playerName;
		}
		set
		{
			if (playerName != value)
			{
				mPlayer.name = value;
				if (isConnected)
				{
					mInstance.mClient.playerName = value;
				}
			}
		}
	}

	public static object playerData
	{
		get
		{
			return (!isConnected) ? mPlayer.data : mInstance.mClient.playerData;
		}
		set
		{
			mPlayer.data = value;
			if (isConnected)
			{
				mInstance.mClient.playerData = value;
			}
		}
	}

	public static DataNode playerDataNode
	{
		get
		{
			DataNode dataNode = playerData as DataNode;
			if (dataNode == null)
			{
				dataNode = (DataNode)(playerData = new DataNode("Version", 11));
			}
			return dataNode;
		}
	}

	public static List<Player> players
	{
		get
		{
			if (isConnected)
			{
				return mInstance.mClient.players;
			}
			if (mPlayers == null)
			{
				mPlayers = new List<Player>();
			}
			return mPlayers;
		}
	}

	public static Player player => (!isConnected) ? mPlayer : mInstance.mClient.player;

	private static TNManager instance
	{
		get
		{
			if (mInstance == null)
			{
				GameObject gameObject = new GameObject("Network Manager");
				mInstance = gameObject.AddComponent<TNManager>();
			}
			return mInstance;
		}
	}

	public static void SyncPlayerData()
	{
		if (isConnected)
		{
			mInstance.mClient.SyncPlayerData();
		}
	}

	public static Player GetPlayer(int id)
	{
		if (isConnected)
		{
			return mInstance.mClient.GetPlayer(id);
		}
		if (id == mPlayer.id)
		{
			return mPlayer;
		}
		return null;
	}

	public static void SetPacketHandler(byte packetID, GameClient.OnPacket callback)
	{
		instance.mClient.packetHandlers[packetID] = callback;
	}

	public static void SetPacketHandler(Packet packet, GameClient.OnPacket callback)
	{
		instance.mClient.packetHandlers[(byte)packet] = callback;
	}

	public static bool StartUDP(int udpPort)
	{
		return instance.mClient.StartUDP(udpPort);
	}

	public static void StopUDP()
	{
		if (mInstance != null)
		{
			mInstance.mClient.StopUDP();
		}
	}

	public static void Ping(IPEndPoint udpEndPoint, GameClient.OnPing callback)
	{
		instance.mClient.Ping(udpEndPoint, callback);
	}

	public static void Connect(IPEndPoint externalIP, IPEndPoint internalIP)
	{
		instance.mClient.Disconnect();
		instance.mClient.playerName = mPlayer.name;
		instance.mClient.playerData = mPlayer.data;
		instance.mClient.Connect(externalIP, internalIP);
	}

	public static void Connect(string address, int port)
	{
		IPEndPoint iPEndPoint = Tools.ResolveEndPoint(address, port);
		if (iPEndPoint == null)
		{
			instance.OnConnect(success: false, "Unable to resolve [" + address + "]");
			return;
		}
		instance.mClient.playerName = mPlayer.name;
		instance.mClient.playerData = mPlayer.data;
		instance.mClient.Connect(iPEndPoint, null);
	}

	public static void Connect(string address)
	{
		string[] array = address.Split(new char[1]
		{
			':'
		});
		int result = 5127;
		if (array.Length > 1)
		{
			int.TryParse(array[1], out result);
		}
		Connect(array[0], result);
	}

	public static void Disconnect()
	{
		if (mInstance != null)
		{
			mInstance.mClient.Disconnect();
		}
	}

	public static void JoinChannel(int channelID, string levelName)
	{
		if (mInstance != null)
		{
			mInstance.mClient.JoinChannel(channelID, levelName, persistent: false, 65535, null);
		}
		else
		{
			Application.LoadLevel(levelName);
		}
	}

	public static void JoinChannel(int channelID, string levelName, bool persistent, int playerLimit, string password)
	{
		if (mInstance != null)
		{
			mInstance.mClient.JoinChannel(channelID, levelName, persistent, playerLimit, password);
		}
		else
		{
			Application.LoadLevel(levelName);
		}
	}

	public static void JoinRandomChannel(string levelName, bool persistent, int playerLimit, string password)
	{
		if (mInstance != null)
		{
			mInstance.mClient.JoinChannel(-2, levelName, persistent, playerLimit, password);
		}
	}

	public static void CreateChannel(string levelName, bool persistent, int playerLimit, string password)
	{
		if (mInstance != null)
		{
			mInstance.mClient.JoinChannel(-1, levelName, persistent, playerLimit, password);
		}
		else
		{
			Application.LoadLevel(levelName);
		}
	}

	public static void CloseChannel()
	{
		if (mInstance != null)
		{
			mInstance.mClient.CloseChannel();
		}
	}

	public static void LeaveChannel()
	{
		if (mInstance != null)
		{
			mInstance.mClient.LeaveChannel();
		}
	}

	public static void SetPlayerLimit(int max)
	{
		if (mInstance != null)
		{
			mInstance.mClient.SetPlayerLimit(max);
		}
	}

	public static void LoadLevel(string levelName)
	{
		if (isConnected)
		{
			mInstance.mClient.LoadLevel(levelName);
		}
		else
		{
			Application.LoadLevel(levelName);
		}
	}

	public static void SaveFile(string filename, byte[] data)
	{
		if (isConnected)
		{
			mInstance.mClient.SaveFile(filename, data);
		}
		else
		{
			try
			{
				Tools.WriteFile(filename, data);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message + " (" + filename + ")");
			}
		}
	}

	public static void LoadFile(string filename, GameClient.OnLoadFile callback)
	{
		if (callback != null)
		{
			if (isConnected)
			{
				mInstance.mClient.LoadFile(filename, callback);
			}
			else
			{
				callback(filename, Tools.ReadFile(filename));
			}
		}
	}

	public static void SetHost(Player player)
	{
		if (mInstance != null)
		{
			mInstance.mClient.SetHost(player);
		}
	}

	public static void SetTimeout(int seconds)
	{
		if (mInstance != null)
		{
			mInstance.mClient.SetTimeout(seconds);
		}
	}

	public static void Create(GameObject go, bool persistent = true)
	{
		CreateEx(0, persistent, go);
	}

	public static void Create(string path, bool persistent = true)
	{
		CreateEx(0, persistent, path);
	}

	public static void Create(GameObject go, Vector3 pos, Quaternion rot, bool persistent = true)
	{
		CreateEx(1, persistent, go, pos, rot);
	}

	public static void Create(string path, Vector3 pos, Quaternion rot, bool persistent = true)
	{
		CreateEx(1, persistent, path, pos, rot);
	}

	public static void Create(GameObject go, Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel, bool persistent = true)
	{
		CreateEx(2, persistent, go, pos, rot, vel, angVel);
	}

	public static void Create(string path, Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel, bool persistent = true)
	{
		CreateEx(2, persistent, path, pos, rot, vel, angVel);
	}

	public static void CreateEx(int rccID, bool persistent, GameObject go, params object[] objs)
	{
		if (!(go != null))
		{
			return;
		}
		int num = IndexOf(go);
		if (isConnected)
		{
			if (num != -1)
			{
				BinaryWriter binaryWriter = mInstance.mClient.BeginSend(Packet.RequestCreate);
				binaryWriter.Write((ushort)num);
				binaryWriter.Write(GetFlag(go, persistent));
				binaryWriter.Write((byte)rccID);
				binaryWriter.WriteArray(objs);
				EndSend();
				return;
			}
			Debug.LogError("\"" + go.name + "\" has not been added to TNManager's list of objects, so it cannot be instantiated.\nConsider placing it into the Resources folder and passing its name instead.", go);
		}
		objs = BinaryExtensions.CombineArrays(go, objs);
		UnityTools.ExecuteAll(GetRCCs(), (byte)rccID, objs);
		UnityTools.Clear(objs);
	}

	public static void CreateEx(int rccID, bool persistent, string path, params object[] objs)
	{
		GameObject gameObject = LoadGameObject(path);
		if (gameObject != null)
		{
			if (isConnected)
			{
				BinaryWriter binaryWriter = mInstance.mClient.BeginSend(Packet.RequestCreate);
				byte flag = GetFlag(gameObject, persistent);
				binaryWriter.Write(ushort.MaxValue);
				binaryWriter.Write(flag);
				binaryWriter.Write(path);
				binaryWriter.Write((byte)rccID);
				binaryWriter.WriteArray(objs);
				EndSend();
			}
			else
			{
				objs = BinaryExtensions.CombineArrays(gameObject, objs);
				UnityTools.ExecuteAll(GetRCCs(), (byte)rccID, objs);
				UnityTools.Clear(objs);
			}
		}
		else
		{
			Debug.LogError("Unable to load " + path);
		}
	}

	public static List<CachedFunc> GetRCCs()
	{
		if (rebuildMethodList)
		{
			rebuildMethodList = false;
			if (mInstance != null)
			{
				MonoBehaviour[] componentsInChildren = mInstance.GetComponentsInChildren<MonoBehaviour>();
				int i = 0;
				for (int num = componentsInChildren.Length; i < num; i++)
				{
					MonoBehaviour monoBehaviour = componentsInChildren[i];
					AddRCCs(monoBehaviour, monoBehaviour.GetType());
				}
			}
			else
			{
				AddRCCs<TNManager>();
			}
		}
		return mRCCs;
	}

	public static void AddRCCs(MonoBehaviour mb)
	{
		AddRCCs(mb, mb.GetType());
	}

	public static void AddRCCs<T>()
	{
		AddRCCs(null, typeof(T));
	}

	private static void AddRCCs(object obj, Type type)
	{
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < methods.Length; i++)
		{
			if (!methods[i].IsDefined(typeof(RCC), inherit: true))
			{
				continue;
			}
			RCC rCC = (RCC)methods[i].GetCustomAttributes(typeof(RCC), inherit: true)[0];
			for (int j = 0; j < mRCCs.size; j++)
			{
				CachedFunc cachedFunc = mRCCs[j];
				if (cachedFunc.id == rCC.id)
				{
					cachedFunc.obj = obj;
					cachedFunc.func = methods[i];
					return;
				}
			}
			CachedFunc item = default(CachedFunc);
			item.obj = obj;
			item.func = methods[i];
			item.id = rCC.id;
			mRCCs.Add(item);
		}
	}

	private static void RemoveRCC(int rccID)
	{
		int num = 0;
		while (true)
		{
			if (num < mRCCs.size)
			{
				CachedFunc cachedFunc = mRCCs[num];
				if (cachedFunc.id == rccID)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		mRCCs.RemoveAt(num);
	}

	private static void RemoveRCCs<T>()
	{
		MethodInfo[] methods = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < methods.Length; i++)
		{
			if (methods[i].IsDefined(typeof(RCC), inherit: true))
			{
				RCC rCC = (RCC)methods[i].GetCustomAttributes(typeof(RCC), inherit: true)[0];
				RemoveRCC(rCC.id);
			}
		}
	}

	[RCC(0)]
	private static GameObject OnCreate0(GameObject go)
	{
		return UnityEngine.Object.Instantiate(go);
	}

	[RCC(1)]
	private static GameObject OnCreate1(GameObject go, Vector3 pos, Quaternion rot)
	{
		return UnityEngine.Object.Instantiate(go, pos, rot) as GameObject;
	}

	[RCC(2)]
	private static GameObject OnCreate2(GameObject go, Vector3 pos, Quaternion rot, Vector3 velocity, Vector3 angularVelocity)
	{
		return UnityTools.Instantiate(go, pos, rot, velocity, angularVelocity);
	}

	public static void Destroy(GameObject go)
	{
		if (isConnected)
		{
			TNObject component = go.GetComponent<TNObject>();
			if (component != null)
			{
				BinaryWriter binaryWriter = mInstance.mClient.BeginSend(Packet.RequestDestroy);
				binaryWriter.Write(component.uid);
				mInstance.mClient.EndSend();
				return;
			}
		}
		UnityEngine.Object.Destroy(go);
	}

	public static BinaryWriter BeginSend(Packet type)
	{
		return mInstance.mClient.BeginSend(type);
	}

	public static BinaryWriter BeginSend(byte packetID)
	{
		return mInstance.mClient.BeginSend(packetID);
	}

	public static void EndSend()
	{
		mInstance.mClient.EndSend(reliable: true);
	}

	public static void EndSend(bool reliable)
	{
		mInstance.mClient.EndSend(reliable);
	}

	public static void EndSend(int port)
	{
		mInstance.mClient.EndSend(port);
	}

	public static void EndSend(IPEndPoint target)
	{
		mInstance.mClient.EndSend(target);
	}

	private void Awake()
	{
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		mInstance = this;
		rebuildMethodList = true;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		GameClient gameClient = mClient;
		gameClient.onError = (GameClient.OnError)Delegate.Combine(gameClient.onError, new GameClient.OnError(OnError));
		GameClient gameClient2 = mClient;
		gameClient2.onConnect = (GameClient.OnConnect)Delegate.Combine(gameClient2.onConnect, new GameClient.OnConnect(OnConnect));
		GameClient gameClient3 = mClient;
		gameClient3.onDisconnect = (GameClient.OnDisconnect)Delegate.Combine(gameClient3.onDisconnect, new GameClient.OnDisconnect(OnDisconnect));
		GameClient gameClient4 = mClient;
		gameClient4.onJoinChannel = (GameClient.OnJoinChannel)Delegate.Combine(gameClient4.onJoinChannel, new GameClient.OnJoinChannel(OnJoinChannel));
		GameClient gameClient5 = mClient;
		gameClient5.onLeftChannel = (GameClient.OnLeftChannel)Delegate.Combine(gameClient5.onLeftChannel, new GameClient.OnLeftChannel(OnLeftChannel));
		GameClient gameClient6 = mClient;
		gameClient6.onLoadLevel = (GameClient.OnLoadLevel)Delegate.Combine(gameClient6.onLoadLevel, new GameClient.OnLoadLevel(OnLoadLevel));
		GameClient gameClient7 = mClient;
		gameClient7.onPlayerJoined = (GameClient.OnPlayerJoined)Delegate.Combine(gameClient7.onPlayerJoined, new GameClient.OnPlayerJoined(OnPlayerJoined));
		GameClient gameClient8 = mClient;
		gameClient8.onPlayerLeft = (GameClient.OnPlayerLeft)Delegate.Combine(gameClient8.onPlayerLeft, new GameClient.OnPlayerLeft(OnPlayerLeft));
		GameClient gameClient9 = mClient;
		gameClient9.onRenamePlayer = (GameClient.OnRenamePlayer)Delegate.Combine(gameClient9.onRenamePlayer, new GameClient.OnRenamePlayer(OnRenamePlayer));
		GameClient gameClient10 = mClient;
		gameClient10.onCreate = (GameClient.OnCreate)Delegate.Combine(gameClient10.onCreate, new GameClient.OnCreate(OnCreateObject));
		GameClient gameClient11 = mClient;
		gameClient11.onDestroy = (GameClient.OnDestroy)Delegate.Combine(gameClient11.onDestroy, new GameClient.OnDestroy(OnDestroyObject));
		GameClient gameClient12 = mClient;
		gameClient12.onForwardedPacket = (GameClient.OnForwardedPacket)Delegate.Combine(gameClient12.onForwardedPacket, new GameClient.OnForwardedPacket(OnForwardedPacket));
	}

	private void OnDestroy()
	{
		if (mInstance == this)
		{
			if (isConnected)
			{
				mClient.Disconnect();
			}
			mClient.StopUDP();
			mInstance = null;
		}
	}

	public static int IndexOf(GameObject go)
	{
		if (go != null && mInstance != null && mInstance.objects != null)
		{
			int i = 0;
			for (int num = mInstance.objects.Length; i < num; i++)
			{
				if (mInstance.objects[i] == go)
				{
					return i;
				}
			}
			Debug.LogError("The game object was not found in the TNManager's list of objects. Did you forget to add it?", go);
		}
		return -1;
	}

	private static GameObject LoadGameObject(string path)
	{
		GameObject gameObject = Resources.Load(path, typeof(GameObject)) as GameObject;
		if (gameObject == null)
		{
			Debug.LogError("Attempting to create a game object that can't be found in the Resources folder: " + path);
		}
		return gameObject;
	}

	private static byte GetFlag(GameObject go, bool persistent)
	{
		TNObject component = go.GetComponent<TNObject>();
		if (component == null)
		{
			return 0;
		}
		return (byte)(persistent ? 1 : 2);
	}

	private void OnCreateObject(int creator, int index, uint objectID, BinaryReader reader)
	{
		mObjectOwner = creator;
		GameObject gameObject = null;
		if (index == 65535)
		{
			gameObject = LoadGameObject(reader.ReadString());
		}
		else
		{
			if (index < 0 || index >= objects.Length)
			{
				Debug.LogError("Attempting to create an invalid object. Index: " + index);
				return;
			}
			gameObject = objects[index];
		}
		gameObject = CreateGameObject(gameObject, reader);
		if (gameObject != null && objectID != 0)
		{
			TNObject component = gameObject.GetComponent<TNObject>();
			if (component != null)
			{
				component.uid = objectID;
				component.Register();
			}
			else
			{
				Debug.LogWarning("The instantiated object has no TNObject component. Don't request an ObjectID when creating it.", gameObject);
			}
		}
	}

	private static GameObject CreateGameObject(GameObject prefab, BinaryReader reader)
	{
		if (prefab != null)
		{
			byte b = reader.ReadByte();
			if (b == 0)
			{
				return UnityEngine.Object.Instantiate(prefab);
			}
			object[] array = reader.ReadArray(prefab);
			UnityTools.ExecuteFirst(GetRCCs(), b, out object retVal, array);
			UnityTools.Clear(array);
			if (retVal == null)
			{
				Debug.LogError("Instantiating \"" + prefab.name + "\" returned null. Did you forget to return the game object from your RCC?");
			}
			return retVal as GameObject;
		}
		return null;
	}

	private void OnDestroyObject(uint objID)
	{
		TNObject tNObject = TNObject.Find(objID);
		if ((bool)tNObject)
		{
			UnityEngine.Object.Destroy(tNObject.gameObject);
		}
	}

	private void OnForwardedPacket(BinaryReader reader)
	{
		TNObject.DecodeUID(reader.ReadUInt32(), out uint objID, out byte rfcID);
		if (rfcID == 0)
		{
			TNObject.FindAndExecute(objID, reader.ReadString(), reader.ReadArray());
		}
		else
		{
			TNObject.FindAndExecute(objID, rfcID, reader.ReadArray());
		}
	}

	private void Update()
	{
		mClient.ProcessPackets();
	}

	private void OnError(string err)
	{
		UnityTools.Broadcast("OnNetworkError", err);
	}

	private void OnConnect(bool success, string message)
	{
		UnityTools.Broadcast("OnNetworkConnect", success, message);
	}

	private void OnDisconnect()
	{
		UnityTools.Broadcast("OnNetworkDisconnect");
	}

	private void OnJoinChannel(bool success, string message)
	{
		UnityTools.Broadcast("OnNetworkJoinChannel", success, message);
	}

	private void OnLeftChannel()
	{
		UnityTools.Broadcast("OnNetworkLeaveChannel");
	}

	private void OnLoadLevel(string levelName)
	{
		if (!string.IsNullOrEmpty(levelName))
		{
			Application.LoadLevel(levelName);
		}
	}

	private void OnPlayerJoined(Player p)
	{
		UnityTools.Broadcast("OnNetworkPlayerJoin", p);
	}

	private void OnPlayerLeft(Player p)
	{
		UnityTools.Broadcast("OnNetworkPlayerLeave", p);
	}

	private void OnRenamePlayer(Player p, string previous)
	{
		mPlayer.name = p.name;
		UnityTools.Broadcast("OnNetworkPlayerRenamed", p, previous);
	}
}
