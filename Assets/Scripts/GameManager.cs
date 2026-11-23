using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static bool IsGameOver { get; private set; }

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject gameOverRoot;
    [SerializeField] float spawnIntervalMin = 1f;
    [SerializeField] float spawnIntervalMax = 2f;

    [SerializeField] Vector2 levelMin = new Vector2(-9f, -6f);
    [SerializeField] Vector2 levelMax = new Vector2(9f, 5.5f);
    [SerializeField] bool useLevelBounds = true;

    Coroutine _spawnRoutine;
    int _lives = 3;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsGameOver = false;
        Time.timeScale = 1f;

        if (gameOverRoot != null)
            gameOverRoot.SetActive(false);
    }

    void Start()
    {
        if (HudView.Instance != null)
            _lives = Mathf.Max(0, HudView.Instance.HeartSlotCount);

        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;
        Time.timeScale = 1f;
    }

    IEnumerator SpawnLoop()
    {
        while (!IsGameOver)
        {
            float wait = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(wait);

            if (IsGameOver)
                yield break;

            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
            return;

        if (!TryGetViewBounds(out float minX, out float maxX, out _, out float maxY))
            return;

        float halfWidth = 0.5f;
        float halfHeight = 0.5f;
        var renderer = enemyPrefab.GetComponent<SpriteRenderer>();
        if (renderer != null && renderer.sprite != null)
        {
            Vector3 scale = enemyPrefab.transform.localScale;
            halfWidth = renderer.sprite.bounds.extents.x * scale.x;
            halfHeight = renderer.sprite.bounds.extents.y * scale.y;
        }

        float minSpawnX = minX + halfWidth;
        float maxSpawnX = maxX - halfWidth;
        if (maxSpawnX < minSpawnX)
            maxSpawnX = minSpawnX;

        float x = Random.Range(minSpawnX, maxSpawnX);
        float y = maxY - halfHeight;

        Instantiate(enemyPrefab, new Vector3(x, y, 0f), Quaternion.identity);
    }

    public void MissEnemy()
    {
        if (IsGameOver)
            return;

        if (HudView.Instance == null)
        {
            TriggerGameOver();
            return;
        }

        _lives = Mathf.Max(0, _lives - 1);
        HudView.Instance.SetHearts(_lives);

        if (_lives <= 0)
            TriggerGameOver();
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;

        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);

        if (gameOverRoot != null)
            gameOverRoot.SetActive(true);

        if (HudView.Instance != null)
            HudView.Instance.SetHearts(0);

        Time.timeScale = 0f;
    }

    public static bool TryGetViewBounds(out float minX, out float maxX, out float minY, out float maxY)
    {
        if (Instance != null && Instance.useLevelBounds)
        {
            minX = Instance.levelMin.x;
            maxX = Instance.levelMax.x;
            minY = Instance.levelMin.y;
            maxY = Instance.levelMax.y;
            return true;
        }

        minX = maxX = minY = maxY = 0f;
        Camera camera = Camera.main;
        if (camera == null)
            return false;

        float depth = Mathf.Abs(camera.transform.position.z);
        Vector3 min = camera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
        Vector3 max = camera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
        minX = min.x;
        maxX = max.x;
        minY = min.y;
        maxY = max.y;
        return true;
    }
}
