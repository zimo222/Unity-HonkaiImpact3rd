using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    [Header("星级概率（总和应为100）")]
    [SerializeField] private float threeStarProbability = 80f;
    [SerializeField] private float fourStarProbability = 15f;
    [SerializeField] private float fiveStarProbability = 5f;

    // 当前卡池数据（按星级分类）
    private GachaPoolSO currentPool;
    private List<PoolItem> threeStarItems;
    private List<PoolItem> fourStarItems;
    private List<PoolItem> fiveStarItems;

    // 保底计数器
    private int pullsSinceLastFourStar = 0;
    private int pullsSinceLastFiveStar = 0;
    private bool guaranteedFourStarNext = false;   // 10抽保底触发
    private bool guaranteedFiveStarNext = false;   // 100抽保底触发
    private bool isFiveStarGuaranteedUp = false;   // 大保底（下次五星必为UP）

    // 公开属性（用于UI显示）
    public int PullsSinceLastFourStar => pullsSinceLastFourStar;
    public int PullsSinceLastFiveStar => pullsSinceLastFiveStar;
    public bool IsGuaranteedFourStarNext => guaranteedFourStarNext;
    public bool IsGuaranteedFiveStarNext => guaranteedFiveStarNext;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 切换卡池（由菜单点击时调用）
    /// </summary>
    public void LoadPool(GachaPoolSO newPool)
    {
        if (newPool == null)
        {
            Debug.LogError("卡池为空！");
            return;
        }

        currentPool = newPool;

        // 按星级分类
        threeStarItems = new List<PoolItem>();
        fourStarItems = new List<PoolItem>();
        fiveStarItems = new List<PoolItem>();

        foreach (var item in newPool.items)
        {
            switch (item.starLevel)
            {
                case 3: threeStarItems.Add(item); break;
                case 4: fourStarItems.Add(item); break;
                case 5: fiveStarItems.Add(item); break;
                default: Debug.LogWarning($"未知星级：{item.starLevel}"); break;
            }
        }

        // 切换卡池时重置保底计数器（可根据游戏需求决定是否重置）
        ResetPityCounters();
    }

    private void ResetPityCounters()
    {
        pullsSinceLastFourStar = 0;
        pullsSinceLastFiveStar = 0;
        guaranteedFourStarNext = false;
        guaranteedFiveStarNext = false;
        isFiveStarGuaranteedUp = false;
    }

    /// <summary>
    /// 公开接口：动态修改三星/四星/五星概率
    /// </summary>
    public void SetStarProbability(float threeStar, float fourStar, float fiveStar)
    {
        float total = threeStar + fourStar + fiveStar;
        if (Mathf.Abs(total - 100f) > 0.01f)
        {
            Debug.LogWarning($"概率总和应为100，当前为{total}，将自动归一化");
            float factor = 100f / total;
            threeStar *= factor;
            fourStar *= factor;
            fiveStar *= factor;
        }
        threeStarProbability = threeStar;
        fourStarProbability = fourStar;
        fiveStarProbability = fiveStar;
    }

    /// <summary>
    /// 抽一次卡，返回抽到的物品ID
    /// </summary>
    public string PerformGacha()
    {
        if (currentPool == null)
        {
            Debug.LogError("未加载卡池！");
            return null;
        }

        // 1. 根据保底和概率决定本次抽到的星级
        int star = DetermineStar();

        // 2. 从对应星级列表中按权重随机选择一个物品
        PoolItem selected = SelectItemByStar(star);

        // 3. 更新保底计数器
        UpdatePity(star);

        return selected?.itemId;
    }

    // 决定星级（考虑保底）
    private int DetermineStar()
    {
        // 五星保底优先
        if (pullsSinceLastFiveStar >= 99 || guaranteedFiveStarNext)
        {
            guaranteedFiveStarNext = false;
            pullsSinceLastFiveStar = 0;
            return 5;
        }

        // 四星保底
        if (pullsSinceLastFourStar >= 9 || guaranteedFourStarNext)
        {
            guaranteedFourStarNext = false;
            pullsSinceLastFourStar = 0;
            return 4;
        }

        // 正常概率随机
        float rand = Random.Range(0f, 100f);
        if (rand < fiveStarProbability)
            return 5;
        if (rand < fiveStarProbability + fourStarProbability)
            return 4;
        return 3;
    }

    // 从指定星级的列表中随机选择一个物品（考虑UP与大保底）
    private PoolItem SelectItemByStar(int star)
    {
        List<PoolItem> list = star == 3 ? threeStarItems :
                               star == 4 ? fourStarItems : fiveStarItems;

        if (list == null || list.Count == 0)
        {
            Debug.LogError($"星级 {star} 没有可用的物品！");
            return null;
        }

        // 五星处理UP和大保底
        if (star == 5)
        {
            var upItems = list.FindAll(item => item.isUp);
            var nonUpItems = list.FindAll(item => !item.isUp);

            // 如果有大保底，必出UP
            if (isFiveStarGuaranteedUp)
            {
                isFiveStarGuaranteedUp = false;
                return WeightedRandom(upItems.Count > 0 ? upItems : list); // 若没有UP则全池随机
            }

            // 正常情况：50%概率出UP（可根据权重细化）
            if (upItems.Count > 0)
            {
                bool wantUp = Random.Range(0f, 1f) < 0.5f;
                if (wantUp)
                {
                    return WeightedRandom(upItems);
                }
                else
                {
                    if (nonUpItems.Count > 0)
                    {
                        // 没出UP，激活大保底
                        isFiveStarGuaranteedUp = true;
                        return WeightedRandom(nonUpItems);
                    }
                    else
                    {
                        // 没有非UP物品，则只能出UP
                        return WeightedRandom(upItems);
                    }
                }
            }
        }

        // 四星和三星直接按权重随机
        return WeightedRandom(list);
    }

    // 权重随机工具
    private PoolItem WeightedRandom(List<PoolItem> items)
    {
        float totalWeight = 0f;
        foreach (var item in items)
            totalWeight += item.weight;

        float rand = Random.Range(0f, totalWeight);
        float cum = 0f;
        foreach (var item in items)
        {
            cum += item.weight;
            if (rand < cum)
                return item;
        }
        return items[items.Count - 1]; // 防溢出
    }

    // 更新保底计数器
    private void UpdatePity(int star)
    {
        pullsSinceLastFourStar++;
        pullsSinceLastFiveStar++;

        if (star == 4)
            pullsSinceLastFourStar = 0;
        else if (star == 5)
            pullsSinceLastFiveStar = 0;

        if (pullsSinceLastFourStar >= 10)
            guaranteedFourStarNext = true;
        if (pullsSinceLastFiveStar >= 100)
            guaranteedFiveStarNext = true;
    }

    // 可选：获取当前卡池信息（用于UI显示）
    public GachaPoolSO GetCurrentPool() => currentPool;
}