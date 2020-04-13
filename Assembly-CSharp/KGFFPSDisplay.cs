using UnityEngine;

public class KGFFPSDisplay : MonoBehaviour
{
    private float itsFPS;

    private int itsFrameCounter;

    private float itsLastMeasurePoint;

    public float itsTimeBetweenMeasurePoints = 2f;

    public int itsFontSize = 30;

    public Color itsFontColor = Color.white;

    private GUIStyle itsStyleText;

    private void Start()
    {
        itsStyleText = new GUIStyle();
        itsStyleText.fontSize = itsFontSize;
        itsStyleText.normal.textColor = Color.white;
    }

    private void Update()
    {
        itsFrameCounter++;
        if (Time.time - itsLastMeasurePoint > itsTimeBetweenMeasurePoints)
        {
            itsFPS = (float)itsFrameCounter / (Time.time - itsLastMeasurePoint);
            itsFrameCounter = 0;
            itsLastMeasurePoint = Time.time;
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(1f, 1f, 200f, 200f), string.Empty + (int)itsFPS + " FPS", itsStyleText);
        GUI.color = itsFontColor;
        GUI.Label(new Rect(0f, 0f, 200f, 200f), string.Empty + (int)itsFPS + " FPS", itsStyleText);
    }
}
