using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ValkyrieView : MonoBehaviour
{
    // ========================= 基础玩家信息UI引用 =========================
    [Header("资源信息")]
    public TMP_Text staminaText;
    public TMP_Text coinsText;
    public TMP_Text crystalsText;

    // ==================== 左侧面板 ====================
    public TMP_Text Name1Text;
    public Image ElementImage;
    public TMP_Text ElementText;
    public TMP_Text Name2Text;
    public Image StarImage;
    public TMP_Text LevelText;
    public TMP_Text CombatPowerText;
    // ==================== 右侧面板 ====================
    public TMP_Text WeaponText;
    public Image TopStigmataImage;
    public Image MiddleStigmataImage;
    public Image BottomStigmataImage;

    // 生成的位置和旋转
    [SerializeField] private string modelPath = "Prefabs/Character/";
    [SerializeField] private Vector3 spawnPosition = new Vector3(-37, 67, 5);
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;
    // 已生成的模型引用
    private GameObject spawnedModel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeUI()
    {
        // 设置默认文本
        if (staminaText != null) staminaText.text = "0/81";
        if (coinsText != null) coinsText.text = "0";
        if (crystalsText != null) crystalsText.text = "0";
        if (Name1Text != null) Name1Text.text = "Name1";
        if (Name2Text != null) Name2Text.text = "Name2";
        if (LevelText != null) LevelText.text = "Lv.1";
        if (CombatPowerText != null) CombatPowerText.text = "0";
    }

    public void UpdateAllUI(PlayerData currentPlayerData, int currentValkyrie)
    {
            if (currentPlayerData == null) return;
            if (staminaText != null) staminaText.text = currentPlayerData.Stamina.ToString() + '/' + (currentPlayerData.Level + 80).ToString();
            if (coinsText != null) coinsText.text = currentPlayerData.Coins.ToString();
            if (crystalsText != null) crystalsText.text = currentPlayerData.Crystals.ToString();
            SpawnModel(currentPlayerData.Characters[currentValkyrie].Id);

            string[] Name = currentPlayerData.Characters[currentValkyrie].Name.Split('-');
            //UI1
            //左面板
            //上面板
            if (Name1Text != null)
                Name1Text.text = Name[0];
            if (ElementImage != null)
                ElementImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/ElementIcon_{currentPlayerData.Characters[currentValkyrie].BaseStats.Element}");
            if (ElementText != null)
                switch (currentPlayerData.Characters[currentValkyrie].BaseStats.Element)
                {
                    case "SW":
                        ElementText.text = "生物";
                        ElementText.color = new Color(1, 178 / 255.0f, 45 / 255.0f, 1);
                        break;
                    case "YN":
                        ElementText.text = "异能";
                        ElementText.color = new Color(1, 70 / 255.0f, 211 / 255.0f, 1);
                        break;
                    case "JX":
                        ElementText.text = "机械";
                        ElementText.color = new Color(43 / 255.0f, 226 / 255.0f, 1, 255);
                        break;
                }
            if (Name2Text != null)
                Name2Text.text = Name[1];
            //下面板
            if (StarImage != null)
            {
                StarImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/Stars_{currentPlayerData.Characters[currentValkyrie].BaseStats.Stars}S");
            }
            if (LevelText != null)
                LevelText.text = "Lv." + currentPlayerData.Characters[currentValkyrie].BaseStats.Level.ToString();
            //右面板
            //上面板
            if (WeaponText != null)
                WeaponText.text = currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1 ? currentPlayerData.WeaponBag[currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex].Name : "无";
            //下面板
            if (TopStigmataImage != null)
                TopStigmataImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/SymIcon/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Stigmata/SymIcon/-1");
            if (MiddleStigmataImage != null)
                MiddleStigmataImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/SymIcon/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Stigmata/SymIcon/-1");
            if (BottomStigmataImage != null)
                BottomStigmataImage.sprite = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1 ? Resources.Load<Sprite>($"Picture/Stigmata/SymIcon/{currentPlayerData.StigmataBag[currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex].Id}") : Resources.Load<Sprite>($"Picture/Stigmata/SymIcon/-1");
    }

    public void SpawnModel(string id)
    {
        // 如果已有模型存在，先销毁
        if (spawnedModel != null)
        {
            Destroy(spawnedModel);
        }

        // 从Resources文件夹加载模型预设
        GameObject modelPrefab = Resources.Load<GameObject>(modelPath + id);

        if (modelPrefab != null)
        {
            // 实例化模型
            spawnedModel = Instantiate(modelPrefab, spawnPosition, spawnRotation);
            spawnedModel.name = "Spawned_Model";

            // 可选：将模型设置为当前游戏对象的子物体
            // spawnedModel.transform.parent = transform;

            Debug.Log($"成功生成模型: {modelPath}");
            ValkyrieCameraManager cameraManager = FindObjectOfType<ValkyrieCameraManager>();
            if (cameraManager != null)
            {
                DOVirtual.DelayedCall(0.3f, () => {
                    Debug.Log("1秒后执行");
                    cameraManager.SetPlayerModelFromSpawned();
                });

                Debug.Log("成了");
            }
        }
        else
        {
            Debug.LogError($"无法从路径加载模型: {modelPath}");
        }
    }
}
