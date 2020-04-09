using System.Collections.Generic;
using UnityEngine;

public abstract class ContentView<TComponent, T> : CanvasGroupDisabler where TComponent : MonoBehaviour
{
	public GameObject template;

	public RectTransform templateTransform;

	public RectTransform container;

	private readonly List<TComponent> components = new List<TComponent>();

	private int currentIndex;

	public List<TComponent> Components => components;

	protected virtual void Awake()
	{
		template.gameObject.SetActive(value: false);
		templateTransform = (template.transform as RectTransform);
		if (container == null)
		{
			container = (base.transform as RectTransform);
		}
	}

	protected virtual void OnDestroy()
	{
		Components.Clear();
	}

	public void OnAddEnd()
	{
		for (int i = currentIndex; i < Components.Count; i++)
		{
			TComponent val = Components[i];
			val.gameObject.SetActive(value: false);
		}
		currentIndex = 0;
	}

	public TComponent Add(T toAdd)
	{
		TComponent val;
		if (currentIndex < Components.Count)
		{
			val = Components[currentIndex];
			val.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = Create(template);
			val = gameObject.GetComponent<TComponent>();
			Components.Add(val);
		}
		currentIndex++;
		OnAdd(val, toAdd);
		return val;
	}

	protected GameObject Create(GameObject template)
	{
		GameObject gameObject = Object.Instantiate(template);
		gameObject.transform.SetParent(container);
		gameObject.transform.localScale = template.transform.localScale;
		gameObject.SetActive(value: true);
		return gameObject;
	}

	protected abstract void OnAdd(TComponent component, T obj);
}
