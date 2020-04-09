using System.Collections.Generic;
using UnityEngine;

public class LODMeshGroup
{
	public Dictionary<Material, List<CombineInstance>> materialInstances;

	public LODMeshGroup()
	{
		materialInstances = new Dictionary<Material, List<CombineInstance>>();
	}
}
