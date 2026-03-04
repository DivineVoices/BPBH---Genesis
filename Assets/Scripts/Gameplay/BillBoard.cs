using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] Camera mainCamera;

    [Header("Rotation personnalisee (en degrees)")]
    [SerializeField] float angleX = 0f;
    [SerializeField] float angleY = 0f;
    [SerializeField] float angleZ = 0f;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void SetCamera(Camera camera)
    {
        camera = mainCamera;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.LogWarning("Main camera not assigned in Billboard.");
        }

        transform.LookAt(
            transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up
        );

        transform.Rotate(angleX, angleY, angleZ, Space.Self);
    }
}