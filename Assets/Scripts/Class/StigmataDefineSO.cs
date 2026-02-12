using Unity.VisualScripting;
using UnityEngine;

//  •∫€∂®“ÂSO
[CreateAssetMenu(fileName = "NewStigmata", menuName = "GameData/StigmataDefine")]
public class StigmataDefineSO : ScriptableObject
{
    public string id;
    public string stigmataName;

    public StigmataPosition Position;

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

    [TextArea] public string introduction;
    [TextArea] public string description;
    /*
    public GameObject weaponPrefab;
    public Sprite icon;
    */
}
