using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 场景跳转数据类
[System.Serializable]
public class SceneTransitionData
{
    public string sceneName;
    public object data; // 可以是任意数据
    public System.Type dataType;

    // 构造函数
    public SceneTransitionData(string scene, object obj = null)
    {
        sceneName = scene;
        data = obj;
        dataType = obj?.GetType();
    }

    // 安全获取数据并转换为指定类型
    public T GetData<T>()
    {
        if (data is T typedData)
        {
            return typedData;
        }
        return default(T);
    }
}

// 单例场景数据管理器
public class SceneDataManager : MonoBehaviour
{
    private static SceneDataManager _instance;
    private Stack<SceneTransitionData> _sceneHistory = new Stack<SceneTransitionData>();
    private Stack<SceneTransitionData> _sceneHistoryWithData = new Stack<SceneTransitionData>();

    // 单例实例访问器
    public static SceneDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("SceneDataManager");
                _instance = obj.AddComponent<SceneDataManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    // 初始化
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化时记录当前场景
            //RecordCurrentScene();

            Debug.Log("SceneDataManager 初始化完成");
        }
    }

    // 记录当前场景到历史堆栈（不带数据）
    public void PushCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneTransitionData transitionData = new SceneTransitionData(currentScene);
        _sceneHistory.Push(transitionData);
        Debug.Log($"记录场景: {currentScene}, 普通堆栈大小: {_sceneHistory.Count}");
    }

    // 记录当前场景到历史堆栈（带数据）
    public void PushCurrentSceneWithData(object data)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneTransitionData transitionData = new SceneTransitionData(currentScene, data);
        _sceneHistoryWithData.Push(transitionData);
        Debug.Log($"记录场景: {currentScene}, 带数据堆栈大小: {_sceneHistoryWithData.Count}");
    }

    // 获取上一个场景并从堆栈移除（不带数据）
    public string PopPreviousScene()
    {
        if (_sceneHistory.Count > 0)
        {
            SceneTransitionData previousScene = _sceneHistory.Pop();
            Debug.Log($"弹出场景: {previousScene.sceneName}, 剩余堆栈大小: {_sceneHistory.Count}");
            return previousScene.sceneName;
        }
        else
        {
            Debug.LogWarning("场景历史堆栈为空，返回默认场景");
            return GetDefaultScene();
        }
    }

    // 获取上一个场景和数据的完整记录并从堆栈移除
    public SceneTransitionData PopPreviousSceneWithData()
    {
        if (_sceneHistoryWithData.Count > 0)
        {
            SceneTransitionData previousScene = _sceneHistoryWithData.Pop();
            Debug.Log($"弹出场景(带数据): {previousScene.sceneName}, 剩余堆栈大小: {_sceneHistoryWithData.Count}");
            return previousScene;
        }
        else
        {
            Debug.LogWarning("带数据的场景历史堆栈为空，返回默认场景");
            return new SceneTransitionData(GetDefaultScene());
        }
    }

    // 获取上一个场景但不移除（用于查看）
    public string PeekPreviousScene()
    {
        if (_sceneHistory.Count > 0)
        {
            return _sceneHistory.Peek().sceneName;
        }
        return null;
    }

    // 获取带数据的上一个场景但不移除（用于查看）
    public SceneTransitionData PeekPreviousSceneWithData()
    {
        if (_sceneHistoryWithData.Count > 0)
        {
            return _sceneHistoryWithData.Peek();
        }
        return null;
    }

    // 记录当前场景（内部使用）
    private void RecordCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        // 只在堆栈为空时记录初始场景
        if (_sceneHistory.Count == 0)
        {
            _sceneHistory.Push(new SceneTransitionData(currentScene));
        }
    }

    // 清空所有历史记录
    public void ClearHistory()
    {
        _sceneHistory.Clear();
        _sceneHistoryWithData.Clear();
        Debug.Log("已清空所有场景历史记录");
    }

    // 清空普通历史记录
    public void ClearHistoryWithoutData()
    {
        _sceneHistory.Clear();
        Debug.Log("已清空普通场景历史记录");
    }

    // 清空带数据的历史记录
    public void ClearHistoryWithData()
    {
        _sceneHistoryWithData.Clear();
        Debug.Log("已清空带数据的场景历史记录");
    }

    // 获取普通历史记录数量
    public int GetHistoryCount()
    {
        return _sceneHistory.Count;
    }

    // 获取带数据的历史记录数量
    public int GetHistoryWithDataCount()
    {
        return _sceneHistoryWithData.Count;
    }

    // 检查是否有历史记录
    public bool HasHistory()
    {
        return _sceneHistory.Count > 0;
    }

    // 检查是否有带数据的历史记录
    public bool HasHistoryWithData()
    {
        return _sceneHistoryWithData.Count > 0;
    }

    // 获取默认场景（可根据需要修改）
    private string GetDefaultScene()
    {
        // 你可以在这里修改默认场景名
        return "HomeScene";
    }

    // 设置默认场景
    public void SetDefaultScene(string sceneName)
    {
        // 这个方法允许你在运行时修改默认场景
        // 注意：这不会改变已有的默认场景返回，只会影响之后的调用
        // 你可以将这个值保存到PlayerPrefs中实现持久化
        PlayerPrefs.SetString("DefaultScene", sceneName);
        PlayerPrefs.Save();
    }

    // 快速跳转场景并记录当前场景（不带数据）
    public void LoadSceneAndRecord(string sceneName)
    {
        PushCurrentScene();
        SceneManager.LoadScene(sceneName);
    }

    // 快速跳转场景并记录当前场景（带数据）
    public void LoadSceneAndRecordWithData(string sceneName, object data = null)
    {
        if (data != null)
        {
            PushCurrentSceneWithData(data);
        }
        else
        {
            PushCurrentScene();
        }
        SceneManager.LoadScene(sceneName);
    }

    // 返回上一个场景
    public void ReturnToPreviousScene()
    {
        string previousScene = PopPreviousScene();
        SceneManager.LoadScene(previousScene);
    }

    // 返回上一个场景（带数据）
    public void ReturnToPreviousSceneWithData()
    {
        SceneTransitionData previousScene = PopPreviousSceneWithData();
        SceneManager.LoadScene(previousScene.sceneName);
    }

    // 异步加载场景并记录当前场景（不带数据）
    public void LoadSceneAsyncAndRecord(string sceneName, System.Action<float> onProgress = null)
    {
        PushCurrentScene();
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onProgress));
    }

    // 异步加载场景协程
    private System.Collections.IEnumerator LoadSceneAsyncCoroutine(string sceneName, System.Action<float> onProgress)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            onProgress?.Invoke(progress);
            yield return null;
        }
    }

    // 获取堆栈中的所有场景名（调试用）
    public string[] GetAllSceneHistory()
    {
        List<string> scenes = new List<string>();
        foreach (var data in _sceneHistory)
        {
            scenes.Add(data.sceneName);
        }
        return scenes.ToArray();
    }

    // 获取带数据堆栈中的所有场景名（调试用）
    public string[] GetAllSceneHistoryWithData()
    {
        List<string> scenes = new List<string>();
        foreach (var data in _sceneHistoryWithData)
        {
            scenes.Add(data.sceneName);
        }
        return scenes.ToArray();
    }

    // 保存历史记录到PlayerPrefs（可选）
    public void SaveHistoryToPlayerPrefs()
    {
        // 保存普通历史记录
        int count = _sceneHistory.Count;
        PlayerPrefs.SetInt("SceneHistoryCount", count);

        int index = 0;
        foreach (var data in _sceneHistory)
        {
            PlayerPrefs.SetString($"SceneHistory_{index}", data.sceneName);
            index++;
        }

        // 注意：由于object无法直接序列化到PlayerPrefs，
        // 带数据的历史记录需要特殊处理，这里只保存场景名
        int countWithData = _sceneHistoryWithData.Count;
        PlayerPrefs.SetInt("SceneHistoryWithDataCount", countWithData);

        index = 0;
        foreach (var data in _sceneHistoryWithData)
        {
            PlayerPrefs.SetString($"SceneHistoryWithData_{index}", data.sceneName);
            index++;
        }

        PlayerPrefs.Save();
        Debug.Log("场景历史记录已保存到PlayerPrefs");
    }

    // 从PlayerPrefs加载历史记录（可选）
    public void LoadHistoryFromPlayerPrefs()
    {
        ClearHistory();

        // 加载普通历史记录
        int count = PlayerPrefs.GetInt("SceneHistoryCount", 0);
        for (int i = 0; i < count; i++)
        {
            string sceneName = PlayerPrefs.GetString($"SceneHistory_{i}", "");
            if (!string.IsNullOrEmpty(sceneName))
            {
                _sceneHistory.Push(new SceneTransitionData(sceneName));
            }
        }

        // 加载带数据的历史记录（数据部分无法恢复，只能恢复场景名）
        int countWithData = PlayerPrefs.GetInt("SceneHistoryWithDataCount", 0);
        for (int i = 0; i < countWithData; i++)
        {
            string sceneName = PlayerPrefs.GetString($"SceneHistoryWithData_{i}", "");
            if (!string.IsNullOrEmpty(sceneName))
            {
                _sceneHistoryWithData.Push(new SceneTransitionData(sceneName));
            }
        }

        Debug.Log("从PlayerPrefs加载场景历史记录完成");
    }
}