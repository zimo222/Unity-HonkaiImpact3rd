using UnityEngine;

// 独立的旋转组件，可以附加到任何需要旋转的模型上
public class AutoRotateY : MonoBehaviour
{
    [Header("旋转设置")]
    [SerializeField] private float rotationSpeed = 30f; // 每秒旋转的角度
    [SerializeField] private bool rotateOnStart = true; // 是否在开始时自动旋转
    [SerializeField] private Space rotationSpace = Space.World; // 旋转空间：World或Self

    private bool isRotating = false;

    void Start()
    {
        if (rotateOnStart)
        {
            StartRotating();
        }
    }

    void Update()
    {
        if (isRotating)
        {
            // 绕Y轴匀速旋转
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, rotationSpace);
        }
    }

    // 公共方法：开始旋转
    public void StartRotating()
    {
        isRotating = true;
    }

    // 公共方法：停止旋转
    public void StopRotating()
    {
        isRotating = false;
    }

    // 公共方法：设置旋转速度
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    // 公共方法：切换旋转方向
    public void ToggleRotationDirection()
    {
        rotationSpeed = -rotationSpeed;
    }
}