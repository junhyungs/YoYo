using EnumTypes;
using System.Collections;
using UnityEngine;

public interface IState
{
    void Enter();
    void ExecuteOnUpdate();
    void Exit();
}

public class EnemyIdleState : IState
{
    private EnemyAI enemyAI;
    private Coroutine returnCoroutine;
    private Coroutine rotateCoroutine;
    private bool isReturning = false;

    public EnemyIdleState(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        enemyAI.animator.SetBool(enemyAI.Idle, true);
        isReturning = true;
        returnCoroutine = enemyAI.StartCoroutine(ReturnToInitialTransform());
    }

    public void ExecuteOnUpdate()
    {
        // 필요한 경우 추가 로직
    }

    public void Exit()
    {
        enemyAI.animator.SetBool(enemyAI.Idle, false);
        if (returnCoroutine != null)
        {
            enemyAI.StopCoroutine(returnCoroutine);
        }
        if (rotateCoroutine != null)
        {
            enemyAI.StopCoroutine(rotateCoroutine);
        }
        isReturning = false;
    }

    private IEnumerator ReturnToInitialTransform()
    {
        Vector3 startPosition = enemyAI.transform.position;
        Quaternion startRotation = enemyAI.transform.rotation;
        float elapsedTime = 0f;
        float returnDuration = 2f; // 부드러운 전환을 위한 시간 (조절 가능)

        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / returnDuration;

            //enemyAI.transform.position = Vector3.Lerp(startPosition, enemyAI.initialPosition, t);
            enemyAI.transform.rotation = Quaternion.Slerp(startRotation, enemyAI.initialRotation, t);

            yield return null;
        }

        // enemyAI.transform.position = enemyAI.initialPosition;
        enemyAI.transform.rotation = enemyAI.initialRotation;

        isReturning = false;
        rotateCoroutine = enemyAI.StartCoroutine(RotateCoroutine());
    }

    private IEnumerator RotateCoroutine()
    {
        while (!isReturning)
        {
            float targetAngle = enemyAI.transform.eulerAngles.y + enemyAI.rotationAngle;
            yield return RotateToAngle(targetAngle);

            yield return new WaitForSeconds(1f);

            targetAngle = enemyAI.transform.eulerAngles.y - enemyAI.rotationAngle;
            yield return RotateToAngle(targetAngle);

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator RotateToAngle(float targetAngle)
    {
        float startAngle = enemyAI.transform.eulerAngles.y;
        float elapsedTime = 0f;
        float rotateDuration = 2f;

        while (elapsedTime < rotateDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotateDuration;
            float angle = Mathf.LerpAngle(startAngle, targetAngle, t);

            enemyAI.transform.rotation = Quaternion.Euler(enemyAI.transform.eulerAngles.x, angle, enemyAI.transform.eulerAngles.z);

            yield return null;
        }

        enemyAI.transform.rotation = Quaternion.Euler(enemyAI.transform.eulerAngles.x, targetAngle, enemyAI.transform.eulerAngles.z);
    }
}


public class EnemyAttackState : IState
{
    private EnemyAI enemyAI;
    private Transform _enemyTr;
    private Transform _playerTr;
    private EnemyFire _enemyFire;

    // 초기 회전 관련 변수
    private float initialRotationSpeed;
    private float initialFireDelay;

    // 지속적인 공격 관련 변수
    private float continuousRotationSpeed;
    private float continuousFireDelay;

    private bool isAimingAtPlayer = true;
    private float aimThreshold = 5f;
    private Coroutine fireCoroutine;

    public EnemyAttackState(EnemyAI enemyAI, float initialRotationSpeed, float initialFireDelay,
                            float continuousRotationSpeed, float continuousFireDelay)
    {
        this.enemyAI = enemyAI;
        _enemyTr = enemyAI.transform;
        _playerTr = enemyAI.playerTr;
        _enemyFire = enemyAI.enemyFire;

        this.initialRotationSpeed = initialRotationSpeed;
        this.initialFireDelay = initialFireDelay;
        this.continuousRotationSpeed = continuousRotationSpeed;
        this.continuousFireDelay = continuousFireDelay;
    }

    public void Enter()
    {
        isAimingAtPlayer = true;
        fireCoroutine = enemyAI.StartCoroutine(FireRoutine());
    }

    public void ExecuteOnUpdate()
    {
        if (isAimingAtPlayer)
        {
            AimAtPlayer(initialRotationSpeed);
        }
        else
        {
            TrackPlayer(continuousRotationSpeed);
        }
    }

    public void Exit()
    {
        if (fireCoroutine != null)
        {
            enemyAI.StopCoroutine(fireCoroutine);
        }
    }

    private void AimAtPlayer(float rotationSpeed)
    {
        Vector3 direction = (_playerTr.position - _enemyTr.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        _enemyTr.rotation = Quaternion.Slerp(_enemyTr.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(_enemyTr.rotation, targetRotation) < aimThreshold)
        {
            isAimingAtPlayer = false;
        }
    }

    private void TrackPlayer(float rotationSpeed)
    {
        Vector3 direction = (_playerTr.position - _enemyTr.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        _enemyTr.rotation = Quaternion.Slerp(_enemyTr.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator FireRoutine()
    {
        // 초기 조준 및 발사
        while (isAimingAtPlayer)
        {
            yield return null;
        }
        yield return new WaitForSeconds(initialFireDelay);
        _enemyFire.Fire();

        // 지속적인 발사
        while (true)
        {
            yield return new WaitUntil(() => !_enemyFire.isFireAnimIng);
            yield return new WaitForSeconds(continuousFireDelay);
            if (enemyAI.EnemyCurstate == EnemyState.Attack)
            {
                _enemyFire.Fire();
            }
            else
            {
                yield break;
            }
        }
    }
}

public class EnemyTraceState : IState
{
    private EnemyAI enemyAI;
    private Transform enemyTr;
    private Transform playerTr;
    private float rotationSpeed = 5f; // 회전 속도

    public EnemyTraceState(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
        this.enemyTr = enemyAI.transform;
        this.playerTr = enemyAI.playerTr;
    }

    public void Enter()
    {
        enemyAI.currentTraceTimer = 0f;
    }

    public void ExecuteOnUpdate()
    {
        // 플레이어 방향으로 회전
        Vector3 direction = (playerTr.position - enemyTr.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        enemyTr.rotation = Quaternion.Slerp(enemyTr.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 시간 측정
        enemyAI.currentTraceTimer += Time.deltaTime;
        if (enemyAI.currentTraceTimer >= enemyAI.maxTraceTime)
        {
            enemyAI.ChangeState(EnemyState.Idle);
        }
    }

    public void Exit()
    {
        enemyAI.currentTraceTimer = 0f;
    }
}




public class EnemyDieState : IState
{
    private EnemyAI enemyAI;

    public EnemyDieState(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        enemyAI.StartCoroutineDie();
    }

    public void ExecuteOnUpdate()
    {
    }

    public void Exit()
    {
    }
}

public class EnemyAssassinationState : IState
{
    private EnemyAI enemyAI;

    public EnemyAssassinationState(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        enemyAI.animator.SetBool(enemyAI.HashAssassinationDie, true);

    }

    public void ExecuteOnUpdate()
    {
    }

    public void Exit()
    {
        enemyAI.animator.SetBool(enemyAI.HashAssassinationDie, false);
    }
}

public class EnemyAssassinationFailState : IState
{
    private EnemyAI enemyAI;

    public EnemyAssassinationFailState(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        enemyAI.animator.SetBool(enemyAI.HashAssassinationFail, true);

    }

    public void ExecuteOnUpdate()
    {
    }

    public void Exit()
    {
        enemyAI.animator.SetBool(enemyAI.HashAssassinationFail, false);
    }
}

public class EnemyAssassinationDieState : IState
{
    private EnemyAI enemyAI;

    public EnemyAssassinationDieState(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        enemyAI.StartCoroutineAssainateDie();
    }

    public void ExecuteOnUpdate()
    {
    }

    public void Exit()
    {
        enemyAI.animator.SetBool(enemyAI.HashAssassinationDie, false);
    }

}

