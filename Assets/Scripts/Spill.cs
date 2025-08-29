using UnityEngine;

public class Spill : MonoBehaviour, Interactable {

    [SerializeField] float damagePerSecond = 4f;
    [SerializeField] float healthRefill = 10f;

    [SerializeField] Color highlightColor = Color.yellow;
    private Color baseColor = Color.blue;

    private GameManager gm;
    
    [Header("Audio")]
    [SerializeField] private AudioClip cleanSpill;
    
    public void Start() {

        // Randomly scale the spill
        float scale = Random.Range(0.5f, 0.8f);
        transform.localScale = new Vector3(scale, 0.01f, scale);

        // Place myself on a valid spawn point
        TaskSpawnPoint tsp = TaskSpawnPoint.FindRandomSpawnPoint(tsp => tsp.allowSpill == true);
        if(tsp != null) {
            transform.SetParent(tsp.transform, false);
        } else {
            Debug.LogWarning("No spawn points found!");
            GameObject.Destroy(gameObject);
        }

        gm = Object.FindAnyObjectByType<GameManager>();

        baseColor = GetComponentInChildren<MeshRenderer>().material.color;
    }

    public void SetHighlight(bool highlighted) {
        GetComponentInChildren<MeshRenderer>().material.color = highlighted ? highlightColor : baseColor;
    }

    public void Interact(Player source) {
        gm.ChangeStoreScore(healthRefill);
        source.PlaySfx(cleanSpill);
        gameObject.SetActive(false);
        Invoke("DestroySpill", cleanSpill.length);
    }

    private void DestroySpill() {
        Destroy(gameObject);
    }
    
    public void Update() {
        gm.ChangeStoreScore(-damagePerSecond * Time.deltaTime);
    }
}
