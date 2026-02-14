using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class GachaView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // 注意：需要添加保底文本和卡池名称文本的字段，如果尚未添加请自行补充
    [Header("保底信息")]
    public TMP_Text fourStarPityText;
    public TMP_Text fiveStarPityText;
    public TMP_Text currentPoolNameText;

    [Header("动态内容区域")]
    [SerializeField] private GameObject contentPanel;      // 唯一的 Panel 对象
    [SerializeField] private VideoPlayer contentVideoPlayer; // Panel 内的 VideoPlayer

    // ========================= 抽卡结果展示UI =========================
    [Header("抽卡结果")]
    public TMP_Text gachaResultText;          // 单抽结果
    public TMP_Text gachaTenResultText;       // 十连结果（显示多行文本）

    // 更新玩家资源
    public void UpdatePlayerResources(PlayerData playerData)
    {
        if (playerData == null) return;
        if (staminaText != null) staminaText.text = playerData.Stamina.ToString();
        if (coinsText != null) coinsText.text = playerData.Coins.ToString();
        if (crystalsText != null) crystalsText.text = playerData.Crystals.ToString();
    }

    // 显示单抽结果
    public void ShowGachaResult(string itemName, int star)
    {
        Debug.Log($"获得：{itemName} (★{star + 4})");
        if (gachaResultText != null)
            gachaResultText.text = $"获得：{itemName} (★{star})";
    }

    // 显示十连结果
    public void ShowGachaTenResult(string[] itemNames, int[] stars)
    {
        //if (gachaTenResultText == null) return;
        string result = "";
        for (int i = 0; i < itemNames.Length; i++)
        {
            result += $"{itemNames[i]} (★{stars[i] + 4})\n";
        }
        Debug.Log(result);
        if(gachaTenResultText != null)
            gachaTenResultText.text = result;
    }

    // 更新保底显示
    public void UpdatePityDisplay(int fourStarPity, int fiveStarPity, bool fourStarGuaranteed, bool fiveStarGuaranteed)
    {
        Debug.Log($"四星保底：{fourStarPity}/10 {(fourStarGuaranteed ? "【保底中】" : "")}");
        if (fourStarPityText != null)
            fourStarPityText.text = $"四星保底：{fourStarPity}/10 {(fourStarGuaranteed ? "【保底中】" : "")}";
        Debug.Log($"五星保底：{fiveStarPity}/100 {(fiveStarGuaranteed ? "【保底中】" : "")}");
        if (fiveStarPityText != null)
            fiveStarPityText.text = $"五星保底：{fiveStarPity}/100 {(fiveStarGuaranteed ? "【保底中】" : "")}";
    }

    // 更新当前卡池名称
    public void UpdateCurrentPoolName(string poolName)
    {
        if (currentPoolNameText != null)
            currentPoolNameText.text = poolName;
    }
}