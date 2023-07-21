using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private SerializableDictionary<string, SimpleAudioClipContainer> sfxDict = new SerializableDictionary<string, SimpleAudioClipContainer>();

    [Header("References")]
    [SerializeField] private AudioMixer mixer;
    private List<AudioSource> audioSourceArray;
    [SerializeField] private Transform parentSpawnedTo;
    [SerializeField] private AudioSource audioSourcePrefab;

    public static AudioManager _Instance { get; private set; }

    private void Awake()
    {
        _Instance = this;

        audioSourceArray = new List<AudioSource>();
        audioSourceArray.AddRange(GetComponentsInChildren<AudioSource>());
    }

    private AudioSource GetAudioSource()
    {
        if (audioSourceArray.Count == 0)
        {
            AudioSource spawned = Instantiate(audioSourcePrefab, parentSpawnedTo);
            audioSourceArray.Add(spawned);
        }
        return audioSourceArray[0];
    }

    private IEnumerator PlayFromSourceUninterrupted(SimpleAudioClipContainer clip, float pitchAdjustment)
    {
        AudioSource source = GetAudioSource();

        audioSourceArray.Remove(source);
        clip.Source = source;
        source.volume = clip.Volume;

        clip.PlayWithPitchAdjustment(pitchAdjustment);

        yield return new WaitUntil(() => !source.isPlaying);

        audioSourceArray.Add(source);
    }

    public void PlayFromSFXDict(string key)
    {
        StartCoroutine(PlayFromSourceUninterrupted(sfxDict[key], 0.0f));
    }

    public void PlayFromSFXDict(string key, float pitchAdjustment)
    {
        StartCoroutine(PlayFromSourceUninterrupted(sfxDict[key], pitchAdjustment));
    }
}
