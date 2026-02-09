using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PlayerDataManager;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class StigmataEvolveView : MonoBehaviour
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
    public TMP_Text levelText;
    public Image starImage;
    public Image sstarImage;
    public GameObject[] stats1Panel;
    public TMP_Text[,] stats1Text = new TMP_Text[4, 2];
    public GameObject[] stats2Panel;
    public TMP_Text[,] stats2Text = new TMP_Text[4, 3];
    [Header("右中")]
    public Transform materialListContent;  // 材料列表容器
    public GameObject materialItemPrefab;   // 材料项预制体
    [Header("左下")]
    public TMP_Text coinCostText;
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
        if (levelText != null) levelText.text = $"{stigmata.Stats.Level}/<color=#FEDF4C>{20 * stigmata.Stats.Stars + 5 * stigmata.Stats.SStars - 10}</color>";
        if (starImage != null) starImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stars_{stigmata.Stats.Stars + (stigmata.Stats.SStars + 1) / 4}S{stigmata.Stats.MaxStars}");
        if (sstarImage != null) sstarImage.sprite = Resources.Load<Sprite>($"Picture/Scene_EquipmentDetail/sStars_{(stigmata.Stats.SStars + 1) % 4}");

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

        for (int i = 0; i < 4; i++)
        {
            TMP_Text[] texts = stats1Panel[i].GetComponentsInChildren<TMP_Text>();
            stats1Text[i, 0] = texts[0];
            stats1Text[i, 1] = texts[1];
        }
        int count = 0;
        if (stigmata.Stats.Health != 0)
        {
            stats1Text[count, 0].text = "生命";
            stats1Text[count, 1].text = stigmata.Stats.Health.ToString();
            count++;
        }
        if (stigmata.Stats.Attack != 0)
        {
            stats1Text[count, 0].text = "攻击";
            stats1Text[count, 1].text = stigmata.Stats.Attack.ToString();
            count++;
        }
        if (stigmata.Stats.Defence != 0)
        {
            stats1Text[count, 0].text = "防御";
            stats1Text[count, 1].text = stigmata.Stats.Defence.ToString();
            count++;
        }
        if (stigmata.Stats.CritRate * 100.0 != 0.0)
        {
            stats1Text[count, 0].text = "会心";
            stats1Text[count, 1].text = ((int)(stigmata.Stats.CritRate * 100)).ToString();
            count++;
        }
        if (count == 3)
        {
            stats1Panel[3].SetActive(false);
        }
        else
        {
            stats1Panel[3].SetActive(true);
        }

        for (int i = 0; i < 4; i++)
        {
            TMP_Text[] texts = stats2Panel[i].GetComponentsInChildren<TMP_Text>();
            stats2Text[i, 0] = texts[0];
            stats2Text[i, 1] = texts[1];
            stats2Text[i, 2] = texts[2];
        }
        count = 0;
        if (stigmata.Stats.Health != 0)
        {
            stats2Text[count, 0].text = "生命";
            stats2Text[count, 1].text = (stigmata.Stats.Health + 20).ToString();
            stats2Text[count, 2].text = $"[+20]";
            count++;
        }
        if (stigmata.Stats.Attack != 0)
        {
            stats2Text[count, 0].text = "攻击";
            stats2Text[count, 1].text = (stigmata.Stats.Attack + 10).ToString();
            stats2Text[count, 2].text = $"[+10]";
            count++;
        }
        if (stigmata.Stats.Defence != 0)
        {
            stats2Text[count, 0].text = "防御";
            stats2Text[count, 1].text = (stigmata.Stats.Defence + 10).ToString();
            stats2Text[count, 2].text = $"[+10]";
            count++;
        }
        if (stigmata.Stats.CritRate * 100.0 != 0.0)
        {
            stats2Text[count, 0].text = "会心";
            stats2Text[count, 1].text = ((int)(stigmata.Stats.CritRate * 100) + 2).ToString();
            stats2Text[count, 2].text = $"[+2]";
            count++;
        }
        if (count == 3)
        {
            stats2Panel[3].SetActive(false);
        }
        else
        {
            stats2Panel[3].SetActive(true);
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
}
