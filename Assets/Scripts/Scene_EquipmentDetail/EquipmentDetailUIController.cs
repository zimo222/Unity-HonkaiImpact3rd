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

    [Header("UI组件")]
    public Image rarityBackground;
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text typeText;
    public TMP_Text levelText;
    public TMP_Text starsText;
    public TMP_Text attackText;
    public TMP_Text healthText;
    public TMP_Text critRateText;
    public TMP_Text critDamageText;
    public TMP_Text elementBonusText;

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
        equipmentIndex = -1;
        InitializeUI();
        LoadSelectedEquipment();
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
            string typeName = currentEquipment.Type == EquipmentType.Weapon ?
                "武器" : $"圣痕({EquipmentHelper.GetStigmataPositionName(currentEquipment.StigmataPosition)})";
            typeText.text = typeName;
        }

        if (levelText != null)
            levelText.text = $"等级: {currentEquipment.Stats.Level}";

        if (starsText != null)
            starsText.text = $"星级: {currentEquipment.Stats.Stars}";

        // 属性信息
        if (attackText != null)
            attackText.text = $"攻击力: {currentEquipment.Attack}";

        if (healthText != null && currentEquipment.Health > 0)
            healthText.text = $"生命值: {currentEquipment.Health}";
        else if (healthText != null)
            healthText.text = "";

        if (critRateText != null && currentEquipment.CritRate > 0)
            critRateText.text = $"暴击率: {currentEquipment.CritRate:P0}";
        else if (critRateText != null)
            critRateText.text = "";

        if (critDamageText != null && currentEquipment.CritDamage > 0)
            critDamageText.text = $"暴击伤害: {currentEquipment.CritDamage:P0}";
        else if (critDamageText != null)
            critDamageText.text = "";

        if (elementBonusText != null && currentEquipment.ElementBonus > 0)
            elementBonusText.text = $"元素加成: {currentEquipment.ElementBonus:P0}";
        else if (elementBonusText != null)
            elementBonusText.text = "";

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

        // 加载图标
        LoadEquipmentIcon();
    }

    void LoadEquipmentIcon()
    {
        if (iconImage == null) return;

        string iconPath = currentEquipment.Type == EquipmentType.Weapon ?
            $"Picture/Scene_Equipment/Weapon/Icon_{currentEquipment.Id}" :
            $"Picture/Scene_Equipment/Stigmata/Icon_{currentEquipment.Id}";

        Sprite icon = Resources.Load<Sprite>(iconPath);

        if (icon != null)
        {
            iconImage.sprite = icon;
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