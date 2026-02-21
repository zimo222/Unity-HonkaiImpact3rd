using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ValkyrieDetailController : MonoBehaviour
{
    [Header("View引用")]
    [SerializeField] private ValkyrieDetailView viewValkyrieDetail;

    // =========================  按钮引用   =========================
    public ModularUIButton[] referencedButtons;


    // ========================= 角色升级晋升 =========================
    [Header("角色升级晋升")]
    public Button levelUpButton;
    public Button promotionButton;
    public GameObject onePanel;
    public Button oneCloseButton;
    public Button sureLevelUpButton;
    public Button surePromotionButton;
    public GameObject levelUp;
    public GameObject promotion;
    public Slider slider;
    public Button[] maButton;
    private string nowMaterial = "MATE_004";

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;
    private List<ValkyrieItemUI> valkyrieItemUIs = new List<ValkyrieItemUI>();
    private int currentValkyrie = 0 == 0 ? 0 : 0;

    // Start is called before the first frame update
    void Start()
    {
        onePanel.SetActive(false);

        currentValkyrie = PlayerPrefs.GetInt("ValkyrieIndex");
        viewValkyrieDetail.InitializeUI();
        LoadPlayerData();


        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Level >= 80)
        {
            HideOnePanel();
            levelUpButton.interactable = false;
        }
        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Stars >= 3)
        {
            HideOnePanel();
            promotionButton.interactable = false;
        }

        viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);

        levelUpButton.onClick.AddListener(ShowLevelUp);
        promotionButton.onClick.RemoveAllListeners();
        promotionButton.onClick.AddListener(ShowPromotion);
        oneCloseButton.onClick.AddListener(HideOnePanel);
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        sureLevelUpButton.onClick.AddListener(LevelUp);
        surePromotionButton.onClick.RemoveAllListeners();
        surePromotionButton.onClick.AddListener(Promotion);


        int count = 4;
        foreach(Button btn in maButton)
        {
            int now = count;
            count--;
            btn.onClick.AddListener(() => onMaterialButtonClick(now));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadPlayerData()
    {
        // 检查PlayerDataManager是否存在
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("PlayerDataManager.Instance 为空，加载默认数据");
            LoadDefaultData();
            viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);
            return;
        }

        // 获取当前玩家数据
        currentPlayerData = PlayerDataManager.Instance.CurrentPlayerData;

        // 检查是否成功获取玩家数据
        if (currentPlayerData == null)
        {
            Debug.LogWarning("当前没有登录的玩家，加载默认数据");
            LoadDefaultData();
            viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);
            return;
        }

        // 成功加载玩家数据
        Debug.Log($"成功加载玩家数据: {currentPlayerData.PlayerName}, 等级: {currentPlayerData.Level}, DailyEXP: {currentPlayerData.DailyEXP}");

        // 刷新玩家数据中的任务状态
        PlayerDataManager.Instance.RefreshTasks();
        viewValkyrieDetail.UpdateAllUI(currentPlayerData, currentValkyrie);
    }

    //加载默认玩家数据
    void LoadDefaultData()
    {
        // 创建默认数据
        currentPlayerData = new PlayerData("舰长")
        {
            Stamina = 120,
            Coins = 5000,
            Crystals = 1500
        };

        viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);
    }

    // ==================== 角色升级晋升 ====================
    void ShowLevelUp()
    {
        // 初始化 Slider 的最大值
        if (slider != null) slider.maxValue = currentPlayerData.MaterialBag.Find(m => m.Id == nowMaterial).Count;
        onePanel.SetActive(true);
        levelUp.SetActive(true);
        promotion.SetActive(false);
        viewValkyrieDetail.UpdateLevelUpUI(currentPlayerData, currentValkyrie, nowMaterial, 0, currentPlayerData.Characters[currentValkyrie].BaseStats.Level, currentPlayerData.Characters[currentValkyrie].BaseStats.Level);
    }

    void ShowPromotion()
    {
        onePanel.SetActive(true);
        levelUp.SetActive(false);
        promotion.SetActive(true);
        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Fragments >= 50)
        {
            surePromotionButton.interactable = true;
        }
        else
        {
            surePromotionButton.interactable = false;
        }
        viewValkyrieDetail.UpdatePromotionUI(currentPlayerData, currentValkyrie);
    }

    // 当 Slider 的值被改变时调用（由用户拖动或代码设置都会触发）
    private void OnSliderValueChanged(float value)
    {
        Debug.Log(value);
        // 只有当用户正在拖动时才跳转音频时间
        PlayerDataManager.Instance.CalLevelTo(currentValkyrie, nowMaterial, (int)value, out int expGain, out int toLevel, out int toExp, out int finalCost);
        slider.value = finalCost;
        viewValkyrieDetail.UpdateLevelUpUI(currentPlayerData, currentValkyrie, nowMaterial, finalCost, currentPlayerData.Characters[currentValkyrie].BaseStats.Level, toLevel);
    }

    void HideOnePanel()
    {
        onePanel.SetActive(false);
    }

    void onMaterialButtonClick(int index)
    {
        nowMaterial = "MATE_00" + index.ToString();
        slider.maxValue = currentPlayerData.MaterialBag.Find(m => m.Id == nowMaterial).Count;
        slider.value = 0;
        viewValkyrieDetail.UpdateLevelUpUI(currentPlayerData, currentValkyrie, nowMaterial, 0, currentPlayerData.Characters[currentValkyrie].BaseStats.Level, currentPlayerData.Characters[currentValkyrie].BaseStats.Level);
    }

    void LevelUp()
    {
        PlayerDataManager.Instance.LevelUpCharacter(currentValkyrie, nowMaterial, (int)slider.value);
        viewValkyrieDetail.Update1PanelUI(currentPlayerData, currentValkyrie);
        OnSliderValueChanged(0);
        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Level >= 80)
        {
            HideOnePanel();
            levelUpButton.interactable = false;
        }
    }

    void Promotion()
    {
        PlayerDataManager.Instance.PromotionCharacter(currentValkyrie);
        viewValkyrieDetail.Update1PanelUI(currentPlayerData, currentValkyrie);

        ShowPromotion();
        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Stars >= 3)
        {
            HideOnePanel();
            promotionButton.interactable = false;
        }
    }
}
