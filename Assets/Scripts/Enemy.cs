using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] float speed = 2f;

    Rigidbody2D _body;
    Collider2D _collider;
    bool _missed;

    void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        _body.bodyType = RigidbodyType2D.Dynamic;
        _body.gravityScale = 0f;
        _body.linearDamping = 0f;
        _body.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        _body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        if (GameManager.IsGameOver)
        {
            _body.linearVelocity = Vector2.zero;
            return;
        }

        _body.linearVelocity = Vector2.down * speed;
    }

    void Update()
    {
        if (GameManager.IsGameOver || GameManager.Instance == null)
            return;

        if (!GameManager.TryGetViewBounds(out _, out _, out float minY, out _))
            return;

        float bottom = _collider != null
            ? _collider.bounds.min.y
            : transform.position.y;

        if (bottom > minY || _missed)
            return;

        _missed = true;
        GameManager.Instance.MissEnemy();
        Destroy(gameObject);
    }
}
