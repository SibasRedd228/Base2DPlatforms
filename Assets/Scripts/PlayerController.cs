using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Корабль игрока: бег по платформам, прыжок и выстрел вверх с перезарядкой.
/// Точка выстрела — дочерний FirePoint, поэтому пуля вылетает оттуда, где стоит корабль.
/// Проект на Input System: клавиши читаются через Keyboard и Mouse.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 8.5f;
    [SerializeField] float jumpSpeed = 11f;
    [SerializeField] float gravityScale = 2.5f;
    [SerializeField] float fireCooldown = 0.35f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float bulletSpawnOffset = 1.2f;
    [SerializeField] Transform firePoint;
    [SerializeField] LayerMask groundMask;

    Rigidbody2D _body;
    Collider2D _collider;
    Vector2 _spawnPoint;
    float _nextFireTime;
    float _horizontal;
    bool _jumpPressed;

    void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        // Гравитация включена: по Y корабль стоит на блоках, а не висит в воздухе.
        _body.bodyType = RigidbodyType2D.Dynamic;
        _body.gravityScale = gravityScale;
        _body.linearDamping = 0f;
        _body.freezeRotation = true;
        _body.constraints = RigidbodyConstraints2D.FreezeRotation;
        _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        _spawnPoint = _body.position;
    }

    void Update()
    {
        if (GameManager.IsGameOver)
        {
            _horizontal = 0f;
            _jumpPressed = false;
            return;
        }

        _horizontal = ReadHorizontal();

        if (ReadJumpPressed())
            _jumpPressed = true;

        if (ReadFirePressed() && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + fireCooldown;
            Shoot();
        }
    }

    void FixedUpdate()
    {
        if (GameManager.IsGameOver)
        {
            _body.linearVelocity = Vector2.zero;
            return;
        }

        bool grounded = CheckGrounded();
        float verticalSpeed = _body.linearVelocity.y;

        if (_jumpPressed && grounded)
            verticalSpeed = jumpSpeed;

        _jumpPressed = false;

        // На земле гасим остаточную скорость вниз, чтобы корабль не дрожал в полу.
        if (grounded && verticalSpeed < 0f)
            verticalSpeed = 0f;

        _body.linearVelocity = new Vector2(_horizontal * moveSpeed, verticalSpeed);
        ClampToLevel();

        // Промах в зазор между блоками роняет корабль ниже уровня. Возвращаем его на старт.
        if (_body.position.y < -9f)
        {
            _body.position = _spawnPoint;
            _body.linearVelocity = Vector2.zero;
        }
    }

    float ReadHorizontal()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return 0f;

        float value = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            value -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            value += 1f;

        return Mathf.Clamp(value, -1f, 1f);
    }

    bool ReadJumpPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return false;

        return keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
    }

    bool ReadFirePressed()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool space = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        bool click = mouse != null && mouse.leftButton.wasPressedThisFrame;
        return space || click;
    }

    void Shoot()
    {
        if (bulletPrefab == null)
            return;

        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * bulletSpawnOffset;

        Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
    }

    bool CheckGrounded()
    {
        if (_collider == null)
            return false;

        // Узкая зона сразу под ногами. Слой Ground — только блоки, не враги и не пули.
        Vector2 origin = new Vector2(_collider.bounds.center.x, _collider.bounds.min.y - 0.06f);
        Vector2 size = new Vector2(_collider.bounds.size.x * 0.65f, 0.12f);
        Collider2D hit = Physics2D.OverlapBox(origin, size, 0f, groundMask);
        return hit != null && hit != _collider && !hit.isTrigger;
    }

    void ClampToLevel()
    {
        if (!GameManager.TryGetViewBounds(out float minX, out float maxX, out _, out _))
            return;

        float halfWidth = _collider != null ? _collider.bounds.extents.x : 0.4f;
        Vector2 position = _body.position;
        float clampedX = Mathf.Clamp(position.x, minX + halfWidth, maxX - halfWidth);

        if (!Mathf.Approximately(position.x, clampedX))
        {
            position.x = clampedX;
            _body.position = position;
            _body.linearVelocity = new Vector2(0f, _body.linearVelocity.y);
        }
    }
}
