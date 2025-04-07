using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public GameObject menuUI;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip menuOpenSound;
    [SerializeField] private AudioClip menuCloseSound;
    
    private bool isMenuActive = false;
    private AudioSource uiAudioSource;

    void Start()
    {
        SetupAudioSource();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMenu();
        }
    }

    private void SetupAudioSource()
    {
        // Create a dedicated AudioSource for UI sounds
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        
        // Configure for UI/2D sound
        uiAudioSource.spatialBlend = 0f; // 0 = 2D, 1 = 3D
        uiAudioSource.playOnAwake = false;
        
        // Adjust these settings as needed
        uiAudioSource.volume = 1f;
        uiAudioSource.loop = false;
    }

    private void ToggleMenu()
    {
        isMenuActive = !isMenuActive;
        menuUI.SetActive(isMenuActive);
        PlayToggleSound();
    }

    private void PlayToggleSound()
    {
        if (isMenuActive)
        {
            if (menuOpenSound != null)
            {
                uiAudioSource.PlayOneShot(menuOpenSound);
            }
        }
        else
        {
            if (menuCloseSound != null)
            {
                uiAudioSource.PlayOneShot(menuCloseSound);
            }
        }
    }
}