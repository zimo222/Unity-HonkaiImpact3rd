using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ValkyrieController : MonoBehaviour
{
    [Header("View引用")]
    [SerializeField] private ValkyrieView viewValkyrie;

    // =========================  按钮引用   =========================
    public ModularUIButton[] referencedButtons;
    public Button[] toDetailButton;

    [Header("女武神列表")]
    public Transform valkyrieListContent;     // 女武神列表容器
    public GameObject valkyrieItemPrefab;     // 女武神项预制体

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;
    private List<ValkyrieItemUI> valkyrieItemUIs = new List<ValkyrieItemUI>();
    private int currentValkyrie = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentValkyrie = PlayerPrefs.GetInt("ValkyrieIndex", 0);
        viewValkyrie.InitializeUI();
        LoadPlayerData();
        viewValkyrie.UpdateAllUI(currentPlayerData,currentValkyrie);
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
            viewValkyrie.UpdateAllUI(currentPlayerData,currentValkyrie);
            return;
        }

        // 获取当前玩家数据
        currentPlayerData = PlayerDataManager.Instance.CurrentPlayerData;

        // 检查是否成功获取玩家数据
        if (currentPlayerData == null)
        {
            Debug.LogWarning("当前没有登录的玩家，加载默认数据");
            LoadDefaultData();
            viewValkyrie.UpdateAllUI(currentPlayerData,currentValkyrie);
            return;
        }

        // 成功加载玩家数据
        Debug.Log($"成功加载玩家数据: {currentPlayerData.PlayerName}, 等级: {currentPlayerData.Level}, DailyEXP: {currentPlayerData.DailyEXP}");

        // 刷新玩家数据中的任务状态
        PlayerDataManager.Instance.RefreshTasks();
        viewValkyrie.UpdateAllUI(currentPlayerData, currentValkyrie);
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

        viewValkyrie.UpdateAllUI(currentPlayerData,currentValkyrie);
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
            CreateValkyrieItem(character, itemCount);
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

    //生成女武神项
    void CreateValkyrieItem(CharacterData valkyrie, int num)
    {
        GameObject valkyrieItemObj = Instantiate(valkyrieItemPrefab, valkyrieListContent);
        valkyrieItemObj.gameObject.SetActive(true);
        valkyrieItemObj.name = $"ValkyrieItem_{valkyrie.Id}_{valkyrie.Name}";

        // 确保预制体的RectTransform正确
        RectTransform rectTransform = valkyrieItemObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }

        ValkyrieItemUI valkyrieItemUI = valkyrieItemObj.GetComponent<ValkyrieItemUI>();

        if (valkyrieItemUI != null)
        {
            valkyrieItemUI.Initialize(num, valkyrie, this);
            valkyrieItemUIs.Add(valkyrieItemUI);
        }
        else
        {
            Debug.LogError("valkyrieItemPrefab上没有valkyrieItemUI组件！");
        }
    }

    public void ShowValkyrieSummary(int CurrentValkyrie)
    {
        currentValkyrie = CurrentValkyrie;
        viewValkyrie.UpdateAllUI(currentPlayerData,currentValkyrie);
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
