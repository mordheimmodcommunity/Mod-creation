using System.IO;

public class CombatLogger
{
	public enum LogMessage
	{
		ROUT_SUCCESS,
		ROUT_FAIL,
		MORAL_BELOW,
		TURN_START,
		TURN_END,
		ROUND_START,
		PERFORM_SKILL,
		PERFORM_SKILL_TARGETS,
		UNIT_ROLL_SUCCESS,
		UNIT_ROLL_FAIL,
		ROLL_SUCCESS,
		ROLL_FAIL,
		CURSE_TARGET,
		DAMAGE,
		DAMAGE_INFLICT,
		DAMAGE_CRIT_INFLICT,
		UNIT_STATUS,
		UNIT_OUT_OF_ACTION,
		LOSE_IDOL,
		GAIN_IDOL,
		STUNNED_HIT,
		HEALING,
		HEALING_RECEIVED
	}

	private const string file = "combat.log";

	public ulong Crc
	{
		get;
		private set;
	}

	public CombatLogger()
	{
		Crc = 0uL;
		using (StreamWriter streamWriter = File.CreateText("combat.log"))
		{
			streamWriter.WriteLine("Welcome To Mordheim V1.4.4.4");
		}
	}

	public void AddLog(Unit unit, LogMessage message, params string[] parms)
	{
		AddLog(PandoraSingleton<MissionManager>.Instance.GetUnitController(unit), message, parms);
	}

	public void AddLog(UnitController unitCtrlr, LogMessage message, params string[] parms)
	{
		if (unitCtrlr.IsImprintVisible())
		{
			AddLog(message, parms);
		}
	}

	public void AddLog(LogMessage message, params string[] parms)
	{
		string stringById = PandoraSingleton<LocalizationManager>.Instance.GetStringById("log_" + message.ToLowerString(), parms);
		PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.COMBAT_LOG, stringById);
		Write(string.Format(GetLogFileString(message), parms));
		PandoraDebug.LogInfo(stringById, "COMBAT LOG");
	}

	private void Write(string log)
	{
		using (StreamWriter streamWriter = File.AppendText("combat.log"))
		{
			streamWriter.WriteLine(log);
		}
	}

	private string GetLogFileString(LogMessage msg)
	{
		switch (msg)
		{
		case LogMessage.ROUT_SUCCESS:
			return "Rout test successful";
		case LogMessage.ROUT_FAIL:
			return "Rout test failed";
		case LogMessage.MORAL_BELOW:
			return "Warband's morale {0} / {1} is below {2}%";
		case LogMessage.TURN_START:
			return "{0}'s turn start";
		case LogMessage.TURN_END:
			return "{0}'s turn end";
		case LogMessage.ROUND_START:
			return "Round {0}";
		case LogMessage.PERFORM_SKILL:
			return "{0} is performing a {1}";
		case LogMessage.PERFORM_SKILL_TARGETS:
			return "{0} is performing {1} and targeting {2}";
		case LogMessage.UNIT_ROLL_SUCCESS:
			return "{0}: Roll {1} success : {2} target {3}";
		case LogMessage.UNIT_ROLL_FAIL:
			return "{0}: Roll {1} fail : {2} target {3}";
		case LogMessage.ROLL_SUCCESS:
			return "Roll {0} success : {1} target {2}";
		case LogMessage.ROLL_FAIL:
			return "Roll {0} fail : {1} target {2}";
		case LogMessage.CURSE_TARGET:
			return "Curse {0} applied on {1}";
		case LogMessage.DAMAGE:
			return "{0} received {1} damages";
		case LogMessage.DAMAGE_INFLICT:
			return "{0} inflicted {1} damages to {2}";
		case LogMessage.DAMAGE_CRIT_INFLICT:
			return "{0} inflicted {1} critical damages to {2}";
		case LogMessage.UNIT_STATUS:
			return "{0} Status is now {1}";
		case LogMessage.UNIT_OUT_OF_ACTION:
			return "{0} is now Out of Action -{1} morale";
		case LogMessage.LOSE_IDOL:
			return "Warband {0} has lost its idol -{1} morale";
		case LogMessage.GAIN_IDOL:
			return "Warband {0} recovered its idol +{1} morale";
		case LogMessage.STUNNED_HIT:
			return "{0} is stunned. Automatic hit.";
		case LogMessage.HEALING:
			return "{0} recovered {1} wounds";
		case LogMessage.HEALING_RECEIVED:
			return "{0} restored {1} wounds to {2}";
		default:
			return string.Empty;
		}
	}
}
