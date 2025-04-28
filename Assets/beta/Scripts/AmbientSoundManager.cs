using UnityEngine;
using System.Collections;

/// <summary>
/// Manages ambient environmental sounds like looping background noise and periodic effects.
/// </summary>
public class AmbientSoundManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("The main ambient sound that loops continuously.")]
    [SerializeField] private AudioClip ambientLoopClip; // Ambient00

    [Tooltip("An ambient sound effect that plays periodically.")]
    [SerializeField] private AudioClip ambientOneShotClip; // Ambient01

    [Tooltip("A wind sound effect that plays periodically.")]
    [SerializeField] private AudioClip windClip; // Wind01

    [Header("Audio Sources (Auto-created)")]
    [Tooltip("AudioSource for the main ambient loop.")]
    private AudioSource ambientLoopSource;

    [Tooltip("AudioSource for the periodic ambient sound.")]
    private AudioSource ambientOneShotSource;

    [Tooltip("AudioSource for the periodic wind sound.")]
    private AudioSource windSource;

    [Header("Timing (Seconds)")]
    [Tooltip("How often the Ambient01 sound plays.")]
    [SerializeField] private float ambientOneShotInterval = 38f;

    [Tooltip("How often the Wind01 sound plays.")]
    [SerializeField] private float windInterval = 74f;

    [Header("Volume Settings")]
    [Tooltip("Volume for the main ambient loop.")]
    [SerializeField] [Range(0f, 1f)] private float ambientLoopVolume = 0.4f;

    [Tooltip("Volume for the periodic ambient sound.")]
    [SerializeField] [Range(0f, 1f)] private float ambientOneShotVolume = 0.6f;

    [Tooltip("Volume for the periodic wind sound.")]
    [SerializeField] [Range(0f, 1f)] private float windVolume = 0.7f;

    // Timers
    private float ambientOneShotTimer;
    private float windTimer;

    void Start()
    {
        // --- Create and Configure AudioSources ---

        // Ambient Loop (Ambient00)
        ambientLoopSource = SetupAudioSource("AmbientLoopSource", ambientLoopClip, true, ambientLoopVolume);
        if (ambientLoopSource != null)
        {
            ambientLoopSource.Play(); // Start the loop immediately
            Debug.Log("[AmbientSoundManager] Started Ambient Loop (Ambient00).");
        }

        // Periodic Ambient (Ambient01)
        ambientOneShotSource = SetupAudioSource("AmbientOneShotSource", ambientOneShotClip, false, ambientOneShotVolume);
        if (ambientOneShotSource != null)
        {
            ambientOneShotTimer = ambientOneShotInterval; // Initialize timer
            Debug.Log($"[AmbientSoundManager] Initialized Ambient One Shot (Ambient01) with interval {ambientOneShotInterval}s.");
        }


        // Periodic Wind (Wind01)
        windSource = SetupAudioSource("WindSource", windClip, false, windVolume);
         if (windSource != null)
        {
             windTimer = windInterval; // Initialize timer
            Debug.Log($"[AmbientSoundManager] Initialized Wind (Wind01) with interval {windInterval}s.");
        }

    }

    /// <summary>
    /// Helper method to create and configure an AudioSource component on this GameObject.
    /// </summary>
    /// <param name="sourceName">Name for the child GameObject holding the AudioSource.</param>
    /// <param name="clip">The AudioClip to assign.</param>
    /// <param name="loop">Whether the sound should loop.</param>
    /// <param name="volume">The volume for the sound.</param>
    /// <returns>The configured AudioSource, or null if the clip is missing.</returns>
    AudioSource SetupAudioSource(string sourceName, AudioClip clip, bool loop, float volume)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[AmbientSoundManager] AudioClip for '{sourceName}' is not assigned in the Inspector. Source not created.");
            return null;
        }

        // Create a child GameObject to hold the source for better organization
        GameObject sourceGO = new GameObject(sourceName);
        sourceGO.transform.parent = this.transform; // Attach to this manager object
        sourceGO.transform.localPosition = Vector3.zero;

        AudioSource source = sourceGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false; // We control playback manually
        source.volume = volume;
        source.spatialBlend = 0f; 
        // Configure other 3D sound settings if needed (Min/Max Distance, etc.)
        // source.minDistance = 10f;
        // source.maxDistance = 500f;

        return source;
    }


    void Update()
    {
        // --- Handle Periodic Sounds ---

        // Timer for Ambient01
        if (ambientOneShotSource != null && ambientOneShotInterval > 0)
        {
            ambientOneShotTimer -= Time.deltaTime;
            if (ambientOneShotTimer <= 0f)
            {
                ambientOneShotSource.Play(); // Play the one-shot sound
                ambientOneShotTimer = ambientOneShotInterval; // Reset timer
                // Debug.Log("[AmbientSoundManager] Played Ambient01."); // Optional: Log playback
            }
        }

        // Timer for Wind01
        if (windSource != null && windInterval > 0)
        {
            windTimer -= Time.deltaTime;
            if (windTimer <= 0f)
            {
                windSource.Play(); // Play the one-shot sound
                windTimer = windInterval; // Reset timer
                // Debug.Log("[AmbientSoundManager] Played Wind01."); // Optional: Log playback
            }
        }
    }
}

