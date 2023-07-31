using System;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager _Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> artifactSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    public float GetValue(ArtifactLabel artifactLabel, string identifier)
    {
        return artifactSpecDict[artifactLabel.ToString()][identifier];
    }

    private void Awake()
    {
        _Instance = this;
    }
}
