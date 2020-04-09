using UnityEngine;
using UnityEngine.UI;

public class MapBeacon : MonoBehaviour
{
	public MapImprint imprint;

	public FlyingOverview flyingOverview;

	public Color imprintColor;

	private void Awake()
	{
		imprint = GetComponent<MapImprint>();
		if (imprint != null)
		{
			imprint.Init("icn_mission_type_mission", null, alwaysVisible: true, MapImprintType.BEACON);
			imprint.Beacon = this;
		}
	}

	private void OnEnable()
	{
		Refresh();
	}

	public void Refresh()
	{
		if (!(imprint == null))
		{
			imprint.RefreshPosition();
			if (flyingOverview != null)
			{
				flyingOverview.Set(imprint, clamp: true, selected: false);
				((Graphic)flyingOverview.bgBeacon).set_color(imprintColor);
				((Graphic)flyingOverview.icon).set_color(imprintColor);
				flyingOverview.gameObject.SetActive(value: true);
				flyingOverview.transform.SetAsLastSibling();
			}
		}
	}

	private void OnDisable()
	{
		if (flyingOverview != null)
		{
			flyingOverview.gameObject.SetActive(value: false);
		}
	}
}
