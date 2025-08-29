using UnityEngine;

public class Checkout : MonoBehaviour, Interactable {
    [SerializeField] float itemDelay = 1f;
    [SerializeField] int itemCount = 5;
    [SerializeField] float damagePerSecond = 8f;
    [SerializeField] float healthRefill = 20f;
    int readyItemCount = 0;
    float itemCountdownSeconds = 0;

    private GameManager gm;

    [SerializeField] Color highlightColor = Color.yellow;
    private Color baseColor = Color.blue;

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
            return;
        }

        gm = Object.FindAnyObjectByType<GameManager>();

        baseColor = GetComponentInParent<MeshRenderer>().material.color;

        GetComponentInChildren<TMPro.TMP_Text>().GetComponent<RectTransform>().rotation = Quaternion.Euler(90, 0, 0);
    }

    public void SetHighlight(bool highlighted) {
        GetComponentInParent<MeshRenderer>().material.color = highlighted ? highlightColor : baseColor;
    }

    public void Interact(Player source) {
        if (readyItemCount > 0) {
            readyItemCount--;
            source.PlaySfx(beep);
            if (readyItemCount <= 0 && itemCount <= 0) {
                gm.ChangeStoreScore(healthRefill);
                if (Random.value > .8) {
                    source.PlaySfx(thankYou);
                }
                SetHighlight(false);
                GameObject.Destroy(gameObject);
            }

            GetComponentInChildren<TMPro.TMP_Text>().text = readyItemCount.ToString();
        } else {
            source.PlaySfx(sadBeep);
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
