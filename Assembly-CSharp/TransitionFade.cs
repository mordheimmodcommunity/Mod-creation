using UnityEngine;

public class TransitionFade : TransitionBase
{
    private const int SCREEN_BLEEDING = 10;

    private const int FADE_GUI_DEPTH = 1000;

    private Color TRANSPARENT = new Color(0f, 0f, 0f, 0f);

    private Color BLACK = new Color(0f, 0f, 0f, 1f);

    private GUIStyle m_backgroundStyle = new GUIStyle();

    private Texture2D m_fadeTexture;

    private Color m_currentOverlayColor = new Color(0f, 0f, 0f, 0f);

    private Color m_targetOverlayColor = new Color(0f, 0f, 0f, 0f);

    private Color m_deltaColor = new Color(0f, 0f, 0f, 0f);

    private bool isVisible;

    private void Awake()
    {
        m_fadeTexture = new Texture2D(1, 1);
        m_backgroundStyle.normal.background = m_fadeTexture;
        SetScreenOverlayColor(TRANSPARENT);
        base.enabled = false;
    }

    private void OnGUI()
    {
        GUI.depth = 1000;
        GUI.Label(new Rect(-10f, -10f, Screen.width + 10, Screen.height + 10), m_fadeTexture, m_backgroundStyle);
    }

    private void SetScreenOverlayColor(Color newScreenOverlayColor)
    {
        m_currentOverlayColor = newScreenOverlayColor;
        m_fadeTexture.SetPixel(0, 0, m_currentOverlayColor);
        m_fadeTexture.Apply();
    }

    public override void Show(bool visible, float duration)
    {
        isVisible = visible;
        if (visible)
        {
            base.enabled = true;
        }
        SetScreenOverlayColor((!visible) ? BLACK : TRANSPARENT);
        m_targetOverlayColor = ((!visible) ? TRANSPARENT : BLACK);
        m_deltaColor = (m_targetOverlayColor - m_currentOverlayColor) / duration;
    }

    public override void ProcessTransition(float progress)
    {
        SetScreenOverlayColor(m_currentOverlayColor + m_deltaColor * Time.deltaTime);
    }

    public override void EndTransition()
    {
        m_currentOverlayColor = m_targetOverlayColor;
        SetScreenOverlayColor(m_currentOverlayColor);
        m_deltaColor = new Color(0f, 0f, 0f, 0f);
        if (!isVisible)
        {
            base.enabled = isVisible;
        }
    }

    public override void Reset()
    {
        SetScreenOverlayColor(TRANSPARENT);
    }
}
