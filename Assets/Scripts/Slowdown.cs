using UnityEngine;

public class Slowdown : MonoBehaviour {
    [SerializeField] private float size = 1f;
    [SerializeField] private float durationSeconds = 8f;

    public float GetSlowdownAtPoint(Vector3 point) {
        Vector3 offset = transform.position - point;
        return Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.z)) <= size ? .2f : 1f;
    }

    public void Start() {
        // Place myself on a valid spawn point
        TaskSpawnPoint tsp = TaskSpawnPoint.FindRandomSpawnPoint(tsp => tsp.allowTantrum == true);
        if (tsp != null) {
            transform.SetParent(tsp.transform, false);
        } else {
            Debug.LogWarning("No spawn points found!");
            GameObject.Destroy(gameObject);
        }
    }
    public void Update() {
        durationSeconds -= Time.deltaTime;
        if (durationSeconds < 0) {
            GameObject.Destroy(gameObject);
        }
    }
}
