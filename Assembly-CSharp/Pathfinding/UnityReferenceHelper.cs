using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[ExecuteInEditMode]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_unity_reference_helper.php")]
	public class UnityReferenceHelper : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		private string guid;

		public string GetGUID()
		{
			return guid;
		}

		public void Awake()
		{
			Reset();
		}

		public void Reset()
		{
			if (string.IsNullOrEmpty(guid))
			{
				guid = Guid.NewGuid().ToString();
				Debug.Log("Created new GUID - " + guid);
				return;
			}
			UnityReferenceHelper[] array = Object.FindObjectsOfType(typeof(UnityReferenceHelper)) as UnityReferenceHelper[];
			int num = 0;
			while (true)
			{
				if (num < array.Length)
				{
					UnityReferenceHelper unityReferenceHelper = array[num];
					if (unityReferenceHelper != this && guid == unityReferenceHelper.guid)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			guid = Guid.NewGuid().ToString();
			Debug.Log("Created new GUID - " + guid);
		}
	}
}
