using UnityEngine;

public interface Interactable
{
    public void SetHighlight(bool highlighted);
    public void Interact(Player source);
}
