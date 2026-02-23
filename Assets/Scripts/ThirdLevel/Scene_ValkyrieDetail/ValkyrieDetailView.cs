using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
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

    // ========================= 角色升级晋升UI引用 =========================
    [Header("角色升级晋升")]
    public TMP_Text expGainText;
    public TMP_Text levelToText;
    public TMP_Text aText;
    public TMP_Text bText;
    public TMP_Text cText;
    public TMP_Text dText;
    public TMP_Text costText;
    public Image beforeStarImage;
    public Image afterStarImage;
    public TMP_Text beforeText;
    public TMP_Text afterText;
    public TMP_Text PlusText;
    public TMP_Text numText;

    // ========================= 武器圣痕替换UI引用 =========================
    [Header("武器圣痕替换")]
    public Transform equipmentListContent;  // 装备/材料列表容器
    public GameObject equipmentItemPrefab;  // 装备项预制体
    public GameObject[] statsPanel;
    public TMP_Text[,] statsText = new TMP_Text[4, 3];
    public TMP_Text equipmentNameText;
    public TMP_Text equipmentLevelText;
    public TMP_Text updateText;

    // Start is called before the first frame update
    void Start()
    {
        //武器替换
        for (int i = 0; i < 4; i++)
        {
            TMP_Text[] texts = statsPanel[i].GetComponentsInChildren<TMP_Text>();
            statsText[i, 0] = texts[0];
            statsText[i, 1] = texts[1];
            statsText[i, 2] = texts[2];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // ==================== 基础UI ====================
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

        Update1PanelUI(currentPlayerData, currentValkyrie);
        Update2PanelUI(currentPlayerData, currentValkyrie);
        Update3PanelUI(currentPlayerData, currentValkyrie);
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

    // ==================== 角色升级晋升 ====================
    public void UpdateLevelUpUI(PlayerData currentPlayerData, int currentValkyrie, string nowMaterialName, int cost, int beforeLevel, int toLevel)
    {
        if (LevelText1 != null) LevelText1.text = "LV." + currentPlayerData.Characters[currentValkyrie].BaseStats.Level.ToString();

        if (expGainText != null) expGainText.text = (currentPlayerData.MaterialBag.Find(c => c.Id == nowMaterialName).Num * cost).ToString();
        if (levelToText != null) levelToText.text = beforeLevel.ToString() + " >> " + toLevel.ToString();
        if (aText != null) aText.text = (currentPlayerData.MaterialBag.Find(m => m.Id == "MATE_004").Count).ToString();
        if (bText != null) bText.text = (currentPlayerData.MaterialBag.Find(m => m.Id == "MATE_003").Count).ToString();
        if (cText != null) cText.text = (currentPlayerData.MaterialBag.Find(m => m.Id == "MATE_002").Count).ToString();
        if (dText != null) dText.text = (currentPlayerData.MaterialBag.Find(m => m.Id == "MATE_001").Count).ToString();
        if (costText != null) costText.text = (cost).ToString();
    }

    public void UpdatePromotionUI(PlayerData currentPlayerData, int currentValkyrie)
    {
        if (beforeStarImage != null) beforeStarImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}S");
        if (afterStarImage != null) afterStarImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars + 1}S");
        CharacterStats stat = PlayerDataManager.Instance.GetCharacterTotalStats(currentValkyrie);
        if (beforeText != null) beforeText.text = stat.Health.ToString() + "\n\n" + stat.Attack.ToString() + "\n\n" + stat.Defence.ToString() + "\n\n" + ((int)(stat.CritRate * 100)).ToString() + "\n\n" + ((int)(stat.CritDamage * 100)).ToString();
        CharacterStats toStat = stat * (1.0 * (currentPlayerData.Characters[currentValkyrie].BaseStats.Stars + 1 + 3) / (currentPlayerData.Characters[currentValkyrie].BaseStats.Stars + 3));
        if (afterText != null) afterText.text = toStat.Health.ToString() + "\n\n" + toStat.Attack.ToString() + "\n\n" + toStat.Defence.ToString() + "\n\n" + ((int)(toStat.CritRate * 100)).ToString() + "\n\n" + ((int)(toStat.CritDamage * 100)).ToString();
        CharacterStats addStat = toStat - stat;
        if(PlusText != null ) PlusText.text = addStat.Health.ToString() + "\n\n" + addStat.Attack.ToString() + "\n\n" + addStat.Defence.ToString() + "\n\n" + ((int)(-(int)(stat.CritRate * 100) + (int)(toStat.CritRate * 100))).ToString() + "\n\n" + ((int)(-(int)(stat.CritDamage * 100)) + (int)(toStat.CritDamage * 100)).ToString();
        if (numText != null) numText.text = "50/" + currentPlayerData.Characters[currentValkyrie].BaseStats.Fragments.ToString();
    }

    public void Update1PanelUI(PlayerData currentPlayerData, int currentValkyrie)
    {
        //右面板
        CharacterStats stat = PlayerDataManager.Instance.GetCharacterTotalStats(currentValkyrie);
        if (StarImage1 != null) StarImage1.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}S");
        if (HealthText != null) HealthText.text = stat.Health.ToString();
        if (AttackText != null) AttackText.text = stat.Attack.ToString();
        if (DefenceText != null) DefenceText.text = stat.Defence.ToString();
        if (ElementBonusText != null) ElementBonusText.text = (stat.ElementBonus * 100).ToString();
        if (CritRateText != null) CritRateText.text = (stat.CritRate * 100).ToString();
        if (CritDamageText != null) CritDamageText.text = (stat.CritDamage * 100).ToString();
        if(stat.Level == 80)
        {
            if (ExpText != null) ExpText.text = (16000).ToString() + '/' + (16000).ToString();
        }
        else
        {
            if (ExpText != null) ExpText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.Exp).ToString() + '/' + (currentPlayerData.Characters[currentValkyrie].BaseStats.Level * 200).ToString();
        }
        
        if (StarImage2 != null) StarImage2.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}S");
        if (FragmentText != null) FragmentText.text = (currentPlayerData.Characters[currentValkyrie].BaseStats.Fragments).ToString() + "/50";
    }

    // ==================== 武器圣痕替换 ====================
    public void UpdateEquipmentUI(EquipmentData beforeEquipment, EquipmentData afterEquipment)
    {
        if(beforeEquipment == afterEquipment)
        {
            updateText.text = "卸下";
        }
        else
        {
            updateText.text = "装备";
        }
            int count = 0;
        if (afterEquipment.Stats.Health != 0)
        {
            statsText[count, 0].text = "生命";
            statsText[count, 1].text = beforeEquipment.Stats.Health.ToString();
            statsText[count, 2].text = afterEquipment.Stats.Health.ToString();
            count++;
        }
        if (afterEquipment.Stats.Attack != 0)
        {
            statsText[count, 0].text = "攻击";
            statsText[count, 1].text = beforeEquipment.Stats.Attack.ToString();
            statsText[count, 2].text = afterEquipment.Stats.Attack.ToString();
            count++;
        }
        if (afterEquipment.Stats.Defence != 0)
        {
            statsText[count, 0].text = "防御";
            statsText[count, 1].text = beforeEquipment.Stats.Defence.ToString();
            statsText[count, 2].text = afterEquipment.Stats.Defence.ToString();
            count++;
        }
        if (afterEquipment.Stats.CritRate * 100.0 != 0.0)
        {
            statsText[count, 0].text = "会心";
            statsText[count, 1].text = ((int)(beforeEquipment.Stats.CritRate * 100)).ToString();
            statsText[count, 2].text = ((int)(afterEquipment.Stats.CritRate * 100)).ToString();
            count++;
        }
        for (int i = 0; i < count; i++)
        {
            statsPanel[i].SetActive(true);
        }
        for (int i = count; i < 4; i++)
        {
            statsPanel[i].SetActive(false);
        }

        if (equipmentNameText != null) equipmentNameText.text = afterEquipment.Name;
        if (equipmentLevelText != null) equipmentLevelText.text = "Lv." + afterEquipment.Stats.Level.ToString();
    }

    public void Update2PanelUI(PlayerData currentPlayerData, int currentValkyrie)
    {
        if (WeaponNameText != null) WeaponNameText.text = (currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Name : "武器");
        if (WeaponStarImage != null) WeaponStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Stars_{currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Stats.Stars}S") : null;
        if (WeaponLevelText != null) WeaponLevelText.text = (currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? "Lv." + currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Stats.Level.ToString() : "");
    }

    public void Update3PanelUI(PlayerData currentPlayerData, int currentValkyrie)
    {
        if (StigmataTOPImage != null) StigmataTOPImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Portrait/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Weapon/None");
        if (StigmataTOPNameText != null) StigmataTOPNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Name : "圣痕上位";
        if (StigmataTOPStarImage != null) StigmataTOPStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Stats.Stars}S") : Resources.Load<Sprite>($"Picture/Weapon/None");
        if (StigmataTOPLevelText != null) StigmataTOPLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Stats.Level.ToString() : "";

        if (StigmataMIDImage != null) StigmataMIDImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Portrait/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Weapon/None");
        if (StigmataMIDNameText != null) StigmataMIDNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Name : "圣痕中位";
        if (StigmataMIDStarImage != null) StigmataMIDStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Stats.Stars}S") : Resources.Load<Sprite>($"Picture/Weapon/None");
        if (StigmataMIDLevelText != null) StigmataMIDLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Stats.Level.ToString() : "";

        if (StigmataBOTImage != null) StigmataBOTImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Portrait/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Weapon/None");
        if (StigmataBOTNameText != null) StigmataBOTNameText.text = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Name : "圣痕下位";
        if (StigmataBOTStarImage != null) StigmataBOTStarImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/Stars_{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Stats.Stars}S") : Resources.Load<Sprite>($"Picture/Weapon/None");
        if (StigmataBOTLevelText != null) StigmataBOTLevelText.text = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? "Lv." + currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Stats.Level.ToString() : "";
    }

    public void UpdateChoiseState(GameObject aEquipment, GameObject bEquipment)
    {
        if(aEquipment != null)
        {
            DisaplayEquipmentItemView aitemView = aEquipment.GetComponent<DisaplayEquipmentItemView>();
            aitemView.UpdateChoiseState();
        }
        DisaplayEquipmentItemView bitemView = bEquipment.GetComponent<DisaplayEquipmentItemView>();
        bitemView.UpdateChoiseState();
    }
    public void UpdateEquipState(GameObject aEquipment, GameObject bEquipment)
    {
        if(aEquipment != null)
        {
            DisaplayEquipmentItemView aitemView = aEquipment.GetComponent<DisaplayEquipmentItemView>();
            aitemView.UpdateEquipState();
        }
        DisaplayEquipmentItemView bitemView = bEquipment.GetComponent<DisaplayEquipmentItemView>();
        bitemView.UpdateEquipState();
    }
}
