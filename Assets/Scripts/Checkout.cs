using UnityEngine;

public class Checkout : MonoBehaviour, Interactable {
    [SerializeField] float itemDelay = 1f;
    [SerializeField] int itemCount = 5;
    int readyItemCount = 0;
    float itemCountdownSeconds = 0;

    public void Start() {
        // Place myself on a valid spawn point
        TaskSpawnPoint tsp = TaskSpawnPoint.FindRandomSpawnPoint(tsp => tsp.allowCheckout == true);
        if (tsp != null) {
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
        }

        if (itemCount == 0) {
            //TODO: cause heavy damage over time
        }
    }
}
