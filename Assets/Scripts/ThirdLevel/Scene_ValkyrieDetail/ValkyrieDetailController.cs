using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ValkyrieDetailController : MonoBehaviour
{
    [Header("View引用")]
    [SerializeField] private ValkyrieDetailView viewValkyrieDetail;

    // =========================  按钮引用   =========================
    public ModularUIButton[] referencedButtons;
    public GameObject[] detailPanel;


    // ========================= 角色升级晋升 =========================
    [Header("角色升级晋升")]
    public Button levelUpButton;
    public Button promotionButton;
    public GameObject onePanel;
    public Button oneCloseButton;
    public Button sureLevelUpButton;
    public Button surePromotionButton;
    public GameObject levelUp;
    public GameObject promotion;
    public Slider slider;
    public Button[] maButton;
    private string nowMaterial = "MATE_004";

    // ========================= 武器圣痕替换 =========================
    [Header("武器圣痕替换")]
    public Button[] replaceButton;
    public GameObject twoPanel;
    public Button returnButton;
    private int nowType;
    private int nowIndex = -1;
    private int toIndex = -1;
    public Transform equipmentListContent;  // 装备/材料列表容器
    public GameObject equipmentItemPrefab;  // 装备项预制体
    public Button updateButton;
    // 新增：活动项列表
    private List<GameObject> activeItems = new List<GameObject>();

    // ================== 私有变量 ==================
    private PlayerData currentPlayerData;
    private List<ValkyrieItemUI> valkyrieItemUIs = new List<ValkyrieItemUI>();
    private int currentValkyrie = 0 == 0 ? 0 : 0;

    // Start is called before the first frame update
    void Start()
    {
        onePanel.SetActive(false);

        currentValkyrie = PlayerPrefs.GetInt("ValkyrieIndex");
        viewValkyrieDetail.InitializeUI();
        LoadPlayerData();


        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Level >= 80)
        {
            HideOnePanel();
            levelUpButton.interactable = false;
        }
        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Stars >= 3)
        {
            HideOnePanel();
            promotionButton.interactable = false;
        }

        viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);

        //角色升级晋升
        levelUpButton.onClick.AddListener(ShowLevelUp);
        promotionButton.onClick.RemoveAllListeners();
        promotionButton.onClick.AddListener(ShowPromotion);
        oneCloseButton.onClick.AddListener(HideOnePanel);
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        sureLevelUpButton.onClick.AddListener(LevelUp);
        surePromotionButton.onClick.RemoveAllListeners();
        surePromotionButton.onClick.AddListener(Promotion);
        int count = 4;
        foreach(Button btn in maButton)
        {
            int now = count;
            count--;
            btn.onClick.AddListener(() => OnMaterialButtonClick(now));
        }
        count = 0;
        foreach(Button btn in replaceButton)
        {
            int now = count;
            count++;
            btn.onClick.AddListener(() => OnReplaceButtonClick(now));
        }
        returnButton.onClick.AddListener(OnReturnButtonClick);
        updateButton.onClick.AddListener(OnUpdateButtonClick);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadPlayerData()
    {
        // 检查PlayerDataManager是否存在
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("PlayerDataManager.Instance 为空，加载默认数据");
            LoadDefaultData();
            viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);
            return;
        }

        // 获取当前玩家数据
        currentPlayerData = PlayerDataManager.Instance.CurrentPlayerData;

        // 检查是否成功获取玩家数据
        if (currentPlayerData == null)
        {
            Debug.LogWarning("当前没有登录的玩家，加载默认数据");
            LoadDefaultData();
            viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);
            return;
        }

        // 成功加载玩家数据
        Debug.Log($"成功加载玩家数据: {currentPlayerData.PlayerName}, 等级: {currentPlayerData.Level}, DailyEXP: {currentPlayerData.DailyEXP}");

        // 刷新玩家数据中的任务状态
        PlayerDataManager.Instance.RefreshTasks();
        viewValkyrieDetail.UpdateAllUI(currentPlayerData, currentValkyrie);
    }

    //加载默认玩家数据
    void LoadDefaultData()
    {
        // 创建默认数据
        currentPlayerData = new PlayerData("舰长")
        {
            Stamina = 120,
            Coins = 5000,
            Crystals = 1500
        };

        viewValkyrieDetail.UpdateAllUI(currentPlayerData,currentValkyrie);
    }

    // ==================== 角色升级晋升 ====================
    void ShowLevelUp()
    {
        // 初始化 Slider 的最大值
        if (slider != null) slider.maxValue = currentPlayerData.MaterialBag.Find(m => m.Id == nowMaterial).Count;
        onePanel.SetActive(true);
        levelUp.SetActive(true);
        promotion.SetActive(false);
        viewValkyrieDetail.UpdateLevelUpUI(currentPlayerData, currentValkyrie, nowMaterial, 0, currentPlayerData.Characters[currentValkyrie].BaseStats.Level, currentPlayerData.Characters[currentValkyrie].BaseStats.Level);
    }

    void ShowPromotion()
    {
        onePanel.SetActive(true);
        levelUp.SetActive(false);
        promotion.SetActive(true);
        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Fragments >= 50)
        {
            surePromotionButton.interactable = true;
        }
        else
        {
            surePromotionButton.interactable = false;
        }
        viewValkyrieDetail.UpdatePromotionUI(currentPlayerData, currentValkyrie);
    }

    // 当 Slider 的值被改变时调用
    private void OnSliderValueChanged(float value)
    {
        Debug.Log(value);
        // 只有当用户正在拖动时才跳转音频时间
        PlayerDataManager.Instance.CalLevelTo(currentValkyrie, nowMaterial, (int)value, out int expGain, out int toLevel, out int toExp, out int finalCost);
        slider.value = finalCost;
        viewValkyrieDetail.UpdateLevelUpUI(currentPlayerData, currentValkyrie, nowMaterial, finalCost, currentPlayerData.Characters[currentValkyrie].BaseStats.Level, toLevel);
    }

    void HideOnePanel()
    {
        onePanel.SetActive(false);
    }

    void OnMaterialButtonClick(int index)
    {
        nowMaterial = "MATE_00" + index.ToString();
        slider.maxValue = currentPlayerData.MaterialBag.Find(m => m.Id == nowMaterial).Count;
        slider.value = 0;
        viewValkyrieDetail.UpdateLevelUpUI(currentPlayerData, currentValkyrie, nowMaterial, 0, currentPlayerData.Characters[currentValkyrie].BaseStats.Level, currentPlayerData.Characters[currentValkyrie].BaseStats.Level);
    }

    void LevelUp()
    {
        PlayerDataManager.Instance.LevelUpCharacter(currentValkyrie, nowMaterial, (int)slider.value);
        viewValkyrieDetail.Update1PanelUI(currentPlayerData, currentValkyrie);
        OnSliderValueChanged(0);
        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Level >= 80)
        {
            HideOnePanel();
            levelUpButton.interactable = false;
        }
    }

    void Promotion()
    {
        PlayerDataManager.Instance.PromotionCharacter(currentValkyrie);
        viewValkyrieDetail.Update1PanelUI(currentPlayerData, currentValkyrie);

        ShowPromotion();
        if (currentPlayerData.Characters[currentValkyrie].BaseStats.Stars >= 3)
        {
            HideOnePanel();
            promotionButton.interactable = false;
        }
    }

    // ==================== 武器圣痕替换 ====================
    void OnReplaceButtonClick(int index)
    {
        twoPanel.SetActive(true);
        nowType = index;
        LoadCurrentTabContent();
        if (index == 0)
        {
            detailPanel[1].SetActive(false);
        }
        else
        {
            detailPanel[2].SetActive(false);
        }
        detailPanel[4].SetActive(false);
    }

    void OnReturnButtonClick()
    {
        ClearItemList();
        twoPanel.SetActive(false);
        if(nowType == 0)
        {
            detailPanel[1].SetActive(true);
        }
        else
        {
            detailPanel[2].SetActive(true);
        }
        detailPanel[4].SetActive(true);
    }

    //加载对应类项
    void LoadCurrentTabContent()
    {
        ClearItemList();
        switch (nowType)
        {
            case 0:
                LoadWeapons();
                nowIndex = currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex;
                break;
            case 1:
                LoadStigmatas(StigmataPosition.Top);
                nowIndex = currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex;
                break;
            case 2:
                LoadStigmatas(StigmataPosition.Middle);
                nowIndex = currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex;
                break;
            case 3:
                LoadStigmatas(StigmataPosition.Bottom);
                nowIndex = currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex;
                break;
        }
        OnEquipmentItemClicked(toIndex);
    }

    //加载武器
    void LoadWeapons()
    {
        if (currentPlayerData.WeaponBag == null) return;

        if (currentPlayerData.Characters[currentValkyrie].EquippedWeaponIndex != -1) toIndex = nowIndex;

        for (int i = 0; i < currentPlayerData.WeaponBag.Count; i++)
        {
            var weapon = currentPlayerData.WeaponBag[i];
            if (weapon.Type == currentPlayerData.Characters[currentValkyrie].WeaponType)
            {
                if (toIndex == -1) toIndex = i;
                CreateEquipmentItem(weapon, i);
            }
        }
    }

    //加载圣痕
    void LoadStigmatas(StigmataPosition nowPosition)
    {
        if (currentPlayerData.StigmataBag == null) return;

        switch (nowPosition)
        {
            case StigmataPosition.Top:
                if (currentPlayerData.Characters[currentValkyrie].EquippedTopStigmataIndex != -1) toIndex = nowIndex;
                break;
            case StigmataPosition.Middle:
                if (currentPlayerData.Characters[currentValkyrie].EquippedMiddleStigmataIndex != -1) toIndex = nowIndex;
                break;
            case StigmataPosition.Bottom:
                if (currentPlayerData.Characters[currentValkyrie].EquippedBottomStigmataIndex != -1) toIndex = nowIndex;
                break;
        }

        for (int i = 0; i < currentPlayerData.StigmataBag.Count; i++)
        {
            var stigmata = currentPlayerData.StigmataBag[i];
            if (stigmata.Position == nowPosition)
            {
                if (toIndex == -1) toIndex = i;
                CreateEquipmentItem(stigmata, i);
            }
        }
    }

    // 创建装备项 - 添加索引参数
    void CreateEquipmentItem(EquipmentData equipment, int index)
    {
        if (equipmentItemPrefab == null || equipmentListContent == null) return;
        GameObject itemObj = Instantiate(equipmentItemPrefab, equipmentListContent);
        itemObj.SetActive(true);
        DisaplayEquipmentItemPrefabs itemView = itemObj.GetComponent<DisaplayEquipmentItemPrefabs>();
        if (itemView != null)
        {
            itemView.Initialize(equipment, OnEquipmentItemClicked, index);
        }
        activeItems.Add(itemObj);
    }

    // 装备项点击
    void OnEquipmentItemClicked(int index)
    {
        Debug.Log(index);
        toIndex = index;
        Debug.Log(nowIndex);
        if(nowType == 0)
        {
            viewValkyrieDetail.UpdateEquipmentUI(nowIndex >= 0 ? currentPlayerData.WeaponBag[nowIndex] : new WeaponData(), currentPlayerData.WeaponBag[index]);
        }
        else
        {
            viewValkyrieDetail.UpdateEquipmentUI(nowIndex >= 0 ? currentPlayerData.StigmataBag[nowIndex] : new StigmataData(), currentPlayerData.StigmataBag[index]);
        }
    }

    //清空容器
    void ClearItemList()
    {
        if (equipmentListContent == null) return;
        foreach (var item in activeItems)
        {
            if (item != null)
                Destroy(item);
        }
        activeItems.Clear();
    }

    void OnUpdateButtonClick()
    {
        switch(nowType)
        {
            case 0:
                PlayerDataManager.Instance.EquipWeaponToCharacter(currentValkyrie, toIndex); 
                viewValkyrieDetail.UpdateEquipmentUI(currentPlayerData.WeaponBag[toIndex], currentPlayerData.WeaponBag[toIndex]);
                viewValkyrieDetail.Update2PanelUI(currentPlayerData, currentValkyrie);
                break;
            case 1:
                PlayerDataManager.Instance.EquipStigmataToCharacter(currentValkyrie, toIndex, StigmataPosition.Top);
                viewValkyrieDetail.UpdateEquipmentUI(currentPlayerData.StigmataBag[toIndex], currentPlayerData.StigmataBag[toIndex]);
                viewValkyrieDetail.Update3PanelUI(currentPlayerData, currentValkyrie);
                break;
            case 2:
                PlayerDataManager.Instance.EquipStigmataToCharacter(currentValkyrie, toIndex, StigmataPosition.Middle);
                viewValkyrieDetail.UpdateEquipmentUI(currentPlayerData.StigmataBag[toIndex], currentPlayerData.StigmataBag[toIndex]);
                viewValkyrieDetail.Update3PanelUI(currentPlayerData, currentValkyrie);
                break;
            case 3:
                PlayerDataManager.Instance.EquipStigmataToCharacter(currentValkyrie, toIndex, StigmataPosition.Bottom);
                viewValkyrieDetail.UpdateEquipmentUI(currentPlayerData.StigmataBag[toIndex], currentPlayerData.StigmataBag[toIndex]);
                viewValkyrieDetail.Update3PanelUI(currentPlayerData, currentValkyrie);
                break;
        }
        
    }
}
