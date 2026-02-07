using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerDataManager : MonoBehaviour
{
    // 单例模式，便于全局访问
    public static PlayerDataManager Instance { get; private set; }

    // 在 PlayerDataManager 类中添加（约在第20行，其他字段附近）
    public static float LastTopQuadYPosition { get; set; } = 1000f; // 默认值

    // 当前已登录的玩家数据
    public PlayerData CurrentPlayerData { get; private set; }

    // 内部存储字典
    private Dictionary<string, AccountInfo> accountDatabase = new Dictionary<string, AccountInfo>();
    private Dictionary<string, PlayerData> playerDataDatabase = new Dictionary<string, PlayerData>();
   
    // 文件存储路径
    private string accountFilePath;
    private string playerDataFilePath;

    void Awake()
    {
        // 实现简单的单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePaths();
            LoadAllDataFromDisk();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePaths()
    {
        // 将文件存储在Unity的持久数据目录中
        string directory = Application.persistentDataPath + "/PlayerData/";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        accountFilePath = directory + "Accounts.json";
        playerDataFilePath = directory + "AllPlayerData.json";
        Debug.Log($"数据存储路径: {directory}");
    }

    // === 核心公共方法 ===

    // 尝试注册
    public bool TryRegister(string username, string password)
    {
        if (accountDatabase.ContainsKey(username))
        {
            Debug.LogWarning($"用户名 '{username}' 已存在。");
            return false;
        }

        // 1. 为账户生成密码哈希（切勿存储明文密码！）
        string passwordHash = ComputeSHA256Hash(password);

        // 2. 创建新的玩家数据
        PlayerData newPlayerData = new PlayerData(username);
        playerDataDatabase[newPlayerData.PlayerID] = newPlayerData;

        // 3. 创建账户信息并关联
        AccountInfo newAccount = new AccountInfo(username, passwordHash, newPlayerData.PlayerID);
        accountDatabase[username] = newAccount;

        // 4. 立即保存到磁盘
        SaveAllDataToDisk();

        Debug.Log($"新账户注册成功: {username}");
        return true;
    }

    // 尝试登录
    public bool TryLogin(string username, string password)
    {
        if (!accountDatabase.ContainsKey(username))
        {
            Debug.LogWarning($"用户名 '{username}' 不存在。");
            return false;
        }

        AccountInfo account = accountDatabase[username];
        string inputPasswordHash = ComputeSHA256Hash(password);

        // 验证密码哈希
        if (account.PasswordHash == inputPasswordHash)
        {
            // 登录成功，加载对应的玩家数据
            if (playerDataDatabase.TryGetValue(account.LinkedPlayerDataID, out PlayerData data))
            {
                CurrentPlayerData = data;
                CurrentPlayerData.LastLoginTime = DateTime.Now;
                // 更新数据文件
                SaveAllDataToDisk();
                Debug.Log($"用户 {username} 登录成功。");
                return true;
            }
            else
            {
                Debug.LogError($"严重错误：账户 {username} 关联的玩家数据丢失！");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"用户 {username} 密码错误。");
            return false;
        }
    }

    // 保存当前玩家数据（游戏过程中可随时调用）
    public void SaveCurrentPlayerData()
    {
        if (CurrentPlayerData != null)
        {
            playerDataDatabase[CurrentPlayerData.PlayerID] = CurrentPlayerData;
            SaveAllDataToDisk();
        }
    }

    // === 内部辅助方法 ===

    // 计算SHA256哈希，用于安全存储密码
    private string ComputeSHA256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2")); // 转换为十六进制字符串
            }
            return builder.ToString();
        }
    }

    // 序列化并保存所有数据到文件
    private void SaveAllDataToDisk()
    {
        try
        {
            // 将账户字典转换为列表以便序列化
            List<AccountInfo> accountList = new List<AccountInfo>(accountDatabase.Values);
            string accountJson = JsonUtility.ToJson(new SerializationWrapper<AccountInfo>(accountList), true);
            File.WriteAllText(accountFilePath, accountJson);

            // 将玩家数据字典转换为列表以便序列化
            List<PlayerData> playerDataList = new List<PlayerData>(playerDataDatabase.Values);
            string playerDataJson = JsonUtility.ToJson(new SerializationWrapper<PlayerData>(playerDataList), true);
            File.WriteAllText(playerDataFilePath, playerDataJson);

            // Debug.Log("所有玩家数据已保存。");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存数据时出错: {e.Message}");
        }
    }

    // 从磁盘加载所有数据
    private void LoadAllDataFromDisk()
    {
        try
        {
            // 加载账户数据
            if (File.Exists(accountFilePath))
            {
                string accountJson = File.ReadAllText(accountFilePath);
                var accountWrapper = JsonUtility.FromJson<SerializationWrapper<AccountInfo>>(accountJson);
                accountDatabase.Clear();
                foreach (var account in accountWrapper.Items)
                {
                    accountDatabase[account.Username] = account;
                }
            }

            // 加载玩家数据
            if (File.Exists(playerDataFilePath))
            {
                string playerDataJson = File.ReadAllText(playerDataFilePath);
                var dataWrapper = JsonUtility.FromJson<SerializationWrapper<PlayerData>>(playerDataJson);
                playerDataDatabase.Clear();
                foreach (var data in dataWrapper.Items)
                {
                    playerDataDatabase[data.PlayerID] = data;
                }
            }
            Debug.Log("玩家数据加载完成。");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载数据时出错: {e.Message}");
        }
    }

    // === 一个辅助包装类，用于序列化列表 ===
    [System.Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> Items;
        public SerializationWrapper(List<T> items) { Items = items; }
    }
    // 在 PlayerDataManager 类中添加以下方法

    // 检查用户名是否已存在
    public bool CheckUsernameExists(string username)
    {
        return accountDatabase.ContainsKey(username);
    }

    // 获取当前登录玩家的用户名
    public string GetCurrentUsername()
    {
        return CurrentPlayerData?.PlayerName ?? "未登录";
    }

    // 登出当前用户
    public void Logout()
    {
        if (CurrentPlayerData != null)
        {
            Debug.Log($"用户 {CurrentPlayerData.PlayerName} 已登出。");
            SaveCurrentPlayerData(); // 登出前保存数据
        }
        CurrentPlayerData = null;
    }

    // 删除账户（谨慎使用！）
    public bool DeleteAccount(string username, string password)
    {
        if (!accountDatabase.ContainsKey(username))
            return false;

        AccountInfo account = accountDatabase[username];
        string inputPasswordHash = ComputeSHA256Hash(password);

        if (account.PasswordHash != inputPasswordHash)
            return false;

        // 删除玩家数据
        if (playerDataDatabase.ContainsKey(account.LinkedPlayerDataID))
        {
            playerDataDatabase.Remove(account.LinkedPlayerDataID);
        }

        // 删除账户信息
        accountDatabase.Remove(username);

        // 如果当前登录的是这个账户，则登出
        if (CurrentPlayerData != null && CurrentPlayerData.PlayerID == account.LinkedPlayerDataID)
        {
            CurrentPlayerData = null;
        }

        SaveAllDataToDisk();
        Debug.Log($"账户 {username} 已被删除。");
        return true;
    }





    // ================== 添加事件系统 ==================

    // 定义委托和事件
    public event Action<PlayerData> OnPlayerDataChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnCrystalsChanged;
    public event Action<int> OnStaminaChanged;
    public event Action<EquipmentData> OnEquipmentChanged;
    public event Action<CharacterData> OnCharacterChanged;

    // 触发事件的方法
    private void TriggerPlayerDataChanged()
    {
        OnPlayerDataChanged?.Invoke(CurrentPlayerData);
        SaveCurrentPlayerData();
    }

    private void TriggerCoinsChanged(int newValue)
    {
        OnCoinsChanged?.Invoke(newValue);
        TriggerPlayerDataChanged();
    }

    private void TriggerCrystalsChanged(int newValue)
    {
        OnCrystalsChanged?.Invoke(newValue);
        TriggerPlayerDataChanged();
    }

    private void TriggerStaminaChanged(int newValue)
    {
        OnStaminaChanged?.Invoke(newValue);
        TriggerPlayerDataChanged();
    }

    // ================== 角色相关方法 ==================
    #region 角色方法
    /// <summary>
    /// 解锁角色
    /// </summary>
    public bool UnlockCharacter(string characterId)
    {
        for (int i = 0; i < CurrentPlayerData.Characters.Count; i++)
        {
            if (CurrentPlayerData.Characters[i].Id == characterId)
            {
                CurrentPlayerData.Characters[i].IsUnlocked = true;
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
        return CurrentPlayerData.Characters.Find(c => c.Id == characterId);
    }

    /// <summary>
    /// 获取已解锁的角色列表
    /// </summary>
    public List<CharacterData> GetUnlockedCharacters()
    {
        return CurrentPlayerData.Characters.FindAll(c => c.IsUnlocked);
    }

    /// <summary>
    /// 获取女武神列表（排序：解锁的 > 未解锁的，按等级排序）
    /// </summary>
    public List<CharacterData> GetSortedCharacters(string Element = null)
    {
        var filteredCharacters = Element != null
            ? CurrentPlayerData.Characters.FindAll(t => t.BaseStats.Element == Element)
            : CurrentPlayerData.Characters;
        // 排序：未解锁的在最后，解锁的按等级排序
        filteredCharacters.Sort((a, b) =>
        {
            int statusOrderA = (a.IsUnlocked == true ? 1 : 0);
            int statusOrderB = (b.IsUnlocked == true ? 1 : 0);
            if (statusOrderA != statusOrderB)
                return statusOrderB.CompareTo(statusOrderA); // 降序排列，优先级高的在前，即解锁的在前
            return b.BaseStats.Level.CompareTo(a.BaseStats.Level);// 同状态按解锁等级排序，即等级高的在前
        });
        return filteredCharacters;
    }
    #endregion


    // ================== 装备相关方法 ==================
    #region 装备方法
    /// <summary>
    /// 添加新装备到背包
    /// </summary>
    public void AddWeapon(WeaponData weapon)
    {
        CurrentPlayerData.WeaponBag.Add(weapon);
    }
    public void AddStigmata(StigmataData stigmata)
    {
        CurrentPlayerData.StigmataBag.Add(stigmata);
    }

    /// <summary>
    /// 移除装备
    /// </summary>
    public bool RemoveWeapon(int index)
    {
        if (index >= 0 && index < CurrentPlayerData.WeaponBag.Count)
        {
            // 检查武器是否已被装备
            var weapon = CurrentPlayerData.WeaponBag[index];
            if (weapon.EquippedToCharacterIndex >= 0)
            {
                return false; // 武器已被装备，不能直接移除
            }
            CurrentPlayerData.WeaponBag.RemoveAt(index);
            return true;
        }
        return false;
    }
    public bool RemoveStigmata(int index)
    {
        if (index >= 0 && index < CurrentPlayerData.StigmataBag.Count)
        {
            // 检查圣痕是否已被装备
            var stigmata = CurrentPlayerData.StigmataBag[index];
            if (stigmata.EquippedToCharacterIndex >= 0)
            {
                return false; // 圣痕已被装备，不能直接移除
            }
            CurrentPlayerData.StigmataBag.RemoveAt(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取指定类型的装备列表
    /// </summary>
    public List<WeaponData> GetWeapon()
    {
        return CurrentPlayerData.WeaponBag;
    }
    public List<StigmataData> GetStigmata()
    {
        return CurrentPlayerData.StigmataBag;
    }

    /// <summary>
    /// 获取指定位置的空闲圣痕
    /// </summary>
    public List<StigmataData> GetAvailableStigmata(StigmataPosition position)
    {
        return CurrentPlayerData.StigmataBag.FindAll(e => e.Position == position && e.EquippedToCharacterIndex < 0);
    }

    /// <summary>
    /// 获取空闲的武器
    /// </summary>
    public List<WeaponData> GetAvailableWeapons()
    {
        return CurrentPlayerData.WeaponBag.FindAll(e => e.EquippedToCharacterIndex < 0);
    }
    #endregion


    // ================== 材料相关方法 ==================
    #region 材料方法
    #endregion


    // ================== 装备管理方法 ==================
    #region 装备管理方法
    /// <summary>
    /// 为角色装备武器
    /// </summary>
    public bool EquipWeaponToCharacter(int characterIndex, int weaponIndex)
    {
        if (characterIndex < 0 || characterIndex >= CurrentPlayerData.Characters.Count || weaponIndex < 0 || weaponIndex >= CurrentPlayerData.WeaponBag.Count)
            return false;
        var character = CurrentPlayerData.Characters[characterIndex];
        var weapon = CurrentPlayerData.WeaponBag[weaponIndex];
        // 检查武器是否已被其他角色装备
        if (weapon.EquippedToCharacterIndex >= 0)
            return false;
        // 先卸下当前装备的武器
        if (character.EquippedWeaponIndex >= 0 && character.EquippedWeaponIndex < CurrentPlayerData.WeaponBag.Count)
        {
            var oldWeapon = CurrentPlayerData.WeaponBag[character.EquippedWeaponIndex];
            oldWeapon.EquippedToCharacterIndex = -1;
        }
        // 装备新武器
        character.EquippedWeaponIndex = weaponIndex;
        weapon.EquippedToCharacterIndex = characterIndex;
        return true;
    }

    /// <summary>
    /// 为角色装备圣痕
    /// </summary>
    public bool EquipStigmataToCharacter(int characterIndex, int stigmataIndex, StigmataPosition position)
    {
        if (characterIndex < 0 || characterIndex >= CurrentPlayerData.Characters.Count || stigmataIndex < 0 || stigmataIndex >= CurrentPlayerData.StigmataBag.Count)
            return false;
        var character = CurrentPlayerData.Characters[characterIndex];
        var stigmata = CurrentPlayerData.StigmataBag[stigmataIndex];
        // 检查圣痕位置是否正确
        if (stigmata.Position != position)
            return false;
        // 检查装备是否已被其他角色装备
        if (stigmata.EquippedToCharacterIndex >= 0)
            return false;
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
        if (currentStigmataIndex >= 0 && currentStigmataIndex < CurrentPlayerData.StigmataBag.Count)
        {
            var oldStigmata = CurrentPlayerData.StigmataBag[currentStigmataIndex];
            oldStigmata.EquippedToCharacterIndex = -1;
        }
        // 装备新圣痕
        switch (position)
        {
            case StigmataPosition.Top:
                character.EquippedTopStigmataIndex = stigmataIndex;
                break;
            case StigmataPosition.Middle:
                character.EquippedMiddleStigmataIndex = stigmataIndex;
                break;
            case StigmataPosition.Bottom:
                character.EquippedBottomStigmataIndex = stigmataIndex;
                break;
        }
        stigmata.EquippedToCharacterIndex = characterIndex;
        return true;
    }

    /// <summary>
    /// 卸下角色的武器
    /// </summary>
    public bool UnequipWeaponFromCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= CurrentPlayerData.Characters.Count)
            return false;
        var character = CurrentPlayerData.Characters[characterIndex];
        if (character.EquippedWeaponIndex < 0)
            return false;
        var weapon = CurrentPlayerData.WeaponBag[character.EquippedWeaponIndex];
        weapon.EquippedToCharacterIndex = -1;
        character.EquippedWeaponIndex = -1;
        return true;
    }

    /// <summary>
    /// 卸下角色的圣痕
    /// </summary>
    public bool UnequipStigmataFromCharacter(int characterIndex, StigmataPosition position)
    {
        if (characterIndex < 0 || characterIndex >= CurrentPlayerData.Characters.Count)
            return false;
        var character = CurrentPlayerData.Characters[characterIndex];
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
        if (stigmataIndex >= 0 && stigmataIndex < CurrentPlayerData.StigmataBag.Count)
        {
            var stigmata = CurrentPlayerData.StigmataBag[stigmataIndex];
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
        if (characterIndex < 0 || characterIndex >= CurrentPlayerData.Characters.Count)
            return new CharacterStats();
        var character = CurrentPlayerData.Characters[characterIndex];
        var totalStats = new CharacterStats()
        {
            Health = character.BaseStats.Health,
            Attack = character.BaseStats.Attack,
            CritRate = character.BaseStats.CritRate,
            CritDamage = character.BaseStats.CritDamage,
            ElementBonus = character.BaseStats.ElementBonus
        };
        // 加上武器属性
        if (character.EquippedWeaponIndex >= 0 && character.EquippedWeaponIndex < CurrentPlayerData.WeaponBag.Count)
        {
            var weapon = CurrentPlayerData.WeaponBag[character.EquippedWeaponIndex];
            totalStats.Attack += weapon.Attack;
            totalStats.CritRate += weapon.CritRate;
            totalStats.CritDamage += weapon.CritDamage;
            totalStats.ElementBonus += weapon.ElementBonus;
        }
        // 加上上位圣痕属性
        if (character.EquippedTopStigmataIndex >= 0 && character.EquippedTopStigmataIndex < CurrentPlayerData.StigmataBag.Count)
        {
            var stigmata = CurrentPlayerData.StigmataBag[character.EquippedTopStigmataIndex];
            totalStats.Health += stigmata.Health;
            totalStats.Attack += stigmata.Attack;
            totalStats.CritRate += stigmata.CritRate;
            totalStats.CritDamage += stigmata.CritDamage;
            totalStats.ElementBonus += stigmata.ElementBonus;
        }
        // 加上中位圣痕属性
        if (character.EquippedMiddleStigmataIndex >= 0 && character.EquippedMiddleStigmataIndex < CurrentPlayerData.StigmataBag.Count)
        {
            var stigmata = CurrentPlayerData.StigmataBag[character.EquippedMiddleStigmataIndex];
            totalStats.Health += stigmata.Health;
            totalStats.Attack += stigmata.Attack;
            totalStats.CritRate += stigmata.CritRate;
            totalStats.CritDamage += stigmata.CritDamage;
            totalStats.ElementBonus += stigmata.ElementBonus;
        }
        // 加上下位圣痕属性
        if (character.EquippedBottomStigmataIndex >= 0 && character.EquippedBottomStigmataIndex < CurrentPlayerData.StigmataBag.Count)
        {
            var stigmata = CurrentPlayerData.StigmataBag[character.EquippedBottomStigmataIndex];
            totalStats.Health += stigmata.Health;
            totalStats.Attack += stigmata.Attack;
            totalStats.CritRate += stigmata.CritRate;
            totalStats.CritDamage += stigmata.CritDamage;
            totalStats.ElementBonus += stigmata.ElementBonus;
        }
        return totalStats;
    }
    #endregion


    // ================== 任务相关方法 ==================
    #region 任务方法
    /// <summary>
    /// 刷新任务状态（检查是否需要重置日常/周常任务）
    /// </summary>
    public void RefreshTasks()
    {
        DateTime now = DateTime.Now;
        DateTime lastCheck = CurrentPlayerData.LastTaskCheckTime;

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
            CurrentPlayerData.DailyEXP = 0; // 重置每日历练值
            ResetDailyEXPRewards(); // 重置奖励领取状态
        }

        // 刷新周常任务
        if (shouldRefreshWeekly)
        {
            ResetWeeklyTasks();
        }

        // 解锁达到等级要求的任务
        foreach (var task in CurrentPlayerData.Tasks)
        {
            if (task.Status == TaskStatus.Locked && CurrentPlayerData.Level >= task.UnlockLevel)
            {
                task.Status = TaskStatus.Unlocked;
            }
        }

        CurrentPlayerData.LastTaskCheckTime = now;
    }

    /// <summary>
    /// 重置日常任务
    /// </summary>
    private void ResetDailyTasks()
    {
        foreach (var task in CurrentPlayerData.Tasks)
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
        foreach (var task in CurrentPlayerData.Tasks)
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
        foreach (var reward in CurrentPlayerData.DailyEXPRewards)
        {
            reward.IsClaimed = false;
        }
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    public bool CompleteTask(string taskId)
    {
        var task = CurrentPlayerData.Tasks.Find(t => t.TaskID == taskId);
        if (task == null || task.Status != TaskStatus.Unlocked)
            return false;

        task.Status = TaskStatus.Completed;

        // 增加每日历练值
        CurrentPlayerData.DailyEXP += task.GetDailyEXPReward();

        return true;
    }

    /// <summary>
    /// 领取任务奖励
    /// </summary>
    public bool ClaimTaskReward(string taskId)
    {
        var task = CurrentPlayerData.Tasks.Find(t => t.TaskID == taskId);
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
        if (index < 0 || index >= CurrentPlayerData.DailyEXPRewards.Count)
            return false;

        var reward = CurrentPlayerData.DailyEXPRewards[index];
        if (reward.IsClaimed || CurrentPlayerData.DailyEXP < reward.RequiredEXP)
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
                CurrentPlayerData.Crystals += reward.Amount;
                break;
            case RewardType.Coins:
                CurrentPlayerData.Coins += reward.Amount;
                break;
            case RewardType.Stamina:
                CurrentPlayerData.Stamina += reward.Amount;
                break;
            case RewardType.DailyEXP:
                CurrentPlayerData.DailyEXP += reward.Amount;
                CurrentPlayerData.CombatEXP += reward.Amount;
                CurrentPlayerData.WeekCombatEXP += reward.Amount;
                if (CurrentPlayerData.CombatEXP >= 1000)
                {
                    CurrentPlayerData.CombatLevel++;
                    CurrentPlayerData.CombatEXP %= 1000;
                }
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
            ? CurrentPlayerData.Tasks.FindAll(t => t.Frequency == frequency.Value)
            : CurrentPlayerData.Tasks;

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
    #endregion 
}