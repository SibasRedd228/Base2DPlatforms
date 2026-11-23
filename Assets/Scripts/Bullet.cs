using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 12f;

    Rigidbody2D _body;

    void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
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
        _body.linearVelocity = Vector2.up * speed;
    }

    void Update()
    {
        if (!GameManager.TryGetViewBounds(out _, out _, out _, out float maxY))
            return;

        if (transform.position.y > maxY + 1f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null)
            return;

        if (!GameManager.IsGameOver && HudView.Instance != null)
            HudView.Instance.AddScore(1);

        Destroy(enemy.gameObject);
        Destroy(gameObject);
    }
}
