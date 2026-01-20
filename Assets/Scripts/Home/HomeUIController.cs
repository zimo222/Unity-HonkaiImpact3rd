using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class HomeUIControllerRefactored : MonoBehaviour
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

    // ================== 其他原有系统（保持不变） ==================
    // ... [你的Panel填充控制系统、确认窗口等] ...
    // ================== Panel填充控制 ==================
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

    // ================== 确认窗口 ==================
    [Header("确认窗口")]
    public GameObject confirmationWindow;           // 确认窗口
    public TMP_Text confirmationText;               // 确认文本
    private System.Action pendingAction;            // 待执行的动作

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;
    private bool isDataLoaded = false;

    void Start()
    {
        InitializeUI();
        LoadPlayerData();
        // 不再需要初始化按钮，因为每个ModularUIButton会自己管理自己
    }

    void InitializeUI()
    {
        // ... [你的UI初始化代码] ...
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
        currentPlayerData = new PlayerData("玩家")
        {
            Level = 1,
            Experience = 25,
            Stamina = 0,
            Coins = 5000,
            Crystals = 1500
        };

        UpdateAllUI();
        isDataLoaded = true;
    }

    void UpdateAllUI()
    {
        // ... [你的UI更新代码] ...
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
            tiliText.text = FormatNumber(currentPlayerData.Stamina) + "/" + FormatNumber(currentPlayerData.Level + 80);

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
            float expPercent = (float)currentPlayerData.Experience / expNeededForNextLevel;
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
        /*
        if (number >= 1000000)
            return $"{(number / 1000000f):F1}M";
        if (number >= 1000)
            return $"{(number / 1000f):F1}K";
        */
        return number.ToString();
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