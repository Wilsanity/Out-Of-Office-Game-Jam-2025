using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class TaskSpawnPoint : MonoBehaviour {
    public bool allowSpill = true;
    // Configuration options go here


    // Search for an available spawn point that fulfills the filter condition
    public static TaskSpawnPoint FindRandomSpawnPoint(System.Func<TaskSpawnPoint, bool> filter) {
        IEnumerable<TaskSpawnPoint> spawns = Object.FindObjectsByType<TaskSpawnPoint>(FindObjectsSortMode.None)
            .Where(filter)
            .Where(tsp => tsp.transform.childCount == 0); // Don't spawn two tasks on top of each other

        if (spawns.Count() > 0) {
            return spawns.ElementAt(Random.Range(0, spawns.Count()));
        }

        return null;
    }
}
