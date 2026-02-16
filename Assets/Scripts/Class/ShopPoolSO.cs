using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShopPool", menuName = "Shop/ShopPool")]
public class ShopPoolSO : ScriptableObject
{
    public string poolName;          // 卡池名称（用于显示）
    public List<ShopPoolItem> items;     // 卡池包含的所有物品
}