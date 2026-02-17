using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ShopView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    [Header("详情面板 - 用于显示")]
    public Image detailRarityImage;             // 详情-背景
    public Image detailIconImage;             // 详情-图标
    public Image detailStarImage;             // 详情-星级
    public TMP_Text detailNameText;             // 详情-名称
    public TMP_Text detailIntroductionText;      // 详情-介绍
    public TMP_Text detailDescriptionText;      // 详情-描述
    public TMP_Text detailCountText;            // 详情-数量

    private Coroutine scaleCoroutine;

    // ========================= 公共方法 =========================
    private void Start()
    {
    }

    public void UpdatePlayerResources(PlayerData playerData)
    {
        if (playerData == null) return;
        if (staminaText != null) staminaText.text = playerData.Stamina.ToString();
        if (coinsText != null) coinsText.text = playerData.Coins.ToString();
        if (crystalsText != null) crystalsText.text = playerData.Crystals.ToString();
    }

    /// <summary>
    /// 刷新单个商品预制体的 UI（显示名称、价格、图标等）
    /// </summary>
    public void RefreshItemUI(GameObject itemGO, ShopPoolItem data)
    {
        // 示例：假设预制体上有 ItemView 组件，通过它设置 UI
        CommodityItemView view = itemGO.GetComponent<CommodityItemView>();
        if (view != null)
        {
            view.SetData(data);
        }
        else
        {
            // 如果没有组件，也可以直接操作子 UI 元素（不推荐）
            Debug.LogError("预制体缺少 ItemView 组件！");
        }
    }
    public void ShowDetail(ShopPoolItem item)
    {
        // 尝试从角色字典查找
        if (GameDataManager.Instance.CharacterDict.TryGetValue(item.itemId, out var character))
            return;

        // 尝试从武器字典查找
        if (GameDataManager.Instance.WeaponDict.TryGetValue(item.itemId, out var weapon))
            return;

        // 尝试从圣痕字典查找
        if (GameDataManager.Instance.StigmataDict.TryGetValue(item.itemId, out var stigmata))
            return;

        // 尝试从材料字典查找
        if (GameDataManager.Instance.MaterialDict.TryGetValue(item.itemId, out var material))
        {
            Debug.Log("找到了");
            if (detailRarityImage != null)
                detailRarityImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Material/Frame_{material.baseStars}S");

            if (detailIconImage != null)
                detailIconImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Material/Icon_{material.id}");

            if (detailStarImage != null)
                detailStarImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{material.baseStars}S");

            if (detailNameText != null)
                detailNameText.text = material.materialName;

            if (detailIntroductionText != null)
                detailIntroductionText.text = $"{material.introduction}";

            if (detailDescriptionText != null)
                detailDescriptionText.text = $"{material.description}";
        }
    }
}