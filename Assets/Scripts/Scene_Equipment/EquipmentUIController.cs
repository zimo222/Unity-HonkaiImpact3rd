using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIController : MonoBehaviour
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
    [Header("分类标签")]
    public Button weaponTabButton;      // 武器标签
    public Button stigmataTabButton;    // 圣痕标签
    public Button materialTabButton;    // 材料标签

    [Header("装备列表")]
    public Transform equipmentListContent;  // 装备/材料列表容器
    public GameObject equipmentItemPrefab;  // 装备项预制体
    public GameObject materialItemPrefab;   // 材料项预制体

    [Header("详情面板")]
    public GameObject detailPanel;          // 全屏详情面板
    public Text detailNameText;             // 详情-名称
    public Text detailTypeText;             // 详情-类型
    public Text detailLevelText;            // 详情-等级
    public Text detailStatsText;            // 详情-属性
    public Button closeDetailButton;        // 关闭详情按钮

    // ================== 数据 ==================
    private PlayerData playerData;
    private List<EquipmentData> currentWeapons;
    private List<EquipmentData> currentStigmatas;
    private List<MaterialData> currentMaterials;

    private object selectedItem;  // 当前选中的项（可能是EquipmentData或MaterialData）
    private ItemType currentTab = ItemType.Weapon;  // 当前标签

    // 分类枚举
    private enum ItemType
    {
        Weapon,
        Stigmata,
        Material
    }

    void Start()
    {
        // 获取玩家数据
        LoadPlayerData();

        // 初始化UI
        InitializeUI();

        // 加载默认标签内容
        SwitchToTab(ItemType.Weapon);
    }

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

            // 添加一些测试材料
            playerData.MaterialBag.Add(new MaterialData("MAT_001", "经验芯片", 50));
            playerData.MaterialBag.Add(new MaterialData("MAT_002", "强化合金", 30));
            playerData.MaterialBag.Add(new MaterialData("MAT_003", "突破核心", 10));
        }

        // 预分类装备数据
        CategorizeEquipments();
    }

    // 预分类装备数据（优化性能）
    void CategorizeEquipments()
    {
        if (playerData == null) return;

        // 分离武器和圣痕
        currentWeapons = playerData.EquipmentBag.FindAll(e => e.Type == EquipmentType.Weapon);
        currentStigmatas = playerData.EquipmentBag.FindAll(e => e.Type == EquipmentType.Stigmata);
        currentMaterials = new List<MaterialData>(playerData.MaterialBag);

        Debug.Log($"分类完成：武器 {currentWeapons.Count} 件，圣痕 {currentStigmatas.Count} 件，材料 {currentMaterials.Count} 件");
    }

    void InitializeUI()
    {
        // 绑定标签按钮事件
        if (weaponTabButton != null)
            weaponTabButton.onClick.AddListener(() => SwitchToTab(ItemType.Weapon));

        if (stigmataTabButton != null)
            stigmataTabButton.onClick.AddListener(() => SwitchToTab(ItemType.Stigmata));

        if (materialTabButton != null)
            materialTabButton.onClick.AddListener(() => SwitchToTab(ItemType.Material));

        // 绑定详情面板关闭按钮
        if (closeDetailButton != null)
            closeDetailButton.onClick.AddListener(HideDetailPanel);

        // 初始化详情面板为隐藏
        if (detailPanel != null)
            detailPanel.SetActive(false);

        // 更新资源显示
        UpdateResourceDisplay();
    }

    // 切换到指定标签
    void SwitchToTab(ItemType tabType)
    {
        currentTab = tabType;

        // 更新标签按钮状态（这里可以添加选中效果）
        UpdateTabButtons(tabType);

        // 加载对应标签的内容
        LoadCurrentTabContent();
    }

    // 更新标签按钮状态
    void UpdateTabButtons(ItemType selectedTab)
    {
        // 这里可以设置按钮的选中状态
        // 例如：改变颜色、添加下划线等
        if (weaponTabButton != null)
            weaponTabButton.interactable = (selectedTab != ItemType.Weapon);

        if (stigmataTabButton != null)
            stigmataTabButton.interactable = (selectedTab != ItemType.Stigmata);

        if (materialTabButton != null)
            materialTabButton.interactable = (selectedTab != ItemType.Material);
    }

    // 加载当前标签内容
    void LoadCurrentTabContent()
    {
        ClearItemList();

        switch (currentTab)
        {
            case ItemType.Weapon:
                LoadWeapons();
                break;
            case ItemType.Stigmata:
                LoadStigmatas();
                break;
            case ItemType.Material:
                LoadMaterials();
                break;
        }
    }

    // 加载武器列表
    void LoadWeapons()
    {
        if (currentWeapons == null) return;

        foreach (var weapon in currentWeapons)
        {
            CreateEquipmentItem(weapon);
        }

        Debug.Log($"加载了 {currentWeapons.Count} 件武器");
    }

    // 加载圣痕列表
    void LoadStigmatas()
    {
        if (currentStigmatas == null) return;

        foreach (var stigmata in currentStigmatas)
        {
            CreateEquipmentItem(stigmata);
        }

        Debug.Log($"加载了 {currentStigmatas.Count} 件圣痕");
    }

    // 加载材料列表
    void LoadMaterials()
    {
        if (currentMaterials == null) return;

        foreach (var material in currentMaterials)
        {
            CreateMaterialItem(material);
        }

        Debug.Log($"加载了 {currentMaterials.Count} 件材料");
    }

    // 创建装备项
    void CreateEquipmentItem(EquipmentData equipment)
    {
        if (equipmentItemPrefab == null || equipmentListContent == null) return;
        GameObject itemObj = Instantiate(equipmentItemPrefab, equipmentListContent);
        EquipmentItemView itemView = itemObj.GetComponent<EquipmentItemView>();

        if (itemView != null)
        {
            itemView.Initialize(equipment, OnEquipmentItemClicked);
        }
    }

    // 创建材料项
    void CreateMaterialItem(MaterialData material)
    {
        if (materialItemPrefab == null || equipmentListContent == null) return;

        GameObject itemObj = Instantiate(materialItemPrefab, equipmentListContent);
        MaterialItemView itemView = itemObj.GetComponent<MaterialItemView>();

        if (itemView != null)
        {
            itemView.Initialize(material, OnMaterialItemClicked);
        }
    }

    // 清空列表
    void ClearItemList()
    {
        if (equipmentListContent == null) return;

        for (int i = equipmentListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(equipmentListContent.GetChild(i).gameObject);
        }
    }

    // ================== 事件处理 ==================

    // 装备项点击
    void OnEquipmentItemClicked(EquipmentData equipment)
    {
        selectedItem = equipment;
        ShowEquipmentDetail(equipment);
    }

    // 材料项点击
    void OnMaterialItemClicked(MaterialData material)
    {
        selectedItem = material;
        ShowMaterialDetail(material);
    }

    // 显示装备详情
    void ShowEquipmentDetail(EquipmentData equipment)
    {
        if (detailPanel == null) return;

        detailPanel.SetActive(true);

        if (detailNameText != null)
            detailNameText.text = equipment.Name;

        if (detailTypeText != null)
        {
            string typeName = equipment.Type == EquipmentType.Weapon ?
                "武器" : $"圣痕({EquipmentHelper.GetStigmataPositionName(equipment.StigmataPosition)})";
            detailTypeText.text = typeName;
        }

        if (detailLevelText != null)
            detailLevelText.text = $"Lv.{equipment.Stats.Level}";

        if (detailStatsText != null)
        {
            string stats = $"攻击: {equipment.Attack}\n";
            if (equipment.Health > 0) stats += $"生命: {equipment.Health}\n";
            if (equipment.CritRate > 0) stats += $"暴击: {equipment.CritRate:P0}\n";
            if (equipment.CritDamage > 0) stats += $"爆伤: {equipment.CritDamage:P0}\n";
            if (equipment.ElementBonus > 0) stats += $"元素: {equipment.ElementBonus:P0}";
            detailStatsText.text = stats;
        }
    }

    // 显示材料详情
    void ShowMaterialDetail(MaterialData material)
    {
        if (detailPanel == null) return;

        detailPanel.SetActive(true);

        if (detailNameText != null)
            detailNameText.text = material.Name;

        if (detailTypeText != null)
            detailTypeText.text = "材料";

        if (detailLevelText != null)
            detailLevelText.text = "";

        if (detailStatsText != null)
            detailStatsText.text = $"数量: {material.Count}";
    }

    // 隐藏详情面板
    void HideDetailPanel()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);

        selectedItem = null;
    }

    // 更新资源显示
    void UpdateResourceDisplay()
    {
        if (playerData == null) return;

        if (coinsText != null)
            coinsText.text = playerData.Coins.ToString();

        if (crystalsText != null)
            crystalsText.text = playerData.Crystals.ToString();

        if (staminaText != null)
            staminaText.text = $"{playerData.Stamina}/{playerData.Level + 80}";
    }
}