using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
public class Fill2048 : MonoBehaviour
{
    public int value;
    [SerializeField] private float speed = 5000f;
    [SerializeField] private GameObject valueDisplay;
    private Image image;
    private bool isRainbow = false;
    private float rainbowTime = 0f;
    public bool mergedThisTurn;

    private static int movingCount = 0;
    public static event Action AllStopped;

    private bool isMoving = false;
    public bool pendingRemove = false;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        // Запускаем анимацию спавна при создании
        transform.localScale = Vector3.zero;
        StartCoroutine(SpawnAnimation());
    }

    private IEnumerator SpawnAnimation()
    {
        float t = 0f;
        float duration = 0.15f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, 1f, t / duration);
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    public void FillValueUpdate(int newValue)
    {
        value = newValue;
        image.color = value switch
        {
            2 => new Color(238f / 255f, 228f / 255f, 218f / 255f),
            4 => new Color(237f / 255f, 224f / 255f, 200f / 255f),
            8 => new Color(242f / 255f, 177f / 255f, 121f / 255f),
            16 => new Color(245f / 255f, 149f / 255f, 99f / 255f),
            32 => new Color(246f / 255f, 124f / 255f, 96f / 255f),
            64 => new Color(246f / 255f, 94f / 255f, 59f / 255f),
            128 => new Color(237f / 255f, 207f / 255f, 115f / 255f),
            256 => new Color(237f / 255f, 204f / 255f, 98f / 255f),
            512 => new Color(237f / 255f, 200f / 255f, 80f / 255f),
            1024 => new Color(237f / 255f, 197f / 255f, 63f / 255f),
            2048 => new Color(237f / 255f, 194f / 255f, 45f / 255f),
            _ => EnableRainbowMode(),
        };
        if (valueDisplay != null)
        {
            valueDisplay.GetComponent<TextMeshProUGUI>().text = value.ToString();
        }
    }
    private Color EnableRainbowMode()
    {
        isRainbow = true;
        rainbowTime = 0f;
        // Начальный цвет (красный)
        return Color.HSVToRGB(0f, 1f, 1f);
    }
    private void Update()
    {
        if (transform.localPosition != Vector3.zero)
        {
            if (!isMoving)
            {
                isMoving = true;
                movingCount++;
            }
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Time.deltaTime * speed);
        }
        else
        {
            // Сначала удаляем, если нужно
            if (pendingRemove)
            {
                var parentCell = transform.parent.GetComponent<Cell2048>();
                if (parentCell != null && parentCell.fill == this)
                    parentCell.fill = null;

                if (isMoving)
                {
                    isMoving = false;
                    movingCount--;
                }

                Destroy(gameObject);

                // Проверяем, не пора ли вызвать AllStopped
                if (movingCount == 0 && AllStopped != null)
                {
                    AllStopped.Invoke();
                }
                return;
            }

            if (isMoving)
            {
                isMoving = false;
                movingCount--;
                if (movingCount == 0 && AllStopped != null)
                {
                    AllStopped.Invoke();
                }
            }
        }
        if (isRainbow)
        {
            rainbowTime += Time.deltaTime;
            float hue = (rainbowTime * .1f) % 1f;
            image.color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }
    public void MarkForRemove()
    {
        pendingRemove = true;
    }
    public void Double()
    {
        value *= 2;
        if (valueDisplay != null)
        {
            valueDisplay.GetComponent<TextMeshProUGUI>().text = value.ToString();
        }
        FillValueUpdate(value);
        GameController.Instance.AddScore(value);
        StartCoroutine(MergeAnimation());
    }

    private IEnumerator MergeAnimation()
    {
        float t = 0f;
        float duration = 0.08f;
        float maxScale = 1.1f;
        // Увеличение
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, maxScale, t / duration);
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        // Возврат к обычному размеру
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(maxScale, 1f, t / duration);
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    public void Remove()
    {
        Destroy(this.gameObject);
    }
}
