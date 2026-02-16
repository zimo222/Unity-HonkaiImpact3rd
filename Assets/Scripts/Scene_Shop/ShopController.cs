using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public class StoreMenuSection
{
    public Button largeButton;
    public GameObject panel;
    public Button[] smallButtons;
    public ShopPoolSO[] smallButtonPools;
}
public class ShopController : MonoBehaviour
{
    [Header("View引用")]
    [SerializeField] private ShopView viewShop;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    [Header("菜单栏")]
    [SerializeField] private StoreMenuSection[] menuSections;
    private int largeIndex = -1, smallIndex = -1;

    [Header("颜色方案")]
    [SerializeField] private Color largeSelectedColor = Color.white;
    [SerializeField] private Color largeNormalColor = Color.gray;
    [SerializeField] private Color smallSelectedColor = Color.yellow;
    [SerializeField] private Color smallNormalColor = Color.white;

    [SerializeField] private Color largeSelectedTextColor = Color.white;
    [SerializeField] private Color smallSelectedTextColor = Color.white;
    [SerializeField] private Color normalTextColor = Color.white;

    [Header("商店设置")]
    private ShopManager shopManager;

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
            playerData = PlayerDataManager.Instance.CurrentPlayerData;
        else
            playerData = new PlayerData("测试玩家");
    }

    void InitializeUI()
    {
        shopManager = ShopManager.Instance;
        if (shopManager == null)
            Debug.LogError("场景中缺少ShopManager，请挂载ShopManager脚本");
        
        for (int i = 0; i < menuSections.Length; i++)
        {
            int largeIdx = i;
            StoreMenuSection section = menuSections[i];
            section.largeButton.onClick.AddListener(() => OnLargeButtonClick(largeIdx));
            for (int j = 0; j < section.smallButtons.Length; j++)
            {
                int smallIdx = j;
                section.smallButtons[j].onClick.AddListener(() => OnSmallButtonClick(largeIdx, smallIdx));
            }
        }

        if (menuSections.Length > 0)
        {
            OnLargeButtonClick(0);
            if (menuSections[0].smallButtonPools != null && menuSections[0].smallButtonPools.Length > 0)
            {
                var firstPool = menuSections[0].smallButtonPools[0];
            }
        }

        viewShop.UpdatePlayerResources(playerData);
        if (viewShop.clickArea != null)
            viewShop.clickArea.onClick.RemoveAllListeners();
    }

    private void OnLargeButtonClick(int LargeIndex)
    {
        if (largeIndex == LargeIndex) return;
        largeIndex = LargeIndex;
        smallIndex = -1;

        for (int i = 0; i < menuSections.Length; i++)
        {
            bool isCurrent = (i == largeIndex);
            SetLargeButtonAppearance(i, isCurrent);
            SetSmallButtonsActive(i, isCurrent);
        }

        if (menuSections[largeIndex].smallButtons.Length > 0)
            OnSmallButtonClick(largeIndex, 0);
    }

    private void OnSmallButtonClick(int largeIdx, int smallIdx)
    {
        if (smallIndex == smallIdx) return;
        smallIndex = smallIdx;
        SetSmallButtonHighlight(largeIdx, smallIdx);
        
        // 检查数组和元素
        if (menuSections[largeIdx].smallButtonPools == null)
        {
            Debug.LogError($"smallButtonPools 数组为 null，largeIdx={largeIdx}");
            return;
        }
        if (menuSections[largeIdx].smallButtonPools.Length <= smallIdx)
        {
            Debug.LogError($"smallButtonPools 数组长度不足：长度={menuSections[largeIdx].smallButtonPools.Length}，索引={smallIdx}");
            return;
        }

        var pool = menuSections[largeIdx].smallButtonPools[smallIdx];
        Debug.Log($"准备加载卡池：pool={(pool != null ? pool.poolName : "null")}, gachaManager={shopManager}");

        if (pool != null && shopManager != null)
        {
            // 加载前打印 currentPool
            var beforePool = shopManager.GetCurrentPool();
            Debug.Log($"加载前 currentPool = {(beforePool != null ? beforePool.poolName : "null")}");

            shopManager.LoadPool(pool);

            // 加载后检查
            var afterPool = shopManager.GetCurrentPool();
            Debug.Log($"加载后 currentPool = {(afterPool != null ? afterPool.poolName : "null")}");

            if (afterPool != null)
                Debug.Log("LoadPool成了");
            else
                Debug.LogError("LoadPool 后 currentPool 仍然为 null！");
        }
        else
        {
            Debug.LogError($"pool 或 gachaManager 为 null: pool={pool}, gachaManager={shopManager}");
        }
    }

    // 修改 GetItemInfo 增加 icon 参数
    private void GetItemInfo(string id, out string name, out int star, out Sprite icon, out Sprite illustration)
    {
        name = "未知";
        star = 0;
        icon = null;
        illustration = null;
        if (string.IsNullOrEmpty(id)) return;

        GameDataManager dataManager = GameDataManager.Instance;
        if (dataManager.CharacterDict.TryGetValue(id, out CharacterDefineSO character))
        {
            name = character.characterName;
            star = character.baseStars + 4;
            icon = character.icon; // 假设 SO 中有 icon 字段
            illustration = character.Illustration;
            return;
        }
        if (dataManager.WeaponDict.TryGetValue(id, out WeaponDefineSO weapon))
        {
            name = weapon.weaponName;
            star = weapon.baseStars + 1;
            icon = weapon.icon;
            illustration = null;
            return;
        }
        if (dataManager.StigmataDict.TryGetValue(id, out StigmataDefineSO stigmata))
        {
            name = stigmata.stigmataName;
            star = stigmata.baseStars;
            icon = stigmata.icon;
            illustration = null;
            return;
        }
    }

    private void SetLargeButtonAppearance(int largeIdx, bool isSelected)
    {
        Button btn = menuSections[largeIdx].largeButton;
        if (btn.targetGraphic != null)
        {
            btn.targetGraphic.color = isSelected ? largeSelectedColor : largeNormalColor;
            TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
            text.color = isSelected ? largeSelectedTextColor : normalTextColor;

        }
    }

    private void SetSmallButtonsActive(int largeIdx, bool active)
    {
        foreach (Button btn in menuSections[largeIdx].smallButtons)
            btn.gameObject.SetActive(active);
    }

    private void SetSmallButtonHighlight(int largeIdx, int smallIdx)
    {
        StoreMenuSection section = menuSections[largeIdx];
        for (int i = 0; i < section.smallButtons.Length; i++)
        {
            Button btn = section.smallButtons[i];
            if (btn.targetGraphic != null)
            {
                btn.targetGraphic.color = (i == smallIdx) ? smallSelectedColor : smallNormalColor;
                TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
                text.color = (i == smallIdx) ? smallSelectedTextColor : normalTextColor;
            }
        }
    }
}