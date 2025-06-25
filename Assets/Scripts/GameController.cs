using UnityEngine;
using System;

public enum GameState
{
    Idle,
    SpawningBlocks,
    Moving,
    GameOver,
    Win
}
public class GameController : MonoBehaviour
{
    [SerializeField] GameObject fillPrefab;
    [SerializeField] Transform[] cells;
    public static Action<string> slide;
    [SerializeField] private GameObject gameOverScreenPrefab;
    private GameObject gameOverScreenInstance;
    [SerializeField] private GameState currentState = GameState.Idle;
    private void Start()
    {
        SpawnFill();
        SpawnFill();
    }
    private void OnEnable()
    {
        Fill2048.AllStopped += OnAllFillsStopped;
    }

    private void OnDisable()
    {
        Fill2048.AllStopped -= OnAllFillsStopped;
    }

    void Update()
    {
        if (currentState != GameState.Idle)
            return;
        if (currentState == GameState.GameOver)
            return;

        if (Input.GetKeyDown(KeyCode.W) && CanMove("up"))
            StartMove("up");
        if (Input.GetKeyDown(KeyCode.A) && CanMove("left"))
            StartMove("left");
        if (Input.GetKeyDown(KeyCode.S) && CanMove("down"))
            StartMove("down");
        if (Input.GetKeyDown(KeyCode.D) && CanMove("right"))
            StartMove("right");
    }
    private bool CanMove(string direction)
    {
        foreach (var cell in cells)
        {
            var cell2048 = cell.GetComponent<Cell2048>();
            if (cell2048.fill == null)
                continue;

            // Получаем нужное поле-соседа по имени направления
            var neighborField = typeof(Cell2048).GetField(direction);
            if (neighborField == null)
                continue;

            var neighbor = neighborField.GetValue(cell2048) as Cell2048;
            if (neighbor != null)
            {
                // Можно сдвинуться, если соседняя ячейка пуста или можно объединить
                if (neighbor.fill == null || neighbor.fill.value == cell2048.fill.value)
                    return true;
            }
        }
        return false;
    }
    private void StartMove(string direction)
    {
        currentState = GameState.Moving;
        slide(direction);
        Cell2048.ResetAllMergedFlags();
        // Ожидание окончания движения Fill2048 через событие AllStopped
    }

    private void OnAllFillsStopped()
    {
        Cell2048.RemoveDelayedFills();
        currentState = GameState.SpawningBlocks;
        SpawnFill();
        // Проверка на проигрыш
        if (!HasAvailableMoves())
        {
            currentState = GameState.GameOver;
            Debug.Log("Игра окончена. Нет доступных ходов.");
            ShowGameOverScreen();
        }
        else
        {
            currentState = GameState.Idle;
        }
    }
    public void SpawnFill()
    {
        bool allCellsFull = true;
        foreach (var cell in cells)
        {
            if (cell.childCount == 0)
            {
                allCellsFull = false;
                break;
            }
        }
        if (allCellsFull)
        {
            Debug.Log("Все клетки заняты. Невозможно создать новый Fill.");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, cells.Length);

        if (cells[randomIndex].childCount > 0)
        {
            SpawnFill();
            return;
        }
        float chance = UnityEngine.Random.Range(0f, 1f);
        if (chance < 0.8f)
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
    private bool HasAvailableMoves()
    {
        foreach (var cell in cells)
        {
            var cell2048 = cell.GetComponent<Cell2048>();
            if (cell2048.fill == null)
                return true; // Есть пустая клетка

            // Проверяем соседей на возможность слияния
            if (cell2048.up != null && cell2048.up.fill != null && cell2048.fill.value == cell2048.up.fill.value)
                return true;
            if (cell2048.down != null && cell2048.down.fill != null && cell2048.fill.value == cell2048.down.fill.value)
                return true;
            if (cell2048.left != null && cell2048.left.fill != null && cell2048.fill.value == cell2048.left.fill.value)
                return true;
            if (cell2048.right != null && cell2048.right.fill != null && cell2048.fill.value == cell2048.right.fill.value)
                return true;
        }
        return false;
    }
    private void ShowGameOverScreen()
    {
        if (gameOverScreenPrefab != null && gameOverScreenInstance == null)
        {
            gameOverScreenInstance = Instantiate(gameOverScreenPrefab);
        }
    }
}
