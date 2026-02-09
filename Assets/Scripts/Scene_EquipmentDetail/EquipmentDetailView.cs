using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EquipmentDetailView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ================== UI引用 ==================
    [Header("UI组件")]
    [Header("左上")]
    public TMP_Text nameText;
    public Image typeImage;
    public TMP_Text typeText;
    public Image starImage;
    public Image sstarImage;
    public GameObject equippedBadge;
    public TMP_Text equippedCharacterText;
    [Header("左中")]
    public TMP_Text descriptionText;
    [Header("左下")]
    public TMP_Text levelText;
    public TMP_Text expText;
    [Header("右上")]
    public TMP_Text stat1Text;
    public TMP_Text stat2Text;

    // 生成的位置和旋转
    [SerializeField] private string modelPath = "Prefabs/Weapon/";
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 0, 500);
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;
    // 已生成的模型引用
    private GameObject spawnedModel;

    // ================== View的公共方法 ==================
    // 更新装备信息
    public void UpdateWeaponInfo(WeaponData weapon)
    {
        SpawnModel(weapon);
        if (weapon == null) return;
        // 基本信息
        if (nameText != null) nameText.text = weapon.Name;
        if (typeImage != null) typeImage.sprite = Resources.Load<Sprite>($"Picture/Scene_EquipmentDetail/Type_{weapon.Type}");
        if (typeText != null) typeText.text = GetWeaponTypeName(weapon.Type);
        if (starImage != null) starImage.sprite = Resources.Load<Sprite>($"Picture/Scene_Equipment/Stars_{weapon.Stats.Stars}S{weapon.Stats.MaxStars}");
        if (sstarImage != null) sstarImage.sprite = Resources.Load<Sprite>($"Picture/Scene_EquipmentDetail/sStars_{weapon.Stats.SStars}");

        if (descriptionText != null) descriptionText.text = weapon.TextStats.Description;

        Debug.Log(weapon.Stats.Stars);
        Debug.Log(weapon.Stats.SStars);
        if (levelText != null) levelText.text = $"{weapon.Stats.Level}/<color=#FEDF4C>{20 * weapon.Stats.Stars + 5 * weapon.Stats.SStars - 10}</color>";
        if (expText != null) expText.text = $"{weapon.Stats.Exp}/<color=#FEDF4C>{100 * weapon.Stats.Level}</color>";
        // 装备状态
        bool isEquipped = weapon.EquippedToCharacterIndex >= 0;
        if (equippedBadge != null) equippedBadge.SetActive(isEquipped);
        if (equippedCharacterText != null)
        {
            equippedCharacterText.text = isEquipped
                ? $"已装备给: 角色{weapon.EquippedToCharacterIndex + 1}"
                : "未装备";
        }
        if (stat1Text != null) stat1Text.text = weapon.Attack.ToString();
        if (stat2Text != null) stat2Text.text = ((int)(weapon.CritRate * 100)).ToString();
    }

    // 更新玩家资源
    public void UpdatePlayerResources(PlayerData playerData)
    {
        if (playerData == null) return;
        if (staminaText != null) staminaText.text = playerData.Stamina.ToString();
        if (coinsText != null) coinsText.text = playerData.Coins.ToString();
        if (crystalsText != null) crystalsText.text = playerData.Crystals.ToString();
    }

    // 辅助方法：获取武器类型名称
    private string GetWeaponTypeName(WeaponType type)
    {
        return type switch
        {
            WeaponType.DualPistols => "双枪",
            WeaponType.SingleHandedSword => "单手剑",
            WeaponType.Spear => "长枪",
            _ => ""
        };
    }

    public void SpawnModel(WeaponData Weapon)
    {
        // 如果已有模型存在，先销毁
        if (spawnedModel != null)
        {
            Destroy(spawnedModel);
        }

        // 从Resources文件夹加载模型预设
        GameObject modelPrefab = Resources.Load<GameObject>(modelPath + Weapon.Id + Weapon.Stats.SStars.ToString());

        if (modelPrefab != null)
        {
            // 实例化模型
            spawnedModel = Instantiate(modelPrefab, spawnPosition, spawnRotation);
            spawnedModel.name = "Spawned_Model";

            // 可选：将模型设置为当前游戏对象的子物体
            // spawnedModel.transform.parent = transform;

            Debug.Log($"成功生成模型: {modelPath}");
        }
        else
        {
            Debug.LogError($"无法从路径加载模型: {modelPath}");
        }
    }
}