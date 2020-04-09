using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class FindToggleGroupInParent : MonoBehaviour
{
	private void Start()
	{
		Toggle component = GetComponent<Toggle>();
		ToggleGroup componentInParent = GetComponentInParent<ToggleGroup>();
		component.set_group(componentInParent);
	}
}
