using System;

namespace UnityEngine.UI
{
    [Serializable]
    public struct ScaleBlock
    {
        public float duration;

        public Vector2 disabledScale;

        public Vector2 highlightedScale;

        public Vector2 normalScale;

        public Vector2 pressedScale;

        public static ScaleBlock defaultScaleBlock
        {
            get
            {
                ScaleBlock result = default(ScaleBlock);
                result.normalScale = new Vector2(1f, 1f);
                result.highlightedScale = new Vector2(1.1f, 1.1f);
                result.pressedScale = new Vector2(1.1f, 1.1f);
                result.disabledScale = new Vector2(0.9f, 0.9f);
                result.duration = 0.1f;
                return result;
            }
        }
    }
}
