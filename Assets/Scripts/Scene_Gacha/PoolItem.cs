using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 物品类型枚举
public enum ItemType
{
    Character,
    Weapon,
    Stigmata,
    Material
}

[Serializable]
public class PoolItem
{
    public string itemId;           // 物品ID，与 GameDataManager 字典中的键对应
    public ItemType itemType;        // 物品类型
    public int starLevel;            // 星级：3、4、5
    public float weight;             // 同星级内随机权重（越大越容易抽到）
    public bool isUp;                // 是否为 UP 物品（影响五星保底）
}