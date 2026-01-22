using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionItemUI : MonoBehaviour
{
    [Header("任务项UI组件")]
    public Image taskIcon;
    public TMP_Text taskNameText;
    public Image reward1Icon;
    public TMP_Text reward1AmountText;
    public Image reward2Icon;
    public TMP_Text reward2AmountText;
    public Button actionButton;
    public TMP_Text actionButtonText;
    public TMP_Text statusText; // 显示"未解锁"/"已完成"状态

    [Header("点击区域")]
    public Button itemButton; // 整个任务项的点击区域

    // 私有变量
    private string taskId;
    private TaskUIController uiController;
    private TaskData taskData;

    public void Initialize(TaskData task, TaskUIController controller)
    {
        taskId = task.TaskID;
        uiController = controller;
        taskData = task;

        // 更新UI元素
        UpdateTaskItemUI(task);

        // 绑定事件
        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }
    }

    void UpdateTaskItemUI(TaskData task)
    {
        // 任务名称
        if (taskNameText != null)
            taskNameText.text = task.TaskName;

        // 更新奖励显示
        UpdateRewardDisplay(task.Reward1, reward1Icon, reward1AmountText);
        UpdateRewardDisplay(task.Reward2, reward2Icon, reward2AmountText);

        // 根据任务状态设置UI
        UpdateTaskStatusUI(task);

        // 设置任务图标
        if (taskIcon != null)
        {
            taskIcon.sprite = GetTaskIcon(task);
        }
    }

    void UpdateRewardDisplay(TaskReward reward, Image icon, TMP_Text amountText)
    {
        if (reward.Type == RewardType.None || icon == null || amountText == null)
        {
            if (icon != null) icon.gameObject.SetActive(false);
            if (amountText != null) amountText.gameObject.SetActive(false);
            return;
        }

        icon.gameObject.SetActive(true);
        amountText.gameObject.SetActive(true);

        // 设置奖励图标
        icon.sprite = GetRewardSprite(reward.Type);
        amountText.text = $"{reward.Amount}";
    }

    void UpdateTaskStatusUI(TaskData task)
    {
        // 更新状态文本和按钮状态
        if (statusText != null)
        {
            switch (task.Status)
            {
                case TaskStatus.Locked:
                    statusText.text = "Locked";
                    statusText.color = Color.gray;
                    break;
                case TaskStatus.Unlocked:
                    statusText.text = "";
                    break;
                case TaskStatus.Completed:
                    statusText.text = "Completed";
                    statusText.color = Color.yellow;
                    break;
                case TaskStatus.Claimed:
                    statusText.text = "Completed";
                    statusText.color = Color.green;
                    break;
            }
        }

        // 设置按钮文本和状态
        if (actionButtonText != null)
        {
            switch (task.Status)
            {
                case TaskStatus.Locked:
                    actionButtonText.text = "Locked";
                    break;
                case TaskStatus.Unlocked:
                    actionButtonText.text = "Action";
                    break;
                case TaskStatus.Completed:
                    actionButtonText.text = "Over";
                    break;
                case TaskStatus.Claimed:
                    actionButtonText.text = "已领取";
                    break;
            }
        }

        // 设置按钮交互状态
        if (actionButton != null)
        {
            actionButton.interactable = (task.Status == TaskStatus.Unlocked || task.Status == TaskStatus.Completed);
        }
    }

    Sprite GetTaskIcon(TaskData task)
    {
        // 根据任务类型返回不同图标
        string iconName = "";

        switch (task.Frequency)
        {
            case TaskFrequency.Daily:
                iconName = "Icon_Daily";
                break;
            case TaskFrequency.Weekly:
                iconName = "Icon_Weekly";
                break;
            default:
                iconName = "Icon_Default";
                break;
        }

        // 也可以根据任务场景或类型设置不同图标
        if (!string.IsNullOrEmpty(task.BattleType))
        {
            switch (task.BattleType)
            {
                case "Normal":
                    iconName = "Icon_Battle_Normal";
                    break;
                case "Boss":
                    iconName = "Icon_Battle_Boss";
                    break;
                case "Material":
                    iconName = "Icon_Material";
                    break;
            }
        }

        Sprite sprite = Resources.Load<Sprite>($"UI/Icons/{iconName}");
        return sprite != null ? sprite : Resources.Load<Sprite>("UI/Icons/Icon_Default");
    }

    Sprite GetRewardSprite(RewardType type)
    {
        string spriteName = "";
        switch (type)
        {
            case RewardType.Crystals: spriteName = "Icon_Crystal"; break;
            case RewardType.Coins: spriteName = "Icon_Coin"; break;
            case RewardType.Stamina: spriteName = "Icon_Stamina"; break;
            case RewardType.DailyEXP: spriteName = "Icon_EXP"; break;
            case RewardType.Equipment: spriteName = "Icon_Equipment"; break;
            case RewardType.CharacterFragment: spriteName = "Icon_Fragment"; break;
            case RewardType.BattlePassEXP: spriteName = "Icon_BattlePass"; break;
            case RewardType.Materials: spriteName = "Icon_Material"; break;
            default: spriteName = "Icon_Default"; break;
        }

        Sprite sprite = Resources.Load<Sprite>($"UI/Icons/{spriteName}");
        return sprite != null ? sprite : Resources.Load<Sprite>("UI/Icons/Icon_Default");
    }

    void OnItemClicked()
    {
        // 点击任务项，显示任务详情面板
        if (uiController != null && taskData != null)
        {
            uiController.ShowTaskDetail(taskData);
        }
    }

    void OnActionButtonClicked()
    {
        // 点击前往/领取按钮，执行任务操作
        if (uiController != null)
        {
            uiController.OnMissionItemClicked(taskId);
        }
    }

    // 当任务状态变化时更新UI
    public void RefreshUI()
    {
        if (taskData != null)
        {
            UpdateTaskItemUI(taskData);
        }
    }
}