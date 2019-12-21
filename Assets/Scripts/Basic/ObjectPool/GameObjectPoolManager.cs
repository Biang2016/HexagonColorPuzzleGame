using System.Collections.Generic;
using UnityEngine;

public class GameObjectPoolManager : MonoSingleton<GameObjectPoolManager>
{
    private GameObjectPoolManager()
    {
    }

    public enum PrefabNames
    {
        NormalGrid,
        Jungle,
        HQ,
        Tile
    }

    public Dictionary<PrefabNames, int> PoolConfigs = new Dictionary<PrefabNames, int>
    {
        {PrefabNames.NormalGrid, 3},
        {PrefabNames.Jungle, 3},
        {PrefabNames.HQ, 3},
        {PrefabNames.Tile, 3},
    };

    public Dictionary<PrefabNames, int> PoolWarmUpDict = new Dictionary<PrefabNames, int>
    {
    };

    public Dictionary<PrefabNames, GameObjectPool> PoolDict = new Dictionary<PrefabNames, GameObjectPool>();

    void Awake()
    {
        ResetPoolDict();
    }

    public void ResetPoolDict()
    {
        PrefabManager.Instance.LoadPrefabs_Editor();

        foreach (KeyValuePair<PrefabNames, GameObjectPool> kv in PoolDict)
        {
            if (kv.Value) DestroyImmediate(kv.Value.gameObject);
        }

        PoolDict.Clear();

        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }

        foreach (GameObject child in children)
        {
            DestroyImmediate(child);
        }

        foreach (KeyValuePair<PrefabNames, int> kv in PoolConfigs)
        {
            string prefabName = kv.Key.ToString();
            GameObject go = new GameObject("Pool_" + prefabName);
            GameObjectPool pool = go.AddComponent<GameObjectPool>();
            PoolDict.Add(kv.Key, pool);
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            PoolObject po = go_Prefab.GetComponent<PoolObject>();
            pool.Initiate(po, kv.Value);
            pool.transform.SetParent(transform);
        }
    }

    public void OptimizeAllGameObjectPools()
    {
        foreach (KeyValuePair<PrefabNames, GameObjectPool> kv in PoolDict)
        {
            kv.Value.OptimizePool();
        }
    }
}