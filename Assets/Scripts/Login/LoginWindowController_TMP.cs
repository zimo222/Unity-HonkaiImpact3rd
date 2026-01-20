using System.Collections.Generic;
// 关键1：引入 TMPro 命名空间
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginWindowController_TMP : MonoBehaviour
{
    // UI元素引用 - 所有 InputField 和 Text 都改为 TMP 类型
    public GameObject loginWindow;

    // 关键2：将类型从 InputField 改为 TMP_InputField
    public TMP_InputField usernameInputField_TMP;
    public TMP_InputField passwordInputField_TMP;

    // 如果你有用于显示提示信息的Text，也需要改为TMP_Text
    public TMP_Text statusText_TMP; // 可选：用于显示登录状态

    // 按钮仍然是UnityEngine.UI.Button类型，保持不变
    public UnityEngine.UI.Button loginButton;
    public UnityEngine.UI.Button registerButton;
    public UnityEngine.UI.Button closeButton;

    // 对玩家数据管理器的引用
    public PlayerDataManager dataManager;

    void Start()
    {
        // 确保游戏开始时窗口是关闭的
        if (loginWindow != null)
        {
            loginWindow.SetActive(false);
        }

        // 绑定按钮点击事件
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterButtonClicked);
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);

        // 可选：为输入框添加结束编辑监听（比如按回车键触发登录）
        if (usernameInputField_TMP != null)
        {
            usernameInputField_TMP.onSubmit.AddListener(delegate { OnInputFieldSubmit(); });
        }
        if (passwordInputField_TMP != null)
        {
            passwordInputField_TMP.onSubmit.AddListener(delegate { OnInputFieldSubmit(); });
        }

        // 可选：初始状态信息
        if (statusText_TMP != null)
        {
            statusText_TMP.text = "点击屏幕开始游戏";
            statusText_TMP.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // 检测鼠标点击（非UI元素部分）来打开窗口
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
        {
            if (loginWindow != null && !loginWindow.activeSelf)
            {
                OpenLoginWindow();
            }
        }
    }

    // 打开登录窗口
    public void OpenLoginWindow()
    {
        if (loginWindow != null)
        {
            loginWindow.SetActive(true);
            // 清空输入框
            if (usernameInputField_TMP != null)
            {
                usernameInputField_TMP.text = "";
                // 自动聚焦到用户名输入框
                usernameInputField_TMP.Select();
                usernameInputField_TMP.ActivateInputField();
            }
            if (passwordInputField_TMP != null)
            {
                passwordInputField_TMP.text = "";
            }
            // 清除状态信息
            if (statusText_TMP != null)
            {
                statusText_TMP.text = "";
            }
        }
    }

    // 关闭登录窗口
    public void CloseLoginWindow()
    {
        if (loginWindow != null)
        {
            loginWindow.SetActive(false);
            // 关闭窗口后可以显示一些状态信息
            if (statusText_TMP != null && dataManager != null && dataManager.CurrentPlayerData != null)
            {
                statusText_TMP.text = $"欢迎，{dataManager.CurrentPlayerData.PlayerName}！";
            }
        }
    }

    // 登录按钮逻辑
    private void OnLoginButtonClicked()
    {
        AttemptLogin();
    }

    // 注册按钮逻辑
    private void OnRegisterButtonClicked()
    {
        AttemptRegister();
    }

    // 关闭按钮逻辑
    private void OnCloseButtonClicked()
    {
        CloseLoginWindow();
    }

    // 输入框提交（按回车键）的逻辑
    private void OnInputFieldSubmit()
    {
        AttemptLogin();
    }

    // 尝试登录的公共方法
    private void AttemptLogin()
    {
        if (usernameInputField_TMP == null || passwordInputField_TMP == null)
        {
            Debug.LogError("TMP_InputField 未正确赋值！");
            return;
        }

        string username = usernameInputField_TMP.text;
        string password = passwordInputField_TMP.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatusMessage("用户名或密码不能为空！", Color.yellow);
            return;
        }

        // 调用数据管理器验证登录
        bool success = dataManager.TryLogin(username, password);

        // ... 验证成功 ...
        if (success)
        {
            ShowStatusMessage("登录成功！", Color.green);

            // 调用过渡
            LoginTransitionSimple transition = FindObjectOfType<LoginTransitionSimple>();
            if (transition != null)
                transition.OnLoginSuccess();
            else
                SceneManager.LoadScene("HomeScene");
        }
        else
        {
            ShowStatusMessage("登录失败！用户名或密码错误。", Color.red);
            // 清空密码框，让用户重新输入
            passwordInputField_TMP.text = "";
            passwordInputField_TMP.Select();
            passwordInputField_TMP.ActivateInputField();
        }
    }
        

    // 尝试注册的公共方法
    private void AttemptRegister()
    {
        if (usernameInputField_TMP == null || passwordInputField_TMP == null)
        {
            Debug.LogError("TMP_InputField 未正确赋值！");
            return;
        }

        string username = usernameInputField_TMP.text;
        string password = passwordInputField_TMP.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatusMessage("用户名或密码不能为空！", Color.yellow);
            return;
        }

        if (password.Length < 6)
        {
            ShowStatusMessage("密码长度至少为6位！", Color.yellow);
            return;
        }

        // 调用数据管理器尝试注册
        bool success = dataManager.TryRegister(username, password);
        if (success)
        {
            ShowStatusMessage($"注册成功！已创建用户：{username}", Color.green);
            // 注册成功后自动尝试登录
            bool loginSuccess = dataManager.TryLogin(username, password);
            if (loginSuccess)
            {
                ShowStatusMessage($"已自动登录，欢迎！", Color.green);
                Invoke(nameof(CloseLoginWindow), 1.5f);
            }
        }
        else
        {
            ShowStatusMessage("注册失败！用户名可能已存在。", Color.red);
        }
    }

    // 显示状态信息
    private void ShowStatusMessage(string message, Color color)
    {
        if (statusText_TMP != null)
        {
            statusText_TMP.text = message;
            statusText_TMP.color = color;
            statusText_TMP.gameObject.SetActive(true);
        }
        Debug.Log(message);
    }

    // 辅助方法：检测鼠标是否在UI元素上
    private bool IsPointerOverUIElement()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
            return false;

        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    // 提供外部调用的重置方法
    public void ResetLoginForm()
    {
        if (usernameInputField_TMP != null) usernameInputField_TMP.text = "";
        if (passwordInputField_TMP != null) passwordInputField_TMP.text = "";
        if (statusText_TMP != null)
        {
            statusText_TMP.text = "";
            statusText_TMP.gameObject.SetActive(false);
        }
    }
}