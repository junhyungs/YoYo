using UnityEngine;
using EventLibrary;
using EnumTypes;

public class EnemyVisionAndMeshScaler : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewAngle = 60f;
    public float viewDistance = 5f;
    public float viewHeight = 15f;
    public LayerMask playerLayer;
    public int detectionResolution = 10;

    [Header("Mesh Scaler Settings")]
    private Vector3 scale;

    private EnemyAI enemyAI;
    private MeshRenderer meshRenderer;

    private static readonly int ScaleProperty = Shader.PropertyToID("_Scale");
    private static readonly int ViewAngleProperty = Shader.PropertyToID("_ViewAngle");

    private bool playerDetected = false;

    private void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateMeshProperties();
    }

    private void Update()
    {
        DetectPlayer();
        UpdateMeshProperties();
    }

    private void DetectPlayer()
    {
        bool currentlyDetected = false;

        for (float y = -viewHeight; y <= viewHeight; y += viewHeight / detectionResolution)
        {
            for (float angle = -viewAngle / 2; angle <= viewAngle / 2; angle += viewAngle / detectionResolution)
            {
                Vector3 dir = DirFromAngle(angle, false);
                Vector3 rayStart = transform.position + Vector3.up * y;
                if (Physics.Raycast(rayStart, dir, out RaycastHit hit, viewDistance, playerLayer))
                {
                    currentlyDetected = true;
                    break;
                }
            }
            if (currentlyDetected) break;
        }

        if (currentlyDetected != playerDetected)
        {
            playerDetected = currentlyDetected;
            if (playerDetected)
            {
                EventManager<EnemyEvents>.TriggerEvent(EnemyEvents.PlayerDetected);
            }
            else
            {
                EventManager<EnemyEvents>.TriggerEvent(EnemyEvents.PlayerLost);
            }
        }
    }

    private void UpdateMeshProperties()
    {
        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            scale = new Vector3(viewDistance, 1, viewDistance);
            if (Application.isPlaying)
            {
                meshRenderer.sharedMaterial.SetVector(ScaleProperty, new Vector4(scale.x, 1, scale.z, 1));
                meshRenderer.sharedMaterial.SetFloat(ViewAngleProperty, viewAngle);
            }
            meshRenderer.transform.localScale = scale;
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 원뿔의 윤곽선 그리기
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);
        // 아래쪽 윤곽선
        Gizmos.DrawLine(transform.position - Vector3.up * viewHeight,
                        transform.position - Vector3.up * viewHeight + viewAngleA * viewDistance);
        Gizmos.DrawLine(transform.position - Vector3.up * viewHeight,
                        transform.position - Vector3.up * viewHeight + viewAngleB * viewDistance);
        // 위쪽 윤곽선
        Gizmos.DrawLine(transform.position + Vector3.up * viewHeight,
                        transform.position + Vector3.up * viewHeight + viewAngleA * viewDistance);
        Gizmos.DrawLine(transform.position + Vector3.up * viewHeight,
                        transform.position + Vector3.up * viewHeight + viewAngleB * viewDistance);
        // 수직 선
        Gizmos.DrawLine(transform.position - Vector3.up * viewHeight,
                        transform.position + Vector3.up * viewHeight);
        Gizmos.DrawLine(transform.position - Vector3.up * viewHeight + viewAngleA * viewDistance,
                        transform.position + Vector3.up * viewHeight + viewAngleA * viewDistance);
        Gizmos.DrawLine(transform.position - Vector3.up * viewHeight + viewAngleB * viewDistance,
                        transform.position + Vector3.up * viewHeight + viewAngleB * viewDistance);
        // 원뿔의 위아래 범위 그리기
        for (float angle = -viewAngle / 2; angle <= viewAngle / 2; angle += viewAngle / detectionResolution)
        {
            Vector3 dir = DirFromAngle(angle, false);
            Vector3 top = transform.position + Vector3.up * viewHeight;
            Vector3 bottom = transform.position - Vector3.up * viewHeight;
            Gizmos.DrawLine(top, top + dir * viewDistance);
            Gizmos.DrawLine(bottom, bottom + dir * viewDistance);
        }
    }
}