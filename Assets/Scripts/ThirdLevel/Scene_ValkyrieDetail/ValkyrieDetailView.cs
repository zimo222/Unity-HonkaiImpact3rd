using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ValkyrieDetailView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ==================== 左侧面板 ==================== 
    public TMP_Text Name2Text1;
    public Image ElementImage1;
    public TMP_Text LevelText1;
    // ==================== 右侧面板 ==================== 
    [Header("Panel1")]
    public Image StarImage1;
    public TMP_Text HealthText;
    public TMP_Text AttackText;
    public TMP_Text DefenceText;
    public TMP_Text ElementBonusText;
    public TMP_Text CritRateText;
    public TMP_Text CritDamageText;
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
    [SerializeField] private Vector3 spawnPosition = new Vector3(-37, 67, 5);
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;
    // 已生成的模型引用
    private GameObject spawnedModel;

    private void Awake()
    {
        SpawnModel("CHAR_001");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeUI()
    {
        // 设置默认文本
        if (staminaText != null) staminaText.text = "0/81";
        if (coinsText != null) coinsText.text = "0";
        if (crystalsText != null) crystalsText.text = "0";
    }

    public void UpdateAllUI(PlayerData currentPlayerData, int currentValkyrie)
    {
        if (currentPlayerData == null) return;
        if (staminaText != null) staminaText.text = currentPlayerData.Stamina.ToString() + '/' + (currentPlayerData.Level + 80).ToString();
        if (coinsText != null) coinsText.text = currentPlayerData.Coins.ToString();
        if (crystalsText != null) crystalsText.text = currentPlayerData.Crystals.ToString();
            SpawnModel(currentPlayerData.Characters[currentValkyrie].Id);


        string[] Name = currentPlayerData.Characters[currentValkyrie].Name.Split('-');
        //左面板
        if (ElementImage1 != null) ElementImage1.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/ElementIcon_{currentPlayerData.Characters[currentValkyrie].BaseStats.Element}");
        if (Name2Text1 != null) Name2Text1.text = Name[1];
        if (LevelText1 != null) LevelText1.text = "LV." + currentPlayerData.Characters[currentValkyrie].BaseStats.Level.ToString();
        //右面板
        CharacterStats stat = PlayerDataManager.Instance.GetCharacterTotalStats(currentValkyrie);
        if (StarImage1 != null) StarImage1.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}S");
        if (HealthText != null) HealthText.text = stat.Health.ToString();
        if (AttackText != null) AttackText.text = stat.Attack.ToString();
        if (DefenceText != null) DefenceText.text = stat.Defence.ToString();
        if (ElementBonusText != null) ElementBonusText.text = (stat.ElementBonus * 100).ToString();
        if (CritRateText != null) CritRateText.text = (stat.CritRate * 100).ToString();
        if (CritDamageText != null) CritDamageText.text = (stat.CritDamage * 100).ToString();
        if (ExpText != null) ExpText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.Exp).ToString() + '/' + (currentPlayerData.Characters[currentValkyrie].BaseStats.Level * 100).ToString();
        if (StarImage2 != null) StarImage2.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}S");
        if (FragmentText != null) FragmentText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.Fragments).ToString() + "/50";

        if (WeaponNameText != null) WeaponNameText.text = (currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Name : "无");
        if (WeaponStarImage != null) WeaponStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Stars_{currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Stats.Stars}S") : null;
        if (WeaponLevelText != null) WeaponLevelText.text = (currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? "Lv." + currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Stats.Level.ToString() : "");

        if (StigmataTOPImage != null) StigmataTOPImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Portrait/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");
        if (StigmataTOPNameText != null) StigmataTOPNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Name : "无";
        if (StigmataTOPStarImage != null) StigmataTOPStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Stats.Stars}S") : null;
        if (StigmataTOPLevelText != null) StigmataTOPLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Stats.Level.ToString() : "";

        if (StigmataMIDImage != null) StigmataMIDImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Portrait/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");
        if (StigmataMIDNameText != null) StigmataMIDNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Name : "无";
        if (StigmataMIDStarImage != null) StigmataMIDStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Stats.Stars}S") : null;
        if (StigmataMIDLevelText != null) StigmataMIDLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Stats.Level.ToString() : "";

        if (StigmataBOTImage != null) StigmataBOTImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Portrait/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Valkyrie/Stigmata/Icon_-1");
        if (StigmataBOTNameText != null) StigmataBOTNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Name : "无";
        if (StigmataBOTStarImage != null) StigmataBOTStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Stats.Stars}S") : null;
        if (StigmataBOTLevelText != null) StigmataBOTLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Stats.Level.ToString() : "";
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
