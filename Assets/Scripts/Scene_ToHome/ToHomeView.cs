using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class ToHomeView : MonoBehaviour
{
    [SerializeField] private float topABoundary = 0.268f, bottomABoundary = -0.268f, scrollASpeed = 0.012f;
    [SerializeField] private List<Transform> backgroundAImages = new List<Transform>();
    
    [SerializeField] private float topBBoundary = 0.3095f, bottomBBoundary = -0.3095f, scrollBSpeed = 0.008f;
    [SerializeField] private List<Transform> backgroundBImages = new List<Transform>();
    
    [SerializeField] private float topCBoundary = 0.1941f, bottomCBoundary = -0.1941f, scrollCSpeed = 0.040f;
    [SerializeField] private List<Transform> backgroundCImages = new List<Transform>();
    
    [SerializeField] private float topDBoundary = 0.02f, bottomDBoundary = -0.08f, scrollDSpeed = 0.006f;
    [SerializeField] private Transform backgroundDImages;

    [SerializeField] private float scrollESpeed = 0.03f;
    [SerializeField] private List<Transform> backgroundEImages = new List<Transform>();
    public float pauseDuration = 0.3f;

    [SerializeField] private Transform backgroundFImages;

    [SerializeField] private Transform backgroundGImages;


    [SerializeField] private GameObject LightA;         
    [SerializeField] private GameObject LightB;            

    [SerializeField] private Animator animatorA;          
    [SerializeField] private Transform mainCamera;        



    // Start is called before the first frame update
    void Start()
    {
        if(PlayerDataManager.Instance != null)
        {
            topABoundary = PlayerDataManager.LastTopQuadAYPosition;
            topBBoundary = PlayerDataManager.LastTopQuadBYPosition;
            topCBoundary = PlayerDataManager.LastTopQuadCYPosition;
            topDBoundary = PlayerDataManager.LastTopQuadDYPosition;
        }

        // 设置初始位置
        SetInitialPositions();

        StartCoroutine(PlayAnimationSequence());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetInitialPositions()
    {
        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = backgroundAImages[i].position;  // 使用原始X和Z
            pos.y = topABoundary + (i * bottomABoundary);              // 仅修改Y
            backgroundAImages[i].position = pos;
            backgroundAImages[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = backgroundBImages[i].position;  // 使用原始X和Z
            pos.y = topBBoundary + (i * bottomBBoundary);              // 仅修改Y
            backgroundBImages[i].position = pos;
            backgroundBImages[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = backgroundCImages[i].position;  // 使用原始X和Z
            pos.y = topCBoundary + (i * bottomCBoundary);              // 仅修改Y
            backgroundCImages[i].position = pos;
            backgroundCImages[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < 1; i++)
        {
            Vector3 pos = backgroundDImages.position;  // 使用原始X和Z
            pos.y = topDBoundary + (i * bottomDBoundary);              // 仅修改Y
            backgroundDImages.position = pos;
            backgroundDImages.gameObject.SetActive(true);
        }

        /*
        Character.position = new Vector3(Character.position.x, baseY + 1800, Character.position.z);
        BackGround.position = new Vector3(BackGround.position.x, baseY + 2133, BackGround.position.z);
        */
    }

    IEnumerator PlayAnimationSequence()
    {
        // 阶段1：所有背景向下移动，直到backgroundEImages[0].y <= 0.003
        Debug.Log("阶段1：所有背景向下移动");
        while (backgroundEImages[0].position.y > 0.003f)   // 条件：未到达目标时继续
        {
            // 移动A组
            foreach (var t in backgroundAImages)
                t.Translate(0, -scrollASpeed * Time.deltaTime, 0, Space.World);

            // 移动B组
            foreach (var t in backgroundBImages)
                t.Translate(0, -scrollBSpeed * Time.deltaTime, 0, Space.World);

            // 移动C组
            foreach (var t in backgroundCImages)
                t.Translate(0, -scrollCSpeed * Time.deltaTime, 0, Space.World);

            // 移动D（单个）
            if (backgroundDImages != null)
                backgroundDImages.Translate(0, -scrollDSpeed * Time.deltaTime, 0, Space.World);

            // 移动E组
            foreach (var t in backgroundEImages)
                t.Translate(0, -scrollESpeed * Time.deltaTime, 0, Space.World);

            yield return null;   // 等待一帧
        }
        Debug.Log("阶段1完成");

        // 阶段2：禁用A、B、C、D，E继续移动直到backgroundEImages[1].y <= -0.0002
        Debug.Log("阶段2：禁用其他背景，E继续移动");
        LightA.SetActive(false);
        LightB.SetActive(true);
        // 禁用A
        foreach (var t in backgroundAImages)
            if (t != null) t.gameObject.SetActive(false);
        // 禁用B
        foreach (var t in backgroundBImages)
            if (t != null) t.gameObject.SetActive(false);
        // 禁用C
        foreach (var t in backgroundCImages)
            if (t != null) t.gameObject.SetActive(false);
        // 禁用D
        if (backgroundDImages != null)
            backgroundDImages.gameObject.SetActive(false);

        // 继续移动E，直到第二个元素达到条件
        while (backgroundEImages[1].position.y > -0.01f)
        {
            foreach (var t in backgroundEImages)
                t.Translate(0, -scrollESpeed * Time.deltaTime, 0, Space.World);
            yield return null;
        }
        Debug.Log("阶段2完成");

        // 阶段3：将Animator A的参数"BoolA"设为true，然后停顿pauseDuration秒
        Debug.Log("阶段3：触发Animator A");
        if (animatorA != null)
            animatorA.SetBool("A", true);   // 请根据实际参数名调整

        backgroundEImages[2].position = new Vector3(0, -0.018f, 0.138f);
        backgroundGImages.position = new Vector3(-0.2f, -4.37f, 0f);

        yield return new WaitForSeconds(pauseDuration);

        // 阶段4：将Animator B的参数"BoolB"设为true，同时移动摄像机到z=0
        Debug.Log("阶段4：触发Animator B，移动摄像机");
        if (animatorA != null)
            animatorA.SetBool("B", true);   // 请根据实际参数名调整
        /*
        if (mainCamera != null)
        {
            Vector3 camPos = mainCamera.position;
            Vector3 camRot = mainCamera.eulerAngles;
            while (camPos.z < 0f)
            {
                backgroundFImages.Translate(0, 0, -0.02f * Time.deltaTime, Space.World);
                camPos.z += 0.03f * Time.deltaTime;
                mainCamera.position = camPos;
                camRot.x += 2.1f * Time.deltaTime;
                mainCamera.eulerAngles = camRot;
                yield return null;
            }
        }
        mainCamera.position = new Vector3(0, 0, 0); 
        mainCamera.eulerAngles = new Vector3(2, 0, 0);
        */
        if (mainCamera != null)
        {
            Vector3 camPos = mainCamera.position;
            Vector3 camRot = mainCamera.eulerAngles;

            // 目标值
            float targetZ = 0f;
            float targetRotX = 2f;   // 你希望的最终旋转角度

            // 速度系数
            float camSpeedZ = 0.03f;
            float rotSpeedX = 2.1f;
            float bgSpeedZ = -0.02f; // background 移动速度（注意方向）

            // 计算需要移动的总距离（用于比例调整）
            float totalZ = targetZ - camPos.z;          // 应为正数（因为camPos.z < 0）
            float totalRotX = targetRotX - camRot.x;    // 旋转变化总量（需注意角度环绕，但你的范围不大，直接减即可）

            while (camPos.z < targetZ)   // 条件改为与目标比较
            {
                // 当前步长
                float stepZ = camSpeedZ * Time.deltaTime;
                float stepRotX = rotSpeedX * Time.deltaTime;

                // 如果这一步会超过目标
                if (camPos.z + stepZ > targetZ)
                {
                    // 剩余距离
                    float remainingZ = targetZ - camPos.z;
                    // 剩余比例
                    float ratio = remainingZ / stepZ;   // 0~1之间

                    // 按比例移动背景和旋转
                    backgroundFImages.Translate(0, 0, bgSpeedZ * Time.deltaTime * ratio, Space.World);

                    // 精确设置摄像机位置和旋转
                    camPos.z = targetZ;
                    camRot.x = targetRotX;   // 直接设为目标值，避免浮点误差

                    mainCamera.position = camPos;
                    mainCamera.eulerAngles = camRot;

                    break;  // 结束循环
                }
                else
                {
                    // 正常移动
                    camPos.z += stepZ;
                    camRot.x += stepRotX;
                    mainCamera.position = camPos;
                    mainCamera.eulerAngles = camRot;
                    backgroundFImages.Translate(0, 0, bgSpeedZ * Time.deltaTime, Space.World);
                }

                yield return null;
            }
        }

        // 阶段5：切换场景
        Debug.Log("阶段5：加载HomeScene");
   // X轴旋转2°
        SceneManager.LoadScene("HomeScene");
    }
}
