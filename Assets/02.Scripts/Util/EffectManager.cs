using System.Collections;
using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    public GameObject bulletHitEffect;
    public GameObject bulletFireEffect;
    public GameObject grenadeEffect;

    public void HitEffectGenenate(Vector3 position)
    {
        GameObject item = ObjectPool.Instance.DequeueObject(bulletHitEffect);
        item.transform.position = position;
        item.transform.rotation = Quaternion.identity;

        StartCoroutine(EnqueueObject(item, 0.5f));
    }
    public void FireEffectGenenate(Vector3 position, Quaternion rotation)
    {
        GameObject item = ObjectPool.Instance.DequeueObject(bulletFireEffect);
        item.transform.position = position;
        item.transform.rotation = rotation;

        StartCoroutine(EnqueueObject(item, 0.5f));
    }

    public void GrenadeEffectGenenate(Vector3 position)
    {
        GameObject item = ObjectPool.Instance.DequeueObject(grenadeEffect);
        item.transform.position = position;
        StartCoroutine(EnqueueObject(item, 3f));
    }

    IEnumerator EnqueueObject(GameObject item, float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPool.Instance.EnqueueObject(item);
    }
}
