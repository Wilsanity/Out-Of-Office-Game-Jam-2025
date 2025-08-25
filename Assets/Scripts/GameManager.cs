using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    
    [Header("Store Management")]
    [SerializeField] private int storeScore = 100;
    [SerializeField] private int maxStoreScore = 100;
    [SerializeField] private int gameOverThreshold = 0;
    
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float levelDuration = 120f; // 2 minutes per level
    [SerializeField] private float timeRemaining;
    
    [Header("Task Spawning")]
    [SerializeField] private float taskSpawnInterval = 10f;
    [SerializeField] private int maxConcurrentTasks = 3;
    
    // Events for UI and other systems
    public Action<int> OnStoreScoreChanged;
    public Action<float> OnTimeChanged;
    public Action<GameState> OnGameStateChanged;
    public Action<int> OnLevelComplete;
    public Action OnGameOver;
    
    [SerializeField] private TaskManager taskManager;
    
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }
    
    private void Awake()
    {
        if (taskManager == null)
        {
            Debug.LogError("TaskManager not found! Please add TaskManager to scene.");
        }
    }
    
    private void Start()
    {
        ChangeGameState(GameState.MainMenu);
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
        // Update timer
        timeRemaining -= Time.deltaTime;
        OnTimeChanged?.Invoke(timeRemaining);
        
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
    }
    
    public void StartGame()
    {
        currentLevel = 1;
        storeScore = maxStoreScore;
        StartLevel();
    }
    
    public void StartLevel()
    {
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
        StartLevel();
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
        Time.timeScale = 1f;
        storeScore = maxStoreScore;
        currentLevel = 1;
        StartLevel();
    }
    
    public void ChangeStoreScore(int amount)
    {
        storeScore = Mathf.Clamp(storeScore + amount, 0, maxStoreScore);
        OnStoreScoreChanged?.Invoke(storeScore);
        
        Debug.Log($"Store Score: {storeScore} (changed by {amount})");
    }
    
    private void ChangeGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log($"Game State: {currentState}");
    }
    
    // Public getters
    public GameState CurrentState => currentState;
    public int StoreScore => storeScore;
    public int CurrentLevel => currentLevel;
    public float TimeRemaining => timeRemaining;
    public bool IsPlaying => currentState == GameState.Playing;
}