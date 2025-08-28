using UnityEngine;

public class Spill : MonoBehaviour, Interactable {
    public void Start() {
        // Place myself on a valid spawn point
        TaskSpawnPoint tsp = TaskSpawnPoint.FindRandomSpawnPoint(tsp => tsp.allowSpill == true);
        if(tsp != null) {
            transform.SetParent(tsp.transform, false);
        } else {
            Debug.LogWarning("No spawn points found!");
            GameObject.Destroy(gameObject);
        }
    }

    public void SetHighlight(bool highlighted) {
        //TODO: implement highlight
    }

    public void Interact(Player source) {
        GameObject.Destroy(gameObject);
    }

    public void Update() {
        //TODO: cause damage over time
    }
}
