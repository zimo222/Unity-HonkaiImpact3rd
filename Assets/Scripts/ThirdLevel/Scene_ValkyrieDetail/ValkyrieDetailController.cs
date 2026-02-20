using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ValkyrieDetailController : MonoBehaviour
{
    [Header("View引用")]
    [SerializeField] private ValkyrieDetailView viewValkyrieDetail;

    // =========================  按钮引用   =========================
    public ModularUIButton[] referencedButtons;
    public Button[] toDetailButton;

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;
    private List<ValkyrieItemUI> valkyrieItemUIs = new List<ValkyrieItemUI>();
    private int currentValkyrie = 0 == 0 ? 0 : 0;

    // Start is called before the first frame update
    void Start()
    {
        currentValkyrie = PlayerPrefs.GetInt("ValkyrieIndex");
        viewValkyrieDetail.InitializeUI();
        LoadPlayerData();
        viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);
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
    
}
