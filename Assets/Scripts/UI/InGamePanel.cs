using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InGamePanel : BaseUIForm
{
    void Awake()
    {
        UIType.IsClearStack = false;
        UIType.IsESCClose = false;
        UIType.IsClickElsewhereClose = false;
        UIType.UIForm_LucencyType = UIFormLucencyTypes.Penetrable;
        UIType.UIForms_Type = UIFormTypes.Normal;
        UIType.UIForms_ShowMode = UIFormShowModes.Normal;

        RestartButton.onClick.AddListener(OnRestartButtonClick);
        UndoButton.onClick.AddListener(OnUndoButtonClick);
    }

    [SerializeField] private Text LevelNameText;
    [SerializeField] private Text StepUsedText;
    [SerializeField] private Text OptimumStepText;
    [SerializeField] private Button RestartButton;
    [SerializeField] private Button UndoButton;

    public void Init(LevelInfo levelInfo)
    {
        OptimumStep = levelInfo.OptimumStep;
        LevelNameText.text = "Level " + levelInfo.LevelID + ": " + levelInfo.LevelName;
        StepUsed = 0;
    }

    private int stepUsed;

    public int StepUsed
    {
        get { return stepUsed; }
        set
        {
            stepUsed = value;
            StepUsedText.text = "You use " + stepUsed + " steps.";
        }
    }

    private int optimumStep;

    public int OptimumStep
    {
        get { return optimumStep; }
        set
        {
            optimumStep = value;
            OptimumStepText.text = "Optimum: " + optimumStep + " steps.";
        }
    }

    private void OnRestartButtonClick()
    {
        GameManager.Instance.Map.ReloadLevel();
        GameManager.Instance.GoalMap.ReloadLevel();
        StepUsed = 0;
    }

    private void OnUndoButtonClick()
    {
        GameManager.Instance.Map.Undo();
    }
}