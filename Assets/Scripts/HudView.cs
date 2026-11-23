using UnityEngine;
using UnityEngine.UI;

public class HudView : MonoBehaviour
{
    public static HudView Instance { get; private set; }

    [SerializeField] Image[] hearts;
    [SerializeField] Sprite heartFull;
    [SerializeField] Sprite heartEmpty;
    [SerializeField] Image[] digits;
    [SerializeField] Sprite[] digitSprites;

    int _score;

    public int HeartSlotCount => hearts != null ? hearts.Length : 0;

    void Awake()
    {
        Instance = this;
        ShowScore(0);
        SetHearts(hearts != null ? hearts.Length : 0);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddScore(int amount)
    {
        _score = Mathf.Max(0, _score + amount);
        ShowScore(_score);
    }

    public void SetHearts(int fullCount)
    {
        if (hearts == null)
            return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            hearts[i].sprite = i < fullCount ? heartFull : heartEmpty;
        }
    }

    void ShowScore(int value)
    {
        if (digits == null || digitSprites == null || digitSprites.Length < 10)
            return;

        value = Mathf.Clamp(value, 0, 999);
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            if (digits[i] == null)
                continue;

            int digit = value % 10;
            value /= 10;
            digits[i].sprite = digitSprites[digit];
        }
    }
}
