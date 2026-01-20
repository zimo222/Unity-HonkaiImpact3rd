using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TrueOrbitalCamera : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target; //摄像机围绕的目标
    public Vector3 targetOffset = Vector3.up * 1.5f; //瞄准点偏移

    [Header("轨道参数")]
    [Range(1f, 20f)] public float distance = 5f; //轨道半径
    [Range(1f, 10f)] public float minDistance = 2f;
    [Range(5f, 30f)] public float maxDistance = 10f;

    [Header("角度参数")]
    [Range(0f, 360f)] public float horizontalAngle = 0f; //水平角度（0-360度）
    [Range(-80f, 80f)] public float verticalAngle = 30f; //垂直角度（-80到80度）

    [Header("控制灵敏度")]
    [Range(0.1f, 10f)] public float mouseSensitivity = 2f;
    [Range(10f, 180f)] public float keyboardRotateSpeed = 90f;

    [Header("平滑设置")]
    [Range(0f, 1f)] public float smoothFactor = 0.2f; //位置平滑度

    [Header("碰撞检测")]
    public LayerMask collisionMask = -1;
    public float collisionRadius = 0.5f;

    //内部变量
    private Vector3 currentPosition;
    private Quaternion currentRotation;
    private Camera cam;
    private bool isInitialized = false;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (target == null)
        {
            Debug.LogError("未设置摄像机目标！请将角色拖拽到Target字段。");
            enabled = false;
            return;
        }

        //初始化：强制计算初始位置和旋转
        Vector3 toCamera = transform.position - (target.position + targetOffset);
        distance = Mathf.Clamp(toCamera.magnitude, minDistance, maxDistance);

        //计算初始角度
        horizontalAngle = Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg;
        float horizontalDistance = new Vector3(toCamera.x, 0, toCamera.z).magnitude;
        verticalAngle = Mathf.Atan2(toCamera.y, horizontalDistance) * Mathf.Rad2Deg;

        currentPosition = CalculateOrbitPosition();
        currentRotation = CalculateLookRotation();

        //强制应用初始位置和旋转
        transform.position = currentPosition;
        transform.rotation = currentRotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || target == null) return;

        //处理所有输入
        ProcessInput();
    }

    void LateUpdate()
    {
        if (!isInitialized || target == null) return;

        //强制更新摄像机位置和旋转
        UpdateCameraTransform();
    }

    void ProcessInput()
    {
        //鼠标输入
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (mouseX != 0)
        {
            horizontalAngle += mouseX * mouseSensitivity;
            horizontalAngle = Mathf.Repeat(horizontalAngle, 360f);
        }

        if (mouseY != 0)
        {
            verticalAngle -= mouseY * mouseSensitivity; //减号：向上移动鼠标时视角上升
            verticalAngle = Mathf.Clamp(verticalAngle, -80f, 80f);
        }

        //键盘Q/E输入
        float keyboardInput = 0f;
        if (Input.GetKey(KeyCode.Q)) keyboardInput = -1f;
        if (Input.GetKey(KeyCode.E)) keyboardInput = 1f;

        if (keyboardInput != 0)
        {
            horizontalAngle += keyboardInput * keyboardRotateSpeed * Time.deltaTime;
            horizontalAngle = Mathf.Repeat(horizontalAngle, 360f);
        }

        //鼠标滚轮调整距离
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            distance -= scroll * 2f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void UpdateCameraTransform()
    {
        //1. 计算理想轨道位置
        Vector3 idealPosition = CalculateOrbitPosition();

        //2. 碰撞检测：避免穿墙
        Vector3 adjustedPosition = HandleCollision(idealPosition);

        //3. 平滑移动到目标位置
        currentPosition = Vector3.Lerp(currentPosition, adjustedPosition, smoothFactor * 10f * Time.deltaTime);

        //4. 强制计算看向目标的旋转
        currentRotation = CalculateLookRotation();

        //5. 直接设置Transform，不依赖任何其他系统
        transform.position = currentPosition;
        transform.rotation = currentRotation;

        //6. 额外安全措施：再次强制看向目标（确保万无一失）
        Vector3 lookTarget = target.position + targetOffset;
        transform.LookAt(lookTarget);
    }

    Vector3 CalculateOrbitPosition()
    {
        //将角度转换为弧度
        float horizontalRad = horizontalAngle * Mathf.Deg2Rad;
        float verticalRad = verticalAngle * Mathf.Deg2Rad;

        //计算球形坐标
        float x = Mathf.Sin(horizontalRad) * Mathf.Cos(verticalRad) * distance;
        float y = Mathf.Sin(verticalRad) * distance;
        float z = Mathf.Cos(horizontalRad) * Mathf.Cos(verticalRad) * distance;

        //返回相对于目标的位置
        return (target.position + targetOffset) + new Vector3(x, y, z);
    }

    Quaternion CalculateLookRotation()
    {
        Vector3 lookTarget = target.position + targetOffset;
        Vector3 lookDirection = (lookTarget - transform.position).normalized;

        //直接创建看向目标的旋转
        return Quaternion.LookRotation(lookDirection, Vector3.up);
    }

    Vector3 HandleCollision(Vector3 desiredPosition)
    {
        Vector3 targetPos = target.position + targetOffset;
        Vector3 direction = desiredPosition - targetPos;
        float maxDistance = direction.magnitude;

        //球形射线检测
        if (Physics.SphereCast(targetPos, collisionRadius, direction.normalized,
                              out RaycastHit hit, maxDistance, collisionMask))
        {
            //遇到障碍物，拉近摄像机
            return targetPos + direction.normalized * (hit.distance - collisionRadius * 0.5f);
        }

        return desiredPosition;
    }

    //调试绘制
    void OnDrawGizmos()
    {
        if (target != null)
        {
            Vector3 targetPos = target.position + targetOffset;

            //绘制目标点
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.2f);

            if (Application.isPlaying)
            {
                //绘制当前摄像机位置
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentPosition, 0.3f);
                Gizmos.DrawLine(targetPos, currentPosition);

                //绘制视线方向
                Gizmos.color = Color.red;
                Gizmos.DrawRay(currentPosition, transform.forward * 2f);
            }
        }
    }

    //公共方法
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null) isInitialized = true;
    }

    public void SetOrbitAngles(float horizontal, float vertical)
    {
        horizontalAngle = Mathf.Repeat(horizontal, 360f);
        verticalAngle = Mathf.Clamp(vertical, -80f, 80f);
    }

    public void SetDistance(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    //获取摄像机方向（用于角色移动）
    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right.normalized;
    }

    //快速测试方法
    [ContextMenu("重置摄像机位置")]
    public void ResetCameraPosition()
    {
        if (target != null)
        {
            horizontalAngle = 0f;
            verticalAngle = 30f;
            distance = 5f;
            UpdateCameraTransform();
        }
    }
}