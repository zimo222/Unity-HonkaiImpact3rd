using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class TaskUIController : MonoBehaviour
{
    // ================== 基础玩家信息UI引用 ==================

    [Header("资源信息")]
    public TMP_Text HomogeneousPureCrystalText;

    // ================== 按钮引用 (可选) ==================
    [Header("按钮引用")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    // ================== 任务系统UI引用 ==================
    [Header("任务系统 - 左侧菜单")]
    public Button combatMissionButton;       // 作战任务按钮
    public Button combatRewardButton;        // 作战奖励按钮
    public Button combatShopButton;          // 作战商店按钮

    [Header("任务系统 - 右侧内容区域")]
    public GameObject combatMissionPanel;    // 作战任务面板
    public GameObject combatRewardPanel;     // 作战奖励面板
    public GameObject combatShopPanel;       // 作战商店面板

    [Header("任务列表")]
    public Transform missionListContent;     // 任务列表容器
    public GameObject missionItemPrefab;     // 任务项预制体

    [Header("作战任务")]
    public TMP_Text combatLevelText;         // 作战等级文本
    public TMP_Text combatEXPText;           // 作战经验文本
    public GameObject combatEXPSlider;       // 作战经验进度条
    public TMP_Text weekcombatEXPText;       // 本周作战经验文本

    [Header("每日历练值")]
    public TMP_Text dailyEXPText;            // 历练值文本
    public GameObject dailyEXPSlider;        // 历练值进度条
    public Button[] dailyRewardButtons;      // 四个档位奖励按钮
    private int[] crystalRewards = { 5, 5, 10, 10, 10 };
    private int[] thresholds = { 120, 240, 360, 480, 600 };

    // ================== 每日奖励按钮功能 ==================

    [Header("简单奖励弹窗")]
    public RewardDetailPanelUI rewardDetailPanel;
    public TMP_Text rewardAmountText;

    [Header("任务详情面板")]
    public TaskDetailPanelUI taskDetailPanel;

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;
    private List<MissionItemUI> missionItemUIs = new List<MissionItemUI>();

    void Start()
    {
        InitializeUI();
        InitializePanelFills();
        LoadPlayerData();
        UpdateAllUI();
        InitializeTaskSystem();

        // 自动绑定按钮事件
        SetupRewardButtonEvents();
    }

    void InitializeUI()
    {
        // 设置默认文本
        if (HomogeneousPureCrystalText != null) HomogeneousPureCrystalText.text = "0";
        if (combatLevelText != null) combatLevelText.text = "Lv.1";
        if (combatEXPText != null) combatEXPText.text = "0/1000";
        if (weekcombatEXPText != null) weekcombatEXPText.text = "0/10000";
    }

    void InitializePanelFills()
    {
        RectTransform transform1 = combatEXPSlider.GetComponent<RectTransform>();
        // 设置位置（假设锚点是在中心）
        transform1.anchoredPosition = new Vector2(934.0f, -93.5f);
        // 设置大小
        transform1.sizeDelta = new Vector2(0.0f, 11.0f);

        RectTransform transform2 = dailyEXPSlider.GetComponent<RectTransform>();
        // 设置位置（假设锚点是在中心）
        transform2.anchoredPosition = new Vector2(1545.0f, -96.0f);
        // 设置大小
        transform2.sizeDelta = new Vector2(0.0f, 11.0f);
    }

    void LoadPlayerData()
    {

        // 检查PlayerDataManager是否存在
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("PlayerDataManager.Instance 为空，加载默认数据");
            LoadDefaultData();
            UpdateAllUI();
            return;
        }

        // 获取当前玩家数据
        currentPlayerData = PlayerDataManager.Instance.CurrentPlayerData;

        // 检查是否成功获取玩家数据
        if (currentPlayerData == null)
        {
            Debug.LogWarning("当前没有登录的玩家，加载默认数据");
            LoadDefaultData();
            UpdateAllUI();
            return;
        }

        // 成功加载玩家数据
        Debug.Log($"成功加载玩家数据: {currentPlayerData.PlayerName}, 等级: {currentPlayerData.Level}, DailyEXP: {currentPlayerData.DailyEXP}");

        // 刷新玩家数据中的任务状态
        PlayerDataManager.Instance.RefreshTasks();

        UpdateAllUI();
    }

    void LoadDefaultData()
    {
        // 创建默认数据
        currentPlayerData = new PlayerData("舰长")
        {
            Level = 20,
            Experience = 25,
            Stamina = 120,
            Coins = 5000,
            Crystals = 1500,
            HomogeneousPureCrystal = 8,
            DailyEXP = 400
        };

        UpdateAllUI();
    }

    void UpdateAllUI()
    {
        if (currentPlayerData == null) return;
        UpdateResources();
        UpdatePanelFills();
    }

    void UpdateResources()
    {
        if (currentPlayerData == null) return;

        if (HomogeneousPureCrystalText != null)
            HomogeneousPureCrystalText.text = currentPlayerData.HomogeneousPureCrystal.ToString();

        if (combatLevelText != null)
            combatLevelText.text = "Lv." + currentPlayerData.CombatLevel.ToString();

        if (combatEXPText != null)
            combatEXPText.text = currentPlayerData.CombatEXP.ToString() + "/1000";

        if (weekcombatEXPText != null)
            weekcombatEXPText.text = currentPlayerData.WeekCombatEXP.ToString() + "/10000";

        for (int i = 0; i < dailyRewardButtons.Length; i++)
        {
            if (currentPlayerData.DailyEXPRewards[i].IsClaimed)
            {
                Transform iconTransform = dailyRewardButtons[i].transform.Find("Image");
                Sprite sprite = Resources.Load<Sprite>($"UI/Icons/Price_DailyEXP Claimed");
                Image childImage = iconTransform.GetComponent<Image>();
                Debug.Log($"UI/Icons/Price_DailyEXP Claimed");
                childImage.sprite = (sprite != null ? sprite : Resources.Load<Sprite>("UI/Icons/Icon_Default"));
            }
            else
            {
                Transform iconTransform = dailyRewardButtons[i].transform.Find("Image");
                Sprite sprite = Resources.Load<Sprite>($"UI/Icons/Price_DailyEXP UnClaim");
                Image childImage = iconTransform.GetComponent<Image>();
                Debug.Log($"UI/Icons/Price_DailyEXP UnClaim");
                childImage.sprite = (sprite != null ? sprite : Resources.Load<Sprite>("UI/Icons/Icon_Default"));
            }
        }
    }

    void UpdatePanelFills()
    {
        RectTransform transform1 = combatEXPSlider.GetComponent<RectTransform>();
        // 设置位置（假设锚点是在中心）
        transform1.anchoredPosition = new Vector2(-802 + 1432 * currentPlayerData.CombatEXP / 1000.0f / 2.0f, -76.0f);
        // 设置大小
        transform1.sizeDelta = new Vector2(1432 * currentPlayerData.CombatEXP / 1000.0f, 11.0f);

        RectTransform transform2 = dailyEXPSlider.GetComponent<RectTransform>();
        // 设置位置（假设锚点是在中心）
        transform2.anchoredPosition = new Vector2(730 + 1630 * currentPlayerData.DailyEXP / 600.0f / 2.0f, -96.0f);
        // 设置大小
        transform2.sizeDelta = new Vector2(1630 * currentPlayerData.DailyEXP / 600.0f, 11.0f);
    }

    // ================== 任务系统初始化 ==================
    void InitializeTaskSystem()
    {
        // 刷新任务状态
        if (currentPlayerData != null)
        {
            PlayerDataManager.Instance.RefreshTasks();

            // 调试：输出任务数据
            DebugTaskData();
        }

        // 设置按钮事件
        if (combatMissionButton != null)
        {
            combatMissionButton.onClick.AddListener(() => SwitchTaskPanel(0));
            // 设置初始选中状态
            SetButtonSelected(combatMissionButton, true);
        }

        if (combatRewardButton != null)
            combatRewardButton.onClick.AddListener(() => SwitchTaskPanel(1));

        if (combatShopButton != null)
            combatShopButton.onClick.AddListener(() => SwitchTaskPanel(2));

        // 初始化筛选按钮

        // 加载默认面板
        SwitchTaskPanel(0);

        // 初始化任务详情面板
        if (taskDetailPanel != null)
        {
            taskDetailPanel.Initialize(this);
        }
        
        // 添加背景点击关闭功能
        StartCoroutine(SetupBackgroundClick());
        UpdateDailyEXPDisplay();
    }

    void SwitchTaskPanel(int panelIndex)
    {
        // 重置所有按钮选中状态
        ResetButtonSelection();

        // 激活对应按钮的选中状态
        switch (panelIndex)
        {
            case 0: // 作战任务
                SetButtonSelected(combatMissionButton, true);
                ShowPanel(combatMissionPanel);
                // 默认加载所有任务
                LoadAllTasks(); // 加载所有任务
                UpdateDailyEXPDisplay();
                break;
            case 1: // 作战奖励
                SetButtonSelected(combatRewardButton, true);
                ShowPanel(combatRewardPanel);
                UpdateDailyEXPDisplay();
                break;
            case 2: // 作战商店
                SetButtonSelected(combatShopButton, true);
                ShowPanel(combatShopPanel);
                // LoadShopItems(); // 商店功能暂未实现
                break;
        }
    }

    void SetButtonSelected(Button button, bool selected)
    {
        if (button == null) return;

        var buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = selected ? new Color(0.5f, 0.8f, 1f, 1f) : new Color(0.125f, 0.25f, 0.5f, 1f);
        }
    }

    void ResetButtonSelection()
    {
        SetButtonSelected(combatMissionButton, false);
        SetButtonSelected(combatRewardButton, false);
        SetButtonSelected(combatShopButton, false);
    }

    void ShowPanel(GameObject panel)
    {
        if (combatMissionPanel != null) combatMissionPanel.SetActive(false);
        if (combatRewardPanel != null) combatRewardPanel.SetActive(false);
        if (combatShopPanel != null) combatShopPanel.SetActive(false);

        if (panel != null) panel.SetActive(true);
    }

    // ================== 任务列表管理 ==================
    public void LoadAllTasks()
    {
        if (currentPlayerData == null || missionListContent == null || missionItemPrefab == null)
            return;

        // 清除现有任务项
        ClearMissionList();

        // 获取所有任务（不筛选频率）
        List<TaskData> allTasks = PlayerDataManager.Instance.GetSortedTasks(null);

        // 调试信息
        Debug.Log($"=== 加载所有任务 ===");
        Debug.Log($"任务总数: {allTasks.Count}");
        Debug.Log($"日常任务: {allTasks.FindAll(t => t.Frequency == TaskFrequency.Daily).Count}");
        Debug.Log($"周常任务: {allTasks.FindAll(t => t.Frequency == TaskFrequency.Weekly).Count}");

        // 创建任务项
        int itemCount = 0;
        foreach (TaskData task in allTasks)
        {
            CreateMissionItem(task);
            itemCount++;

            // 调试每个任务
            Debug.Log($"创建任务项: {task.TaskName} ({task.Frequency}) - 状态: {task.Status}");
        }

        // 强制布局重建（如果需要）
        StartCoroutine(RebuildLayout());
    }

    void ClearMissionList()
    {
        // 清除现有任务项
        for (int i = missionListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(missionListContent.GetChild(i).gameObject);
        }
        missionItemUIs.Clear();
    }

    void CreateMissionItem(TaskData task)
    {
        GameObject missionItemObj = Instantiate(missionItemPrefab, missionListContent);
        missionItemObj.name = $"MissionItem_{task.TaskID}_{task.Frequency}";

        // 确保预制体的RectTransform正确
        RectTransform rectTransform = missionItemObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }

        MissionItemUI missionItemUI = missionItemObj.GetComponent<MissionItemUI>();

        if (missionItemUI != null)
        {
            missionItemUI.Initialize(task, this);
            missionItemUIs.Add(missionItemUI);
        }
        else
        {
            Debug.LogError("MissionItemPrefab上没有MissionItemUI组件！");
        }
    }

    IEnumerator RebuildLayout()
    {
        // 等待一帧让Unity更新布局
        yield return null;

        // 强制重建布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(missionListContent as RectTransform);
    }

    public void OnMissionItemClicked(string taskId)
    {
        Debug.Log($"任务被点击: {taskId}");

        if (currentPlayerData == null) return;

        TaskData task = currentPlayerData.Tasks.Find(t => t.TaskID == taskId);
        if (task == null) return;

        switch (task.Status)
        {
            case TaskStatus.Unlocked:
                // 跳转到任务场景
                if (!string.IsNullOrEmpty(task.SceneName))
                {
                    SceneManager.LoadScene(task.SceneName);
                }
                break;

            case TaskStatus.Completed:
                // 领取奖励
                if (PlayerDataManager.Instance.ClaimTaskReward(taskId))
                {
                    // 刷新界面
                    LoadAllTasks();
                    UpdateDailyEXPDisplay();
                    UpdateAllUI(); // 更新资源显示
                }
                break;
        }
    }

    public void ShowTaskDetail(TaskData task)
    {
        if (taskDetailPanel != null)
        {
            taskDetailPanel.ShowTaskDetail(task);
        }
    }

    public void OnTaskDetailClosed()
    {
        // 详情面板关闭后可以执行一些清理操作
        Debug.Log("任务详情面板已关闭");
    }

    public void OnRewardDetailClosed()
    {
        // 详情面板关闭后可以执行一些清理操作
        Debug.Log("任务详情面板已关闭");
    }

    // ================== 每日历练值管理 ==================
    public void UpdateDailyEXPDisplay()
    {
        if (dailyEXPText == null)
        {
            Debug.LogError("dailyEXPText 为空！");
            return;
        }

        if (currentPlayerData == null)
        {
            Debug.LogWarning("UpdateDailyEXPDisplay: currentPlayerData 为空");
            return;
        }

        // 调试信息
        Debug.Log($"更新每日历练值显示: {currentPlayerData.DailyEXP}/600");

        // 更新文本
        dailyEXPText.text = $"{currentPlayerData.DailyEXP}";
        combatLevelText.text = $"Lv.{currentPlayerData.CombatLevel}";
        combatEXPText.text = $"{currentPlayerData.CombatEXP}/1000";

        Debug.Log($"{currentPlayerData.DailyEXP}");

        // 检查奖励按钮数组
        if (dailyRewardButtons == null || dailyRewardButtons.Length == 0)
        {
            Debug.LogWarning("dailyRewardButtons 未配置");
            return;
        }

        // 更新奖励项状态
        for (int i = 0; i < dailyRewardButtons.Length && i < currentPlayerData.DailyEXPRewards.Count; i++)
        {
            var reward = currentPlayerData.DailyEXPRewards[i];
            bool isClaimed = reward.IsClaimed;
            bool canClaim = currentPlayerData.DailyEXP >= reward.RequiredEXP && !isClaimed;

            // 调试信息
            Debug.Log($"奖励 {i}: 需要 {reward.RequiredEXP} EXP, 已领取: {isClaimed}, 可领取: {canClaim}");

            // 设置按钮交互状态
            if (dailyRewardButtons[i] != null)
            {
                /*
                dailyRewardButtons[i].interactable = canClaim;
                */
            }
            else
            {
                Debug.LogWarning($"dailyRewardButtons[{i}] 为空");
            }
        }
    }

    // ================== 调试功能 ==================
    void DebugTaskData()
    {
        if (currentPlayerData == null) return;

        Debug.Log($"=== 玩家任务数据调试 ===");
        Debug.Log($"玩家等级: {currentPlayerData.Level}");
        Debug.Log($"任务总数: {currentPlayerData.Tasks.Count}");

        int dailyCount = 0;
        int weeklyCount = 0;
        int unlockedCount = 0;
        int lockedCount = 0;
        int completedCount = 0;
        int claimedCount = 0;

        foreach (var task in currentPlayerData.Tasks)
        {
            if (task.Frequency == TaskFrequency.Daily) dailyCount++;
            if (task.Frequency == TaskFrequency.Weekly) weeklyCount++;

            switch (task.Status)
            {
                case TaskStatus.Locked: lockedCount++; break;
                case TaskStatus.Unlocked: unlockedCount++; break;
                case TaskStatus.Completed: completedCount++; break;
                case TaskStatus.Claimed: claimedCount++; break;
            }

            Debug.Log($"任务: {task.TaskName}");
            Debug.Log($"  ID: {task.TaskID}");
            Debug.Log($"  类型: {task.Frequency}");
            Debug.Log($"  状态: {task.Status}");
            Debug.Log($"  解锁等级: {task.UnlockLevel}");
            Debug.Log($"  当前等级是否满足: {currentPlayerData.Level >= task.UnlockLevel}");
        }

        Debug.Log($"=== 统计 ===");
        Debug.Log($"日常任务: {dailyCount}");
        Debug.Log($"周常任务: {weeklyCount}");
        Debug.Log($"未解锁: {lockedCount}");
        Debug.Log($"已解锁: {unlockedCount}");
        Debug.Log($"已完成: {completedCount}");
        Debug.Log($"已领取: {claimedCount}");
    }
    
    // ================== 背景点击关闭面板 ==================
    IEnumerator SetupBackgroundClick()
    {
        yield return new WaitForSeconds(0.1f);

        // 添加背景点击事件
        if (taskDetailPanel != null && taskDetailPanel.detailPanel != null)
        {
            // 创建一个透明的全屏按钮作为背景
            GameObject bgClickObj = new GameObject("DetailPanelBackground");
            RectTransform bgRect = bgClickObj.AddComponent<RectTransform>();
            bgRect.SetParent(taskDetailPanel.detailPanel.transform.parent);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bgRect.SetAsFirstSibling(); // 放在最下面

            Image bgImage = bgClickObj.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0f); // 半透明黑色背景
            bgImage.raycastTarget = true;

            Button bgButton = bgClickObj.AddComponent<Button>();
            bgButton.transition = Selectable.Transition.None;
            bgButton.onClick.AddListener(() => {
                if (taskDetailPanel != null && taskDetailPanel.detailPanel.activeSelf)
                {
                    taskDetailPanel.HidePanel();
                }
            });

            // 初始隐藏
            bgClickObj.SetActive(false);

            // 监听面板显示/隐藏状态
            StartCoroutine(MonitorPanelState(bgClickObj));
        }
    }

    IEnumerator MonitorPanelState(GameObject bgObject)
    {
        while (true)
        {
            if (taskDetailPanel != null && bgObject != null)
            {
                bool shouldShow = taskDetailPanel.detailPanel != null &&
                                taskDetailPanel.detailPanel.activeSelf;

                if (bgObject.activeSelf != shouldShow)
                {
                    bgObject.SetActive(shouldShow);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // ================== 示例：如何通过脚本与模块化按钮交互 ==================
    /// <summary>
    /// 示例：动态禁用一个按钮。
    /// </summary>
    public void DisableButtonByName(string targetButtonName)
    {
        foreach (var button in referencedButtons)
        {
            if (button.buttonName == targetButtonName)
            {
                button.GetComponent<Button>().interactable = false;
                Debug.Log($"已禁用按钮: {targetButtonName}");
                break;
            }
        }
    }

    /// <summary>
    /// 示例：动态改变一个按钮的行为。
    /// </summary>
    public void ChangeButtonToOpenDifferentPanel(string targetButtonName, GameObject newPanel)
    {
        foreach (var modularButton in referencedButtons)
        {
            if (modularButton.buttonName == targetButtonName)
            {
                modularButton.SetActionType(ModularUIButton.ButtonAction.OpenPanel);
                modularButton.SetPanelToOpen(newPanel);
                Debug.Log($"已修改按钮 '{targetButtonName}' 行为，将打开新面板: {newPanel.name}");
                break;
            }
        }
    }
    public void RefreshAllUI()
    {
        // 重新加载玩家数据
        LoadPlayerData();

        // 更新所有UI
        UpdateAllUI();

        // 更新每日历练值显示
        UpdateDailyEXPDisplay();

        // 刷新任务列表
        LoadAllTasks();

        Debug.Log("TaskUIController: 所有UI已刷新");
    }

    // 显示奖励弹窗的方法
    public void ShowRewardPopup(TaskReward rewarda, TaskReward rewardb)
    {
        if (rewardDetailPanel != null)
        {
            rewardDetailPanel.ShowRewardDetail(rewarda, rewardb);

        Debug.Log("111");
            // 3秒后自动关闭弹窗
            StartCoroutine(AutoClosePopup());
        }
    }

    // 自动关闭弹窗的协程
    IEnumerator AutoClosePopup()
    {
        yield return new WaitForSeconds(2);
        if (rewardDetailPanel != null)
        {
            rewardDetailPanel.HidePanel();
        }
    }

    // 手动关闭弹窗的方法（在弹窗按钮上调用）
    public void CloseRewardPopup()
    {
        if (rewardDetailPanel != null)
        {
            rewardDetailPanel.HidePanel();
        }
    }
    // ================== 每日奖励按钮功能 ==================

    void SetupRewardButtonEvents()
    {
        // 检查按钮数组是否已设置
        if (dailyRewardButtons == null || dailyRewardButtons.Length != 5)
        {
            Debug.LogError("dailyRewardButtons数组未正确设置！应有5个按钮。");
            return;
        }

        // 为每个按钮绑定事件
        for (int i = 0; i < dailyRewardButtons.Length; i++)
        {
            if (dailyRewardButtons[i] == null)
            {
                Debug.LogWarning($"dailyRewardButtons[{i}]为空！");
                continue;
            }

            // 清除旧的事件监听器
            dailyRewardButtons[i].onClick.RemoveAllListeners();

            // 根据索引绑定对应的方法
            int index = i;
            dailyRewardButtons[i].onClick.AddListener(() => OnRewardButtonClicked(index));
            Debug.Log(index); 
        }

        Debug.Log("奖励按钮事件已自动绑定");
    }

    // 通用的奖励按钮点击方法
    void OnRewardButtonClicked(int buttonIndex)
    {
        if (currentPlayerData == null)
        {
            Debug.LogError("当前没有玩家数据！");
            return;
        }

        // 检查索引范围
        if (buttonIndex < 0 || buttonIndex >= thresholds.Length)
        {
            Debug.LogError($"无效的按钮索引: {buttonIndex}");
            return;
        }

        int requiredEXP = thresholds[buttonIndex];
        int crystalAmount = crystalRewards[buttonIndex];

        // 检查是否达到阈值
        if (currentPlayerData.DailyEXP >= requiredEXP)
        {
            if (currentPlayerData.DailyEXPRewards[buttonIndex].IsClaimed)
            {
                Debug.Log("已领取");
                return;
            }
            // 给予水晶奖励
            PlayerDataManager.Instance.ClaimDailyEXPReward(buttonIndex);
            // 显示奖励弹窗
            ShowRewardPopup(currentPlayerData.DailyEXPRewards[buttonIndex].Reward1, currentPlayerData.DailyEXPRewards[buttonIndex].Reward2);

            // 保存数据
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SaveCurrentPlayerData();
            }
            // 更新UI
            UpdateAllUI();

            Debug.Log($"成功领取奖励！获得水晶: {crystalAmount}");
        }
        else
        {
            Debug.Log($"每日历练值不足：需要{requiredEXP}，当前{currentPlayerData.DailyEXP}");
        }
    }
}