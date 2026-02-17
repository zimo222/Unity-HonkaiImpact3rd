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
        Crystals = 50000;
        Coins = 300000000;
        Stamina = Level + 80;

        // 初始化默认角色和装备和材料
        InitializeDefaultCharacters();
        InitializeDefaultEquipment();
        InitializeDefaultMaterial();
        SortedBag();
        // 初始化任务系统
        InitializeDefaultTasks();
        InitializeDailyEXPRewards();
    }

    public void SortedBag()
    {

        WeaponBag.Sort((a, b) =>
        {
            int statusOrderA = a.Stats.Stars;
            int statusOrderB = b.Stats.Stars;

            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前
            return b.Stats.Level.CompareTo(a.Stats.Level);
        });
        StigmataBag.Sort((a, b) =>
        {
            int statusOrderA = a.Stats.Stars;
            int statusOrderB = b.Stats.Stars;

            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前
            return 0;
        });
        MaterialBag.Sort((a, b) =>
        {
            int statusOrderA = a.Stars;
            int statusOrderB = b.Stars;

            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前
            return 0;
        });

    }
    #endregion


    // ================== 角色相关方法 ==================
    #region 角色方法
    /// <summary>
    /// 初始化默认角色
    /// </summary>
    private void InitializeDefaultCharacters()
    {
        for (int i = 1; i <= 3; i++)
        {
            AddDefaultCharacter("CHAR_0" + (i >= 10 ? "" : "0") + i.ToString(), true);
        }
        for (int i = 4; i <= 17; i++)
        {
            AddDefaultCharacter("CHAR_0" + (i >= 10 ? "" : "0") + i.ToString(), false);
        }
    }

    private void AddDefaultCharacter(string defineId, bool isUnlocked)
    {
        var def = GameDataManager.Instance.CharacterDict[defineId];
        var character = new CharacterData(
            id: def.id, name: def.characterName,
            isUnlocked: isUnlocked,
            element: def.element,
            stars: def.baseStars, maxstars: def.maxStars,
            health: def.baseHealth, attack: def.baseAttack, defence: def.baseDefence,
            energy: def.baseEnergy, critRate: def.baseCritRate, critDamage: def.baseCritDamage, elementBonus: def.baseElementBonus
        );
        Characters.Add(character);
    }
    #endregion


    // ================== 装备相关方法 ==================
    #region 装备方法
    /// <summary>
    /// 初始化默认装备
    /// </summary>
    private void InitializeDefaultEquipment()
    {
        for (int i = 1; i <= 30; i++)
        {
            AddDefaultWeapon("WEAP_0" + (i >= 10 ? "" : "0") + i.ToString());
        }

        AddDefaultStigmata("STIG_001_TOP");
        AddDefaultStigmata("STIG_001_MID");
        AddDefaultStigmata("STIG_001_BOT");
    }

    private void AddDefaultWeapon(string defineId)
    {
        var def = GameDataManager.Instance.WeaponDict[defineId];
        var weapon = new WeaponData(
            id: def.id, name: def.weaponName,
            type: def.type,
            element: def.element,
            stars: def.baseStars, maxstars: def.maxStars,
            health: def.baseHealth, attack: def.baseAttack, defence: def.baseDefence,
            energy: def.baseEnergy, critRate: def.baseCritRate, critDamage: def.baseCritDamage, elementBonus: def.baseElementBonus,
            introduction: def.introduction, description: def.description
        );
        WeaponBag.Add(weapon);
    }

    private void AddDefaultStigmata(string defineId)
    {
        var def = GameDataManager.Instance.StigmataDict[defineId];
        var stigmata = new StigmataData(
            id: def.id, name: def.stigmataName,
            position: def.Position,
            element: def.element,
            stars: def.baseStars, maxstars: def.maxStars,
            health: def.baseHealth, attack: def.baseAttack, defence: def.baseDefence,
            energy: def.baseEnergy, critRate: def.baseCritRate, critDamage: def.baseCritDamage, elementBonus: def.baseElementBonus,
            introduction: def.introduction, description: def.description
        );
        StigmataBag.Add(stigmata);
    }
    #endregion


    // ================== 材料相关方法 ==================
    #region  材料方法
    /// <summary>
    /// 初始化默认材料
    /// </summary>
    private void InitializeDefaultMaterial()
    {
        for (int i = 1; i <= 9; i++)
        {
            AddDefaultMaterial("MATE_0" + (i >= 10 ? "" : "0") + i.ToString(), 3333);
        }
    }

    public void AddDefaultMaterial(string defineId, int Count)
    {
        var def = GameDataManager.Instance.MaterialDict[defineId];
        var material = new MaterialData(
            id: def.id, name: def.materialName,
            stars: def.baseStars, count: Count, num: def.num,
            introduction: def.introduction, description: def.description
        );
        MaterialBag.Add(material);
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
                        string element, int stars, int maxstars,
                        int health, int attack, int defence, 
                        int energy, float critRate, float critDamage, float elementBonus)
    {
        Id = id; Name = name; IsUnlocked = isUnlocked;
        BaseStats = new CharacterStats()
        {
            Element = element, Level = 1, Stars = stars, MaxStars = maxstars,
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
                        string element = "", int stars = 0, int maxstars = 0,
                        int health = 0, int attack = 0, int defence = 0,
                        int energy = 0, float critRate = 0f, float critDamage = 0f, float elementBonus = 0f,
                        string introduction = "", string description = "")
    {
        Id = id; Name = name;
        Stats = new CharacterStats()
        {
            Element = element, Level = 1, Stars = stars, MaxStars = maxstars, SStars = 0, Fragments = 0,
            Health = health, Attack = attack, Defence = defence,
            Energy = energy, CritRate = critRate, CritDamage = critDamage, ElementBonus = elementBonus
        };
        TextStats = new TextStats()
        {
            Introduction = introduction, Description = description
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
                        string element = "", int stars = 0, int maxstars = 0,
                        int health = 0, int attack = 0, int defence = 0,
                        int energy = 0, float critRate = 0f, float critDamage = 0f, float elementBonus = 0f,
                        string introduction = "", string description = "")
        : base(id, name, element, stars, maxstars, health, attack, defence, energy, critRate, critDamage, elementBonus, introduction, description)
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
                        string element = "", int stars = 0, int maxstars = 0,
                        int health = 0, int attack = 0, int defence = 0,
                        int energy = 0, float critRate = 0f, float critDamage = 0f, float elementBonus = 0f,
                        string introduction = "", string description = "")
        : base(id, name, element, stars, maxstars, health, attack, defence, energy, critRate, critDamage, elementBonus, introduction, description)
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
    public int Stars;                             // 星级
    public int Count;                                // 材料数量
    public int Num;                                  // 数值

    public TextStats textStats;

    public MaterialData() { }

    public MaterialData(string id, string name, int stars, int count = 0, int num = 0, string introduction = null, string description = null)
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

    public int Stars;                                // 星级
    public int MaxStars;                             // 最大星级
    public int SStars;                               // 小星级
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
    HeavyArtillery,                                  // 重炮
    Claymore,                                        // 大剑
    Cross,                                           // 十字架
    Spear                                            // 长枪
}

public enum StigmataPosition
{
    None,                                            // 无（武器使用）
    Top,                                             // 上位圣痕
    Middle,                                          // 中位圣痕
    Bottom                                           // 下位圣痕
}