using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    private int loadedCount;

    private void Awake()
    {
        PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/camera/", AssetBundleId.SCENE_PREFABS, "game_camera.prefab", delegate (Object go)
        {
            GameObject gameObject = (GameObject)Object.Instantiate(go);
            gameObject.name = "game_camera";
            PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/systems/", AssetBundleId.SCENE_PREFABS, "game_systems.prefab", delegate (Object go2)
            {
                GameObject gameObject2 = (GameObject)Object.Instantiate(go2);
                gameObject2.name = "game_systems";
                PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/gui/combat/", AssetBundleId.SCENE_PREFABS, "gui_mission.prefab", delegate (Object go3)
                {
                    GameObject gameObject5 = (GameObject)Object.Instantiate(go3);
                    gameObject5.name = "gui_mission";
                });
                PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/gui/combat/", AssetBundleId.SCENE_PREFABS, "gui_player_retroaction.prefab", delegate (Object go4)
                {
                    GameObject gameObject4 = (GameObject)Object.Instantiate(go4);
                    gameObject4.name = "gui_retroaction";
                });
                PandoraSingleton<AssetBundleLoader>.Instance.LoadAssetAsync<GameObject>("Assets/prefabs/gui/generic/", AssetBundleId.SCENE_PREFABS, "gui_tutorial_popup_large.prefab", delegate (Object go5)
                {
                    GameObject gameObject3 = (GameObject)Object.Instantiate(go5);
                    gameObject3.name = "gui_tutorial";
                });
            });
        });
        Object.Destroy(base.gameObject);
    }
}
