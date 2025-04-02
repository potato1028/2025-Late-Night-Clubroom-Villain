using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera mainCamera;
    public float smoothSpeed = 5f; // 카메라 이동 속도

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}