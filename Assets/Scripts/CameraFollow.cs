using UnityEngine;

/// <summary>
/// Камера держит в кадре корабль и платформы вокруг него.
/// Смещение вверх показывает блок, на который предстоит запрыгнуть,
/// а смещение по скорости показывает кусок уровня впереди по ходу.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothTime = 0.18f;
    [SerializeField] Vector2 lookOffset = new Vector2(0f, 1.05f);
    [SerializeField] float lookAheadDistance = 2.6f;
    [SerializeField] float lookAheadSharpness = 4f;
    [SerializeField] Vector2 limitMin = new Vector2(-16f, -8f);
    [SerializeField] Vector2 limitMax = new Vector2(40f, 8f);

    Camera _camera;
    Rigidbody2D _targetBody;
    Vector3 _velocity;
    float _lookAhead;

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void Start()
    {
        if (target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
                target = player.transform;
        }

        if (target != null)
            _targetBody = target.GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (target == null || _camera == null)
            return;

        float wantedAhead = 0f;
        if (_targetBody != null)
            wantedAhead = Mathf.Clamp(_targetBody.linearVelocity.x * 0.28f, -lookAheadDistance, lookAheadDistance);

        float blend = 1f - Mathf.Exp(-lookAheadSharpness * Time.deltaTime);
        _lookAhead = Mathf.Lerp(_lookAhead, wantedAhead, blend);

        Vector3 goal = target.position + (Vector3)lookOffset;
        goal.x += _lookAhead;
        goal.z = transform.position.z;

        float halfHeight = _camera.orthographicSize;
        float halfWidth = halfHeight * _camera.aspect;
        goal.x = ClampAxis(goal.x, limitMin.x, limitMax.x, halfWidth);
        goal.y = ClampAxis(goal.y, limitMin.y, limitMax.y, halfHeight);

        transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, smoothTime);
    }

    static float ClampAxis(float value, float min, float max, float halfView)
    {
        float low = min + halfView;
        float high = max - halfView;
        if (low > high)
            return (min + max) * 0.5f;

        return Mathf.Clamp(value, low, high);
    }
}
