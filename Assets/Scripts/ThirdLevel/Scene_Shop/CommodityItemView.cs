using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CommodityItemView : MonoBehaviour
{
    public Image rarityImage;
    public Image commodityIconImage;
    public TMP_Text countText;
    public TMP_Text nameText;
    public Image ResourceIconImage;
    public TMP_Text numText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(ShopPoolItem data)
    {
        rarityImage.color = GetColor(data.starLevel);
        commodityIconImage.sprite = data.icon;
        countText.text = "x" + data.count.ToString();
        // 尝试从角色字典查找
        if (GameDataManager.Instance.CharacterDict.TryGetValue(data.itemId, out var character))
            nameText.text = character.characterName;

        // 尝试从武器字典查找
        if (GameDataManager.Instance.WeaponDict.TryGetValue(data.itemId, out var weapon))
            nameText.text = weapon.weaponName;

        // 尝试从圣痕字典查找
        if (GameDataManager.Instance.StigmataDict.TryGetValue(data.itemId, out var stigmata))
            nameText.text = stigmata.stigmataName;

        // 尝试从材料字典查找
        if (GameDataManager.Instance.MaterialDict.TryGetValue(data.itemId, out var material))
            nameText.text = material.materialName;

        switch(data.resourceId)
        {
            case "Coin":
                {
                    ResourceIconImage.sprite = Resources.Load<Sprite>("Picture/Source/Icons/Icon_Coin");
                    break;
                }
            case "Crystal":
                {
                    ResourceIconImage.sprite = Resources.Load<Sprite>("Picture/Source/Icons/Icon_Crystal");
                    break;
                }
            case "StarStone":
                {
                    ResourceIconImage.sprite = Resources.Load<Sprite>("Picture/Source/Icons/Icon_StarStone");
                    break;
                }
            case "WitchOrb":
                {
                    ResourceIconImage.sprite = Resources.Load<Sprite>("Picture/Source/Icons/Icon_WitchOrb");
                    break;
                }
            case "PureWitchPearl":
                {
                    ResourceIconImage.sprite = Resources.Load<Sprite>("Picture/Source/Icons/Icon_PureWitchPearl");
                    break;
                }
        }
        numText.text = data.num.ToString();
    }

    Color GetColor(int stars)
    {
        // 5星橙色，4星紫色，3星蓝色，其他灰色
        if (stars == 5)
            return new Color(255 / 255.0f, 163 / 255.0f, 32 / 255.0f);
        else if (stars == 4)
            return new Color(160 / 255.0f, 79 / 255.0f, 189 / 255.0f); // 紫色
        else if (stars == 3 || stars == 2)
            return new Color(40 / 255.0f, 165 / 255.0f, 225 / 255.0f); // 蓝色
        else if (stars == 1)
            return new Color(78 / 255.0f, 179 / 255.0f, 131 / 255.0f); // 绿色
        else
            return Color.gray; // 灰色
    }
}
