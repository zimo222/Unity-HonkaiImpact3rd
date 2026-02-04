using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 模块化UI按钮控制器。
/// 将此脚本附加到任何Button上，即可在Inspector中配置其点击行为。
/// </summary>
[RequireComponent(typeof(Button))]
public class ModularUIButton : MonoBehaviour
{
    public enum ButtonAction
    {
        None = 0,
        OpenPanel = 1,
        SwitchScene = 2,
        CustomEvent = 3
    }

    [Header("基础标识")]
    [Tooltip("按钮的功能名称，仅用于标识和调试")]
    public string buttonName = "功能按钮";

    [Header("核心动作配置")]
    [Tooltip("选择此按钮触发何种类型的操作")]
    public ButtonAction actionType = ButtonAction.None;

    [Header("打开面板设置 (当动作类型为OpenPanel时生效)")]
    [Tooltip("需要打开的面板对象。拖拽Hierarchy中的Panel至此。")]
    public GameObject panelToOpen;
    [Tooltip("打开新面板时，是否自动关闭其他由本类管理的面板？")]
    public bool closeOtherPanelsOnOpen = false;
    [Tooltip("面板打开/关闭的淡入淡出时间（秒）")]
    [Range(0f, 2f)] public float fadeDuration = 0.3f;

    [Header("切换场景设置 (当动作类型为SwitchScene时生效)")]
    [Tooltip("需要加载的场景名称。确保场景已在Build Settings中添加。")]
    public string targetSceneName;
    [Tooltip("场景加载模式：Single(替换当前) 或 Additive(叠加)")]
    public LoadSceneMode sceneLoadMode = LoadSceneMode.Single;

    [Header("自定义事件 (当动作类型为CustomEvent时生效)")]
    [Tooltip("点击按钮时将触发的自定义事件，可拖拽函数至此。")]
    public UnityEvent onCustomEvent;

    [Header("按钮状态")]
    [Tooltip("是否需要在执行前显示确认弹窗？")]
    public bool requireConfirmation = false;
    [Tooltip("确认弹窗上显示的提示文本")]
    [TextArea] public string confirmationMessage = "确定要执行此操作吗？";

    // 内部组件引用
    private Button _button;
    private CanvasGroup _panelCanvasGroup; // 缓存Panel的CanvasGroup

    /// <summary>
    /// 静态面板管理器：记录所有由ModularUIButton管理的面板，用于“关闭其他”功能。
    /// </summary>
    private static System.Collections.Generic.List<GameObject> s_ManagedPanels = new System.Collections.Generic.List<GameObject>();

    void Awake()
    {
        _button = GetComponent<Button>();
        if (_button == null)
        {
            Debug.LogError($"ModularUIButton 需要附加在带有Button组件的物体上。", this);
            return;
        }

        // 为关联的面板初始化CanvasGroup（如果存在）
        if (panelToOpen != null)
        {
            _panelCanvasGroup = panelToOpen.GetComponent<CanvasGroup>();
            if (_panelCanvasGroup == null && panelToOpen.activeInHierarchy)
            {
                // 如果面板当前是激活的，我们才自动添加，否则可能由其他逻辑控制
                // _panelCanvasGroup = panelToOpen.AddComponent<CanvasGroup>();
            }
            // 登记管理面板
            if (!s_ManagedPanels.Contains(panelToOpen))
            {
                s_ManagedPanels.Add(panelToOpen);
            }
        }
    }

    void OnEnable()
    {
        // 订阅按钮点击事件
        _button.onClick.AddListener(OnButtonClick);
    }

    void OnDisable()
    {
        // 取消订阅，防止内存泄漏
        _button.onClick.RemoveListener(OnButtonClick);
    }

    void OnDestroy()
    {
        // 从静态管理列表中移除
        if (panelToOpen != null)
        {
            s_ManagedPanels.Remove(panelToOpen);
        }
    }

    /// <summary>
    /// 按钮点击事件触发的入口函数。
    /// </summary>
    private void OnButtonClick()
    {
        Debug.Log($"按钮 '{buttonName}' 被点击，动作类型: {actionType}");

        if (requireConfirmation)
        {
            // 这里应该调用你自己的确认窗口管理器，例如：
            // ConfirmationPopup.Show(confirmationMessage, ExecuteAction);
            // 为简单起见，这里直接执行
            Debug.LogWarning($"需要确认: {confirmationMessage}");
            ExecuteAction();
        }
        else
        {
            ExecuteAction();
        }
    }

    /// <summary>
    /// 通用执行函数：根据配置执行对应的操作。
    /// </summary>
    private void ExecuteAction()
    {
        switch (actionType)
        {
            case ButtonAction.None:
                Debug.Log($"按钮 '{buttonName}' 未配置任何动作。");
                break;

            case ButtonAction.OpenPanel:
                if (panelToOpen != null)
                {
                    OpenTargetPanel();
                }
                else
                {
                    Debug.LogError($"按钮 '{buttonName}' 配置为打开面板，但未指定Panel对象。", this);
                }
                break;

            case ButtonAction.SwitchScene:
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    LoadTargetScene();
                }
                else
                {
                    Debug.LogError($"按钮 '{buttonName}' 配置为切换场景，但未指定场景名称。", this);
                }
                break;

            case ButtonAction.CustomEvent:
                onCustomEvent?.Invoke();
                Debug.Log($"按钮 '{buttonName}' 触发了自定义事件。");
                break;
        }
    }

    /// <summary>
    /// 打开目标面板，支持淡入效果和关闭其他面板。
    /// </summary>
    private void OpenTargetPanel()
    {
        // 如果面板已经打开，则关闭它
        if (panelToOpen.activeSelf)
        {
            StartCoroutine(ClosePanelCoroutine(panelToOpen, fadeDuration));
            return;
        }

        // 关闭其他被管理的面板
        if (closeOtherPanelsOnOpen)
        {
            foreach (var panel in s_ManagedPanels)
            {
                if (panel != null && panel != panelToOpen && panel.activeSelf)
                {
                    StartCoroutine(ClosePanelCoroutine(panel, fadeDuration));
                }
            }
        }

        // 确保CanvasGroup存在以支持淡入
        if (_panelCanvasGroup == null)
        {
            _panelCanvasGroup = panelToOpen.GetComponent<CanvasGroup>();
        }
        if (_panelCanvasGroup == null)
        {
            _panelCanvasGroup = panelToOpen.AddComponent<CanvasGroup>();
        }

        // 激活并淡入
        panelToOpen.SetActive(true);
        StartCoroutine(FadePanelCoroutine(panelToOpen, 0f, 1f, fadeDuration));

        Debug.Log($"按钮 '{buttonName}' 打开了面板: {panelToOpen.name}");
    }

    /// <summary>
    /// 加载目标场景。
    /// </summary>
    private void LoadTargetScene()
    {
        Debug.Log($"按钮 '{buttonName}' 正在加载场景: {targetSceneName}");

        // 在实际项目中，这里可能先保存游戏数据
        // SaveSystem.SaveGame();

        if (targetSceneName == "LastScene")
            targetSceneName = PlayerPrefs.GetString("LastScene");
            Debug.Log(targetSceneName);

        SceneManager.LoadScene(targetSceneName, sceneLoadMode);
    }

    // ================== 协程：面板淡入淡出效果 ==================
    private IEnumerator FadePanelCoroutine(GameObject panel, float startAlpha, float endAlpha, float duration)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        cg.alpha = startAlpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
    }

    private IEnumerator ClosePanelCoroutine(GameObject panel, float duration)
    {
        yield return FadePanelCoroutine(panel, panel.GetComponent<CanvasGroup>().alpha, 0f, duration);
        panel.SetActive(false);
        Debug.Log($"关闭了面板: {panel.name}");
    }

    // ================== 公开方法（供其他脚本调用） ==================
    /// <summary>
    /// 外部手动触发此按钮的动作（模拟点击）。
    /// </summary>
    public void TriggerButtonAction()
    {
        OnButtonClick();
    }

    /// <summary>
    /// 动态更改此按钮的动作类型。
    /// </summary>
    public void SetActionType(ButtonAction newActionType)
    {
        actionType = newActionType;
    }

    /// <summary>
    /// 动态设置要打开的面板。
    /// </summary>
    public void SetPanelToOpen(GameObject newPanel)
    {
        // 从旧面板解除登记
        if (panelToOpen != null)
        {
            s_ManagedPanels.Remove(panelToOpen);
        }

        panelToOpen = newPanel;
        _panelCanvasGroup = newPanel?.GetComponent<CanvasGroup>();

        // 登记新面板
        if (newPanel != null && !s_ManagedPanels.Contains(newPanel))
        {
            s_ManagedPanels.Add(newPanel);
        }
    }
}