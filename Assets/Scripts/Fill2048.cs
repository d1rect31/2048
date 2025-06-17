using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Fill2048 : MonoBehaviour
{
    public int value;
    //private float speed = 5000f; 
    [SerializeField] private GameObject valueDisplay; 
    private Image image;

    public bool mergedThisTurn;

    private void Awake()
    {
        image = GetComponent<Image>();
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
            _ => new Color(0.1f, 0.1f, 0.1f),
        };
        if (valueDisplay != null)
        {
            valueDisplay.GetComponent<TextMeshProUGUI>().text = value.ToString();
        }
    }

    private void Update()
    {
        if (transform.localPosition != Vector3.zero)
        {
            transform.localPosition = Vector3.zero;
        }
        else
        {
            if (transform.parent != null && transform.parent.GetChild(0) != this.transform)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Double()
    {
        value *= 2;
        if (valueDisplay != null)
        {
            valueDisplay.GetComponent<TextMeshProUGUI>().text = value.ToString();
        }
        FillValueUpdate(value);
    }

    public void Remove()
    {
        Destroy(this.gameObject);
    }
}
