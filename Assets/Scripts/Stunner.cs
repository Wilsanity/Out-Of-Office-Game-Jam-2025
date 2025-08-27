using UnityEngine;

public class Stunner : MonoBehaviour
{
    [SerializeField] float durationSeconds = 1.5f;

    void OnTriggerEnter(Collider other) {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        player.Stun(durationSeconds);
    }
}
