using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Data
{
	public class UserDataStore_PlayerPrefs : UserDataStore
	{
		[SerializeField]
		private bool isEnabled = true;

		[SerializeField]
		private bool loadDataOnStart = true;

		[SerializeField]
		private string playerPrefsKeyPrefix = "RewiredSaveData";

		public UserDataStore_PlayerPrefs()
			: this()
		{
		}

		public override void Save()
		{
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				SaveAll();
			}
		}

		public override void SaveControllerData(int playerId, ControllerType controllerType, int controllerId)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				SaveControllerDataNow(playerId, controllerType, controllerId);
			}
		}

		public override void SaveControllerData(ControllerType controllerType, int controllerId)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				SaveControllerDataNow(controllerType, controllerId);
			}
		}

		public override void SavePlayerData(int playerId)
		{
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				SavePlayerDataNow(playerId);
			}
		}

		public override void SaveInputBehavior(int playerId, int behaviorId)
		{
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not save any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				SaveInputBehaviorNow(playerId, behaviorId);
			}
		}

		public override void Load()
		{
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				LoadAll();
			}
		}

		public override void LoadControllerData(int playerId, ControllerType controllerType, int controllerId)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				LoadControllerDataNow(playerId, controllerType, controllerId);
			}
		}

		public override void LoadControllerData(ControllerType controllerType, int controllerId)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				LoadControllerDataNow(controllerType, controllerId);
			}
		}

		public override void LoadPlayerData(int playerId)
		{
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				LoadPlayerDataNow(playerId);
			}
		}

		public override void LoadInputBehavior(int playerId, int behaviorId)
		{
			if (!isEnabled)
			{
				Debug.LogWarning("UserDataStore_PlayerPrefs is disabled and will not load any data.", (UnityEngine.Object)(object)this);
			}
			else
			{
				LoadInputBehaviorNow(playerId, behaviorId);
			}
		}

		protected override void OnInitialize()
		{
			if (loadDataOnStart)
			{
				Load();
			}
		}

		protected override void OnControllerConnected(ControllerStatusChangedEventArgs args)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			if (isEnabled && (int)args.get_controllerType() == 2)
			{
				LoadJoystickData(args.get_controllerId());
			}
		}

		protected override void OnControllerPreDiscconnect(ControllerStatusChangedEventArgs args)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			if (isEnabled && (int)args.get_controllerType() == 2)
			{
				SaveJoystickData(args.get_controllerId());
			}
		}

		protected override void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
		{
			if (isEnabled)
			{
			}
		}

		private void LoadAll()
		{
			IList<Player> allPlayers = ReInput.get_players().get_AllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				LoadPlayerDataNow(allPlayers[i]);
			}
			LoadAllJoystickCalibrationData();
		}

		private void LoadPlayerDataNow(int playerId)
		{
			LoadPlayerDataNow(ReInput.get_players().GetPlayer(playerId));
		}

		private void LoadPlayerDataNow(Player player)
		{
			if (player != null)
			{
				LoadInputBehaviors(player.get_id());
				LoadControllerMaps(player.get_id(), (ControllerType)0, 0);
				LoadControllerMaps(player.get_id(), (ControllerType)1, 0);
				foreach (Joystick joystick in player.controllers.get_Joysticks())
				{
					LoadControllerMaps(player.get_id(), (ControllerType)2, ((Controller)joystick).id);
				}
			}
		}

		private void LoadAllJoystickCalibrationData()
		{
			IList<Joystick> joysticks = ReInput.get_controllers().get_Joysticks();
			for (int i = 0; i < joysticks.Count; i++)
			{
				LoadJoystickCalibrationData(joysticks[i]);
			}
		}

		private void LoadJoystickCalibrationData(Joystick joystick)
		{
			if (joystick != null)
			{
				((ControllerWithAxes)joystick).ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick));
			}
		}

		private void LoadJoystickCalibrationData(int joystickId)
		{
			LoadJoystickCalibrationData(ReInput.get_controllers().GetJoystick(joystickId));
		}

		private void LoadJoystickData(int joystickId)
		{
			IList<Player> allPlayers = ReInput.get_players().get_AllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Player val = allPlayers[i];
				if (val.controllers.ContainsController((ControllerType)2, joystickId))
				{
					LoadControllerMaps(val.get_id(), (ControllerType)2, joystickId);
				}
			}
			LoadJoystickCalibrationData(joystickId);
		}

		private void LoadControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			LoadControllerMaps(playerId, controllerType, controllerId);
			LoadControllerDataNow(controllerType, controllerId);
		}

		private void LoadControllerDataNow(ControllerType controllerType, int controllerId)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			if ((int)controllerType == 2)
			{
				LoadJoystickCalibrationData(controllerId);
			}
		}

		private void LoadControllerMaps(int playerId, ControllerType controllerType, int controllerId)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			Player player = ReInput.get_players().GetPlayer(playerId);
			if (player == null)
			{
				return;
			}
			Controller controller = ReInput.get_controllers().GetController(controllerType, controllerId);
			if (controller != null)
			{
				List<string> allControllerMapsXml = GetAllControllerMapsXml(player, userAssignableMapsOnly: true, controllerType, controller);
				if (allControllerMapsXml.Count != 0)
				{
					player.controllers.maps.AddMapsFromXml(controllerType, controllerId, allControllerMapsXml);
				}
			}
		}

		private void LoadInputBehaviors(int playerId)
		{
			Player player = ReInput.get_players().GetPlayer(playerId);
			if (player != null)
			{
				IList<InputBehavior> inputBehaviors = ReInput.get_mapping().GetInputBehaviors(player.get_id());
				for (int i = 0; i < inputBehaviors.Count; i++)
				{
					LoadInputBehaviorNow(player, inputBehaviors[i]);
				}
			}
		}

		private void LoadInputBehaviorNow(int playerId, int behaviorId)
		{
			Player player = ReInput.get_players().GetPlayer(playerId);
			if (player != null)
			{
				InputBehavior inputBehavior = ReInput.get_mapping().GetInputBehavior(playerId, behaviorId);
				if (inputBehavior != null)
				{
					LoadInputBehaviorNow(player, inputBehavior);
				}
			}
		}

		private void LoadInputBehaviorNow(Player player, InputBehavior inputBehavior)
		{
			if (player != null && inputBehavior != null)
			{
				string inputBehaviorXml = GetInputBehaviorXml(player, inputBehavior.get_id());
				if (inputBehaviorXml != null && !(inputBehaviorXml == string.Empty))
				{
					inputBehavior.ImportXmlString(inputBehaviorXml);
				}
			}
		}

		private void SaveAll()
		{
			IList<Player> allPlayers = ReInput.get_players().get_AllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				SavePlayerDataNow(allPlayers[i]);
			}
			SaveAllJoystickCalibrationData();
			PlayerPrefs.Save();
		}

		private void SavePlayerDataNow(int playerId)
		{
			SavePlayerDataNow(ReInput.get_players().GetPlayer(playerId));
		}

		private void SavePlayerDataNow(Player player)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (player != null)
			{
				PlayerSaveData saveData = player.GetSaveData(true);
				SaveInputBehaviors(player, saveData);
				SaveControllerMaps(player, saveData);
			}
		}

		private void SaveAllJoystickCalibrationData()
		{
			IList<Joystick> joysticks = ReInput.get_controllers().get_Joysticks();
			for (int i = 0; i < joysticks.Count; i++)
			{
				SaveJoystickCalibrationData(joysticks[i]);
			}
		}

		private void SaveJoystickCalibrationData(int joystickId)
		{
			SaveJoystickCalibrationData(ReInput.get_controllers().GetJoystick(joystickId));
		}

		private void SaveJoystickCalibrationData(Joystick joystick)
		{
			if (joystick != null)
			{
				JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
				string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(calibrationMapSaveData);
				PlayerPrefs.SetString(joystickCalibrationMapPlayerPrefsKey, ((CalibrationMapSaveData)calibrationMapSaveData).get_map().ToXmlString());
			}
		}

		private void SaveJoystickData(int joystickId)
		{
			IList<Player> allPlayers = ReInput.get_players().get_AllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Player val = allPlayers[i];
				if (val.controllers.ContainsController((ControllerType)2, joystickId))
				{
					SaveControllerMaps(val.get_id(), (ControllerType)2, joystickId);
				}
			}
			SaveJoystickCalibrationData(joystickId);
		}

		private void SaveControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			SaveControllerMaps(playerId, controllerType, controllerId);
			SaveControllerDataNow(controllerType, controllerId);
		}

		private void SaveControllerDataNow(ControllerType controllerType, int controllerId)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			if ((int)controllerType == 2)
			{
				SaveJoystickCalibrationData(controllerId);
			}
		}

		private void SaveControllerMaps(Player player, PlayerSaveData playerSaveData)
		{
			foreach (ControllerMapSaveData allControllerMapSaveDatum in ((PlayerSaveData)(ref playerSaveData)).get_AllControllerMapSaveData())
			{
				string controllerMapPlayerPrefsKey = GetControllerMapPlayerPrefsKey(player, allControllerMapSaveDatum);
				PlayerPrefs.SetString(controllerMapPlayerPrefsKey, allControllerMapSaveDatum.get_map().ToXmlString());
			}
		}

		private void SaveControllerMaps(int playerId, ControllerType controllerType, int controllerId)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			Player player = ReInput.get_players().GetPlayer(playerId);
			if (player == null || !player.controllers.ContainsController(controllerType, controllerId))
			{
				return;
			}
			ControllerMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData(controllerType, controllerId, true);
			if (mapSaveData != null)
			{
				for (int i = 0; i < mapSaveData.Length; i++)
				{
					string controllerMapPlayerPrefsKey = GetControllerMapPlayerPrefsKey(player, mapSaveData[i]);
					PlayerPrefs.SetString(controllerMapPlayerPrefsKey, mapSaveData[i].get_map().ToXmlString());
				}
			}
		}

		private void SaveInputBehaviors(Player player, PlayerSaveData playerSaveData)
		{
			if (player != null)
			{
				InputBehavior[] inputBehaviors = ((PlayerSaveData)(ref playerSaveData)).get_inputBehaviors();
				for (int i = 0; i < inputBehaviors.Length; i++)
				{
					SaveInputBehaviorNow(player, inputBehaviors[i]);
				}
			}
		}

		private void SaveInputBehaviorNow(int playerId, int behaviorId)
		{
			Player player = ReInput.get_players().GetPlayer(playerId);
			if (player != null)
			{
				InputBehavior inputBehavior = ReInput.get_mapping().GetInputBehavior(playerId, behaviorId);
				if (inputBehavior != null)
				{
					SaveInputBehaviorNow(player, inputBehavior);
				}
			}
		}

		private void SaveInputBehaviorNow(Player player, InputBehavior inputBehavior)
		{
			if (player != null && inputBehavior != null)
			{
				string inputBehaviorPlayerPrefsKey = GetInputBehaviorPlayerPrefsKey(player, inputBehavior);
				PlayerPrefs.SetString(inputBehaviorPlayerPrefsKey, inputBehavior.ToXmlString());
			}
		}

		private string GetBasePlayerPrefsKey(Player player)
		{
			string str = playerPrefsKeyPrefix;
			return str + "|playerName=" + player.get_name();
		}

		private string GetControllerMapPlayerPrefsKey(Player player, ControllerMapSaveData saveData)
		{
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
			basePlayerPrefsKey += "|dataType=ControllerMap";
			basePlayerPrefsKey = basePlayerPrefsKey + "|controllerMapType=" + saveData.get_mapTypeString();
			string text = basePlayerPrefsKey;
			basePlayerPrefsKey = text + "|categoryId=" + saveData.get_map().get_categoryId() + "|layoutId=" + saveData.get_map().get_layoutId();
			basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareIdentifier=" + saveData.get_controllerHardwareIdentifier();
			if ((object)saveData.get_mapType() == typeof(JoystickMap))
			{
				basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareGuid=" + ((JoystickMapSaveData)saveData).get_joystickHardwareTypeGuid().ToString();
			}
			return basePlayerPrefsKey;
		}

		private string GetControllerMapXml(Player player, ControllerType controllerType, int categoryId, int layoutId, Controller controller)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Invalid comparison between Unknown and I4
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
			basePlayerPrefsKey += "|dataType=ControllerMap";
			basePlayerPrefsKey = basePlayerPrefsKey + "|controllerMapType=" + controller.get_mapTypeString();
			string text = basePlayerPrefsKey;
			basePlayerPrefsKey = text + "|categoryId=" + categoryId + "|layoutId=" + layoutId;
			basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareIdentifier=" + controller.get_hardwareIdentifier();
			if ((int)controllerType == 2)
			{
				Joystick val = (Joystick)(object)(Joystick)controller;
				basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareGuid=" + val.get_hardwareTypeGuid().ToString();
			}
			if (!PlayerPrefs.HasKey(basePlayerPrefsKey))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(basePlayerPrefsKey);
		}

		private List<string> GetAllControllerMapsXml(Player player, bool userAssignableMapsOnly, ControllerType controllerType, Controller controller)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			List<string> list = new List<string>();
			IList<InputMapCategory> mapCategories = ReInput.get_mapping().get_MapCategories();
			for (int i = 0; i < mapCategories.Count; i++)
			{
				InputMapCategory val = mapCategories[i];
				if (userAssignableMapsOnly && !((InputCategory)val).get_userAssignable())
				{
					continue;
				}
				IList<InputLayout> list2 = ReInput.get_mapping().MapLayouts(controllerType);
				for (int j = 0; j < list2.Count; j++)
				{
					InputLayout val2 = list2[j];
					string controllerMapXml = GetControllerMapXml(player, controllerType, ((InputCategory)val).get_id(), val2.get_id(), controller);
					if (!(controllerMapXml == string.Empty))
					{
						list.Add(controllerMapXml);
					}
				}
			}
			return list;
		}

		private string GetJoystickCalibrationMapPlayerPrefsKey(JoystickCalibrationMapSaveData saveData)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			string str = playerPrefsKeyPrefix;
			str += "|dataType=CalibrationMap";
			str = str + "|controllerType=" + ((Enum)((CalibrationMapSaveData)saveData).get_controllerType()).ToString();
			str = str + "|hardwareIdentifier=" + ((CalibrationMapSaveData)saveData).get_hardwareIdentifier();
			return str + "|hardwareGuid=" + saveData.get_joystickHardwareTypeGuid().ToString();
		}

		private string GetJoystickCalibrationMapXml(Joystick joystick)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			string str = playerPrefsKeyPrefix;
			str += "|dataType=CalibrationMap";
			str = str + "|controllerType=" + ((Enum)((Controller)joystick).get_type()).ToString();
			str = str + "|hardwareIdentifier=" + ((Controller)joystick).get_hardwareIdentifier();
			str = str + "|hardwareGuid=" + joystick.get_hardwareTypeGuid().ToString();
			if (!PlayerPrefs.HasKey(str))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(str);
		}

		private string GetInputBehaviorPlayerPrefsKey(Player player, InputBehavior saveData)
		{
			string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
			basePlayerPrefsKey += "|dataType=InputBehavior";
			return basePlayerPrefsKey + "|id=" + saveData.get_id();
		}

		private string GetInputBehaviorXml(Player player, int id)
		{
			string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
			basePlayerPrefsKey += "|dataType=InputBehavior";
			basePlayerPrefsKey = basePlayerPrefsKey + "|id=" + id;
			if (!PlayerPrefs.HasKey(basePlayerPrefsKey))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(basePlayerPrefsKey);
		}
	}
}
