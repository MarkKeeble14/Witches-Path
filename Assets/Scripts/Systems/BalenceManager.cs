using System;
using System.Collections.Generic;
using UnityEngine;

public class BalenceManager : MonoBehaviour
{
    public static BalenceManager _Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> artifactSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> bookSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> afflictionSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> spellSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> mapNodeSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();


    public float GetValue(ArtifactLabel artifactLabel, string identifier)
    {
        return artifactSpecDict[artifactLabel.ToString()][identifier];
    }

    public float GetValue(BookLabel bookLabel, string specIdentifier)
    {
        return bookSpecDict[bookLabel.ToString()][specIdentifier];
    }

    public float GetValue(AfflictionType afflictionType, string identifier)
    {
        return afflictionSpecDict[afflictionType.ToString()][identifier];
    }

    public float GetValue(SpellLabel spellLabel, string identifier)
    {
        float v = spellSpecDict[spellLabel.ToString()][identifier];
        if (GameManager._Instance.HasBook(BookLabel.ClarksTimeCard) && identifier.Equals("Cooldown"))
        {
            v /= 2;
        }
        return v;
    }

    public float GetValue(MapNodeType type, string identifier)
    {
        return mapNodeSpecDict[type.ToString()][identifier];
    }

    private void Awake()
    {
        _Instance = this;
    }
}
