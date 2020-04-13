using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UIObjectivesController : CanvasGroupDisabler
{
    public const string RES_DUAL_COUNTER = "{0} / {1}";

    public GameObject ObjectiveCatPrefab;

    public GameObject ObjectivePrefab;

    public GameObject SubObjectivePrefab;

    private ListGroup mainGroup;

    private List<ObjectiveView> objectiveViews = new List<ObjectiveView>();

    private bool sequencePlaying;

    private void Awake()
    {
        PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.GAME_OBJECTIVE_UPDATE, UpdateObjective);
        mainGroup = GetComponent<ListGroup>();
        mainGroup.Setup(null, ObjectiveCatPrefab);
    }

    private void UpdateObjective()
    {
        if (objectiveViews.Count == 0)
        {
            objectiveViews = new List<ObjectiveView>();
            GameObject gameObject = null;
            ListGroup listGroup = null;
            mainGroup.ClearList();
            List<Objective> objectives = (List<Objective>)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
            SetObjectives(objectives);
            if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign)
            {
                gameObject = mainGroup.AddToList();
                listGroup = SetCategory(gameObject, "mission_battleground_ressources", ObjectivePrefab, isOptional: false);
                Vector2 vector = (Vector2)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
                AddObjective(listGroup, null, -1, PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_counter_search"), loading: false, (int)vector.x, (int)vector.y);
                Vector2 vector2 = (Vector2)PandoraSingleton<NoticeManager>.Instance.Parameters[2];
                AddObjective(listGroup, null, -1, PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_counter_wyrdstone_collected"), loading: false, (int)vector2.x, (int)vector2.y);
            }
        }
        else if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign)
        {
            int i;
            for (i = 0; i < objectiveViews.Count - 2; i++)
            {
                objectiveViews[i].UpdateObjective(loading: false);
            }
            Vector2 vector3 = (Vector2)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
            objectiveViews[i++].UpdateObjective(loading: false, (int)vector3.x, (int)vector3.y);
            Vector2 vector4 = (Vector2)PandoraSingleton<NoticeManager>.Instance.Parameters[2];
            objectiveViews[i].UpdateObjective(loading: false, (int)vector4.x, (int)vector4.y);
        }
        else
        {
            for (int j = 0; j < objectiveViews.Count; j++)
            {
                objectiveViews[j].UpdateObjective(loading: false);
            }
        }
    }

    private ListGroup SetCategory(GameObject cat, string title, GameObject prefab, bool isOptional)
    {
        ListGroup component = cat.GetComponent<ListGroup>();
        if (!string.IsNullOrEmpty(title))
        {
            title = PandoraSingleton<LocalizationManager>.Instance.GetStringById(title);
            if (isOptional)
            {
                title = title + " " + PandoraSingleton<LocalizationManager>.Instance.GetStringById("mission_obj_optional");
            }
        }
        component.SetupLocalized(title, prefab);
        return component;
    }

    public void SetObjectives(List<Objective> objectives, bool loading = false)
    {
        if (loading)
        {
            PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.GAME_OBJECTIVE_UPDATE, UpdateObjective);
        }
        if (objectives == null)
        {
            return;
        }
        GameObject gameObject = null;
        ListGroup listGroup = null;
        if (!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign || PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isTuto)
        {
            gameObject = mainGroup.AddToList();
            listGroup = SetCategory(gameObject, "mission_battleground", ObjectivePrefab, isOptional: false);
            AddObjective(listGroup, null, -1, PandoraSingleton<LocalizationManager>.Instance.GetStringById("battleground_objective"), loading);
        }
        if (objectives.Count <= 0 || objectives[0] == null)
        {
            return;
        }
        gameObject = mainGroup.AddToList();
        bool isOptional = PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.VictoryTypeId != 2;
        listGroup = ((!PandoraSingleton<MissionStartData>.Instance.CurrentMission.missionSave.isCampaign) ? SetCategory(gameObject, "mission_obj_" + PandoraSingleton<MissionStartData>.Instance.CurrentMission.GetObjectiveTypeId(PandoraSingleton<Hermes>.Instance.PlayerIndex), ObjectivePrefab, isOptional) : SetCategory(gameObject, "mission_objectives", ObjectivePrefab, isOptional));
        for (int i = 0; i < objectives.Count; i++)
        {
            GameObject cat = AddObjective(listGroup, objectives[i], -1, objectives[i].desc, loading);
            if (objectives[i].subDesc.Count > 0)
            {
                ListGroup listGroup2 = SetCategory(cat, string.Empty, SubObjectivePrefab, isOptional: false);
                for (int j = 0; j < objectives[i].subDesc.Count; j++)
                {
                    AddObjective(listGroup2, objectives[i], j, objectives[i].subDesc[j], loading);
                }
            }
        }
    }

    private GameObject AddObjective(ListGroup listGroup, Objective objective, int subIndex, string desc, bool loading = false, int counter1 = -1, int counter2 = -1)
    {
        GameObject gameObject = listGroup.AddToList();
        ObjectiveView objectiveView = gameObject.GetComponentsInChildren<ObjectiveView>(includeInactive: true)[0];
        objectiveView.mainObjective = objective;
        objectiveView.subIndex = subIndex;
        objectiveView.Set(objective, subIndex, desc, loading);
        objectiveView.UpdateObjective(loading, counter1, counter2);
        objectiveViews.Add(objectiveView);
        return gameObject;
    }

    private void Update()
    {
        if (objectiveViews.Count == 0 || !PandoraSingleton<SequenceManager>.Exists())
        {
            return;
        }
        if (!sequencePlaying && PandoraSingleton<SequenceManager>.Instance.isPlaying)
        {
            sequencePlaying = true;
            for (int i = 0; i < objectiveViews.Count; i++)
            {
                DOTween.Pause((object)objectiveViews[i]);
            }
        }
        else if (sequencePlaying && !PandoraSingleton<SequenceManager>.Instance.isPlaying)
        {
            sequencePlaying = false;
            for (int j = 0; j < objectiveViews.Count; j++)
            {
                DOTween.Play((object)objectiveViews[j]);
            }
        }
    }
}
