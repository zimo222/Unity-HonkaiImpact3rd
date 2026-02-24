using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInitialEffect : MonoBehaviour
{
    public Button button;
    public float duration = 0.5f;

    void Start()
    {
        // 初始状态：透明、缩放为0
        button.transform.localScale = Vector3.zero;
        button.targetGraphic.canvasRenderer.SetAlpha(0f);

        // 播放动画：缩放到正常大小，并淡入
        Sequence seq = DOTween.Sequence();
        seq.Join(button.transform.DOScale(1f, duration).SetEase(Ease.OutBack));
        seq.Join(button.targetGraphic.DOFade(1f, duration));
        seq.Play();
    }
}