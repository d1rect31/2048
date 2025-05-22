using System;
using UnityEngine;

public class Cell2048 : MonoBehaviour
{
    public Cell2048 left;
    public Cell2048 right;
    public Cell2048 up;
    public Cell2048 down;
    public Fill2048 fill;
    public void OnEnable()
    {
        GameController.slide += OnSlide;
    }
    public void OnDisable()
    {
        GameController.slide -= OnSlide;
    }
    
    private void OnSlide(string recievedDirection)
    {
        // Debug.Log(recievedDirection);
        if (recievedDirection == "up")
        {
            if (up != null)
                return;
            Cell2048 cell = this;
            SlideUp(cell);
        }
        if (recievedDirection == "down")
        {

        }
        if (recievedDirection == "left")
        {

        }
        if (recievedDirection == "right")
        {

        }
    }
    void SlideUp(Cell2048 currentCell)
    {
        Debug.Log(currentCell.name);
        if (currentCell.down == null)
            return; 
        if (currentCell.fill != null)
        {
            Cell2048 nextCell = currentCell.down;
            while (nextCell.down != null && nextCell.fill == null)
            {
                nextCell = nextCell.down;
            }
            if (nextCell.fill != null)
            {
                if (currentCell.fill.value == nextCell.fill.value)
                {
                    Debug.Log("Merging");
                    nextCell.fill.transform.parent = currentCell.transform;
                    currentCell.fill = nextCell.fill;
                    nextCell.fill = null;
                }
                else
                {
                    Debug.Log("Not Merged");
                    nextCell.fill.transform.parent = currentCell.down.transform;
                    currentCell.down.fill = nextCell.fill;
                    nextCell.fill = null;
                }
            }
        }
        else
        {
            Cell2048 nextCell = currentCell.down;
            while (nextCell.down != null && nextCell.fill == null)
            {
                nextCell = nextCell.down;
            }
            if (nextCell.fill != null)
            {
                nextCell.fill.transform.parent = currentCell.transform;
                currentCell.fill = nextCell.fill;
                nextCell.fill = null;
                SlideUp(currentCell);
                Debug.Log("Slided to Empty");
            }
        }
        if (currentCell.down == null)
            return;
        SlideUp(currentCell.down);
    }
}
