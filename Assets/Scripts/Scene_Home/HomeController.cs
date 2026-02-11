using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class HomeController : MonoBehaviour
{
    // ================== 依赖注入 ==================
    [Header("View引用")]
    [SerializeField] private HomeView viewHome;

    // ================== 按钮引用 (可选) ==================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    public ModularUIButton[] referencedButtons;

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;

    // Start is called before the first frame update
    void Start()
    {
        LoadPlayerData();
        viewHome.UpdateAllUI(currentPlayerData);
        // 不再需要初始化按钮，因为每个ModularUIButton会自己管理自己
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadPlayerData()
    {
        // 加载玩家数据
        if (PlayerDataManager.Instance != null)
        {
            currentPlayerData = PlayerDataManager.Instance.CurrentPlayerData;

            if (currentPlayerData != null)
            {
                return;
            }
            else
            {
                LoadDefaultData();
            }
        }
        else
        {
            LoadDefaultData();
        }
    }

    void LoadDefaultData()
    {
        // 创建默认数据
        currentPlayerData = new PlayerData("玩家")
        {
            Level = 1,
            Experience = 25,
            Stamina = 0,
            Coins = 5000,
            Crystals = 1500
        };
    }
}
