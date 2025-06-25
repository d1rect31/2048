using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class Cell2048 : MonoBehaviour
{
    public Cell2048 left;
    public Cell2048 right;
    public Cell2048 up;
    public Cell2048 down;
    public Fill2048 fill;
    public bool mergedThisTurn = false;
    public static bool wasAnyMoveOrMerge = false;
    public static readonly List<Cell2048> AllCells = new();
    private static readonly List<Fill2048> fillsToRemove = new();

    private void OnEnable()
    {
        AllCells.Add(this);
        GameController.slide += OnSlide;
    }

    private void OnDisable()
    {
        AllCells.Remove(this);
        GameController.slide -= OnSlide;
    }

    private void OnSlide(string recievedDirection)
    {
        wasAnyMoveOrMerge = false;
        switch (recievedDirection)
        {
            case "up":
                if (up == null)
                    Slide(this, c => c.down, FindNextFilledDown);
                break;
            case "down":
                if (down == null)
                    Slide(this, c => c.up, FindNextFilledUp);
                break;
            case "left":
                if (left == null)
                    Slide(this, c => c.right, FindNextFilledRight);
                break;
            case "right":
                if (right == null)
                    Slide(this, c => c.left, FindNextFilledLeft);
                break;
        }

        ResetAllMergedFlags();
    }

    /// <summary>
    /// —брасывает флаг mergedThisTurn у всех €чеек и их фишек на сцене.
    /// </summary>
    public static void ResetAllMergedFlags()
    {
        foreach (var cell in AllCells)
        {
            cell.mergedThisTurn = false;
            if (cell.fill != null)
                cell.fill.mergedThisTurn = false;
        }
    }

    private delegate Cell2048 NextCellDelegate(Cell2048 cell);
    private delegate Cell2048 FindNextFilledDelegate(Cell2048 cell);

    private void Slide(Cell2048 currentCell, NextCellDelegate getNext, FindNextFilledDelegate findNextFilled)
    {
        var nextCell = getNext(currentCell);
        if (nextCell == null)
            return;

        var filledCell = findNextFilled(currentCell);
        if (filledCell == null || filledCell.fill == null)
        {
            if (nextCell != null)
                Slide(nextCell, getNext, findNextFilled);
            return;
        }

        if (currentCell.fill == null)
        {
            filledCell.fill.transform.parent = currentCell.transform;
            currentCell.fill = filledCell.fill;
            filledCell.fill = null;
            Slide(currentCell, getNext, findNextFilled);
        }
        else if (currentCell.fill.value == filledCell.fill.value && !currentCell.mergedThisTurn && !filledCell.mergedThisTurn)
        {
            currentCell.fill.Double();
            filledCell.fill.transform.parent = currentCell.transform;
            filledCell.fill.MarkForRemove();
            fillsToRemove.Add(filledCell.fill);
            filledCell.fill = null;
            currentCell.mergedThisTurn = true;
        }
        else if (getNext(currentCell).fill == null)
        {
            filledCell.fill.transform.parent = getNext(currentCell).transform;
            getNext(currentCell).fill = filledCell.fill;
            filledCell.fill = null;
        }

        if (getNext(currentCell) != null)
            Slide(getNext(currentCell), getNext, findNextFilled);
    }

    private static Cell2048 FindNextFilledDown(Cell2048 cell)
    {
        Cell2048 next = cell.down;
        while (next != null && next.fill == null && next.down != null)
        {
            next = next.down;
        }
        return next;
    }
    private static Cell2048 FindNextFilledUp(Cell2048 cell)
    {
        Cell2048 next = cell.up;
        while (next != null && next.fill == null && next.up != null)
        {
            next = next.up;
        }
        return next;
    }
    private static Cell2048 FindNextFilledRight(Cell2048 cell)
    {
        Cell2048 next = cell.right;
        while (next != null && next.fill == null && next.right != null)
        {
            next = next.right;
        }
        return next;
    }
    private static Cell2048 FindNextFilledLeft(Cell2048 cell)
    {
        Cell2048 next = cell.left;
        while (next != null && next.fill == null && next.left != null)
        {
            next = next.left;
        }
        return next;
    }
    public static void RemoveDelayedFills()
    {
        fillsToRemove.Clear();
    }
}
