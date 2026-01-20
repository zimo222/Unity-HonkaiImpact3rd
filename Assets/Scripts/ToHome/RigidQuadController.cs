using UnityEngine;

[ExecuteInEditMode] // 允许在编辑模式下预览效果
public class RigidQuadController : MonoBehaviour
{
    [Header("刚体效果设置")]
    [Tooltip("是否冻结旋转")]
    public bool FreezeRotation = true;

    [Tooltip("是否锁定缩放")]
    public bool LockScale = true;

    [Tooltip("初始缩放（锁定后保持不变）")]
    public Vector3 InitialScale = Vector3.one;

    [Header("材质设置")]
    public Material RigidQuadMaterial;

    [Header("网格设置")]
    [Tooltip("网格细分级别（越高越平滑）")]
    [Range(1, 4)]
    public int MeshSubdivision = 2;

    [Tooltip("边缘厚度（视觉上的厚度感）")]
    [Range(0, 0.1f)]
    public float EdgeThickness = 0.01f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh originalMesh;
    private Mesh subdividedMesh;

    void Start()
    {
        InitializeQuad();
        ApplyRigidSettings();
    }

    void Update()
    {
        // 运行时持续确保刚体属性
        if (Application.isPlaying)
        {
            MaintainRigidProperties();
        }
    }

    void InitializeQuad()
    {
        // 获取或添加必要组件
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // 保存原始网格
        if (meshFilter.sharedMesh != null)
        {
            originalMesh = meshFilter.sharedMesh;

            // 创建细分网格（可选，提高视觉质量）
            if (MeshSubdivision > 1)
            {
                subdividedMesh = CreateSubdividedMesh(originalMesh, MeshSubdivision);
                meshFilter.mesh = subdividedMesh;
            }
        }

        // 应用刚性材质
        if (RigidQuadMaterial != null)
        {
            meshRenderer.material = RigidQuadMaterial;
        }
        else
        {
            // 使用内置的Unlit材质作为备选
            meshRenderer.material = new Material(Shader.Find("Unlit/Texture"));
        }

        // 设置初始缩放
        if (LockScale)
        {
            transform.localScale = InitialScale;
        }
    }

    void ApplyRigidSettings()
    {
        // 确保Transform属性
        if (FreezeRotation)
        {
            transform.rotation = Quaternion.identity;
        }

        // 确保不会有不均匀缩放
        Vector3 currentScale = transform.localScale;
        if (Mathf.Abs(currentScale.x - currentScale.y) > 0.001f ||
            Mathf.Abs(currentScale.x - currentScale.z) > 0.001f)
        {
            float avgScale = (currentScale.x + currentScale.y + currentScale.z) / 3f;
            transform.localScale = new Vector3(avgScale, avgScale, avgScale);
            Debug.LogWarning($"修正不均匀缩放: {currentScale} -> {transform.localScale}", this);
        }

        // 禁用不必要的物理组件
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null && !GetComponent<MeshCollider>())
        {
            DestroyImmediate(collider);
        }
    }

    void MaintainRigidProperties()
    {
        // 运行时保持属性
        if (FreezeRotation && transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.identity;
        }

        if (LockScale && transform.localScale != InitialScale)
        {
            transform.localScale = InitialScale;
        }
    }

    Mesh CreateSubdividedMesh(Mesh sourceMesh, int subdivisionLevel)
    {
        // 简单的网格细分（提高视觉质量）
        // 注意：对于复杂动画，可能需要更高级的细分算法
        Mesh newMesh = new Mesh();

        Vector3[] vertices = sourceMesh.vertices;
        Vector2[] uv = sourceMesh.uv;
        int[] triangles = sourceMesh.triangles;

        // 这里可以添加网格细分逻辑
        // 简化版本：直接复制原网格
        newMesh.vertices = vertices;
        newMesh.uv = uv;
        newMesh.triangles = triangles;
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        return newMesh;
    }

    void OnDrawGizmosSelected()
    {
        // 在编辑器中可视化刚体边界
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;

        // 绘制边框
        float halfSize = 0.5f + EdgeThickness;
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-halfSize, -halfSize, 0),
            new Vector3(halfSize, -halfSize, 0),
            new Vector3(halfSize, halfSize, 0),
            new Vector3(-halfSize, halfSize, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }

    // 编辑器扩展方法
#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/3D Object/Rigid Quad", false, 10)]
    static void CreateRigidQuad()
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "RigidQuad";

        // 移除默认的MeshCollider
        MeshCollider meshCollider = quad.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            DestroyImmediate(meshCollider);
        }

        // 添加刚体控制器
        RigidQuadController controller = quad.AddComponent<RigidQuadController>();

        // 定位到场景视图中心
        if (UnityEditor.SceneView.lastActiveSceneView != null)
        {
            quad.transform.position = UnityEditor.SceneView.lastActiveSceneView.pivot;
        }

        UnityEditor.Selection.activeGameObject = quad;
    }
#endif
}