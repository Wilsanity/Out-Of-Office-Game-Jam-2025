using UnityEngine;

public class SpawnableTask : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    protected void PlaySfx(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
}
