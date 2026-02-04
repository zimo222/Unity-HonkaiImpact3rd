using JetBrains.Annotations;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ValkyrieItemUI : MonoBehaviour
{
    [Header("女武神项UI组件")]
    public Image ElementImage;
    public Image ValkyrieImage;
    public TMP_Text StatusText;

    [Header("点击区域")]
    public Button itemButton;

    [Header("ModularUIButton配置")]
    public ModularUIButton modularUIButton;

    // 私有变量
    private int num;
    private string valkyrieId;
    private ValkyrieUIController uiController;
    private CharacterData valkyrieData;
    void Start()
    {
        // 确保ModularUIButton存在
        EnsureModularUIButton();
    }

    void EnsureModularUIButton()
    {
        if (modularUIButton == null && itemButton != null)
        {
            modularUIButton = itemButton.GetComponent<ModularUIButton>();
            if (modularUIButton == null)
            {
                modularUIButton = itemButton.gameObject.AddComponent<ModularUIButton>();
            }
        }
    }

    public void Initialize(int Num, CharacterData valkyrie, ValkyrieUIController controller)
    {
        num = Num;
        valkyrieId = valkyrie.Id;
        uiController = controller;

        // 确保valkyrieData引用的是PlayerData中的实际对象，而不是副本
        if (PlayerDataManager.Instance?.CurrentPlayerData != null)
        {
            // 从当前玩家数据中获取最新的任务引用
            var playerValkyrie = PlayerDataManager.Instance.CurrentPlayerData.Characters.Find(t => t.Id == valkyrieId);
            if (playerValkyrie != null)
            {
                valkyrieData = playerValkyrie;
            }
            else
            {
                valkyrieData = valkyrie;
            }
        }
        else
        {
            valkyrieData = valkyrie;
        }

        // 确保ModularUIButton存在
        EnsureModularUIButton();

        // 更新UI元素
        UpdateValkyrieItemUI(valkyrieData);

        // 绑定事件
        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }

        // 根据任务状态设置ModularUIButton
        SetupModularUIButton(valkyrieData);
    }
    void UpdateValkyrieItemUI(CharacterData valkyrie)
    {
        if (StatusText != null)
            if (valkyrie.IsUnlocked)
            { 
                StatusText.text = "Lv." + valkyrie.BaseStats.Level;
            }
            else
            {
                StatusText.text = "LOCKED";
            }

        //Debug.Log($"Picture/Valkyrie/{valkyrie.BaseStats.Element}Back");
        //Debug.Log($"Picture/Valkyrie/CharacterCard/{valkyrie.Id}");

        ElementImage.sprite =  Resources.Load<Sprite>($"Picture/Valkyrie/ElementBack_{valkyrie.BaseStats.Element}");
        ValkyrieImage.sprite = Resources.Load<Sprite>($"Picture/Valkyrie/CharacterCard/{valkyrie.Id}");
    }

    void SetupModularUIButton(CharacterData valkyrie)
    {
        if (modularUIButton == null) return;

        // 根据任务状态配置ModularUIButton
        /*
        switch (valkyrie.IsUnlocked)
        {
        }
        */
        // 设置其他通用配置
        modularUIButton.requireConfirmation = false;
        modularUIButton.fadeDuration = 0.2f;

        // 强制刷新Inspector显示（仅在编辑器中）
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(modularUIButton);
#endif
    }

    void OnItemClicked()
    {
        if (uiController != null && valkyrieData != null)
        {
            uiController.ShowValkyrieSummary(num);
        }
    }
}