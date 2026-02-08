using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controller层：处理业务逻辑，连接Model和View
public class EquipmentDetailController : MonoBehaviour
{
    // ================== 依赖注入 ==================
    [Header("View引用")]
    [SerializeField] private EquipmentDetailView viewa;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    [Header("按钮引用")]
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button evolveButton;

    // ================== 数据 ==================
    private bool isWeapon;
    private WeaponData currentWeapon;
    private StigmataData currentStigmata;
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
        if(PlayerPrefs.GetInt("isWeapon") == 1)
        {
            isWeapon = true;
            currentWeapon = playerData.WeaponBag[PlayerPrefs.GetInt("SelectedEquipmentIndex")];
            if (currentWeapon == null)
            {
                Debug.LogError("未找到选择的装备");
                return;
            }
            Debug.Log($"已选择装备: {currentWeapon.Name}");
        }
        else
        {
            isWeapon = false;
            currentStigmata = playerData.StigmataBag[PlayerPrefs.GetInt("SelectedEquipmentIndex")];
            if (currentStigmata == null)
            {
                Debug.LogError("未找到选择的装备");
                return;
            }
            Debug.Log($"已选择装备: {currentStigmata.Name}");
        }
    }

    void InitializeUI()
    {
        // 更新View
        if (viewa != null)
        {
            if (isWeapon)
            {
                // 处理武器
                Debug.Log($"这是武器: {currentWeapon.Name}, 类型: {currentWeapon.Type}");
                viewa.UpdateWeaponInfo(currentWeapon);
                viewa.UpdatePlayerResources(playerData);
            }
            else
            {
                // 处理圣痕
                Debug.Log($"这是圣痕: {currentStigmata.Name}, 位置: {currentStigmata.Position}");
                //ProcessStigmata(stigmata);
            }
        }
    }

    // ================== 按钮事件处理方法 ==================
    void BindEvents()
    {
        // 绑定按钮事件
        if (enhanceButton != null)
            enhanceButton.onClick.AddListener(OnEnhanceClicked);
        if (evolveButton != null)
            evolveButton.onClick.AddListener(OnEvolveClicked);
    }
    // ================== 按钮事件处理方法 ==================
    void OnEnhanceClicked()
    {
        SceneDataManager.Instance.PushCurrentScene();
        SceneManager.LoadScene("EnhanceScene");
    }

    void OnEvolveClicked()
    {
        SceneDataManager.Instance.PushCurrentScene();
        if (isWeapon)
        {
            SceneManager.LoadScene("WeaponEvolveScene");
        }
        else
        {
            SceneManager.LoadScene("StigmataEvolveScene");
        }
    }
}