using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Playing;

    [Header("Store Management")]
    [SerializeField] private float storeScore = 100f;
    [SerializeField] private float maxStoreScore = 100f;
    [SerializeField] private int gameOverThreshold = 0;

    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float levelDuration = 120f; // 2 minutes per level
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float healingMultiplier = 1f;
    [SerializeField] private float timeRemaining;

    [Header("Task Spawning")]
    [SerializeField] private float taskSpawnInterval = 10f;
    [SerializeField] private int maxConcurrentTasks = 3;

    // Events for UI and other systems
    public Action<float> OnStoreScoreChanged;
    public Action<float, float> OnTimeChanged;
    public Action<GameState> OnGameStateChanged;
    public Action<int> OnLevelComplete;
    public Action OnGameOver;

    [SerializeField] private TaskManager taskManager;

    public enum GameState
    {
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }

    private void Awake()
    {
        if (taskManager == null)
        {
            // Debug.LogError("TaskManager not found! Please add TaskManager to scene.");
        }
    }

    private void Start() {
        StartLevel();
    }

    private void Update()
    {
        switch (currentState)
        {
            case GameState.Playing:
                UpdateGameplay();
                break;
        }
    }

    private void UpdateGameplay()
    {
        // Check win condition
        if (timeRemaining <= 0)
        {
            CompleteLevel();
        }

        // Check lose condition
        if (storeScore <= gameOverThreshold)
        {
            GameOver();
        }

        // Update timer
        timeRemaining -= Time.deltaTime;
        OnTimeChanged?.Invoke(timeRemaining, levelDuration);
    }

    public void StartLevel()
    {
        Time.timeScale = 1f;
        timeRemaining = levelDuration;
        ChangeGameState(GameState.Playing);

        // Start spawning tasks
        if (taskManager != null)
        {
            taskManager.StartLevel(currentLevel, maxConcurrentTasks, taskSpawnInterval);
        }

        Debug.Log($"Level {currentLevel} started!");
    }

    public void CompleteLevel()
    {
        ChangeGameState(GameState.LevelComplete);

        if (taskManager != null)
        {
            taskManager.StopLevel();
        }

        OnLevelComplete?.Invoke(currentLevel);
        Debug.Log($"Level {currentLevel} complete!");

        // Auto-advance to next level after delay
        StartCoroutine(NextLevelDelay());
    }

    private IEnumerator NextLevelDelay()
    {
        yield return new WaitForSeconds(3f);
        currentLevel++;
        LoadLevel(currentLevel);
    }

    public void LoadLevel(int level)
    {
        SceneManager.LoadScene("Level" + level.ToString());
    }


    public void GameOver()
    {
        ChangeGameState(GameState.GameOver);

        if (taskManager != null)
        {
            taskManager.StopLevel();
        }

        OnGameOver?.Invoke();
        Debug.Log("Game Over!");
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            ChangeGameState(GameState.Paused);
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeGameState(GameState.Playing);
            Time.timeScale = 1f;
        }
    }

    public void RestartGame()
    {
        LoadLevel(currentLevel);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ChangeStoreScore(float amount)
    {
        amount *= amount > 0 ? healingMultiplier : damageMultiplier;
        storeScore = Mathf.Clamp(storeScore + amount, 0, maxStoreScore);
        OnStoreScoreChanged?.Invoke(storeScore);

        //Debug.Log($"Store Score: {storeScore} (changed by {amount})");
    }

    private void ChangeGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log($"Game State: {currentState}");
    }

    // Public getters
    public GameState CurrentState => currentState;
    public float StoreScore => storeScore;
    public int CurrentLevel => currentLevel;
    public float TimeRemaining => timeRemaining;
    public bool IsPlaying => currentState == GameState.Playing;
}