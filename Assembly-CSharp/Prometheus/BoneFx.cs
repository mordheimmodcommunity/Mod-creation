using System;
using UnityEngine;

namespace Prometheus
{
    [Serializable]
    public class BoneFx
    {
        public bool active;

        public BoneId bone;

        public Vector3 offset;

        public Vector3 rotation;

        public bool rotationWorldSpace;

        public bool lockRotation;

        public float scale;
    }
}
