using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // ================== 基础账户信息 ==================
    public string PlayerID;                         // 唯一标识
    public string PlayerName;
    public DateTime CreateTime;                     // 账号创建时间
    public DateTime LastLoginTime;                  // 上次登录时间

    // ================== 游戏进度与资源 ==================
    public int Level = 88;
    public int Experience;
    public int Crystals;                           // 水晶
    public int Coins;                              // 金币
    public int Stamina;                            // 体力
    public int HomogeneousPureCrystal;             // 恒质纯晶

    // ================== 角色与装备系统 ==================
    [Header("角色系统")]
    public List<CharacterData> Characters = new List<CharacterData>();          // 角色列表
    public List<WeaponData> WeaponBag = new List<WeaponData>();
    public List<StigmataData> StigmataBag = new List<StigmataData>();
    public List<MaterialData> MaterialBag = new List<MaterialData>();        // 装备背包

    // ================== 设置与其他 ==================
    public float MusicVolume = 0.8f;
    public float SFXVolume = 0.8f;
    public string LastLoginIP = "";

    // ================== 任务系统 ==================
    [Header("任务系统")]
    public List<TaskData> Tasks = new List<TaskData>();                  // 所有任务
    public DateTime LastTaskCheckTime = DateTime.Now;                    // 上次查看任务时间
    public int CombatLevel;
    public int CombatEXP;
    public int WeekCombatEXP;
    public int DailyEXP = 0;                                            // 当日历练值
    public List<DailyEXPReward> DailyEXPRewards = new List<DailyEXPReward>(); // 每日历练值奖励

    // ================== 构造函数 ==================
    #region 构造方法
    // 空构造函数为JSON反序列化所需
    public PlayerData() { }

    public PlayerData(string playerName)
    {
        PlayerID = System.Guid.NewGuid().ToString();
        PlayerName = playerName;
        CreateTime = DateTime.Now;
        LastLoginTime = DateTime.Now;

        // 初始化默认资源
        Crystals = 500;
        Coins = 3000;
        Stamina = Level + 80;

        // 初始化默认角色和装备和材料
        InitializeDefaultCharacters();
        InitializeDefaultEquipment();
        InitializeDefaultMaterial();
        // 初始化任务系统
        InitializeDefaultTasks();
        InitializeDailyEXPRewards();
    }
    #endregion


    // ================== 角色相关方法 ==================
    #region 角色方法
    /// <summary>
    /// 初始化默认角色
    /// </summary>
    private void InitializeDefaultCharacters()
    {
        // 琪亚娜·薪炎之律者
        Characters.Add(new CharacterData(
            id: "CHAR_001", name: "琪亚娜-薪炎之律者", isUnlocked: true,
            element: "YN", stars: "S",
            health: 2004, attack: 501, defence: 168,
            energy: 140, critRate: 0.1f, critDamage: 1.5f, elementBonus: 0.3f
        ));
        // 琪亚娜·白练
        Characters.Add(new CharacterData(
            id: "CHAR_002", name: "琪亚娜-领域装·白练", isUnlocked: true, 
            element: "JX", stars: "S",
            health: 1200, attack: 250, defence: 150,
            energy: 130, critRate: 0.08f, critDamage: 1.3f, elementBonus: 0f
        ));
        // 叶瞬光
        Characters.Add(new CharacterData(
            id: "CHAR_003", name: "叶瞬光-虚狩·青暝司命", isUnlocked: true,
            element: "SW", stars: "S",
            health: 1400, attack: 280, defence: 150,
            energy: 150, critRate: 0.12f, critDamage: 1.6f, elementBonus: 0.2f
        ));
        // 胡桃
        Characters.Add(new CharacterData(
            id: "CHAR_004", name: "胡桃-往生堂堂主", isUnlocked: false,
            element: "SW", stars: "S",
            health: 1100, attack: 320, defence: 130,
            energy: 130, critRate: 0.15f, critDamage: 1.8f, elementBonus: 0.4f
        ));
        // 流萤
        Characters.Add(new CharacterData(
            id: "CHAR_005", name: "流萤-神秘机甲女", isUnlocked: false,
            element: "YN", stars: "S",
            health: 1300, attack: 270, defence: 120,
            energy: 120, critRate: 0.09f, critDamage: 1.4f, elementBonus: 0.25f
        ));
    }
    #endregion


    // ================== 装备相关方法 ==================
    #region 装备方法
    /// <summary>
    /// 初始化默认装备
    /// </summary>
    private void InitializeDefaultEquipment()
    {
        Debug.Log("111");
        // 添加一些初始武器
        WeaponBag.Add(new WeaponData(
            id: "WEAP_001", name: "焢煌之钥", type: WeaponType.DualPistols,
            stars: "4S1",
            health: 0, attack: 298, defence: 0,
            energy: 0, critRate: 0.12f, critDamage: 0f, elementBonus: 0.15f,
            description: "少女在炽焰中窥见的，是温柔而赞许的眼神。\n那眼神带来的温度，给予了她担起未来的勇气。\n身后伸来的手，为她的信念增添柴薪。\n这意志化为煌燃的剑，点亮黑夜。"
        ));
        WeaponBag.Add(new WeaponData(
            id: "WEAP_002", name: "月神之守护", type: WeaponType.DualPistols,
            stars: "4S1",
            health: 0, attack: 150, defence: 0,
            energy: 0, critRate: 0.05f, critDamage: 0f, elementBonus: 0.15f
        ));
        WeaponBag.Add(new WeaponData(
            id: "WEAP_003", name: "青溟剑", type: WeaponType.SingleHandedSword,
            stars: "4S1",
            health: 0, attack: 80, defence: 0,
            energy: 0, critRate: 0f, critDamage: 0f, elementBonus: 0f
        ));
        WeaponBag.Add(new WeaponData(
            id: "WEAP_004", name: "萨姆召唤剑", type: WeaponType.SingleHandedSword,
            stars: "4S1",
            health: 0, attack: 90, defence: 0,
            energy: 0, critRate: 0.02f, critDamage: 0f, elementBonus: 0f
        ));
        WeaponBag.Add(new WeaponData(
            id: "WEAP_005", name: "护摩之剑(杖)", type: WeaponType.SingleHandedSword,
            stars: "4S1",
            health: 0, attack: 90, defence: 0,
            energy: 0, critRate: 0.02f, critDamage: 0f, elementBonus: 0f
        ));
        // 添加一些圣痕
        // 上位圣痕
        StigmataBag.Add(new StigmataData(
            id: "STIG_001_TOP", name: "无量塔姬子(上)", position: StigmataPosition.Top,
            stars: "5S",
            health: 200, attack: 50, defence: 10,
            energy: 0, critRate: 0f, critDamage: 0.1f, elementBonus: 0.08f
        ));
        // 中位圣痕
        StigmataBag.Add(new StigmataData(
            id: "STIG_001_MID", name: "无量塔姬子(中)", position: StigmataPosition.Middle,
            stars: "5S",
            health: 150, attack: 0, defence: 10,
            energy: 0, critRate: 0.02f, critDamage: 0.15f, elementBonus: 0.1f
        ));
        // 下位圣痕
        StigmataBag.Add(new StigmataData(
            id: "STIG_001_BOT", name: "无量塔姬子(下)", position: StigmataPosition.Bottom,
            stars: "5S",
            health: 180, attack: 40, defence: 10,
            energy: 0, critRate: 0.04f, critDamage: 0.08f, elementBonus: 0.06f
        ));
    }
    #endregion


    // ================== 材料相关方法 ==================
    #region  材料方法
    /// <summary>
    /// 初始化默认材料
    /// </summary>
    private void InitializeDefaultMaterial()
    {
        // 添加一些初始材料
        MaterialBag.Add(new MaterialData(
            id: "MATE_001",
            name: "特级学习芯片",
            stars: "4S",
            count: 3,
            num: 7500,
            introduction: "提供7500点角色或武装人偶经验值。",
            description: "全方位记录了真实崩坏战场的稀有芯片，对使用者的成长将有质的突破。但若是没有足够强大的内心，则可能被战场上残酷的血雨腥风所侵蚀。"
        ));
        MaterialBag.Add(new MaterialData(
            id: "MATE_002",
            name: "高级学习芯片",
            stars: "3S",
            count: 30,
            num: 1500,
            introduction: "提供1500点角色或武装人偶经验值。",
            description: "能够将一些高级战略战术、实战经验、以及复杂机甲的操作方法置入使用者大脑的芯片，可大幅提高作战能力，但吸收程度依使用者资质而异。"
        ));
        MaterialBag.Add(new MaterialData(
            id: "MATE_003",
            name: "进阶学习芯片",
            stars: "2S",
            count: 300,
            num: 300,
            introduction: "提供300点角色或武装人偶经验值。",
            description: "能够将一些基本的格斗术和武器使用技巧直接置入使用者大脑的芯片，对实战格斗具有较好的指导作用。"
        ));
        MaterialBag.Add(new MaterialData(
            id: "MATE_004",
            name: "基础学习芯片",
            stars: "1S",
            count: 3000,
            num: 60,
            introduction: "提供60点角色或武装人偶经验值。",
            description: "记录了一些关于世界的基本常识和历史的学习芯片，有助于使用者简单了解自己身处的环境。"
        ));
        MaterialBag.Add(new MaterialData(
            id: "MATE_005",
            name: "双子灵魂结晶",
            stars: "4S",
            count: 3,
            num: 15000,
            introduction: "提供15000点装备经验值。",
            description: "两块黄色结晶，上面刻着耀眼的橙色的纹路，可以大幅提升武器或圣痕的力量。"
        ));
        MaterialBag.Add(new MaterialData(
            id: "MATE_006",
            name: "灵魂结晶",
            stars: "3S",
            count: 30,
            num: 7500,
            introduction: "提供7500点装备经验值。",
            description: "一块黄色结晶，上面刻着耀眼的橙色的纹路，可以显著提升武器或圣痕的力量。"
        ));
        MaterialBag.Add(new MaterialData(
            id: "MATE_007",
            name: "双子灵魂碎片",
            stars: "2S",
            count: 300,
            num: 3750,
            introduction: "提供3750点装备经验值。",
            description: "两块黄色碎片，上面刻着橙色的纹路，可以有效强化武器或圣痕。"
        ));
        MaterialBag.Add(new MaterialData(
            id: "MATE_008",
            name: "灵魂碎片",
            stars: "1S",
            count: 3000,
            num: 1500,
            introduction: "提供1500点装备经验值。",
            description: "一块黄色碎片，上面刻着橙色的纹路，可以强化武器或圣痕。"
        ));
    }
    #endregion


    // ================== 任务相关方法 ==================
    #region 任务方法
    /// <summary>
    /// 初始化默认任务
    /// </summary>
    private void InitializeDefaultTasks()
    {
        // 日常任务
        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_DAILY_001",
            name: "芽衣的加餐",
            unlockLevel: 10,
            frequency: TaskFrequency.Daily,
            reward1: new TaskReward(RewardType.DailyEXP, 50),
            reward2: new TaskReward(RewardType.Stamina, 60),
            maxTime: 1,
            description: "Check-in Task",
            sceneName: "NoneScene",
            battleType: "Normal"
        ));

        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_DAILY_002",
            name: "金币采集",
            unlockLevel: 15,
            frequency: TaskFrequency.Daily,
            reward1: new TaskReward(RewardType.DailyEXP, 50),
            reward2: new TaskReward(RewardType.EXP, 50),
            maxTime: 1,
            description: "Go to the home base to collect coins",
            sceneName: "HomeLandScene",
            battleType: "Normal"
        ));

        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_DAILY_003",
            name: "材料活动",
            unlockLevel: 15,
            frequency: TaskFrequency.Daily,
            reward1: new TaskReward(RewardType.DailyEXP, 200),
            reward2: new TaskReward(RewardType.EXP, 200),
            maxTime: 3,
            description: "Go to battle to obtain materials",
            sceneName: "MaterialScene",
            battleType: "Material"
        ));

        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_DAILY_004",
            name: "家园打工",
            unlockLevel: 20,
            frequency: TaskFrequency.Daily,
            reward1: new TaskReward(RewardType.DailyEXP, 50),
            reward2: new TaskReward(RewardType.EXP, 50),
            maxTime: 1,
            description: "It's mygo!!!",
            sceneName: "BossScene",
            battleType: "Boss"
        ));

        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_DAILY_005",
            name: "剧情关卡",
            unlockLevel: 1,
            frequency: TaskFrequency.Daily,
            reward1: new TaskReward(RewardType.DailyEXP, 150),
            reward2: new TaskReward(RewardType.EXP, 300),
            maxTime: 5,
            description: "fight!fight!fight!",
            sceneName: "BattleScene",
            battleType: "All"
        ));

        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_DAILY_006",
            name: "持续作战",
            unlockLevel: 10,
            frequency: TaskFrequency.Daily,
            reward1: new TaskReward(RewardType.DailyEXP, 100),
            reward2: new TaskReward(RewardType.EXP, 100),
            maxTime: 5,
            description: "Only fight!!!",
            sceneName: "BattleScene",
            battleType: "All"
        ));

        // 周常任务
        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_WEEKLY_001",
            name: "每周考题",
            unlockLevel: 30,
            frequency: TaskFrequency.Weekly,
            reward1: new TaskReward(RewardType.DailyEXP, 300),
            reward2: new TaskReward(RewardType.EXP, 200),
            maxTime: 1,
            description: "I don't like exam.",
            sceneName: "BattleScene",
            battleType: "Normal"
        ));

        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_WEEKLY_002",
            name: "模拟作战室",
            unlockLevel: 20,
            frequency: TaskFrequency.Weekly,
            reward1: new TaskReward(RewardType.DailyEXP, 300),
            reward2: new TaskReward(RewardType.EXP, 200),
            maxTime: 5,
            description: "Just fight.",
            sceneName: "BossScene",
            battleType: "Boss"
        ));

        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_WEEKLY_003",
            name: "无尽深渊",
            unlockLevel: 25,
            frequency: TaskFrequency.Weekly,
            reward1: new TaskReward(RewardType.DailyEXP, 1000),
            reward2: new TaskReward(RewardType.EXP, 1000),
            maxTime: 10,
            description: "You don't ao le.",
            sceneName: "EquipmentScene",
            battleType: "None"
        ));
    }

    /// <summary>
    /// 初始化每日历练值奖励
    /// </summary>
    private void InitializeDailyEXPRewards()
    {
        DailyEXPRewards.Clear();
        DailyEXPRewards.Add(new DailyEXPReward(120, new TaskReward(RewardType.Crystals, 5), new TaskReward(RewardType.Coins, 100)));
        DailyEXPRewards.Add(new DailyEXPReward(240, new TaskReward(RewardType.Crystals, 5), new TaskReward(RewardType.Coins, 100)));
        DailyEXPRewards.Add(new DailyEXPReward(360, new TaskReward(RewardType.Crystals, 10), new TaskReward(RewardType.Coins, 200)));
        DailyEXPRewards.Add(new DailyEXPReward(480, new TaskReward(RewardType.Crystals, 10), new TaskReward(RewardType.Coins, 200)));
        DailyEXPRewards.Add(new DailyEXPReward(600, new TaskReward(RewardType.Crystals, 10), new TaskReward(RewardType.Coins, 200)));
    }
    #endregion 
}

// ================== 角色数据类 ==================
[System.Serializable]
public class CharacterData
{
    public string Id;                                // 角色ID
    public string Name;                              // 角色名称
    public bool IsUnlocked;                          // 是否解锁
    public CharacterStats BaseStats;                 // 基础属性

    // 装备索引（指向EquipmentBag的下标）
    public int EquippedWeaponIndex = -1;             // 装备的武器索引
    public int EquippedTopStigmataIndex = -1;        // 上位圣痕索引
    public int EquippedMiddleStigmataIndex = -1;     // 中位圣痕索引
    public int EquippedBottomStigmataIndex = -1;     // 下位圣痕索引

    public CharacterData() { }

    public CharacterData(string id, string name, bool isUnlocked, 
                        string element, string stars,
                        int health, int attack, int defence, 
                        int energy, float critRate, float critDamage, float elementBonus)
    {
        Id = id; Name = name; IsUnlocked = isUnlocked;
        BaseStats = new CharacterStats()
        {
            Element = element, Level = 1, Stars = stars,
            Health = health, Attack = attack, Defence = defence,
            Energy = energy, CritRate = critRate,CritDamage = critDamage,ElementBonus = elementBonus
        };
    }
}

// ================== 装备数据类 ==================
[System.Serializable]
public class EquipmentData
{
    public string Id;                                // 装备ID
    public string Name;                              // 装备名称
    public CharacterStats Stats;                     // 装备属性
    public TextStats TextStats;                     // 文本属性

    // 装备状态
    public int EquippedToCharacterIndex = -1;        // 被哪个角色装备（-1表示未装备）

    public EquipmentData() { }

    public EquipmentData(string id, string name,
                        string element = "", string stars = "",
                        int health = 0, int attack = 0, int defence = 0,
                        int energy = 0, float critRate = 0f, float critDamage = 0f, float elementBonus = 0f,
                        string introduction = "", string description = "")
    {
        Id = id; Name = name;
        Stats = new CharacterStats()
        {
            Element = element,
            Level = 1,
            Stars = stars,
            Health = health,
            Attack = attack,
            Defence = defence,
            Energy = energy,
            CritRate = critRate,
            CritDamage = critDamage,
            ElementBonus = elementBonus
        };
        TextStats = new TextStats()
        {
            Introduction = introduction,
            Description = description
        };
    }

    public int Health => Stats.Health;
    public int Attack => Stats.Attack;
    public float CritRate => Stats.CritRate;
    public float CritDamage => Stats.CritDamage;
    public float ElementBonus => Stats.ElementBonus;
}
[System.Serializable]  
public class WeaponData : EquipmentData
{
    public WeaponType Type;

    public WeaponData() { }

    public WeaponData(string id, string name, WeaponType type,
                        string element = "", string stars = "",
                        int health = 0, int attack = 0, int defence = 0,
                        int energy = 0, float critRate = 0f, float critDamage = 0f, float elementBonus = 0f,
                        string introduction = "", string description = "")
        : base(id, name, element, stars, health, attack, defence, energy, critRate, critDamage, elementBonus, introduction, description)
    {
        Type = type;
    }
}
[System.Serializable]
public class StigmataData : EquipmentData
{
    public StigmataPosition Position;

    public StigmataData() { }

    public StigmataData(string id, string name, StigmataPosition position,
                        string element = "", string stars = "",
                        int health = 0, int attack = 0, int defence = 0,
                        int energy = 0, float critRate = 0f, float critDamage = 0f, float elementBonus = 0f,
                        string introduction = "", string description = "")
        : base(id, name, element, stars, health, attack, defence, energy, critRate, critDamage, elementBonus, introduction, description)
    {
        Position = position;
    }
}
// ================== 材料数据类 ==================
[System.Serializable]
public class MaterialData
{
    public string Id;                                // 材料ID
    public string Name;                              // 材料名称
    public string Stars;                             // 星级
    public int Count;                                // 材料数量
    public int Num;                                  // 数值

    public TextStats textStats;

    public MaterialData() { }

    public MaterialData(string id, string name, string stars, int count = 0, int num = 0, string introduction = null, string description = null)
    {
        Id = id;
        Name = name;
        Stars = stars;
        Count = count;
        Num = num;
        textStats = new TextStats
        {
            Introduction = introduction,
            Description = description
        };
    }
}

// ================== 属性结构体 ==================
[System.Serializable]
public struct CharacterStats
{
    public string Element;                           // 元素

    public int Level;                                // 等级
    public int Exp;                                  // 经验

    public string Stars;                             // 星级
    public int Fragments;                            // 碎片

    public int Health;                               // 生命值
    public int Attack;                               // 攻击力
    public int Defence;                              // 防御力

    public int Energy;                               // 能量
    public float CritRate;                           // 暴击率（0-1）
    public float CritDamage;                         // 暴击伤害（倍率，如1.5表示150%）
    public float ElementBonus;                       // 元素伤害加成（百分比，如0.3表示30%）

    public override string ToString()
    {
        return $"生命: {Health}, 攻击: {Attack}, 暴击: {CritRate:P0}, 爆伤: {CritDamage:P0}, 元素: {ElementBonus:P0}";
    }
}
[System.Serializable]
public struct TextStats
{
    public string Introduction;                          // 介绍
    public string Description;                           // 描述
}

// ================== 枚举定义 ==================
public enum WeaponType
{
    None,                                            // 无（圣痕使用）
    DualPistols,                                     // 双枪
    SingleHandedSword,                               // 单手剑
    Spear                                            // 长枪
}

public enum StigmataPosition
{
    None,                                            // 无（武器使用）
    Top,                                             // 上位圣痕
    Middle,                                          // 中位圣痕
    Bottom                                           // 下位圣痕
}