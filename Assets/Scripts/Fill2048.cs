using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Fill2048 : MonoBehaviour
{
    public int value;
    private float speed = 5000f;
    [SerializeField] GameObject valueDisplay;
    public bool mergedThisTurn;
    public void FillValueUpdate(int newValue)
    {
        value = newValue;
        if (valueDisplay != null)
        {
            valueDisplay.GetComponent<TextMeshProUGUI>().text = value.ToString();
        }
    }
    private void Update()
    {
        if (transform.localPosition != Vector3.zero)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Time.deltaTime * speed);
        }
        else if (mergedThisTurn == false)
        {
            if (transform.parent.GetChild(0) != this.transform)
            {
                Destroy(transform.gameObject);
            }
            mergedThisTurn = true;
        }
    }
    public void Double()
    {
        value *= 2;
        if (valueDisplay != null)
        {
            valueDisplay.GetComponent<TextMeshProUGUI>().text = value.ToString();
        }
    }
}
