using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [SerializeField] private GameObject splashScreenContainer;
    [SerializeField] private GameObject levelSelectContainer;

    public void GoToLevelSelect() {
        splashScreenContainer.SetActive(false);
        levelSelectContainer.SetActive(true);
    }

    public void LoadScene(string name) {
        SceneManager.LoadScene(name);
    }
}
