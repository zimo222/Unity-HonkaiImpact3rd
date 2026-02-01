// Scripts/Equipment/EquipmentUIController.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIController : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text tiliText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    // ================== UI引用 ==================
    [Header("装备列表区域")]
    public Transform equipmentListContent;     // 装备列表容器
    public GameObject equipmentItemPrefab;     // 装备项预制体

    [Header("筛选按钮")]
    //public Button allButton;                   // 全部按钮
    public Button weaponButton;                // 武器按钮
    public Button stigmataButton;              // 圣痕按钮

    [Header("详情面板")]
    public GameObject detailPanel;             // 详情面板
    public Text detailNameText;                // 装备名称
    public Text detailTypeText;                // 装备类型
    public Text detailLevelText;               // 装备等级
    //public Button equipButton;                 // 装备/卸下按钮
    public Button closeDetailButton;           // 关闭详情按钮

    // ================== 数据 ==================
    private PlayerData playerData;             // 玩家数据
    private List<EquipmentData> currentDisplayEquipments;  // 当前显示的装备
    private EquipmentData selectedEquipment;   // 当前选中的装备

    // 当前筛选类型
    private EquipmentType currentFilterType = EquipmentType.Weapon;

    void Start()
    {
        // 获取玩家数据（这里假设有全局的PlayerDataManager）
        LoadPlayerData();

        // 初始化UI
        InitializeUI();

        // 加载装备列表
        LoadEquipmentList();
    }

    void LoadPlayerData()
    {
        // 从PlayerDataManager获取数据
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.CurrentPlayerData;
        }
        else
        {
            // 如果没有PlayerDataManager，创建测试数据
            playerData = new PlayerData("测试玩家");
        }
    }

    void InitializeUI()
    {
        // 绑定按钮事件
        /*
        if (allButton != null)
            allButton.onClick.AddListener(() => OnFilterButtonClicked(null)); // null表示显示全部
        */

        if (weaponButton != null)
            weaponButton.onClick.AddListener(() => OnFilterButtonClicked(EquipmentType.Weapon));

        if (stigmataButton != null)
            stigmataButton.onClick.AddListener(() => OnFilterButtonClicked(EquipmentType.Stigmata));
        /*
        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        */
        if (closeDetailButton != null)
            closeDetailButton.onClick.AddListener(HideDetailPanel);

        // 初始化详情面板为隐藏
        if (detailPanel != null)
            detailPanel.SetActive(false);

        // 更新资源显示
        UpdateResourceDisplay();
    }

    // 加载装备列表
    void LoadEquipmentList()
    {
        if (playerData == null || playerData.EquipmentBag == null)
        {
            Debug.LogWarning("玩家数据或装备背包为空");
            return;
        }

        // 根据当前筛选类型获取装备
        currentDisplayEquipments = GetFilteredEquipments(currentFilterType);

        // 清除现有列表
        ClearEquipmentList();

        // 创建装备项
        for (int i = 0; i < currentDisplayEquipments.Count; i++)
        {
            CreateEquipmentItem(currentDisplayEquipments[i]);
        }

        Debug.Log($"加载了 {currentDisplayEquipments.Count} 件装备");
    }

    // 获取筛选后的装备列表
    List<EquipmentData> GetFilteredEquipments(EquipmentType? filterType)
    {
        if (playerData == null) return new List<EquipmentData>();

        if (filterType == null)
        {
            // 显示全部
            return new List<EquipmentData>(playerData.EquipmentBag);
        }
        else
        {
            // 显示指定类型
            return playerData.EquipmentBag.FindAll(e => e.Type == filterType);
        }
    }

    // 清除装备列表
    void ClearEquipmentList()
    {
        if (equipmentListContent == null) return;

        for (int i = equipmentListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(equipmentListContent.GetChild(i).gameObject);
        }
    }

    // 创建装备项
    void CreateEquipmentItem(EquipmentData equipment)
    {
        if (equipmentItemPrefab == null || equipmentListContent == null) return;

        GameObject itemObj = Instantiate(equipmentItemPrefab, equipmentListContent);
        EquipmentItemUI itemUI = itemObj.GetComponent<EquipmentItemUI>();

        if (itemUI != null)
        {
            itemUI.Initialize(equipment, OnEquipmentItemClicked);
        }
    }

    // ================== 事件处理 ==================

    // 筛选按钮点击
    void OnFilterButtonClicked(EquipmentType? type)
    {
        currentFilterType = type ?? EquipmentType.Weapon;
        LoadEquipmentList();
    }

    // 装备项点击
    void OnEquipmentItemClicked(EquipmentData equipment)
    {
        selectedEquipment = equipment;
        ShowDetailPanel(equipment);
    }

    // 显示详情面板
    void ShowDetailPanel(EquipmentData equipment)
    {
        if (detailPanel == null) return;

        detailPanel.SetActive(true);

        // 更新详情信息
        if (detailNameText != null)
            detailNameText.text = equipment.Name;

        if (detailTypeText != null)
            detailTypeText.text = GetEquipmentTypeName(equipment);

        if (detailLevelText != null)
            detailLevelText.text = $"Lv.{equipment.Stats.Level}";

        // 更新装备按钮文本
        /*
        if (equipButton != null)
        {
            bool isEquipped = equipment.EquippedToCharacterIndex >= 0;
            equipButton.GetComponentInChildren<Text>().text = isEquipped ? "卸下" : "装备";
        }
        */
    }

    // 隐藏详情面板
    void HideDetailPanel()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);

        selectedEquipment = null;
    }

    // 装备/卸下按钮点击
    void OnEquipButtonClicked()
    {
        if (selectedEquipment == null) return;

        // TODO: 实现装备/卸下逻辑
        Debug.Log($"装备/卸下: {selectedEquipment.Name}");

        // 重新加载列表以更新显示
        LoadEquipmentList();
        HideDetailPanel();
    }

    // 更新资源显示
    void UpdateResourceDisplay()
    {
        if (playerData == null) return;

        if (tiliText != null)
            tiliText.text = playerData.Stamina.ToString();

        if (coinsText != null)
            coinsText.text = playerData.Coins.ToString();

        if (crystalsText != null)
            crystalsText.text = playerData.Crystals.ToString();
    }

    // 获取装备类型名称
    string GetEquipmentTypeName(EquipmentData equipment)
    {
        if (equipment.Type == EquipmentType.Weapon)
        {
            return "武器";
        }
        else if (equipment.Type == EquipmentType.Stigmata)
        {
            string position = equipment.StigmataPosition.ToString();
            return $"圣痕·{position}";
        }
        return "未知";
    }
}