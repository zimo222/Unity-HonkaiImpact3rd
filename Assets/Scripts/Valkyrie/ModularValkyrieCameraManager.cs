using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModularValkyrieCameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraPreset
    {
        public string presetName;                 // 预设名称
        public Vector3 cameraPosition;            // 摄像头位置
        public Vector3 cameraRotation;            // 摄像头旋转角度

        [Header("UI模块控制")]
        public CanvasGroup[] additionalUIModules; // 此预设特有的UI模块
        public bool[] moduleStates;               // 对应模块的显示状态（true=显示）

        public float transitionTime = 1.0f;       // 过渡时间
        public Ease moveEase = Ease.InOutQuad;    // 移动缓动类型
    }

    [System.Serializable]
    public class CharImagePreset
    {
        public Vector3 position = Vector3.zero;   // 位置 (包含Z坐标)
        public Vector3 rotation = Vector3.zero;   // 旋转
        public Vector3 scale = Vector3.one;       // 缩放
        public Vector2 size = Vector2.one * 100;  // UI大小
        public float alpha = 1.0f;                // 透明度
        public Color color = Color.white;         // 颜色
    }

    [Header("公共UI模块")]
    public CanvasGroup commonBaseUI;              // 基础UI（始终显示）
    public CanvasGroup[] sharedUIModules;         // 共享UI模块（1-4共用）

    [Header("摄像头设置")]
    public Camera characterCamera;                // 引用 Cam_CharacterUI
    public Transform characterCameraTransform;    // 摄像头的Transform组件

    [Header("CharImage 设置")]
    public RawImage characterRawImage;            // RawImage组件
    public RectTransform characterImageTransform; // CharImage的RectTransform

    [Header("预设数组")]
    public CameraPreset[] cameraPresets = new CameraPreset[5];
    public CharImagePreset[] charImagePresets = new CharImagePreset[5];

    [Header("切换按钮")]
    public Button[] presetButtons = new Button[5];

    [Header("过渡效果")]
    public Image fadeOverlay;                     // 淡入淡出遮罩
    public float uiFadeTime = 0.3f;               // UI淡入淡出时间
    public float charImageTransitionTime = 0.5f;  // CharImage过渡时间

    [Header("控制选项")]
    public bool syncCharImageWithCamera = true;   // 是否同步CharImage
    public bool startWithFirstPreset = true;      // 是否以第一个预设开始

    // 私有变量
    private int currentPresetIndex = 0;
    private bool isTransitioning = false;
    private DG.Tweening.Sequence currentTransitionSequence;
    private List<CanvasGroup> allModules = new List<CanvasGroup>();

    void Start()
    {
        InitializeComponents();
        SetupButtons();
        InitializeUIModules();
        InitializeArrays();

        if (startWithFirstPreset)
        {
            SetToFirstPreset();
        }
    }

    void InitializeComponents()
    {
        // 验证引用
        if (characterCamera == null)
        {
            Debug.LogError("请指定角色摄像头！");
            return;
        }

        if (characterCameraTransform == null && characterCamera != null)
        {
            characterCameraTransform = characterCamera.transform;
        }

        // 自动获取CharImage的RectTransform
        if (characterRawImage != null && characterImageTransform == null)
        {
            characterImageTransform = characterRawImage.GetComponent<RectTransform>();
        }
    }

    void InitializeArrays()
    {
        // 确保摄像头预设数组长度为5
        if (cameraPresets.Length != 5)
        {
            System.Array.Resize(ref cameraPresets, 5);
        }

        // 确保CharImage预设数组长度为5
        if (charImagePresets.Length != 5)
        {
            System.Array.Resize(ref charImagePresets, 5);
        }

        // 初始化默认值
        for (int i = 0; i < 5; i++)
        {
            // 摄像头预设默认值
            if (cameraPresets[i] == null)
            {
                cameraPresets[i] = new CameraPreset
                {
                    presetName = $"角度{i + 1}",
                    cameraPosition = characterCameraTransform != null
                        ? characterCameraTransform.position + Vector3.right * i * 2
                        : new Vector3(i * 2, 1, -5),
                    cameraRotation = Vector3.zero,
                    transitionTime = 1.0f,
                    moveEase = Ease.InOutQuad
                };
            }

            // CharImage预设默认值（基于当前状态）
            if (charImagePresets[i] == null)
            {
                charImagePresets[i] = new CharImagePreset();

                if (characterImageTransform != null)
                {
                    // 位置和变换（包含Z坐标）
                    charImagePresets[i].position = characterImageTransform.localPosition;
                    charImagePresets[i].rotation = characterImageTransform.localEulerAngles;
                    charImagePresets[i].scale = characterImageTransform.localScale;
                    charImagePresets[i].size = characterImageTransform.sizeDelta;
                }

                if (characterRawImage != null)
                {
                    // 视觉效果
                    charImagePresets[i].alpha = characterRawImage.color.a;
                    charImagePresets[i].color = characterRawImage.color;
                }
            }
        }
    }

    void SetupButtons()
    {
        for (int i = 0; i < presetButtons.Length; i++)
        {
            if (presetButtons[i] == null) continue;

            int index = i; // 创建闭包变量
            presetButtons[i].onClick.AddListener(() => SwitchToPreset(index));

            // 可选：在按钮上显示预设名称
            Text buttonText = presetButtons[i].GetComponentInChildren<Text>();
            if (buttonText != null && i < cameraPresets.Length && cameraPresets[i] != null)
            {
                buttonText.text = cameraPresets[i].presetName;
            }
        }
    }

    void InitializeUIModules()
    {
        allModules.Clear();

        // 初始化淡入淡出遮罩
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.gameObject.SetActive(false);
        }

        // 收集所有UI模块
        CollectAllUIModules();
    }

    void CollectAllUIModules()
    {
        allModules.Clear();

        // 添加基础UI
        if (commonBaseUI != null && !allModules.Contains(commonBaseUI))
        {
            allModules.Add(commonBaseUI);
        }

        // 添加共享模块
        if (sharedUIModules != null)
        {
            foreach (var module in sharedUIModules)
            {
                if (module != null && !allModules.Contains(module))
                {
                    allModules.Add(module);
                }
            }
        }

        // 添加各个预设的特有模块
        foreach (var preset in cameraPresets)
        {
            if (preset != null && preset.additionalUIModules != null)
            {
                foreach (var module in preset.additionalUIModules)
                {
                    if (module != null && !allModules.Contains(module))
                    {
                        allModules.Add(module);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 强制设置为第一个预设的状态
    /// </summary>
    private void SetToFirstPreset()
    {
        if (cameraPresets.Length == 0) return;

        // 确保第一个预设存在
        if (cameraPresets[0] == null)
        {
            cameraPresets[0] = new CameraPreset
            {
                presetName = "默认角度",
                cameraPosition = characterCameraTransform != null ? characterCameraTransform.position : new Vector3(0, 1, -5),
                cameraRotation = Vector3.zero,
                transitionTime = 1.0f,
                moveEase = Ease.InOutQuad
            };
        }

        // 确保第一个CharImage预设存在
        if (charImagePresets[0] == null)
        {
            charImagePresets[0] = new CharImagePreset();
        }

        // 强制设置到第一个预设（无过渡）
        SetCameraToPreset(0);
        SetCharImageToPreset(0);
        SetUIModulesForPreset(0);

        currentPresetIndex = 0;

        Debug.Log("已设置为第一个预设状态");
    }

    /// <summary>
    /// 切换到指定的预设角度
    /// </summary>
    public void SwitchToPreset(int presetIndex, bool useTransition = true)
    {
        // 功能1：如果目标索引和当前索引相同，什么都不做
        if (presetIndex == currentPresetIndex)
        {
            Debug.LogWarning($"目标预设{presetIndex}与当前预设相同，忽略操作");
            return;
        }

        // 检查是否正在过渡中
        if (isTransitioning)
        {
            Debug.LogWarning("正在切换中，请稍候...");
            return;
        }

        // 检查索引有效性
        if (presetIndex < 0 || presetIndex >= cameraPresets.Length)
        {
            Debug.LogError($"预设索引{presetIndex}超出范围！");
            return;
        }

        // 检查预设是否为空
        if (cameraPresets[presetIndex] == null)
        {
            Debug.LogError($"预设{presetIndex}为空！");
            return;
        }

        // 开始过渡
        if (useTransition)
        {
            StartCoroutine(TransitionToPreset(presetIndex));
        }
        else
        {
            // 直接设置（用于初始化）
            SetCameraToPreset(presetIndex);
            SetCharImageToPreset(presetIndex);
            SetUIModulesForPreset(presetIndex);
            currentPresetIndex = presetIndex;
        }
    }

    /// <summary>
    /// 过渡协程（支持模块化UI）
    /// </summary>
    private System.Collections.IEnumerator TransitionToPreset(int newIndex)
    {
        isTransitioning = true;

        CameraPreset fromPreset = cameraPresets[currentPresetIndex];
        CameraPreset toPreset = cameraPresets[newIndex];

        CharImagePreset fromCharImagePreset = charImagePresets[currentPresetIndex];
        CharImagePreset toCharImagePreset = charImagePresets[newIndex];

        Debug.Log($"切换到预设{newIndex}: {toPreset.presetName}");

        // 步骤1：获取需要隐藏和显示的模块
        List<CanvasGroup> modulesToHide = GetModulesToHide(currentPresetIndex, newIndex);
        List<CanvasGroup> modulesToShow = GetModulesToShow(newIndex);

        // 步骤2：淡出需要隐藏的模块
        if (modulesToHide.Count > 0)
        {
            Sequence hideSequence = DOTween.Sequence();
            foreach (var module in modulesToHide)
            {
                if (module != null && module.gameObject.activeSelf)
                {
                    hideSequence.Join(module.DOFade(0, uiFadeTime).SetEase(Ease.OutQuad));
                }
            }

            if (hideSequence.Duration() > 0)
            {
                hideSequence.OnComplete(() => {
                    foreach (var module in modulesToHide)
                    {
                        if (module != null)
                        {
                            module.gameObject.SetActive(false);
                        }
                    }
                });

                yield return hideSequence.WaitForCompletion();
            }
        }

        // 步骤3：淡入淡出遮罩效果（可选）
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.DOFade(0.7f, uiFadeTime * 0.3f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(uiFadeTime * 0.3f);
        }

        // 步骤4：创建主动画序列
        currentTransitionSequence = DOTween.Sequence();

        // 步骤5：摄像头移动
        if (characterCameraTransform != null)
        {
            currentTransitionSequence.Append(characterCameraTransform.DOMove(
                toPreset.cameraPosition,
                toPreset.transitionTime
            ).SetEase(toPreset.moveEase));

            currentTransitionSequence.Join(characterCameraTransform.DORotate(
                toPreset.cameraRotation,
                toPreset.transitionTime,
                RotateMode.Fast
            ).SetEase(toPreset.moveEase));
        }

        // 步骤6：CharImage同步移动（如果启用）
        if (syncCharImageWithCamera && characterImageTransform != null)
        {
            // 计算CharImage动画时间
            float charImageTime = Mathf.Min(toPreset.transitionTime, charImageTransitionTime);

            // === 位置变换（包含Z坐标）===
            currentTransitionSequence.Join(characterImageTransform.DOLocalMove(
                toCharImagePreset.position,  // 包含Z坐标的Vector3
                charImageTime
            ).SetEase(Ease.InOutQuad));

            // === 旋转变换 ===
            currentTransitionSequence.Join(characterImageTransform.DOLocalRotate(
                toCharImagePreset.rotation,
                charImageTime,
                RotateMode.Fast
            ).SetEase(Ease.InOutQuad));

            // === 缩放变换 ===
            currentTransitionSequence.Join(characterImageTransform.DOScale(
                toCharImagePreset.scale,
                charImageTime
            ).SetEase(Ease.InOutQuad));

            // === UI大小变换 ===
            currentTransitionSequence.Join(characterImageTransform.DOSizeDelta(
                toCharImagePreset.size,
                charImageTime
            ).SetEase(Ease.InOutQuad));

            // === 视觉效果 ===
            if (characterRawImage != null)
            {
                // 透明度变化
                currentTransitionSequence.Join(characterRawImage.DOFade(
                    toCharImagePreset.alpha,
                    charImageTime
                ).SetEase(Ease.InOutQuad));

                // 颜色变化
                currentTransitionSequence.Join(characterRawImage.DOColor(
                    toCharImagePreset.color,
                    charImageTime
                ).SetEase(Ease.InOutQuad));
            }
        }

        // 步骤7：等待动画完成
        yield return currentTransitionSequence.WaitForCompletion();

        // 步骤8：淡出遮罩
        if (fadeOverlay != null)
        {
            fadeOverlay.DOFade(0, uiFadeTime * 0.3f).SetEase(Ease.InQuad)
                .OnComplete(() => fadeOverlay.gameObject.SetActive(false));
            yield return new WaitForSeconds(uiFadeTime * 0.3f);
        }

        // 步骤9：显示新模块
        if (modulesToShow.Count > 0)
        {
            Sequence showSequence = DOTween.Sequence();
            foreach (var module in modulesToShow)
            {
                if (module != null)
                {
                    module.gameObject.SetActive(true);
                    module.alpha = 0;
                    showSequence.Join(module.DOFade(1, uiFadeTime).SetEase(Ease.InQuad));

                    // 启用交互
                    module.interactable = true;
                    module.blocksRaycasts = true;
                }
            }

            if (showSequence.Duration() > 0)
            {
                yield return showSequence.WaitForCompletion();
            }
        }

        // 步骤10：确保基础UI始终显示
        if (commonBaseUI != null)
        {
            commonBaseUI.alpha = 1;
            commonBaseUI.gameObject.SetActive(true);
            commonBaseUI.interactable = true;
            commonBaseUI.blocksRaycasts = true;
        }

        // 更新当前预设索引
        currentPresetIndex = newIndex;
        isTransitioning = false;

        Debug.Log($"切换完成: {toPreset.presetName}");
    }

    /// <summary>
    /// 直接设置摄像头位置（无过渡）
    /// </summary>
    private void SetCameraToPreset(int presetIndex)
    {
        if (characterCameraTransform == null || presetIndex >= cameraPresets.Length)
            return;

        CameraPreset preset = cameraPresets[presetIndex];
        if (preset == null) return;

        characterCameraTransform.position = preset.cameraPosition;
        characterCameraTransform.eulerAngles = preset.cameraRotation;

        Debug.Log($"设置摄像头到预设{presetIndex}: {preset.cameraPosition}");
    }

    /// <summary>
    /// 直接设置CharImage位置（无过渡）
    /// </summary>
    private void SetCharImageToPreset(int presetIndex)
    {
        if (!syncCharImageWithCamera || presetIndex >= charImagePresets.Length || characterImageTransform == null)
            return;

        CharImagePreset preset = charImagePresets[presetIndex];
        if (preset == null) return;

        // 设置位置（包含Z坐标）
        characterImageTransform.localPosition = preset.position;

        // 设置旋转
        characterImageTransform.localEulerAngles = preset.rotation;

        // 设置缩放
        characterImageTransform.localScale = preset.scale;

        // 设置UI大小
        characterImageTransform.sizeDelta = preset.size;

        // 设置视觉效果
        if (characterRawImage != null)
        {
            Color color = preset.color;
            color.a = preset.alpha;
            characterRawImage.color = color;
        }

        Debug.Log($"设置CharImage到预设{presetIndex}: 位置={preset.position}, Z={preset.position.z}");
    }

    /// <summary>
    /// 设置UI模块显示状态
    /// </summary>
    private void SetUIModulesForPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= cameraPresets.Length || cameraPresets[presetIndex] == null)
            return;

        CameraPreset preset = cameraPresets[presetIndex];

        // 第一步：确保基础UI始终显示
        if (commonBaseUI != null)
        {
            commonBaseUI.alpha = 1;
            commonBaseUI.gameObject.SetActive(true);
            commonBaseUI.interactable = true;
            commonBaseUI.blocksRaycasts = true;
        }

        // 第二步：设置共享模块（预设1-4显示，预设0隐藏）
        if (sharedUIModules != null)
        {
            bool showShared = (presetIndex >= 1 && presetIndex <= 4);
            foreach (var module in sharedUIModules)
            {
                if (module != null)
                {
                    if (showShared)
                    {
                        module.alpha = 1;
                        module.gameObject.SetActive(true);
                        module.interactable = true;
                        module.blocksRaycasts = true;
                    }
                    else
                    {
                        module.alpha = 0;
                        module.gameObject.SetActive(false);
                        module.interactable = false;
                        module.blocksRaycasts = false;
                    }
                }
            }
        }

        // 第三步：隐藏所有预设的特有模块
        for (int i = 0; i < cameraPresets.Length; i++)
        {
            if (cameraPresets[i] != null && cameraPresets[i].additionalUIModules != null)
            {
                foreach (var module in cameraPresets[i].additionalUIModules)
                {
                    if (module != null && module != commonBaseUI)
                    {
                        module.alpha = 0;
                        module.gameObject.SetActive(false);
                        module.interactable = false;
                        module.blocksRaycasts = false;
                    }
                }
            }
        }

        // 第四步：显示当前预设的特有模块
        if (preset.additionalUIModules != null)
        {
            for (int i = 0; i < preset.additionalUIModules.Length; i++)
            {
                var module = preset.additionalUIModules[i];
                if (module != null)
                {
                    bool shouldShow = preset.moduleStates != null && i < preset.moduleStates.Length
                                    ? preset.moduleStates[i] : true;

                    if (shouldShow)
                    {
                        module.alpha = 1;
                        module.gameObject.SetActive(true);
                        module.interactable = true;
                        module.blocksRaycasts = true;
                    }
                }
            }
        }

        Debug.Log($"设置UI模块到预设{presetIndex}");
    }

    /// <summary>
    /// 获取需要隐藏的模块
    /// </summary>
    private List<CanvasGroup> GetModulesToHide(int fromIndex, int toIndex)
    {
        List<CanvasGroup> hideList = new List<CanvasGroup>();

        if (fromIndex < 0 || fromIndex >= cameraPresets.Length || cameraPresets[fromIndex] == null)
            return hideList;

        if (toIndex < 0 || toIndex >= cameraPresets.Length || cameraPresets[toIndex] == null)
            return hideList;

        CameraPreset fromPreset = cameraPresets[fromIndex];
        CameraPreset toPreset = cameraPresets[toIndex];

        // 如果从预设1-4切换到预设0，需要隐藏共享模块
        if (fromIndex >= 1 && fromIndex <= 4 && toIndex == 0)
        {
            if (sharedUIModules != null)
            {
                foreach (var module in sharedUIModules)
                {
                    if (module != null && IsModuleVisible(module))
                    {
                        hideList.Add(module);
                    }
                }
            }
        }

        // 如果从预设0切换到预设1-4，不需要隐藏共享模块（因为它们本来就没显示）

        // 隐藏from预设的特有模块（如果to预设不包含）
        if (fromPreset.additionalUIModules != null)
        {
            foreach (var module in fromPreset.additionalUIModules)
            {
                if (module != null && IsModuleVisible(module) && !IsModuleInPreset(module, toPreset))
                {
                    hideList.Add(module);
                }
            }
        }

        return hideList;
    }

    /// <summary>
    /// 获取需要显示的模块
    /// </summary>
    private List<CanvasGroup> GetModulesToShow(int presetIndex)
    {
        List<CanvasGroup> showList = new List<CanvasGroup>();

        if (presetIndex < 0 || presetIndex >= cameraPresets.Length || cameraPresets[presetIndex] == null)
            return showList;

        CameraPreset preset = cameraPresets[presetIndex];

        // 如果切换到预设1-4，需要显示共享模块
        if (presetIndex >= 1 && presetIndex <= 4 && sharedUIModules != null)
        {
            foreach (var module in sharedUIModules)
            {
                if (module != null && !IsModuleVisible(module))
                {
                    showList.Add(module);
                }
            }
        }

        // 显示预设的特有模块
        if (preset.additionalUIModules != null)
        {
            for (int i = 0; i < preset.additionalUIModules.Length; i++)
            {
                var module = preset.additionalUIModules[i];
                if (module != null)
                {
                    bool shouldShow = preset.moduleStates != null && i < preset.moduleStates.Length
                                    ? preset.moduleStates[i] : true;

                    if (shouldShow && !IsModuleVisible(module))
                    {
                        showList.Add(module);
                    }
                }
            }
        }

        return showList;
    }

    /// <summary>
    /// 检查模块是否在预设中
    /// </summary>
    private bool IsModuleInPreset(CanvasGroup module, CameraPreset preset)
    {
        if (preset == null || preset.additionalUIModules == null) return false;

        foreach (var m in preset.additionalUIModules)
        {
            if (m == module) return true;
        }

        return false;
    }

    /// <summary>
    /// 检查模块当前是否可见
    /// </summary>
    private bool IsModuleVisible(CanvasGroup module)
    {
        if (module == null) return false;
        return module.gameObject.activeSelf && module.alpha > 0;
    }

    // ========== 新增：智能切换方法 ==========

    /// <summary>
    /// 智能切换到指定预设（检查是否相同）
    /// </summary>
    public void SmartSwitchToPreset(int presetIndex)
    {
        // 如果相同，直接返回
        if (presetIndex == currentPresetIndex)
        {
            Debug.Log($"已在预设{presetIndex}，无需切换");
            return;
        }

        SwitchToPreset(presetIndex);
    }

    /// <summary>
    /// 重置到第一个预设状态
    /// </summary>
    public void ResetToFirstPreset(bool useTransition = false)
    {
        if (currentPresetIndex == 0 && useTransition)
        {
            Debug.Log("已在第一个预设，无需重置");
            return;
        }

        if (useTransition)
        {
            SwitchToPreset(0);
        }
        else
        {
            SetCameraToPreset(0);
            SetCharImageToPreset(0);
            SetUIModulesForPreset(0);
            currentPresetIndex = 0;

            Debug.Log("已重置到第一个预设");
        }
    }

    /// <summary>
    /// 快速切换到下一个预设
    /// </summary>
    public void SwitchToNextPreset()
    {
        int nextIndex = (currentPresetIndex + 1) % cameraPresets.Length;
        // 如果下一个就是当前，什么都不做
        if (nextIndex != currentPresetIndex)
        {
            SwitchToPreset(nextIndex);
        }
    }

    /// <summary>
    /// 快速切换到上一个预设
    /// </summary>
    public void SwitchToPreviousPreset()
    {
        int prevIndex = (currentPresetIndex - 1 + cameraPresets.Length) % cameraPresets.Length;
        // 如果上一个就是当前，什么都不做
        if (prevIndex != currentPresetIndex)
        {
            SwitchToPreset(prevIndex);
        }
    }

    // ========== 保存和复制方法 ==========

    /// <summary>
    /// 保存当前摄像头状态到指定索引
    /// </summary>
    public void SaveCurrentCameraToPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= cameraPresets.Length ||
            characterCameraTransform == null)
            return;

        if (cameraPresets[presetIndex] == null)
            cameraPresets[presetIndex] = new CameraPreset();

        // 保存当前状态
        cameraPresets[presetIndex].cameraPosition = characterCameraTransform.position;
        cameraPresets[presetIndex].cameraRotation = characterCameraTransform.eulerAngles;

        Debug.Log($"已保存摄像头状态到预设 {presetIndex}");
    }

    /// <summary>
    /// 保存当前CharImage状态到指定索引
    /// </summary>
    public void SaveCurrentCharImageToPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= charImagePresets.Length ||
            characterImageTransform == null || characterRawImage == null)
            return;

        if (charImagePresets[presetIndex] == null)
            charImagePresets[presetIndex] = new CharImagePreset();

        // 保存位置和变换（包含Z坐标）
        charImagePresets[presetIndex].position = characterImageTransform.localPosition;
        charImagePresets[presetIndex].rotation = characterImageTransform.localEulerAngles;
        charImagePresets[presetIndex].scale = characterImageTransform.localScale;
        charImagePresets[presetIndex].size = characterImageTransform.sizeDelta;

        // 保存视觉效果
        charImagePresets[presetIndex].alpha = characterRawImage.color.a;
        charImagePresets[presetIndex].color = characterRawImage.color;

        Debug.Log($"已保存CharImage状态到预设 {presetIndex}, Z={characterImageTransform.localPosition.z}");
    }

    /// <summary>
    /// 一键保存当前所有状态到指定索引
    /// </summary>
    public void SaveCurrentAllToPreset(int presetIndex)
    {
        SaveCurrentCameraToPreset(presetIndex);
        SaveCurrentCharImageToPreset(presetIndex);

        Debug.Log($"已保存摄像头和CharImage状态到预设 {presetIndex}");
    }

    /// <summary>
    /// 复制一个预设到另一个
    /// </summary>
    public void CopyPreset(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= cameraPresets.Length ||
            toIndex < 0 || toIndex >= cameraPresets.Length)
            return;

        // 复制摄像头预设
        if (cameraPresets[fromIndex] != null)
        {
            cameraPresets[toIndex] = new CameraPreset
            {
                presetName = cameraPresets[fromIndex].presetName + " 副本",
                cameraPosition = cameraPresets[fromIndex].cameraPosition,
                cameraRotation = cameraPresets[fromIndex].cameraRotation,
                additionalUIModules = cameraPresets[fromIndex].additionalUIModules,
                moduleStates = cameraPresets[fromIndex].moduleStates != null ?
                    (bool[])cameraPresets[fromIndex].moduleStates.Clone() : null,
                transitionTime = cameraPresets[fromIndex].transitionTime,
                moveEase = cameraPresets[fromIndex].moveEase
            };
        }

        // 复制CharImage预设
        if (charImagePresets[fromIndex] != null)
        {
            charImagePresets[toIndex] = new CharImagePreset
            {
                position = charImagePresets[fromIndex].position,
                rotation = charImagePresets[fromIndex].rotation,
                scale = charImagePresets[fromIndex].scale,
                size = charImagePresets[fromIndex].size,
                alpha = charImagePresets[fromIndex].alpha,
                color = charImagePresets[fromIndex].color
            };
        }

        Debug.Log($"已复制预设 {fromIndex} 到 {toIndex}");
    }

    /// <summary>
    /// 切换CharImage同步功能
    /// </summary>
    public void ToggleCharImageSync(bool enable)
    {
        syncCharImageWithCamera = enable;
        Debug.Log($"CharImage同步: {(enable ? "启用" : "禁用")}");
    }

    /// <summary>
    /// 检查是否可以切换到指定预设
    /// </summary>
    public bool CanSwitchToPreset(int presetIndex)
    {
        // 检查是否相同
        if (presetIndex == currentPresetIndex)
            return false;

        // 检查是否正在切换
        if (isTransitioning)
            return false;

        // 检查索引范围
        if (presetIndex < 0 || presetIndex >= cameraPresets.Length)
            return false;

        // 检查预设是否存在
        if (cameraPresets[presetIndex] == null)
            return false;

        return true;
    }

    void OnDestroy()
    {
        // 清理DoTween动画
        if (currentTransitionSequence != null && currentTransitionSequence.IsActive())
        {
            currentTransitionSequence.Kill();
        }
    }

    // ========== 编辑器扩展 ==========
#if UNITY_EDITOR
    [ContextMenu("初始化所有数组")]
    void InitializeAllArraysInEditor()
    {
        InitializeArrays();
        Debug.Log("已初始化所有数组");
    }

    [ContextMenu("设置为第一个预设状态")]
    void SetToFirstPresetInEditor()
    {
        SetToFirstPreset();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("已设置为第一个预设状态");
    }

    [ContextMenu("保存当前状态到第一个预设")]
    void SaveCurrentToFirstPresetInEditor()
    {
        SaveCurrentAllToPreset(0);
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("已保存当前状态到第一个预设");
    }

    [ContextMenu("收集所有UI模块")]
    void CollectAllUIModulesInEditor()
    {
        CollectAllUIModules();
        Debug.Log($"已收集 {allModules.Count} 个UI模块");
    }
#endif
}