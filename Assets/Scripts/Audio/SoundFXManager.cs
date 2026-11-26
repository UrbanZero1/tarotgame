using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    
    [SerializeField] private AudioSource audioSourcePrefab;
    
    private void Awake()
    {
        if (instance == null) {
            instance = this;
        }
    }
    
    public void PlaySound(AudioClip clip, Transform spawnTransform, float volume = 1.0f)
    {
        if (clip == null) {
            Debug.LogWarning("Attempted to play a null AudioClip.");
            return;
        }

        AudioSource audioSource = Instantiate(audioSourcePrefab, spawnTransform.position, Quaternion.identity);
        
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        
        float length = audioSource.clip.length;
        
        Destroy(audioSource.gameObject, length);
    }
}
