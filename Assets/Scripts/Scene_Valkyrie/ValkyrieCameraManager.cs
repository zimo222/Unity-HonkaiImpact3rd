using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValkyrieCameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraPreset
    {
        public string presetName;                 // 预设名称
        public Vector3 cameraPosition;            // 摄像头位置
        public Vector3 cameraRotation;            // 摄像头旋转角度
        public Vector3 playerRotation;            // 角色模型旋转角度（新增）
        public bool[] uiModuleStates;             // 对应UI组件数组中每个组件的显示状态
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

    [Header("摄像头设置")]
    public Camera characterCamera;                // 引用 Cam_CharacterUI
    public Transform characterCameraTransform;    // 摄像头的Transform组件

    [Header("角色模型设置")]
    public Transform playerModelTransform;        // 角色模型的Transform（新增）

    [Header("CharImage 设置")]
    public RawImage characterRawImage;            // RawImage组件
    public RectTransform characterImageTransform; // CharImage的RectTransform

    [Header("预设数量")]
    [Range(1, 20)]
    public int presetCount = 6;                   // 预设数量

    [Header("UI组件数组")]
    public CanvasGroup[] uiModules = new CanvasGroup[0]; // 所有可能用到的UI组件

    [Header("摄像头预设数组")]
    public CameraPreset[] cameraPresets;          // 预设数组

    [Header("CharImage预设数组")]
    public CharImagePreset[] charImagePresets;    // 对应预设数组

    [Header("切换按钮")]
    public Button[] presetButtons;                // 预设按钮数组

    [Header("过渡效果")]
    public Image fadeOverlay;                     // 淡入淡出遮罩
    public float uiFadeTime = 0.3f;               // UI淡入淡出时间
    public float charImageTransitionTime = 0.5f;  // CharImage过渡时间

    [Header("控制选项")]
    public bool syncCharImageWithCamera = true;   // 是否同步CharImage
    public bool syncPlayerRotation = true;        // 是否同步角色旋转（新增）

    [Header("默认状态")]
    public bool startWithFirstPreset = true;      // 是否以第一个预设开始

    // 私有变量
    private int currentPresetIndex = 0;
    private bool isTransitioning = false;
    private DG.Tweening.Sequence currentTransitionSequence;
    private bool[] currentUIStates;               // 当前UI组件的显示状态

    void Start()
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

        // 根据presetCount调整数组大小
        AdjustArraysByPresetCount();

        // 初始化数组
        InitializeArrays();

        // 设置按钮事件
        SetupButtons();

        // 初始化UI状态
        InitializeUI();

        // 默认设置为第一个预设状态
        if (startWithFirstPreset)
        {
            SetToFirstPreset();
        }
    }

    /// <summary>
    /// 根据presetCount调整数组大小
    /// </summary>
    private void AdjustArraysByPresetCount()
    {
        // 调整摄像头预设数组
        if (cameraPresets == null || cameraPresets.Length != presetCount)
        {
            System.Array.Resize(ref cameraPresets, presetCount);
        }

        // 调整CharImage预设数组
        if (charImagePresets == null || charImagePresets.Length != presetCount)
        {
            System.Array.Resize(ref charImagePresets, presetCount);
        }

        // 调整按钮数组
        if (presetButtons == null || presetButtons.Length != presetCount)
        {
            System.Array.Resize(ref presetButtons, presetCount);
        }
    }

    void InitializeArrays()
    {
        // 初始化当前UI状态数组
        currentUIStates = new bool[uiModules.Length];
        for (int i = 0; i < uiModules.Length; i++)
        {
            currentUIStates[i] = false; // 默认都隐藏
        }

        // 初始化预设数组
        for (int i = 0; i < presetCount; i++)
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
                    playerRotation = playerModelTransform != null
                        ? playerModelTransform.eulerAngles
                        : Vector3.zero, // 默认使用角色当前旋转
                    uiModuleStates = new bool[uiModules.Length],
                    transitionTime = 1.0f,
                    moveEase = Ease.InOutQuad
                };

                // 初始化bool数组，默认全部false
                for (int j = 0; j < uiModules.Length; j++)
                {
                    cameraPresets[i].uiModuleStates[j] = false;
                }
            }
            // 如果预设存在但uiModuleStates为null，初始化它
            else if (cameraPresets[i].uiModuleStates == null)
            {
                cameraPresets[i].uiModuleStates = new bool[uiModules.Length];
                for (int j = 0; j < uiModules.Length; j++)
                {
                    cameraPresets[i].uiModuleStates[j] = false;
                }
            }
            // 如果数组长度不匹配，调整长度
            else if (cameraPresets[i].uiModuleStates.Length != uiModules.Length)
            {
                System.Array.Resize(ref cameraPresets[i].uiModuleStates, uiModules.Length);
            }

            // CharImage预设默认值
            if (charImagePresets[i] == null)
            {
                charImagePresets[i] = new CharImagePreset();

                if (characterImageTransform != null)
                {
                    charImagePresets[i].position = characterImageTransform.localPosition;
                    charImagePresets[i].rotation = characterImageTransform.localEulerAngles;
                    charImagePresets[i].scale = characterImageTransform.localScale;
                    charImagePresets[i].size = characterImageTransform.sizeDelta;
                }

                if (characterRawImage != null)
                {
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

    void InitializeUI()
    {
        // 初始化淡入淡出遮罩
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.gameObject.SetActive(false);
        }

        // 初始隐藏所有UI组件
        for (int i = 0; i < uiModules.Length; i++)
        {
            if (uiModules[i] != null)
            {
                uiModules[i].alpha = 0;
                uiModules[i].gameObject.SetActive(false);
                uiModules[i].interactable = false;
                uiModules[i].blocksRaycasts = false;
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
                playerRotation = playerModelTransform != null ? playerModelTransform.eulerAngles : Vector3.zero,
                uiModuleStates = new bool[uiModules.Length],
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
        SetPlayerRotationToPreset(0); // 新增：设置角色旋转
        SetCharImageToPreset(0);
        SetUIToPreset(0);

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
        if (presetIndex < 0 || presetIndex >= presetCount)
        {
            Debug.LogError($"预设索引{presetIndex}超出范围！(0-{presetCount - 1})");
            return;
        }

        // 检查预设是否为空
        if (presetIndex >= cameraPresets.Length || cameraPresets[presetIndex] == null)
        {
            Debug.LogError($"预设{presetIndex}为空！");
            return;
        }

        // 检查UI状态数组是否匹配
        if (cameraPresets[presetIndex].uiModuleStates == null ||
            cameraPresets[presetIndex].uiModuleStates.Length != uiModules.Length)
        {
            Debug.LogError($"预设{presetIndex}的UI状态数组不匹配！");
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
            SetPlayerRotationToPreset(presetIndex); // 新增：设置角色旋转
            SetCharImageToPreset(presetIndex);
            SetUIToPreset(presetIndex);
            currentPresetIndex = presetIndex;
        }
    }

    /// <summary>
    /// 过渡协程（包含角色旋转）
    /// </summary>
    private System.Collections.IEnumerator TransitionToPreset(int newIndex)
    {
        isTransitioning = true;

        CameraPreset fromPreset = cameraPresets[currentPresetIndex];
        CameraPreset toPreset = cameraPresets[newIndex];

        CharImagePreset fromCharImagePreset = charImagePresets[currentPresetIndex];
        CharImagePreset toCharImagePreset = charImagePresets[newIndex];

        Debug.Log($"切换到预设{newIndex}: {toPreset.presetName}");

        // 步骤1：分析哪些UI组件需要隐藏，哪些需要显示
        List<CanvasGroup> modulesToHide = new List<CanvasGroup>();
        List<CanvasGroup> modulesToShow = new List<CanvasGroup>();

        for (int i = 0; i < uiModules.Length; i++)
        {
            if (uiModules[i] == null) continue;

            bool currentState = currentUIStates[i];
            bool targetState = toPreset.uiModuleStates[i];

            if (currentState && !targetState)
            {
                modulesToHide.Add(uiModules[i]);
            }
            else if (!currentState && targetState)
            {
                modulesToShow.Add(uiModules[i]);
            }
        }

        // 步骤2：淡出需要隐藏的UI组件
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

            hideSequence.OnComplete(() => {
                foreach (var module in modulesToHide)
                {
                    if (module != null)
                    {
                        module.gameObject.SetActive(false);
                        module.interactable = false;
                        module.blocksRaycasts = false;
                    }
                }
            });

            yield return hideSequence.WaitForCompletion();
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

        // 步骤6：角色模型旋转（如果启用）
        if (syncPlayerRotation && playerModelTransform != null)
        {
            currentTransitionSequence.Join(playerModelTransform.DORotate(
                toPreset.playerRotation,
                toPreset.transitionTime,
                RotateMode.Fast
            ).SetEase(toPreset.moveEase));
        }

        // 步骤7：CharImage同步移动（如果启用）
        if (syncCharImageWithCamera && characterImageTransform != null)
        {
            float charImageTime = Mathf.Min(toPreset.transitionTime, charImageTransitionTime);

            currentTransitionSequence.Join(characterImageTransform.DOLocalMove(
                toCharImagePreset.position,
                charImageTime
            ).SetEase(Ease.InOutQuad));

            currentTransitionSequence.Join(characterImageTransform.DOLocalRotate(
                toCharImagePreset.rotation,
                charImageTime,
                RotateMode.Fast
            ).SetEase(Ease.InOutQuad));

            currentTransitionSequence.Join(characterImageTransform.DOScale(
                toCharImagePreset.scale,
                charImageTime
            ).SetEase(Ease.InOutQuad));

            currentTransitionSequence.Join(characterImageTransform.DOSizeDelta(
                toCharImagePreset.size,
                charImageTime
            ).SetEase(Ease.InOutQuad));

            if (characterRawImage != null)
            {
                currentTransitionSequence.Join(characterRawImage.DOFade(
                    toCharImagePreset.alpha,
                    charImageTime
                ).SetEase(Ease.InOutQuad));

                currentTransitionSequence.Join(characterRawImage.DOColor(
                    toCharImagePreset.color,
                    charImageTime
                ).SetEase(Ease.InOutQuad));
            }
        }

        // 步骤8：等待动画完成
        yield return currentTransitionSequence.WaitForCompletion();

        // 步骤9：淡出遮罩
        if (fadeOverlay != null)
        {
            fadeOverlay.DOFade(0, uiFadeTime * 0.3f).SetEase(Ease.InQuad)
                .OnComplete(() => fadeOverlay.gameObject.SetActive(false));
            yield return new WaitForSeconds(uiFadeTime * 0.3f);
        }

        // 步骤10：显示需要显示的UI组件
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
                }
            }

            showSequence.OnComplete(() => {
                foreach (var module in modulesToShow)
                {
                    if (module != null)
                    {
                        module.interactable = true;
                        module.blocksRaycasts = true;
                    }
                }
            });

            yield return showSequence.WaitForCompletion();
        }

        // 更新当前UI状态
        for (int i = 0; i < uiModules.Length; i++)
        {
            if (uiModules[i] != null)
            {
                currentUIStates[i] = toPreset.uiModuleStates[i];
            }
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
        if (characterCameraTransform == null || presetIndex >= presetCount)
            return;

        CameraPreset preset = cameraPresets[presetIndex];
        if (preset == null) return;

        characterCameraTransform.position = preset.cameraPosition;
        characterCameraTransform.eulerAngles = preset.cameraRotation;

        Debug.Log($"设置摄像头到预设{presetIndex}: {preset.cameraPosition}");
    }

    /// <summary>
    /// 直接设置角色旋转（无过渡）
    /// </summary>
    private void SetPlayerRotationToPreset(int presetIndex)
    {
        if (!syncPlayerRotation || playerModelTransform == null || presetIndex >= presetCount)
            return;

        CameraPreset preset = cameraPresets[presetIndex];
        if (preset == null) return;

        playerModelTransform.eulerAngles = preset.playerRotation;

        Debug.Log($"设置角色旋转到预设{presetIndex}: {preset.playerRotation}");
    }

    /// <summary>
    /// 直接设置CharImage位置（无过渡）
    /// </summary>
    private void SetCharImageToPreset(int presetIndex)
    {
        if (!syncCharImageWithCamera || presetIndex >= presetCount || characterImageTransform == null)
            return;

        CharImagePreset preset = charImagePresets[presetIndex];
        if (preset == null) return;

        characterImageTransform.localPosition = preset.position;
        characterImageTransform.localEulerAngles = preset.rotation;
        characterImageTransform.localScale = preset.scale;
        characterImageTransform.sizeDelta = preset.size;

        if (characterRawImage != null)
        {
            Color color = preset.color;
            color.a = preset.alpha;
            characterRawImage.color = color;
        }

        Debug.Log($"设置CharImage到预设{presetIndex}: 位置={preset.position}, Z={preset.position.z}");
    }

    /// <summary>
    /// 直接设置UI状态（无过渡）
    /// </summary>
    private void SetUIToPreset(int presetIndex)
    {
        if (presetIndex >= presetCount) return;

        CameraPreset preset = cameraPresets[presetIndex];
        if (preset == null || preset.uiModuleStates == null) return;

        for (int i = 0; i < uiModules.Length; i++)
        {
            if (uiModules[i] == null) continue;

            bool shouldShow = preset.uiModuleStates[i];

            if (shouldShow)
            {
                uiModules[i].alpha = 1;
                uiModules[i].gameObject.SetActive(true);
                uiModules[i].interactable = true;
                uiModules[i].blocksRaycasts = true;
            }
            else
            {
                uiModules[i].alpha = 0;
                uiModules[i].gameObject.SetActive(false);
                uiModules[i].interactable = false;
                uiModules[i].blocksRaycasts = false;
            }

            currentUIStates[i] = shouldShow;
        }

        Debug.Log($"设置UI到预设{presetIndex}");
    }

    // ========== 保存方法 ==========

    /// <summary>
    /// 保存当前UI状态到指定预设
    /// </summary>
    public void SaveCurrentUIStateToPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= presetCount || cameraPresets[presetIndex] == null)
            return;

        if (cameraPresets[presetIndex].uiModuleStates == null ||
            cameraPresets[presetIndex].uiModuleStates.Length != uiModules.Length)
        {
            cameraPresets[presetIndex].uiModuleStates = new bool[uiModules.Length];
        }

        for (int i = 0; i < uiModules.Length; i++)
        {
            if (uiModules[i] != null)
            {
                cameraPresets[presetIndex].uiModuleStates[i] = currentUIStates[i];
            }
        }

        Debug.Log($"已保存当前UI状态到预设{presetIndex}");
    }

    /// <summary>
    /// 保存当前摄像头状态到指定索引
    /// </summary>
    public void SaveCurrentCameraToPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= presetCount ||
            characterCameraTransform == null)
            return;

        if (presetIndex >= cameraPresets.Length || cameraPresets[presetIndex] == null)
            cameraPresets[presetIndex] = new CameraPreset();

        cameraPresets[presetIndex].cameraPosition = characterCameraTransform.position;
        cameraPresets[presetIndex].cameraRotation = characterCameraTransform.eulerAngles;

        Debug.Log($"已保存摄像头状态到预设 {presetIndex}");
    }

    /// <summary>
    /// 保存当前角色旋转状态到指定索引
    /// </summary>
    public void SaveCurrentPlayerRotationToPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= presetCount ||
            playerModelTransform == null)
            return;

        if (presetIndex >= cameraPresets.Length || cameraPresets[presetIndex] == null)
            cameraPresets[presetIndex] = new CameraPreset();

        cameraPresets[presetIndex].playerRotation = playerModelTransform.eulerAngles;

        Debug.Log($"已保存角色旋转状态到预设 {presetIndex}: {playerModelTransform.eulerAngles}");
    }

    /// <summary>
    /// 保存当前CharImage状态到指定索引
    /// </summary>
    public void SaveCurrentCharImageToPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= presetCount ||
            characterImageTransform == null || characterRawImage == null)
            return;

        if (presetIndex >= charImagePresets.Length || charImagePresets[presetIndex] == null)
            charImagePresets[presetIndex] = new CharImagePreset();

        charImagePresets[presetIndex].position = characterImageTransform.localPosition;
        charImagePresets[presetIndex].rotation = characterImageTransform.localEulerAngles;
        charImagePresets[presetIndex].scale = characterImageTransform.localScale;
        charImagePresets[presetIndex].size = characterImageTransform.sizeDelta;
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
        SaveCurrentPlayerRotationToPreset(presetIndex);
        SaveCurrentCharImageToPreset(presetIndex);
        SaveCurrentUIStateToPreset(presetIndex);

        Debug.Log($"已保存所有状态到预设 {presetIndex}");
    }

    // ========== 其他工具方法 ==========

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
            SetPlayerRotationToPreset(0);
            SetCharImageToPreset(0);
            SetUIToPreset(0);
            currentPresetIndex = 0;

            Debug.Log("已重置到第一个预设");
        }
    }

    /// <summary>
    /// 快速切换到下一个预设
    /// </summary>
    public void SwitchToNextPreset()
    {
        int nextIndex = (currentPresetIndex + 1) % presetCount;
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
        int prevIndex = (currentPresetIndex - 1 + presetCount) % presetCount;
        if (prevIndex != currentPresetIndex)
        {
            SwitchToPreset(prevIndex);
        }
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
    /// 切换角色旋转同步功能
    /// </summary>
    public void TogglePlayerRotationSync(bool enable)
    {
        syncPlayerRotation = enable;
        Debug.Log($"角色旋转同步: {(enable ? "启用" : "禁用")}");
    }

    /// <summary>
    /// 检查是否可以切换到指定预设
    /// </summary>
    public bool CanSwitchToPreset(int presetIndex)
    {
        if (presetIndex == currentPresetIndex)
            return false;

        if (isTransitioning)
            return false;

        if (presetIndex < 0 || presetIndex >= presetCount)
            return false;

        if (presetIndex >= cameraPresets.Length || cameraPresets[presetIndex] == null)
            return false;

        return true;
    }

    /// <summary>
    /// 获取当前预设名称
    /// </summary>
    public string GetCurrentPresetName()
    {
        if (currentPresetIndex < cameraPresets.Length && cameraPresets[currentPresetIndex] != null)
        {
            return cameraPresets[currentPresetIndex].presetName;
        }
        return "未知";
    }

    /// <summary>
    /// 获取预设数量
    /// </summary>
    public int GetPresetCount()
    {
        return presetCount;
    }

    /// <summary>
    /// 获取当前预设索引
    /// </summary>
    public int GetCurrentPresetIndex()
    {
        return currentPresetIndex;
    }

    void OnDestroy()
    {
        if (currentTransitionSequence != null && currentTransitionSequence.IsActive())
        {
            currentTransitionSequence.Kill();
        }
    }

    // ========== 编辑器扩展 ==========
#if UNITY_EDITOR
    [ContextMenu("根据预设数量调整数组")]
    void AdjustArraysByPresetCountInEditor()
    {
        AdjustArraysByPresetCount();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"已根据预设数量({presetCount})调整数组");
    }

    [ContextMenu("初始化所有数组")]
    void InitializeAllArraysInEditor()
    {
        AdjustArraysByPresetCount();
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

    [ContextMenu("保存当前角色旋转到所有预设")]
    void SaveCurrentPlayerRotationToAllPresetsInEditor()
    {
        if (playerModelTransform == null)
        {
            Debug.LogError("角色模型Transform为空！");
            return;
        }

        for (int i = 0; i < presetCount && i < cameraPresets.Length; i++)
        {
            if (cameraPresets[i] == null)
                cameraPresets[i] = new CameraPreset();

            cameraPresets[i].playerRotation = playerModelTransform.eulerAngles;
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"已将当前角色旋转保存到所有{presetCount}个预设");
    }

    [ContextMenu("同步所有预设的UI数组长度")]
    void SyncAllPresetUIArraysInEditor()
    {
        for (int i = 0; i < presetCount && i < cameraPresets.Length; i++)
        {
            if (cameraPresets[i] != null)
            {
                if (cameraPresets[i].uiModuleStates == null ||
                    cameraPresets[i].uiModuleStates.Length != uiModules.Length)
                {
                    System.Array.Resize(ref cameraPresets[i].uiModuleStates, uiModules.Length);
                    Debug.Log($"已调整预设{i}的UI数组长度");
                }
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}