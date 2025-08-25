using UnityEngine;
using System.Collections;

public abstract class StoreTask : MonoBehaviour
{
    [Header("Task Settings")]
    [SerializeField] protected string taskName = "Generic Task";
    [SerializeField] protected float maxDuration = 60f;
    [SerializeField] protected int rewardAmount = 5;
    [SerializeField] protected int penaltyAmount = 10;
    [SerializeField] protected TaskPriority priority = TaskPriority.Medium;
    
    [Header("Visual Feedback")]
    [SerializeField] protected GameObject taskIndicator;
    [SerializeField] protected Color priorityColor = Color.yellow;
    
    protected TaskManager taskManager;
    protected float timeRemaining;
    protected bool isActive = true;
    protected bool isPlayerInRange = false;
    
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    // Events
    public System.Action<StoreTask> OnTaskStarted;
    public System.Action<StoreTask, bool> OnTaskCompleted;
    public System.Action<StoreTask> OnTaskTimeOut;
    
    // Public properties
    public string TaskName => taskName;
    public float TimeRemaining => timeRemaining;
    public float MaxDuration => maxDuration;
    public int RewardAmount => rewardAmount;
    public int PenaltyAmount => penaltyAmount;
    public TaskPriority Priority => priority;
    public bool IsActive => isActive;
    
    protected virtual void Start()
    {
        SetupTaskIndicator();
    }
    
    protected virtual void Update()
    {
        if (!isActive) return;
        
        // Update timer
        timeRemaining -= Time.deltaTime;
        
        // Check for timeout
        if (timeRemaining <= 0)
        {
            TaskTimeout();
        }
        
        // Update visual indicators
        UpdateVisuals();
    }
    
    public virtual void Initialize(TaskManager manager, int currentLevel)
    {
        taskManager = manager;
        timeRemaining = maxDuration;
        isActive = true;
        
        // Scale difficulty with level
        AdjustForLevel(currentLevel);
        
        OnTaskStarted?.Invoke(this);
        StartTask();
    }
    
    protected virtual void AdjustForLevel(int level)
    {
        // Make tasks more urgent at higher levels
        float levelMultiplier = 1f - (level * 0.1f);
        levelMultiplier = Mathf.Clamp(levelMultiplier, 0.3f, 1f);
        maxDuration *= levelMultiplier;
        timeRemaining = maxDuration;
        
        // Increase rewards/penalties
        rewardAmount += (level - 1) * 2;
        penaltyAmount += (level - 1) * 3;
    }
    
    protected abstract void StartTask();
    
    public virtual void CompleteTask(bool successful = true)
    {
        if (!isActive) return;
        
        isActive = false;
        OnTaskCompleted?.Invoke(this, successful);
        
        if (taskManager != null)
        {
            taskManager.OnTaskCompleted(this, successful);
        }
    }
    
    protected virtual void TaskTimeout()
    {
        Debug.Log($"Task {taskName} timed out!");
        OnTaskTimeOut?.Invoke(this);
        CompleteTask(false);
    }
    
    protected virtual void SetupTaskIndicator()
    {
        if (taskIndicator != null)
        {
            // Set color based on priority
            Renderer renderer = taskIndicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = GetPriorityColor();
            }
        }
    }
    
    protected virtual void UpdateVisuals()
    {
        if (taskIndicator != null)
        {
            // Flash when urgent (less than 25% time remaining)
            float urgencyThreshold = maxDuration * 0.25f;
            if (timeRemaining < urgencyThreshold)
            {
                float flash = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
                taskIndicator.SetActive(flash > 0.5f);
            }
            else
            {
                taskIndicator.SetActive(true);
            }
        }
    }
    
    protected Color GetPriorityColor()
    {
        switch (priority)
        {
            case TaskPriority.Low: return Color.green;
            case TaskPriority.Medium: return Color.yellow;
            case TaskPriority.High: return Color.orange;
            case TaskPriority.Critical: return Color.red;
            default: return Color.white;
        }
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            OnPlayerEnterRange();
        }
    }
    
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            OnPlayerExitRange();
        }
    }
    
    protected virtual void OnPlayerEnterRange() { }
    protected virtual void OnPlayerExitRange() { }
}
