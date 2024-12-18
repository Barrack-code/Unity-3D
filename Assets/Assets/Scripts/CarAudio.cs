using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 1.5f;
    [SerializeField] private float idleVolume = 0.2f;
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float smoothing = 5f;
    
    private AudioSource audioSource;
    private Rigidbody rb;
    private float currentPitch = 1f;
    private float currentVolume;
    private Backup carController;
    private bool isEngineMuted = false;
    
    void Start()
    {
        Debug.Log("CarAudio: Start called");
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<Backup>();
        
        if (audioSource == null)
        {
            Debug.LogError($"{gameObject.name}: No AudioSource found!");
            return;
        }

        if (carController == null)
        {
            Debug.LogError($"{gameObject.name}: No Backup component found!");
            return;
        }
        
        // Set up the audio source
        audioSource.loop = true;
        audioSource.spatialBlend = 1f; // Full 3D sound
        audioSource.pitch = minPitch;
        audioSource.volume = idleVolume;
        
        // Start playing
        if (!audioSource.isPlaying && audioSource.enabled)
        {
            audioSource.Play();
            Debug.Log("CarAudio: Started playing audio");
        }
    }

    void Update()
    {
        if (audioSource == null || rb == null || carController == null) return;

        // Calculate pitch based on current speed
        float currentSpeed = rb.linearVelocity.magnitude;  
        float normalizedSpeed = currentSpeed / carController.maxSpeed;  
        
        // Adjust pitch based on speed
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * smoothing);
        
        // Set the pitch
        audioSource.pitch = currentPitch;

        // Adjust volume based on speed
        float targetVolume = Mathf.Lerp(idleVolume, maxVolume, normalizedSpeed);
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * smoothing);
        
        // Set the volume
        if (!isEngineMuted)
        {
            audioSource.volume = currentVolume;
        }
    }

    public void StopEngine()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void MuteEngine(bool mute)
    {
        if (audioSource != null)
        {
            isEngineMuted = mute;
            if (isEngineMuted)
            {
                audioSource.volume = 0f;
            }
            else
            {
                audioSource.volume = currentVolume;
            }
        }
    }
}
