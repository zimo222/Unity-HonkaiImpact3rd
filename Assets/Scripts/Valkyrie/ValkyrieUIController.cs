using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class ValkyrieUIController : MonoBehaviour
{
    // ================== 基础玩家信息UI引用 ==================
    [Header("玩家信息")]
    public TMP_Text playerNameText;
    public TMP_Text playerLevelText;
    public TMP_Text playerExperienceText;
    public Slider experienceSlider;

    [Header("资源信息")]
    public TMP_Text crystalsText;
    public TMP_Text coinsText;

    // ================== 按钮引用 (可选) ==================
    [Header("按钮引用 (如果你需要通过脚本访问它们)")]
    [Tooltip("你可以在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    // ================== 其他原有系统（保持不变） ==================
    // ... [你的Panel填充控制系统、确认窗口等] ...

    private PlayerData currentPlayerData;

    void Start()
    {
        InitializeUI();
        LoadPlayerData();
        // 不再需要初始化按钮，因为每个ModularUIButton会自己管理自己
    }

    void InitializeUI()
    {
        // ... [你的UI初始化代码] ...
    }

    void LoadPlayerData()
    {
        // ... [你的数据加载代码] ...
        UpdateAllUI();
    }

    void UpdateAllUI()
    {
        // ... [你的UI更新代码] ...
    }

    // ================== 示例：如何通过脚本与模块化按钮交互 ==================
    /// <summary>
    /// 示例：动态禁用一个按钮。
    /// </summary>
    public void DisableButtonByName(string targetButtonName)
    {
        foreach (var button in referencedButtons)
        {
            if (button.buttonName == targetButtonName)
            {
                button.GetComponent<Button>().interactable = false;
                Debug.Log($"已禁用按钮: {targetButtonName}");
                break;
            }
        }
    }

    /// <summary>
    /// 示例：动态改变一个按钮的行为。
    /// </summary>
    public void ChangeButtonToOpenDifferentPanel(string targetButtonName, GameObject newPanel)
    {
        foreach (var modularButton in referencedButtons)
        {
            if (modularButton.buttonName == targetButtonName)
            {
                modularButton.SetActionType(ModularUIButton.ButtonAction.OpenPanel);
                modularButton.SetPanelToOpen(newPanel);
                Debug.Log($"已修改按钮 '{targetButtonName}' 行为，将打开新面板: {newPanel.name}");
                break;
            }
        }
    }
}