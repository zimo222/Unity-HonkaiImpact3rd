using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGachaPool", menuName = "Gacha/GachaPool")]
public class GachaPoolSO : ScriptableObject
{
    public string poolName;          // 卡池名称（用于显示）
    public List<PoolItem> items;     // 卡池包含的所有物品

    [Header("填充设置（仅在编辑器中使用）")]
    public string[] upFiveStarIds;   // 在 Inspector 中填写要设为 UP 的五星 ID

    /// <summary>
    /// 从 Resources/GameData/Characters 中加载所有角色数据并填充卡池
    /// </summary>
    [ContextMenu("从角色数据填充卡池")]
    public void PopulateFromCharacterData()
    {
        // 直接加载所有角色数据，不依赖 GameDataManager 的单例
        CharacterDefineSO[] characters = Resources.LoadAll<CharacterDefineSO>("GameData/Characters");
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("未找到任何角色数据，请检查 Resources/GameData/Characters 路径");
            return;
        }

        items = new List<PoolItem>();

        foreach (var character in characters)
        {
            if (character == null) continue;

            int star = 0;
            // 根据 baseStars 映射：-1 -> 三星, 0 -> 四星, 1 -> 五星
            if (character.baseStars == -1) star = 3;
            else if (character.baseStars == 0) star = 4;
            else if (character.baseStars == 1) star = 5;
            else
            {
                Debug.LogWarning($"角色 {character.id} 的 baseStars 值 {character.baseStars} 不在预期范围内，跳过");
                continue;
            }

            PoolItem item = new PoolItem
            {
                itemId = character.id,
                itemType = ItemType.Character,
                starLevel = star,
                weight = 1, // 默认权重为1，可之后手动调整
                isUp = (star == 5 && upFiveStarIds != null && System.Array.IndexOf(upFiveStarIds, character.id) >= 0)
            };
            items.Add(item);
        }

        Debug.Log($"卡池 {poolName} 已从角色数据填充，共 {items.Count} 个物品");
    }

    /// <summary>
    /// 从 Resources/GameData/Weapons 中加载所有武器数据并填充卡池
    /// </summary>
    [ContextMenu("从武器数据填充卡池")]
    public void PopulateFromWeaponData()
    {
        // 直接加载所有武器数据，不依赖 GameDataManager 的单例
        WeaponDefineSO[] weapons = Resources.LoadAll<WeaponDefineSO>("GameData/Weapons");
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("未找到任何武器数据，请检查 Resources/GameData/Weapons 路径");
            return;
        }

        items = new List<PoolItem>();

        foreach (var weapon in weapons)
        {
            if (weapon == null) continue;

            int star = 0;
            if (weapon.baseStars == 1 || weapon.baseStars == 2) star = 3;
            else if (weapon.baseStars == 3) star = 4;
            else if (weapon.baseStars == 4) star = 5;
            else
            {
                Debug.LogWarning($"角色 {weapon.id} 的 baseStars 值 {weapon.baseStars} 不在预期范围内，跳过");
                continue;
            }

            PoolItem item = new PoolItem
            {
                itemId = weapon.id,
                itemType = ItemType.Weapon,
                starLevel = star,
                weight = 1, // 默认权重为1，可之后手动调整
                isUp = (star == 5 && upFiveStarIds != null && System.Array.IndexOf(upFiveStarIds, weapon.id) >= 0)
            };
            items.Add(item);
        }

        Debug.Log($"卡池 {poolName} 已从武器数据填充，共 {items.Count} 个物品");
    }
}