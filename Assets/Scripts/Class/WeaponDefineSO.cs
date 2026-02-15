using Unity.VisualScripting;
using UnityEngine;

// Œ‰∆˜∂®“ÂSO
[CreateAssetMenu(fileName = "NewWeapon", menuName = "GameData/WeaponDefine")]
public class WeaponDefineSO : ScriptableObject
{
    public string id;
    public string weaponName;

    public WeaponType type;

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

    public Sprite icon;

    [TextArea] public string introduction;
    [TextArea] public string description;
    /*
    public GameObject weaponPrefab;
    public Sprite icon;
    */
}
