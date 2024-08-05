using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaladinWeapon : MonoBehaviour
{
    [Header("NPC_HitObject")]
    [SerializeField] private GameObject _hitParticle;

    private float _power;
    private bool canHit = true;
    private BoxCollider _hitCollider;

    void Start()
    {
        _hitCollider = GetComponent<BoxCollider>();
        ObjectPool.Instance.CreatePool(_hitParticle);
    }

    public void SetPower(float power)
    {
        _power = power;
    }

    public void OnHitCollider()
    {
        _hitCollider.enabled = true;

        canHit = true;

        Invoke(nameof(OffCollider), 1.0f);
    }

    private void OffCollider()
    {
        _hitCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player") && canHit)
        {
            DebugLogger.Log("Paladin Attack");
            
            OffCollider();

            canHit = false;

            IDamage hit = other.gameObject.GetComponent<IDamage>();
            Player player = other.gameObject.GetComponent<Player>();
            PlayerStateMachine playerStateMachine = player.PlayerStateMachine;

            if (hit != null && playerStateMachine._currentState !=
                                playerStateMachine._stateDictionary[State.SkillDefense])
            {
                GameObject hitParticle = ObjectPool.Instance.DequeueObject(_hitParticle);
                hitParticle.transform.position = other.transform.position + new Vector3(0, 1.5f, 0);
                ParticleSystem particleSystem = hitParticle.GetComponent<ParticleSystem>();
                particleSystem.Play();
                hit.TakeDamage(_power);
            }
        }
    }
}
