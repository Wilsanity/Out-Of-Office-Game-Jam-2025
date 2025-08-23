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

    private float totalChance = 0f;
    private float spawnTimer = 0f;


    void Start() {
        foreach(TaskSlot ts in taskSlots) {
            totalChance += ts.spawnChance;
        }
    }

    void Update() {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer < 0) {
            SpawnWeightedRandom();
        }
    }

    void SpawnWeightedRandom() {
        float r = Random.value * totalChance;
        for (int i = 0; i < taskSlots.Length; i++) {
            r -= taskSlots[i].spawnChance;
            if (r <= 0) {
                Spawn(i);
                spawnTimer += taskSlots[i].spawnDelay;
                break;
            }
        }
    }

    void Spawn(int taskIndex) {
        TaskSpawnPoint[] spawns = GetComponentsInChildren<TaskSpawnPoint>();
        int nAvailable = 0;
        foreach (TaskSpawnPoint tsp in spawns) {
            if (tsp.allowSpawn[taskIndex]) {
                nAvailable ++;
            }
        }
        int randSpawnIndex = Random.Range(0, nAvailable);
        foreach (TaskSpawnPoint tsp in spawns) {
            if (tsp.allowSpawn[taskIndex]) {
                if (randSpawnIndex <= 0) {
                    GameObject.Instantiate(taskSlots[randSpawnIndex].taskPrefab, tsp.transform.position, tsp.transform.rotation);
                    break;
                }
                randSpawnIndex--;
            }
        }
    }
}
