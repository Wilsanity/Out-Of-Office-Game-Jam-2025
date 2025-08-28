using UnityEngine;

public class CommonInteractions : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    protected void PlaySfx(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
}
