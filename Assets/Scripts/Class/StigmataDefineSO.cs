using Unity.VisualScripting;
using UnityEngine;
using static WeaponDefineSO;

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

    public Sprite icon;

    [TextArea] public string introduction;
    [TextArea] public string description;

    [System.Serializable]
    public class stigmataSkill
    {
        public string name;
        [TextArea] public string description;
    }
    public stigmataSkill[] skill;
    /*
    public GameObject weaponPrefab;
    public Sprite icon;
    */
}
