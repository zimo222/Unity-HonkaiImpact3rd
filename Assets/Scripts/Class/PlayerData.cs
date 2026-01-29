using System;
using System.Collections.Generic;
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
    public List<EquipmentData> EquipmentBag = new List<EquipmentData>();        // 装备背包

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

        // 初始化默认角色和装备
        InitializeDefaultCharacters();
        InitializeDefaultEquipment();

        // 初始化任务系统
        InitializeDefaultTasks();
        InitializeDailyEXPRewards();
    }

    // ================== 任务相关方法 ==================

    /// <summary>
    /// 初始化默认任务
    /// </summary>
    private void InitializeDefaultTasks()
    {
        // 日常任务
        Tasks.Add(new TaskData(
            level: Level,
            id: "TASK_DAILY_001",
            name: "Mei's Snack",
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
            name: "Coin Collection",
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
            name: "Material Activities",
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
            name: "Homeland Part-Time Job",
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
            name: "Story Level",
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
            name: "Continuous Combat",
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
            name: "Weekly Exam",
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
            name: "Simulation Operations Room",
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
            name: "Endless Abyss",
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

    /// <summary>
    /// 刷新任务状态（检查是否需要重置日常/周常任务）
    /// </summary>
    public void RefreshTasks()
    {
        DateTime now = DateTime.Now;
        DateTime lastCheck = LastTaskCheckTime;

        // 计算每日刷新时间（4:00 AM）
        DateTime dailyResetTime = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0);
        if (now.Hour < 4)
        {
            dailyResetTime = dailyResetTime.AddDays(-1);
        }

        // 计算每周刷新时间（周一4:00 AM）
        DateTime weeklyResetTime = dailyResetTime;
        while (weeklyResetTime.DayOfWeek != DayOfWeek.Monday)
        {
            weeklyResetTime = weeklyResetTime.AddDays(-1);
        }

        bool shouldRefreshDaily = lastCheck < dailyResetTime;
        bool shouldRefreshWeekly = lastCheck < weeklyResetTime;

        // 刷新日常任务
        if (shouldRefreshDaily)
        {
            ResetDailyTasks();
            DailyEXP = 0; // 重置每日历练值
            ResetDailyEXPRewards(); // 重置奖励领取状态
        }

        // 刷新周常任务
        if (shouldRefreshWeekly)
        {
            ResetWeeklyTasks();
        }

        // 解锁达到等级要求的任务
        foreach (var task in Tasks)
        {
            if (task.Status == TaskStatus.Locked && Level >= task.UnlockLevel)
            {
                task.Status = TaskStatus.Unlocked;
            }
        }

        LastTaskCheckTime = now;
    }

    /// <summary>
    /// 重置日常任务
    /// </summary>
    private void ResetDailyTasks()
    {
        foreach (var task in Tasks)
        {
            if (task.Frequency == TaskFrequency.Daily &&
                task.Status != TaskStatus.Locked)
            {
                task.Status = TaskStatus.Unlocked;
            }
        }
    }

    /// <summary>
    /// 重置周常任务
    /// </summary>
    private void ResetWeeklyTasks()
    {
        foreach (var task in Tasks)
        {
            if (task.Frequency == TaskFrequency.Weekly &&
                task.Status != TaskStatus.Locked)
            {
                task.Status = TaskStatus.Unlocked;
            }
        }
    }

    /// <summary>
    /// 重置每日历练值奖励
    /// </summary>
    private void ResetDailyEXPRewards()
    {
        foreach (var reward in DailyEXPRewards)
        {
            reward.IsClaimed = false;
        }
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    public bool CompleteTask(string taskId)
    {
        var task = Tasks.Find(t => t.TaskID == taskId);
        if (task == null || task.Status != TaskStatus.Unlocked)
            return false;

        task.Status = TaskStatus.Completed;

        // 增加每日历练值
        DailyEXP += task.GetDailyEXPReward();

        return true;
    }

    /// <summary>
    /// 领取任务奖励
    /// </summary>
    public bool ClaimTaskReward(string taskId)
    {
        var task = Tasks.Find(t => t.TaskID == taskId);
        if (task == null || task.Status != TaskStatus.Completed)
            return false;

        // 发放奖励1
        GiveReward(task.Reward1);

        // 发放奖励2
        GiveReward(task.Reward2);

        task.Status = TaskStatus.Claimed;
        return true;
    }

    /// <summary>
    /// 领取每日历练值奖励
    /// </summary>
    public bool ClaimDailyEXPReward(int index)
    {
        if (index < 0 || index >= DailyEXPRewards.Count)
            return false;

        var reward = DailyEXPRewards[index];
        if (reward.IsClaimed || DailyEXP < reward.RequiredEXP)
            return false;

        GiveReward(reward.Reward1);
        GiveReward(reward.Reward2);
        reward.IsClaimed = true;
        return true;
    }

    /// <summary>
    /// 发放奖励
    /// </summary>
    private void GiveReward(TaskReward reward)
    {
        switch (reward.Type)
        {
            case RewardType.Crystals:
                Crystals += reward.Amount;
                break;
            case RewardType.Coins:
                Coins += reward.Amount;
                break;
            case RewardType.Stamina:
                Stamina += reward.Amount;
                break;
            case RewardType.DailyEXP:
                DailyEXP += reward.Amount;
                CombatEXP += reward.Amount;
                WeekCombatEXP += reward.Amount;
                if (CombatEXP >= 1000)
                {
                    CombatLevel++;
                    CombatEXP %= 1000;
                }
                break;
            case RewardType.Equipment:
                // 生成随机装备
                EquipmentData randomEquipment = EquipmentHelper.GenerateRandomEquipment();
                EquipmentBag.Add(randomEquipment);
                break;
                // 其他奖励类型...
        }
    }

    /// <summary>
    /// 获取任务列表（排序：解锁的未完成 > 已完成的 > 未解锁的，按等级排序）
    /// </summary>
    public List<TaskData> GetSortedTasks(TaskFrequency? frequency = null)
    {
        var filteredTasks = frequency.HasValue
            ? Tasks.FindAll(t => t.Frequency == frequency.Value)
            : Tasks;

        // 排序：未解锁(Locked)的在最后，完成的(Claimed)在次后，完成的(Completed)和未完成的(Unlocked)按等级排序
        filteredTasks.Sort((a, b) =>
        {
            // 状态优先级：Unlocked > Completed > Claimed > Locked
            int statusOrderA = GetStatusOrder(a.Status);
            int statusOrderB = GetStatusOrder(b.Status);

            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前

            // 同状态按解锁等级排序
            return a.UnlockLevel.CompareTo(b.UnlockLevel);
        });

        return filteredTasks;
    }

    private int GetStatusOrder(TaskStatus status)
    {
        switch (status)
        {
            case TaskStatus.Completed: return 3;
            case TaskStatus.Unlocked: return 2;
            case TaskStatus.Claimed: return 1;
            case TaskStatus.Locked: return 0;
            default: return 0;
        }
    }

    // 空构造函数为JSON反序列化所需
    public PlayerData() { }

    // ================== 角色相关方法 ==================

    /// <summary>
    /// 初始化默认角色
    /// </summary>
    private void InitializeDefaultCharacters()
    {
        // 琪亚娜·薪炎之律者
        Characters.Add(new CharacterData(
            id: "CHAR_001",
            name: "琪亚娜-薪炎之律者",
            isUnlocked: true,
            element: "YN",
            stars: "S",
            health: 1500,
            attack: 300,
            critRate: 0.1f,
            critDamage: 1.5f,
            elementBonus: 0.3f
        ));

        // 琪亚娜·白练
        Characters.Add(new CharacterData(
            id: "CHAR_002",
            name: "琪亚娜-领域装·白练",
            isUnlocked: false,
            element: "JX",
            stars: "S",
            health: 1200,
            attack: 250,
            critRate: 0.08f,
            critDamage: 1.3f,
            elementBonus: 0f
        ));

        // 叶瞬光
        Characters.Add(new CharacterData(
            id: "CHAR_003",
            name: "叶瞬光-虚狩·青暝司命",
            isUnlocked: false,
            element: "SW",
            stars: "S",
            health: 1400,
            attack: 280,
            critRate: 0.12f,
            critDamage: 1.6f,
            elementBonus: 0.2f
        ));
        // 胡桃
        Characters.Add(new CharacterData(
            id: "CHAR_004",
            name: "胡桃-往生堂堂主",
            isUnlocked: false,
            element: "SW",
            stars: "S",
            health: 1100,
            attack: 320,
            critRate: 0.15f,
            critDamage: 1.8f,
            elementBonus: 0.4f
        ));

        // 流萤
        Characters.Add(new CharacterData(
            id: "CHAR_005",
            name: "流萤-神秘机甲女",
            isUnlocked: false,
            element: "YN",
            stars: "S",
            health: 1300,
            attack: 270,
            critRate: 0.09f,
            critDamage: 1.4f,
            elementBonus: 0.25f
        ));
    }

    /// <summary>
    /// 解锁角色
    /// </summary>
    public bool UnlockCharacter(string characterId)
    {
        for (int i = 0; i < Characters.Count; i++)
        {
            if (Characters[i].Id == characterId)
            {
                Characters[i].IsUnlocked = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取指定角色
    /// </summary>
    public CharacterData GetCharacter(string characterId)
    {
        return Characters.Find(c => c.Id == characterId);
    }

    /// <summary>
    /// 获取已解锁的角色列表
    /// </summary>
    public List<CharacterData> GetUnlockedCharacters()
    {
        return Characters.FindAll(c => c.IsUnlocked);
    }

    /// <summary>
    /// 获取女武神列表（排序：解锁的 > 未解锁的，按等级排序）
    /// </summary>
    public List<CharacterData> GetSortedCharacters(string Element = null)
    {
        var filteredCharacters = Element != null
            ? Characters.FindAll(t => t.BaseStats.Element == Element)
            : Characters;

        // 排序：未解锁(Locked)的在最后，完成的(Claimed)在次后，完成的(Completed)和未完成的(Unlocked)按等级排序
        filteredCharacters.Sort((a, b) =>
        {
            // 状态优先级：Unlocked > Completed > Claimed > Locked
            int statusOrderA = GetUnlockOrder(a.IsUnlocked);
            int statusOrderB = GetUnlockOrder(b.IsUnlocked);

            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前

            // 同状态按解锁等级排序
            return b.BaseStats.Level.CompareTo(a.BaseStats.Level);
        });

        return filteredCharacters;
    }

    private int GetUnlockOrder(bool status)
    {
        switch (status)
        {
            case true: return 1;
            case false: return 0;
           //default: return 0;
        }
    }

    // ================== 装备相关方法 ==================

    /// <summary>
    /// 初始化默认装备
    /// </summary>
    private void InitializeDefaultEquipment()
    {
        // 添加一些初始武器
        EquipmentBag.Add(new EquipmentData(
            id: "WEAP_001",
            name: "焢煌之钥",
            type: EquipmentType.Weapon,
            weaponType: WeaponType.DualPistols,
            health: 0,
            attack: 150,
            critRate: 0.05f,
            critDamage: 0f,
            elementBonus: 0.15f
        ));

        EquipmentBag.Add(new EquipmentData(
            id: "WEAP_002",
            name: "训练单手剑",
            type: EquipmentType.Weapon,
            weaponType: WeaponType.SingleHandedSword,
            health: 0,
            attack: 80,
            critRate: 0f,
            critDamage: 0f,
            elementBonus: 0f
        ));

        EquipmentBag.Add(new EquipmentData(
            id: "WEAP_003",
            name: "新手长枪",
            type: EquipmentType.Weapon,
            weaponType: WeaponType.Spear,
            health: 0,
            attack: 90,
            critRate: 0.02f,
            critDamage: 0f,
            elementBonus: 0f
        ));

        // 添加一些圣痕
        // 上位圣痕
        EquipmentBag.Add(new EquipmentData(
            id: "STIG_001_TOP",
            name: "燃烧之羽",
            type: EquipmentType.Stigmata,
            stigmataPosition: StigmataPosition.Top,
            health: 200,
            attack: 50,
            critRate: 0.03f,
            critDamage: 0.1f,
            elementBonus: 0.08f
        ));

        // 中位圣痕
        EquipmentBag.Add(new EquipmentData(
            id: "STIG_001_MID",
            name: "燃烧之心",
            type: EquipmentType.Stigmata,
            stigmataPosition: StigmataPosition.Middle,
            health: 150,
            attack: 60,
            critRate: 0.02f,
            critDamage: 0.15f,
            elementBonus: 0.1f
        ));

        // 下位圣痕
        EquipmentBag.Add(new EquipmentData(
            id: "STIG_001_BOT",
            name: "燃烧之足",
            type: EquipmentType.Stigmata,
            stigmataPosition: StigmataPosition.Bottom,
            health: 180,
            attack: 40,
            critRate: 0.04f,
            critDamage: 0.08f,
            elementBonus: 0.06f
        ));
    }

    /// <summary>
    /// 添加新装备到背包
    /// </summary>
    public void AddEquipment(EquipmentData equipment)
    {
        EquipmentBag.Add(equipment);
    }

    /// <summary>
    /// 移除装备
    /// </summary>
    public bool RemoveEquipment(int index)
    {
        if (index >= 0 && index < EquipmentBag.Count)
        {
            // 检查装备是否已被装备
            var equipment = EquipmentBag[index];
            if (equipment.EquippedToCharacterIndex >= 0)
            {
                return false; // 装备已被装备，不能直接移除
            }

            EquipmentBag.RemoveAt(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取指定类型的装备列表
    /// </summary>
    public List<EquipmentData> GetEquipmentByType(EquipmentType type)
    {
        return EquipmentBag.FindAll(e => e.Type == type);
    }

    /// <summary>
    /// 获取指定位置的空闲圣痕
    /// </summary>
    public List<EquipmentData> GetAvailableStigmata(StigmataPosition position)
    {
        return EquipmentBag.FindAll(e =>
            e.Type == EquipmentType.Stigmata &&
            e.StigmataPosition == position &&
            e.EquippedToCharacterIndex < 0);
    }

    /// <summary>
    /// 获取空闲的武器
    /// </summary>
    public List<EquipmentData> GetAvailableWeapons()
    {
        return EquipmentBag.FindAll(e =>
            e.Type == EquipmentType.Weapon &&
            e.EquippedToCharacterIndex < 0);
    }

    // ================== 装备管理方法 ==================

    /// <summary>
    /// 为角色装备武器
    /// </summary>
    public bool EquipWeaponToCharacter(int characterIndex, int equipmentIndex)
    {
        if (characterIndex < 0 || characterIndex >= Characters.Count ||
            equipmentIndex < 0 || equipmentIndex >= EquipmentBag.Count)
        {
            return false;
        }

        var character = Characters[characterIndex];
        var equipment = EquipmentBag[equipmentIndex];

        // 检查装备是否是武器
        if (equipment.Type != EquipmentType.Weapon)
        {
            return false;
        }

        // 检查装备是否已被其他角色装备
        if (equipment.EquippedToCharacterIndex >= 0)
        {
            return false;
        }

        // 先卸下当前装备的武器
        if (character.EquippedWeaponIndex >= 0 && character.EquippedWeaponIndex < EquipmentBag.Count)
        {
            var oldWeapon = EquipmentBag[character.EquippedWeaponIndex];
            oldWeapon.EquippedToCharacterIndex = -1;
        }

        // 装备新武器
        character.EquippedWeaponIndex = equipmentIndex;
        equipment.EquippedToCharacterIndex = characterIndex;

        return true;
    }

    /// <summary>
    /// 为角色装备圣痕
    /// </summary>
    public bool EquipStigmataToCharacter(int characterIndex, int equipmentIndex, StigmataPosition position)
    {
        if (characterIndex < 0 || characterIndex >= Characters.Count ||
            equipmentIndex < 0 || equipmentIndex >= EquipmentBag.Count)
        {
            return false;
        }

        var character = Characters[characterIndex];
        var equipment = EquipmentBag[equipmentIndex];

        // 检查装备是否是圣痕且位置正确
        if (equipment.Type != EquipmentType.Stigmata || equipment.StigmataPosition != position)
        {
            return false;
        }

        // 检查装备是否已被其他角色装备
        if (equipment.EquippedToCharacterIndex >= 0)
        {
            return false;
        }

        // 根据位置卸下当前装备的圣痕
        int currentStigmataIndex = -1;
        switch (position)
        {
            case StigmataPosition.Top:
                currentStigmataIndex = character.EquippedTopStigmataIndex;
                break;
            case StigmataPosition.Middle:
                currentStigmataIndex = character.EquippedMiddleStigmataIndex;
                break;
            case StigmataPosition.Bottom:
                currentStigmataIndex = character.EquippedBottomStigmataIndex;
                break;
        }

        if (currentStigmataIndex >= 0 && currentStigmataIndex < EquipmentBag.Count)
        {
            var oldStigmata = EquipmentBag[currentStigmataIndex];
            oldStigmata.EquippedToCharacterIndex = -1;
        }

        // 装备新圣痕
        switch (position)
        {
            case StigmataPosition.Top:
                character.EquippedTopStigmataIndex = equipmentIndex;
                break;
            case StigmataPosition.Middle:
                character.EquippedMiddleStigmataIndex = equipmentIndex;
                break;
            case StigmataPosition.Bottom:
                character.EquippedBottomStigmataIndex = equipmentIndex;
                break;
        }

        equipment.EquippedToCharacterIndex = characterIndex;

        return true;
    }

    /// <summary>
    /// 卸下角色的武器
    /// </summary>
    public bool UnequipWeaponFromCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= Characters.Count)
        {
            return false;
        }

        var character = Characters[characterIndex];
        if (character.EquippedWeaponIndex < 0)
        {
            return false;
        }

        var weapon = EquipmentBag[character.EquippedWeaponIndex];
        weapon.EquippedToCharacterIndex = -1;
        character.EquippedWeaponIndex = -1;

        return true;
    }

    /// <summary>
    /// 卸下角色的圣痕
    /// </summary>
    public bool UnequipStigmataFromCharacter(int characterIndex, StigmataPosition position)
    {
        if (characterIndex < 0 || characterIndex >= Characters.Count)
        {
            return false;
        }

        var character = Characters[characterIndex];
        int stigmataIndex = -1;

        switch (position)
        {
            case StigmataPosition.Top:
                stigmataIndex = character.EquippedTopStigmataIndex;
                character.EquippedTopStigmataIndex = -1;
                break;
            case StigmataPosition.Middle:
                stigmataIndex = character.EquippedMiddleStigmataIndex;
                character.EquippedMiddleStigmataIndex = -1;
                break;
            case StigmataPosition.Bottom:
                stigmataIndex = character.EquippedBottomStigmataIndex;
                character.EquippedBottomStigmataIndex = -1;
                break;
        }

        if (stigmataIndex >= 0 && stigmataIndex < EquipmentBag.Count)
        {
            var stigmata = EquipmentBag[stigmataIndex];
            stigmata.EquippedToCharacterIndex = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取角色当前的总属性（基础属性+装备属性）
    /// </summary>
    public CharacterStats GetCharacterTotalStats(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= Characters.Count)
        {
            return new CharacterStats();
        }

        var character = Characters[characterIndex];
        var totalStats = new CharacterStats()
        {
            Health = character.BaseStats.Health,
            Attack = character.BaseStats.Attack,
            CritRate = character.BaseStats.CritRate,
            CritDamage = character.BaseStats.CritDamage,
            ElementBonus = character.BaseStats.ElementBonus
        };

        // 加上武器属性
        if (character.EquippedWeaponIndex >= 0 && character.EquippedWeaponIndex < EquipmentBag.Count)
        {
            var weapon = EquipmentBag[character.EquippedWeaponIndex];
            totalStats.Attack += weapon.Attack;
            totalStats.CritRate += weapon.CritRate;
            totalStats.CritDamage += weapon.CritDamage;
            totalStats.ElementBonus += weapon.ElementBonus;
        }

        // 加上上位圣痕属性
        if (character.EquippedTopStigmataIndex >= 0 && character.EquippedTopStigmataIndex < EquipmentBag.Count)
        {
            var stigmata = EquipmentBag[character.EquippedTopStigmataIndex];
            totalStats.Health += stigmata.Health;
            totalStats.Attack += stigmata.Attack;
            totalStats.CritRate += stigmata.CritRate;
            totalStats.CritDamage += stigmata.CritDamage;
            totalStats.ElementBonus += stigmata.ElementBonus;
        }

        // 加上中位圣痕属性
        if (character.EquippedMiddleStigmataIndex >= 0 && character.EquippedMiddleStigmataIndex < EquipmentBag.Count)
        {
            var stigmata = EquipmentBag[character.EquippedMiddleStigmataIndex];
            totalStats.Health += stigmata.Health;
            totalStats.Attack += stigmata.Attack;
            totalStats.CritRate += stigmata.CritRate;
            totalStats.CritDamage += stigmata.CritDamage;
            totalStats.ElementBonus += stigmata.ElementBonus;
        }

        // 加上下位圣痕属性
        if (character.EquippedBottomStigmataIndex >= 0 && character.EquippedBottomStigmataIndex < EquipmentBag.Count)
        {
            var stigmata = EquipmentBag[character.EquippedBottomStigmataIndex];
            totalStats.Health += stigmata.Health;
            totalStats.Attack += stigmata.Attack;
            totalStats.CritRate += stigmata.CritRate;
            totalStats.CritDamage += stigmata.CritDamage;
            totalStats.ElementBonus += stigmata.ElementBonus;
        }

        return totalStats;
    }
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

    public CharacterData(string id, string name, bool isUnlocked, string element, string stars,
                        int health, int attack, float critRate,
                        float critDamage, float elementBonus)
    {
        Id = id;
        Name = name;
        IsUnlocked = isUnlocked;
        BaseStats = new CharacterStats()
        {
            Level = 1,
            Element = element,
            Stars = stars,
            Health = health,
            Attack = attack,
            CritRate = critRate,
            CritDamage = critDamage,
            ElementBonus = elementBonus
        };
    }
}

// ================== 装备数据类 ==================
[System.Serializable]
public class EquipmentData
{
    public string Id;                                // 装备ID
    public string Name;                              // 装备名称
    public EquipmentType Type;                       // 装备类型
    public WeaponType WeaponType;                    // 武器类型（如果是武器）
    public StigmataPosition StigmataPosition;        // 圣痕位置（如果是圣痕）

    public CharacterStats Stats;                     // 装备属性

    // 装备状态
    public int EquippedToCharacterIndex = -1;        // 被哪个角色装备（-1表示未装备）

    public EquipmentData() { }

    public EquipmentData(string id, string name, EquipmentType type,
                        WeaponType weaponType = WeaponType.None,
                        StigmataPosition stigmataPosition = StigmataPosition.None,
                        int health = 0, int attack = 0,
                        float critRate = 0f, float critDamage = 0f,
                        float elementBonus = 0f)
    {
        Id = id;
        Name = name;
        Type = type;
        WeaponType = weaponType;
        StigmataPosition = stigmataPosition;

        Stats = new CharacterStats()
        {
            Health = health,
            Attack = attack,
            CritRate = critRate,
            CritDamage = critDamage,
            ElementBonus = elementBonus
        };
    }

    public int Health => Stats.Health;
    public int Attack => Stats.Attack;
    public float CritRate => Stats.CritRate;
    public float CritDamage => Stats.CritDamage;
    public float ElementBonus => Stats.ElementBonus;
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
    public float CritRate;                           // 暴击率（0-1）
    public float CritDamage;                         // 暴击伤害（倍率，如1.5表示150%）
    public float ElementBonus;                       // 元素伤害加成（百分比，如0.3表示30%）

    public override string ToString()
    {
        return $"生命: {Health}, 攻击: {Attack}, 暴击: {CritRate:P0}, 爆伤: {CritDamage:P0}, 元素: {ElementBonus:P0}";
    }
}

// ================== 枚举定义 ==================
public enum EquipmentType
{
    Weapon,                                          // 武器
    Stigmata                                         // 圣痕
}

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

// ================== 辅助类（用于UI显示等） ==================
public static class EquipmentHelper
{
    /// <summary>
    /// 获取装备类型的中文名称
    /// </summary>
    public static string GetEquipmentTypeName(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Weapon: return "武器";
            case EquipmentType.Stigmata: return "圣痕";
            default: return "未知";
        }
    }

    /// <summary>
    /// 获取武器类型的中文名称
    /// </summary>
    public static string GetWeaponTypeName(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.DualPistols: return "双枪";
            case WeaponType.SingleHandedSword: return "单手剑";
            case WeaponType.Spear: return "长枪";
            default: return "未知";
        }
    }

    /// <summary>
    /// 获取圣痕位置的中文名称
    /// </summary>
    public static string GetStigmataPositionName(StigmataPosition position)
    {
        switch (position)
        {
            case StigmataPosition.Top: return "上位";
            case StigmataPosition.Middle: return "中位";
            case StigmataPosition.Bottom: return "下位";
            default: return "未知";
        }
    }

    /// <summary>
    /// 生成随机装备（用于测试）
    /// </summary>
    public static EquipmentData GenerateRandomEquipment()
    {
        System.Random rand = new System.Random();

        // 随机选择装备类型
        EquipmentType type = rand.Next(2) == 0 ? EquipmentType.Weapon : EquipmentType.Stigmata;

        if (type == EquipmentType.Weapon)
        {
            // 随机武器类型
            WeaponType weaponType = (WeaponType)rand.Next(1, 4);
            string[] weaponNames =
            {
                "烈焰双枪", "寒冰剑", "雷霆长枪",
                "风暴双枪", "大地剑", "流水长枪"
            };

            return new EquipmentData(
                id: $"WEAP_RAND_{DateTime.Now.Ticks}",
                name: weaponNames[rand.Next(weaponNames.Length)],
                type: EquipmentType.Weapon,
                weaponType: weaponType,
                health: 0,
                attack: rand.Next(50, 200),
                critRate: (float)rand.NextDouble() * 0.1f,
                critDamage: 0f,
                elementBonus: (float)rand.NextDouble() * 0.2f
            );
        }
        else
        {
            // 随机圣痕位置
            StigmataPosition position = (StigmataPosition)rand.Next(1, 4);
            string[] stigmataNames =
            {
                "勇气之证", "智慧印记", "力量纹章",
                "守护徽记", "疾风符印", "烈焰烙印"
            };

            return new EquipmentData(
                id: $"STIG_RAND_{DateTime.Now.Ticks}",
                name: stigmataNames[rand.Next(stigmataNames.Length)],
                type: EquipmentType.Stigmata,
                stigmataPosition: position,
                health: rand.Next(50, 200),
                attack: rand.Next(20, 80),
                critRate: (float)rand.NextDouble() * 0.05f,
                critDamage: (float)rand.NextDouble() * 0.15f,
                elementBonus: (float)rand.NextDouble() * 0.1f
            );
        }
    }
}