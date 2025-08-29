using UnityEngine;

public class BlinkHighlight : MonoBehaviour {
    public Color highlightColor = Color.yellow;
    public float blinkSpeed = 2f;

    private Renderer rend;
    private Color originalEmission;
    private bool isHighlighting = false;

    [SerializeField] private float highlightDuration;

    void Start() {
        rend = GetComponent<Renderer>();
        // Make sure material supports emission
        // rend.material.EnableKeyword("_EMISSION");
        // originalEmission = rend.material.GetColor("_EmissionColor");
        originalEmission = rend.material.color;
    }

    void Update() {
        if (isHighlighting) {
            // float emission = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            // Color finalColor = highlightColor * emission;
            // rend.material.SetColor("_EmissionColor", finalColor);
            
            //rend.material.SetColor("_EmissionColor", highlightColor);
            
            rend.material.color = Color.Lerp(originalEmission, highlightColor, blinkSpeed * Time.deltaTime);
        }
    }

    public void ShowHighlightForDuration(float duration, bool highlighted)
    {
        isHighlighting = highlighted;
        Invoke("StopHighlight",  duration);
    }

    public void Highlight(bool highlighted)
    {
        if (highlighted) StartHighlight();
        else StopHighlight();
    }

    public void StartHighlight() => isHighlighting = true;
    
    public void StopHighlight() {
        isHighlighting = false;
        // rend.material.SetColor("_EmissionColor", originalEmission);
        rend.material.color = originalEmission;
    }
}