using UnityEngine;

public class AutoCombat : MonoBehaviour
{
    private void Start()
    {
        Animator component = GetComponent<Animator>();
        component.SetInteger("atk_count", 4);
        component.SetInteger("combat_style", 1);
        component.SetLayerWeight(1, 1f);
    }

    private void Update()
    {
    }
}
