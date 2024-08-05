using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CineMachineCamera : MonoBehaviour
{
    [Header("PlayerCameraPrefab")]
    [SerializeField] private GameObject virtualCameraPrefab;

    [Header("Player_Look")]
    [SerializeField] private GameObject lookObject;

    [Header("CameraFollowOffSet")]
    [SerializeField] private Vector3 virtualCameraOffSet;

    [Header("CameraStartRotarion")]
    [SerializeField] private Quaternion cameraStartRotarion;

    [Header("LerpSpeed")]
    [SerializeField] private float lerpSpeed;

    [Header("XDamping")]
    [SerializeField] private float xDamping;

    [Header("YDamping")]
    [SerializeField] private float yDamping;

    [Header("ZDamping")]
    [SerializeField] private float zDamping;

    private CinemachineVirtualCamera virtualCamera;
    private GameObject playerCamera;
    private float virtualCameraMaxDistance = 10.0f;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        CameraMovement();
    }

    private void Init()
    {
        playerCamera = Instantiate(virtualCameraPrefab);

        playerCamera.transform.rotation = cameraStartRotarion;

        virtualCamera = playerCamera.GetComponent<CinemachineVirtualCamera>();

        virtualCamera.Follow = transform;
        virtualCamera.LookAt = transform;

        var doNothing = virtualCamera.GetCinemachineComponent<CinemachineComposer>();

        if (doNothing != null)
        {
            Destroy(doNothing);
        }

        var transPoser = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        if(transPoser != null)
        {
            transPoser.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            transPoser.m_FollowOffset = virtualCameraOffSet;
            transPoser.m_XDamping = xDamping;
            transPoser.m_YDamping = yDamping;
            transPoser.m_ZDamping = zDamping;
        }
    }

    private void CameraMovement()
    {
        bool isKeyDown = Input.anyKey;

        switch (isKeyDown)
        {
            case true:
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    CameraRayPosition();
                    virtualCamera.Follow = lookObject.transform;
                    virtualCamera.LookAt = lookObject.transform;
                }
                break;
            case false:
                virtualCamera.Follow = transform;
                virtualCamera.LookAt = transform;
                break;
        }
    }

    private void CameraRayPosition()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 rayPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);

            Vector3 moveDirection = (rayPos - transform.position).normalized;

            float distanceTolook = Vector3.Distance(transform.position, rayPos);

            if(distanceTolook > virtualCameraMaxDistance)
            {
                rayPos = transform.position + moveDirection * virtualCameraMaxDistance;
            }

            lookObject.transform.position = Vector3.Lerp(lookObject.transform.position, rayPos, lerpSpeed * Time.deltaTime);
        }
    }
}
