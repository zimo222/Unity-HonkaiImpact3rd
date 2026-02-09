using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PlayerDataManager;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class WeaponEnhanceView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ================== UI引用 ==================

    [Header("UI组件")]
    [Header("左上")]
    public Image typeImage;
    public TMP_Text nameText;
    public Image starImage;
    public Image sstarImage;
    [Header("左中")]
    public TMP_Text coinCostText;
    public TMP_Text expGainText;
    [Header("左下")]
    public TMP_Text levelText;
    public TMP_Text expText;
    [Header("右上")]
    public TMP_Text stat1Text;
    public TMP_Text addStat1Text;
    public TMP_Text stat2Text;
    public TMP_Text addStat2Text;
    [Header("右中")]
    public Transform materialListContent;  // 材料列表容器
    public GameObject materialItemPrefab;   // 材料项预制体

    [Header("面板")]
    [SerializeField] public GameObject EnhanceResultPanel;

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
        EnhanceResultPanel.SetActive(false);
        if (weapon == null) return;
        // 基本信息
        if (typeImage != null) typeImage.sprite = Resources.Load<Sprite>($"Picture/Scene_EquipmentDetail/Type_{weapon.Type}");
        if (nameText != null) nameText.text = weapon.Name;
        if (starImage != null) starImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stars_{weapon.Stats.Stars}S{weapon.Stats.MaxStars}");
        if (sstarImage != null) sstarImage.sprite = Resources.Load<Sprite>($"Picture/Scene_EquipmentDetail/sStars_{weapon.Stats.SStars}");

        int expGain = 0, expg = 0;
        foreach (MaterialData mat in costMaterial)
        {
            expGain += mat.Num * mat.Count;
            Debug.Log(mat.Num.ToString() + mat.Count.ToString());
        }
        expg = expGain;
        int toLevel = weapon.Stats.Level, toExp = weapon.Stats.Exp;
        while(expg >= toLevel * 100 - toExp && toLevel < (20 * weapon.Stats.Stars + 5 * weapon.Stats.SStars - 10))
        {
            expg -= toLevel * 100 - toExp;
            toLevel++;
            toExp = 0;
        }
        toExp = expg;

        if (coinCostText != null) coinCostText.text = PlayerDataManager.Instance.CalculateEnhanceCoinCost(costMaterial).ToString();
        if (expGainText != null) expGainText.text = expGain.ToString();

        if (levelText != null) levelText.text = $"{weapon.Stats.Level}/<color=#FEDF4C> >> </color><color=#00C3FF>{toLevel}</color>";
        if (expText != null) expText.text = $"{weapon.Stats.Exp}/<color=#FEDF4C>{100 * weapon.Stats.Level}</color>";

        if (stat1Text != null) stat1Text.text = weapon.Attack.ToString();
        if (addStat1Text != null) addStat1Text.text = $"[+{(toLevel - weapon.Stats.Level)}]";
        if (stat2Text != null) stat2Text.text = ((int)(weapon.CritRate * 100)).ToString();
        if (addStat2Text != null) addStat2Text.text = $"[+{(toLevel / 5 - weapon.Stats.Level / 5)}]";

        foreach(MaterialData mat in costMaterial)
        {
            GameObject itemObj = Instantiate(materialItemPrefab, materialListContent);
            MaterialItemView itemView = itemObj.GetComponent<MaterialItemView>();
            if (itemView != null)
            {
                itemView.Initialize(mat, null);
            }
        }
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
    public void ShowResultPanel(EnhancementResult result)
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
                    resultText.text = result.success == true ? "强化成功！" : result.message;
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
}
