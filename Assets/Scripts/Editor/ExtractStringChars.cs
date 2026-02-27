using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

public class ExtractAllChars : EditorWindow
{
    private string searchFolder = "Assets";
    private string outputPath = "Assets/Resources/Fonts/SceneTextChars.txt";
    private string extensions = ".cs,.json,.csv,.xml,.txt,.html,.bytes";
    private string excludeFilesList = "";
    private bool extractStringsOnlyFromCs = true;
    private bool enableBinaryDetection = true;
    private bool enableCharFilter = true;
    private string customRanges = "20-7E,4E00-9FFF,3000-303F,FF00-FFEF";

    [MenuItem("Tools/提取所有字符集 (增强版)")]
    static void ShowWindow()
    {
        GetWindow<ExtractAllChars>("字符提取工具 (支持静态数据)");
    }

    void OnGUI()
    {
        GUILayout.Label("设置", EditorStyles.boldLabel);
        searchFolder = EditorGUILayout.TextField("搜索文件夹", searchFolder);
        outputPath = EditorGUILayout.TextField("输出文件", outputPath);
        extensions = EditorGUILayout.TextField("文件扩展名（逗号分隔）", extensions);

        GUILayout.Label("排除文件列表（每行一个相对路径）", EditorStyles.wordWrappedLabel);
        excludeFilesList = EditorGUILayout.TextArea(excludeFilesList, GUILayout.Height(60));

        extractStringsOnlyFromCs = EditorGUILayout.Toggle("仅从.cs提取字符串字面量", extractStringsOnlyFromCs);
        enableBinaryDetection = EditorGUILayout.Toggle("启用二进制文件检测", enableBinaryDetection);
        enableCharFilter = EditorGUILayout.Toggle("启用字符过滤", enableCharFilter);
        customRanges = EditorGUILayout.TextField("自定义Unicode范围", customRanges);

        GUILayout.Space(10);
        GUILayout.Label("从 ScriptableObject 提取", EditorStyles.boldLabel);
        if (GUILayout.Button("从选中的 ScriptableObject 资产提取字符"))
        {
            ExtractFromSelectedScriptableObjects();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("开始提取（扫描文件）"))
        {
            ExtractFromFiles();
        }
    }

    // ---------- 文件扫描提取 ----------
    void ExtractFromFiles()
    {
        var excludePaths = ParseExcludeList();
        string[] extList = ParseExtensionList();
        List<string> allFiles = GetFilesByExtensions(extList, excludePaths);

        HashSet<char> allChars = new HashSet<char>();

        // 【新增】加载已有结果（如果存在）
        if (File.Exists(outputPath))
        {
            string existing = File.ReadAllText(outputPath, Encoding.UTF8);
            foreach (char c in existing) allChars.Add(c);
        }

        int totalFiles = allFiles.Count;
        for (int i = 0; i < totalFiles; i++)
        {
            string file = allFiles[i];
            EditorUtility.DisplayProgressBar("提取字符", $"处理文件: {Path.GetFileName(file)} ({i + 1}/{totalFiles})", (float)i / totalFiles);
            if (enableBinaryDetection && IsBinaryFile(file))
            {
                Debug.LogWarning($"跳过二进制文件: {file}");
                continue;
            }
            string ext = Path.GetExtension(file).ToLower();
            try
            {
                string content = File.ReadAllText(file, Encoding.UTF8);
                HashSet<char> charsInFile = ExtractFromContent(content, ext);
                foreach (char c in charsInFile) allChars.Add(c);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"无法读取文件 {file}: {e.Message}");
            }
        }
        EditorUtility.ClearProgressBar();

        if (enableCharFilter) allChars = FilterChars(allChars, customRanges);
        SaveResult(allChars);
    }

    // ---------- 新增：递归遍历 ScriptableObject ----------
    void ExtractFromSelectedScriptableObjects()
    {
        Object[] selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        if (selectedAssets.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请在 Project 窗口中选择至少一个 ScriptableObject 资产", "确定");
            return;
        }

        HashSet<char> allChars = new HashSet<char>();
        // 加载已有结果
        if (File.Exists(outputPath))
        {
            string existing = File.ReadAllText(outputPath, Encoding.UTF8);
            foreach (char c in existing) allChars.Add(c);
        }

        HashSet<object> visited = new HashSet<object>(); // 避免循环引用

        int processed = 0;
        foreach (Object obj in selectedAssets)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!path.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"跳过非 .asset 文件: {path}");
                continue;
            }

            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null)
            {
                Debug.LogWarning($"无法加载为 ScriptableObject: {path}");
                continue;
            }

            EditorUtility.DisplayProgressBar("提取 ScriptableObject", $"处理: {so.name}", (float)processed / selectedAssets.Length);
            ExtractCharsFromObject(so, allChars, visited);
            processed++;
        }

        EditorUtility.ClearProgressBar();

        if (allChars.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "未提取到任何字符（选中的资产中可能没有字符串字段）", "确定");
            return;
        }

        if (enableCharFilter) allChars = FilterChars(allChars, customRanges);
        SaveResult(allChars);
    }

    // 递归遍历对象，提取所有字符串字符
    void ExtractCharsFromObject(object obj, HashSet<char> chars, HashSet<object> visited)
    {
        if (obj == null) return;
        if (visited.Contains(obj)) return; // 防止循环
        visited.Add(obj);

        // 处理字符串
        if (obj is string str)
        {
            foreach (char c in str)
                chars.Add(c);
            return;
        }

        // 处理集合（数组、List等）
        if (obj is IEnumerable enumerable && !(obj is string))
        {
            foreach (var item in enumerable)
            {
                ExtractCharsFromObject(item, chars, visited);
            }
            return; // 集合内的元素已递归处理，不需要再反射字段（因为集合本身没有字段）
        }

        // 处理普通对象：反射其字段
        System.Type type = obj.GetType();
        // 只处理可序列化的自定义类（Unity 或用户定义）
        if (type.IsPrimitive || type.IsEnum || type == typeof(decimal)) return;

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            // 只处理可序列化字段（public 或 [SerializeField]）
            if (!field.IsPublic && !System.Attribute.IsDefined(field, typeof(SerializeField)))
                continue;

            object fieldValue = field.GetValue(obj);
            ExtractCharsFromObject(fieldValue, chars, visited);
        }
    }

    // ---------- 原有辅助方法（保持不变） ----------
    List<string> ParseExcludeList()
    {
        return excludeFilesList.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                               .Select(line => line.Trim())
                               .Where(line => !string.IsNullOrEmpty(line))
                               .ToList();
    }

    string[] ParseExtensionList()
    {
        return extensions.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim().ToLower())
                         .ToArray();
    }

    List<string> GetFilesByExtensions(string[] extList, List<string> excludePaths)
    {
        List<string> allFiles = new List<string>();
        foreach (string ext in extList)
        {
            string pattern = ext.StartsWith(".") ? ext : "." + ext;
            string[] files = Directory.GetFiles(searchFolder, "*" + pattern, SearchOption.AllDirectories);
            allFiles.AddRange(files);
        }

        if (excludePaths.Count > 0)
        {
            allFiles = allFiles.Where(file =>
            {
                foreach (string exclude in excludePaths)
                {
                    if (file.IndexOf(exclude, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return false;
                }
                return true;
            }).ToList();
        }

        return allFiles;
    }

    HashSet<char> ExtractFromContent(string content, string ext)
    {
        HashSet<char> chars = new HashSet<char>();
        if (ext == ".cs" && extractStringsOnlyFromCs)
        {
            return ExtractCharsFromCSharp(content);
        }
        else if (ext == ".json")
        {
            return ExtractCharsFromJson(content);
        }
        else if (ext == ".csv")
        {
            return ExtractCharsFromCsv(content);
        }
        else if (ext == ".xml" || ext == ".html" || ext == ".txt")
        {
            return ExtractCharsFromPlainText(content);
        }
        else
        {
            return ExtractCharsFromPlainText(content);
        }
    }

    bool IsBinaryFile(string filePath)
    {
        byte[] sample = new byte[1024];
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            int read = fs.Read(sample, 0, sample.Length);
            int zeroCount = 0, nonPrintableCount = 0;
            for (int i = 0; i < read; i++)
            {
                if (sample[i] == 0)
                    zeroCount++;
                else if (sample[i] < 32 && sample[i] != 9 && sample[i] != 10 && sample[i] != 13)
                    nonPrintableCount++;
            }
            if (zeroCount > 0) return true;
            if ((float)nonPrintableCount / read > 0.3f) return true;
        }
        return false;
    }

    HashSet<char> ExtractCharsFromCSharp(string code)
    {
        HashSet<char> chars = new HashSet<char>();
        MatchCollection matches = Regex.Matches(code, @"@?""(.*?)""", RegexOptions.Singleline);
        foreach (Match match in matches)
        {
            string str = match.Groups[1].Value;
            str = Regex.Unescape(str);
            str = str.Replace("\"\"", "\"");
            foreach (char c in str) chars.Add(c);
        }
        return chars;
    }

    HashSet<char> ExtractCharsFromJson(string json)
    {
        HashSet<char> chars = new HashSet<char>();
        MatchCollection matches = Regex.Matches(json, @"\""((?:\\\""|[^\""])*)\""", RegexOptions.Singleline);
        foreach (Match match in matches)
        {
            string str = match.Groups[1].Value;
            str = Regex.Unescape(str);
            foreach (char c in str) chars.Add(c);
        }
        return chars;
    }

    HashSet<char> ExtractCharsFromCsv(string csv)
    {
        HashSet<char> chars = new HashSet<char>();
        var lines = csv.Split('\n');
        foreach (var line in lines)
        {
            var fields = line.Split(',');
            foreach (var field in fields)
            {
                string trimmed = field.Trim().Trim('\"');
                foreach (char c in trimmed) chars.Add(c);
            }
        }
        return chars;
    }

    HashSet<char> ExtractCharsFromPlainText(string text)
    {
        HashSet<char> chars = new HashSet<char>();
        foreach (char c in text)
        {
            if (c >= 0x20 && c != 0x7F) chars.Add(c);
        }
        return chars;
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
        Debug.Log($"提取完成！共 {chars.Count} 个不同字符。结果已保存至：{outputPath}");
    }
}