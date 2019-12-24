using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorPanel : BaseUIForm
{
    void Awake()
    {
        UIType.IsClearStack = false;
        UIType.IsESCClose = false;
        UIType.IsClickElsewhereClose = false;
        UIType.UIForm_LucencyType = UIFormLucencyTypes.Penetrable;
        UIType.UIForms_Type = UIFormTypes.Normal;
        UIType.UIForms_ShowMode = UIFormShowModes.Normal;

        TogglePanelButton.onClick.AddListener(OnTogglePanelButtonClick);

        LevelIDUpButton.onClick.AddListener(OnLevelIDUpButtonClick);
        LevelIDDownButton.onClick.AddListener(OnLevelIDDownButtonClick);

        SaveAsLevelStartButton.onClick.AddListener(OnSaveAsLevelStartButtonClick);
        SaveAsLevelGoalButton.onClick.AddListener(OnSaveAsLevelGoalButtonClick);
        LoadLevelButton.onClick.AddListener(OnLoadLevelButtonClick);
        SwapStartAndGoalButton.onClick.AddListener(SwapMaps);
        MapRoundsSlider.onValueChanged.AddListener(OnChangeMapRounds);

        MapGridTypeDropdown.options.Clear();
        foreach (string str in Enum.GetNames(typeof(MapGridTypes)))
        {
            MapGridTypeDropdown.options.Add(new Dropdown.OptionData(str));
        }

        MapGridTypeDropdown.onValueChanged.AddListener(OnChangeMapGridType);

        MapGridColorDropdown.options.Clear();
        foreach (string str in Enum.GetNames(typeof(MapGridColorTypes)))
        {
            MapGridColorDropdown.options.Add(new Dropdown.OptionData(str));
        }

        MapGridColorDropdown.onValueChanged.AddListener(OnChangeMapGridColorType);
    }

    void Start()
    {
        IsShow = false;
    }

    [SerializeField] private Button TogglePanelButton;
    [SerializeField] private GameObject Panel;

    [SerializeField] private InputField LevelIDInputField;
    [SerializeField] private Button LevelIDUpButton;
    [SerializeField] private Button LevelIDDownButton;

    [SerializeField] private InputField LevelNameInputField;

    [SerializeField] private InputField OptimumStepInputField;

    [SerializeField] private Button LoadLevelButton;
    [SerializeField] private Button SaveAsLevelStartButton;
    [SerializeField] private Button SaveAsLevelGoalButton;
    [SerializeField] private Button SwapStartAndGoalButton;

    [SerializeField] private Dropdown MapGridTypeDropdown;
    [SerializeField] private Dropdown MapGridColorDropdown;

    [SerializeField] private Slider MapRoundsSlider;
    [SerializeField] private Text MapRoundsText;

    private bool isShow = false;

    public bool IsShow
    {
        get { return isShow; }
        set
        {
            Panel.SetActive(value);
            isShow = value;
            GameManager.Instance.IsLevelEditorMode = value;
        }
    }

    private void OnTogglePanelButtonClick()
    {
        IsShow = !IsShow;
    }

    private void OnChangeMapGridType(int value)
    {
        GameManager.Instance.Map.CurrentMapGridType = (MapGridTypes) value;
    }

    private void OnChangeMapRounds(float value)
    {
        int mapRound = Mathf.RoundToInt(value);
        GameManager.Instance.Map.CachedLevelInfo.MapRounds = mapRound;
        GameManager.Instance.Map.ReloadLevel();
        GameManager.Instance.GoalMap.CachedLevelInfo.MapRounds = mapRound;
        GameManager.Instance.GoalMap.ReloadLevel();
        MapRoundsText.text = "Map Size: " + mapRound;
    }

    private void OnChangeMapGridColorType(int value)
    {
        GameManager.Instance.Map.CurrentMapGridColorType = (MapGridColorTypes) value;
    }

    private void OnLevelIDUpButtonClick()
    {
        if (int.TryParse(LevelIDInputField.text, out int levelID))
        {
            int newLevelID = levelID + 1;
            GameManager.Instance.LoadLevel(newLevelID);
        }
        else
        {
            GameManager.Instance.LoadLevel(0);
        }
    }

    private void OnLevelIDDownButtonClick()
    {
        if (int.TryParse(LevelIDInputField.text, out int levelID))
        {
            int newLevelID = levelID - 1;
            GameManager.Instance.LoadLevel(newLevelID);
        }
        else
        {
            GameManager.Instance.LoadLevel(0);
        }
    }

    public void OnLoadLevel(LevelInfo levelInfo)
    {
        LevelIDInputField.text = levelInfo.LevelID.ToString();
        OptimumStepInputField.text = levelInfo.OptimumStep.ToString();
        LevelNameInputField.text = levelInfo.LevelName;
        MapRoundsSlider.value = levelInfo.MapRounds;
    }

    public void OnLoadLevelButtonClick()
    {
        if (int.TryParse(LevelIDInputField.text, out int levelID))
        {
            GameManager.Instance.LoadLevel(levelID);
        }
    }

    public void OnSaveAsLevelStartButtonClick()
    {
        OnSave(MapTypes.Start);
    }

    public void OnSaveAsLevelGoalButtonClick()
    {
        OnSave(MapTypes.Goal);
    }

    private void OnSave(MapTypes mapTypes)
    {
        if (int.TryParse(LevelIDInputField.text, out int levelID))
        {
            if (int.TryParse(OptimumStepInputField.text, out int optimumStep))
            {
                ConfirmPanel cp = UIManager.Instance.ShowUIForms<ConfirmPanel>();
                cp.Initialize("Confirm to save?", "Yes", "No", delegate
                {
                    LevelInfo levelInfo = GameManager.Instance.Map.CurrentLevelInfo;
                    levelInfo.LevelID = levelID;
                    levelInfo.OptimumStep = optimumStep;
                    levelInfo.LevelName = LevelNameInputField.text;
                    if (mapTypes == MapTypes.Start)
                    {
                        levelInfo.StartMapInfo = GameManager.Instance.Map.CurrentMapInfo.Clone();
                    }
                    else
                    {
                        levelInfo.GoalMapInfo = GameManager.Instance.Map.CurrentMapInfo.Clone();
                    }

                    AllLevels.RefreshLevelXML(levelInfo);
                    cp.CloseUIForm();
                }, delegate { cp.CloseUIForm(); });
            }
            else
            {
                ConfirmPanel cp = UIManager.Instance.ShowUIForms<ConfirmPanel>();
                cp.Initialize("Please enter correct OptimumStep", "OK", null, delegate { cp.CloseUIForm(); }, null);
            }
        }
        else
        {
            ConfirmPanel cp = UIManager.Instance.ShowUIForms<ConfirmPanel>();
            cp.Initialize("Please enter correct Level ID", "OK", null, delegate { cp.CloseUIForm(); }, null);
        }
    }

    public void SwapMaps()
    {
        GameManager.Instance.Map.SwapShowStartAndGoal();
        GameManager.Instance.GoalMap.SwapShowStartAndGoal();
    }
}