using System.Collections.Generic;
using UnityEngine;

public static class QueueExtensions
{
    // 오브젝트 풀에 오브젝트를 추가하는 확장 메서드
    public static void EnqueuePool<T>(this Queue<T> queue, T item) where T : Component
    {
        item.gameObject.SetActive(false);
        queue.Enqueue(item);
    }

    // 오브젝트 풀에서 오브젝트를 가져오는 확장 메서드
    public static T DequeuePool<T>(this Queue<T> queue) where T : Component
    {
        if (queue.Count == 0)
        {
            Debug.LogWarning("Queue.Count == 0");
            return null;
        }

        T item = queue.Dequeue();
        item.gameObject.SetActive(true);
        return item;
    }
}

public class Pool   //  풀 관리 클래스
{
    public Queue<Component> queue;
    public int count;
    public Transform transform;

    public Pool(Transform transform)
    {
        queue = new Queue<Component>();
        count = 0;
        this.transform = transform;
    }
}

public class ObjectPool : Singleton<ObjectPool>
{
    // 오브젝트를 풀링 형식으로 사용할 수 있도록 도와주는 싱글톤 클래스
    private Dictionary<string, Pool> objectPools = new Dictionary<string, Pool>();
    // 생성할 프리팹 타입은 string으로 검사
    // 이름이 같으면 동일한 프리팹으로 취급하기 때문에 이름 설정에 주의

    public void CreatePool(GameObject prefab, int count = 100) //풀을 count만큼 생성.
    {
        string itemType = prefab.name;
        if (!objectPools.ContainsKey(itemType)) //  키가 없을 경우
        {
            // 풀을 생성할 트랜스폼을 추가하고, 생성한 풀은 딕셔너리에 추가
            GameObject pool = new GameObject();
            pool.transform.SetParent(this.transform);
            pool.name = prefab.name + "Pool";
            objectPools.Add(itemType, new Pool(pool.transform));
        }

        for (int i = 0; i < count; i++) // count만큼 프리팹 오브젝트를 생성해서 인큐
        {
            GameObject item = Instantiate(prefab, objectPools[itemType].transform);
            item.name = itemType;
            objectPools[itemType].queue.EnqueuePool(item.GetComponent<Component>());
            objectPools[itemType].count++;
        }
    }

    public void EnqueueObject(GameObject item)  //사용한 것은 큐에 다시 인큐, Destroy를 대체
    {
        string itemType = item.name;
        if (!objectPools.ContainsKey(itemType)) // 키가 없는 경우
        {
            CreatePool(item);   // 자동으로 풀을 생성
        }
        item.transform.SetParent(objectPools[itemType].transform);
        objectPools[itemType].queue.EnqueuePool(item.GetComponent<Component>());
    }

    public void AllDestroyObject(GameObject prefab) // prefab과 같은 타입의 모든 오브젝트를 큐에 다시 인큐
    {
        string itemType = prefab.name;
        if (!objectPools.ContainsKey(itemType)) // 키가 없는 경우
        {
            CreatePool(prefab); // 자동으로 풀을 생성
        }

        for (int i = 0; i < objectPools[itemType].transform.childCount; i++)    // 키 값에 해당하는 트랜스폼의 모든 아이템 검사
        {
            GameObject item = objectPools[itemType].transform.GetChild(i).gameObject;
            if (item.activeSelf)
            {
                EnqueueObject(item);    // 활성화되어있을 경우 디큐
            }
        }
    }

    public GameObject DequeueObject(GameObject prefab)  // 사용할 오브젝트를 반환 Instantiate를 대체
    {
        string itemType = prefab.name;
        if (!objectPools.ContainsKey(itemType)) // 키가 없는 경우
        {
            CreatePool(prefab); // 자동으로 풀을 생성
        }

        Component? dequeneObject = objectPools[itemType].queue.DequeuePool();

        // 디큐 시도, 큐에 있는 모든 아이템이 사용 중일경우 null을 반환
        if (dequeneObject != null)  // 큐에 오브젝트가 있을 경우
        {
            return dequeneObject.gameObject;    // 해당 오브젝트를 반환
        }
        else // 큐에 내용물이 없는 경우
        {
            CreatePool(prefab, objectPools[itemType].count);    // 풀 확장
            return DequeueObject(prefab);   // 추가한 풀에서 디큐
        }
    }
}