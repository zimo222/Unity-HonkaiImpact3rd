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

    [Header("保底信息")]
    public TMP_Text fourStarPityText;
    public TMP_Text fiveStarPityText;
    public TMP_Text currentPoolNameText;

    [Header("动态内容区域")]
    [SerializeField] private GameObject contentPanel;      // 唯一的 Panel 对象
    [SerializeField] private VideoPlayer contentVideoPlayer; // Panel 内的 VideoPlayer

    // ========================= 抽卡结果展示UI（单个物品展示） =========================
    [Header("抽卡展示流程")]
    public GameObject animationPanel;           // 播放抽卡动画的面板（包含VideoPlayer）
    public VideoPlayer gachaAnimationVideoPlayer; // 抽卡动画视频播放器
    public GameObject singleItemPanel;          // 单个道具展示面板
    public Image itemIconImage;                 // 道具图标
    public TMP_Text itemNameText;                // 道具名称
    public TMP_Text itemStarText;                // 道具星级（如 "★5"）
    public Button clickArea;                     // 用于点击切换的全屏透明按钮

    [Header("动画设置")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 缩放曲线
    public float scaleDuration = 0.2f; // 缩放持续时间

    private Coroutine scaleCoroutine;

    // ========================= 公共方法 =========================
    private void Start()
    {
        ShowAnimationPanel(false);
        ShowSingleItemPanel(false);
    }

    public void UpdatePlayerResources(PlayerData playerData)
    {
        if (playerData == null) return;
        if (staminaText != null) staminaText.text = playerData.Stamina.ToString();
        if (coinsText != null) coinsText.text = playerData.Coins.ToString();
        if (crystalsText != null) crystalsText.text = playerData.Crystals.ToString();
    }

    public void ShowAnimationPanel(bool show)
    {
        if (animationPanel != null) animationPanel.SetActive(show);
    }

    public void ShowSingleItemPanel(bool show)
    {
        if (singleItemPanel != null) singleItemPanel.SetActive(show);
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
}