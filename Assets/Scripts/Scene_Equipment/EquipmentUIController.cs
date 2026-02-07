using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("详情面板 - 用于材料显示")]
    public GameObject detailPanel;              // 详情面板（仅用于材料）
    public Image detailRarityImage;             // 详情-背景
    public Image detailIconImage;             // 详情-图标
    public Image detailStarImage;             // 详情-星级
    public TMP_Text detailNameText;             // 详情-名称
    public TMP_Text detailIntroductionText;      // 详情-介绍
    public TMP_Text detailDescriptionText;      // 详情-描述
    public TMP_Text detailCountText;            // 详情-数量
    public Button closeDetailButton;            // 关闭详情按钮

    // ================== 数据 ==================
    private PlayerData playerData;
    private static List<WeaponData> currentWeapons;
    private static List<StigmataData> currentStigmatas;
    private static List<MaterialData> currentMaterials;

    // 新增：活动项列表
    private List<GameObject> activeItems = new List<GameObject>();

    private object selectedItem;  // 当前选中的项（仅材料使用）
    private static ItemType currentTab = ItemType.Weapon;  // 当前标签

    // 静态变量用于场景间传递数据
    private static int selectedEquipmentIndex;

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

        // 预创建对象池
        EquipmentObjectPool.Instance.Prewarm(equipmentItemPrefab.name, equipmentItemPrefab, 20);
        EquipmentObjectPool.Instance.Prewarm(materialItemPrefab.name, materialItemPrefab, 20);

        // 加载默认标签内容
        SwitchToTab(currentTab);
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

        // 预分类装备数据
        CategorizeEquipments();
    }
    //分离背包元素
    void CategorizeEquipments()
    {
        if (playerData == null) return;

        // 分离武器和圣痕
        currentWeapons = new List<WeaponData>(playerData.WeaponBag);
        currentStigmatas = new List<StigmataData>(playerData.StigmataBag);
        currentMaterials = new List<MaterialData>(playerData.MaterialBag);

        //按照稀有度从高到低排序
        currentWeapons.Sort((a, b) =>
        {
            int statusOrderA = a.Stats.Stars;
            int statusOrderB = b.Stats.Stars;

            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前
            return 0;
        });
        currentStigmatas.Sort((a, b) =>
        {
            int statusOrderA = a.Stats.Stars;
            int statusOrderB = b.Stats.Stars;

            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前
            return 0;
        });
        currentMaterials.Sort((a, b) =>
        {
            string statusOrderA = a.Stars;
            string statusOrderB = b.Stars;

            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前
            return 0;
        });

        Debug.Log($"分类完成：武器 {currentWeapons.Count} 件，圣痕 {currentStigmatas.Count} 件，材料 {currentMaterials.Count} 件");
    }
    //初始化UI
    void InitializeUI()
    {
        // 绑定标签按钮事件
        if (weaponTabButton != null)
            weaponTabButton.onClick.AddListener(() => SwitchToTab(ItemType.Weapon));

        if (stigmataTabButton != null)
            stigmataTabButton.onClick.AddListener(() => SwitchToTab(ItemType.Stigmata));

        if (materialTabButton != null)
            materialTabButton.onClick.AddListener(() => SwitchToTab(ItemType.Material));

        // 绑定详情面板关闭按钮（仅用于材料）
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

        // 更新标签按钮状态
        UpdateTabButtons(tabType);

        // 加载对应标签的内容
        LoadCurrentTabContent();
    }
    //更新标签按钮交互状态
    void UpdateTabButtons(ItemType selectedTab)
    {
        // 更新按钮交互状态
        if (weaponTabButton != null)
            weaponTabButton.interactable = (selectedTab != ItemType.Weapon);

        if (stigmataTabButton != null)
            stigmataTabButton.interactable = (selectedTab != ItemType.Stigmata);

        if (materialTabButton != null)
            materialTabButton.interactable = (selectedTab != ItemType.Material);
    }
    //加载对应类项
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
    //加载武器
    void LoadWeapons()
    {
        if (currentWeapons == null) return;

        for (int i = 0; i < currentWeapons.Count; i++)
        {
            var weapon = currentWeapons[i];
            CreateEquipmentItem(weapon, i);
        }

        Debug.Log($"加载了 {currentWeapons.Count} 件武器");
    }
    //加载圣痕
    void LoadStigmatas()
    {
        if (currentStigmatas == null) return;

        for (int i = 0; i < currentStigmatas.Count; i++)
        {
            var stigmata = currentStigmatas[i];
            CreateEquipmentItem(stigmata, i);
        }

        Debug.Log($"加载了 {currentStigmatas.Count} 件圣痕");
    }
    //加载材料
    void LoadMaterials()
    {
        if (currentMaterials == null) return;

        foreach (var material in currentMaterials)
        {
            CreateMaterialItem(material);
        }

        Debug.Log($"加载了 {currentMaterials.Count} 件材料");
    }
    // 创建装备项 - 添加索引参数
    void CreateEquipmentItem(EquipmentData equipment, int index)
    {
        if (equipmentItemPrefab == null || equipmentListContent == null) return;
        /*
        GameObject itemObj = Instantiate(equipmentItemPrefab, equipmentListContent);
        EquipmentItemView itemView = itemObj.GetComponent<EquipmentItemView>();

        if (itemView != null)
        {
            // 传递索引给点击回调
            itemView.Initialize(equipment, OnEquipmentItemClicked, index);
        }
        */

        GameObject itemObj = EquipmentObjectPool.Instance.GetObject(
            equipmentItemPrefab.name,
            equipmentListContent
        );

        if (itemObj == null)
        {
            // 对象池返回null，需要创建新对象
            itemObj = Instantiate(equipmentItemPrefab, equipmentListContent);
        }

        EquipmentItemView itemView = itemObj.GetComponent<EquipmentItemView>();

        if (itemView != null)
        {
            itemView.Initialize(equipment, OnEquipmentItemClicked, index);
        }

        activeItems.Add(itemObj);
    }
    // 创建材料项
    void CreateMaterialItem(MaterialData material)
    {
        if (materialItemPrefab == null || equipmentListContent == null) return;
        /*
        GameObject itemObj = Instantiate(materialItemPrefab, equipmentListContent);
        MaterialItemView itemView = itemObj.GetComponent<MaterialItemView>();

        if (itemView != null)
        {
            itemView.Initialize(material, OnMaterialItemClicked);
        }
        */

        GameObject itemObj = EquipmentObjectPool.Instance.GetObject(
            materialItemPrefab.name,
            equipmentListContent
        );

        if (itemObj == null)
        {
            itemObj = Instantiate(materialItemPrefab, equipmentListContent);
        }

        MaterialItemView itemView = itemObj.GetComponent<MaterialItemView>();

        if (itemView != null)
        {
            itemView.Initialize(material, OnMaterialItemClicked);
        }

        activeItems.Add(itemObj);
    }
    //清空容器
    void ClearItemList()
    {
        if (equipmentListContent == null) return;
        /*
        for (int i = equipmentListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(equipmentListContent.GetChild(i).gameObject);
        }
        */

        foreach (var item in activeItems)
        {
            if (item != null)
            {
                // 判断是哪种类型的项
                if (item.GetComponent<EquipmentItemView>() != null)
                {
                    EquipmentObjectPool.Instance.ReturnObject(equipmentItemPrefab.name, item);
                }
                else if (item.GetComponent<MaterialItemView>() != null)
                {
                    EquipmentObjectPool.Instance.ReturnObject(materialItemPrefab.name, item);
                }
                else
                {
                    Destroy(item);
                }
            }
        }

        activeItems.Clear();

    }

    // ================== 事件处理 ==================

    // 装备项点击 - 修改为跳转场景
    void OnEquipmentItemClicked(int Index)
    {
        // 保存装备信息以便场景间传递
        SaveEquipmentSelection(Index);

        // 跳转到装备详情场景
        PlayerPrefs.SetString("LastScene", "3EquipmentScene");
        SceneManager.LoadScene("EquipmentDetailScene");
    }
    // 材料项点击 - 保持原有逻辑
    void OnMaterialItemClicked(MaterialData material)
    {
        selectedItem = material;
        ShowMaterialDetail(material);
    }
    // 保存装备选择信息
    void SaveEquipmentSelection(int Index)
    {
        // 使用PlayerPrefs（如果需要持久化）
        PlayerPrefs.SetInt("SelectedEquipmentIndex", Index);
        selectedEquipmentIndex = Index;
        PlayerPrefs.Save();

        EquipmentData equipment = currentTab == ItemType.Weapon ? currentWeapons[Index] : currentStigmatas[Index] ;

        Debug.Log($"已选择装备: {equipment.Name}, ID: {equipment.Id}, Tab: {currentTab}");
    }
    // 显示材料详情（仅材料使用）
    void ShowMaterialDetail(MaterialData material)
    {
        if (detailPanel == null) return;

        detailPanel.SetActive(true);

        if (detailRarityImage != null)
            detailRarityImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Material/Frame_{material.Stars}");

        if (detailIconImage != null)
            detailIconImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Material/Icon_{material.Id}");

        if (detailStarImage != null)
            detailStarImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{material.Stars}");

        if (detailNameText != null)
            detailNameText.text = material.Name;

        if (detailCountText != null)
            detailCountText.text = $"{material.Count}";

        if (detailIntroductionText != null)
            detailIntroductionText.text = $"{material.textStats.Introduction}";

        if (detailDescriptionText != null)
            detailDescriptionText.text = $"{material.textStats.Description}";

    }

    //隐藏详情面板
    void HideDetailPanel()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);

        selectedItem = null;
    }
    //更新材料UI
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

    // ================== 静态方法供其他场景访问 ==================
    // 获取选中的装备信息
    public static EquipmentData GetSelectedEquipment()
    {
        return currentTab == ItemType.Weapon ? currentWeapons[selectedEquipmentIndex] : currentStigmatas[selectedEquipmentIndex];
    }

    // 新增：在销毁时清理对象池
    void OnDestroy()
    {
        EquipmentObjectPool.Instance.ClearAllPools();
    }
}