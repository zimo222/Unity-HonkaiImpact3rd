using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    // 当前商品池数据
    private ShopPoolSO currentPool;


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

    }

    // 可选：获取当前卡池信息（用于UI显示）
    public ShopPoolSO GetCurrentPool() => currentPool;
}