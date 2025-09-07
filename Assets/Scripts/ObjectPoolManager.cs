using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string key;
        public GameObject prefab;
        public int initialSize = 5;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.key, objectQueue);
        }
    }

    public GameObject GetObject(string key, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"Pool with key {key} doesn't exist!");
            return null;
        }

        Queue<GameObject> objectQueue = poolDictionary[key];

        GameObject obj;
        if (objectQueue.Count > 0 && !objectQueue.Peek().activeSelf)
        {
            obj = objectQueue.Dequeue();
        }
        else
        {
            // Tạo thêm nếu không đủ
            var pool = pools.Find(p => p.key == key);
            if (pool == null) return null;

            obj = Instantiate(pool.prefab);
        }

        obj.SetActive(true);
        obj.transform.SetPositionAndRotation(position, rotation);

        return obj;
    }
    public bool HasActive(string key)
    {
        if (!poolDictionary.ContainsKey(key)) return false;
        foreach (var obj in poolDictionary[key])
        {
            if (obj.activeInHierarchy) return true;
        }
        return false;
    }

    public void ReturnObject(string key, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"Pool with key {key} doesn't exist!");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj);
    }
}
