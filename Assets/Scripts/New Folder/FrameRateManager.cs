using UnityEngine;
using UnityEngine.UI;

public class FrameRateManager : MonoBehaviour
{
    [Header("帧率限制")]
    public int maxFrameRate = 120; // 限制最大帧率
    public bool useVSync = false; // 是否使用垂直同步

    [Header("物理设置")]
    public float fixedTimeStep = 0.00833333333333f; // 60Hz物理更新

    void Start()
    {
        // 1. 限制最大帧率
        Application.targetFrameRate = maxFrameRate;

        // 2. 垂直同步设置（如果不需要可以关闭）
        QualitySettings.vSyncCount = useVSync ? 1 : 0;

        // 3. 优化物理更新步长（非常重要！）
        Time.fixedDeltaTime = fixedTimeStep; // 0.0167 ≈ 60次/秒
        Time.maximumDeltaTime = 0.1f; // 防止卡顿时物理爆炸

        Debug.Log($"帧率设置: 最大{maxFrameRate}FPS, 物理更新{1 / fixedTimeStep}Hz");
    }

    void Update()
    {
        // 实时显示帧率（按F键显示）
        if (Input.GetKeyDown(KeyCode.F))
        {
            float fps = 1f / Time.unscaledDeltaTime;
            Debug.Log($"当前帧率: {fps:F1} FPS | 物理更新: {1 / Time.fixedDeltaTime:F0} Hz");
        }
    }
}