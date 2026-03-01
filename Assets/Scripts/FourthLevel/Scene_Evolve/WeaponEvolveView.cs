using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PlayerDataManager;
using static UnityEditor.Timeline.TimelinePlaybackControls;
using static WeaponDetailView;

public class WeaponEvolveView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ================== UI引用 ==================

    [Header("UI组件")]
    [Header("左上")]
    public Image type1Image;
    public TMP_Text name1Text;
    public TMP_Text level1Text;
    public TMP_Text stat1Text;
    public TMP_Text stat2Text;
    public Image star1Image;
    public Image sstar1Image;
    [Header("右上")]
    public Image type2Image;
    public TMP_Text name2Text;
    public TMP_Text level2Text;
    public TMP_Text Stat1Text;
    public TMP_Text Stat2Text;
    public TMP_Text addStat1Text;
    public TMP_Text addStat2Text;
    public Image star2Image;
    public Image sstar2Image;
    [Header("左中")]
    public GameObject[] skillPanel;
    public EquipmentSkillUI[] weaponSkillUI;
    [Header("右中")]
    public Transform materialListContent;  // 材料列表容器
    public GameObject materialItemPrefab;   // 材料项预制体
    [Header("左下")]
    public TMP_Text coinCostText;
    [Header("面板")]
    [SerializeField] public GameObject EnhanceResultPanel;

    // 生成的位置和旋转
    [SerializeField] private string modelPath = "Prefabs/Weapon/";
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 0, 500);
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;
    // 已生成的模型引用
    private GameObject spawnedModel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // ================== View的公共方法 ==================
    // 更新装备信息
    public void UpdateWeaponInfo(WeaponData weapon, List<MaterialData> costMaterial)
    {
        WeaponDefineSO nowWeapon = null;
        if (GameDataManager.Instance != null) nowWeapon = GameDataManager.Instance.WeaponDict[weapon.Id];

        SpawnModel(weapon);

        EnhanceResultPanel.SetActive(false);
        if (weapon == null) return;
        // 基本信息
        if (type1Image != null && type2Image != null) type1Image.sprite = type2Image.sprite = Resources.Load<Sprite>($"Picture/Scene/Scene_EquipmentDetail/Type_{weapon.Type}");
        if (name1Text != null && name2Text != null) name1Text.text = name2Text.text = weapon.Name;
        if (level1Text != null) level1Text.text = $"{weapon.Stats.Level}/<color=#FEDF4C>{20 * weapon.Stats.Stars + 5 * weapon.Stats.SStars - 10}</color>"; 
        if (level2Text != null) level2Text.text = $"{1}/<color=#FEDF4C>{(20 * weapon.Stats.Stars + 5 * weapon.Stats.SStars - 10 + 5)}</color>";
        if (star1Image != null) star1Image.sprite = Resources.Load<Sprite>($"Picture/Scene/Scene_Equipment/Stars_{weapon.Stats.Stars}S{weapon.Stats.MaxStars}");
        if (star2Image != null) star2Image.sprite = Resources.Load<Sprite>($"Picture/Scene/Scene_Equipment/Stars_{weapon.Stats.Stars + (weapon.Stats.SStars + 1) / 4}S{weapon.Stats.MaxStars}");
        if (sstar1Image != null) sstar1Image.sprite = Resources.Load<Sprite>($"Picture/Scene/Scene_EquipmentDetail/sStars_{weapon.Stats.SStars}");
        if (sstar2Image != null) sstar2Image.sprite = Resources.Load<Sprite>($"Picture/Scene/Scene_EquipmentDetail/sStars_{(weapon.Stats.SStars + 1) % 4}");


        //右中
        for (int i = 0; i < 1; i++)
        {
            skillPanel[i].SetActive(true);
            weaponSkillUI[i].nameText.text = nowWeapon.skill[i].name;
            weaponSkillUI[i].descriptionText.text = nowWeapon.skill[i].description;
        }

        foreach (MaterialData mat in costMaterial)
        {
            GameObject itemObj = Instantiate(materialItemPrefab, materialListContent);
            MaterialItemView itemView = itemObj.GetComponent<MaterialItemView>();
            if (itemView != null)
            {
                itemView.Initialize(mat, null);
            }
        }

        if (coinCostText != null) coinCostText.text = PlayerDataManager.Instance.CalculateEnhanceCoinCost(costMaterial).ToString();

        if (stat1Text != null && Stat1Text != null) stat1Text.text = Stat1Text.text = weapon.Attack.ToString();
        if (addStat1Text != null) addStat1Text.text = $"[+10]";
        if (stat2Text != null && Stat2Text != null) stat2Text.text = Stat2Text.text = ((int)(weapon.CritRate * 100)).ToString();
        if (addStat2Text != null) addStat2Text.text = $"[+2]";
    }

    // 更新玩家资源
    public void UpdatePlayerResources(PlayerData playerData)
    {
        if (playerData == null) return;
        if (staminaText != null) staminaText.text = playerData.Stamina.ToString();
        if (coinsText != null) coinsText.text = playerData.Coins.ToString();
        if (crystalsText != null) crystalsText.text = playerData.Crystals.ToString();
    }

    // 显示结果面板的方法
    // 修改方法签名，使用 object 而不是 dynamic
    public void ShowResultPanel(EvolutionResult result)
    {
        if (EnhanceResultPanel != null)
        {
            EnhanceResultPanel.SetActive(true);
            TMP_Text resultText = EnhanceResultPanel.GetComponentInChildren<TMP_Text>();
            if (resultText != null)
            {
                // 使用反射获取 success 属性
                try
                {
                    resultText.text = result.success == true ? "进化成功！" : result.message;
                }
                catch (Exception e)
                {
                    resultText.text = $"错误: {e.Message}";
                    Debug.LogError($"反射获取结果属性失败: {e.Message}");
                }
            }

            Button closeButton = EnhanceResultPanel.GetComponentInChildren<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => {
                    EnhanceResultPanel.SetActive(false);
                    SceneManager.LoadScene(SceneDataManager.Instance.PopPreviousScene());
                });
            }
        }
    }

    // 延迟隐藏面板的协程
    public IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (EnhanceResultPanel != null && EnhanceResultPanel.activeSelf)
        {
            EnhanceResultPanel.SetActive(false);
            SceneManager.LoadScene(SceneDataManager.Instance.PopPreviousScene());
        }
    }

    public void SpawnModel(WeaponData Weapon)
    {
        // 如果已有模型存在，先销毁
        if (spawnedModel != null)
        {
            Destroy(spawnedModel);
        }

        // 从Resources文件夹加载模型预设
        GameObject modelPrefab = Resources.Load<GameObject>(modelPath + Weapon.Id + "_" + Weapon.Stats.Stars.ToString());

        if (modelPrefab != null)
        {
            // 实例化模型
            spawnedModel = Instantiate(modelPrefab, spawnPosition, spawnRotation);
            spawnedModel.name = "Spawned_Model";

            // 可选：将模型设置为当前游戏对象的子物体
            // spawnedModel.transform.parent = transform;

            Debug.Log($"成功生成模型: {modelPath}");
        }
        else
        {
            Debug.Log($"无法从路径加载模型: {modelPath}");
        }
    }
}
