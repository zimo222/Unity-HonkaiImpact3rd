using Unity.VisualScripting;
using UnityEngine;

// ≤ƒ¡œ∂®“ÂSO
[CreateAssetMenu(fileName = "NewMaterial", menuName = "GameData/MaterialDefine")]
public class MaterialDefineSO : ScriptableObject
{
    public string id;
    public string materialName;

    public int baseStars;
    public int num;

    public Sprite icon;

    [TextArea] public string introduction;
    [TextArea] public string description;
    /*
    public GameObject weaponPrefab;
    public Sprite icon;
    */
}