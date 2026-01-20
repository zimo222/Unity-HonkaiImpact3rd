using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginTransitionSimple : MonoBehaviour
{
    public string homeSceneName = "ToHomeScene";
    public Transform[] loginBackQuads = new Transform[5]; // 按从上到下顺序

    void Start()
    {
        // 可选：自动查找并排序
        if (loginBackQuads[0] == null) FindAndSortBackQuads();
    }

    // 登录成功后调用
    public void OnLoginSuccess()
    {
        if (loginBackQuads[0] != null)
        {
            float topQuadY = Mathf.Max(loginBackQuads[0].position.y, loginBackQuads[1].position.y, loginBackQuads[2].position.y, loginBackQuads[3].position.y, loginBackQuads[4].position.y);
            PlayerDataManager.LastTopQuadYPosition = topQuadY;
            Debug.Log($"保存最上面Quad Y坐标: {topQuadY}");
        }
        else
        {
            PlayerDataManager.LastTopQuadYPosition = 1400f;
        }

        SceneManager.LoadScene(homeSceneName);
    }

    void FindAndSortBackQuads()
    {
        GameObject[] allQuads = GameObject.FindGameObjectsWithTag("BackQuad");
        if (allQuads.Length >= 5)
        {
            System.Array.Sort(allQuads, (a, b) =>
                b.transform.position.y.CompareTo(a.transform.position.y));

            for (int i = 0; i < 5; i++)
                loginBackQuads[i] = allQuads[i].transform;
        }
    }
}