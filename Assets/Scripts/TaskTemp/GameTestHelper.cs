using UnityEngine;

public class GameTestHelper : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private KeyCode startGameKey = KeyCode.Space;
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    
    [SerializeField] private GameManager gameManager;
    
    private void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found! Please add GameManager to scene.");
        }
    }
    
    private void Update()
    {
        if (gameManager == null) return;
        
        // Test controls
        if (Input.GetKeyDown(startGameKey))
        {
            if (gameManager.CurrentState == GameManager.GameState.MainMenu || 
                gameManager.CurrentState == GameManager.GameState.GameOver)
            {
                gameManager.StartGame();
                Debug.Log("Game started!");
            }
        }
        
        if (Input.GetKeyDown(restartKey))
        {
            gameManager.RestartGame();
            Debug.Log("Game restarted!");
        }
    }
    
    private void OnGUI()
    {
        if (gameManager == null) return;
        
        // Simple debug UI
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        
        GUILayout.Label($"Game State: {gameManager.CurrentState}");
        GUILayout.Label($"Store Score: {gameManager.StoreScore}");
        GUILayout.Label($"Level: {gameManager.CurrentLevel}");
        GUILayout.Label($"Time: {gameManager.TimeRemaining:F1}s");
        
        GUILayout.Space(10);
        
        if (gameManager.CurrentState == GameManager.GameState.MainMenu ||
            gameManager.CurrentState == GameManager.GameState.GameOver)
        {
            GUILayout.Label($"Press {startGameKey} to start game");
        }
        
        GUILayout.Label($"Press {restartKey} to restart");
        
        GUILayout.EndArea();
    }
}
