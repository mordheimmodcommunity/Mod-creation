using UnityEngine;

[AddComponentMenu("Infinity Code/Note")]
public class Note : MonoBehaviour
{
    public static Texture2D defaultIcon;

    public bool expanded = true;

    public float height = 45f;

    public Texture2D icon;

    public string iconPath = string.Empty;

    public bool isPrefab;

    public bool lockHeight;

    public float managerHeight = 45f;

    public Vector2 managerScrollPos;

    public float maxHeight = 800f;

    public Vector2 scrollPos;

    public string text = string.Empty;

    public bool wordWrap;

    private int _instanceID = int.MinValue;

    public string title
    {
        get
        {
            string text = base.gameObject.name;
            if (isPrefab)
            {
                text = "Prefab: " + text;
            }
            return text;
        }
    }

    public int instanceID
    {
        get
        {
            if (_instanceID == int.MinValue)
            {
                _instanceID = base.gameObject.GetInstanceID();
            }
            return _instanceID;
        }
    }
}
