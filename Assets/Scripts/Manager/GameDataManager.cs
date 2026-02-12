using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    public Dictionary<string, CharacterDefineSO> CharacterDict { get; private set; }
    public Dictionary<string, WeaponDefineSO> WeaponDict { get; private set; }
    public Dictionary<string, StigmataDefineSO> StigmataDict { get; private set; }
    public Dictionary<string, MaterialDefineSO> MaterialDict { get; private set; }

    void Awake()
    {
        CharacterDefineSO[] chars = Resources.LoadAll<CharacterDefineSO>("GameData/Characters");
        CharacterDict = chars.ToDictionary(c => c.id, c => c);

        WeaponDefineSO[] weapons = Resources.LoadAll<WeaponDefineSO>("GameData/Weapons");
        WeaponDict = weapons.ToDictionary(w => w.id, w => w);

        StigmataDefineSO[] stigmatas = Resources.LoadAll<StigmataDefineSO>("GameData/Stigmatas");
        StigmataDict = stigmatas.ToDictionary(w => w.id, w => w);

        MaterialDefineSO[] materials = Resources.LoadAll<MaterialDefineSO>("GameData/Materials");
        MaterialDict = materials.ToDictionary(w => w.id, w => w);
    }
}