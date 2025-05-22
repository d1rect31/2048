using System;
using UnityEngine;

public class Cell2048 : MonoBehaviour
{
    public Cell2048 left;
    public Cell2048 right;
    public Cell2048 up;
    public Cell2048 down;
    public Fill2048 fill;
    public bool mergedThisTurn = false;
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
            SlideUp(this);
        }
        if (recievedDirection == "down")
        {
            if (down != null)
                return;
            SlideDown(this);
        }
        if (recievedDirection == "left")
        {
            if (left != null)
                return;
            SlideLeft(this);
        }
        if (recievedDirection == "right")
        {
            if (right != null)
                return;
            SlideRight(this);
        }
    }
    void SlideUp(Cell2048 currentCell)
    {
        if (currentCell.down == null)
            return;

        Cell2048 nextCell = FindNextFilledDown(currentCell);
        if (nextCell == null || nextCell.fill == null)
        {
            if (currentCell.down != null)
                SlideUp(currentCell.down);
            return;
        }

        if (currentCell.fill == null)
        {
            nextCell.fill.transform.parent = currentCell.transform;
            currentCell.fill = nextCell.fill;
            nextCell.fill = null;
            SlideUp(currentCell);
        }
        else if (currentCell.fill.value == nextCell.fill.value && !currentCell.mergedThisTurn && !nextCell.mergedThisTurn)
        {
            currentCell.fill.Double();
            nextCell.fill = null;
            currentCell.mergedThisTurn = true;
        }
        else if (currentCell.down.fill == null)
        {
            nextCell.fill.transform.parent = currentCell.down.transform;
            currentCell.down.fill = nextCell.fill;
            nextCell.fill = null;
        }

        if (currentCell.down != null)
            SlideUp(currentCell.down);
    }
    void SlideDown(Cell2048 currentCell)
    {
        if (currentCell.up == null)
            return;

        Cell2048 nextCell = FindNextFilledUp(currentCell);
        if (nextCell == null || nextCell.fill == null)
        {
            if (currentCell.up != null)
                SlideDown(currentCell.up);
            return;
        }

        if (currentCell.fill == null)
        {
            nextCell.fill.transform.parent = currentCell.transform;
            currentCell.fill = nextCell.fill;
            nextCell.fill = null;
            SlideDown(currentCell);
        }
        else if (currentCell.fill.value == nextCell.fill.value && !currentCell.mergedThisTurn && !nextCell.mergedThisTurn)
        {
            currentCell.fill.Double();
            nextCell.fill = null;
            currentCell.mergedThisTurn = true;
        }
        else if (currentCell.up.fill == null)
        {
            nextCell.fill.transform.parent = currentCell.up.transform;
            currentCell.up.fill = nextCell.fill;
            nextCell.fill = null;
        }

        if (currentCell.up != null)
            SlideDown(currentCell.up);
    }

    void SlideLeft(Cell2048 currentCell)
    {
        if (currentCell.right == null)
            return;

        Cell2048 nextCell = FindNextFilledRight(currentCell);
        if (nextCell == null || nextCell.fill == null)
        {
            if (currentCell.right != null)
                SlideLeft(currentCell.right);
            return;
        }

        if (currentCell.fill == null)
        {
            nextCell.fill.transform.parent = currentCell.transform;
            currentCell.fill = nextCell.fill;
            nextCell.fill = null;
            SlideLeft(currentCell);
        }
        else if (currentCell.fill.value == nextCell.fill.value && !currentCell.mergedThisTurn && !nextCell.mergedThisTurn)
        {
            currentCell.fill.Double();
            nextCell.fill = null;
            currentCell.mergedThisTurn = true;
        }
        else if (currentCell.right.fill == null)
        {
            nextCell.fill.transform.parent = currentCell.right.transform;
            currentCell.right.fill = nextCell.fill;
            nextCell.fill = null;
        }

        if (currentCell.right != null)
            SlideLeft(currentCell.right);
    }

    void SlideRight(Cell2048 currentCell)
    {
        if (currentCell.left == null)
            return;

        Cell2048 nextCell = FindNextFilledLeft(currentCell);
        if (nextCell == null || nextCell.fill == null)
        {
            if (currentCell.left != null)
                SlideRight(currentCell.left);
            return;
        }

        if (currentCell.fill == null)
        {
            nextCell.fill.transform.parent = currentCell.transform;
            currentCell.fill = nextCell.fill;
            nextCell.fill = null;
            SlideRight(currentCell);
        }
        else if (currentCell.fill.value == nextCell.fill.value && !currentCell.mergedThisTurn && !nextCell.mergedThisTurn)
        {
            currentCell.fill.Double();
            nextCell.fill = null;
            currentCell.mergedThisTurn = true;
        }
        else if (currentCell.left.fill == null)
        {
            nextCell.fill.transform.parent = currentCell.left.transform;
            currentCell.left.fill = nextCell.fill;
            nextCell.fill = null;
        }

        if (currentCell.left != null)
            SlideRight(currentCell.left);
    }
    private Cell2048 FindNextFilledDown(Cell2048 cell)
    {
        Cell2048 next = cell.down;
        while (next != null && next.fill == null && next.down != null)
        {
            next = next.down;
        }
        return next;
    }
    private Cell2048 FindNextFilledUp(Cell2048 cell)
    {
        Cell2048 next = cell.up;
        while (next != null && next.fill == null && next.up != null)
        {
            next = next.up;
        }
        return next;
    }

    private Cell2048 FindNextFilledRight(Cell2048 cell)
    {
        Cell2048 next = cell.right;
        while (next != null && next.fill == null && next.right != null)
        {
            next = next.right;
        }
        return next;
    }

    private Cell2048 FindNextFilledLeft(Cell2048 cell)
    {
        Cell2048 next = cell.left;
        while (next != null && next.fill == null && next.left != null)
        {
            next = next.left;
        }
        return next;
    }
}
