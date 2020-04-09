using HighlightingSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuNode : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public GameObject content;

	private Color highLightColor = new Color(59f / 510f, 19f / 51f, 11f / 51f);

	private BoxCollider boxCollider;

	public Highlighter highlightable
	{
		get;
		private set;
	}

	public bool KeepSelected
	{
		get;
		set;
	}

	public bool IsSelectable
	{
		get;
		set;
	}

	private void Awake()
	{
		IsSelectable = true;
		boxCollider = GetComponent<BoxCollider>();
	}

	public bool IsOccupied()
	{
		return content != null;
	}

	public GameObject GetContent()
	{
		return content;
	}

	public void RemoveContent()
	{
		if (content != null)
		{
			content.transform.SetParent(null);
			content.SetActive(value: false);
			content = null;
		}
		if (boxCollider != null)
		{
			boxCollider.enabled = true;
		}
		Unselect();
	}

	public void DestroyContent()
	{
		if (content != null)
		{
			Object.Destroy(content);
			content = null;
		}
	}

	public void SetContent(GameObject newContent)
	{
		RemoveContent();
		content = newContent;
		content.SetActive(value: true);
		content.transform.SetParent(base.transform);
		if (content != null)
		{
			content.transform.position = base.transform.position;
			content.transform.rotation = base.transform.rotation;
			Hide();
		}
		else
		{
			Unselect();
		}
	}

	public void SetContent(UnitMenuController unitCtrlr, MenuNode otherNode = null)
	{
		RemoveContent();
		content = unitCtrlr.gameObject;
		content.SetActive(value: true);
		content.transform.SetParent(base.transform);
		if (content != null)
		{
			Vector3 vector = base.transform.position;
			Quaternion quaternion = base.transform.rotation;
			if (otherNode != null)
			{
				vector = Vector3.Lerp(vector, otherNode.transform.position, 0.5f);
				quaternion = Quaternion.Lerp(quaternion, otherNode.transform.rotation, 0.5f);
			}
			unitCtrlr.InstantMove(vector, quaternion);
			Hide();
		}
		else
		{
			Unselect();
		}
	}

	public void Hide()
	{
	}

	public bool Visible()
	{
		return content;
	}

	public bool IsContent(GameObject c)
	{
		if ((bool)content || !c)
		{
			return content == c;
		}
		return base.gameObject == c;
	}

	public bool IsContent(string gameObjectName)
	{
		if (!content)
		{
			return false;
		}
		return gameObjectName.Equals(content.name);
	}

	public void Select()
	{
		if ((bool)content)
		{
			Highlight(highlight: true, highLightColor);
		}
	}

	public void Unselect()
	{
		if (!KeepSelected && (bool)content)
		{
			Highlight(highlight: false, highLightColor);
		}
	}

	public void Lock()
	{
		if ((bool)content)
		{
			Highlight(highlight: true, Color.red);
		}
	}

	public void SetHighlightColor(Color newColor)
	{
		highLightColor = newColor;
	}

	private void Highlight(bool highlight, Color color)
	{
		if (!content)
		{
			return;
		}
		highlightable = content.GetComponent<Highlighter>();
		if ((Object)(object)highlightable == null)
		{
			highlightable = content.AddComponent<Highlighter>();
			highlightable.seeThrough = false;
		}
		if ((bool)(Object)(object)highlightable)
		{
			if (highlight)
			{
				highlightable.ConstantOn(color, 0.25f);
			}
			else
			{
				highlightable.Off();
			}
		}
	}

	private void Activate(bool activate, GameObject go)
	{
		if (go != null && activate != go.activeSelf)
		{
			go.SetActive(activate);
		}
	}

	private void OnDrawGizmos()
	{
		PandoraUtils.DrawFacingGizmoCube(base.transform, 2f, 0.75f, 0.75f);
		Gizmos.DrawIcon(base.transform.position + new Vector3(0f, 2f, 0f), "start.png", allowScaling: true);
		Gizmos.DrawSphere(base.transform.position, 0.03f);
	}

	public void OnSelect(BaseEventData eventData)
	{
		Select();
	}
}
