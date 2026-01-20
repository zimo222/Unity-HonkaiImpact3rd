using UnityEngine;

public class OffsetRootMotion : MonoBehaviour
{
    private Vector3 initialLocalPosition; // 改为记录初始本地位置

    void Start()
    {
        // 记录游戏开始时，物体相对于其父对象的本地位置
        initialLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        // 核心：每一帧都将物体的本地位置重置为初始值
        // 这样无论动画或任何其他逻辑如何影响它，它都会“弹回”原处
        transform.localPosition = initialLocalPosition;
    }
}