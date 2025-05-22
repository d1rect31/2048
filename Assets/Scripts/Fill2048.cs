using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Fill2048 : MonoBehaviour
{
    public int value;
    [SerializeField] GameObject valueDisplay;
    public void FillValueUpdate(int newValue)
    {
        value = newValue;
        if (valueDisplay != null)
        {
            valueDisplay.GetComponent<TextMeshProUGUI>().text = value.ToString();
        }
    }
}
