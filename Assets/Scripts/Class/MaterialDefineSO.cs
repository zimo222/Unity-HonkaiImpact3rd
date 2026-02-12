using Unity.VisualScripting;
using UnityEngine;

// ≤ƒ¡œ∂®“ÂSO
[CreateAssetMenu(fileName = "NewMaterial", menuName = "GameData/MaterialDefine")]
public class MaterialDefineSO : ScriptableObject
{
    public string id;
    public string name;

    public int baseStars;
    public int num;

    [TextArea] public string introduction;
    [TextArea] public string description;
    /*
    public GameObject weaponPrefab;
    public Sprite icon;
    */
}