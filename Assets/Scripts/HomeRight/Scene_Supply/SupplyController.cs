using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyController : MonoBehaviour
{    // ================== 依赖注入 ==================
    [Header("View引用")]
    [SerializeField] private SupplyView viewSupply;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    // ================== 数据 ==================
    private PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        // 初始化
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void Initialize()
    {
        // 加载数据
        LoadData();
        // 初始化UI
        InitializeUI();
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
    }

    void InitializeUI()
    {
    }
}
