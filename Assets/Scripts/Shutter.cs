using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Shutter : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shutterSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayShutter()
    {
        if (audioSource != null && shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }
    }
}
