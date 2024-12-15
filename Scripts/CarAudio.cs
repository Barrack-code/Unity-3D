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
    private CarController carController;
    
    void Start()
    {
        Debug.Log("CarAudio: Start called");
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();
        
        if (audioSource == null)
        {
            Debug.LogError("CarAudio: No AudioSource found!");
            return;
        }

        if (audioSource.clip == null)
        {
            Debug.LogError("CarAudio: No AudioClip assigned to AudioSource!");
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
        if (audioSource == null || !audioSource.enabled || carController == null) return;

        // Get current speed from car controller
        float currentSpeed = carController.GetCurrentSpeed();
        float speedRatio = currentSpeed / carController.maxSpeed;
        
        // Calculate target pitch and volume
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
        float targetVolume = Mathf.Lerp(idleVolume, maxVolume, speedRatio);

        // Smoothly adjust pitch and volume
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * smoothing);
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * smoothing);

        // Apply pitch and volume
        audioSource.pitch = currentPitch;
        audioSource.volume = currentVolume;

        // Ensure audio is playing if it should be
        if (!audioSource.isPlaying && audioSource.enabled)
        {
            audioSource.Play();
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
            audioSource.mute = mute;
        }
    }
}
