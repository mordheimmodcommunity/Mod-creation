using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class ResetToZero : MonoBehaviour
{
    private void Awake()
    {
        (base.transform as RectTransform).anchoredPosition = Vector3.zero;
    }
}
