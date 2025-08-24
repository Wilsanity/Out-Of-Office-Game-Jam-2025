using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    [Header("Task Settings")]
    [SerializeField] private List<StoreTask> activeTasks = new List<StoreTask>();
    [SerializeField] private Transform[] taskSpawnPoints;
    
    [Header("Task Prefabs")]
    [SerializeField] private GameObject[] taskPrefabs;
    
    [SerializeField] private GameManager gameManager;
    private bool isSpawning = false;
    private int currentLevel = 1;
    private int maxConcurrentTasks = 3;
    private float spawnInterval = 10f;
    
    public void StartLevel(int level, int maxTasks, float interval)
    {
        currentLevel = level;
        maxConcurrentTasks = maxTasks;
        spawnInterval = interval;
        
        // Clear any existing tasks
        ClearAllTasks();
        
        // Start spawning
        isSpawning = true;
        StartCoroutine(TaskSpawnLoop());
        
        // Spawn initial tasks
        for (int i = 0; i < Mathf.Min(2, maxConcurrentTasks); i++)
        {
            SpawnRandomTask();
        }
    }
    
    public void StopLevel()
    {
        isSpawning = false;
        StopAllCoroutines();
        ClearAllTasks();
    }
    
    private IEnumerator TaskSpawnLoop()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (activeTasks.Count < maxConcurrentTasks)
            {
                SpawnRandomTask();
            }
        }
    }
    
    private void SpawnRandomTask()
    {
        if (taskPrefabs.Length == 0 || taskSpawnPoints.Length == 0)
        {
            Debug.LogWarning("No task prefabs or spawn points assigned!");
            return;
        }
        
        // Choose random task type
        GameObject taskPrefab = taskPrefabs[Random.Range(0, taskPrefabs.Length)];
        
        // Choose random spawn point
        Transform spawnPoint = taskSpawnPoints[Random.Range(0, taskSpawnPoints.Length)];
        
        // Spawn task
        GameObject taskObj = Instantiate(taskPrefab, spawnPoint.position, spawnPoint.rotation);
        StoreTask task = taskObj.GetComponent<StoreTask>();
        
        if (task != null)
        {
            activeTasks.Add(task);
            task.Initialize(this, currentLevel);
            Debug.Log($"Spawned task: {task.TaskName} at {spawnPoint.position}");
        }
    }
    
    public void OnTaskCompleted(StoreTask task, bool wasSuccessful)
    {
        if (activeTasks.Contains(task))
        {
            activeTasks.Remove(task);
            
            if (gameManager != null)
            {
                if (wasSuccessful)
                {
                    gameManager.ChangeStoreScore(task.RewardAmount);
                    Debug.Log($"Task completed successfully: +{task.RewardAmount} store score");
                }
                else
                {
                    gameManager.ChangeStoreScore(-task.PenaltyAmount);
                    Debug.Log($"Task failed: -{task.PenaltyAmount} store score");
                }
            }
            
            Destroy(task.gameObject);
        }
    }
    
    public void OnTaskTimeout(StoreTask task)
    {
        OnTaskCompleted(task, false);
    }
    
    private void ClearAllTasks()
    {
        foreach (StoreTask task in activeTasks.ToArray())
        {
            if (task != null)
            {
                Destroy(task.gameObject);
            }
        }
        activeTasks.Clear();
    }
    
    // Public getters
    public int ActiveTaskCount => activeTasks.Count;
    public List<StoreTask> ActiveTasks => new List<StoreTask>(activeTasks);
}
