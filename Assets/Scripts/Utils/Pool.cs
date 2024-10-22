using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public GameObject Prefab;

    public int InitalCount = 0;

    public int LeftCount => pool.Count;
    public int TotalCount => spawned.Count + pool.Count;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> spawned = new List<GameObject>();

    public IReadOnlyList<GameObject> Spawned => spawned;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        // �ʱ� ���� ��ŭ ���� ������Ʈ�� �߰��մϴ�.
        for (int i = 0; i < InitalCount; i++)
        {
            pool.Enqueue(InstantiateGO());
        }
    }

    private GameObject InstantiateGO()
    {
        GameObject GO = Instantiate(Prefab);

        // ���� ������Ʈ ���� ����
        GO.transform.SetParent(transform);
        GO.SetActive(false);

        return GO;
    }

    public GameObject Spawn()
    {
        // Ǯ ��Ͽ��� ����
        if (!pool.TryDequeue(out GameObject GO))
        {
            GO = InstantiateGO();
        }

        // ���� ��Ͽ� �߰�
        spawned.Add(GO);

        PoolUtil.Register(this, GO);

        // ���� ������Ʈ ���� ����
        GO.transform.SetParent(null);
        GO.SetActive(true);

        return GO;
    }

    internal void Despawn(GameObject GO)
    {
        // ���� ��Ͽ��� ����
        spawned.Remove(GO);

        // Ǯ ��Ͽ� �߰�
        pool.Enqueue(GO);

        // ���� ������Ʈ ���� ����
        GO.transform.SetParent(transform);
        GO.SetActive(false);
    }
}

public static class PoolUtil
{
    private static SerializedDictionary<GameObject, Pool> GOPool = new SerializedDictionary<GameObject, Pool>();

    internal static void Register(Pool pool, GameObject GO)
    {
        if (GOPool.ContainsKey(GO))
        {
            GOPool[GO] = pool;
        }
        else
        {
            GOPool.Add(GO, pool);
        }
    }

    public static void Despawn(this GameObject GO)
    {
        if (GOPool.ContainsKey(GO))
        {
            GOPool[GO].Despawn(GO);
        
            GOPool.Remove(GO);
        }
        else
        {
            MonoBehaviour.Destroy(GO);
        }
    }
}