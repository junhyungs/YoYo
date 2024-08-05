using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnParticle : MonoBehaviour
{

    public void EnqueuePaticle()
    {
        StartCoroutine(Return());
    }

    private IEnumerator Return()
    {
        yield return new WaitForSeconds(1.5f);

        ObjectPool.Instance.EnqueueObject(this.gameObject);
    }
}
