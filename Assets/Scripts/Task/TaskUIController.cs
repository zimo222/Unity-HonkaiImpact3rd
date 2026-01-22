using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class TaskUIController : MonoBehaviour
{
    // ================== 基础玩家信息UI引用 ==================
    [Header("玩家信息")]
    public TMP_Text playerNameText;
    public TMP_Text playerLevelText;
    public TMP_Text playerExperienceText;
    public Slider experienceSlider;

    [Header("资源信息")]
    public TMP_Text tiliText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ================== 按钮引用 (可选) ==================
    [Header("按钮引用 (如果你需要通过脚本访问它们)")]
    [Tooltip("你可以在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    // ================== 任务系统UI引用 ==================
    [Header("任务系统 - 左侧菜单")]
    public Button combatMissionButton;      // 作战任务按钮
    public Button combatRewardButton;       // 作战奖励按钮
    public Button combatShopButton;         // 作战商店按钮

    [Header("任务系统 - 右侧内容区域")]
    public GameObject combatMissionPanel;    // 作战任务面板
    public GameObject combatRewardPanel;     // 作战奖励面板
    public GameObject combatShopPanel;       // 作战商店面板

    [Header("任务列表")]
    public Transform missionListContent;     // 任务列表容器
    public GameObject missionItemPrefab;     // 任务项预制体
    public TMP_Text missionTitleText;        // 任务标题
    public TMP_Text dailyRefreshText;        // 每日刷新时间文本
    public TMP_Text weeklyRefreshText;       // 每周刷新时间文本

    [Header("每日历练值")]
    public Slider dailyEXPSlider;            // 历练值进度条
    public TMP_Text dailyEXPText;            // 历练值文本
    public Button[] dailyRewardButtons;      // 四个档位奖励按钮
    public Image[] dailyRewardIcons;         // 奖励图标
    public TMP_Text[] dailyRewardAmounts;    // 奖励数量
    public GameObject[] dailyRewardClaimed;  // 已领取标记
    public TMP_Text[] dailyRewardThresholds; // 奖励阈值文本

    [Header("任务详情面板")]
    public TaskDetailPanelUI taskDetailPanel;

    // ================== 任务筛选功能 ==================
    [Header("任务筛选")]
    public Button filterAllButton;
    public Button filterDailyButton;
    public Button filterWeeklyButton;
    public TMP_Text currentFilterText;

    private TaskFilter currentFilter = TaskFilter.All;

    private enum TaskFilter
    {
        All,
        Daily,
        Weekly
    }

    // ================== 其他原有系统（保持不变） ==================
    [Header("Panel填充控制")]
    public PanelFillData[] panelFillControllers;    // Panel填充控制器数组

    [System.Serializable]
    public class PanelFillData
    {
        public string fillName;                     // 填充名称（用于标识）
        public GameObject targetPanel;              // 目标Panel对象
        public float fillAmount;                    // 当前填充值
        [Range(0f, 1f)] public float minFill = 0f;  // 最小填充值
        [Range(0f, 1f)] public float maxFill = 1f;  // 最大填充值
        public bool useCustomFill = false;          // 是否使用自定义填充值
        public float customFillValue = 0.5f;        // 自定义填充值

        [HideInInspector] public Image panelImage;  // Panel的Image组件
        [HideInInspector] public bool isInitialized = false; // 是否已初始化
    }

    [Header("确认窗口")]
    public GameObject confirmationWindow;           // 确认窗口
    public TMP_Text confirmationText;               // 确认文本
    private System.Action pendingAction;            // 待执行的动作

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;
    private bool isDataLoaded = false;
    private List<MissionItemUI> missionItemUIs = new List<MissionItemUI>();

    void Start()
    {
        InitializeUI();
        LoadPlayerData();
        InitializeTaskSystem();

        // 初始化Panel填充
        InitializePanelFills();
    }

    void InitializeUI()
    {
        // 设置默认文本
        if (playerNameText != null) playerNameText.text = "加载中...";
        if (playerLevelText != null) playerLevelText.text = "Lv.0";
        if (playerExperienceText != null) playerExperienceText.text = "0/100";
        if (tiliText != null) tiliText.text = "0";
        if (coinsText != null) coinsText.text = "0";
        if (crystalsText != null) crystalsText.text = "0";

        // 初始化经验条
        if (experienceSlider != null)
        {
            experienceSlider.minValue = 0;
            experienceSlider.maxValue = 100;
            experienceSlider.value = 0;
        }

        // 初始化确认窗口
        if (confirmationWindow != null)
            confirmationWindow.SetActive(false);

        // 设置任务系统默认标题
        if (missionTitleText != null)
            missionTitleText.text = "作战任务";
    }

    void InitializePanelFills()
    {
        foreach (PanelFillData fillData in panelFillControllers)
        {
            if (fillData.targetPanel != null)
            {
                fillData.panelImage = fillData.targetPanel.GetComponent<Image>();
                if (fillData.panelImage != null)
                {
                    fillData.panelImage.fillAmount = fillData.minFill;
                    fillData.isInitialized = true;
                }
            }
        }
    }

    void LoadPlayerData()
    {
        // 加载玩家数据
        if (PlayerDataManager.Instance != null)
        {
            currentPlayerData = PlayerDataManager.Instance.CurrentPlayerData;

            if (currentPlayerData != null)
            {
                UpdateAllUI();
                isDataLoaded = true;
            }
            else
            {
                LoadDefaultData();
            }
        }
        else
        {
            LoadDefaultData();
        }
        UpdateAllUI();
    }

    void LoadDefaultData()
    {
        // 创建默认数据
        currentPlayerData = new PlayerData("舰长")
        {
            Level = 20, // 提高等级以便看到周常任务
            Experience = 25,
            Stamina = 120,
            Coins = 5000,
            Crystals = 1500
        };

        UpdateAllUI();
        isDataLoaded = true;
    }

    void UpdateAllUI()
    {
        if (currentPlayerData == null) return;

        UpdatePlayerInfo();
        UpdateResources();
        UpdatePanelFills();
    }

    void UpdatePlayerInfo()
    {
        if (currentPlayerData == null) return;

        if (playerNameText != null)
            playerNameText.text = currentPlayerData.PlayerName;

        if (playerLevelText != null)
            playerLevelText.text = $"Lv.{currentPlayerData.Level}";

        int expNeededForNextLevel = currentPlayerData.Level * 100;

        if (playerExperienceText != null)
            playerExperienceText.text = $"{currentPlayerData.Experience}/{expNeededForNextLevel}";

        if (experienceSlider != null)
        {
            experienceSlider.maxValue = expNeededForNextLevel;
            experienceSlider.value = currentPlayerData.Experience;
        }
    }

    void UpdateResources()
    {
        if (currentPlayerData == null) return;

        if (tiliText != null)
            tiliText.text = FormatNumber(currentPlayerData.Stamina);

        if (coinsText != null)
            coinsText.text = FormatNumber(currentPlayerData.Coins);

        if (crystalsText != null)
            crystalsText.text = FormatNumber(currentPlayerData.Crystals);
    }

    void UpdatePanelFills()
    {
        if (panelFillControllers != null && currentPlayerData != null)
        {
            int expNeededForNextLevel = currentPlayerData.Level * 100;
            float expPercent = expNeededForNextLevel > 0 ? (float)currentPlayerData.Experience / expNeededForNextLevel : 0;
            float crystalPercent = Mathf.Clamp01(currentPlayerData.Crystals / 10000f);
            float coinPercent = Mathf.Clamp01(currentPlayerData.Coins / 50000f);

            foreach (PanelFillData fillData in panelFillControllers)
            {
                if (fillData.panelImage != null)
                {
                    float fillValue = fillData.fillAmount;

                    switch (fillData.fillName)
                    {
                        case "Exp":
                            fillValue = expPercent;
                            break;
                        case "水晶进度":
                            fillValue = crystalPercent;
                            break;
                        case "金币进度":
                            fillValue = coinPercent;
                            break;
                        default:
                            if (fillData.useCustomFill)
                            {
                                fillValue = fillData.customFillValue;
                            }
                            break;
                    }

                    fillData.panelImage.fillAmount = Mathf.Clamp(fillValue, fillData.minFill, fillData.maxFill);
                    fillData.fillAmount = fillData.panelImage.fillAmount;
                }
            }
        }
    }

    string FormatNumber(int number)
    {
        if (number >= 1000000)
            return $"{(number / 1000000f):F1}M";
        if (number >= 1000)
            return $"{(number / 1000f):F1}K";
        return number.ToString();
    }

    // ================== 任务系统初始化 ==================
    void InitializeTaskSystem()
    {
        // 刷新任务状态
        if (currentPlayerData != null)
        {
            currentPlayerData.RefreshTasks();

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
        if (filterAllButton != null)
            filterAllButton.onClick.AddListener(() => SetTaskFilter(TaskFilter.All));
        if (filterDailyButton != null)
            filterDailyButton.onClick.AddListener(() => SetTaskFilter(TaskFilter.Daily));
        if (filterWeeklyButton != null)
            filterWeeklyButton.onClick.AddListener(() => SetTaskFilter(TaskFilter.Weekly));

        // 加载默认面板
        SwitchTaskPanel(0);

        // 更新刷新时间文本
        UpdateRefreshTimeText();

        // 初始化任务详情面板
        if (taskDetailPanel != null)
        {
            taskDetailPanel.Initialize(this);
        }
        
        // 添加背景点击关闭功能
        StartCoroutine(SetupBackgroundClick());
        
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
                SetTaskFilter(TaskFilter.All);
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

    // ================== 任务筛选功能 ==================
    void SetTaskFilter(TaskFilter filter)
    {
        currentFilter = filter;

        // 更新按钮状态
        UpdateFilterButtons();

        // 根据筛选加载任务
        switch (filter)
        {
            case TaskFilter.All:
                LoadAllTasks(); // 加载所有任务
                if (currentFilterText != null) currentFilterText.text = "全部任务";
                break;
            case TaskFilter.Daily:
                LoadTasksByFrequency(TaskFrequency.Daily);
                if (currentFilterText != null) currentFilterText.text = "日常任务";
                break;
            case TaskFilter.Weekly:
                LoadTasksByFrequency(TaskFrequency.Weekly);
                if (currentFilterText != null) currentFilterText.text = "周常任务";
                break;
        }
    }

    void UpdateFilterButtons()
    {
        // 设置按钮选中状态
        SetFilterButtonSelected(filterAllButton, currentFilter == TaskFilter.All);
        SetFilterButtonSelected(filterDailyButton, currentFilter == TaskFilter.Daily);
        SetFilterButtonSelected(filterWeeklyButton, currentFilter == TaskFilter.Weekly);
    }

    void SetFilterButtonSelected(Button button, bool selected)
    {
        if (button == null) return;

        var buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = selected ? new Color(0.3f, 0.6f, 1f, 1f) : new Color(0.125f, 0.25f, 0.5f, 1f);
        }

        // 也可以改变按钮文本颜色
        var buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.color = selected ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
        }
    }

    // ================== 任务列表管理 ==================
    void LoadAllTasks()
    {
        if (currentPlayerData == null || missionListContent == null || missionItemPrefab == null)
            return;

        // 清除现有任务项
        ClearMissionList();

        // 获取所有任务（不筛选频率）
        List<TaskData> allTasks = currentPlayerData.GetSortedTasks(null);

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

        // 如果没有任务，显示空状态
        if (itemCount == 0)
        {
            ShowEmptyState();
        }

        // 更新任务标题
        if (missionTitleText != null)
        {
            missionTitleText.text = "作战任务 - 全部";
        }

        // 强制布局重建（如果需要）
        StartCoroutine(RebuildLayout());
    }

    void LoadTasksByFrequency(TaskFrequency frequency)
    {
        if (currentPlayerData == null || missionListContent == null || missionItemPrefab == null)
            return;

        // 清除现有任务项
        ClearMissionList();

        // 获取指定频率的任务
        List<TaskData> tasks = currentPlayerData.GetSortedTasks(frequency);

        // 调试信息
        Debug.Log($"=== 加载{frequency}任务 ===");
        Debug.Log($"任务数量: {tasks.Count}");

        // 创建任务项
        int itemCount = 0;
        foreach (TaskData task in tasks)
        {
            CreateMissionItem(task);
            itemCount++;
        }

        // 如果没有任务，显示空状态
        if (itemCount == 0)
        {
            ShowEmptyState();
        }

        // 更新任务标题
        if (missionTitleText != null)
        {
            string title = frequency == TaskFrequency.Daily ? "作战任务 - 日常" : "作战任务 - 周常";
            missionTitleText.text = title;
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

    void ShowEmptyState()
    {
        // 创建一个空状态提示
        GameObject emptyState = new GameObject("EmptyState");
        emptyState.transform.SetParent(missionListContent);

        RectTransform rect = emptyState.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800, 100);
        rect.localScale = Vector3.one;

        TextMeshProUGUI text = emptyState.AddComponent<TextMeshProUGUI>();
        text.text = "当前没有可显示的任务";
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.gray;
        text.fontSize = 24;
    }

    IEnumerator RebuildLayout()
    {
        // 等待一帧让Unity更新布局
        yield return null;

        // 强制重建布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(missionListContent as RectTransform);
    }

    public void RefreshMissionList()
    {
        // 根据当前筛选重新加载任务
        switch (currentFilter)
        {
            case TaskFilter.All:
                LoadAllTasks();
                break;
            case TaskFilter.Daily:
                LoadTasksByFrequency(TaskFrequency.Daily);
                break;
            case TaskFilter.Weekly:
                LoadTasksByFrequency(TaskFrequency.Weekly);
                break;
        }
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
                    ShowConfirmationWindow($"是否前往{task.TaskName}？", () =>
                    {
                        SceneManager.LoadScene(task.SceneName);
                    });
                }
                break;

            case TaskStatus.Completed:
                // 领取奖励
                if (currentPlayerData.ClaimTaskReward(taskId))
                {
                    // 刷新界面
                    RefreshMissionList();
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

    public void GoToTaskScene(TaskData task)
    {
        if (task == null || string.IsNullOrEmpty(task.SceneName)) return;

        // 显示确认窗口
        ShowConfirmationWindow($"是否前往【{task.TaskName}】？", () =>
        {
            // 保存当前任务状态
            PlayerPrefs.SetString("CurrentTask", task.TaskID);

            // 加载场景
            SceneManager.LoadScene(task.SceneName);
        });
    }

    // ================== 每日历练值管理 ==================
    void UpdateDailyEXPDisplay()
    {
        if (currentPlayerData == null || dailyEXPSlider == null) return;

        // 更新滑动条
        dailyEXPSlider.maxValue = 600;
        dailyEXPSlider.value = currentPlayerData.DailyEXP;

        if (dailyEXPText != null)
            dailyEXPText.text = $"{currentPlayerData.DailyEXP}/600";

        // 更新奖励项状态
        for (int i = 0; i < dailyRewardButtons.Length && i < currentPlayerData.DailyEXPRewards.Count; i++)
        {
            var reward = currentPlayerData.DailyEXPRewards[i];
            bool isClaimed = reward.IsClaimed;
            bool canClaim = currentPlayerData.DailyEXP >= reward.RequiredEXP && !isClaimed;

            // 设置按钮交互状态
            if (dailyRewardButtons[i] != null)
            {
                dailyRewardButtons[i].interactable = canClaim;
            }

            // 设置已领取标记
            if (dailyRewardClaimed[i] != null)
            {
                dailyRewardClaimed[i].SetActive(isClaimed);
            }

            // 设置阈值文本
            if (dailyRewardThresholds[i] != null)
            {
                dailyRewardThresholds[i].text = reward.RequiredEXP.ToString();
            }
        }
    }

    public void OnDailyRewardClicked(int index)
    {
        if (currentPlayerData == null) return;

        if (currentPlayerData.ClaimDailyEXPReward(index))
        {
            UpdateDailyEXPDisplay();
            UpdateAllUI(); // 更新资源显示
        }
    }

    // ================== 刷新时间文本 ==================
    void UpdateRefreshTimeText()
    {
        if (dailyRefreshText != null)
        {
            dailyRefreshText.text = "每日4:00刷新";
        }

        if (weeklyRefreshText != null)
        {
            weeklyRefreshText.text = "每周一4:00刷新";
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

    // ================== 确认窗口 ==================
    void ShowConfirmationWindow(string message, System.Action confirmAction)
    {
        if (confirmationWindow == null || confirmationText == null) return;

        confirmationText.text = message;
        pendingAction = confirmAction;
        confirmationWindow.SetActive(true);
    }

    public void OnConfirm()
    {
        pendingAction?.Invoke();
        confirmationWindow.SetActive(false);
    }

    public void OnCancel()
    {
        pendingAction = null;
        confirmationWindow.SetActive(false);
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
}