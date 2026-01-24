using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TaskDetailPanelUI : MonoBehaviour
{
    [Header("面板引用")]
    public GameObject detailPanel;
    public CanvasGroup canvasGroup;
    public Button confirmButton; // 确定/返回按钮

    [Header("任务基本信息")]
    public Image taskIcon;
    public TMP_Text taskNameText;
    public TMP_Text taskTypeText; // 日常/周常
    public TMP_Text unlockLevelText; // 解锁等级

    [Header("任务描述")]
    public TMP_Text taskDescriptionText;

    [Header("主要奖励显示")]
    public Image mainReward1Icon;
    public TMP_Text mainReward1AmountText;
    public Image mainReward2Icon;
    public TMP_Text mainReward2AmountText;

    [Header("详细奖励显示")]
    public TMP_Text detailReward1Text;
    public TMP_Text detailReward2Text;

    [Header("任务状态")]
    public TMP_Text taskStatusText;

    // 私有变量
    private TaskData currentTask;
    private TaskUIController uiController;
    private bool isAnimating = false;

    void Awake()
    {
        // 确保CanvasGroup存在
        if (canvasGroup == null && detailPanel != null)
        {
            canvasGroup = detailPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = detailPanel.AddComponent<CanvasGroup>();
            }
        }

        // 初始化面板状态
        if (detailPanel != null)
        {
            detailPanel.SetActive(true); // 保持激活状态
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 绑定按钮事件
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HidePanel);
        }
    }

    public void Initialize(TaskUIController controller)
    {
        uiController = controller;
    }

    public void ShowTaskDetail(TaskData task)
    {
        if (detailPanel == null || task == null) return;

        currentTask = task;
        UpdateTaskDetailUI(task);

        // 显示面板并播放动画
        if (!detailPanel.activeSelf)
            detailPanel.SetActive(true);

        // 设置CanvasGroup属性
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 显示面板并播放动画
        if (!isAnimating)
        {
            StartCoroutine(ShowPanelAnimation());
        }
    }

    void UpdateTaskDetailUI(TaskData task)
    {
        // 任务基本信息
        if (taskNameText != null)
            taskNameText.text = task.TaskName;

        if (taskDescriptionText != null)
            taskDescriptionText.text = task.Description;

        // 任务类型
        if (taskTypeText != null)
        {
            string typeText = "";
            switch (task.Frequency)
            {
                case TaskFrequency.Daily:
                    typeText = "Daily";
                    break;
                case TaskFrequency.Weekly:
                    typeText = "Weekly";
                    break;
                case TaskFrequency.Achievement:
                    typeText = "成就任务";
                    break;
                default:
                    typeText = "普通任务";
                    break;
            }
            taskTypeText.text = $"{typeText}";
        }

        // 解锁等级
        if (unlockLevelText != null)
            unlockLevelText.text = $"Lv.{task.UnlockLevel}";

        // 任务图标
        if (taskIcon != null)
        {
            taskIcon.sprite = GetTaskIcon(task);
        }

        // 主要奖励（图标）
        UpdateMainRewards(task);

        // 详细奖励（图标+数量）
        UpdateDetailRewards(task);

        // 任务状态
        UpdateTaskStatus(task);
    }

    void UpdateMainRewards(TaskData task)
    {
        // 奖励1
        if (mainReward1Icon != null && mainReward1AmountText != null)
        {
            if (task.Reward1.Type != RewardType.None)
            {
                mainReward1Icon.gameObject.SetActive(true);
                mainReward1Icon.sprite = GetRewardSprite(task.Reward1.Type);
                mainReward1AmountText.text = $"{task.Reward1.Amount}";
            }
            else
            {
                mainReward1Icon.gameObject.SetActive(false);
            }
        }

        // 奖励2
        if (mainReward2Icon != null && mainReward2AmountText != null)
        {
            if (task.Reward2.Type != RewardType.None)
            {
                mainReward2Icon.gameObject.SetActive(true);
                mainReward2Icon.sprite = GetRewardSprite(task.Reward2.Type);
                mainReward2AmountText.text = $"{task.Reward2.Amount}";
            }
            else
            {
                mainReward2Icon.gameObject.SetActive(false);
            }
        }
    }

    void UpdateDetailRewards(TaskData task)
    {
        // 奖励1详情
        if (detailReward1Text != null)
        {
            if (task.Reward1.Type != RewardType.None)
            {
                detailReward1Text.text = $"{GetRewardName(task.Reward1.Type)} × {task.Reward1.Amount}";
            }
            else
            {
                detailReward1Text.text = "";
            }
        }

        // 奖励2详情
        if (detailReward2Text != null)
        {
            if (task.Reward2.Type != RewardType.None)
            {
                detailReward2Text.text = $"{GetRewardName(task.Reward2.Type)} × {task.Reward2.Amount}";
            }
            else
            {
                detailReward2Text.text = "";
            }
        }
    }

    void UpdateTaskStatus(TaskData task)
    {
        if (taskStatusText == null) return;

        switch (task.Status)
        {
            case TaskStatus.Locked:
                taskStatusText.text = "状态: 未解锁";
                taskStatusText.color = Color.gray;
                break;
            case TaskStatus.Unlocked:
                taskStatusText.text = "状态: 进行中";
                taskStatusText.color = Color.blue;
                break;
            case TaskStatus.Completed:
                taskStatusText.text = "状态: 已完成";
                taskStatusText.color = Color.yellow;
                break;
            case TaskStatus.Claimed:
                taskStatusText.text = "状态: 已领取";
                taskStatusText.color = Color.green;
                break;
        }
    }

    Sprite GetTaskIcon(TaskData task)
    {
        // 详情面板使用更大的图标
        string iconName = "";

        if (!string.IsNullOrEmpty(task.SceneName))
        {
            switch (task.SceneName)
            {
                case "BattleScene":
                    iconName = "Icon_Battle";
                    break;
                case "MaterialScene":
                    iconName = "Icon_Material";
                    break;
                case "BossScene":
                    iconName = "Icon_Boss";
                    break;
                case "EquipmentScene":
                    iconName = "Icon_Equipment";
                    break;
            }
        }

        Debug.Log($"UI/Icons/{iconName}");
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
            case RewardType.DailyEXP: spriteName = "Icon_DailyEXP"; break;
            case RewardType.EXP: spriteName = "Icon_EXP"; break;
            case RewardType.Equipment: spriteName = "Icon_Equipment"; break;
            case RewardType.CharacterFragment: spriteName = "Icon_Fragment"; break;
            case RewardType.BattlePassEXP: spriteName = "Icon_BattlePass"; break;
            case RewardType.Materials: spriteName = "Icon_Material"; break;
            default: spriteName = "Icon_Default"; break;
        }

        Debug.Log($"UI/Icons/Large/{spriteName}");
        Sprite sprite = Resources.Load<Sprite>($"UI/Icons/{spriteName}");
        return sprite != null ? sprite : Resources.Load<Sprite>("UI/Icons/Icon_Default");
    }

    string GetRewardName(RewardType type)
    {
        switch (type)
        {
            case RewardType.Crystals: return "水晶";
            case RewardType.Coins: return "金币";
            case RewardType.Stamina: return "体力";
            case RewardType.DailyEXP: return "每日历练值";
            case RewardType.Equipment: return "装备箱";
            case RewardType.CharacterFragment: return "角色碎片";
            case RewardType.BattlePassEXP: return "作战凭证经验";
            case RewardType.Materials: return "材料";
            default: return "奖励";
        }
    }

    public void HidePanel()
    {
        if (detailPanel == null || !detailPanel.activeSelf) return;

        if (!isAnimating)
        {
            StartCoroutine(HidePanelAnimation());
        }
    }

    IEnumerator ShowPanelAnimation()
    {
        isAnimating = true;
        detailPanel.SetActive(true);

        float duration = 0.25f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        isAnimating = false;
    }

    IEnumerator HidePanelAnimation()
    {
        isAnimating = true;

        float duration = 0.15f;
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // 禁用交互，但不禁用GameObject
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        isAnimating = false;

        // 通知UI控制器面板已关闭
        if (uiController != null)
        {
            uiController.OnTaskDetailClosed();
        }
    }

    // 检查点击是否在面板上
    public bool IsPointerOverPanel()
    {
        return detailPanel.activeSelf;
    }
}