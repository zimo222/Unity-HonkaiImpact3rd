using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

// 重新定义 MenuSection，增加 smallButtonPools 数组
[System.Serializable]
public class MenuSection
{
    public Button largeButton;                 // 大栏位按钮
    public GameObject panel;                    // 大栏位对应的右侧面板（如果有）
    public Button[] smallButtons;               // 小栏位按钮数组
    public GachaPoolSO[] smallButtonPools;      // 与小按钮一一对应的卡池数据（长度应与 smallButtons 一致）
}

public class GachaController : MonoBehaviour
{
    // ================== 依赖注入 ==================
    [Header("View引用")]
    [SerializeField] private GachaView viewGacha;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    public ModularUIButton[] referencedButtons;

    [Header("菜单栏")]
    [SerializeField] private MenuSection[] menuSections;
    private int largeIndex = -1, smallIndex = -1;

    [Header("颜色方案")]
    [SerializeField] private Color largeSelectedColor = Color.white;   // 选中大按钮颜色 A
    [SerializeField] private Color largeNormalColor = Color.gray;      // 未选中大按钮颜色 B
    [SerializeField] private Color smallSelectedColor = Color.yellow;  // 选中小按钮颜色 C
    [SerializeField] private Color smallNormalColor = Color.white;     // 未选中小按钮颜色 D

    [Header("抽卡设置")]
    [SerializeField] private Button gachaButton;          // 抽卡按钮（需在Inspector中拖拽）
    [SerializeField] private Button gachaTenButton;       // 新增：十连按钮
    private GachaManager gachaManager;                     // 引用单例，无需手动赋值

    // ================== 数据 ==================
    private PlayerData playerData;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        LoadData();
        InitializeUI();
    }

    void LoadData()
    {
        if (PlayerDataManager.Instance != null)
        {
            playerData = PlayerDataManager.Instance.CurrentPlayerData;
        }
        else
        {
            playerData = new PlayerData("测试玩家");
        }
    }

    void InitializeUI()
    {
        // 1. 先获取 GachaManager 单例
        gachaManager = GachaManager.Instance;
        if (gachaManager == null)
            Debug.LogError("场景中缺少GachaManager，请挂载GachaManager脚本");

        // 2. 绑定按钮事件
        for (int i = 0; i < menuSections.Length; i++)
        {
            int largeIdx = i;
            MenuSection section = menuSections[i];

            section.largeButton.onClick.AddListener(() => OnLargeButtonClick(largeIdx));

            for (int j = 0; j < section.smallButtons.Length; j++)
            {
                int smallIdx = j;
                section.smallButtons[j].onClick.AddListener(() => OnSmallButtonClick(largeIdx, smallIdx));
            }
        }

        // 3. 绑定抽卡按钮
        if (gachaButton != null)
            gachaButton.onClick.AddListener(OnGachaButtonClick);

        // 绑定十连按钮
        if (gachaTenButton != null)
            gachaTenButton.onClick.AddListener(OnGachaTenButtonClick);

        // 4. 初始化默认选中第一个大栏位
        if (menuSections.Length > 0)
        {
            OnLargeButtonClick(0); // 内部会调用 OnSmallButtonClick，此时 gachaManager 已就绪
                                   // 更新卡池名称显示
            if (menuSections[0].smallButtonPools != null && menuSections[0].smallButtonPools.Length > 0)
            {
                var firstPool = menuSections[0].smallButtonPools[0];
                if (firstPool != null)
                    viewGacha.UpdateCurrentPoolName(firstPool.poolName);
            }
        }

        // 5. 更新玩家资源
        viewGacha.UpdatePlayerResources(playerData);
    }

    private void OnLargeButtonClick(int LargeIndex)
    {
        if (largeIndex == LargeIndex)
            return;

        largeIndex = LargeIndex;
        smallIndex = 0;

        // 1. 更新所有大栏位颜色 + 控制小栏位显隐
        for (int i = 0; i < menuSections.Length; i++)
        {
            bool isCurrent = (i == largeIndex);
            SetLargeButtonAppearance(i, isCurrent);
            SetSmallButtonsActive(i, isCurrent);
        }

        // 2. 自动选中当前大栏位的第一个小按钮
        if (menuSections[largeIndex].smallButtons.Length > 0)
        {
            // 手动触发小栏位点击事件，加载卡池并高亮
            OnSmallButtonClick(largeIndex, 0);
        }
    }

    private void OnSmallButtonClick(int largeIdx, int smallIdx)
    {
        SetSmallButtonHighlight(largeIdx, smallIdx);

        // === 更新卡池信息 ===
        if (menuSections[largeIdx].smallButtonPools != null && menuSections[largeIdx].smallButtonPools.Length > smallIdx)
        {
            Debug.Log(largeIdx.ToString() + "," + smallIdx.ToString());
            var pool = menuSections[largeIdx].smallButtonPools[smallIdx];
            if (pool != null && gachaManager != null)
            {
                gachaManager.LoadPool(pool);
                viewGacha.UpdateCurrentPoolName(pool.poolName);
            }
        }

        // 可选：切换右侧面板
        // menuSections[largeIdx].panel.SetActive(true);
    }

    // 辅助方法：根据物品ID获取名称和星级
    private void GetItemInfo(string id, out string name, out int star)
    {
        name = "未知";
        star = 0;
        if (string.IsNullOrEmpty(id)) return;

        GameDataManager dataManager = GameDataManager.Instance;
        dataManager.CharacterDict.TryGetValue(id, out CharacterDefineSO character);
        if (character != null)
        {
            name = character.characterName;
            star = character.baseStars;
            return;
        }
        dataManager.WeaponDict.TryGetValue(id, out WeaponDefineSO weapon);
        if (weapon != null)
        {
            name = weapon.weaponName;
            star = weapon.baseStars;
            return;
        }
        dataManager.StigmataDict.TryGetValue(id, out StigmataDefineSO stigmata);
        if (stigmata != null)
        {
            name = stigmata.stigmataName;
            star = stigmata.baseStars;
            return;
        }
    }

    // 修改单抽方法，使用辅助方法
    private void OnGachaButtonClick()
    {
        if (gachaManager == null) return;

        // 确保卡池已加载
        if (gachaManager.GetCurrentPool() == null && largeIndex >= 0 && smallIndex >= 0)
        {
            var pool = menuSections[largeIndex].smallButtonPools[smallIndex];
            if (pool != null)
                gachaManager.LoadPool(pool);
        }

        string itemId = gachaManager.PerformGacha();
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("抽卡失败，可能未加载卡池");
            return;
        }

        GetItemInfo(itemId, out string itemName, out int star);
        viewGacha.ShowGachaResult(itemName, star);
        UpdatePityDisplay();
    }
    // 新增：十连抽方法
    private void OnGachaTenButtonClick()
    {
        if (gachaManager == null) return;

        // 确保卡池已加载
        if (gachaManager.GetCurrentPool() == null && largeIndex >= 0 && smallIndex >= 0)
        {
            var pool = menuSections[largeIndex].smallButtonPools[smallIndex];
            if (pool != null)
                gachaManager.LoadPool(pool);
        }

        // 执行十连抽，存储结果
        string[] itemIds = new string[10];
        for (int i = 0; i < 10; i++)
        {
            itemIds[i] = gachaManager.PerformGacha();
        }

        // 转换为显示信息
        string[] itemNames = new string[10];
        int[] stars = new int[10];
        for (int i = 0; i < 10; i++)
        {
            GetItemInfo(itemIds[i], out itemNames[i], out stars[i]);
        }

        // 更新保底显示
        UpdatePityDisplay();

        // 显示十连结果
        viewGacha.ShowGachaTenResult(itemNames, stars);
    }

    private void UpdatePityDisplay()
    {
        if (gachaManager == null) return;
        viewGacha.UpdatePityDisplay(
            gachaManager.PullsSinceLastFourStar,
            gachaManager.PullsSinceLastFiveStar,
            gachaManager.IsGuaranteedFourStarNext,
            gachaManager.IsGuaranteedFiveStarNext
        );
    }

    // 公开接口：修改抽卡概率（可由设置面板调用）
    public void SetGachaProbability(float threeStar, float fourStar, float fiveStar)
    {
        if (gachaManager == null) return;
        gachaManager.SetStarProbability(threeStar, fourStar, fiveStar);
    }

    // ---------- 辅助函数 ----------
    private void SetLargeButtonAppearance(int largeIdx, bool isSelected)
    {
        Button btn = menuSections[largeIdx].largeButton;
        if (btn.targetGraphic != null)
            btn.targetGraphic.color = isSelected ? largeSelectedColor : largeNormalColor;
    }

    private void SetSmallButtonsActive(int largeIdx, bool active)
    {
        foreach (Button btn in menuSections[largeIdx].smallButtons)
            btn.gameObject.SetActive(active);
    }

    private void SetSmallButtonHighlight(int largeIdx, int smallIdx)
    {
        MenuSection section = menuSections[largeIdx];
        for (int i = 0; i < section.smallButtons.Length; i++)
        {
            Button btn = section.smallButtons[i];
            if (btn.targetGraphic != null)
                btn.targetGraphic.color = (i == smallIdx) ? smallSelectedColor : smallNormalColor;
        }
    }
}