using System.Collections;
using EnumTypes;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyFire : MonoBehaviour
{
    [PropertySpace(0f, 5f)] public Transform firePos; // 총구 포지션
    
    [FoldoutGroup("State")] public bool isFireAnimIng = false; // 현재 발사 중인지 확인하는 변수
    
    //private bool _pendingIdleState = false;

    [FoldoutGroup("Bullet")] public GameObject bullet;
    [FoldoutGroup("Bullet")] public float bulletRange = 10.0f;  // 사정거리
    [FoldoutGroup("Bullet")] public float bulletSpeed = 50f;      // 총알 속도
    [FoldoutGroup("Bullet")] public float bulletLifetime = 5f;    // 총알의 수명

    private Animator _animator;
    private Transform _playerTr;
    private EnemyAI _enemyAI;

    private readonly int _hashFire = Animator.StringToHash("Fire");

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        _animator = GetComponent<Animator>();
        _enemyAI = GetComponent<EnemyAI>();

        SphereCollider rangeCollider = gameObject.AddComponent<SphereCollider>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = bulletRange;

        _playerTr = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public IEnumerator FireAfterRotation()
    {
        yield return new WaitForSeconds(0.5f); // 0.5초 대기 후 발사
        Fire();
    }

    public void Fire()
    {
        if (isFireAnimIng) return;
        
        isFireAnimIng = true;
        FireBullet();
        _animator.SetTrigger(_hashFire);

    }

    public void FireBullet()
    {
        Vector3 targetPosition = _playerTr.position + new Vector3(0, 2f, 0);
        Vector3 direction = (targetPosition - firePos.position).normalized;
        GameObject bulletInstance = ObjectPool.Instance.DequeueObject(bullet);
        bulletInstance.SetActive(true);  // 추가된 라인
        bulletInstance.transform.position = firePos.position;
        bulletInstance.transform.rotation = Quaternion.LookRotation(direction);
        Bullet bulletScript = bulletInstance.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetShooter(this.gameObject);
            bulletScript.SetBulletLifeTime(bulletLifetime);
        }
        Rigidbody bulletRb = bulletInstance.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.velocity = direction * bulletSpeed;
        }
    }

    public void OnFireAnimationEnd()
    {
        isFireAnimIng = false;

        if (_enemyAI.EnemyCurstate == EnemyState.Attack)
        {
            _enemyAI.StartCoroutine(WaitAndFire());
        }
    }

    private IEnumerator WaitAndFire()
    {
        yield return new WaitForSeconds(2f);
        Fire();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, bulletRange);
        if (firePos != null && _playerTr != null)
        {
            Vector3 targetPosition = _playerTr.position + new Vector3(0, 2f, 0);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(firePos.position, targetPosition); // firePos에서 targetPosition까지 선 그리기
            Gizmos.DrawSphere(targetPosition, 0.2f);
        }
    }
}