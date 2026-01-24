using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

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
    public TMP_Text statusText;

    [Header("点击区域")]
    public Button itemButton;

    [Header("ModularUIButton配置")]
    public ModularUIButton modularUIButton;

    // 私有变量
    private string taskId;
    private TaskUIController uiController;
    private TaskData taskData;

    void Start()
    {
        // 确保ModularUIButton存在
        EnsureModularUIButton();
    }

    void EnsureModularUIButton()
    {
        if (modularUIButton == null && actionButton != null)
        {
            modularUIButton = actionButton.GetComponent<ModularUIButton>();
            if (modularUIButton == null)
            {
                modularUIButton = actionButton.gameObject.AddComponent<ModularUIButton>();
            }
        }
    }

    public void Initialize(TaskData task, TaskUIController controller)
    {
        taskId = task.TaskID;
        uiController = controller;

        // 确保taskData引用的是PlayerData中的实际对象，而不是副本
        if (PlayerDataManager.Instance?.CurrentPlayerData != null)
        {
            // 从当前玩家数据中获取最新的任务引用
            var playerTask = PlayerDataManager.Instance.CurrentPlayerData.Tasks.Find(t => t.TaskID == taskId);
            if (playerTask != null)
            {
                taskData = playerTask;
            }
            else
            {
                taskData = task;
            }
        }
        else
        {
            taskData = task;
        }

        // 确保ModularUIButton存在
        EnsureModularUIButton();

        // 更新UI元素
        UpdateTaskItemUI(taskData);

        // 绑定事件
        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }

        // 根据任务状态设置ModularUIButton
        SetupModularUIButton(taskData);
    }

    void SetupModularUIButton(TaskData task)
    {
        if (modularUIButton == null) return;

        // 根据任务状态配置ModularUIButton
        switch (task.Status)
        {
            case TaskStatus.Locked:
                modularUIButton.actionType = ModularUIButton.ButtonAction.None;
                modularUIButton.buttonName = $"Locked_{task.TaskName}";

                // 确保onCustomEvent为空
                modularUIButton.onCustomEvent = new UnityEvent();
                break;

            case TaskStatus.Unlocked:
                modularUIButton.actionType = ModularUIButton.ButtonAction.SwitchScene;
                modularUIButton.buttonName = $"GoTo_{task.TaskName}";

                // 设置目标场景
                string sceneName = !string.IsNullOrEmpty(task.SceneName) ? task.SceneName : FormatSceneName(task.TaskName);
                modularUIButton.targetSceneName = sceneName;
                modularUIButton.sceneLoadMode = UnityEngine.SceneManagement.LoadSceneMode.Single;

                // 确保onCustomEvent为空
                modularUIButton.onCustomEvent = new UnityEvent();
                break;

            case TaskStatus.Completed:
                modularUIButton.actionType = ModularUIButton.ButtonAction.CustomEvent;
                modularUIButton.buttonName = $"Claim_{task.TaskName}";

                // 创建一个新的UnityEvent
                modularUIButton.onCustomEvent = new UnityEvent();

                // 添加监听器 - 这里很关键！
                // 我们需要创建一个新的Action来调用ClaimTaskRewards
                modularUIButton.onCustomEvent.AddListener(ClaimTaskRewards);
                break;

            case TaskStatus.Claimed:
                modularUIButton.actionType = ModularUIButton.ButtonAction.None;
                modularUIButton.buttonName = $"Claimed_{task.TaskName}";
                modularUIButton.onCustomEvent = new UnityEvent();
                break;
        }

        // 设置其他通用配置
        modularUIButton.requireConfirmation = false;
        modularUIButton.fadeDuration = 0.2f;

        // 强制刷新Inspector显示（仅在编辑器中）
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(modularUIButton);
#endif
    }

    string FormatSceneName(string taskName)
    {
        string formattedName = taskName.Replace(" ", "").Replace(":", "").Replace("-", "");
        return $"{formattedName}Scene";
    }

    void ClaimTaskRewards()
    {
        if (taskData == null)
        {
            Debug.LogError($"ClaimTaskRewards: taskData为空");
            return;
        }

        Debug.Log($"正在领取任务奖励: {taskData.TaskName}");

        // 检查PlayerDataManager是否已初始化
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError($"PlayerDataManager.Instance为空");
            return;
        }

        // 检查当前玩家数据
        if (PlayerDataManager.Instance.CurrentPlayerData == null)
        {
            Debug.LogError($"当前没有登录的玩家");
            return;
        }

        // 使用PlayerData中已有的ClaimTaskReward方法
        bool success = PlayerDataManager.Instance.CurrentPlayerData.ClaimTaskReward(taskId);

        if (success)
        {
            // 更新本地任务数据状态
            taskData.Status = TaskStatus.Claimed;

            // 调试：显示数据变化
            PlayerData playerData = PlayerDataManager.Instance.CurrentPlayerData;
            Debug.Log($"奖励领取成功！玩家数据更新：");
            Debug.Log($"  - 水晶: {playerData.Crystals}");
            Debug.Log($"  - 金币: {playerData.Coins}");
            Debug.Log($"  - 体力: {playerData.Stamina}");
            Debug.Log($"  - 每日历练值: {playerData.DailyEXP}");

            // 保存数据到磁盘 - 这是关键！
            PlayerDataManager.Instance.SaveCurrentPlayerData();
            Debug.Log($"玩家数据已保存到磁盘");

            // 通知UI控制器更新整个界面
            if (uiController != null)
            {
                // 使用反射或修改TaskUIController以提供公共方法
                uiController.RefreshAllUI();
                uiController.UpdateDailyEXPDisplay();
                uiController.LoadAllTasks();

                // 同时调用OnMissionItemClicked以保持原有逻辑
                uiController.OnMissionItemClicked(taskId);
            }
            else
            {
                Debug.LogWarning("uiController为空，无法刷新UI");
            }

            // 刷新UI显示
            RefreshUI();

            Debug.Log($"奖励领取成功！任务 {taskData.TaskName} 已标记为已领取");
        }
        else
        {
            Debug.LogWarning($"领取任务 {taskData.TaskName} 奖励失败");
        }
    }

    void UpdateTaskItemUI(TaskData task)
    {
        if (taskNameText != null)
            taskNameText.text = task.TaskName;

        UpdateRewardDisplay(task.Reward1, reward1Icon, reward1AmountText);
        UpdateRewardDisplay(task.Reward2, reward2Icon, reward2AmountText);

        UpdateTaskStatusUI(task);

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

        icon.sprite = GetRewardSprite(reward.Type);
        amountText.text = $"{reward.Amount}";
    }

    void UpdateTaskStatusUI(TaskData task)
    {
        if (statusText != null)
        {
            switch (task.Status)
            {
                case TaskStatus.Locked:
                    statusText.text = "Locked";
                    statusText.color = Color.gray;
                    break;
                case TaskStatus.Unlocked:
                    statusText.text = task.nowTimes + "/" + task.maxTimes;
                    statusText.color = Color.white;
                    break;
                case TaskStatus.Completed:
                    statusText.text = task.maxTimes + "/" + task.maxTimes;
                    statusText.color = Color.white;
                    break;
                case TaskStatus.Claimed:
                    statusText.text = "Completed";
                    statusText.color = Color.green;
                    break;
            }
        }

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
                    actionButtonText.text = "Receive";
                    break;
                case TaskStatus.Claimed:
                    actionButtonText.text = "Received";
                    break;
            }
        }

        if (actionButton != null)
        {
            actionButton.interactable = (task.Status == TaskStatus.Unlocked || task.Status == TaskStatus.Completed);
        }
    }

    Sprite GetTaskIcon(TaskData task)
    {
        string iconName = "";

        switch (task.Frequency)
        {
            case TaskFrequency.Daily:
                iconName = "Icon_Daily";
                break;
            case TaskFrequency.Weekly:
                iconName = "Icon_Weekly";
                break;
            case TaskFrequency.Achievement:
                iconName = "Icon_Achievement";
                break;
            default:
                iconName = "Icon_Default";
                break;
        }

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

        iconName = task.TaskName;
        Sprite sprite = Resources.Load<Sprite>($"UI/Icons/Task_{iconName}");
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

        Sprite sprite = Resources.Load<Sprite>($"UI/Icons/{spriteName}");
        return sprite != null ? sprite : Resources.Load<Sprite>("UI/Icons/Icon_Default");
    }

    void OnItemClicked()
    {
        if (uiController != null && taskData != null)
        {
            uiController.ShowTaskDetail(taskData);
        }
    }

    public void RefreshUI()
    {
        if (taskData != null)
        {
            UpdateTaskItemUI(taskData);
            // 重新设置ModularUIButton
            SetupModularUIButton(taskData);
        }
    }
}