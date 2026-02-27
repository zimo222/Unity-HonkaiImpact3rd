using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// 可选：如果项目中有TextMeshPro，则启用TMP支持
#if USING_TMP || UNITY_2017_1_OR_NEWER
using TMPro;
#endif

public class ExtractSceneTextChars : EditorWindow
{
    private string outputPath = "Assets/Resources/Fonts/SceneTextChars.txt";
    private bool includeUnityUIText = true;
    private bool includeTextMeshPro = true;
    private bool includeInputFieldText = true;
    private bool mergeWithExisting = true;
    private bool enableCharFilter = true;
    private string customRanges = "20-7E,4E00-9FFF,3000-303F,FF00-FFEF";

    [MenuItem("Tools/提取场景文字字符集")]
    static void ShowWindow()
    {
        GetWindow<ExtractSceneTextChars>("场景文字提取");
    }

    void OnGUI()
    {
        GUILayout.Label("场景文字字符提取", EditorStyles.boldLabel);
        outputPath = EditorGUILayout.TextField("输出文件", outputPath);

        GUILayout.Space(10);
        GUILayout.Label("提取选项", EditorStyles.boldLabel);
        includeUnityUIText = EditorGUILayout.Toggle("Unity UI Text", includeUnityUIText);
        includeTextMeshPro = EditorGUILayout.Toggle("TextMeshPro (如果存在)", includeTextMeshPro);
        includeInputFieldText = EditorGUILayout.Toggle("InputField 文本", includeInputFieldText);

        GUILayout.Space(10);
        mergeWithExisting = EditorGUILayout.Toggle("合并到已有文件", mergeWithExisting);
        enableCharFilter = EditorGUILayout.Toggle("启用字符过滤", enableCharFilter);
        customRanges = EditorGUILayout.TextField("自定义Unicode范围", customRanges);

        GUILayout.Space(20);
        if (GUILayout.Button("提取当前场景文字", GUILayout.Height(40)))
        {
            ExtractFromCurrentScene();
        }
    }

    void ExtractFromCurrentScene()
    {
        // 获取当前激活的场景
        Scene currentScene = SceneManager.GetActiveScene();
        if (!currentScene.IsValid())
        {
            EditorUtility.DisplayDialog("错误", "当前没有打开的场景或场景无效", "确定");
            return;
        }

        // 获取场景中的所有根GameObject
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        HashSet<char> allChars = new HashSet<char>();

        // 如果合并已有文件，先加载
        if (mergeWithExisting && File.Exists(outputPath))
        {
            string existing = File.ReadAllText(outputPath, Encoding.UTF8);
            foreach (char c in existing)
                allChars.Add(c);
        }

        int totalObjects = CountAllGameObjects(rootObjects);
        int processed = 0;

        foreach (GameObject root in rootObjects)
        {
            processed = TraverseGameObject(root, allChars, processed, totalObjects);
        }

        EditorUtility.ClearProgressBar();

        if (enableCharFilter)
            allChars = FilterChars(allChars, customRanges);

        if (allChars.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "未提取到任何字符", "确定");
            return;
        }

        SaveResult(allChars);
    }

    int TraverseGameObject(GameObject go, HashSet<char> chars, int processedCount, int totalObjects)
    {
        if (go == null) return processedCount;

        // 更新进度条
        EditorUtility.DisplayProgressBar("提取场景文字", $"处理: {go.name}", (float)processedCount / totalObjects);
        processedCount++;

        // 提取当前GameObject上的文本组件
        ExtractTextFromGameObject(go, chars);

        // 递归子对象
        foreach (Transform child in go.transform)
        {
            if (child != null)
                processedCount = TraverseGameObject(child.gameObject, chars, processedCount, totalObjects);
        }

        return processedCount;
    }

    void ExtractTextFromGameObject(GameObject go, HashSet<char> chars)
    {
        // Unity UI Text
        if (includeUnityUIText)
        {
            Text uiText = go.GetComponent<Text>();
            if (uiText != null && !string.IsNullOrEmpty(uiText.text))
            {
                foreach (char c in uiText.text)
                    chars.Add(c);
            }
        }

        // TextMeshPro (如果启用并且TMP存在)
#if USING_TMP || UNITY_2017_1_OR_NEWER
        if (includeTextMeshPro)
        {
            // 尝试获取TMP组件（通过反射或直接类型）
            // 方式1：直接使用TMPro类型（需要using TMPro;）
            TMPro.TextMeshProUGUI tmpUGUI = go.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpUGUI != null && !string.IsNullOrEmpty(tmpUGUI.text))
            {
                foreach (char c in tmpUGUI.text)
                    chars.Add(c);
            }

            TMPro.TextMeshPro tmp3D = go.GetComponent<TMPro.TextMeshPro>();
            if (tmp3D != null && !string.IsNullOrEmpty(tmp3D.text))
            {
                foreach (char c in tmp3D.text)
                    chars.Add(c);
            }
        }
#endif

        // InputField 文本 (提取其输入框内的文本)
        if (includeInputFieldText)
        {
            InputField inputField = go.GetComponent<InputField>();
            if (inputField != null && !string.IsNullOrEmpty(inputField.text))
            {
                foreach (char c in inputField.text)
                    chars.Add(c);
            }

            // TMP_InputField 如果存在
#if USING_TMP || UNITY_2017_1_OR_NEWER
            TMPro.TMP_InputField tmpInput = go.GetComponent<TMPro.TMP_InputField>();
            if (tmpInput != null && !string.IsNullOrEmpty(tmpInput.text))
            {
                foreach (char c in tmpInput.text)
                    chars.Add(c);
            }
#endif
        }
    }

    int CountAllGameObjects(GameObject[] roots)
    {
        int count = 0;
        foreach (GameObject root in roots)
        {
            count += CountGameObjectAndChildren(root);
        }
        return count;
    }

    int CountGameObjectAndChildren(GameObject go)
    {
        int count = 1; // 自身
        foreach (Transform child in go.transform)
        {
            if (child != null)
                count += CountGameObjectAndChildren(child.gameObject);
        }
        return count;
    }

    HashSet<char> FilterChars(HashSet<char> rawChars, string rangesCsv)
    {
        var ranges = new List<(uint start, uint end)>();
        var parts = rangesCsv.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var range = part.Trim().Split('-');
            if (range.Length == 2)
            {
                uint start = uint.Parse(range[0], System.Globalization.NumberStyles.HexNumber);
                uint end = uint.Parse(range[1], System.Globalization.NumberStyles.HexNumber);
                ranges.Add((start, end));
            }
        }

        HashSet<char> filtered = new HashSet<char>();
        foreach (char c in rawChars)
        {
            uint code = (uint)c;
            foreach (var r in ranges)
            {
                if (code >= r.start && code <= r.end)
                {
                    filtered.Add(c);
                    break;
                }
            }
        }
        return filtered;
    }

    void SaveResult(HashSet<char> chars)
    {
        string result = new string(chars.OrderBy(c => c).ToArray());
        File.WriteAllText(outputPath, result, new UTF8Encoding(false));
        AssetDatabase.Refresh();
        Debug.Log($"场景文字提取完成！共 {chars.Count} 个不同字符。结果已保存至：{outputPath}");
    }
}