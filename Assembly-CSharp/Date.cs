using System.Collections.Generic;
using System.Text;

public class Date
{
	public int CurrentDate
	{
		get;
		private set;
	}

	public int Day
	{
		get;
		private set;
	}

	public WeekDayId WeekDay
	{
		get;
		private set;
	}

	public MonthId Month
	{
		get;
		private set;
	}

	public int Year
	{
		get;
		private set;
	}

	public HolidayId Holiday
	{
		get;
		private set;
	}

	public MoonId Moon
	{
		get;
		private set;
	}

	public Date(int currentDate)
	{
		CurrentDate = currentDate;
		Init();
	}

	private void Init()
	{
		Day = 0;
		WeekDay = WeekDayId.NONE;
		Month = MonthId.NONE;
		Holiday = HolidayId.NONE;
		Moon = MoonId.NONE;
		Year = CurrentDate / Constant.GetInt(ConstantId.CAL_DAYS_PER_YEAR);
		int num = CurrentDate - Year * Constant.GetInt(ConstantId.CAL_DAYS_PER_YEAR);
		Year += Constant.GetInt(ConstantId.CAL_YEAR_START);
		int num2 = 0;
		List<MonthData> list = PandoraSingleton<DataFactory>.Instance.InitData<MonthData>();
		list.Sort(delegate(MonthData x, MonthData y)
		{
			if (x.Id < y.Id)
			{
				return -1;
			}
			return (x.Id > y.Id) ? 1 : 0;
		});
		foreach (MonthData item in list)
		{
			List<HolidayJoinMonthData> list2 = PandoraSingleton<DataFactory>.Instance.InitData<HolidayJoinMonthData>(new string[2]
			{
				"fk_month_id",
				"intercalary"
			}, new string[2]
			{
				((int)item.Id).ToString(),
				"1"
			});
			if (num == 0 && list2.Count == 1)
			{
				Holiday = list2[0].HolidayId;
				Moon = list2[0].MoonId;
				WeekDay = WeekDayId.NONE;
				Month = MonthId.NONE;
				break;
			}
			if (num < item.NumDays + list2.Count)
			{
				Day = num - list2.Count + 1;
				Month = item.Id;
				List<HolidayJoinMonthData> list3 = PandoraSingleton<DataFactory>.Instance.InitData<HolidayJoinMonthData>(new string[3]
				{
					"fk_month_id",
					"day",
					"intercalary"
				}, new string[3]
				{
					((int)item.Id).ToString(),
					Day.ToString(),
					"0"
				});
				if (list3.Count == 1)
				{
					Holiday = list3[0].HolidayId;
					Moon = list3[0].MoonId;
				}
				num2 += Day;
				WeekDay = (WeekDayId)((num2 - 1 + Year % 4 * 2) % 8 + 1);
				break;
			}
			num -= item.NumDays + list2.Count;
			num2 += item.NumDays;
		}
	}

	public int NextDay()
	{
		CurrentDate++;
		Init();
		return CurrentDate;
	}

	public string ToLocalizedHoliday()
	{
		if (Holiday != 0 && Month != 0)
		{
			return PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_holiday_" + Holiday.ToString());
		}
		return string.Empty;
	}

	public string ToLocalizedString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Month == MonthId.NONE)
		{
			stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_holiday_" + Holiday.ToString()));
		}
		else
		{
			stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_weekday_" + WeekDay));
			stringBuilder.Append(", ");
			stringBuilder.Append(Day);
			stringBuilder.Append(" ");
			stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_month_" + Month));
			stringBuilder.Append(" ");
			stringBuilder.Append(Year);
		}
		return stringBuilder.ToString();
	}

	public string ToLocalizedAbbrString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Month == MonthId.NONE)
		{
			stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_holiday_" + Holiday.ToString()));
		}
		else
		{
			stringBuilder.Append(Day);
			stringBuilder.Append(" ");
			stringBuilder.Append(PandoraSingleton<LocalizationManager>.Instance.GetStringById("calendar_month_" + Month));
			stringBuilder.Append(" ");
			stringBuilder.Append(Year);
		}
		return stringBuilder.ToString();
	}

	public int GetNextDay(WeekDayId weekDayId)
	{
		Date date = new Date(CurrentDate + 1);
		while (date.WeekDay != weekDayId)
		{
			date.NextDay();
		}
		return date.CurrentDate;
	}
}
