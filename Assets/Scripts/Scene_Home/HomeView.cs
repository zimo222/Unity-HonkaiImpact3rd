using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class HomeView : MonoBehaviour
{
    // ================== 基础玩家信息UI引用 ==================
    [Header("玩家信息")]
    public TMP_Text playerNameText;
    public TMP_Text playerLevelText;

    [Header("资源信息")]
    public TMP_Text tiliText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ================== Panel填充控制 ==================
    [Header("Panel填充控制")]
    public PanelFillData[] panelFillControllers;    // Panel填充控制器数组

    [System.Serializable]
    public class PanelFillData
    {
        public string fillName;                     // 填充名称（用于标识）
        public GameObject targetPanel;              // 目标Panel对象
        public float fillAmount;                    // 当前填充值
        [Range(0f, 1f)] public float minFill = 0f;  // 最小填充值
        [Range(0f, 1f)] public float maxFill = 1f;  // 最大填充值
        public bool useCustomFill = false;          // 是否使用自定义填充值
        public float customFillValue = 0.5f;        // 自定义填充值

        [HideInInspector] public Image panelImage;  // Panel的Image组件
        [HideInInspector] public bool isInitialized = false; // 是否已初始化
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateAllUI(PlayerData playerData)
    {
        if (playerData == null) return;
        UpdatePlayerInfo(playerData);
        UpdateResources(playerData);
        UpdatePanelFills(playerData);
    }

    void UpdatePlayerInfo(PlayerData playerData)
    {
        if (playerData == null) return;
        if (playerNameText != null)
            playerNameText.text = playerData.PlayerName;
        if (playerLevelText != null)
            playerLevelText.text = $"Lv.{playerData.Level}";
    }

    void UpdateResources(PlayerData playerData)
    {
        if (playerData == null) return;
        if (tiliText != null)
            tiliText.text = playerData.Stamina.ToString() + "/" + (playerData.Level + 80).ToString();
        if (coinsText != null)
            coinsText.text = playerData.Coins.ToString();
        if (crystalsText != null)
            crystalsText.text = playerData.Crystals.ToString();
    }

    void UpdatePanelFills(PlayerData playerData)
    {
        if (panelFillControllers != null && playerData != null)
        {
            int expNeededForNextLevel = playerData.Level * 100;
            float expPercent = (float)playerData.Experience / expNeededForNextLevel;
            float crystalPercent = Mathf.Clamp01(playerData.Crystals / 10000f);
            float coinPercent = Mathf.Clamp01(playerData.Coins / 50000f);

            foreach (PanelFillData fillData in panelFillControllers)
            {
                if (fillData.panelImage != null)
                {
                    float fillValue = fillData.fillAmount;

                    switch (fillData.fillName)
                    {
                        case "Exp":
                            fillValue = expPercent;
                            break;
                        case "水晶进度":
                            fillValue = crystalPercent;
                            break;
                        case "金币进度":
                            fillValue = coinPercent;
                            break;
                        default:
                            if (fillData.useCustomFill)
                            {
                                fillValue = fillData.customFillValue;
                            }
                            break;
                    }
                    fillData.panelImage.fillAmount = Mathf.Clamp(fillValue, fillData.minFill, fillData.maxFill);
                    fillData.fillAmount = fillData.panelImage.fillAmount;
                }
            }
        }
    }
}
