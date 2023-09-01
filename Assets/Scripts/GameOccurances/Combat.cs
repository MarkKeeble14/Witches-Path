using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemRewardInfo
{
    public RewardNumericalType NumericalType;
    public bool ForceRarity;
    public Rarity Rarity;
    public Vector2 Numbers;
}

public abstract class Combat : GameOccurance
{
    [SerializeField] private AudioClip mainMusic; // Music file, attach from editor
    public AudioClip MainMusic { get => mainMusic; }

    [SerializeField] private EnemyType enemyType;
    private Enemy spawnedEnemy;
    public Enemy SpawnedEnemy
    {
        get
        {
            // Spawn enemy if needed
            if (spawnedEnemy == null)
            {
                spawnedEnemy = Enemy.GetEnemyOfType(enemyType);
            }
            return spawnedEnemy;
        }
    }

    [SerializeField]
    private SerializableDictionary<RewardType, PercentageMap<ItemRewardInfo>> itemRewards
        = new SerializableDictionary<RewardType, PercentageMap<ItemRewardInfo>>();

    [SerializeField]
    private SerializableDictionary<RewardType, PercentageMap<Vector2Int>> currencyRewards
    = new SerializableDictionary<RewardType, PercentageMap<Vector2Int>>();

    private void ParseItemRewardInfo(ItemRewardInfo info, Action act)
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

        // Item rewards
        foreach (RewardType type in itemRewards.Keys())
        {
            ItemRewardInfo info = itemRewards[type].GetOption();
            switch (type)
            {
                case RewardType.Artifact:
                    ParseItemRewardInfo(info, delegate
                    {
                        if (info.ForceRarity)
                        {
                            RewardManager._Instance.AddReward(GameManager._Instance.GetRandomArtifactOfRarity(info.Rarity));
                        }
                        else
                        {
                            RewardManager._Instance.AddReward(GameManager._Instance.GetRandomArtifact());
                        }
                    });
                    break;
                case RewardType.Book:
                    ParseItemRewardInfo(info, delegate
                    {
                        if (info.ForceRarity)
                        {
                            RewardManager._Instance.AddReward(GameManager._Instance.GetRandomBookOfRarity(info.Rarity));
                        }
                        else
                        {
                            RewardManager._Instance.AddReward(GameManager._Instance.GetRandomBook());
                        }
                    });
                    break;
                case RewardType.Spell:
                    ParseItemRewardInfo(info, delegate
                    {
                        if (info.ForceRarity)
                        {
                            RewardManager._Instance.AddChooseSpellReward(spell =>
                                GameManager._Instance.AcceptSpellRewardFunc(spell) && spell.Rarity == info.Rarity);
                        }
                        else
                        {
                            RewardManager._Instance.AddChooseSpellReward(GameManager._Instance.AcceptSpellRewardFunc);
                        }
                    });
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }

        // Currency Rewards
        foreach (RewardType type in currencyRewards.Keys())
        {
            int num = RandomHelper.RandomIntExclusive(currencyRewards[type].GetOption());

            // if the number is 0, there is no point in showing
            if (num <= 0)
            {
                continue;
            }

            switch (type)
            {
                case RewardType.Pelts:
                    RewardManager._Instance.AddPeltsReward(num);
                    break;
                case RewardType.Currency:
                    RewardManager._Instance.AddGoldReward(num);
                    break;
                case RewardType.ActiveSpellSlot:
                    RewardManager._Instance.AddActiveSpellSlotReward(num);
                    break;
                case RewardType.PassiveSpellSlot:
                    RewardManager._Instance.AddPassiveSpellSlotReward(num);
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }

        yield return RewardManager._Instance.ShowRewardScreen();
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");
        yield return GameManager._Instance.StartCoroutine(CombatManager._Instance.StartCombat(this));
    }
}
