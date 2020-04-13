using UnityEngine;

namespace WellFired
{
    [USequencerEvent("Fullscreen/Display Image")]
    [USequencerFriendlyName("Display Image")]
    [USequencerEventHideDuration]
    public class USDisplayImageEvent : USEventBase
    {
        public UILayer uiLayer;

        public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f), new Keyframe(3f, 1f), new Keyframe(4f, 0f));

        public Texture2D displayImage;

        public UIPosition displayPosition;

        public UIPosition anchorPosition;

        private float currentCurveSampleTime;

        public USDisplayImageEvent()
            : this()
        {
        }

        public override void FireEvent()
        {
            if (!displayImage)
            {
                Debug.LogWarning("Trying to use a DisplayImage Event, but you didn't give it an image to display", (Object)(object)this);
            }
        }

        public override void ProcessEvent(float deltaTime)
        {
            currentCurveSampleTime = deltaTime;
        }

        public override void EndEvent()
        {
            float b = fadeCurve.Evaluate(fadeCurve.keys[fadeCurve.length - 1].time);
            b = Mathf.Min(Mathf.Max(0f, b), 1f);
        }

        public override void StopEvent()
        {
            UndoEvent();
        }

        public override void UndoEvent()
        {
            currentCurveSampleTime = 0f;
        }

        private void OnGUI()
        {
            //IL_00d4: Unknown result type (might be due to invalid IL or missing references)
            //IL_00d9: Unknown result type (might be due to invalid IL or missing references)
            //IL_00db: Unknown result type (might be due to invalid IL or missing references)
            //IL_00de: Unknown result type (might be due to invalid IL or missing references)
            //IL_00f4: Expected I4, but got Unknown
            //IL_0172: Unknown result type (might be due to invalid IL or missing references)
            //IL_0177: Unknown result type (might be due to invalid IL or missing references)
            //IL_0179: Unknown result type (might be due to invalid IL or missing references)
            //IL_0194: Expected I4, but got Unknown
            //IL_0256: Unknown result type (might be due to invalid IL or missing references)
            //IL_0260: Expected I4, but got Unknown
            if (!((USEventBase)this).get_Sequence().get_IsPlaying())
            {
                return;
            }
            float num = 0f;
            Keyframe[] keys = fadeCurve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                Keyframe keyframe = keys[i];
                if (keyframe.time > num)
                {
                    num = keyframe.time;
                }
            }
            ((USEventBase)this).set_Duration(num);
            float b = fadeCurve.Evaluate(currentCurveSampleTime);
            b = Mathf.Min(Mathf.Max(0f, b), 1f);
            if ((bool)displayImage)
            {
                Rect position = new Rect((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, displayImage.width, displayImage.height);
                UIPosition val = displayPosition;
                switch (val - 1)
                {
                    case 0:
                        position.x = 0f;
                        position.y = 0f;
                        break;
                    case 1:
                        position.x = Screen.width;
                        position.y = 0f;
                        break;
                    case 2:
                        position.x = 0f;
                        position.y = Screen.height;
                        break;
                    case 3:
                        position.x = Screen.width;
                        position.y = Screen.height;
                        break;
                }
                val = anchorPosition;
                switch ((int)val)
                {
                    case 0:
                        position.x -= (float)displayImage.width * 0.5f;
                        position.y -= (float)displayImage.height * 0.5f;
                        break;
                    case 2:
                        position.x -= displayImage.width;
                        break;
                    case 3:
                        position.y -= displayImage.height;
                        break;
                    case 4:
                        position.x -= displayImage.width;
                        position.y -= displayImage.height;
                        break;
                }
                GUI.depth = (int)uiLayer;
                Color color = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, b);
                GUI.DrawTexture(position, displayImage);
                GUI.color = color;
            }
        }
    }
}
