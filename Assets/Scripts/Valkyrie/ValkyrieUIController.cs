using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ValkyrieUIController : MonoBehaviour
{
    // ================== 基础玩家信息UI引用 ==================
    [Header("资源信息")]
    public TMP_Text tiliText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ================== 按钮引用 (可选) ==================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    // ====================== 左侧面板 =========================
    [Header("上面板")]
    public TMP_Text Name1Text;
    public Image ElementImage;
    public TMP_Text ElementText;
    public TMP_Text Name2Text;
    [Header("下面板")]
    public Image StarImage;
    public TMP_Text LevelText;
    public TMP_Text CombatPowerText;


    [Header("女武神列表")]
    public Transform valkyrieListContent;     // 女武神列表容器
    public GameObject valkyrieItemPrefab;     // 女武神项预制体

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;
    private List<ValkyrieItemUI> valkyrieItemUIs = new List<ValkyrieItemUI>();
    private int currentValkyrie = 0;

    void Start()
    {
        InitializeUI();
        LoadPlayerData();
        UpdateAllUI();
        LoadAllValkyries(); // 加载所有任务
    }

    void InitializeUI()
    {
        // 设置默认文本
        if (tiliText != null) tiliText.text = "0/81";
        if (coinsText != null) coinsText.text = "0";
        if (crystalsText != null) crystalsText.text = "0";
        if (Name1Text != null) Name1Text.text = "Name1";
        if (Name2Text != null) Name2Text.text = "Name2";
        if (LevelText != null) LevelText.text = "Lv.1";
        if (CombatPowerText != null) CombatPowerText.text = "0";
    }

    void LoadPlayerData()
    {

        // 检查PlayerDataManager是否存在
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("PlayerDataManager.Instance 为空，加载默认数据");
            LoadDefaultData();
            UpdateAllUI();
            return;
        }

        // 获取当前玩家数据
        currentPlayerData = PlayerDataManager.Instance.CurrentPlayerData;

        // 检查是否成功获取玩家数据
        if (currentPlayerData == null)
        {
            Debug.LogWarning("当前没有登录的玩家，加载默认数据");
            LoadDefaultData();
            UpdateAllUI();
            return;
        }

        // 成功加载玩家数据
        Debug.Log($"成功加载玩家数据: {currentPlayerData.PlayerName}, 等级: {currentPlayerData.Level}, DailyEXP: {currentPlayerData.DailyEXP}");

        // 刷新玩家数据中的任务状态
        currentPlayerData.RefreshTasks();

        UpdateAllUI();
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

        UpdateAllUI();
    }

    void UpdateAllUI()
    {
        if (currentPlayerData == null) return;
        UpdateResources();
    }

    void UpdateResources()
    {
        if (currentPlayerData == null) return;

        if (tiliText != null)
            tiliText.text = currentPlayerData.Stamina.ToString() + '/' + (currentPlayerData.Level + 80).ToString();

        if (coinsText != null)
            coinsText.text = currentPlayerData.Coins.ToString();

        if (crystalsText != null)
            crystalsText.text = currentPlayerData.Crystals.ToString();

        string[] Name = currentPlayerData.Characters[currentValkyrie].Name.Split('-');

        //上面板
        if (Name1Text != null)
            Name1Text.text = Name[0];

        if(ElementImage != null)
            ElementImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/ElementIcon_{currentPlayerData.Characters[currentValkyrie].BaseStats.Element}");

        if(ElementText != null)
            switch(currentPlayerData.Characters[currentValkyrie].BaseStats.Element)
            {
                case "SW":
                    ElementText.text = "生物";
                    ElementText.color = new Color(1, 178/255.0f, 45/255.0f, 1);
                    break;
                case "YN":
                    ElementText.text = "异能";
                    ElementText.color = new Color(1, 70/255.0f, 211/255.0f, 1);
                    break;
                case "JX":
                    ElementText.text = "机械";
                    ElementText.color = new Color(43/255.0f, 226/255.0f, 1, 255);
                    break;
            }

        if (Name2Text != null)
            Name2Text.text = Name[1];

        //下面板
        if(StarImage != null)
        {
            StarImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}");
        }

        if (LevelText != null)
            LevelText.text = "LV." + currentPlayerData.Characters[currentValkyrie].BaseStats.Level.ToString();
    }

    // ================== 女武神列表管理 ==================
    public void LoadAllValkyries()
    {
        if (currentPlayerData == null || valkyrieListContent == null || valkyrieItemPrefab == null)
            return;

        // 清除现有女武神项
        ClearValkyrieList();

        // 获取所有女武神（不筛选频率）
        List<CharacterData> allCharacters = currentPlayerData.GetSortedCharacters(null);

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
        UpdateAllUI();
    }

    IEnumerator RebuildLayout()
    {
        // 等待一帧让Unity更新布局
        yield return null;

        // 强制重建布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(valkyrieListContent as RectTransform);
    }
}