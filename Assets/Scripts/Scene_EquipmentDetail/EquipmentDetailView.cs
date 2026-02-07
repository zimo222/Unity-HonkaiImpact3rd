using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EquipmentDetailView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ================== UI引用 ==================
    [Header("UI组件")]
    [Header("左上")]
    public TMP_Text nameText;
    public Image typeImage;
    public TMP_Text typeText;
    public Image starImage;
    public GameObject equippedBadge;
    public TMP_Text equippedCharacterText;
    [Header("左中")]
    public TMP_Text descriptionText;
    [Header("左下")]
    public TMP_Text levelText;
    public TMP_Text expText;
    [Header("右上")]
    public Transform statPanelsParent;           // 存放所有属性面板的父对象
    public GameObject statPanelPrefab;           // 属性面板的预制体
    public Vector2 panelStartPosition = new Vector2(0, 0); // 起始位置
    public float panelSpacing = 60f;            // 面板之间的间距

    // 存储动态生成的面板
    private List<GameObject> dynamicPanels = new List<GameObject>();

    // ================== View的公共方法 ==================

    // 更新装备信息
    public void UpdateWeaponInfo(WeaponData weapon)
    {
        if (weapon == null) return;

        // 基本信息
        if (nameText != null) nameText.text = weapon.Name;
        if (typeText != null) typeText.text = weapon.Type.ToString();
        if (descriptionText != null) descriptionText.text = weapon.TextStats.Description;
        if (levelText != null) levelText.text = $"{weapon.Stats.Level}/<color=#FEDF4C>30</color>";

        // 装备状态
        bool isEquipped = weapon.EquippedToCharacterIndex >= 0;
        if (equippedBadge != null) equippedBadge.SetActive(isEquipped);

        if (equippedCharacterText != null)
        {
            equippedCharacterText.text = isEquipped
                ? $"已装备给: 角色{weapon.EquippedToCharacterIndex + 1}"
                : "未装备";
        }

        // 动态创建属性面板
        CreateDynamicStatPanels(weapon.Stats);
    }

    // 更新玩家资源
    public void UpdatePlayerResources(PlayerData playerData)
    {
        if (playerData == null) return;

        if (staminaText != null) staminaText.text = playerData.Stamina.ToString();
        if (coinsText != null) coinsText.text = playerData.Coins.ToString();
        if (crystalsText != null) crystalsText.text = playerData.Crystals.ToString();
    }

    // 动态创建属性面板
    private void CreateDynamicStatPanels(CharacterStats stats)
    {
        // 清理旧面板
        ClearDynamicPanels();

        if (statPanelPrefab == null || statPanelsParent == null) return;

        // 收集要显示的属性
        List<StatDisplayInfo> statsToShow = GetStatsToDisplay(stats);

        // 动态创建面板
        for (int i = 0; i < statsToShow.Count; i++)
        {
            var statInfo = statsToShow[i];
            GameObject panel = Instantiate(statPanelPrefab, statPanelsParent);
            panel.SetActive(true);

            // 设置位置
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -i * 60); // 每个面板间隔60像素

            // 设置文本
            TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length >= 2)
            {
                texts[0].text = statInfo.displayName;
                texts[1].text = statInfo.displayValue;
            }

            dynamicPanels.Add(panel);
        }
    }

    // 获取要显示的属性列表
    private List<StatDisplayInfo> GetStatsToDisplay(CharacterStats stats)
    {
        List<StatDisplayInfo> statsList = new List<StatDisplayInfo>();

        // 遍历所有属性，只显示非零值
        if (stats.Health > 0)
            statsList.Add(new StatDisplayInfo("生命值", stats.Health.ToString()));

        if (stats.Attack > 0)
            statsList.Add(new StatDisplayInfo("攻击力", stats.Attack.ToString()));

        if (stats.Defence > 0)
            statsList.Add(new StatDisplayInfo("防御力", stats.Defence.ToString()));

        if (stats.CritRate > 0)
            statsList.Add(new StatDisplayInfo("暴击率", $"{stats.CritRate:P1}"));

        if (stats.CritDamage > 0)
            statsList.Add(new StatDisplayInfo("暴击伤害", $"{stats.CritDamage:P1}"));

        if (stats.ElementBonus > 0)
            statsList.Add(new StatDisplayInfo("元素加成", $"{stats.ElementBonus:P1}"));

        // 总是显示等级和星级
        statsList.Add(new StatDisplayInfo("等级", stats.Level.ToString()));

        if (!string.IsNullOrEmpty(stats.Stars))
            statsList.Add(new StatDisplayInfo("星级", stats.Stars));

        return statsList;
    }

    // 清理动态面板
    private void ClearDynamicPanels()
    {
        foreach (var panel in dynamicPanels)
        {
            if (panel != null) Destroy(panel);
        }
        dynamicPanels.Clear();
    }

    // 辅助方法：获取武器类型名称
    private string GetWeaponTypeName(WeaponType type)
    {
        return type switch
        {
            WeaponType.DualPistols => "双枪",
            WeaponType.SingleHandedSword => "单手剑",
            WeaponType.Spear => "长枪",
            _ => ""
        };
    }

    // 辅助结构：用于属性显示
    private struct StatDisplayInfo
    {
        public string displayName;
        public string displayValue;

        public StatDisplayInfo(string name, string value)
        {
            displayName = name;
            displayValue = value;
        }
    }

    void OnDestroy()
    {
        ClearDynamicPanels();
    }
}