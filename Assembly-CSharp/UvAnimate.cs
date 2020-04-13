using UnityEngine;

public class UvAnimate : MonoBehaviour
{
    public float m_fScrollSpeedX = 1f;

    public float m_fScrollSpeedY;

    public float m_fTilingX = 1f;

    public float m_fTilingY = 1f;

    public float m_fOffsetX;

    public float m_fOffsetY;

    public bool m_bFixedTileSize;

    public bool m_bRepeat = true;

    public bool m_bAutoDestruct;

    protected Vector3 m_OriginalScale = default(Vector3);

    protected Vector2 m_OriginalTiling = default(Vector2);

    protected Vector2 m_EndOffset = default(Vector2);

    protected Vector2 m_RepeatOffset = default(Vector2);

    protected Renderer m_Renderer;

    private void Start()
    {
        m_Renderer = GetComponent<Renderer>();
        if (m_Renderer == null || m_Renderer.sharedMaterial == null || m_Renderer.sharedMaterial.mainTexture == null)
        {
            base.enabled = false;
            return;
        }
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(m_fTilingX, m_fTilingY);
        float num = m_fOffsetX + m_fTilingX;
        m_RepeatOffset.x = num - (float)(int)num;
        if (m_RepeatOffset.x < 0f)
        {
            m_RepeatOffset.x += 1f;
        }
        num = m_fOffsetY + m_fTilingY;
        m_RepeatOffset.y = num - (float)(int)num;
        if (m_RepeatOffset.y < 0f)
        {
            m_RepeatOffset.y += 1f;
        }
        m_EndOffset.x = 1f - (m_fTilingX - (float)(int)m_fTilingX + (float)((m_fTilingX - (float)(int)m_fTilingX < 0f) ? 1 : 0));
        m_EndOffset.y = 1f - (m_fTilingY - (float)(int)m_fTilingY + (float)((m_fTilingY - (float)(int)m_fTilingY < 0f) ? 1 : 0));
    }

    private void OnWillRenderObject()
    {
        if (m_Renderer == null || m_Renderer.sharedMaterial == null)
        {
            return;
        }
        if (m_bFixedTileSize)
        {
            if (m_fScrollSpeedX != 0f && m_OriginalScale.x != 0f)
            {
                float x = m_OriginalTiling.x;
                Vector3 lossyScale = base.transform.lossyScale;
                m_fTilingX = x * (lossyScale.x / m_OriginalScale.x);
            }
            if (m_fScrollSpeedY != 0f && m_OriginalScale.y != 0f)
            {
                float y = m_OriginalTiling.y;
                Vector3 lossyScale2 = base.transform.lossyScale;
                m_fTilingY = y * (lossyScale2.y / m_OriginalScale.y);
            }
            GetComponent<Renderer>().material.mainTextureScale = new Vector2(m_fTilingX, m_fTilingY);
        }
        m_fOffsetX += Time.deltaTime * m_fScrollSpeedX;
        m_fOffsetY += Time.deltaTime * m_fScrollSpeedY;
        bool flag = false;
        if (!m_bRepeat)
        {
            m_RepeatOffset.x += Time.deltaTime * m_fScrollSpeedX;
            if (m_RepeatOffset.x < 0f || 1f < m_RepeatOffset.x)
            {
                m_fOffsetX = m_EndOffset.x;
                base.enabled = false;
                flag = true;
            }
            m_RepeatOffset.y += Time.deltaTime * m_fScrollSpeedY;
            if (m_RepeatOffset.y < 0f || 1f < m_RepeatOffset.y)
            {
                m_fOffsetY = m_EndOffset.y;
                base.enabled = false;
                flag = true;
            }
        }
        Vector2 vector = new Vector2(m_fOffsetX, m_fOffsetY);
        m_Renderer.material.mainTextureOffset = vector;
        if (m_Renderer.material.HasProperty("_BumpMap"))
        {
            m_Renderer.material.SetTextureOffset("_BumpMap", vector);
        }
        if (m_Renderer.material.HasProperty("_SpecTex"))
        {
            m_Renderer.material.SetTextureOffset("_SpecTex", vector);
        }
        if (flag && m_bAutoDestruct)
        {
            Object.DestroyObject(base.gameObject);
        }
    }
}
