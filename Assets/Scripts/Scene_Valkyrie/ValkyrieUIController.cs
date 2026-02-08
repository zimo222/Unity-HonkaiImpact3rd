using DG.Tweening;
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
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text tiliText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    // =========================        UI0         =========================
    [Header("UI0")]
    // ==================== 左侧面板 ====================
    public TMP_Text Name1Text;
    public Image ElementImage;
    public TMP_Text ElementText;
    public TMP_Text Name2Text;
    public Image StarImage;
    public TMP_Text LevelText;
    public TMP_Text CombatPowerText;
    // ==================== 右侧面板 ====================
    public TMP_Text WeaponText;
    public Image TopStigmataImage;
    public Image MiddleStigmataImage;
    public Image BottomStigmataImage;
    [Header("女武神列表")]
    public Transform valkyrieListContent;     // 女武神列表容器
    public GameObject valkyrieItemPrefab;     // 女武神项预制体

    // =========================        UI1         =========================
    [Header("UI1")]
    // ==================== 左侧面板 ==================== 
    public TMP_Text Name2Text1;
    public Image ElementImage1;
    public TMP_Text LevelText1;
    // ==================== 右侧面板 ==================== 
    [Header("Panel1")]
    public Image StarImage1;
    public TMP_Text HealthText;
    public TMP_Text AttackText;
    public TMP_Text CritRateText;
    public TMP_Text CritDamageText;
    public TMP_Text ElementBonusText;
    public TMP_Text ExpText;
    public Image StarImage2;
    public TMP_Text FragmentText;
    [Header("Panel2")]
    public TMP_Text WeaponNameText;
    public Image WeaponStarImage;
    public TMP_Text WeaponLevelText;
    [Header("Panel3")]
    public Image StigmataTOPImage;
    public TMP_Text StigmataTOPNameText;
    public Image StigmataTOPStarImage;
    public TMP_Text StigmataTOPLevelText;
    public Image StigmataMIDImage;
    public TMP_Text StigmataMIDNameText;
    public Image StigmataMIDStarImage;
    public TMP_Text StigmataMIDLevelText;
    public Image StigmataBOTImage;
    public TMP_Text StigmataBOTNameText;
    public Image StigmataBOTStarImage;
    public TMP_Text StigmataBOTLevelText;



    // 生成的位置和旋转
    [SerializeField] private string modelPath = "Prefabs/Character/";
    [SerializeField] private Vector3 spawnPosition = new Vector3(-40, 120, 500);
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;
    // 已生成的模型引用
    private GameObject spawnedModel;

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
        PlayerDataManager.Instance.RefreshTasks();

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

        SpawnModel(currentPlayerData.Characters[currentValkyrie].Id);

        string[] Name = currentPlayerData.Characters[currentValkyrie].Name.Split('-');
        //UI1
        //左面板
        //上面板
        if (Name1Text != null)
            Name1Text.text = Name[0];
        if (ElementImage != null)
            ElementImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/ElementIcon_{currentPlayerData.Characters[currentValkyrie].BaseStats.Element}");
        if (ElementText != null)
            switch (currentPlayerData.Characters[currentValkyrie].BaseStats.Element)
            {
                case "SW":
                    ElementText.text = "生物";
                    ElementText.color = new Color(1, 178 / 255.0f, 45 / 255.0f, 1);
                    break;
                case "YN":
                    ElementText.text = "异能";
                    ElementText.color = new Color(1, 70 / 255.0f, 211 / 255.0f, 1);
                    break;
                case "JX":
                    ElementText.text = "机械";
                    ElementText.color = new Color(43 / 255.0f, 226 / 255.0f, 1, 255);
                    break;
            }
        if (Name2Text != null)
            Name2Text.text = Name[1];
        //下面板
        if (StarImage != null)
        {
            StarImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}");
        }
        if (LevelText != null)
            LevelText.text = "LV." + currentPlayerData.Characters[currentValkyrie].BaseStats.Level.ToString();
        //右面板
        //上面板
        if (WeaponText != null)
            WeaponText.text = currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Name : "无";
        //下面板
        if (TopStigmataImage != null)
            TopStigmataImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");
        if (MiddleStigmataImage != null)
            MiddleStigmataImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");
        if (BottomStigmataImage != null)
            BottomStigmataImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");

        //UI1
        //左面板
        if (ElementImage1 != null)
            ElementImage1.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/ElementIcon_{currentPlayerData.Characters[currentValkyrie].BaseStats.Element}");
        if (Name2Text1 != null)
            Name2Text1.text = Name[1];
        if (LevelText1 != null)
            LevelText1.text = "LV." + currentPlayerData.Characters[currentValkyrie].BaseStats.Level.ToString();
        //右面板
        if (StarImage1 != null)
        {
            StarImage1.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}");
        }
        if (HealthText != null)
            HealthText.text = currentPlayerData.Characters[currentValkyrie].BaseStats.Health.ToString();
        if (AttackText != null)
            AttackText.text = currentPlayerData.Characters[currentValkyrie].BaseStats.Attack.ToString();
        if (CritRateText != null)
            CritRateText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.CritRate * 100).ToString();
        if (CritDamageText != null)
            CritDamageText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.CritDamage * 100).ToString();
        if (ElementBonusText != null)
            ElementBonusText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.ElementBonus * 100).ToString();
        if (ExpText != null)
            ExpText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.Exp).ToString() + '/' + (currentPlayerData.Characters[currentValkyrie].BaseStats.Level * 100).ToString();
        if (StarImage2 != null)
             StarImage2.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}");
        if(FragmentText != null)
            FragmentText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.Fragments).ToString() + "/50";

        if (WeaponNameText != null)
            WeaponNameText.text = (currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Name : "无");
        if (WeaponStarImage != null)
            WeaponStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Stats.Stars}") : null;
        if (WeaponLevelText != null)
            WeaponLevelText.text = (currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? "Lv." + currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Stats.Level.ToString() : "");

        if (StigmataTOPImage != null)
            StigmataTOPImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Picture_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");
        if (StigmataTOPNameText != null)
            StigmataTOPNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Name : "无";
        if (StigmataTOPStarImage != null)
            StigmataTOPStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Stats.Stars}") : null;
        if (StigmataTOPLevelText != null)
            StigmataTOPLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Stats.Level.ToString() : "";
        if (StigmataMIDImage != null)
            StigmataMIDImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Picture_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");
        if (StigmataMIDNameText != null)
            StigmataMIDNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Name : "无";
        if (StigmataMIDStarImage != null)
            StigmataMIDStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Stats.Stars}") : null;
        if (StigmataMIDLevelText != null)
            StigmataMIDLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Stats.Level.ToString() : "";
        if (StigmataBOTImage != null)
            StigmataBOTImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Picture_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");
        if (StigmataBOTNameText != null)
            StigmataBOTNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Name : "无";
        if (StigmataBOTStarImage != null)
            StigmataBOTStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Stats.Stars}") : null;
        if (StigmataBOTLevelText != null)
            StigmataBOTLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Stats.Level.ToString() : "";

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

    public void SpawnModel(string id)
    {
        // 如果已有模型存在，先销毁
        if (spawnedModel != null)
        {
            Destroy(spawnedModel);
        }

        // 从Resources文件夹加载模型预设
        GameObject modelPrefab = Resources.Load<GameObject>(modelPath + id);

        if (modelPrefab != null)
        {
            // 实例化模型
            spawnedModel = Instantiate(modelPrefab, spawnPosition, spawnRotation);
            spawnedModel.name = "Spawned_Model";

            // 可选：将模型设置为当前游戏对象的子物体
            // spawnedModel.transform.parent = transform;

            Debug.Log($"成功生成模型: {modelPath}");
            ValkyrieCameraManager cameraManager = FindObjectOfType<ValkyrieCameraManager>();
            if (cameraManager != null)
            {
                DOVirtual.DelayedCall(0.3f, () => {
                    Debug.Log("1秒后执行");
                    cameraManager.SetPlayerModelFromSpawned();
                });
                
                Debug.Log("成了");
            }
        }
        else
        {
            Debug.LogError($"无法从路径加载模型: {modelPath}");
        }
    }
}