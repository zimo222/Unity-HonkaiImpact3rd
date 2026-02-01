// Scripts/Equipment/EquipmentItemUI.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentItemUI : MonoBehaviour, IPointerClickHandler
{
    // UI组件
    [Header("UI组件")]
    public Image iconImage;           // 装备图标
    public Text nameText;             // 装备名称
    public Text levelText;            // 装备等级
    public Image rarityFrame;         // 稀有度边框
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

        // 已装备标志
        if (equippedBadge != null)
            equippedBadge.SetActive(equipmentData.EquippedToCharacterIndex >= 0);

        // 稀有度边框颜色
        if (rarityFrame != null)
        {
            // 根据星级设置颜色（这里简化处理）
            string stars = equipmentData.Stats.Stars;
            Color rarityColor = GetRarityColor(stars);
            rarityFrame.color = rarityColor;
        }

        // 加载图标（同步加载，简单实现）
        LoadIcon();
    }

    // 加载图标
    void LoadIcon()
    {
        if (iconImage == null) return;

        // 构建图标路径（根据你的项目结构）
        string iconPath = $"Equipment/Icons/{equipmentData.Id}";
        Sprite icon = Resources.Load<Sprite>(iconPath);

        if (icon != null)
        {
            iconImage.sprite = icon;
        }
        else
        {
            // 加载失败，使用默认图标
            Debug.LogWarning($"无法加载装备图标: {iconPath}");
            iconImage.sprite = Resources.Load<Sprite>("Equipment/Icons/default");
        }
    }

    // 获取稀有度颜色
    Color GetRarityColor(string stars)
    {
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