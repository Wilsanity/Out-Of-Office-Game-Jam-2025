using UnityEngine;

[System.Serializable]
public struct TaskSlot {
    public GameObject taskPrefab;
    public float spawnChance;
    public float spawnDelay;
}

public class TaskSpawner : MonoBehaviour
{
    public TaskSlot[] taskSlots;

    private float spawnTimer = 0f;

    void Update() {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer < 0) {
            SpawnWeightedRandom();
        }
    }

    // Spawn a random task, weighted by spawnChance
    void SpawnWeightedRandom() {
        float totalChance = 0f;
        foreach (TaskSlot ts in taskSlots) {
            totalChance += ts.spawnChance;
        }

        float r = Random.value * totalChance;

        for (int i = 0; i < taskSlots.Length; i++) {
            r -= taskSlots[i].spawnChance;
            if (r <= 0) {
                GameObject.Instantiate(taskSlots[i].taskPrefab);

                // Wait for spawnDelay before spawning the next task
                spawnTimer += taskSlots[i].spawnDelay;
                break;
            }
        }
    }
}
