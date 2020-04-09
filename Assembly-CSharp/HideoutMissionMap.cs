using System.Collections.Generic;
using UnityEngine;

public class HideoutMissionMap : MenuNodeGroup
{
	public List<MenuNode> campaignNodes;

	public List<MenuNode> procNodesDis01;

	public List<MenuNode> procNodesDis02;

	private MenuNodeDelegateNode onSelectCampaign;

	private MenuNodeDelegateNode onSelectDis01Node;

	private MenuNodeDelegateNode onSelectDis02Node;

	protected override void Awake()
	{
		base.Awake();
		nodes.Sort(delegate(MenuNode a, MenuNode b)
		{
			Vector3 position = b.transform.position;
			ref float x = ref position.x;
			Vector3 position2 = a.transform.position;
			return x.CompareTo(position2.x);
		});
	}

	public void Activate(MenuNodeDelegateNode selectCampaign, MenuNodeDelegateNode selectDis01Node, MenuNodeDelegateNode selectDis02Node, MenuNodeDelegateNode unSelect, MenuNodeDelegateNode confirm, PandoraInput.InputLayer workingLayer, bool unselectOverOut = true)
	{
		onSelectCampaign = selectCampaign;
		onSelectDis01Node = selectDis01Node;
		onSelectDis02Node = selectDis02Node;
		Activate(OnSelectNode, unSelect, confirm, workingLayer, unselectOverOut);
	}

	private void OnSelectNode(MenuNode node, int idx)
	{
		node.SetSelected(force: true);
		idx = campaignNodes.IndexOf(node);
		if (idx != -1)
		{
			onSelectCampaign(node, idx);
			return;
		}
		idx = procNodesDis01.IndexOf(node);
		if (idx != -1)
		{
			onSelectDis01Node(node, idx);
			return;
		}
		idx = procNodesDis02.IndexOf(node);
		if (idx != -1)
		{
			onSelectDis02Node(node, idx);
		}
	}
}
