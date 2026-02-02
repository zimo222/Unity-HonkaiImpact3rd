using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaterialItemView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    public Image iconImage;           // 图标
    public Text nameText;             // 名称
    public Text countText;            // 数量

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

        // 名称
        if (nameText != null)
            nameText.text = materialData.Name;

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
        string iconPath = $"Material/Icons/{materialData.Id}";
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

    // 点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(materialData);
    }
}