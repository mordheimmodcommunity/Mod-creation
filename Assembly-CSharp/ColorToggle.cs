using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ColorToggle : MonoBehaviour
{
    public Toggle target;

    public Graphic targetGraphic;

    private Transform cachedTransform;

    public ColorBlock color = MordheimColorBlock.defaultColorBlock;

    protected Transform Transform
    {
        get
        {
            if (cachedTransform == null)
            {
                cachedTransform = base.transform;
            }
            return cachedTransform;
        }
    }

    private void Awake()
    {
        if ((Object)(object)target == null)
        {
            target = GetComponent<Toggle>();
            ((UnityEvent<bool>)(object)target.onValueChanged).AddListener((UnityAction<bool>)OnValueChanged);
        }
        if ((Object)(object)targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }
    }

    private void OnEnable()
    {
        OnValueChanged(isOn: false);
    }

    private void OnValueChanged(bool isOn)
    {
        Color((!isOn) ? ((ColorBlock)(ref color)).get_normalColor() : ((ColorBlock)(ref color)).get_highlightedColor());
    }

    private void Color(Color newColor)
    {
        if (Application.isPlaying)
        {
            if ((Object)(object)targetGraphic == null)
            {
                ((Selectable)target).get_targetGraphic().CrossFadeColor(newColor, ((ColorBlock)(ref color)).get_fadeDuration(), true, true);
            }
            else
            {
                targetGraphic.CrossFadeColor(newColor, ((ColorBlock)(ref color)).get_fadeDuration(), true, true);
            }
        }
    }
}
