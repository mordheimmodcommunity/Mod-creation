using UnityEngine;

public class test_launch_loading : MonoBehaviour
{
    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("prefabs/transitions/transition_fade"));
            TransitionBase component = gameObject.GetComponent<TransitionBase>();
            PandoraSingleton<TransitionManager>.Instance.LoadNextScene("test_move_actions", SceneLoadingTypeId.MAIN_MENU, 1.5f, waitForAction: true);
        }
    }
}
