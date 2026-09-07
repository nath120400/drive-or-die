using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offsetMin = new Vector3(0f, 6f, -12f);
    [SerializeField] private Vector3 _offsetMax = new Vector3(0f, 9f, -20f);
    [SerializeField] private float _fovMin = 60f;
    [SerializeField] private float _fovMax = 70f;
    [SerializeField] private float _smoothTime = 0.15f;

    private Vector3 _currentVelocity = Vector3.zero;

    [SerializeField] private float _progress;

    private void LateUpdate()
    {
        Vector3 offset = Vector3.Lerp(_offsetMin, _offsetMax, _progress);
        Vector3 targetPosition = _target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _smoothTime);

        Camera.main.fieldOfView = Mathf.Lerp(_fovMin, _fovMax, _progress);
    }
}
