using UnityEngine;

public class test_points : MonoBehaviour
{
	private void OnGUI()
	{
		UnitController currentUnit = PandoraSingleton<MissionManager>.Instance.GetCurrentUnit();
		if (currentUnit != null)
		{
			GUI.Button(new Rect(0f, 0f, 200f, 50f), "Strat Points = " + currentUnit.unit.CurrentStrategyPoints);
			GUI.Button(new Rect(0f, 50f, 200f, 50f), "Off Points = " + currentUnit.unit.CurrentOffensePoints);
			GUI.Button(new Rect(0f, 100f, 200f, 50f), "Temp Strat Points = " + currentUnit.unit.tempStrategyPoints);
			GUI.Button(new Rect(0f, 150f, 200f, 50f), "Temp Off Points = " + currentUnit.unit.tempOffensePoints);
		}
	}
}
