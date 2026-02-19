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

    [Header("女武神列表")]
    public Transform valkyrieListContent;     // 女武神列表容器
    public GameObject valkyrieItemPrefab;     // 女武神项预制体

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
        LoadAllValkyries(); // 加载所有任务

        int num = 0;
        foreach(Button btn in toDetailButton)
        {
            int cnt = num;
            btn.onClick.AddListener(() => OntoDetailButtonClick(cnt));
            num++;
        }      
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
    
    // ================== 女武神列表管理 ==================
    public void LoadAllValkyries()
    {
        if (currentPlayerData == null || valkyrieListContent == null || valkyrieItemPrefab == null)
            return;

        // 清除现有女武神项
        ClearValkyrieList();

        // 获取所有女武神（不筛选频率）
        List<CharacterData> allCharacters = PlayerDataManager.Instance.GetSortedCharacters(null);

        // 调试信息
        Debug.Log($"=== 加载所有女武神 ===");
        Debug.Log($"任务总数: {allCharacters.Count}");
        Debug.Log($"机械女武神: {allCharacters.FindAll(t => t.BaseStats.Element == "JX").Count}");
        Debug.Log($"异能女武神: {allCharacters.FindAll(t => t.BaseStats.Element == "YN").Count}");
        Debug.Log($"生物女武神: {allCharacters.FindAll(t => t.BaseStats.Element == "SW").Count}");

        // 创建女武神项
        int itemCount = 0;
        foreach (CharacterData character in allCharacters)
        {
            itemCount++;

            // 调试每个女武神
        }

        // 强制布局重建（如果需要）
        StartCoroutine(RebuildLayout());
    }

    //清空女武神列表容器
    void ClearValkyrieList()
    {
        // 清除现有任务项
        for (int i = valkyrieListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(valkyrieListContent.GetChild(i).gameObject);
        }
        valkyrieItemUIs.Clear();
    }

    public void ShowValkyrieSummary(int CurrentValkyrie)
    {
        currentValkyrie = CurrentValkyrie;
        viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);
    }

    IEnumerator RebuildLayout()
    {
        // 等待一帧让Unity更新布局
        yield return null;

        // 强制重建布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(valkyrieListContent as RectTransform);
    }

    void OntoDetailButtonClick(int index)
    {
        PlayerPrefs.SetInt("ValkyrieIndex", currentValkyrie);
        PlayerPrefs.SetInt("ValkyrieDetailIndex", index);
        SceneDataManager.Instance.PushCurrentScene();
        SceneManager.LoadScene("ValkyrieDetailScene");
    }
}
