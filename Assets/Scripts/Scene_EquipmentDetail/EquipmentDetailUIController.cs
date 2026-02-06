using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EquipmentDetailUIController : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    // ================== UI引用 ==================
    [Header("UI组件")]
    [Header("左上")]
    public TMP_Text nameText;
    public Image typeImage;
    public TMP_Text typeText;
    public Image starImage;
    [Header("左中")]
    public TMP_Text descriptionText;
    [Header("左下")]
    public TMP_Text levelText;
    public TMP_Text expText;
    [Header("右上")]
    public GameObject[] statPanel;
    [Header("右中")]

    [Header("操作按钮")]
    public Button backButton;
    public Button enhanceButton;
    public Button equipButton;
    public Button sellButton;

    [Header("装备状态")]
    public GameObject equippedBadge;
    public TMP_Text equippedCharacterText;

    private EquipmentData currentEquipment;
    private PlayerData playerData;
    private int equipmentIndex = -1;

    void Start()
    {
        // 获取玩家数据
        LoadPlayerData();

        equipmentIndex = -1;
        InitializeUI();
        LoadSelectedEquipment();
    }
    //加载玩家数据
    void LoadPlayerData()
    {
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.CurrentPlayerData;
        }
        else
        {
            // 创建测试数据
            playerData = new PlayerData("测试玩家");
        }
    }

    void InitializeUI()
    {
        // 绑定按钮事件
        if (backButton != null)
            backButton.onClick.AddListener(ReturnToEquipmentList);

        if (enhanceButton != null)
            enhanceButton.onClick.AddListener(OnEnhanceClicked);

        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipClicked);

        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellClicked);
    }

    void LoadSelectedEquipment()
    {
        // 使用静态变量获取装备
        currentEquipment = EquipmentUIController.GetSelectedEquipment();

        Debug.Log($"已选择装备: {currentEquipment.Name}, ID: {currentEquipment.Id}, Tab: {currentEquipment.WeaponType}");

        if (currentEquipment == null)
        {
            Debug.LogError($"未找到下标为{equipmentIndex}的装备");
            return;
        }

        // 更新UI显示
        UpdateEquipmentUI();
    }

    void UpdateEquipmentUI()
    {
        if (currentEquipment == null) return;

        // 基本信息
        if (nameText != null)
            nameText.text = currentEquipment.Name;
        if (typeText != null)
        {
            switch(currentEquipment.WeaponType)
            {
                case WeaponType.DualPistols:
                    typeText.text = "双枪";
                    break;
                case WeaponType.SingleHandedSword:
                    typeText.text = "单手剑";
                    break;
                case WeaponType.Spear:
                    typeText.text = "长枪";
                    break;
                default:
                    typeText.text = "";
                    break;
            }
        }

        if (starImage != null)
            starImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stars_{currentEquipment.Stats.Stars}"); ;


        if (descriptionText != null)
            descriptionText.text = currentEquipment.TextStats.Description;

        if (levelText != null)
            levelText.text = $"{currentEquipment.Stats.Level}/<color=#FEDF4C>30</color>";

        // 装备状态
        if (equippedBadge != null)
            equippedBadge.SetActive(currentEquipment.EquippedToCharacterIndex >= 0);

        if (equippedCharacterText != null)
        {
            if (currentEquipment.EquippedToCharacterIndex >= 0)
            {
                equippedCharacterText.text = $"已装备给: 角色{currentEquipment.EquippedToCharacterIndex + 1}";
            }
            else
            {
                equippedCharacterText.text = "未装备";
            }
        }
    }

    // ================== 按钮事件 ==================

    void ReturnToEquipmentList()
    {
        SceneManager.LoadScene("EquipmentUIScene");
    }

    void OnEnhanceClicked()
    {
        Debug.Log($"强化装备: {currentEquipment.Name}");
        // TODO: 打开强化界面
    }

    void OnEquipClicked()
    {
        Debug.Log($"装备: {currentEquipment.Name}");
        // TODO: 打开角色选择界面
    }

    void OnSellClicked()
    {
        Debug.Log($"出售装备: {currentEquipment.Name}");
        // TODO: 打开出售确认界面
    }
}