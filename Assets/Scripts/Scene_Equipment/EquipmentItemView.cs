using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class EquipmentItemView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    public Image rarityImage;         // 稀有度背景
    public Image iconImage;           // 图标
    public TMP_Text levelText;        // 等级
    public Image starsImage;          // 星级显示
    public GameObject equippedBadge;  // 已装备标志

    // 数据
    private EquipmentData equipmentData;
    private System.Action<int> onClickCallback;
    private int index;

    // 初始化
    public void Initialize(EquipmentData data, System.Action<int> onClick, int Index)
    {
        equipmentData = data;
        onClickCallback = onClick;
        index = Index;

        UpdateUI();
    }

    // 更新UI显示
    void UpdateUI()
    {
        if (equipmentData == null) return;

        // 稀有度边框颜色
        if (rarityImage != null)
        {
            // 根据星级设置颜色
            string stars = equipmentData.Stats.Stars;
            rarityImage.color = GetRarityColor(stars);
        }

        // 已装备标志
        if (equippedBadge != null)
            equippedBadge.SetActive(equipmentData.EquippedToCharacterIndex >= 0);

        // 等级
        if (levelText != null)
            levelText.text = $"Lv.{equipmentData.Stats.Level}";

        // 星级
        if (starsImage != null)
            starsImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stars_{equipmentData.Stats.Stars}");

        // 加载图标
        LoadIcon();
    }

    // 加载图标
    void LoadIcon()
    {
        if (iconImage == null) return;

        // 构建图标路径
        string iconPath = equipmentData.Type == EquipmentType.Weapon ? $"Picture/Scene_Equipment/Weapon/Icon_{equipmentData.Id}" : $"Picture/Scene_Equipment/Stigmata/Icon_{equipmentData.Id}";
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
        Debug.Log(stars);
        // 5星橙色，4星紫色，3星蓝色，其他灰色
        if (stars == "5S" || stars == "4S" || stars == "4S1")
            return new Color(160 / 255.0f, 79 / 255.0f, 189 / 255.0f); // 紫色
        else if (stars == "3S" || stars == "2S" || stars == "2S1")
            return new Color(40 / 255.0f, 165 / 255.0f, 225 / 255.0f); // 蓝色
        else if (stars == "1S" || stars == "1S1")
            return new Color(78 / 255.0f, 179 / 255.0f, 131 / 255.0f); // 绿色
        else
            return Color.gray; // 灰色
    }

    // 点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(index);
    }
}