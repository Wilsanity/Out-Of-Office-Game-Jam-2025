using UnityEngine;

public class Checkout : SpawnableTask, Interactable {
    [SerializeField] float itemDelay = 1f;
    [SerializeField] int itemCount = 5;
    [SerializeField] float damagePerSecond = 8f;
    [SerializeField] float healthRefill = 20f;
    int readyItemCount = 0;
    float itemCountdownSeconds = 0;

    private GameManager gm;

    [Header("Audio")] 
    [SerializeField] private AudioClip beep;
    [SerializeField] private AudioClip thankYou;
    [SerializeField] private AudioClip sadBeep;

    public void Start() {
        // Place myself on a valid spawn point
        TaskSpawnPoint tsp = TaskSpawnPoint.FindRandomSpawnPoint(tsp => tsp.allowCheckout == true);
        if (tsp != null) {
            transform.SetParent(tsp.transform, false);
        } else {
            Debug.LogWarning("No spawn points found!");
            GameObject.Destroy(gameObject);
        }

        gm = Object.FindAnyObjectByType<GameManager>();
    }

    public void SetHighlight(bool highlighted) {
        //TODO: implement highlight
    }

    public void Interact(Player source) {
        if (readyItemCount > 0) {
            readyItemCount--;
            PlaySfx(beep);
            if (readyItemCount <= 0 && itemCount <= 0) {
                gm.ChangeStoreScore(healthRefill);
                PlaySfx(thankYou);
                GameObject.Destroy(gameObject);
            }

            GetComponentInChildren<TMPro.TMP_Text>().text = readyItemCount.ToString();
        } else {
            PlaySfx(sadBeep);
        }
    }

    public void Update() {
        if (itemCount > 0) {
            itemCountdownSeconds -= Time.deltaTime;
            if (itemCountdownSeconds < 0) {
                itemCountdownSeconds += itemDelay;
                readyItemCount++;
                GetComponentInChildren<TMPro.TMP_Text>().text = readyItemCount.ToString();
                itemCount--;
                if (itemCount == 0) {
                    //TODO: Do something to attract attention from the player
                }
            }
        } else { // ready and waiting for player
            gm.ChangeStoreScore(-damagePerSecond * Time.deltaTime);
        }
    }
}
