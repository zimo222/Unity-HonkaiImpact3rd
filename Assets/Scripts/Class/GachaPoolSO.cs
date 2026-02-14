using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGachaPool", menuName = "Gacha/GachaPool")]
public class GachaPoolSO : ScriptableObject
{
    public string poolName;          // 卡池名称（用于UI显示）
    public List<PoolItem> items;     // 卡池包含的所有物品
}