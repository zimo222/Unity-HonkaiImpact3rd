using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 为按钮列表提供可自定义的入场动画（缩放 + 淡入），支持相邻按钮依次延迟播放。
/// 透明度使用 CanvasGroup 控制，避免 Button 过渡干扰。
/// 播放模式：
/// - Always: 每次激活都播放
/// - Once: 每个实例生命周期内仅首次播放（实例变量）
/// - OnceGlobally: 全局仅首次播放（基于唯一ID的静态字典，跨实例持久）
/// </summary>
public class ButtonSequenceEffect : MonoBehaviour
{
    [Header("按钮列表（可手动拖拽，也可自动获取子物体）")]
    public Button[] buttons;

    [Header("动画时间控制")]
    public float duration = 0.5f;
    public float delayBetween = 0.1f;
    public float startDelay = 0f;

    [Header("缩放参数（常态大小为1）")]
    public float initialScale = 0f;
    public float maxScale = 1.2f;            // 两阶段缩放时的最大大小
    [Range(0.1f, 0.9f)]
    public float expandRatio = 0.6f;         // 放大阶段占比

    [Header("透明度参数")]
    [Range(0f, 1f)]
    public float initialAlpha = 0f;          // 初始透明度

    [Header("缩放模式")]
    public bool useElasticScale = true;      // true=弹性缩放，false=两阶段缩放

    [Header("视觉结构")]
    [Tooltip("如果按钮本体透明，由子物体 Image 显示内容，请勾选此项。脚本会自动为按钮或子物体添加 CanvasGroup 并控制透明度。")]
    public bool useChildImageAsVisual = false;

    [Header("播放模式")]
    public AnimationPlayMode playMode = AnimationPlayMode.Once; // 默认仅首次播放

    [Header("全局唯一标识（仅当 PlayMode = OnceGlobally 时需要）")]
    [Tooltip("用于全局标记的唯一字符串，相同 ID 的实例将共享播放状态。建议使用物体名称或其他不会冲突的标识。")]
    public string uniqueID = "";

    // 实例级别标记（用于 Once 模式）
    private bool _hasPlayedForThisInstance = false;

    // 静态字典：存储全局已播放状态（键为 uniqueID）
    private static Dictionary<string, bool> _globalPlayedStates = new Dictionary<string, bool>();

    void OnEnable()
    {
        bool shouldPlay = ShouldPlay();

        if (shouldPlay)
        {
            InitializeButtons();
            StartCoroutine(PlaySequenceCoroutine());
            MarkAsPlayed();
        }
        else
        {
            SetButtonsToFinalState();
        }
    }

    void OnDisable()
    {
        DOTween.Kill(this);
    }

    /// <summary>
    /// 根据播放模式判断是否应该播放动画
    /// </summary>
    bool ShouldPlay()
    {
        switch (playMode)
        {
            case AnimationPlayMode.Always:
                return true;

            case AnimationPlayMode.Once:
                return !_hasPlayedForThisInstance;

            case AnimationPlayMode.OnceGlobally:
                if (string.IsNullOrEmpty(uniqueID))
                {
                    Debug.LogWarning($"{name} 的 PlayMode 为 OnceGlobally 但未设置 uniqueID，将回退为 Once 模式。", this);
                    return !_hasPlayedForThisInstance;
                }
                // 如果字典中不存在该 ID，或者存在但为 false，则应该播放
                if (!_globalPlayedStates.ContainsKey(uniqueID))
                    return true;
                return !_globalPlayedStates[uniqueID];

            default:
                return true;
        }
    }

    /// <summary>
    /// 标记该实例/ID 已经播放过
    /// </summary>
    void MarkAsPlayed()
    {
        switch (playMode)
        {
            case AnimationPlayMode.Once:
                _hasPlayedForThisInstance = true;
                break;
            case AnimationPlayMode.OnceGlobally:
                if (!string.IsNullOrEmpty(uniqueID))
                {
                    _globalPlayedStates[uniqueID] = true;
                }
                else
                {
                    _hasPlayedForThisInstance = true; // 降级
                }
                break;
        }
    }

    void InitializeButtons()
    {
        if (buttons == null || buttons.Length == 0)
            buttons = GetComponentsInChildren<Button>();

        foreach (Button btn in buttons)
        {
            PrepareButton(btn);
        }
    }

    void SetButtonsToFinalState()
    {
        if (buttons == null || buttons.Length == 0)
            buttons = GetComponentsInChildren<Button>();

        foreach (Button btn in buttons)
        {
            if (btn == null) continue;

            btn.transform.localScale = Vector3.one;

            CanvasGroup cg = GetTargetCanvasGroup(btn);
            if (cg != null)
            {
                cg.alpha = 1f;
            }
            else
            {
                Graphic visual = GetVisualGraphic(btn);
                if (visual != null)
                {
                    Color c = visual.color;
                    c.a = 1f;
                    visual.color = c;
                }
            }
        }
    }

    void PrepareButton(Button btn)
    {
        GameObject targetForCanvasGroup;
        if (useChildImageAsVisual)
        {
            Image childImage = btn.GetComponentInChildren<Image>();
            if (childImage != null)
            {
                targetForCanvasGroup = childImage.gameObject;
                btn.targetGraphic = childImage;
            }
            else
            {
                Debug.LogWarning($"按钮 {btn.name} 没有子物体 Image，将使用按钮自身控制透明度。", btn.gameObject);
                targetForCanvasGroup = btn.gameObject;
            }
        }
        else
        {
            targetForCanvasGroup = btn.gameObject;
        }

        CanvasGroup cg = targetForCanvasGroup.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = targetForCanvasGroup.AddComponent<CanvasGroup>();

        cg.alpha = initialAlpha;

        ColorBlock colors = btn.colors;
        colors.colorMultiplier = 1f;
        btn.colors = colors;
    }

    IEnumerator PlaySequenceCoroutine()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
                PlayButtonAnimation(buttons[i]);

            if (i < buttons.Length - 1)
                yield return new WaitForSeconds(delayBetween);
        }
    }

    void PlayButtonAnimation(Button btn)
    {
        CanvasGroup cg = GetTargetCanvasGroup(btn);
        if (cg == null)
        {
            Debug.LogWarning($"按钮 {btn.name} 没有 CanvasGroup，无法播放透明度动画。", btn.gameObject);
            return;
        }

        btn.transform.localScale = Vector3.one * initialScale;

        Sequence seq = DOTween.Sequence();

        if (useElasticScale)
        {
            seq.Append(btn.transform.DOScale(1f, duration).SetEase(Ease.OutBack));
        }
        else
        {
            seq.Append(btn.transform.DOScale(maxScale, duration * expandRatio).SetEase(Ease.OutQuad));
            seq.Append(btn.transform.DOScale(1f, duration * (1f - expandRatio)).SetEase(Ease.InQuad));
        }

        seq.Join(cg.DOFade(1f, duration));
        seq.Play();
    }

    CanvasGroup GetTargetCanvasGroup(Button btn)
    {
        if (useChildImageAsVisual)
        {
            Image childImage = btn.GetComponentInChildren<Image>();
            if (childImage != null)
                return childImage.GetComponent<CanvasGroup>();
        }
        return btn.GetComponent<CanvasGroup>();
    }

    Graphic GetVisualGraphic(Button btn)
    {
        if (useChildImageAsVisual)
        {
            Image childImage = btn.GetComponentInChildren<Image>();
            if (childImage != null)
                return childImage;
        }
        return btn.targetGraphic;
    }
}

/// <summary>
/// 动画播放模式枚举
/// </summary>
public enum AnimationPlayMode
{
    Always,          // 每次激活都播放
    Once,            // 每个实例生命周期内仅首次播放（对象不销毁则持续）
    OnceGlobally      // 全局仅首次播放（基于唯一ID，跨实例持久，直到游戏关闭）
}