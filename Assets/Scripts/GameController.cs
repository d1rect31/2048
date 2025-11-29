using UnityEngine;
using System;
using UnityEngine.SocialPlatforms.Impl;
using System.Collections.Generic;

public enum GameState
{
    Idle,
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
        scoreText.GetComponent<TMPro.TextMeshProUGUI>().text = Convert.ToString(Score);
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
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended) && !swipeDetected)
            {
                touchEndPos = touch.position;
                Vector2 delta = touchEndPos - touchStartPos;

                if (delta.magnitude > minSwipeDist && currentState == GameState.Idle)
                {
                    swipeDetected = true;
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        if (delta.x > 0 && CanMove("right"))
                            StartMove("right");
                        else if (delta.x < 0 && CanMove("left"))
                            StartMove("left");
                    }
                    else
                    {
                        if (delta.y > 0 && CanMove("up"))
                            StartMove("up");
                        else if (delta.y < 0 && CanMove("down"))
                            StartMove("down");
                    }
                }
            }
        }
        #endif

        if (currentState != GameState.Idle)
            return;

        if (Input.GetKeyDown(KeyCode.W) && CanMove("up"))
            StartMove("up");
        else if (Input.GetKeyDown(KeyCode.A) && CanMove("left"))
            StartMove("left");
        else if (Input.GetKeyDown(KeyCode.S) && CanMove("down"))
            StartMove("down");
        else if (Input.GetKeyDown(KeyCode.D) && CanMove("right"))
            StartMove("right");
    }
    private bool CanMove(string direction)
    {
        foreach (var cell in cells)
        {
            var cell2048 = cell.GetComponent<Cell2048>();
            if (cell2048.fill == null)
                continue;

            var neighborField = typeof(Cell2048).GetField(direction);
            if (neighborField == null)
                continue;

            var neighbor = neighborField.GetValue(cell2048) as Cell2048;
            if (neighbor != null)
            {
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
                return true;

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
    public void ExitGame()
    {
        Application.Quit();
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
