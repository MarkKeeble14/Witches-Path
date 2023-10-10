using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ContinuousAudioSource : MonoBehaviour
{
    [SerializeField] private bool initActive;
    public bool Active { get; set; }

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Active = initActive;
    }

    private void Update()
    {
        audioSource.mute = !Active;
    }
}
