using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

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

    // ================== 添加装备操作方法 ==================

    // 强化装备
    public bool EnhanceEquipment(int equipmentIndex, int cost = 100)
    {
        if (CurrentPlayerData == null || equipmentIndex < 0 ||
            equipmentIndex >= CurrentPlayerData.EquipmentBag.Count)
            return false;

        // 检查金币是否足够
        if (CurrentPlayerData.Coins < cost)
            return false;

        // 扣除金币
        CurrentPlayerData.Coins -= cost;

        // 强化装备
        var equipment = CurrentPlayerData.EquipmentBag[equipmentIndex];
        equipment.Stats.Level++;
        equipment.Stats.Attack += 10;
        equipment.Stats.Health += 50;

        // 触发事件
        OnEquipmentChanged?.Invoke(equipment);
        TriggerCoinsChanged(CurrentPlayerData.Coins);

        return true;
    }

    // 出售装备
    public bool SellEquipment(int equipmentIndex)
    {
        if (CurrentPlayerData == null || equipmentIndex < 0 ||
            equipmentIndex >= CurrentPlayerData.EquipmentBag.Count)
            return false;

        var equipment = CurrentPlayerData.EquipmentBag[equipmentIndex];

        // 检查是否已装备
        if (equipment.EquippedToCharacterIndex >= 0)
            return false;

        // 计算售价
        int sellPrice = CalculateSellPrice(equipment);
        CurrentPlayerData.Coins += sellPrice;

        // 移除装备
        CurrentPlayerData.EquipmentBag.RemoveAt(equipmentIndex);

        // 触发事件
        OnEquipmentChanged?.Invoke(equipment);
        TriggerCoinsChanged(CurrentPlayerData.Coins);

        return true;
    }

    // 装备武器给角色
    public bool EquipWeapon(int characterIndex, int equipmentIndex)
    {
        if (CurrentPlayerData == null) return false;

        bool success = CurrentPlayerData.EquipWeaponToCharacter(characterIndex, equipmentIndex);

        if (success)
        {
            var equipment = CurrentPlayerData.EquipmentBag[equipmentIndex];
            OnEquipmentChanged?.Invoke(equipment);

            var character = CurrentPlayerData.Characters[characterIndex];
            OnCharacterChanged?.Invoke(character);
        }

        return success;
    }

    private int CalculateSellPrice(EquipmentData equipment)
    {
        int basePrice = 100;
        int starMultiplier = equipment.Stats.Stars.Length;
        int levelMultiplier = equipment.Stats.Level;

        return basePrice * starMultiplier * levelMultiplier;
    }

    // ================== 添加便捷访问方法 ==================

    // 获取当前选择的装备（跨场景传递）
    private EquipmentData selectedEquipment;
    private int selectedEquipmentIndex = -1;

    public void SetSelectedEquipment(EquipmentData equipment, int index = -1)
    {
        selectedEquipment = equipment;
        selectedEquipmentIndex = index;
    }

    public EquipmentData GetSelectedEquipment()
    {
        return selectedEquipment;
    }

    public int GetSelectedEquipmentIndex()
    {
        return selectedEquipmentIndex;
    }

    // 获取装备列表（支持过滤）
    public List<EquipmentData> GetEquipmentByType(EquipmentType? type = null)
    {
        if (CurrentPlayerData == null) return new List<EquipmentData>();

        if (type.HasValue)
        {
            return CurrentPlayerData.EquipmentBag.FindAll(e => e.Type == type.Value);
        }

        return CurrentPlayerData.EquipmentBag;
    }

    // 获取可用武器（未装备的）
    public List<EquipmentData> GetAvailableWeapons()
    {
        if (CurrentPlayerData == null) return new List<EquipmentData>();

        return CurrentPlayerData.EquipmentBag.FindAll(e =>
            e.Type == EquipmentType.Weapon &&
            e.EquippedToCharacterIndex < 0);
    }
}