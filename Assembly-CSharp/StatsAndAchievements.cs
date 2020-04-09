using Steamworks;
using UnityEngine;

internal class StatsAndAchievements
{
	private enum Achievement
	{
		ACH_WIN_ONE_GAME,
		ACH_WIN_100_GAMES,
		ACH_HEAVY_FIRE,
		ACH_TRAVEL_FAR_ACCUM,
		ACH_TRAVEL_FAR_SINGLE
	}

	private class Achievement_t
	{
		public Achievement m_eAchievementID;

		public string m_rgchName;

		public string m_rgchDescription;

		public bool m_bAchieved;

		public int m_iIconImage;

		public Achievement_t(Achievement achievement, string name, string desc, bool achieved, int icon)
		{
			m_eAchievementID = achievement;
			m_rgchName = name;
			m_rgchDescription = desc;
			m_bAchieved = achieved;
			m_iIconImage = icon;
		}
	}

	private Achievement_t[] m_Achievements = new Achievement_t[4]
	{
		new Achievement_t(Achievement.ACH_WIN_ONE_GAME, "Winner", string.Empty, achieved: false, 0),
		new Achievement_t(Achievement.ACH_WIN_100_GAMES, "Champion", string.Empty, achieved: false, 0),
		new Achievement_t(Achievement.ACH_TRAVEL_FAR_ACCUM, "Interstellar", string.Empty, achieved: false, 0),
		new Achievement_t(Achievement.ACH_TRAVEL_FAR_SINGLE, "Orbiter", string.Empty, achieved: false, 0)
	};

	private static StatsAndAchievements m_instance;

	private CGameID m_GameID;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	private float m_flGameFeetTraveled;

	private float m_ulTickCountGameStart;

	private double m_flGameDurationSeconds;

	private int m_nTotalGamesPlayed;

	private int m_nTotalNumWins;

	private int m_nTotalNumLosses;

	private float m_flTotalFeetTraveled;

	private float m_flMaxFeetTraveled;

	private float m_flAverageSpeed;

	private void OnEnable()
	{
		if (m_instance == null)
		{
			m_instance = this;
		}
		m_GameID = new CGameID(SteamUtils.GetAppID());
		new Callback<UserStatsReceived_t>(OnUserStatsReceived);
		new Callback<UserStatsStored_t>(OnUserStatsStored);
		new Callback<UserAchievementStored_t>(OnAchievementStored);
	}

	private void FixedUpdate()
	{
		if (!m_bRequestedStats)
		{
			m_bRequestedStats = true;
		}
		else if (m_bStatsValid)
		{
			Achievement_t[] achievements = m_Achievements;
			foreach (Achievement_t achievement in achievements)
			{
				EvaluateAchievement(achievement);
			}
			StoreStatsIfNecessary();
		}
	}

	public void AddDistanceTraveled(float flDistance)
	{
		m_flGameFeetTraveled += flDistance / 72f / 12f;
	}

	public void OnGameStateChange(EClientGameState eNewState)
	{
		if (m_bStatsValid)
		{
			switch (eNewState)
			{
			default:
				return;
			case EClientGameState.k_EClientGameStartServer:
			case EClientGameState.k_EClientGameWaitingForPlayers:
			case EClientGameState.k_EClientGameMenu:
			case EClientGameState.k_EClientGameQuitMenu:
			case EClientGameState.k_EClientGameExiting:
			case EClientGameState.k_EClientGameInstructions:
			case EClientGameState.k_EClientGameConnecting:
			case EClientGameState.k_EClientGameConnectionFailure:
			case EClientGameState.k_EClientStatsAchievements:
				return;
			case EClientGameState.k_EClientFindInternetServers:
				return;
			case EClientGameState.k_EClientGameActive:
				m_flGameFeetTraveled = 0f;
				m_ulTickCountGameStart = Time.time;
				return;
			case EClientGameState.k_EClientGameWinner:
				m_nTotalNumWins++;
				m_nTotalNumLosses++;
				break;
			case EClientGameState.k_EClientGameDraw:
				break;
			}
			m_nTotalGamesPlayed++;
			m_flTotalFeetTraveled += m_flGameFeetTraveled;
			if (m_flGameFeetTraveled > m_flMaxFeetTraveled)
			{
				m_flMaxFeetTraveled = m_flGameFeetTraveled;
			}
			m_flGameDurationSeconds = Time.time - m_ulTickCountGameStart;
			m_bStoreStats = true;
		}
	}

	private void EvaluateAchievement(Achievement_t achievement)
	{
		if (achievement.m_bAchieved)
		{
			return;
		}
		switch (achievement.m_eAchievementID)
		{
		case Achievement.ACH_HEAVY_FIRE:
			break;
		case Achievement.ACH_WIN_ONE_GAME:
			if (m_nTotalNumWins != 0)
			{
				UnlockAchievement(achievement);
			}
			break;
		case Achievement.ACH_WIN_100_GAMES:
			if (m_nTotalNumWins >= 100)
			{
				UnlockAchievement(achievement);
			}
			break;
		case Achievement.ACH_TRAVEL_FAR_ACCUM:
			if (m_flTotalFeetTraveled >= 5280f)
			{
				UnlockAchievement(achievement);
			}
			break;
		case Achievement.ACH_TRAVEL_FAR_SINGLE:
			if (m_flGameFeetTraveled > 500f)
			{
				UnlockAchievement(achievement);
			}
			break;
		}
	}

	private void UnlockAchievement(Achievement_t achievement)
	{
		achievement.m_bAchieved = true;
		achievement.m_iIconImage = 0;
		SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());
		m_bStoreStats = true;
	}

	private void StoreStatsIfNecessary()
	{
		if (m_bStoreStats)
		{
			SteamUserStats.SetStat("NumGames", m_nTotalGamesPlayed);
			SteamUserStats.SetStat("NumWins", m_nTotalNumWins);
			SteamUserStats.SetStat("NumLosses", m_nTotalNumLosses);
			SteamUserStats.SetStat("FeetTraveled", m_flTotalFeetTraveled);
			SteamUserStats.SetStat("MaxFeetTraveled", m_flMaxFeetTraveled);
			SteamUserStats.UpdateAvgRateStat("AverageSpeed", m_flGameFeetTraveled, m_flGameDurationSeconds);
			SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);
			bool flag = SteamUserStats.StoreStats();
			m_bStoreStats = !flag;
		}
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if ((ulong)m_GameID != pCallback.m_nGameID)
		{
			return;
		}
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			Debug.Log("Received stats and achievements from Steam\n");
			m_bStatsValid = true;
			Achievement_t[] achievements = m_Achievements;
			foreach (Achievement_t achievement_t in achievements)
			{
				SteamUserStats.GetAchievement(achievement_t.m_eAchievementID.ToString(), out achievement_t.m_bAchieved);
				achievement_t.m_rgchName = SteamUserStats.GetAchievementDisplayAttribute(achievement_t.m_eAchievementID.ToString(), "name");
				achievement_t.m_rgchDescription = SteamUserStats.GetAchievementDisplayAttribute(achievement_t.m_eAchievementID.ToString(), "desc");
			}
			SteamUserStats.GetStat("NumGames", out m_nTotalGamesPlayed);
			SteamUserStats.GetStat("NumWins", out m_nTotalNumWins);
			SteamUserStats.GetStat("NumLosses", out m_nTotalNumLosses);
			SteamUserStats.GetStat("FeetTraveled", out m_flTotalFeetTraveled);
			SteamUserStats.GetStat("MaxFeetTraveled", out m_flMaxFeetTraveled);
			SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);
		}
		else
		{
			Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
		}
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("StoreStats - success");
			}
			else if (pCallback.m_eResult == EResult.k_EResultInvalidParam)
			{
				Debug.Log("StoreStats - some failed to validate");
				UserStatsReceived_t pCallback2 = default(UserStatsReceived_t);
				pCallback2.m_eResult = EResult.k_EResultOK;
				pCallback2.m_nGameID = (ulong)m_GameID;
				OnUserStatsReceived(pCallback2);
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
			}
			else
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
			}
		}
	}

	public void Render()
	{
		GUILayout.Label("m_ulTickCountGameStart: " + m_ulTickCountGameStart);
		GUILayout.Label("m_flGameDurationSeconds: " + m_flGameDurationSeconds);
		GUILayout.Label("m_flGameFeetTraveled: " + m_flGameFeetTraveled);
		GUILayout.Space(10f);
		GUILayout.Label("NumGames: " + m_nTotalGamesPlayed);
		GUILayout.Label("NumWins: " + m_nTotalNumWins);
		GUILayout.Label("NumLosses: " + m_nTotalNumLosses);
		GUILayout.Label("FeetTraveled: " + m_flTotalFeetTraveled);
		GUILayout.Label("MaxFeetTraveled: " + m_flMaxFeetTraveled);
		GUILayout.Label("AverageSpeed: " + m_flAverageSpeed);
		GUILayout.BeginArea(new Rect(Screen.width - 300, 0f, 300f, 800f));
		Achievement_t[] achievements = m_Achievements;
		foreach (Achievement_t achievement_t in achievements)
		{
			GUILayout.Label(achievement_t.m_eAchievementID.ToString());
			GUILayout.Label(achievement_t.m_rgchName + " - " + achievement_t.m_rgchDescription);
			GUILayout.Label("Achieved: " + achievement_t.m_bAchieved);
			GUILayout.Space(20f);
		}
		if (GUILayout.Button("RESET STATS AND ACHIEVEMENTS"))
		{
			SteamUserStats.ResetAllStats(bAchievementsToo: true);
			SteamUserStats.RequestCurrentStats();
			OnGameStateChange(EClientGameState.k_EClientGameMenu);
		}
		GUILayout.EndArea();
	}
}
