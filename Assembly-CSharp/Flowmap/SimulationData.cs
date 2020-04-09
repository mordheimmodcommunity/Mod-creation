using System;
using UnityEngine;

namespace Flowmap
{
	[Serializable]
	internal struct SimulationData
	{
		public float height;

		public float fluid;

		public float addFluid;

		public float removeFluid;

		public Vector3 force;

		public Vector4 outflow;

		public Vector2 velocity;

		public Vector3 velocityAccumulated;
	}
}
