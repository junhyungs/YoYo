using EnumTypes;
using EventLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class OnEffect : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private int _key;

    [Header("HitEffect")]
    [SerializeField] private GameObject _hitEffect;
    
    private HashSet<GameObject> _hitTarget;
    private float _power;

    private void Awake()
    {
        EventManager<PlayerEvents>.StartListening<int>(PlayerEvents.OnAttackEffect, OnHit);
        _hitTarget = new HashSet<GameObject>();
    }

    private void Start()
    {
        if (_hitEffect == null)
            return;

        ObjectPool.Instance.CreatePool(_hitEffect, 20);
    }

    public void SetPower(float power)
    {
        _power = power;
    }

    public void AttackEvent(int key)
    {
        EventManager<PlayerEvents>.TriggerEvent<int>(PlayerEvents.OnAttackEffect, key);
    }

    private void OnHit(int key)
    {
        if (_key == key)
        {
            Hit(key);
        }
    }

    //오버랩 스피어를 발생시켜서 충돌을 검사하는 메소드. (충돌이 4번씩 판정되서 일단 보류시킴.)
    private void Hit(int key) 
    {
        ClearHashSet();

        Vector3 boxSize = BoxSize(key);

        Vector3 boxPosition = transform.position + transform.forward * 1.5f;

        if(key == 3)
        {
            boxPosition = transform.position;
        }

        Collider[] colliders = Physics.OverlapBox(boxPosition, boxSize / 2, transform.rotation, LayerMask.GetMask("Enemy"));

        foreach(Collider target in colliders)
        {
            if (!_hitTarget.Contains(target.gameObject))
            {
                IDamage hit = target.GetComponent<IDamage>();

                if (hit != null)
                {
                    hit.TakeDamage(_power);

                    GameObject hitEffect = ObjectPool.Instance.DequeueObject(_hitEffect);

                    hitEffect.transform.position = target.transform.position + new Vector3(0, 1f, 0);

                    ParticleSystem hitParticle = hitEffect.GetComponent<ParticleSystem>();

                    hitParticle.Play();

                    ReturnParticle returnComponent = hitEffect.GetComponent<ReturnParticle>();

                    returnComponent.EnqueuePaticle();

                    _hitTarget.Add(target.gameObject);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 boxSize = BoxSize(1); // Adjust the key if needed
        Vector3 boxPosition = transform.position + transform.forward * 1.5f;

        // Draw a wireframe box
        Gizmos.matrix = Matrix4x4.TRS(boxPosition, transform.rotation, boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private Vector3 BoxSize(int key)
    {
        Vector3 boxSize = Vector3.zero;

        switch (key)
        {
            case 1:
                boxSize = new Vector3(3.54f, 0.18f, 3f);
                break;
            case 2:
                boxSize = new Vector3(3.54f, 0.18f, 3f);
                break;
            case 3:
                boxSize = new Vector3(4.0f, 0.6f, 4.6f);
                break;
        }

        return boxSize;
    }

    private void ClearHashSet()
    {
        _hitTarget.Clear();
    }

}
