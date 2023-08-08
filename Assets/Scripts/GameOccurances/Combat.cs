using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum RewardType
{
    Artifact,
    Book,
}

public abstract class Combat : GameOccurance
{
    [Header("Map")]
    [SerializeField] private DefaultAsset mapFile; // Map file (.osu format), attach from editor
    public DefaultAsset MapFile { get => mapFile; }
    [SerializeField] private AudioClip mainMusic; // Music file, attach from editor
    public AudioClip MainMusic { get => mainMusic; }
    [SerializeField] private AudioClip hitSound; // Hit sound
    public AudioClip HitSound { get => hitSound; }
    [SerializeField] private AudioClip missSound; // Hit sound
    public AudioClip MissSound { get => missSound; }

    [SerializeField] private Enemy enemy;
    public Enemy Enemy { get => enemy; }

    [SerializeField] private Vector2 minMaxCurrencyReward;
    [SerializeField] private SerializableDictionary<RewardType, Vector2> chanceForOtherRewards = new SerializableDictionary<RewardType, Vector2>();

    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");

        foreach (RewardType type in chanceForOtherRewards.Keys())
        {
            if (RandomHelper.EvaluateChanceTo(chanceForOtherRewards[type]))
            {
                switch (type)
                {
                    case RewardType.Artifact:
                        RewardManager._Instance.AddReward(GameManager._Instance.GetRandomArtifact());
                        break;
                    case RewardType.Book:
                        RewardManager._Instance.AddReward(GameManager._Instance.GetRandomBook());
                        break;
                }
            }
        }

        RewardManager._Instance.AddReward(RandomHelper.RandomIntExclusive(minMaxCurrencyReward));

        yield return RewardManager._Instance.ShowRewardScreen();

        Debug.Log("2");
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");
        yield return GameManager._Instance.StartCoroutine(CombatManager._Instance.StartCombat(this));
    }
}
