using UnityEngine;
using System;
using UnityEngine.SocialPlatforms.Impl;
using System.Collections.Generic;

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
    [SerializeField] private GameObject winScreenPrefab;
    private GameObject gameOverScreenInstance;
    private GameObject winScreenInstance;
    [SerializeField] private GameObject scoreText;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private bool swipeDetected;
    private const float minSwipeDist = 50f;

    [SerializeField] private GameState currentState = GameState.Idle;
    private readonly Queue<string> moveBuffer = new();
    private const int MaxBufferSize = 2;
    public bool won;
    public static GameController Instance { get; private set; }
    public int Score { get; private set; }
    public int HighScore
    {
        get => PlayerPrefs.GetInt("HighScore", 0);
        private set
        {
            PlayerPrefs.SetInt("HighScore", value);
            PlayerPrefs.Save();
        }
    }
    private void Awake()
    {
        Instance = this;
    }

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
    public void AddScore(int value)
    {
        Score += value;
        scoreText.GetComponent<TMPro.TextMeshProUGUI>().text = "Score: " + Score;
    }
    void Update()
    {
        
        #if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                swipeDetected = false;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                Vector2 delta = touchEndPos - touchStartPos;

                if (!swipeDetected && delta.magnitude > minSwipeDist)
                {
                    swipeDetected = true;
                    string direction = null;
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        if (delta.x > 0 && CanMove("right") && moveBuffer.Count < MaxBufferSize)
                            direction = "right";
                        else if (delta.x < 0 && CanMove("left") && moveBuffer.Count < MaxBufferSize)
                            direction = "left";
                    }
                    else
                    {
                        if (delta.y > 0 && CanMove("up") && moveBuffer.Count < MaxBufferSize)
                            direction = "up";
                        else if (delta.y < 0 && CanMove("down") && moveBuffer.Count < MaxBufferSize)
                            direction = "down";
                    }
                    if (direction != null)
                        moveBuffer.Enqueue(direction);
                }
            }
        }
        #endif
        
        if (Input.GetKeyDown(KeyCode.W) && CanMove("up") && moveBuffer.Count < MaxBufferSize)
            moveBuffer.Enqueue("up");
        else if (Input.GetKeyDown(KeyCode.A) && CanMove("left") && moveBuffer.Count < MaxBufferSize)
            moveBuffer.Enqueue("left");
        else if (Input.GetKeyDown(KeyCode.S) && CanMove("down") && moveBuffer.Count < MaxBufferSize)
            moveBuffer.Enqueue("down");
        else if (Input.GetKeyDown(KeyCode.D) && CanMove("right") && moveBuffer.Count < MaxBufferSize)
            moveBuffer.Enqueue("right");

        // Выполняем ход только если Idle и есть ходы в буфере
        if (currentState == GameState.Idle && moveBuffer.Count > 0)
        {
            // Пропускаем невозможные ходы
            while (moveBuffer.Count > 0)
            {
                string dir = moveBuffer.Peek();
                if (CanMove(dir))
                {
                    StartMove(moveBuffer.Dequeue());
                    break;
                }
                else
                {
                    moveBuffer.Dequeue(); // Удаляем невозможный ход
                }
            }
        }
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
    }

    private void OnAllFillsStopped()
    {
        SpawnFill();

        if (!HasAvailableMoves())
        {
            currentState = GameState.GameOver;
            Debug.Log("Игра окончена. Нет доступных ходов.");
            ShowGameOverScreen();
        }
        else if (Array.Exists(cells, cell =>
            cell.childCount > 0 &&
            cell.GetComponent<Cell2048>() != null &&
            cell.GetComponent<Cell2048>().fill != null &&
            cell.GetComponent<Cell2048>().fill.value == 2048) && won == false)
        {
            currentState = GameState.Win;
            Debug.Log("Вы выиграли!");
            won = true;
            ShowWinScreen();
        }
        else
        {
            currentState = GameState.Idle;
            if (moveBuffer.Count > 0)
                StartMove(moveBuffer.Dequeue());
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
            if (Score > HighScore)
                HighScore = Score;

            var highScoreText = gameOverScreenInstance.transform.Find("HighScoreText");
            if (highScoreText != null)
            {
                var tmp = highScoreText.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                    tmp.text = "Highscore: " + HighScore;
            }
            var restartButton = gameOverScreenInstance.transform.Find("RestartButton");
            if (restartButton != null)
            {
                var btn = restartButton.GetComponent<UnityEngine.UI.Button>();
                if (btn != null)
                    btn.onClick.AddListener(RestartLevel);
            }
        }
    }
    private void ShowWinScreen()
    {
        if (winScreenPrefab != null && winScreenInstance == null)
        {
            winScreenInstance = Instantiate(winScreenPrefab);

            if (Score > HighScore)
                HighScore = Score;

            var highScoreText = winScreenInstance.transform.Find("HighScoreText");
            if (highScoreText != null)
            {
                var tmp = highScoreText.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                    tmp.text = "Highscore: " + HighScore;
            }

            // Кнопка Continue
            var continueButton = winScreenInstance.transform.Find("ContinueButton");
            if (continueButton != null)
            {
                var btn = continueButton.GetComponent<UnityEngine.UI.Button>();
                if (btn != null)
                    btn.onClick.AddListener(ContinueAfterWin);
            }
        }
    }

    private void ContinueAfterWin()
    {
        if (winScreenInstance != null)
        {
            Destroy(winScreenInstance);
            winScreenInstance = null;
        }
        currentState = GameState.Idle;
    }
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
