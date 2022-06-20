using UnityEngine;

public class StepSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private AudioSource audioSource;
    
    // On Step animation event
    public void Step()
    {
        print("Step");
        
        audioSource.clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(audioSource.clip);
    }
}