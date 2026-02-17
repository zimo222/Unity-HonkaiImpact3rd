using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PlayerDataManager;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class StigmataEnhanceView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ================== UI引用 ==================

    [Header("UI组件")]
    [Header("左上")]
    public Image illustrationImage;
    public Image iconImage;
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
    public GameObject[] statsPanel;
    public TMP_Text[,] statsText = new TMP_Text[4, 3];
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
    public void UpdateStigmataInfo(StigmataData stigmata, List<MaterialData> costMaterial)
    {
        EnhanceResultPanel.SetActive(false);
        if (stigmata == null) return;
        // 基本信息
        if (illustrationImage != null) illustrationImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stigmata/Illustration_{stigmata.Id}");
        if (iconImage != null) iconImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stigmata/SymIcon_{stigmata.Id}");
        if (nameText != null) nameText.text = stigmata.Name;
        if (starImage != null) starImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stars_{stigmata.Stats.Stars}S{stigmata.Stats.MaxStars}");
        if (sstarImage != null) sstarImage.sprite = Resources.Load<Sprite>($"Picture/Scene_EquipmentDetail/sStars_{stigmata.Stats.SStars}");

        int expGain = 0, expg = 0;
        foreach (MaterialData mat in costMaterial)
        {
            expGain += mat.Num * mat.Count;
            Debug.Log(mat.Num.ToString() + mat.Count.ToString());
        }
        expg = expGain;
        int toLevel = stigmata.Stats.Level, toExp = stigmata.Stats.Exp;
        while(expg >= toLevel * 100 - toExp && toLevel < (20 * stigmata.Stats.Stars + 5 * stigmata.Stats.SStars - 10))
        {
            expg -= toLevel * 100 - toExp;
            toLevel++;
            toExp = 0;
        }
        toExp = expg;

        if (coinCostText != null) coinCostText.text = PlayerDataManager.Instance.CalculateEnhanceCoinCost(costMaterial).ToString();
        if (expGainText != null) expGainText.text = expGain.ToString();

        if (levelText != null) levelText.text = $"{stigmata.Stats.Level}/<color=#FEDF4C> >> </color><color=#00C3FF>{toLevel}</color>";
        if (expText != null) expText.text = $"{stigmata.Stats.Exp}/<color=#FEDF4C>{100 * stigmata.Stats.Level}</color>";

        for (int i = 0; i < 4; i++)
        {
            TMP_Text[] texts = statsPanel[i].GetComponentsInChildren<TMP_Text>();
            statsText[i, 0] = texts[0];
            statsText[i, 1] = texts[1];
            statsText[i, 2] = texts[2];
        }
        int count = 0;
        if (stigmata.Stats.Health != 0)
        {
            statsText[count, 0].text = "生命";
            statsText[count, 1].text = stigmata.Stats.Health.ToString();
            statsText[count, 2].text = $"[+{(toLevel - stigmata.Stats.Level) * 2}]";
            count++;
        }
        if (stigmata.Stats.Attack != 0)
        {
            statsText[count, 0].text = "攻击";
            statsText[count, 1].text = stigmata.Stats.Attack.ToString();
            statsText[count, 2].text = $"[+{(toLevel - stigmata.Stats.Level)}]";
            count++;
        }
        if (stigmata.Stats.Defence != 0)
        {
            statsText[count, 0].text = "防御";
            statsText[count, 1].text = stigmata.Stats.Defence.ToString();
            statsText[count, 2].text = $"[+{(toLevel - stigmata.Stats.Level)}]";
            count++;
        }
        if (stigmata.Stats.CritRate * 100.0 != 0.0)
        {
            statsText[count, 0].text = "会心";
            statsText[count, 1].text = ((int)(stigmata.Stats.CritRate * 100)).ToString();
            statsText[count, 2].text = $"[+{(toLevel / 5 - stigmata.Stats.Level / 5)}]";
            count++;
        }
        if (count == 3)
        {
            statsPanel[3].SetActive(false);
        }
        else
        {
            statsPanel[3].SetActive(true);
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
