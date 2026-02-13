using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MenuSection
{
    public Button largeButton;          // 大栏位按钮（如“角色”）
    public GameObject panel;             // 大栏位对应的右侧面板（如果有）
    public Button[] smallButtons;        // 该大栏位下的小栏位按钮（如“角色1”“角色2”）
}
public class GachaController : MonoBehaviour
{    // ================== 依赖注入 ==================
    [Header("View引用")]
    [SerializeField] private GachaView viewGacha;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    [Header("菜单栏")]
    [SerializeField] private MenuSection[] menuSections;
    private int largeIndex = 0, smallIndex = 0;

    [Header("颜色方案")]
    [SerializeField] private Color largeSelectedColor = Color.white;   // 选中大按钮颜色 A
    [SerializeField] private Color largeNormalColor = Color.gray;      // 未选中大按钮颜色 B
    [SerializeField] private Color smallSelectedColor = Color.yellow;  // 选中小按钮颜色 C
    [SerializeField] private Color smallNormalColor = Color.white;     // 未选中小按钮颜色 D

    // ================== 数据 ==================
    private PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        // 初始化
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void Initialize()
    {
        // 加载数据
        LoadData();
        // 初始化UI
        InitializeUI();
    }

    void LoadData()
    {
        // 获取玩家数据
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.CurrentPlayerData;
        }
        else
        {
            // 测试数据
            playerData = new PlayerData("测试玩家");
        }
    }

    void InitializeUI()
    {
        // 为所有按钮绑定事件
        for (int i = 0; i < menuSections.Length; i++)
        {
            int largeIndex = i; // 闭包捕获，避免所有按钮都使用最后一个索引
            MenuSection section = menuSections[i];

            // 大栏位按钮点击事件
            section.largeButton.onClick.AddListener(() => OnLargeButtonClick(largeIndex));

            // 小栏位按钮点击事件
            for (int j = 0; j < section.smallButtons.Length; j++)
            {
                int smallIndex = j;
                section.smallButtons[j].onClick.AddListener(() => OnSmallButtonClick(largeIndex, smallIndex));
            }
        }

        // 初始化默认选中第一个大栏位
        if (menuSections.Length > 0)
        {
            OnLargeButtonClick(0);
        }
        viewGacha.UpdatePlayerResources(playerData);
    }

    /// <summary>
    /// 大栏位点击处理：切换展开/折叠，并自动选中第一个小栏位
    /// </summary>
    private void OnLargeButtonClick(int largeIndex)
    {
        // 1. 更新所有大栏位颜色 + 控制小栏位显隐
        for (int i = 0; i < menuSections.Length; i++)
        {
            bool isCurrent = (i == largeIndex);
            SetLargeButtonAppearance(i, isCurrent);

            // 只有当前大栏位的小按钮显示，其余隐藏
            SetSmallButtonsActive(i, isCurrent);
        }

        // 2. 自动选中当前大栏位的第一个小按钮（如果存在）
        if (menuSections[largeIndex].smallButtons.Length > 0)
        {
            SetSmallButtonHighlight(largeIndex, 0);
        }
    }

    /// <summary>
    /// 小栏位点击处理：切换小按钮高亮
    /// </summary>
    private void OnSmallButtonClick(int largeIndex, int smallIndex)
    {
        SetSmallButtonHighlight(largeIndex, smallIndex);
        // 这里可以扩展：例如切换右侧面板内容
        // 例如：menuSections[largeIndex].panel.SetActive(true);
    }

    /// <summary>
    /// 设置大栏位按钮的外观（颜色）
    /// </summary>
    private void SetLargeButtonAppearance(int largeIndex, bool isSelected)
    {
        Button btn = menuSections[largeIndex].largeButton;
        if (btn.targetGraphic != null)
        {
            btn.targetGraphic.color = isSelected ? largeSelectedColor : largeNormalColor;
        }
    }

    /// <summary>
    /// 控制某个大栏位下所有小栏位的显隐
    /// </summary>
    private void SetSmallButtonsActive(int largeIndex, bool active)
    {
        foreach (Button btn in menuSections[largeIndex].smallButtons)
        {
            btn.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 设置某个大栏位下小按钮的高亮（当前选中的为 selectedColor，其余为 normalColor）
    /// </summary>
    private void SetSmallButtonHighlight(int largeIndex, int smallIndex)
    {
        MenuSection section = menuSections[largeIndex];
        for (int i = 0; i < section.smallButtons.Length; i++)
        {
            Button btn = section.smallButtons[i];
            if (btn.targetGraphic != null)
            {
                btn.targetGraphic.color = (i == smallIndex) ? smallSelectedColor : smallNormalColor;
            }
        }
    }
}
