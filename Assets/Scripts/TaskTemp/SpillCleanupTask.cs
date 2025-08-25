using UnityEngine;

public class SpillCleanupTask : StoreTask
{
    [Header("Spill Cleanup Settings")]
    [SerializeField] private float cleanupTime = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    private bool isBeingCleaned = false;
    private float cleanupProgress = 0f;
    
    protected override void StartTask()
    {
        taskName = "Clean up spill";
        priority = TaskPriority.Medium;
        
        // Visual setup - you can replace with spill sprite/model
        if (taskIndicator == null)
        {
            // Create a simple visual indicator
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = new Vector3(1f, 0.1f, 1f);
            indicator.GetComponent<Renderer>().material.color = Color.brown; // Spill color
            taskIndicator = indicator;
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (!isActive) return;
        
        // Handle cleanup interaction
        if (isPlayerInRange && Input.GetKey(interactKey))
        {
            if (!isBeingCleaned)
            {
                isBeingCleaned = true;
                Debug.Log("Started cleaning spill...");
            }
            
            // Progress cleanup
            cleanupProgress += Time.deltaTime;
            
            // Update visual feedback (scale down the spill)
            if (taskIndicator != null)
            {
                float progress = cleanupProgress / cleanupTime;
                float scale = Mathf.Lerp(1f, 0.1f, progress);
                taskIndicator.transform.localScale = new Vector3(scale, 0.1f, scale);
            }
            
            // Complete when done
            if (cleanupProgress >= cleanupTime)
            {
                CompleteTask(true);
            }
        }
        else if (isBeingCleaned)
        {
            // Stop cleaning if player releases key or leaves area
            isBeingCleaned = false;
            Debug.Log("Stopped cleaning spill");
        }
    }
    
    protected override void OnPlayerEnterRange()
    {
        Debug.Log($"Press {interactKey} to clean up spill");
        
        // Show UI prompt
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowInteractionPrompt($"Press {interactKey} to clean up spill");
        }
    }
    
    protected override void OnPlayerExitRange()
    {
        if (isBeingCleaned)
        {
            isBeingCleaned = false;
            Debug.Log("Stopped cleaning spill - player left area");
        }
        
        // Hide UI prompt
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.HideInteractionPrompt();
        }
    }
    
    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
        
        // Additional visual feedback for cleaning progress
        if (isBeingCleaned && taskIndicator != null)
        {
            // Flash green while being cleaned
            float flash = Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f;
            Color cleanColor = Color.Lerp(Color.brown, Color.green, flash);
            taskIndicator.GetComponent<Renderer>().material.color = cleanColor;
        }
    }
}
