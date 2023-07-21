using UnityEngine;

namespace Map
{
    public enum NodeType
    {
        MinorEnemy,
        Boss,
        Event,
        Clothier,
        Apothecary,
        Library,
        Campfire
    }
}

namespace Map
{
    [CreateAssetMenu]
    public class NodeBlueprint : ScriptableObject
    {
        public Sprite sprite;
        public NodeType nodeType;
    }
}