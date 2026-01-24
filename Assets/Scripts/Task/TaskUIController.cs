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

    [Header("资源信息")]
    public TMP_Text HomogeneousPureCrystalText;

    // ================== 按钮引用 (可选) ==================
    [Header("按钮引用")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
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
        if (HomogeneousPureCrystalText != null) HomogeneousPureCrystalText.text = "0";
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
        // 先重置数据加载状态
        isDataLoaded = false;

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
        currentPlayerData.RefreshTasks();

        UpdateAllUI();
        isDataLoaded = true;
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
            HomogeneousPureCrystal = 8
        };

        UpdateAllUI();
        isDataLoaded = true;
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
                if (currentPlayerData.ClaimTaskReward(taskId))
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

    // ================== 每日历练值管理 ==================
    public void UpdateDailyEXPDisplay()
    {
        // 检查是否所有UI组件都已正确引用
        /*
        if (dailyEXPSlider == null)
        {
            Debug.LogError("dailyEXPSlider 为空！");
            return;
        }
        */
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

        // 更新滑动条
        /*
        dailyEXPSlider.maxValue = 600;
        dailyEXPSlider.value = currentPlayerData.DailyEXP;
        */
        // 更新文本
        dailyEXPText.text = $"{currentPlayerData.DailyEXP}";
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
                dailyRewardButtons[i].interactable = canClaim;
            }
            else
            {
                Debug.LogWarning($"dailyRewardButtons[{i}] 为空");
            }

            // 设置已领取标记
            if (dailyRewardClaimed != null && i < dailyRewardClaimed.Length && dailyRewardClaimed[i] != null)
            {
                dailyRewardClaimed[i].SetActive(isClaimed);
            }

            // 设置阈值文本
            if (dailyRewardThresholds != null && i < dailyRewardThresholds.Length && dailyRewardThresholds[i] != null)
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
}