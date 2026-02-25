// ½ÇÉ«¶¨ÒåSO
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "GameData/CharacterDefine")]
public class CharacterDefineSO : ScriptableObject
{
    public string id;
    public string characterName;
    public WeaponType weaponType;

    public string element;

    public int baseStars;
    public int maxStars;

    public int baseHealth;
    public int baseAttack;
    public int baseDefence;


    public int baseEnergy;
    public float baseCritRate;
    public float baseCritDamage;
    public float baseElementBonus;

    public SkillData[] skills = new SkillData[6];

    [TextArea] public string introduction;
    [TextArea] public string description;

    public Sprite icon;
    public Sprite illustration;
    public GameObject model;
    public Sprite[] skillIcon;

    public string Id => id;
}
