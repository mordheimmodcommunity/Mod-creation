using System.Collections.Generic;
using UnityEngine;

public class MenuNodeGroup : MonoBehaviour
{
	public bool setDOF = true;

	public List<MenuNode> nodes;

	private bool isOn;

	private Vector3 mouseLastPosition;

	private MenuNode currentNode;

	private int nodeIdx;

	private MenuNodeDelegateNode selectDel;

	private MenuNodeDelegateNode unSelectDel;

	private MenuNodeDelegateNode confirmDel;

	private bool unSelectOnOverOut;

	private PandoraInput.InputLayer layer;

	protected virtual void Awake()
	{
		isOn = false;
	}

	public void Activate(MenuNodeDelegateNode select, MenuNodeDelegateNode unSelect, MenuNodeDelegateNode confirm, PandoraInput.InputLayer workingLayer, bool unselectOverOut = true)
	{
		selectDel = select;
		unSelectDel = unSelect;
		confirmDel = confirm;
		layer = workingLayer;
		unSelectOnOverOut = unselectOverOut;
		UnSelectCurrentNode();
		isOn = true;
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].Unselect();
		}
		mouseLastPosition = PandoraSingleton<PandoraInput>.Instance.GetMousePosition();
	}

	public void Deactivate()
	{
		selectDel = null;
		unSelectDel = null;
		confirmDel = null;
		isOn = false;
		UnSelectCurrentNode();
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].Unselect();
		}
	}

	public void Clear()
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].RemoveContent();
		}
	}

	private void Update()
	{
		if (!isOn)
		{
			return;
		}
		Vector3 mousePosition = PandoraSingleton<PandoraInput>.Instance.GetMousePosition();
		if (mousePosition != mouseLastPosition)
		{
			mouseLastPosition = mousePosition;
			MenuNode overNode = GetOverNode();
			if (overNode != null && nodes.IndexOf(overNode) != -1)
			{
				SelectNode(overNode);
			}
			else if (unSelectOnOverOut)
			{
				UnSelectCurrentNode();
			}
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("h") || PandoraSingleton<PandoraInput>.Instance.GetNegKeyDown("h"))
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodeIdx += (PandoraSingleton<PandoraInput>.Instance.GetKeyDown("h") ? 1 : (-1));
				nodeIdx = ((nodeIdx < nodes.Count) ? nodeIdx : 0);
				nodeIdx = ((nodeIdx >= 0) ? nodeIdx : (nodes.Count - 1));
				if (nodes[nodeIdx].IsSelectable && nodes[nodeIdx].gameObject.activeSelf)
				{
					break;
				}
			}
			SelectNode(nodes[nodeIdx]);
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("action"))
		{
			ConfirmNode();
		}
		else if (PandoraSingleton<PandoraInput>.Instance.GetKeyUp("mouse_click"))
		{
			MenuNode overNode2 = GetOverNode();
			if (overNode2 != null && nodes.IndexOf(overNode2) != -1)
			{
				ConfirmNode();
			}
		}
	}

	public void SelectNode(MenuNode node)
	{
		if (currentNode != node)
		{
			UnSelectCurrentNode();
			currentNode = node;
			node.Select();
			nodeIdx = nodes.IndexOf(node);
			if (selectDel != null)
			{
				selectDel(currentNode, nodeIdx);
			}
			if (setDOF)
			{
				PandoraSingleton<HideoutManager>.Instance.CamManager.SetDOFTarget(currentNode.gameObject.transform, 0f);
			}
		}
	}

	public void UnSelectCurrentNode()
	{
		if (currentNode != null)
		{
			MenuNode node = currentNode;
			int idx = nodeIdx;
			currentNode.Unselect();
			currentNode = null;
			nodeIdx = 0;
			if (unSelectDel != null)
			{
				unSelectDel(node, idx);
			}
		}
	}

	private void ConfirmNode()
	{
		if (!(currentNode == null))
		{
			MenuNode node = currentNode;
			int idx = nodeIdx;
			UnSelectCurrentNode();
			if (confirmDel != null)
			{
				confirmDel(node, idx);
			}
		}
	}

	private MenuNode GetOverNode()
	{
		if (PandoraSingleton<PandoraInput>.Instance.CurrentInputLayer == (int)layer)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hitInfo, float.PositiveInfinity, LayerMaskManager.menuNodeMask))
			{
				MenuNode componentInParent = hitInfo.transform.GetComponentInParent<MenuNode>();
				if (componentInParent != null && componentInParent.IsSelectable)
				{
					return componentInParent;
				}
			}
		}
		return null;
	}
}
