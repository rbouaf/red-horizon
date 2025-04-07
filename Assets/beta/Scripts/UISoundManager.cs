using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UISoundManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundMapping
    {
        public Selectable uiElement;  // Works for buttons, toggles, etc.
        public AudioClip interactionSound;
    }

    [Header("Global Settings")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip defaultSound;

    [Header("Element-Specific Sounds")]
    [SerializeField] private List<SoundMapping> soundMappings = new List<SoundMapping>();

    void Awake()
    {
        InitializeAudioSource();
        SetupAllUiSounds();
    }

    private void InitializeAudioSource()
    {
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.spatialBlend = 0f; // Pure 2D sound
            uiAudioSource.playOnAwake = false;
            uiAudioSource.volume = 1f;
        }
    }

    private void SetupAllUiSounds()
    {
        foreach (SoundMapping mapping in soundMappings)
        {
            if (mapping.uiElement != null)
            {
                // For buttons
                if (mapping.uiElement is Button button)
                {
                    button.onClick.AddListener(() => PlaySound(mapping.interactionSound));
                }
                // For toggles
                else if (mapping.uiElement is Toggle toggle)
                {
                    toggle.onValueChanged.AddListener((_) => PlaySound(mapping.interactionSound));
                }
                // Add more UI types here as needed
            }
        }
    }

    public void PlaySound(AudioClip clip = null)
    {
        AudioClip soundToPlay = clip ?? defaultSound;
        if (soundToPlay != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(soundToPlay);
        }
    }

    // Optional: Universal click sound for unassigned UI elements
    public void PlayDefaultSound()
    {
        if (defaultSound != null)
        {
            uiAudioSource.PlayOneShot(defaultSound);
        }
    }
}