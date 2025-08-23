using UnityEngine;

public class Spill : MonoBehaviour, Interactable
{
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
