using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
        LevelSelect,
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            //currentState = Instance.currentState;
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }

        if (taskManager == null)
        {
            Debug.LogError("TaskManager not found! Please add TaskManager to scene.");
        }
    }
    
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            ChangeGameState(GameState.MainMenu);
        }
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
        // Default to level 1
        StartGame(1);
    }

    public void StartGame(int level)
    {
        currentLevel = level;
        storeScore = maxStoreScore;
        StartLevel(level);
    }

    public void StartLevel(int level)
    {
        SceneManager.LoadScene("Level" + level.ToString());

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
        StartLevel(currentLevel);
    }
    
    public void GoToLevelSelect()
    {
        ChangeGameState(GameState.LevelSelect);
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
        StartLevel(currentLevel);
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