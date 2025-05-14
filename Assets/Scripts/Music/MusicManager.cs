using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public AudioClip[] ambientClips;
    public AudioSource audioSource;

    private int currentClipIndex = -1; // Start before first to cycle properly

    public void PlayNextAmbientClip()
    {
        if (ambientClips.Length == 0 || audioSource == null) return;

        // Cycle to the next index
        currentClipIndex = (currentClipIndex + 1) % ambientClips.Length;

        // Play the new clip
        audioSource.clip = ambientClips[currentClipIndex];
        audioSource.Play();
    }

}
