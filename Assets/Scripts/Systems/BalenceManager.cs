using System;
using System.Collections.Generic;
using UnityEngine;

public class BalenceManager : MonoBehaviour
{
    public static BalenceManager _Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> artifactSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> afflictionSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    public float GetValue(ArtifactLabel artifactLabel, string identifier)
    {
        return artifactSpecDict[artifactLabel.ToString()][identifier];
    }

    public float GetValue(AfflictionType afflictionType, string identifier)
    {
        return afflictionSpecDict[afflictionType.ToString()][identifier];
    }

    private void Awake()
    {
        _Instance = this;
    }
}
