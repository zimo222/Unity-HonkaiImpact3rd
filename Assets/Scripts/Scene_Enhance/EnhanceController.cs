using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controller层：处理业务逻辑，连接Model和View
public class EnhanceController : MonoBehaviour
{
    // ================== 依赖注入 ==================
    [Header("View引用")]
    [SerializeField] private EnhanceView view;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    [Header("按钮引用")]
    [SerializeField] private Button enhanceButton;

    // ================== 数据 ==================
    private WeaponData currentWeapon;
    private List<MaterialData> costMaterial; 
    private PlayerData playerData;

    void Start()
    {
        // 初始化
        Initialize();
    }

    void Initialize()
    {
        // 加载数据
        LoadData();

        InitializeEnhance();

        // 初始化UI
        InitializeUI();

        enhanceButton.onClick.AddListener(StartEnhance);
    }

    void LoadData()
    {
        // 获取玩家数据
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.CurrentPlayerData;
        }
        else
        {
            // 测试数据
            playerData = new PlayerData("测试玩家");
        }

        // 获取选中的装备
        currentWeapon = playerData.WeaponBag[PlayerPrefs.GetInt("SelectedEquipmentIndex")];

        if (currentWeapon == null)
        {
            Debug.LogError("未找到选择的装备");
            return;
        }

        Debug.Log($"已选择装备: {currentWeapon.Name}");
    }

    void InitializeEnhance()
    {
        costMaterial = PlayerDataManager.Instance.QuickSelectMaterials(currentWeapon);
    }

    void InitializeUI()
    {
        // 更新View
        if (view != null)
        {
            if (currentWeapon is WeaponData weapon)
            {
                // 处理武器
                Debug.Log($"这是武器: {weapon.Name}, 类型: {weapon.Type}");
                view.UpdateWeaponInfo(weapon, costMaterial);
            }
            view.UpdatePlayerResources(playerData);
        }
    }

    void StartEnhance()
    {
        var result = PlayerDataManager.Instance.EnhanceEquipment(currentWeapon, costMaterial, PlayerDataManager.Instance.CalculateEnhanceCoinCost(costMaterial));
        Debug.Log(result);
        if(result.success)
        {
            SceneManager.LoadScene(SceneDataManager.Instance.PopPreviousScene());
        }
    }
}