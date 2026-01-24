using System;
using System.Collections.Generic;
using UnityEditor;

// ================== 任务奖励类型 ==================
public enum RewardType
{
    None,            // 无奖励
    Crystals,        // 水晶
    Coins,           // 金币
    Stamina,         // 体力
    EXP,              //经验
    DailyEXP,        // 每日历练值
    Equipment,       // 装备
    CharacterFragment, // 角色碎片
    BattlePassEXP,   // 作战凭证经验
    Materials       // 材料
}

// ================== 任务奖励结构 ==================
[System.Serializable]
public struct TaskReward
{
    public RewardType Type;
    public int Amount;
    public string ItemId; // 如果是装备或角色碎片，需要指定ID

    public TaskReward(RewardType type, int amount, string itemId = "")
    {
        Type = type;
        Amount = amount;
        ItemId = itemId;
    }
}

// ================== 任务类型 ==================
public enum TaskFrequency
{
    Daily,           // 日常任务
    Weekly,          // 周常任务
    Achievement      // 成就任务
}

// ================== 任务状态 ==================
public enum TaskStatus
{
    Locked,          // 未解锁
    Unlocked,        // 已解锁未完成
    Completed,       // 已完成未领取
    Claimed          // 已领取
}

// ================== 任务数据类 ==================
[System.Serializable]
public class TaskData
{
    public string TaskID;              // 任务唯一ID
    public string TaskName;            // 任务名称
    public int UnlockLevel;            // 解锁等级
    public TaskFrequency Frequency;    // 任务类型
    public TaskStatus Status;          // 任务状态
    public int maxTimes;
    public int nowTimes;

    // 任务奖励（最多两个）
    public TaskReward Reward1;
    public TaskReward Reward2;

    // 任务描述
    public string Description;

    // 任务相关参数
    public string SceneName;           // 跳转场景
    public string BattleType;          // 战斗类型

    // 时间相关
    public DateTime LastCheckTime;     // 上次查看时间

    public TaskData() { }

    public TaskData(int level, string id, string name, int unlockLevel, TaskFrequency frequency,
                    TaskReward reward1, TaskReward reward2, int maxTime,  string description = "",
                    string sceneName = "", string battleType = "")
    {
        TaskID = id;
        TaskName = name;
        UnlockLevel = unlockLevel;
        Frequency = frequency;
        Status = unlockLevel <= 10 ? TaskStatus.Completed : (unlockLevel <= level ? TaskStatus.Unlocked : TaskStatus.Locked);
        Reward1 = reward1;
        Reward2 = reward2;
        nowTimes = (Status == TaskStatus.Claimed || Status == TaskStatus.Completed ?  maxTime : 0);
        maxTimes = maxTime;
        Description = description;
        SceneName = sceneName;
        BattleType = battleType;
        LastCheckTime = DateTime.Now;
    }

    // 获取奖励中的每日历练值
    public int GetDailyEXPReward()
    {
        int exp = 0;
        if (Reward1.Type == RewardType.DailyEXP) exp += Reward1.Amount;
        if (Reward2.Type == RewardType.DailyEXP) exp += Reward2.Amount;
        return exp;
    }
}

// ================== 每日历练值奖励 ==================
[System.Serializable]
public class DailyEXPReward
{
    public int RequiredEXP;    // 所需历练值
    public TaskReward Reward;  // 奖励
    public bool IsClaimed;     // 是否已领取

    public DailyEXPReward(int exp, TaskReward reward)
    {
        RequiredEXP = exp;
        Reward = reward;
        IsClaimed = false;
    }
}