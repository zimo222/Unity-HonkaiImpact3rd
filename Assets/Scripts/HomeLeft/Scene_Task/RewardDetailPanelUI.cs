using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RewardDetailPanelUI : MonoBehaviour
{
    [Header("面板引用")]
    public GameObject detailPanel;
    public CanvasGroup canvasGroup;
    public Button confirmButton; // 确定/返回按钮

    [Header("主要奖励显示")]
    public Image mainReward1Icon;
    public TMP_Text mainReward1AmountText;
    public Image mainReward2Icon;
    public TMP_Text mainReward2AmountText;

    // 私有变量
    private TaskUIController uiController;
    private bool isAnimating = false;

    void Awake()
    {
        // 确保CanvasGroup存在
        if (canvasGroup == null && detailPanel != null)
        {
            canvasGroup = detailPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = detailPanel.AddComponent<CanvasGroup>();
            }
        }

        // 初始化面板状态
        if (detailPanel != null)
        {
            detailPanel.SetActive(true); // 保持激活状态
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 绑定按钮事件
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HidePanel);
        }
    }

    public void Initialize(TaskUIController controller)
    {
        uiController = controller;
    }

    public void ShowRewardDetail(TaskReward rewarda, TaskReward rewardb)
    {
        if (detailPanel == null) return;
        UpdateRewardDetailUI(rewarda, rewardb);

        // 显示面板并播放动画
        if (!detailPanel.activeSelf)
            detailPanel.SetActive(true);

        // 设置CanvasGroup属性
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 显示面板并播放动画
        if (!isAnimating)
        {
            StartCoroutine(ShowPanelAnimation());
        }
    }

    void UpdateRewardDetailUI(TaskReward rewarda, TaskReward rewardb)
    {
        // 详细奖励（图标+数量）
        UpdateDetailRewards(rewarda, rewardb);
    }

    void UpdateDetailRewards(TaskReward rewarda, TaskReward rewardb)
    {

        // 奖励1
        if (mainReward1Icon != null && mainReward1AmountText != null)
        {
            if (rewarda.Type != RewardType.None)
            {
                mainReward1Icon.gameObject.SetActive(true);
                mainReward1Icon.sprite = GetRewardSprite(rewarda.Type);
                mainReward1AmountText.text = $"{rewarda.Amount}";
            }
            else
            {
                mainReward1Icon.gameObject.SetActive(false);
            }
        }

        // 奖励2
        if (mainReward2Icon != null && mainReward2AmountText != null)
        {
            if (rewardb.Type != RewardType.None)
            {
                mainReward2Icon.gameObject.SetActive(true);
                mainReward2Icon.sprite = GetRewardSprite(rewardb.Type);
                mainReward2AmountText.text = $"{rewardb.Amount}";
            }
            else
            {
                mainReward2Icon.gameObject.SetActive(false);
            }
        }
    }

    Sprite GetRewardSprite(RewardType type)
    {
        string spriteName = "";
        switch (type)
        {
            case RewardType.Crystals: spriteName = "Icon_Crystal"; break;
            case RewardType.Coins: spriteName = "Icon_Coin"; break;
            case RewardType.Stamina: spriteName = "Icon_Stamina"; break;
            case RewardType.DailyEXP: spriteName = "Icon_DailyEXP"; break;
            case RewardType.EXP: spriteName = "Icon_EXP"; break;
            case RewardType.Equipment: spriteName = "Icon_Equipment"; break;
            case RewardType.CharacterFragment: spriteName = "Icon_Fragment"; break;
            case RewardType.BattlePassEXP: spriteName = "Icon_BattlePass"; break;
            case RewardType.Materials: spriteName = "Icon_Material"; break;
            default: spriteName = "Icon_Default"; break;
        }

        Debug.Log($"UI/Icons/Large/{spriteName}");
        Sprite sprite = Resources.Load<Sprite>($"UI/Icons/{spriteName}");
        return sprite != null ? sprite : Resources.Load<Sprite>("UI/Icons/Icon_Default");
    }

    public void HidePanel()
    {
        if (detailPanel == null || !detailPanel.activeSelf) return;

        if (!isAnimating)
        {
            StartCoroutine(HidePanelAnimation());
        }
    }

    IEnumerator ShowPanelAnimation()
    {
        isAnimating = true;
        detailPanel.SetActive(true);

        float duration = 0.25f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        isAnimating = false;
    }

    IEnumerator HidePanelAnimation()
    {
        isAnimating = true;

        float duration = 0.15f;
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // 禁用交互，但不禁用GameObject
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        isAnimating = false;

        // 通知UI控制器面板已关闭
        if (uiController != null)
        {
            uiController.OnRewardDetailClosed();
        }
    }

    // 检查点击是否在面板上
    public bool IsPointerOverPanel()
    {
        return detailPanel.activeSelf;
    }
}