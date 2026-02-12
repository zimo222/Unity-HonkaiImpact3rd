// ½ÇÉ«¶¨ÒåSO
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "GameData/CharacterDefine")]
public class CharacterDefineSO : ScriptableObject
{
    public string id;
    public string characterName;

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

    public string Id => id;
}
