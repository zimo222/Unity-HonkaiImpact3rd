using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class EnhanceView : MonoBehaviour
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
        /*
        if (typeText != null) typeText.text = GetWeaponTypeName(weapon.Type);

        if (descriptionText != null) descriptionText.text = weapon.TextStats.Description;

        Debug.Log(weapon.Stats.Stars);
        Debug.Log(weapon.Stats.SStars);
        // 装备状态
        bool isEquipped = weapon.EquippedToCharacterIndex >= 0;
        if (equippedBadge != null) equippedBadge.SetActive(isEquipped);
        if (equippedCharacterText != null)
        {
            equippedCharacterText.text = isEquipped
                ? $"已装备给: 角色{weapon.EquippedToCharacterIndex + 1}"
                : "未装备";
        }
        */
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
