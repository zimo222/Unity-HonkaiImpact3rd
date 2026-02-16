using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("星级概率（总和应为100）")]
    [SerializeField] private float threeStarProbability = 50f;
    [SerializeField] private float fourStarProbability = 30f;
    [SerializeField] private float fiveStarProbability = 20f;
    [SerializeField] private float buwai = 0.2f;

    // 当前卡池数据（按星级分类）
    private ShopPoolSO currentPool;

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
    public void LoadPool(ShopPoolSO newPool)
    {
        Debug.Log($"LoadPool 被调用，传入卡池：{(newPool != null ? newPool.poolName : "null")}");
        if (newPool == null)
        {
            Debug.LogError("卡池为空！");
            return;
        }

        // 关键赋值
        currentPool = newPool;
        Debug.Log($"currentPool 已设置为：{currentPool.poolName}");

        foreach (var item in newPool.items)
        {
            switch (item.starLevel)
            {
                default: Debug.LogWarning($"未知星级：{item.starLevel}"); break;
            }
        }

    }

    // 可选：获取当前卡池信息（用于UI显示）
    public ShopPoolSO GetCurrentPool() => currentPool;
}