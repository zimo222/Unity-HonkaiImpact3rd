using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSequenceEffect : MonoBehaviour
{
    [Header("按钮列表（可手动拖拽，也可自动获取子物体）")]
    public Button[] buttons;               // 如果不填，则自动获取所有子物体中的 Button

    [Header("动画参数")]
    public float duration = 0.5f;          // 单个按钮动画时长
    public float delayBetween = 0.1f;       // 相邻按钮启动延迟
    public float startDelay = 0f;           // 整体启动延迟

    [Header("弹性缩放效果")]
    public bool useElasticScale = true;     // 是否使用弹性缩放（Ease.OutBack）

    void Start()
    {
        // 如果没有手动赋值，自动获取所有子物体中的 Button
        if (buttons == null || buttons.Length == 0)
        {
            buttons = GetComponentsInChildren<Button>();
        }

        // 开始逐个播放动画
        PlaySequence();
    }

    void PlaySequence()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Button btn = buttons[i];
            int index = i; // 防止闭包问题

            // 初始状态：透明、缩放到0
            btn.transform.localScale = Vector3.zero;
            btn.targetGraphic = btn.GetComponentInChildren<Image>();
            btn.targetGraphic.canvasRenderer.SetAlpha(0f);

            // 计算当前按钮的启动延迟
            float currentDelay = startDelay + index * delayBetween;

            // 创建序列动画
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(currentDelay);                     // 先等待
            seq.Append(btn.transform.DOScale(1.2f, duration * 0.6f).SetEase(Ease.OutQuad)); // 稍微放大一点
            seq.Append(btn.transform.DOScale(1f, duration * 0.4f).SetEase(Ease.InQuad));     // 回弹到正常
            if (useElasticScale)
            {
                // 如果使用弹性缩放，可以用 Ease.OutBack 代替上面的两段，更简单
                // 但为了演示组合效果，这里保留了两段；你也可以直接使用一行：
                // seq.Join(btn.transform.DOScale(1f, duration).SetEase(Ease.OutBack));
            }
            seq.Join(btn.targetGraphic.DOFade(1f, duration));    // 同时淡入
            seq.Play();
            /*
            // 在动画末尾加一个循环呼吸效果
            seq.Append(btn.transform.DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo));
            */
        }
    }
}