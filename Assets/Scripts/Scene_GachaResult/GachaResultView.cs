using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GachaResultView : MonoBehaviour
{
    [Header("UI引用")]
    public Transform gridContainer;           // 放置图标的父对象（如 GridLayoutGroup）
    public GameObject itemIconPrefab;          // 每个道具图标的预制体（需包含 Image 组件）
    public Button confirmButton;               // 确定按钮，返回抽卡场景

    private List<GachaResultItem> results;

    void Start()
    {
        results = GachaResultData.CurrentResults;
        if (results == null || results.Count == 0)
        {
            Debug.LogWarning("没有抽卡结果数据，直接返回抽卡场景");
            ReturnToGachaScene();
            return;
        }

        // 生成所有道具图标
        foreach (var item in results)
        {
            GameObject iconObj = Instantiate(itemIconPrefab, gridContainer);
            // 查找名为 "Icon" 的子对象（根据你的实际命名调整）
            Transform iconTransform = iconObj.transform.Find("IconImage");
            if (iconTransform != null)
            {
                Image img = iconTransform.GetComponent<Image>();
                if (img != null && item.icon != null)
                {
                    img.sprite = item.icon;
                    img.SetNativeSize(); // 自动将Image尺寸设置为sprite的原生大小
                    img.rectTransform.sizeDelta *= 1.4f; // 再缩放 1.4 倍
                }
            }
                // 可选：显示星级文本，可在预制体中包含 TMP_Text 并赋值
                TMP_Text starText = iconObj.GetComponentInChildren<TMP_Text>();
            if (starText != null)
                starText.text = item.star + "星";
        }

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ReturnToGachaScene);
    }

    private void ReturnToGachaScene()
    {
        // 清空静态数据（可选）
        GachaResultData.Clear();
        // 返回抽卡主场景（请根据实际场景名称修改）
        SceneManager.LoadScene("GachaScene");
    }

    void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(ReturnToGachaScene);
    }
}