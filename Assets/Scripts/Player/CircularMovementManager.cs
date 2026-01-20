using UnityEngine;

[System.Serializable]
public class CircularMovementData
{
    public Vector3 centerPosition;     // 圆心世界坐标
    public float radius;               // 圆半径
    public float currentAngle;         // 当前角度（弧度）
    public float targetAngle;          // 目标角度（弧度）
    public float angularSpeed;         // 角速度（弧度/秒）
    public bool isActive;              // 是否激活同心圆运动
    public Vector3 forwardDirection;   // 前进方向
}

public class CircularMovementManager : MonoBehaviour
{
    [Header("同心圆参数")]
    public float defaultRadius = 5f;   // 默认圆半径
    public float maxRadius = 10f;      // 最大半径
    public float minRadius = 2f;       // 最小半径

    [Header("运动参数")]
    public float moveSpeed = 5f;       // 角色移动速度
    public float angularSpeedFactor = 0.5f; // 角速度系数

    private CircularMovementData characterData = new CircularMovementData();
    private CircularMovementData cameraData = new CircularMovementData();

    private Transform characterTransform;
    private Transform cameraTransform;

    private Vector3 lastCharacterPosition;
    private bool isInitialized = false;

    // 初始化
    public void Initialize(Transform character, Transform camera)
    {
        if (character == null)
        {
            Debug.LogError("Character transform is null in CircularMovementManager.Initialize");
            return;
        }

        if (camera == null)
        {
            Debug.LogWarning("Camera transform is null in CircularMovementManager.Initialize");
        }

        characterTransform = character;
        cameraTransform = camera;
        lastCharacterPosition = character.position;
        isInitialized = true;

        Debug.Log($"CircularMovementManager initialized with character: {character.name}");
    }

    // 开始同心圆运动
    public void StartCircularMovement(Vector3 moveDirection, bool moveRight)
    {
        if (!isInitialized)
        {
            Debug.LogError("CircularMovementManager not initialized. Call Initialize first.");
            return;
        }

        if (characterTransform == null)
        {
            Debug.LogError("Character transform is null in StartCircularMovement");
            return;
        }

        // 计算圆心位置
        // 圆心在角色移动方向的垂直方向上，距离为半径
        Vector3 perpendicular = Vector3.Cross(Vector3.up, moveDirection).normalized;

        // 如果是向右移动，圆心在左侧；向左移动，圆心在右侧
        if (moveRight)
        {
            characterData.centerPosition = characterTransform.position - perpendicular * defaultRadius;
        }
        else
        {
            characterData.centerPosition = characterTransform.position + perpendicular * defaultRadius;
        }

        // 设置角色数据
        characterData.radius = defaultRadius;
        characterData.isActive = true;
        characterData.forwardDirection = moveDirection;

        // 计算角色当前相对于圆心的角度
        Vector3 toCharacter = characterTransform.position - characterData.centerPosition;
        characterData.currentAngle = Mathf.Atan2(toCharacter.x, toCharacter.z);

        // 设置摄像机数据（与角色相对180度）
        cameraData.centerPosition = characterData.centerPosition; // 相同圆心
        cameraData.radius = defaultRadius;
        cameraData.isActive = true;

        // 摄像机角度 = 角色角度 + π（180度）
        cameraData.currentAngle = characterData.currentAngle + Mathf.PI;

        // 设置目标角度（旋转方向）
        if (moveRight)
        {
            characterData.targetAngle = characterData.currentAngle + Mathf.PI * 2; // 顺时针一圈
            cameraData.targetAngle = cameraData.currentAngle + Mathf.PI * 2;
        }
        else
        {
            characterData.targetAngle = characterData.currentAngle - Mathf.PI * 2; // 逆时针一圈
            cameraData.targetAngle = cameraData.currentAngle - Mathf.PI * 2;
        }

        // 计算角速度（基于移动速度）
        characterData.angularSpeed = (moveSpeed / defaultRadius) * angularSpeedFactor;
        cameraData.angularSpeed = characterData.angularSpeed; // 相同角速度

        Debug.Log($"Started circular movement. Center: {characterData.centerPosition}, Radius: {characterData.radius}, MoveRight: {moveRight}");
    }

    // 停止同心圆运动
    public void StopCircularMovement()
    {
        characterData.isActive = false;
        cameraData.isActive = false;
    }

    // 更新角色位置（基于同心圆运动）
    public Vector3 UpdateCharacterPosition(float deltaTime)
    {
        if (!characterData.isActive || !isInitialized || characterTransform == null)
        {
            return characterTransform != null ? characterTransform.position : Vector3.zero;
        }

        // 更新角度
        if (characterData.currentAngle < characterData.targetAngle)
        {
            characterData.currentAngle += characterData.angularSpeed * deltaTime;
            characterData.currentAngle = Mathf.Min(characterData.currentAngle, characterData.targetAngle);
        }
        else if (characterData.currentAngle > characterData.targetAngle)
        {
            characterData.currentAngle -= characterData.angularSpeed * deltaTime;
            characterData.currentAngle = Mathf.Max(characterData.currentAngle, characterData.targetAngle);
        }

        // 计算新位置
        Vector3 newPosition = CalculateCirclePosition(
            characterData.centerPosition,
            characterData.radius,
            characterData.currentAngle
        );

        // 更新摄像机角度以保持相对
        cameraData.currentAngle = characterData.currentAngle + Mathf.PI;

        return newPosition;
    }

    // 更新摄像机位置（基于同心圆运动）
    public Vector3 UpdateCameraPosition(float deltaTime)
    {
        if (!cameraData.isActive || !isInitialized || cameraTransform == null)
        {
            return cameraTransform != null ? cameraTransform.position : Vector3.zero;
        }

        // 更新摄像机目标角度
        cameraData.targetAngle = characterData.targetAngle + Mathf.PI;

        // 更新角度
        if (cameraData.currentAngle < cameraData.targetAngle)
        {
            cameraData.currentAngle += cameraData.angularSpeed * deltaTime;
            cameraData.currentAngle = Mathf.Min(cameraData.currentAngle, cameraData.targetAngle);
        }
        else if (cameraData.currentAngle > cameraData.targetAngle)
        {
            cameraData.currentAngle -= cameraData.angularSpeed * deltaTime;
            cameraData.currentAngle = Mathf.Max(cameraData.currentAngle, cameraData.targetAngle);
        }

        // 计算新位置
        return CalculateCirclePosition(
            cameraData.centerPosition,
            cameraData.radius,
            cameraData.currentAngle
        );
    }

    // 计算圆上的位置
    private Vector3 CalculateCirclePosition(Vector3 center, float radius, float angle)
    {
        float x = Mathf.Sin(angle) * radius;
        float z = Mathf.Cos(angle) * radius;
        return center + new Vector3(x, 0, z);
    }

    // 计算摄像机看向角色的旋转
    public Quaternion CalculateCameraRotation(Vector3 cameraPosition, Vector3 characterPosition)
    {
        Vector3 lookDirection = (characterPosition - cameraPosition).normalized;
        return Quaternion.LookRotation(lookDirection, Vector3.up);
    }

    // 获取当前运动状态
    public bool IsCircularMovementActive()
    {
        return characterData.isActive;
    }

    // 获取角色前进方向（切线方向）
    public Vector3 GetCharacterForwardDirection()
    {
        if (!characterData.isActive || characterTransform == null)
            return characterTransform != null ? characterTransform.forward : Vector3.forward;

        // 在圆上，前进方向是切线方向
        Vector3 toCenter = characterData.centerPosition - characterTransform.position;
        Vector3 tangent = Vector3.Cross(Vector3.up, toCenter).normalized;

        // 根据旋转方向调整
        if (characterData.targetAngle > characterData.currentAngle)
        {
            return tangent; // 顺时针
        }
        else
        {
            return -tangent; // 逆时针
        }
    }

    // 调试绘制
    public void DrawDebugGizmos()
    {
        if (!isInitialized || !characterData.isActive) return;

        // 绘制圆心
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(characterData.centerPosition, 0.5f);

        // 绘制圆形轨迹
        Gizmos.color = Color.yellow;
        for (int i = 0; i <= 360; i += 10)
        {
            float angle = i * Mathf.Deg2Rad;
            Vector3 point = CalculateCirclePosition(characterData.centerPosition, characterData.radius, angle);
            Gizmos.DrawWireSphere(point, 0.1f);
        }

        // 绘制角色到圆心的连线
        if (characterTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(characterTransform.position, characterData.centerPosition);
        }

        // 绘制摄像机到圆心的连线
        if (cameraTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(cameraTransform.position, characterData.centerPosition);
        }
    }
}