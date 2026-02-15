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
            Transform backTransform = iconObj.transform.Find("RarityImage");
            if (backTransform != null)
            {
                Image img = backTransform.GetComponent<Image>();
                if (img != null && item.icon != null)
                {
                    img.color = GetColor(item.star);
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

    Color GetColor(int stars)
    {
        // 5星橙色，4星紫色，3星蓝色，其他灰色
        if (stars == 5)
            return new Color(255 / 255.0f, 163 / 255.0f, 32 / 255.0f);
        else if (stars == 4)
            return new Color(160 / 255.0f, 79 / 255.0f, 189 / 255.0f); // 紫色
        else if (stars == 3 || stars == 2)
            return new Color(40 / 255.0f, 165 / 255.0f, 225 / 255.0f); // 蓝色
        else if (stars == 1)
            return new Color(78 / 255.0f, 179 / 255.0f, 131 / 255.0f); // 绿色
        else
            return Color.gray; // 灰色
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