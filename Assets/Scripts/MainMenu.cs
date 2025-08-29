using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [SerializeField] private GameObject splashScreenContainer;
    [SerializeField] private GameObject levelSelectContainer;
    private void Start() {
        if (levelSelectContainer) {
            Button[] levelSelectButtons = levelSelectContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < levelSelectButtons.Length; i++) {
                Button button = levelSelectButtons[i];
                int cachedI = i + 1;
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText) {
                    buttonText.text = cachedI.ToString();

                }

                button.onClick.AddListener(() => SceneManager.LoadScene("Level" + cachedI.ToString()));
            }
        }
    }

    public void GoToLevelSelect() {
        splashScreenContainer.SetActive(false);
        levelSelectContainer.SetActive(true);
    }
}
