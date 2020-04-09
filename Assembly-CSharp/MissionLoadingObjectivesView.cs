using UnityEngine;

public class MissionLoadingObjectivesView : MonoBehaviour
{
	public GameObject mainObjectiveTemplate;

	public GameObject secondaryObjectiveTemplate;

	private void Awake()
	{
		mainObjectiveTemplate.SetActive(value: false);
		secondaryObjectiveTemplate.SetActive(value: false);
	}

	public void Add(bool done, string text, bool isPrimary)
	{
		GameObject gameObject = Object.Instantiate((!isPrimary) ? secondaryObjectiveTemplate : mainObjectiveTemplate);
		gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
		gameObject.SetActive(value: true);
		ObjectiveView component = gameObject.GetComponent<ObjectiveView>();
		component.toggleObjective.set_isOn(done);
		component.objectiveText.set_text(text);
	}

	public void Add(Objective objective)
	{
		Add(objective.done, PandoraSingleton<LocalizationManager>.Instance.GetStringById(objective.desc), isPrimary: true);
		for (int i = 0; (float)i < objective.counter.y; i++)
		{
			if (objective.TypeId == PrimaryObjectiveTypeId.BOUNTY)
			{
				Add(objective.dones[i], PandoraSingleton<LocalizationManager>.Instance.GetStringById(objective.subDesc[2 * i]), isPrimary: false);
			}
			else
			{
				Add(objective.dones[i], PandoraSingleton<LocalizationManager>.Instance.GetStringById(objective.subDesc[i]), isPrimary: false);
			}
		}
	}
}
