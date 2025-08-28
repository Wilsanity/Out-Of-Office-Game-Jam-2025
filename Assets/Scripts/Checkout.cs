using UnityEngine;

public class Checkout : MonoBehaviour, Interactable {
    [SerializeField] float itemDelay = 1f;
    [SerializeField] int itemCount = 5;
    [SerializeField] float damagePerSecond = 8f;
    int readyItemCount = 0;
    float itemCountdownSeconds = 0;

    private GameManager gm;

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
            // TODO: "BEEP!"
            if (readyItemCount <= 0 && itemCount <= 0) {
                GameObject.Destroy(gameObject);
                //TODO: "Thank you, come again!"
            }

            GetComponentInChildren<TMPro.TMP_Text>().text = readyItemCount.ToString();
        } else {
            // TODO: "sad beep"
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
