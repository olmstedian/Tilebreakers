using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Level Sounds")]
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip levelFailedSound;
    [SerializeField] private AudioClip gameCompleteSound;
    
    [Header("Settings")]
    [SerializeField] [Range(0f, 1f)] private float volume = 1.0f;
    [SerializeField] private bool useSoundEffects = true;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayLevelCompleteSound()
    {
        if (!useSoundEffects || levelCompleteSound == null) return;
        
        audioSource.PlayOneShot(levelCompleteSound, volume);
    }
    
    public void PlayLevelFailedSound()
    {
        if (!useSoundEffects || levelFailedSound == null) return;
        
        audioSource.PlayOneShot(levelFailedSound, volume);
    }
    
    public void PlayGameCompleteSound()
    {
        if (!useSoundEffects || gameCompleteSound == null) return;
        
        audioSource.PlayOneShot(gameCompleteSound, volume);
    }
    
    public void SetSoundEffectsEnabled(bool enabled)
    {
        useSoundEffects = enabled;
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
    }
}
