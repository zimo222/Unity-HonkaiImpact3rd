using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controller层：处理业务逻辑，连接Model和View
public class EquipmentDetailController : MonoBehaviour
{
    // ================== 依赖注入 ==================
    [Header("View引用")]
    [SerializeField] private EquipmentDetailView view;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    [Header("按钮引用")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button sellButton;

    // ================== 数据 ==================
    private EquipmentData currentEquipment;
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

        // 初始化UI
        InitializeUI();

        // 绑定事件
        BindEvents();
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
        currentEquipment = EquipmentUIController.GetSelectedEquipment();

        if (currentEquipment == null)
        {
            Debug.LogError("未找到选择的装备");
            return;
        }

        Debug.Log($"已选择装备: {currentEquipment.Name}");
    }

    void InitializeUI()
    {
        // 更新View
        if (view != null)
        {
            if (currentEquipment is WeaponData weapon)
            {
                // 处理武器
                Debug.Log($"这是武器: {weapon.Name}, 类型: {weapon.Type}");
                view.UpdateWeaponInfo(weapon);
            }
            else if (currentEquipment is StigmataData stigmata)
            {
                // 处理圣痕
                Debug.Log($"这是圣痕: {stigmata.Name}, 位置: {stigmata.Position}");
                //ProcessStigmata(stigmata);
            }
            view.UpdatePlayerResources(playerData);
        }
    }

    void BindEvents()
    {
        // 绑定按钮事件
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        if (enhanceButton != null)
            enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);

        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipButtonClicked);

        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellButtonClicked);
    }

    // ================== 按钮事件处理方法 ==================

    void OnBackButtonClicked()
    {
        ReturnToEquipmentList();
    }

    void OnEnhanceButtonClicked()
    {
        // 业务逻辑：强化装备
        EnhanceEquipment();
    }

    void OnEquipButtonClicked()
    {
        // 业务逻辑：装备装备
        //EquipEquipment();
    }

    void OnSellButtonClicked()
    {
        // 业务逻辑：出售装备
        SellEquipment();
    }

    // ================== 业务逻辑方法 ==================

    void ReturnToEquipmentList()
    {
        SceneManager.LoadScene("EquipmentUIScene");
    }

    void EnhanceEquipment()
    {
        // 1. 验证条件
        if (currentEquipment == null || playerData == null)
        {
            Debug.LogWarning("无法强化：数据为空");
            return;
        }

        // 2. 计算消耗
        int cost = 100;
        if (playerData.Coins < cost)
        {
            Debug.LogWarning($"金币不足，需要{cost}，当前{playerData.Coins}");
            return;
        }

        // 3. 修改Model
        playerData.Coins -= cost;
        currentEquipment.Stats.Level++;
        currentEquipment.Stats.Attack += 10;
        currentEquipment.Stats.Health += 50;

        // 4. 保存数据
        SavePlayerData();

        // 5. 更新View
        UpdateAllUI();

        Debug.Log($"强化成功！{currentEquipment.Name} 等级提升到{currentEquipment.Stats.Level}");
    }
    /*
    void EquipEquipment()
    {
        if (currentEquipment == null || playerData == null) return;

        // TODO: 打开角色选择界面
        // 这里简化处理，假设装备给第一个角色
        int characterIndex = 0;

        // 调用PlayerData的装备方法
        bool success = playerData.EquipWeaponToCharacter(characterIndex,
            playerData.EquipmentBag.IndexOf(currentEquipment));

        if (success)
        {
            SavePlayerData();
            UpdateAllUI();
            Debug.Log($"成功装备{currentEquipment.Name}给角色{characterIndex}");
        }
        else
        {
            Debug.LogWarning("装备失败");
        }
    }
    */
    void SellEquipment()
    {
        if (currentEquipment == null || playerData == null) return;

        // 检查是否已装备
        if (currentEquipment.EquippedToCharacterIndex >= 0)
        {
            Debug.LogWarning("无法出售已装备的装备");
            return;
        }

        // 计算售价
        int sellPrice = CalculateSellPrice(currentEquipment);

        // 修改数据
        playerData.Coins += sellPrice;
        /*
        int equipmentIndex = playerData.EquipmentBag.IndexOf(currentEquipment);
        if (equipmentIndex >= 0)
        {
            playerData.EquipmentBag.RemoveAt(equipmentIndex);
        }
        */
        // 保存数据
        SavePlayerData();

        // 返回装备列表
        ReturnToEquipmentList();

        Debug.Log($"出售成功！获得{sellPrice}金币");
    }

    int CalculateSellPrice(EquipmentData equipment)
    {
        int basePrice = 100;
        int starMultiplier = equipment.Stats.Stars.Length;
        int levelMultiplier = equipment.Stats.Level;

        return basePrice * starMultiplier * levelMultiplier;
    }

    // ================== 辅助方法 ==================

    void SavePlayerData()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SaveCurrentPlayerData();
        }
    }

    void UpdateAllUI()
    {
        if (view != null)
        {
            if (currentEquipment is WeaponData weapon)
            {
                // 处理武器
                Debug.Log($"这是武器: {weapon.Name}, 类型: {weapon.Type}");
                view.UpdateWeaponInfo(weapon);
            }
            else if (currentEquipment is StigmataData stigmata)
            {
                // 处理圣痕
                Debug.Log($"这是圣痕: {stigmata.Name}, 位置: {stigmata.Position}");
                //ProcessStigmata(stigmata);
            }
            view.UpdatePlayerResources(playerData);
        }
    }

    void OnDestroy()
    {
        // 清理工作
        UnbindEvents();
    }

    void UnbindEvents()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);

        if (enhanceButton != null)
            enhanceButton.onClick.RemoveListener(OnEnhanceButtonClicked);

        if (equipButton != null)
            equipButton.onClick.RemoveListener(OnEquipButtonClicked);

        if (sellButton != null)
            sellButton.onClick.RemoveListener(OnSellButtonClicked);
    }
}