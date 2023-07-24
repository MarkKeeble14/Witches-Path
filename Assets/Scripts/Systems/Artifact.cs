using UnityEngine;

[CreateAssetMenu]
public class Artifact : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private string identifier;

    public Sprite GetSprite()
    {
        return sprite;
    }

    public string GetIdentifier()
    {
        return identifier;
    }
}
