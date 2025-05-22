using UnityEngine;
using System;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject fillPrefab;
    [SerializeField] Transform[] cells;
    public static Action<string> slide;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnFill();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            slide("up");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            slide("left");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            slide("down");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            slide("right");
        }
    }
    public void SpawnFill()
    {
        int randomIndex = UnityEngine.Random.Range(0, cells.Length);
        if (cells[randomIndex].childCount > 0)
        {
            Debug.Log(cells[randomIndex].name + " is full");
            SpawnFill();
            return;
        }
        float chance = UnityEngine.Random.Range(0f, 1f);
        Debug.Log(chance);
        if (chance < 0.2f)
        {
            return;
        }
        else if (chance < 0.8f)
        {
            GameObject tempFill = Instantiate(fillPrefab, cells[randomIndex]);
            Fill2048 tempfill = tempFill.GetComponent<Fill2048>();
            cells[randomIndex].GetComponent<Cell2048>().fill = tempfill;
            tempfill.FillValueUpdate(2);
        }
        else
        {
            GameObject tempFill = Instantiate(fillPrefab, cells[randomIndex]);
            Fill2048 tempfill = tempFill.GetComponent<Fill2048>();
            cells[randomIndex].GetComponent<Cell2048>().fill = tempfill;
            tempfill.FillValueUpdate(4);
        }
    }
}
