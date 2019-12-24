using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public MapSettings MapSettings;

    public Map Map;
    public Map GoalMap;

    public int Layer_MapBoard;
    public bool IsLevelEditorMode = false;

    void Awake()
    {
        Layer_MapBoard = 1 << LayerMask.NameToLayer("MapBoard");
    }

    void Start()
    {
        AllLevels.AddAllLevels();
        UIManager.Instance.ShowUIForms<LevelEditorPanel>();
        Map = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Map].AllocateGameObject<Map>(transform);
        GoalMap = Map.GoalMap;
        LoadLevel(0);
    }

    public void LoadLevel(int levelID)
    {
        if (AllLevels.LevelDict.ContainsKey(levelID))
        {
            LevelInfo levelInfo = AllLevels.LevelDict[levelID].Clone();
            Map.LoadLevel(levelInfo, MapTypes.Start);
            GoalMap.LoadLevel(levelInfo, MapTypes.Goal);
            UIManager.Instance.GetBaseUIForm<LevelEditorPanel>().OnLoadLevel(levelInfo);
        }
    }
}