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
    private int maxTasks = 1;


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
            if (tsp.allowSpawn[taskIndex] && tsp.transform.childCount < maxTasks) {
                if (randSpawnIndex <= 0) {
                    
                    GameObject obj = GameObject.Instantiate(taskSlots[randSpawnIndex].taskPrefab, tsp.transform.position, tsp.transform.rotation);
                    obj.transform.parent = tsp.transform;
                    break;
                }
                randSpawnIndex--;
            }
        }
    }
}
