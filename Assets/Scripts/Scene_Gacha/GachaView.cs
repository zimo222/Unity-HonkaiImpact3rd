using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GachaView : MonoBehaviour
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

    public void UpdatePityDisplay(int fourStarPity, int fiveStarPity, bool fourStarGuaranteed, bool fiveStarGuaranteed)
    {
        Debug.Log($"四星保底：{fourStarPity}/10 {(fourStarGuaranteed ? "【保底中】" : "")}");
        if (fourStarPityText != null)
            fourStarPityText.text = $"四星保底：{fourStarPity}/10 {(fourStarGuaranteed ? "【保底中】" : "")}";
        Debug.Log($"五星保底：{fiveStarPity}/100 {(fiveStarGuaranteed ? "【保底中】" : "")}");
        if (fiveStarPityText != null)
            fiveStarPityText.text = $"五星保底：{fiveStarPity}/100 {(fiveStarGuaranteed ? "【保底中】" : "")}";
    }

    public void UpdateCurrentPoolName(string poolName)
    {
        if (currentPoolNameText != null)
            currentPoolNameText.text = poolName;
    }

    public void ShowAnimationPanel(bool show)
    {
        if (animationPanel != null) animationPanel.SetActive(show);
    }

    public void ShowSingleItemPanel(bool show)
    {
        if (singleItemPanel != null) singleItemPanel.SetActive(show);
    }

    public void UpdateSingleItemDisplay(Sprite icon, string name, int star)
    {
        if (itemIconImage != null) itemIconImage.sprite = icon;
        if (itemNameText != null) itemNameText.text = name;
        if (itemStarText != null) itemStarText.text = "★" + star;
    }

    public void PlayVideoFromResources(int largeIdx, int smallIdx)
    {
        // 加载Resources/Video下的VideoClip资源
        VideoClip clip = Resources.Load<VideoClip>("Video/" + largeIdx.ToString() + "_" + smallIdx.ToString());
        if (clip != null)
        {
            contentVideoPlayer.clip = clip;
            contentVideoPlayer.Play();
        }
        else
        {
            Debug.LogError("未找到视频资源: " + largeIdx + "_" + smallIdx);
        }
    }
}