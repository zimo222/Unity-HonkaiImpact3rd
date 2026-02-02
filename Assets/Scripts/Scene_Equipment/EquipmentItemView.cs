using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class EquipmentItemView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    public Image iconImage;           // 图标
    public TMP_Text nameText;             // 名称
    public TMP_Text levelText;            // 等级
    public Image rarityFrame;         // 稀有度边框
    public TMP_Text starsText;            // 星级显示
    public GameObject equippedBadge;  // 已装备标志

    // 数据
    private EquipmentData equipmentData;
    private System.Action<EquipmentData> onClickCallback;

    // 初始化
    public void Initialize(EquipmentData data, System.Action<EquipmentData> onClick)
    {
        equipmentData = data;
        onClickCallback = onClick;

        UpdateUI();
    }

    // 更新UI显示
    void UpdateUI()
    {
        if (equipmentData == null) return;

        // 名称
        if (nameText != null)
            nameText.text = equipmentData.Name;

        // 等级
        if (levelText != null)
            levelText.text = $"Lv.{equipmentData.Stats.Level}";

        // 星级
        if (starsText != null)
            starsText.text = equipmentData.Stats.Stars;

        // 已装备标志
        if (equippedBadge != null)
            equippedBadge.SetActive(equipmentData.EquippedToCharacterIndex >= 0);

        // 稀有度边框颜色
        if (rarityFrame != null)
        {
            // 根据星级设置颜色
            string stars = equipmentData.Stats.Stars;
            rarityFrame.color = GetRarityColor(stars);
        }

        // 加载图标
        LoadIcon();
    }

    // 加载图标
    void LoadIcon()
    {
        if (iconImage == null) return;

        // 构建图标路径
        string iconPath = $"Equipment/Icons/{equipmentData.Id}";
        Sprite icon = Resources.Load<Sprite>(iconPath);

        if (icon != null)
        {
            iconImage.sprite = icon;
        }
        else
        {
            // 加载失败，使用默认图标
            string defaultIcon = equipmentData.Type == EquipmentType.Weapon ?
                "Equipment/Icons/weapon_default" : "Equipment/Icons/stigmata_default";
            iconImage.sprite = Resources.Load<Sprite>(defaultIcon);
        }
    }

    // 获取稀有度颜色
    Color GetRarityColor(string stars)
    {
        // 5星橙色，4星紫色，3星蓝色，其他灰色
        if (stars.Contains("5"))
            return new Color(1f, 0.5f, 0f); // 橙色
        else if (stars.Contains("4"))
            return new Color(0.5f, 0f, 1f); // 紫色
        else if (stars.Contains("3"))
            return new Color(0f, 0.5f, 1f); // 蓝色
        else
            return Color.gray; // 灰色
    }

    // 点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(equipmentData);
    }
}