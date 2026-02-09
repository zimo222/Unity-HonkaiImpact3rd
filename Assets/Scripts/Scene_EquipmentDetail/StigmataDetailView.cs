using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StigmataDetailView : MonoBehaviour
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
    public GameObject equippedBadge;
    public TMP_Text equippedCharacterText;
    [Header("左中")]
    public TMP_Text descriptionText;
    [Header("左下")]
    public TMP_Text levelText;
    public TMP_Text expText;
    [Header("右上")]
    public GameObject[] statsPanel;
    public TMP_Text[, ] statsText = new TMP_Text[4, 2]; 

    // ================== View的公共方法 ==================
    // 更新装备信息
    public void UpdateStigmataInfo(StigmataData stigmata)
    {
        if (stigmata == null) return;
        // 基本信息
        if (illustrationImage != null) illustrationImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stigmata/Illustration_{stigmata.Id}");
        if (iconImage != null) iconImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stigmata/SymIcon_{stigmata.Id}");
        if (nameText != null) nameText.text = stigmata.Name;
        if (starImage != null) starImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stars_{stigmata.Stats.Stars}S{stigmata.Stats.MaxStars}");
        if (sstarImage != null) sstarImage.sprite = Resources.Load<Sprite>($"Picture/Scene_EquipmentDetail/sStars_{stigmata.Stats.SStars}");

        if (descriptionText != null) descriptionText.text = stigmata.TextStats.Description;

        if (levelText != null) levelText.text = $"{stigmata.Stats.Level}/<color=#FEDF4C>{20 * stigmata.Stats.Stars + 5 * stigmata.Stats.SStars - 10}</color>";
        if (expText != null) expText.text = $"{stigmata.Stats.Exp}/<color=#FEDF4C>{100 * stigmata.Stats.Level}</color>";
        // 装备状态
        bool isEquipped = stigmata.EquippedToCharacterIndex >= 0;
        if (equippedBadge != null) equippedBadge.SetActive(isEquipped);
        if (equippedCharacterText != null)
        {
            equippedCharacterText.text = isEquipped
                ? $"已装备给: 角色{stigmata.EquippedToCharacterIndex + 1}"
                : "未装备";
        }

        for (int i = 0; i < 4; i++)
        {
            TMP_Text[] texts = statsPanel[i].GetComponentsInChildren<TMP_Text>();
            statsText[i, 0] = texts[0];
            statsText[i, 1] = texts[1];
        }

        int count = 0;
        if (stigmata.Stats.Health != 0)
        {
            statsText[count, 0].text = "生命";
            statsText[count, 1].text = stigmata.Stats.Health.ToString();
            count++;
        }
        if (stigmata.Stats.Attack != 0)
        {
            statsText[count, 0].text = "攻击";
            statsText[count, 1].text = stigmata.Stats.Attack.ToString();
            count++;
        }
        if (stigmata.Stats.Defence != 0)
        {
            statsText[count, 0].text = "防御";
            statsText[count, 1].text = stigmata.Stats.Defence.ToString();
            count++;
        }
        if (stigmata.Stats.CritRate * 100.0 != 0.0)
        {
            statsText[count, 0].text = "会心";
            statsText[count, 1].text = ((int)(stigmata.Stats.CritRate * 100)).ToString();
            count++;
        }
        if(count == 3)
        {
            statsPanel[0].transform.localPosition = new Vector2(175f - 577.5f, -108 + 108);
            statsPanel[1].transform.localPosition = new Vector2(563 - 577.5f, -108 + 108);
            statsPanel[2].transform.localPosition = new Vector2(951 - 577.5f, -108 + 108);
            statsPanel[3].SetActive(false);
        }
        else
        {
            statsPanel[0].transform.localPosition = new Vector2(220 - 577.5f, -58 + 108);
            statsPanel[1].transform.localPosition = new Vector2(815 - 577.5f, -58 + 108);
            statsPanel[2].transform.localPosition = new Vector2(220 - 577.5f, -158 + 108);
            statsPanel[3].SetActive(true);
            statsPanel[3].transform.localPosition = new Vector2(815 - 577.5f, -158 + 108);
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
}