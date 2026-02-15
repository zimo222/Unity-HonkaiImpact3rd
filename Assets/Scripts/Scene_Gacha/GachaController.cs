using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public class MenuSection
{
    public Button largeButton;
    public GameObject panel;
    public Button[] smallButtons;
    public GachaPoolSO[] smallButtonPools;
}

public class GachaController : MonoBehaviour
{
    [Header("View引用")]
    [SerializeField] private GachaView viewGacha;

    // =========================  按钮引用 (可选)   =========================
    [Header("按钮引用 (如果需要通过脚本访问它们)")]
    [Tooltip("在这里拖拽那些已经附加了ModularUIButton组件的按钮对象，方便通过脚本获取。")]
    public ModularUIButton[] referencedButtons;

    [Header("菜单栏")]
    [SerializeField] private MenuSection[] menuSections;
    private int largeIndex = -1, smallIndex = -1;

    [Header("颜色方案")]
    [SerializeField] private Color largeSelectedColor = Color.white;
    [SerializeField] private Color largeNormalColor = Color.gray;
    [SerializeField] private Color smallSelectedColor = Color.yellow;
    [SerializeField] private Color smallNormalColor = Color.white;

    [Header("抽卡设置")]
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button gachaTenButton;
    private GachaManager gachaManager;

    private List<GachaResultItem> currentResults = new List<GachaResultItem>();
    private int currentShowIndex = 0;
    private GachaState currentState = GachaState.Idle;

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
        gachaManager = GachaManager.Instance;
        if (gachaManager == null)
            Debug.LogError("场景中缺少GachaManager，请挂载GachaManager脚本");

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

        if (gachaButton != null)
            gachaButton.onClick.AddListener(OnGachaButtonClick);
        if (gachaTenButton != null)
            gachaTenButton.onClick.AddListener(OnGachaTenButtonClick);

        if (menuSections.Length > 0)
        {
            OnLargeButtonClick(0);
            if (menuSections[0].smallButtonPools != null && menuSections[0].smallButtonPools.Length > 0)
            {
                var firstPool = menuSections[0].smallButtonPools[0];
                if (firstPool != null)
                    viewGacha.UpdateCurrentPoolName(firstPool.poolName);
            }
        }

        viewGacha.UpdatePlayerResources(playerData);
        if (viewGacha.clickArea != null)
            viewGacha.clickArea.onClick.RemoveAllListeners();
        viewGacha.ShowAnimationPanel(false);
        viewGacha.ShowSingleItemPanel(false);
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
        viewGacha.PlayVideoFromResources(largeIndex, smallIndex);
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
        Debug.Log($"准备加载卡池：pool={(pool != null ? pool.poolName : "null")}, gachaManager={gachaManager}");

        if (pool != null && gachaManager != null)
        {
            // 加载前打印 currentPool
            var beforePool = gachaManager.GetCurrentPool();
            Debug.Log($"加载前 currentPool = {(beforePool != null ? beforePool.poolName : "null")}");

            gachaManager.LoadPool(pool);

            // 加载后检查
            var afterPool = gachaManager.GetCurrentPool();
            Debug.Log($"加载后 currentPool = {(afterPool != null ? afterPool.poolName : "null")}");

            if (afterPool != null)
                viewGacha.UpdateCurrentPoolName(pool.poolName);
            else
                Debug.LogError("LoadPool 后 currentPool 仍然为 null！");
        }
        else
        {
            Debug.LogError($"pool 或 gachaManager 为 null: pool={pool}, gachaManager={gachaManager}");
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

    private void OnGachaButtonClick()
    {
        if (gachaManager == null || playerData.Crystals < 280) return;
        playerData.Crystals -= 280;
        if (!EnsurePoolLoaded()) // 如果无法加载卡池，直接返回
        {
            Debug.LogError("卡池加载失败，无法抽卡");
            return;
        }

        string itemId = gachaManager.PerformGacha();
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("抽卡返回空");
            return;
        }

        List<string> ids = new List<string> { itemId };
        StartGachaSequence(ids);
    }

    private void OnGachaTenButtonClick()
    {
        if (gachaManager == null || playerData.Crystals < 2800) return;
        playerData.Crystals -= 2800;
        if (!EnsurePoolLoaded())
        {
            Debug.LogError("卡池加载失败，无法抽卡");
            return;
        }

        List<string> ids = new List<string>();
        for (int i = 0; i < 10; i++)
            ids.Add(gachaManager.PerformGacha());
        StartGachaSequence(ids);
    }

    private void StartGachaSequence(List<string> itemIds)
    {
        currentResults.Clear();
        foreach (string id in itemIds)
        {
            GetItemInfo(id, out string name, out int star, out Sprite icon, out Sprite illustration);
            currentResults.Add(new GachaResultItem
            {
                itemId = id,
                itemName = name,
                star = star,
                icon = icon,
                illustration = illustration
            });
        }

        currentState = GachaState.PlayingAnimation;
        viewGacha.ShowAnimationPanel(true);
        viewGacha.ShowSingleItemPanel(false);

        if (viewGacha.gachaAnimationVideoPlayer != null)
        {
            viewGacha.gachaAnimationVideoPlayer.loopPointReached += OnAnimationEnd;
            viewGacha.gachaAnimationVideoPlayer.Play();
        }
        else
        {
            OnAnimationEnd(null);
        }
    }

    private void OnAnimationEnd(VideoPlayer vp)
    {
        if (vp != null)
            vp.loopPointReached -= OnAnimationEnd;

        currentShowIndex = 0;
        currentState = GachaState.ShowingItems;
        viewGacha.ShowAnimationPanel(false);
        ShowCurrentItem();

        if (viewGacha.clickArea != null)
            viewGacha.clickArea.onClick.AddListener(OnClickNext);
    }

    private void ShowCurrentItem()
    {
        if (currentShowIndex < currentResults.Count)
        {
            var item = currentResults[currentShowIndex];
            if(item.illustration != null)
            {
                   viewGacha.UpdateSuperItemDisplay(item.illustration, item.itemName, item.star);
            }
            else
            {
                viewGacha.UpdateSingleItemDisplay(item.icon, item.itemName, item.star);
            }

                // 播放缩放动画
                viewGacha.PlayItemScaleAnimation();
            viewGacha.ShowSingleItemPanel(true);
        }
        else
        {
            // 所有道具展示完毕，先处理玩家数据
            ProcessGachaResults();
            GoToResultScene();
        }
    }

    private void OnClickNext()
    {
        if (currentState != GachaState.ShowingItems) return;
        currentShowIndex++;
        ShowCurrentItem();
    }

    private void GoToResultScene()
    {
        // 保存结果到静态数据
        GachaResultData.CurrentResults = new List<GachaResultItem>(currentResults);

        // 移除点击监听
        if (viewGacha.clickArea != null)
            viewGacha.clickArea.onClick.RemoveListener(OnClickNext);

        // 隐藏所有面板
        viewGacha.ShowAnimationPanel(false);
        viewGacha.ShowSingleItemPanel(false);

        // 加载结果场景
        SceneManager.LoadScene("GachaResultScene");
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

    public void SetGachaProbability(float threeStar, float fourStar, float fiveStar)
    {
        if (gachaManager == null) return;
        gachaManager.SetStarProbability(threeStar, fourStar, fiveStar);
    }

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

    private bool EnsurePoolLoaded()
    {
        if (gachaManager == null)
        {
            Debug.LogError("gachaManager 为 null");
            return false;
        }

        if (gachaManager.GetCurrentPool() != null)
        {
            Debug.Log("卡池已加载，无需重新加载");
            return true;
        }

        Debug.LogWarning("当前卡池为 null，尝试重新加载...");

        // 如果当前索引无效，尝试重置为第一个大栏位的第一个有效小栏位
        if (largeIndex < 0 || largeIndex >= menuSections.Length)
        {
            Debug.Log("largeIndex 无效，尝试全局查找");
            for (int i = 0; i < menuSections.Length; i++)
            {
                var section = menuSections[i];
                for (int j = 0; j < section.smallButtons.Length; j++)
                {
                    if (section.smallButtonPools != null && j < section.smallButtonPools.Length && section.smallButtonPools[j] != null)
                    {
                        Debug.Log($"找到有效小栏位：大栏位 {i}，小栏位 {j}，准备加载");
                        OnSmallButtonClick(i, j);
                        return gachaManager.GetCurrentPool() != null;
                    }
                }
            }
            Debug.LogError("没有任何小栏位配置了有效的卡池！");
            return false;
        }

        // 尝试用当前索引加载卡池
        var currentSection = menuSections[largeIndex];
        if (currentSection.smallButtonPools != null && smallIndex >= 0 && smallIndex < currentSection.smallButtonPools.Length &&
            currentSection.smallButtonPools[smallIndex] != null)
        {
            Debug.Log($"使用当前索引 {largeIndex}-{smallIndex} 加载卡池");
            OnSmallButtonClick(largeIndex, smallIndex);
            return gachaManager.GetCurrentPool() != null;
        }

        // 如果当前小栏位无效，在当前大栏位中寻找第一个有效小栏位
        Debug.Log($"当前小栏位 {smallIndex} 无效，在当前大栏位 {largeIndex} 中查找");
        for (int j = 0; j < currentSection.smallButtons.Length; j++)
        {
            if (currentSection.smallButtonPools != null && j < currentSection.smallButtonPools.Length && currentSection.smallButtonPools[j] != null)
            {
                Debug.Log($"在当前大栏位中找到有效小栏位 {j}");
                OnSmallButtonClick(largeIndex, j);
                return gachaManager.GetCurrentPool() != null;
            }
        }

        // 如果当前大栏位也没有有效卡池，全局寻找
        Debug.Log("当前大栏位无有效卡池，全局查找");
        for (int i = 0; i < menuSections.Length; i++)
        {
            var section = menuSections[i];
            for (int j = 0; j < section.smallButtons.Length; j++)
            {
                if (section.smallButtonPools != null && j < section.smallButtonPools.Length && section.smallButtonPools[j] != null)
                {
                    Debug.Log($"在全局中找到有效小栏位：大栏位 {i}，小栏位 {j}");
                    OnSmallButtonClick(i, j);
                    return gachaManager.GetCurrentPool() != null;
                }
            }
        }

        Debug.LogError("所有尝试均失败，没有任何小栏位配置了有效的卡池！");
        return false;
    }

    private void ProcessGachaResults()
    {
        if (currentResults == null || currentResults.Count == 0) return;

        var dataManager = GameDataManager.Instance;
        var playerDataManager = PlayerDataManager.Instance;
        if (dataManager == null || playerDataManager == null || playerDataManager.CurrentPlayerData == null) return;

        var playerData = playerDataManager.CurrentPlayerData;

        foreach (var result in currentResults)
        {
            string id = result.itemId;

            // 1. 检查是否为角色
            if (dataManager.CharacterDict.TryGetValue(id, out CharacterDefineSO charDef))
            {
                // 查找是否已有该角色
                var existingChar = playerData.Characters.Find(c => c.Id == id);
                if (existingChar != null && existingChar.IsUnlocked == true)
                {
                    // 已有：碎片 +10
                    Debug.Log(id + "+10");
                    existingChar.BaseStats.Fragments += 10;
                }
                else
                {
                    Debug.Log(id + "Unlock");
                    /*
                    // 没有：解锁角色
                    var newChar = new CharacterData(
                        id: charDef.id,
                        name: charDef.characterName,
                        isUnlocked: true,
                        element: charDef.element,
                        stars: charDef.baseStars,
                        maxstars: charDef.maxStars,
                        health: charDef.baseHealth,
                        attack: charDef.baseAttack,
                        defence: charDef.baseDefence,
                        energy: charDef.baseEnergy,
                        critRate: charDef.baseCritRate,
                        critDamage: charDef.baseCritDamage,
                        elementBonus: charDef.baseElementBonus
                    );
                    playerData.Characters.Add(newChar);
                    */
                    existingChar.IsUnlocked = true;
                }
            }
            // 2. 检查是否为武器
            else if (dataManager.WeaponDict.TryGetValue(id, out WeaponDefineSO weaponDef))
            {
                var newWeapon = new WeaponData(
                    id: weaponDef.id,
                    name: weaponDef.weaponName,
                    type: weaponDef.type,
                    element: weaponDef.element,
                    stars: weaponDef.baseStars,
                    maxstars: weaponDef.maxStars,
                    health: weaponDef.baseHealth,
                    attack: weaponDef.baseAttack,
                    defence: weaponDef.baseDefence,
                    energy: weaponDef.baseEnergy,
                    critRate: weaponDef.baseCritRate,
                    critDamage: weaponDef.baseCritDamage,
                    elementBonus: weaponDef.baseElementBonus,
                    introduction: weaponDef.introduction,
                    description: weaponDef.description
                );
                playerData.WeaponBag.Add(newWeapon);
            }
            // 3. 检查是否为圣痕
            else if (dataManager.StigmataDict.TryGetValue(id, out StigmataDefineSO stigmataDef))
            {
                var newStigmata = new StigmataData(
                    id: stigmataDef.id,
                    name: stigmataDef.stigmataName,
                    position: stigmataDef.Position,
                    element: stigmataDef.element,
                    stars: stigmataDef.baseStars,
                    maxstars: stigmataDef.maxStars,
                    health: stigmataDef.baseHealth,
                    attack: stigmataDef.baseAttack,
                    defence: stigmataDef.baseDefence,
                    energy: stigmataDef.baseEnergy,
                    critRate: stigmataDef.baseCritRate,
                    critDamage: stigmataDef.baseCritDamage,
                    elementBonus: stigmataDef.baseElementBonus,
                    introduction: stigmataDef.introduction,
                    description: stigmataDef.description
                );
                playerData.StigmataBag.Add(newStigmata);
            }
            // 4. 检查是否为材料
            else if (dataManager.MaterialDict.TryGetValue(id, out MaterialDefineSO materialDef))
            {
                // 查找是否已有该材料
                var existingMaterial = playerData.MaterialBag.Find(m => m.Id == id);
                if (existingMaterial != null)
                {
                    existingMaterial.Count += 1;
                }
                else
                {
                    var newMaterial = new MaterialData(
                        id: materialDef.id,
                        name: materialDef.materialName,
                        stars: materialDef.baseStars.ToString() + "S",
                        count: 1,
                        num: materialDef.num,
                        introduction: materialDef.introduction,
                        description: materialDef.description
                    );
                    playerData.MaterialBag.Add(newMaterial);
                }
            }
            else
            {
                Debug.LogWarning($"未知物品ID: {id}，无法处理");
            }
        }

        // 可选：对背包进行排序（例如按稀有度）
        playerDataManager.SortEquipment();

        // 保存数据
        playerDataManager.SaveCurrentPlayerData();
    }
}

[System.Serializable]
public class GachaResultItem
{
    public string itemId;
    public string itemName;
    public int star;
    public Sprite icon;
    public Sprite illustration;
}

public enum GachaState
{
    Idle,
    PlayingAnimation,
    ShowingItems
}