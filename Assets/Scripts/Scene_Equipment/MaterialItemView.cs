using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaterialItemView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    public Image rarityImage;         // 稀有度背景
    public Image iconImage;           // 图标
    public TMP_Text countText;        // 数量
    public Image starsImage;          // 星级显示

    // 数据
    private MaterialData materialData;
    private System.Action<MaterialData> onClickCallback;

    // 初始化
    public void Initialize(MaterialData data, System.Action<MaterialData> onClick)
    {
        materialData = data;
        onClickCallback = onClick;

        UpdateUI();
    }

    // 更新UI显示
    void UpdateUI()
    {
        if (materialData == null) return;

        // 稀有度边框颜色
        if (rarityImage != null)
        {
            // 根据星级设置颜色
            string stars = materialData.Stars;
            rarityImage.color = GetRarityColor(stars);
        }

        // 星级
        if (starsImage != null)
            starsImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Material/Stars_{materialData.Stars}");

        // 数量
        if (countText != null)
            countText.text = $"x{materialData.Count}";

        // 加载图标
        LoadIcon();
    }

    // 加载图标
    void LoadIcon()
    {
        if (iconImage == null) return;

        // 构建图标路径
        string iconPath = $"Picture/Scene_Equipment/Material/Icon_{materialData.Id}";
        Sprite icon = Resources.Load<Sprite>(iconPath);

        if (icon != null)
        {
            iconImage.sprite = icon;
        }
        else
        {
            // 加载失败，使用默认图标
            iconImage.sprite = Resources.Load<Sprite>("Material/Icons/default");
        }
    }

    // 获取稀有度颜色
    Color GetRarityColor(string stars)
    {
        Debug.Log(stars);
        // 5星橙色，4星紫色，3星蓝色，其他灰色
        if (stars == "5S" || stars == "4S")
            return new Color(160 / 255.0f, 79 / 255.0f, 189 / 255.0f); // 紫色
        else if (stars == "3S" || stars == "2S")
            return new Color(40 / 255.0f, 165 / 255.0f, 225 / 255.0f); // 蓝色
        else if (stars == "1S")
            return new Color(78 / 255.0f, 179 / 255.0f, 131 / 255.0f); // 绿色
        else
            return Color.gray; // 灰色
    }

    // 点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(materialData);
    }
}