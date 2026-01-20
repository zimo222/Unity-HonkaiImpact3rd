using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeTransitionManager : MonoBehaviour
{
    // ================== 场景元素引用 ==================
    [Header("场景元素")]
    public Transform[] BackQuads = new Transform[5];      // 5个Back对象
    public Transform[] ToHomeQuads = new Transform[3];    // 3个ToHome对象
    public Transform TransitionQuad;                       // 新增的Transition对象
    public Transform Character;                       // 新增的Transition对象
    public Transform BackGround;                       // 新增的Transition对象
    public Camera MainCamera;
    public Camera MainCamera1;
    public GameObject HomeUI;

    // ================== Back参数 ==================
    [Header("Back参数")]
    public float BackMoveSpeed = 100f;    // Back下移速度
    public float BackDisableY = -1400f;   // Back禁用边界

    // ================== ToHome参数 ==================
    [Header("ToHome参数")]
    public float ToHomeMoveSpeed = 350f;   // ToHome下移速度
    public float ToHomeStopY = -1000f;      // ToHome停止下移的Y坐标
    public float PauseDuration = 1f;      // 停顿时间

    // ================== ToHome起始Y偏移 ==================
    [Header("ToHome起始Y偏移")]
    public float ToHome1YOffset = 100f;   // 第一个ToHome的Y偏移
    public float ToHome2YOffset = 1850f;   // 第二个ToHome的Y偏移
    public float ToHome3YOffset = 2215f;   // 第三个ToHome的Y偏移

    // ================== ToHome移出参数 ==================
    [Header("ToHome移出参数")]
    public Vector3 ToHome1Direction = new Vector3(-1, 0, 0);  // 第一个移动方向
    public float ToHome1Speed = 150f;                          // 第一个移出速度
    public float ToHome1ExitTime = 2f;                         // 第一个移出时间

    public Vector3 ToHome2Direction = new Vector3(0, -1, 0);  // 第二个移动方向
    public float ToHome2Speed = 150f;                          // 第二个移出速度
    public float ToHome2ExitTime = 2f;                         // 第二个移出时间

    public Vector3 ToHome3Direction = new Vector3(0, 1, 0);   // 第三个移动方向
    public float ToHome3Speed = 150f;                          // 第三个移出速度
    public float ToHome3ExitTime = 2f;                         // 第三个移出时间

    // ================== TransitionQuad参数 ==================
    [Header("TransitionQuad参数")]
    public Vector3 TransitionQuadDirection = new Vector3(1, 0, 0);  // 移动方向
    public float TransitionQuadSpeed = 120f;                        // 移动速度
    public float TransitionQuadExitTime = 2.5f;                     // 移出时间
    public bool EnableTransitionQuad = true;                       // 是否启用

    // ================== 相机参数 ==================
    [Header("相机参数")]
    public float CameraMoveDistance = 3f;    // 相机前移距离
    public float CameraMoveDuration = 2f;    // 相机移动时间

    // ================== 私有变量 ==================
    private bool isTransitionActive = true;
    private Vector3[] originalBackPositions;     // 保存Back的原始位置
    private Vector3[] originalToHomePositions;   // 保存ToHome的原始位置
    private Vector3 originalTransitionQuadPosition; // 保存TransitionQuad的原始位置
    private float baseY;                         // 从数据库获取的基础Y坐标

    void Start()
    {
        // 初始化引用
        if (MainCamera == null) MainCamera = Camera.main;
        // 初始化引用
        if (MainCamera1 == null) MainCamera1 = Camera.main;

        // 从数据管理器获取基础Y坐标
        baseY = PlayerDataManager.LastTopQuadYPosition;
        Debug.Log($"从数据库获取的基础Y坐标: {baseY}");

        // 隐藏Home UI
        //if (HomeUI != null) HomeUI.SetActive(false);

        // 保存原始位置
        SaveOriginalPositions();

        // 设置初始位置
        SetInitialPositions();

        // 开始过渡动画
        StartCoroutine(PlayTransitionAnimation());
    }

    void SaveOriginalPositions()
    {
        originalBackPositions = new Vector3[5];
        originalToHomePositions = new Vector3[3];

        for (int i = 0; i < 5; i++)
        {
            if (BackQuads[i] != null)
                originalBackPositions[i] = BackQuads[i].position;
        }
        for (int i = 0; i < 3; i++)
        {
            if (ToHomeQuads[i] != null)
                originalToHomePositions[i] = ToHomeQuads[i].position;
        }

        // 保存TransitionQuad原始位置
        if (TransitionQuad != null)
            originalTransitionQuadPosition = TransitionQuad.position;
    }

    void SetInitialPositions()
    {
        Debug.Log($"设置初始位置，基础Y: {baseY}");

        // 1. 设置Back位置（仅Y坐标使用传递的数据）
        for (int i = 0; i < 5; i++)
        {
            if (BackQuads[i] != null)
            {
                Vector3 pos = originalBackPositions[i];  // 使用原始X和Z
                pos.y = baseY - (i * 700f);              // 仅修改Y
                BackQuads[i].position = pos;
                BackQuads[i].gameObject.SetActive(true);

                Debug.Log($"Back {i}: X={pos.x:F1}, Y={pos.y:F1}, Z={pos.z:F1}");
            }
        }

        // 2. 设置ToHome位置（使用原始X和Z，Y按照指定规则设置）
        float[] toHomeYOffsets = { ToHome1YOffset, ToHome2YOffset, ToHome3YOffset };

        for (int i = 0; i < 3; i++)
        {
            if (ToHomeQuads[i] != null)
            {
                Vector3 pos = originalToHomePositions[i];  // 使用原始X和Z
                pos.y = baseY + toHomeYOffsets[i];          // 修改Y：y + 偏移量
                ToHomeQuads[i].position = pos;
                ToHomeQuads[i].gameObject.SetActive(true);

                Debug.Log($"ToHome {i}: X={pos.x:F1}, Y={pos.y:F1} (基础Y{baseY} + 偏移{toHomeYOffsets[i]}), Z={pos.z:F1}");
            }
        }

        // 3. 设置TransitionQuad位置（使用原始位置）
        if (TransitionQuad != null && EnableTransitionQuad)
        {
            TransitionQuad.position = originalTransitionQuadPosition;
            TransitionQuad.gameObject.SetActive(true);

            Debug.Log($"TransitionQuad: X={originalTransitionQuadPosition.x:F1}, Y={originalTransitionQuadPosition.y:F1}, Z={originalTransitionQuadPosition.z:F1}");
        }

        Character.position = new Vector3(Character.position.x, baseY + 1800, Character.position.z);
        BackGround.position = new Vector3(BackGround.position.x, baseY + 2133, BackGround.position.z);
    }

    IEnumerator PlayTransitionAnimation()
    {
        Debug.Log("=== 开始过渡动画 ===");

        // 阶段1: Back和ToHome同时下移
        yield return StartCoroutine(Phase1_MoveDownTogether());

        // 阶段2: 停顿
        yield return new WaitForSeconds(PauseDuration);

        // 阶段3: ToHome向不同方向移出，同时TransitionQuad平移
        yield return StartCoroutine(Phase3_ToHomeAndTransitionExit());

        // 阶段4: 相机前移
        yield return StartCoroutine(Phase4_CameraMove());

        // 阶段5: 显示Home UI
        //Phase5_ShowHomeUI();
    }

    IEnumerator Phase1_MoveDownTogether()
    {
        Debug.Log("阶段1: Back和ToHome同时下移");

        bool[] backDisabled = new bool[5];
        int backDisabledCount = 0;

        bool toHomeStopped = false;

        while (backDisabledCount < 5 || !toHomeStopped)
        {
            // 1. 移动Back
            for (int i = 0; i < 5; i++)
            {
                if (!backDisabled[i] && BackQuads[i] != null && BackQuads[i].gameObject.activeSelf)
                {
                    // 向下移动
                    Vector3 pos = BackQuads[i].position;
                    pos.y -= BackMoveSpeed * Time.deltaTime;
                    BackQuads[i].position = pos;

                    // 检查是否到达禁用边界
                    if (pos.y < BackDisableY || ToHomeQuads[0].position.y <= -100)
                    {
                        BackQuads[i].gameObject.SetActive(false);
                        backDisabled[i] = true;
                        backDisabledCount++;

                        if (backDisabledCount == 5)
                            Debug.Log("所有Back已禁用");
                    }
                }
                /*
                // 向下移动
                Vector3 os = Character.position;
                os.y -= BackMoveSpeed * Time.deltaTime;
                Character.position = os;*/

            }

            // 2. 移动ToHome（如果还没停止）
            if (!toHomeStopped)
            {
                // 检查最下面的ToHome是否到达停止边界
                float lowestY = float.MaxValue;
                int lowestIndex = -1;

                for (int i = 0; i < 3; i++)
                {
                    if (ToHomeQuads[i] != null && ToHomeQuads[i].gameObject.activeSelf)
                    {
                        float currentY = ToHomeQuads[i].position.y;
                        if (currentY < lowestY)
                        {
                            lowestY = currentY;
                            lowestIndex = i;
                        }
                    }
                }

                // 如果还没到达停止边界，继续下移
                if (lowestY > ToHomeStopY)
                {

                    
                    for (int i = 0; i < 3; i++)
                    {
                        if (ToHomeQuads[i] != null && ToHomeQuads[i].gameObject.activeSelf)
                        {
                            Vector3 pos = ToHomeQuads[i].position;
                            pos.y -= ToHomeMoveSpeed * Time.deltaTime;
                            ToHomeQuads[i].position = pos;
                        }
                    }
                    // 向下移动
                    Vector3 os = Character.position;
                    os.y -= ToHomeMoveSpeed * Time.deltaTime;
                    Character.position = os;
                    os = BackGround.position;
                    os.y -= ToHomeMoveSpeed * Time.deltaTime;
                    BackGround.position = os;

                }
                else
                {
                    toHomeStopped = true;
                    Debug.Log($"最下面的ToHome[{lowestIndex}]已到达停止边界Y={ToHomeStopY}");
                }
            }

            yield return null;
        }

        Debug.Log("阶段1完成: Back全部禁用，ToHome到达停止边界");
    }

    IEnumerator Phase3_ToHomeAndTransitionExit()
    {
        Debug.Log("阶段3: ToHome向不同方向移出，同时TransitionQuad平移");

        // 准备ToHome移出参数
        Vector3[] directions = {
            ToHome1Direction.normalized,
            ToHome2Direction.normalized,
            ToHome3Direction.normalized
        };

        float[] speeds = { ToHome1Speed, ToHome2Speed, ToHome3Speed };
        float[] exitTimes = { ToHome1ExitTime, ToHome2ExitTime, ToHome3ExitTime };

        // TransitionQuad移出参数
        Vector3 transitionDirection = TransitionQuadDirection.normalized;
        float transitionSpeed = TransitionQuadSpeed;
        float transitionExitTime = TransitionQuadExitTime;

        // 移出计时器
        float[] timers = new float[3];
        bool[] exitCompleted = new bool[3];
        float transitionTimer = 0f;
        bool transitionExitCompleted = !EnableTransitionQuad || TransitionQuad == null;

        // 确保TransitionQuad在开始位置
        if (EnableTransitionQuad && TransitionQuad != null)
        {
            TransitionQuad.gameObject.SetActive(true);
        }

        while (!(exitCompleted[0] && exitCompleted[1] && exitCompleted[2] && transitionExitCompleted))
        {
            // 1. 移动ToHome
            for (int i = 0; i < 3; i++)
            {
                if (!exitCompleted[i] && ToHomeQuads[i] != null && ToHomeQuads[i].gameObject.activeSelf)
                {
                    // 移动
                    ToHomeQuads[i].Translate(directions[i] * speeds[i] * Time.deltaTime, Space.World);

                    // 计时
                    timers[i] += Time.deltaTime;

                    // 检查是否到达移出时间
                    if (timers[i] >= exitTimes[i])
                    {
                        ToHomeQuads[i].gameObject.SetActive(false);
                        exitCompleted[i] = true;
                        Debug.Log($"ToHome[{i}]移出完成");
                    }
                }
            }

            // 2. 移动TransitionQuad（与ToHome同步开始）
            if (!transitionExitCompleted && TransitionQuad != null && TransitionQuad.gameObject.activeSelf)
            {
                // 移动
                TransitionQuad.Translate(transitionDirection * transitionSpeed * Time.deltaTime, Space.World);

                // 计时
                transitionTimer += Time.deltaTime;

                // 检查是否到达移出时间
                if (transitionTimer >= transitionExitTime)
                {
                    TransitionQuad.gameObject.SetActive(false);
                    transitionExitCompleted = true;
                    Debug.Log($"TransitionQuad移出完成");
                }
            }

            yield return null;
        }

        Debug.Log("阶段3完成: 所有对象移出完成");
    }

    IEnumerator Phase4_CameraMove()
    {
        Debug.Log("阶段4: 相机前移");

        if (MainCamera == null || MainCamera.transform == null)
            yield break;

        Transform camTransform = MainCamera.transform;
        Vector3 startPos = camTransform.position;
        Vector3 targetPos = startPos + camTransform.forward * CameraMoveDistance;

        Transform camTransform1 = MainCamera1.transform;
        Vector3 startPos1 = camTransform1.position;
        Vector3 targetPos1 = startPos1 + camTransform1.forward * CameraMoveDistance;

        float timer = 0f;

        while (timer < CameraMoveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / CameraMoveDuration;
            camTransform.position = Vector3.Lerp(startPos, targetPos, t);
            camTransform1.position = Vector3.Lerp(startPos1, targetPos1, t);
            yield return null;
        }

        Debug.Log("阶段4完成: 相机移动完成");
        SceneManager.LoadScene("HomeScene");
    }

    void Phase5_ShowHomeUI()
    {
        Debug.Log("阶段5: 显示Home UI");

        if (HomeUI != null)
        {
            HomeUI.SetActive(true);

            // 淡入效果
            CanvasGroup group = HomeUI.GetComponent<CanvasGroup>();
            if (group == null) group = HomeUI.AddComponent<CanvasGroup>();

            StartCoroutine(FadeInUI(group));
        }

        // 通知过渡完成
        isTransitionActive = false;

        // 加载玩家数据
        LoadPlayerData();
    }

    IEnumerator FadeInUI(CanvasGroup group)
    {
        group.alpha = 0f;
        float duration = 0.5f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = time / duration;
            yield return null;
        }

        group.alpha = 1f;
    }

    void LoadPlayerData()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.CurrentPlayerData != null)
        {
            var playerData = PlayerDataManager.Instance.CurrentPlayerData;
            Debug.Log($"欢迎回来，{playerData.PlayerName}！");
        }
    }

    // ================== 调试方法 ==================
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || MainCamera == null)
            return;

        // 绘制Back禁用边界
        Gizmos.color = Color.red;
        Vector3 backBoundaryStart = new Vector3(-50, BackDisableY, 0);
        Vector3 backBoundaryEnd = new Vector3(50, BackDisableY, 0);
        Gizmos.DrawLine(backBoundaryStart, backBoundaryEnd);

        // 绘制ToHome停止边界
        Gizmos.color = Color.yellow;
        Vector3 toHomeBoundaryStart = new Vector3(-50, ToHomeStopY, 0);
        Vector3 toHomeBoundaryEnd = new Vector3(50, ToHomeStopY, 0);
        Gizmos.DrawLine(toHomeBoundaryStart, toHomeBoundaryEnd);

        // 绘制屏幕中心参考点
        Gizmos.color = Color.green;
        Vector3 screenCenter = MainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        Gizmos.DrawWireSphere(screenCenter, 1f);

        // 绘制TransitionQuad移动方向（如果在编辑器中设置了方向）
        if (TransitionQuad != null && EnableTransitionQuad && TransitionQuadDirection != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Vector3 startPos = TransitionQuad.position;
            Vector3 endPos = startPos + TransitionQuadDirection.normalized * 5f;
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(endPos, 0.3f);
        }
    }

    // ================== 辅助方法 ==================

    /// <summary>
    /// 重置所有对象到初始状态（用于调试）
    /// </summary>
    [ContextMenu("重置过渡状态")]
    public void ResetTransition()
    {
        StopAllCoroutines();
        SetInitialPositions();
        isTransitionActive = true;

        if (HomeUI != null)
            HomeUI.SetActive(false);

        Debug.Log("过渡状态已重置");
    }

    /// <summary>
    /// 开始过渡动画（外部调用）
    /// </summary>
    [ContextMenu("开始过渡动画")]
    public void StartTransition()
    {
        if (!isTransitionActive)
        {
            ResetTransition();
            StartCoroutine(PlayTransitionAnimation());
        }
    }

    /// <summary>
    /// 打印当前所有对象位置信息（用于调试）
    /// </summary>
    [ContextMenu("打印位置信息")]
    public void PrintPositionInfo()
    {
        Debug.Log("=== 当前对象位置信息 ===");
        Debug.Log($"基础Y坐标: {baseY}");

        for (int i = 0; i < 3; i++)
        {
            if (BackQuads[i] != null)
            {
                Debug.Log($"Back[{i}]: {BackQuads[i].position}");
            }

            if (ToHomeQuads[i] != null)
            {
                Debug.Log($"ToHome[{i}]: {ToHomeQuads[i].position}");
            }
        }

        if (TransitionQuad != null)
        {
            Debug.Log($"TransitionQuad: {TransitionQuad.position}");
        }
    }
}