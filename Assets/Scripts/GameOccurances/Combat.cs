using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum RewardType
{
    Artifact,
    Book,
    Currency,
    ClothierCurrency
}

public enum RewardNumericalType
{
    ChanceTo,
    Between
}

[System.Serializable]
public class RewardInfo
{
    public RewardNumericalType NumericalType;
    public Vector2 Numbers;
}

public abstract class Combat : GameOccurance
{
    [SerializeField] private AudioClip mainMusic; // Music file, attach from editor
    public AudioClip MainMusic { get => mainMusic; }

    [SerializeField] private Enemy enemy;
    public Enemy Enemy { get => enemy; }

    [SerializeField]
    private SerializableDictionary<RewardType, PercentageMap<RewardInfo>> itemRewards
        = new SerializableDictionary<RewardType, PercentageMap<RewardInfo>>();

    [SerializeField]
    private SerializableDictionary<RewardType, PercentageMap<Vector2Int>> currencyRewards
    = new SerializableDictionary<RewardType, PercentageMap<Vector2Int>>();

    private void ParseItemRewardInfo(RewardInfo info, Action act)
    {
        if (info.NumericalType == RewardNumericalType.Between)
        {
            int num = RandomHelper.RandomIntInclusive(info.Numbers);
            for (int i = 0; i < num; i++)
            {
                act?.Invoke();
            }
        }
        else if (info.NumericalType == RewardNumericalType.ChanceTo)
        {
            if (RandomHelper.EvaluateChanceTo(info.Numbers))
            {
                act?.Invoke();
            }
        }
    }

    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");

        // Add 1 Charge to all Books
        GameManager._Instance.AlterAllBookCharge(1);

        foreach (RewardType type in itemRewards.Keys())
        {
            RewardInfo info = itemRewards[type].GetOption();
            switch (type)
            {
                case RewardType.Artifact:
                    ParseItemRewardInfo(info, () => RewardManager._Instance.AddReward(GameManager._Instance.GetRandomArtifact()));
                    break;
                case RewardType.Book:
                    ParseItemRewardInfo(info, () => RewardManager._Instance.AddReward(GameManager._Instance.GetRandomBook()));
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }

        foreach (RewardType type in currencyRewards.Keys())
        {
            int num = RandomHelper.RandomIntExclusive(currencyRewards[type].GetOption());
            switch (type)
            {
                case RewardType.ClothierCurrency:
                    RewardManager._Instance.AddClothierCurrencyReward(num);
                    break;
                case RewardType.Currency:
                    RewardManager._Instance.AddCurrencyReward(num);
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }

        yield return RewardManager._Instance.ShowRewardScreen();

        Debug.Log("2");
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");
        yield return GameManager._Instance.StartCoroutine(CombatManager._Instance.StartCombat(this));
    }
}
