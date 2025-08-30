using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public InputActionReference pauseAction;

    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager;
    
    [Header("Main HUD")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI storeRatingText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider storeRatingSlider;
    [SerializeField] private Slider timerSlider;


    [Header("Task UI")]
    [SerializeField] private GameObject taskListPanel;
    [SerializeField] private Transform taskListContainer;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private TextMeshProUGUI activeTaskCountText;
    
    [Header("Game State Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject pausePanel;
    
    [Header("Main Menu")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionsText;

    [Header("Level Select")]
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private Transform levelSelectContainer;

    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI gameOverTitleText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalLevelText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Level Complete")]
    [SerializeField] private TextMeshProUGUI levelCompleteText;
    [SerializeField] private TextMeshProUGUI levelScoreText;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    
    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseMainMenuButton;
    
    [Header("Player Interaction")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    
    [Header("Settings")]
    [SerializeField] private Color urgentTaskColor = Color.red;
    [SerializeField] private Color normalTaskColor = Color.white;
    [SerializeField] private Color completedTaskColor = Color.green;
    
    [SerializeField] private TaskManager taskManager;
    
    private void Awake()
    {
        SetupButtons();
    }
    
    private void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager reference not set in UIManager.");
        }

        // Subscribe to GameManager events
        if (gameManager != null)
        {
            gameManager.OnStoreScoreChanged += UpdateStoreScore;
            gameManager.OnTimeChanged += UpdateTimer;
            gameManager.OnGameStateChanged += UpdateGameState;
            gameManager.OnLevelComplete += ShowLevelComplete;
            gameManager.OnGameOver += ShowGameOver;
        }

        // Initialize UI
        bool gameManagerValid = gameManager != null;
        UpdateGameState(gameManager != null ? gameManager.CurrentState : GameManager.GameState.Playing);
        UpdateStoreScore(gameManager != null ? gameManager.StoreScore : 100f);

        pauseAction.action.started += context => OnPausePressed();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnStoreScoreChanged -= UpdateStoreScore;
            gameManager.OnTimeChanged -= UpdateTimer;
            gameManager.OnGameStateChanged -= UpdateGameState;
            gameManager.OnLevelComplete -= ShowLevelComplete;
            gameManager.OnGameOver -= ShowGameOver;
        }
    }
    
    private void Update()
    {
        // Update task list periodically
        if (gameManager != null && gameManager.IsPlaying)
        {
            UpdateTaskList();
        }
    }

    private void OnPausePressed() {
        if (gameManager != null) {
            if (gameManager.CurrentState == GameManager.GameState.Playing) {
                gameManager.PauseGame();
            } else if (gameManager.CurrentState == GameManager.GameState.Paused) {
                gameManager.ResumeGame();
            }
        }
    }

    private void SetupButtons()
    {
        if (levelSelectContainer)
        {
            Button[] levelSelectButtons = levelSelectContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < levelSelectButtons.Length; i++)
            {
                Button button = levelSelectButtons[i];
                int cachedI = i + 1;
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText)
                {
                    buttonText.text = cachedI.ToString();

                }

                button.onClick.AddListener(() => gameManager?.LoadLevel(cachedI));
            }
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(() => gameManager?.RestartGame());
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => ReturnToMainMenu());
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => gameManager?.ResumeGame());
        
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(() => gameManager?.RestartGame());
        
        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => ReturnToMainMenu());
    }
    
    private void UpdateStoreScore(float score)
    {
        if (storeRatingText != null)
            storeRatingText.text = $"Store Rating";
        
        if (storeRatingSlider != null)
        {
            storeRatingSlider.value = score / 100f; // Assuming max score is 100
            
            // Change color based on score
            Image fillImage = storeRatingSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                if (score > 60)
                    fillImage.color = Color.green;
                else if (score > 30)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
        }
    }
    
    private void UpdateTimer(float timeRemaining, float timeTotal)
    {
        Color timerColor;

        // Change color when time is running out
        if (timeRemaining < timeTotal / 4)
            timerColor = Color.red;
        else if (timeRemaining < timeTotal / 2)
            timerColor = Color.yellow;
        else
            timerColor = Color.white;

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"Time Left: {minutes:00}:{seconds:00}";
            timerText.color = timerColor;
        }

        if (timerSlider != null)
        {
            timerSlider.value = timeRemaining / timeTotal; // Assuming 2 minutes per level

            // Change color based on time remaining
            Image fillImage = timerSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = timerColor;
            }
        }
    }
    
    private void UpdateGameState(GameManager.GameState newState)
    {
        // Hide all panels first
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(levelSelectPanel, false);
        SetPanelActive(hudPanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(levelCompletePanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(taskListPanel, false);
        SetPanelActive(interactionPrompt, false);
    
        // Show appropriate panels based on state
        switch (newState)
        {
            case GameManager.GameState.Playing:
                SetPanelActive(mainMenuPanel, false);
                SetPanelActive(hudPanel, true);
                SetPanelActive(taskListPanel, true);
                SetPanelActive(pausePanel, false);
                if (levelText != null && gameManager != null)
                    levelText.text = $"Level: {gameManager.CurrentLevel}";
                break;
                
            case GameManager.GameState.Paused:
                SetPanelActive(hudPanel, true);
                SetPanelActive(pausePanel, true);
                break;
                
            case GameManager.GameState.LevelComplete:
                SetPanelActive(hudPanel, true);
                SetPanelActive(levelCompletePanel, true);
                break;
                
            case GameManager.GameState.GameOver:
                SetPanelActive(gameOverPanel, true);
                break;
        }
    }
    
    private void ShowLevelComplete(int level)
    {
        if (levelCompleteText != null)
            levelCompleteText.text = $"Level {level} Complete!";
        
        if (levelScoreText != null && gameManager != null)
            levelScoreText.text = $"Store Rating: {gameManager.StoreScore}";
        
        if (nextLevelText != null)
            nextLevelText.text = $"Preparing Level {level + 1}...";
    }
    
    private void ShowGameOver()
    {
        if (gameOverTitleText != null)
            gameOverTitleText.text = "Store Closed!";
        
        if (finalScoreText != null && gameManager != null)
            finalScoreText.text = $"Final Rating: {gameManager.StoreScore}";
        
        if (finalLevelText != null && gameManager != null)
            finalLevelText.text = $"Reached Level: {gameManager.CurrentLevel}";
    }
    
    private void UpdateTaskList()
    {
        if (taskManager == null || taskListContainer == null) return;
        
        // Update task count
        if (activeTaskCountText != null)
            activeTaskCountText.text = $"Active Tasks: {taskManager.ActiveTaskCount}";
        
        // Clear existing task items
        foreach (Transform child in taskListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create task items for active tasks
        var activeTasks = taskManager.ActiveTasks;
        foreach (var task in activeTasks)
        {
            if (task != null)
                CreateTaskItem(task);
        }
    }
    
    private void CreateTaskItem(StoreTask task)
    {
        if (taskItemPrefab == null) return;
        
        GameObject taskItem = Instantiate(taskItemPrefab, taskListContainer);
        
        // Find components in the task item
        TextMeshProUGUI taskNameText = taskItem.GetComponentInChildren<TextMeshProUGUI>();
        Slider taskProgressSlider = taskItem.GetComponentInChildren<Slider>();
        Image background = taskItem.GetComponent<Image>();
        
        if (taskNameText != null)
        {
            taskNameText.text = task.TaskName;
            
            // Color based on urgency
            float urgencyRatio = task.TimeRemaining / task.MaxDuration;
            if (urgencyRatio < 0.25f)
                taskNameText.color = urgentTaskColor;
            else
                taskNameText.color = normalTaskColor;
        }
        
        if (taskProgressSlider != null)
        {
            taskProgressSlider.value = task.TimeRemaining / task.MaxDuration;
        }
        
        if (background != null)
        {
            float urgencyRatio = task.TimeRemaining / task.MaxDuration;
            if (urgencyRatio < 0.25f)
                background.color = Color.Lerp(Color.red, Color.white, 0.8f);
            else
                background.color = Color.white;
        }
    }
    
    public void ShowInteractionPrompt(string text)
    {
        if (interactionPrompt != null && interactionText != null)
        {
            SetPanelActive(interactionPrompt, true);
            interactionText.text = text;
        }
    }
    
    public void HideInteractionPrompt()
    {
        SetPanelActive(interactionPrompt, false);
    }
    
    private void ReturnToMainMenu()
    {
        if (gameManager != null)
        {
            gameManager.ReturnToMainMenu(); // This will reset to main menu state
        }
    }
    
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}