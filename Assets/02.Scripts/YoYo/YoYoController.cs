using System.Collections;
using UnityEngine;
using EventLibrary;
using EnumTypes;

public class YoYoController : MonoBehaviour
{
    // 요요 프리팹
    public GameObject yoYoPrefab;
    // 요요를 던지는 힘
    public float throwForce = 10f;
    // 요요를 끌어당기는 시간
    public float pullDuration = 1f;
    // 로프 액션 지속 시간
    public float ropeActionDuration = 1f;
    // 요요가 충돌하지 않을 때 돌아오는 시간
    public float returnDuration = 0.5f; // 돌아오는 속도 빠르게 설정

    // 손의 위치
    public Transform handPosition;

    // 방어 여부를 나타내는 플래그
    private bool _isDefending;

    // 요요 상태 추적
    private GameObject _currentYoYo;
    private GameObject _targetEnemy;
    private bool _isYoYoAttached;
    private Coroutine _returnYoYoCoroutine;

    private void OnEnable()
    {
        EventManager<YoYoEvents>.StartListening<GameObject>(YoYoEvents.YoYoAttached, AttachYoYoToEnemy);
    }

    private void OnDisable()
    {
        EventManager<YoYoEvents>.StopListening<GameObject>(YoYoEvents.YoYoAttached, AttachYoYoToEnemy);
    }

    private void Update()
    {
        // 요요 던지기 (마우스 좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            if (_currentYoYo == null)
            {
                ThrowYoYo();
            }
            else
            {
                // 요요가 날아가는 중에 마우스 좌클릭으로 요요 복귀
                ReturnYoYoManually();
            }
        }

        // 요요로 적 끌어당기기 (키: Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_isYoYoAttached && _targetEnemy != null)
            {
                PullEnemy(_targetEnemy);
            }
            else
            {
                GameObject enemy = FindClosestEnemy();
                if (enemy != null)
                {
                    ThrowYoYoAtEnemy(enemy);
                }
            }
        }

        // 요요로 로프 액션 (키: W, 타겟 필요)
        if (Input.GetKeyDown(KeyCode.W))
        {
            GameObject target = FindRopeTarget();
            if (target != null)
            {
                InteractWithEnvironment(target);
            }
        }

        // 요요로 암살 (키: E, 가까운 적 필요)
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameObject enemy = FindClosestEnemy();
            if (enemy != null)
            {
                AssassinateEnemy(enemy);
            }
        }

        // 요요 방어 (키: R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            DefendWithYoYo();
        }
    }

    // 요요를 던지는 메소드
    public void ThrowYoYo()
    {
        _currentYoYo = Instantiate(yoYoPrefab, handPosition.position, Quaternion.identity);
        Rigidbody rb = _currentYoYo.GetComponent<Rigidbody>();

        // 마우스 위치로부터 방향 계산
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000); // 멀리까지 쏘기
        }

        Vector3 direction = (targetPoint - handPosition.position).normalized;
        rb.AddForce(direction * throwForce, ForceMode.Impulse);

        Debug.Log("요요를 마우스 방향으로 던졌습니다.");

        // 요요가 적에게 닿지 않으면 돌아오게 설정
        _returnYoYoCoroutine = StartCoroutine(CheckYoYoReturn());
    }

    private IEnumerator CheckYoYoReturn()
    {
        yield return new WaitForSeconds(returnDuration);
        if (!_isYoYoAttached && _currentYoYo != null)
        {
            StartCoroutine(ReturnYoYo());
        }
    }

    private void ReturnYoYoManually()
    {
        if (_currentYoYo != null && !_isYoYoAttached)
        {
            if (_returnYoYoCoroutine != null)
            {
                StopCoroutine(_returnYoYoCoroutine);
            }
            StartCoroutine(ReturnYoYo());
        }
    }

    private IEnumerator ReturnYoYo()
    {
        Vector3 startPosition = _currentYoYo.transform.position;
        Vector3 endPosition = handPosition.position;

        float elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            _currentYoYo.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / returnDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(_currentYoYo);
        _currentYoYo = null;

        Debug.Log("요요가 돌아왔습니다.");
    }

    // 요요를 적에게 던지는 메소드
    public void ThrowYoYoAtEnemy(GameObject enemy)
    {
        if (_currentYoYo == null)
        {
            _currentYoYo = Instantiate(yoYoPrefab, handPosition.position, Quaternion.identity);
            YoYo yoYoScript = _currentYoYo.GetComponent<YoYo>();
            Rigidbody rb = _currentYoYo.GetComponent<Rigidbody>();
            Vector3 direction = (enemy.transform.position - handPosition.position).normalized;
            rb.AddForce(direction * throwForce, ForceMode.Impulse);
            _targetEnemy = enemy;

            Debug.Log("요요를 적에게 던졌습니다.");
        }
    }

    // 요요로 적을 끌어당기는 메소드
    public void PullEnemy(GameObject enemy)
    {
        StartCoroutine(PullEnemyCoroutine(enemy));
    }

    private IEnumerator PullEnemyCoroutine(GameObject enemy)
    {
        Vector3 startPosition = enemy.transform.position;
        Vector3 endPosition = transform.position;

        float elapsedTime = 0f;
        while (elapsedTime < pullDuration)
        {
            enemy.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / pullDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _isYoYoAttached = false;
        Destroy(_currentYoYo);
        _currentYoYo = null;
        _targetEnemy = null;

        Debug.Log("적을 끌어당겼습니다.");
    }

    // 요요를 사용하여 특정 지형지물에 상호작용하는 메소드
    public void InteractWithEnvironment(GameObject target)
    {
        if (_currentYoYo == null)
        {
            LineRenderer lineRenderer = yoYoPrefab.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, handPosition.position);
            lineRenderer.SetPosition(1, target.transform.position);

            StartCoroutine(RopeActionCoroutine(target));
        }
    }

    private IEnumerator RopeActionCoroutine(GameObject target)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = target.transform.position;

        float elapsedTime = 0f;
        while (elapsedTime < ropeActionDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / ropeActionDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("로프 액션을 완료했습니다.");
    }

    // 요요 와이어로 적을 암살하는 메소드
    public void AssassinateEnemy(GameObject enemy)
    {
        Vector3 assassinationPosition = enemy.transform.position - enemy.transform.forward * 0.5f;
        transform.position = assassinationPosition;

        // animator.SetTrigger("Assassinate");

        Destroy(enemy);

        Debug.Log("적을 암살했습니다.");
    }

    // 요요를 방어용으로 사용하는 메소드
    public void DefendWithYoYo()
    {
        _isDefending = true;
        // animator.SetTrigger("Defend");

        Debug.Log("방어 모드를 활성화했습니다.");
    }

    // 공격을 막는 로직 (피격 이벤트에서 호출)
    public void OnHitByEnemy()
    {
        if (_isDefending)
        {
            Debug.Log("공격을 방어했습니다.");
        }
        else
        {
            TakeDamage();
        }
    }

    // 데미지를 받는 메소드
    private void TakeDamage()
    {
        // 데미지 처리 로직
        Debug.Log("데미지를 받았습니다.");
    }

    // 가까운 적을 찾는 메소드 (임시 구현)
    private GameObject FindClosestEnemy()
    {
        // 예제: 가장 가까운 적 찾기 (적 태그가 "Enemy"인 경우)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                closestEnemy = enemy;
                minDistance = distance;
            }
        }

        return closestEnemy;
    }

    // 로프 타겟을 찾는 메소드 (임시 구현)
    private GameObject FindRopeTarget()
    {
        // 예제: 로프 타겟 찾기 (타겟 태그가 "RopeTarget"인 경우)
        GameObject[] targets = GameObject.FindGameObjectsWithTag("RopeTarget");
        GameObject closestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < minDistance)
            {
                closestTarget = target;
                minDistance = distance;
            }
        }

        return closestTarget;
    }

    // 요요가 적에게 붙었을 때 호출되는 메소드
    public void AttachYoYoToEnemy(GameObject enemy)
    {
        _isYoYoAttached = true;
        _targetEnemy = enemy;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_currentYoYo != null && collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 forceDirection = (collision.transform.position - _currentYoYo.transform.position).normalized;
                enemyRb.AddForce(forceDirection * throwForce, ForceMode.Impulse);
            }

            Debug.Log("요요가 적을 밀어냈습니다.");

            // 요요가 적을 밀어낸 후 돌아오게 설정
            ReturnYoYoManually();
        }
    }
}
