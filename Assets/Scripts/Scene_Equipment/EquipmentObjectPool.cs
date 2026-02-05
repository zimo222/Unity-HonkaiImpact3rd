using System.Collections.Generic;
using UnityEngine;

public class EquipmentObjectPool : MonoBehaviour
{
    private static EquipmentObjectPool instance;
    public static EquipmentObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EquipmentObjectPool>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EquipmentObjectPool");
                    instance = go.AddComponent<EquipmentObjectPool>();
                }
            }
            return instance;
        }
    }

    // 对象池字典：预制体名称 -> 对象池
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    // 预创建对象（可选）
    public void Prewarm(string prefabName, GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefabName))
        {
            pools[prefabName] = new Queue<GameObject>();
            prefabs[prefabName] = prefab;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pools[prefabName].Enqueue(obj);
        }
    }

    // 获取对象
    public GameObject GetObject(string prefabName, Transform parent = null)
    {
        if (!pools.ContainsKey(prefabName) || pools[prefabName].Count == 0)
        {
            // 池为空，需要检查是否有预制体
            if (!prefabs.ContainsKey(prefabName))
            {
                Debug.LogError($"未找到预制体: {prefabName}");
                return null;
            }

            // 创建新对象
            return CreateNewObject(prefabName, parent);
        }

        // 从池中获取对象
        GameObject obj = pools[prefabName].Dequeue();
        obj.SetActive(true);

        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
        }
        else
        {
            obj.transform.SetParent(null);
        }

        return obj;
    }

    // 返回对象
    public void ReturnObject(string prefabName, GameObject obj)
    {
        if (obj == null) return;

        if (!pools.ContainsKey(prefabName))
        {
            pools[prefabName] = new Queue<GameObject>();
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform, false);
        pools[prefabName].Enqueue(obj);
    }

    // 创建新对象
    private GameObject CreateNewObject(string prefabName, Transform parent = null)
    {
        if (!prefabs.ContainsKey(prefabName))
        {
            Debug.LogError($"未找到预制体: {prefabName}");
            return null;
        }

        GameObject prefab = prefabs[prefabName];
        GameObject obj = Instantiate(prefab, parent);
        return obj;
    }

    // 清理所有对象池
    public void ClearAllPools()
    {
        foreach (var pool in pools.Values)
        {
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                    Destroy(obj);
            }
        }
        pools.Clear();
        prefabs.Clear();
    }
}