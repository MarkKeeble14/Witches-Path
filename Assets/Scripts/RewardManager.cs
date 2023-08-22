using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{
    Artifact,
    Book,
    Currency,
    Pelts,
    Spell,
    PotionIngredient
}

public enum RewardNumericalType
{
    ChanceTo,
    Between
}

public class RewardManager : MonoBehaviour
{
    public static RewardManager _Instance { get; private set; }
    private void Awake()
    {
        _Instance = this;
    }

    [SerializeField] private RewardDisplay simpleRewardDisplay;
    [SerializeField] private Transform rewardList;
    [SerializeField] private GameObject rewardScreen;
    private List<RewardDisplay> spawnedRewards = new List<RewardDisplay>();

    [SerializeField] private Vector2 toolTipOffset;

    [SerializeField] private SerializableDictionary<RewardType, Sprite> rewardTypeSpriteDict = new SerializableDictionary<RewardType, Sprite>();

    private bool resolved;

    public void Resolve()
    {
        resolved = true;
    }

    public void AddReward(SpellLabel label)
    {
        Debug.Log("Adding Spell Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);

        Spell spell = GameManager._Instance.GetSpellOfType(label);
        GameObject spawnedToolTip = null;
        spawned.Set(spell.Name, rewardTypeSpriteDict[RewardType.Spell],
            delegate
            {
                StartCoroutine(GameManager._Instance.StartSwapSpellSequnce(label));
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(spell, spawned.transform);
            }, delegate
            {
                Destroy(spawnedToolTip);
            }
        );
        spawnedRewards.Add(spawned);
    }

    public void AddReward(ArtifactLabel label)
    {
        Debug.Log("Adding Artifact Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        GameObject spawnedToolTip = null;
        Artifact artifact = GameManager._Instance.GetArtifactOfType(label);
        spawned.Set(artifact.Name, rewardTypeSpriteDict[RewardType.Artifact],
            delegate
            {
                GameManager._Instance.AddArtifact(label);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(artifact, spawned.transform);
            }, delegate
            {
                Destroy(spawnedToolTip);
            }
        );
        spawnedRewards.Add(spawned);
    }

    public void AddReward(BookLabel label)
    {
        Debug.Log("Adding Book Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        GameObject spawnedToolTip = null;
        Book book = GameManager._Instance.GetBookOfType(label);
        spawned.Set(book.Name, rewardTypeSpriteDict[RewardType.Book],
            delegate
            {
                GameManager._Instance.SwapBooks(GameManager._Instance.GetOwnedBookLabel(0), label);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(book, spawned.transform);
            }, delegate
            {
                Destroy(spawnedToolTip);
            }
        );
        spawnedRewards.Add(spawned);
    }

    public void AddReward(PotionIngredientType label)
    {
        Debug.Log("Adding Potion Ingredient Reward: " + label);
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        GameObject spawnedToolTip = null;
        PotionIngredient ingredient = GameManager._Instance.GetPotionIngredientOfType(label);
        spawned.Set(ingredient.Name, UIManager._Instance.GetPotionIngredientCategorySprite(ingredient.Category),
            delegate
            {
                GameManager._Instance.AddPotionIngredient(label);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(ingredient, spawned.transform);
            }, delegate
            {
                Destroy(spawnedToolTip);
            }
        );
        spawnedRewards.Add(spawned);
    }

    public void AddCurrencyReward(int currencyAmount)
    {
        if (currencyAmount <= 0)
        {
            return;
        }

        Debug.Log("Adding Currency Reward: " + currencyAmount);

        // Lucky Coin Effect
        if (GameManager._Instance.HasArtifact(ArtifactLabel.LuckyCoin))
        {
            currencyAmount = Mathf.CeilToInt(currencyAmount * (LuckyCoin.CurrencyMultiplier / 100));
            GameManager._Instance.AnimateArtifact(ArtifactLabel.LuckyCoin);
        }

        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(currencyAmount.ToString() + " Gold", rewardTypeSpriteDict[RewardType.Currency],
            delegate
            {
                GameManager._Instance.AlterCurrency(currencyAmount);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, null, null);
        spawnedRewards.Add(spawned);
    }

    public void AddClothierCurrencyReward(int currencyAmount)
    {
        if (currencyAmount <= 0)
        {
            return;
        }

        Debug.Log("Adding Clothier Currency Reward: " + currencyAmount);

        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(currencyAmount.ToString() + " Pelt" + (currencyAmount > 1 ? "s" : ""), rewardTypeSpriteDict[RewardType.Pelts],
            delegate
            {
                GameManager._Instance.AlterClothierCurrency(currencyAmount);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, null, null);

        spawnedRewards.Add(spawned);
    }

    public IEnumerator ShowRewardScreen()
    {
        rewardScreen.gameObject.SetActive(true);

        yield return new WaitUntil(() => resolved);
        resolved = false;

        ClearRewardsScreen();
        HideRewardScreen();
    }

    public void HideRewardScreen()
    {
        rewardScreen.SetActive(false);
    }

    public void ClearRewardsScreen()
    {
        while (spawnedRewards.Count > 0)
        {
            RewardDisplay reward = spawnedRewards[0];
            spawnedRewards.RemoveAt(0);
            Destroy(reward.gameObject);
        }
    }
}
