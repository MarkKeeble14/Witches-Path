using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseSpellRewardData
{
    public RewardDisplay Listing;
    public List<Spell> Choices = new List<Spell>();
}

public enum RewardType
{
    Artifact,
    Book,
    Currency,
    Pelts,
    Spell,
    PotionIngredient,
    ActiveSpellSlot,
    PassiveSpellSlot
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

        // Initialize Variable
        NumSpellsPerChoice = defaultNumSpellsPerChoice;
    }

    [SerializeField] private RewardDisplay simpleRewardDisplay;
    [SerializeField] private Transform rewardList;
    [SerializeField] private GameObject rewardScreen;
    private List<RewardDisplay> spawnedRewards = new List<RewardDisplay>();

    [SerializeField] private Vector2 toolTipOffset;

    [SerializeField] private SerializableDictionary<RewardType, Sprite> rewardTypeSpriteDict = new SerializableDictionary<RewardType, Sprite>();

    private bool resolved;

    [SerializeField] private GameObject selectSpellScreen;
    [SerializeField] private Transform parentSpellChoicesTo;
    [SerializeField] private VisualSpellDisplay visualSpellDisplayPrefab;

    private ChooseSpellRewardData activeChooseSpellReward;

    public int NumSpellsPerChoice { get; private set; }
    [SerializeField] private int defaultNumSpellsPerChoice = 3;
    private int maxNumSpellsPerChoice = 5;

    public void AlterNumSpellsPerChoice(int alterBy)
    {
        if (NumSpellsPerChoice + alterBy < 0)
        {
            NumSpellsPerChoice = 0;
        }
        else if (NumSpellsPerChoice + alterBy > maxNumSpellsPerChoice)
        {
            NumSpellsPerChoice = maxNumSpellsPerChoice;
        }
        else
        {
            NumSpellsPerChoice += alterBy;
        }
    }

    public void Resolve()
    {
        resolved = true;
    }

    private bool cancelSpellRewardDecision;

    public void CancelSpellRewardDecision()
    {
        cancelSpellRewardDecision = true;
    }

    public IEnumerator ShowSpellRewardDecision(int numOptions, bool forceRarity = false, Rarity rarity = Rarity.Common)
    {
        Debug.Log("Showing Spell Reward Screen");
        selectSpellScreen.SetActive(true);

        List<VisualSpellDisplay> spawnedOptions = new List<VisualSpellDisplay>();

        bool hasStartedSpellSwapSequence = false;
        if (activeChooseSpellReward.Choices.Count == 0)
        {
            for (int i = 0; i < numOptions; i++)
            {
                // Get new Options
                Spell spell = GameManager._Instance.GetSpellOfType(GameManager._Instance.GetRandomSpell(forceRarity, rarity));

                // Add Spell to Class to Track
                activeChooseSpellReward.Choices.Add(spell);
            }
        }

        for (int i = 0; i < activeChooseSpellReward.Choices.Count; i++)
        {
            Spell spell = activeChooseSpellReward.Choices[i];

            VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, parentSpellChoicesTo);
            spawnedOptions.Add(spawned);

            // Set the Spell
            spawned.SetSpell(spell);

            // Add Events
            spawned.AddOnClick(delegate
            {
                StartCoroutine(GameManager._Instance.StartSwapSpellSequnce(spell.Label));
                hasStartedSpellSwapSequence = true;
            });
            GameObject spawnedToolTip = null;
            spawned.AddOnEnter(delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(spell, spawned.transform);
            });
            spawned.AddOnExit(delegate
            {
                Destroy(spawnedToolTip);
            });
        }

        // Wait until we either cancel the decision, or we HAD started the sequence and are no longer in the sequence
        yield return new WaitUntil(() => cancelSpellRewardDecision || (hasStartedSpellSwapSequence && !GameManager._Instance.InSwappingSpellSequence));

        // Reset
        // Destroy Options
        while (spawnedOptions.Count > 0)
        {
            VisualSpellDisplay current = spawnedOptions[0];
            spawnedOptions.RemoveAt(0);
            Destroy(current.gameObject);
        }

        // if we have swapped a spell
        if (hasStartedSpellSwapSequence && GameManager._Instance.GetDidSwapSpell())
        {
            spawnedRewards.Remove(activeChooseSpellReward.Listing);
            if (activeChooseSpellReward.Listing != null)
                Destroy(activeChooseSpellReward.Listing.gameObject);
            activeChooseSpellReward = null;

            // Reset this Checker
            GameManager._Instance.ResetDidSwapSpell();

            // Disable Screen
            selectSpellScreen.SetActive(false);
        }
        else // We haven't swapped a spell
        {
            if (cancelSpellRewardDecision)
            {
                cancelSpellRewardDecision = false;

                // Disable Screen
                selectSpellScreen.SetActive(false);
            }
            else
            {
                cancelSpellRewardDecision = false;
                yield return StartCoroutine(ShowSpellRewardDecision(numOptions, forceRarity, rarity));
            }

        }
    }

    public void AddChooseSpellReward(bool forceRarity = false, Rarity rarity = Rarity.Common)
    {
        Debug.Log("Adding Choose Spell Reward: ");
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set("Choose a Spell to Learn", rewardTypeSpriteDict[RewardType.Spell],
            delegate
            {
                if (activeChooseSpellReward == null)
                {
                    activeChooseSpellReward = new ChooseSpellRewardData();
                    activeChooseSpellReward.Listing = spawned;
                }
                StartCoroutine(ShowSpellRewardDecision(NumSpellsPerChoice, forceRarity, rarity));
            }, delegate
            {
                //
            }, delegate
            {
                //
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


    public void AddActiveSpellSlotReward(int numSlots)
    {
        Debug.Log("Adding Active Spell Slot Reward: " + numSlots);

        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(numSlots.ToString() + " Active Spell Slot" + (numSlots > 1 ? "s" : ""), rewardTypeSpriteDict[RewardType.ActiveSpellSlot],
            delegate
            {
                GameManager._Instance.AddActiveSpellSlots(numSlots);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, null, null);

        spawnedRewards.Add(spawned);
    }

    public void AddPassiveSpellSlotReward(int numSlots)
    {
        Debug.Log("Adding Passive Spell Slot Reward: " + numSlots);

        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(numSlots.ToString() + " Passive Spell Slot" + (numSlots > 1 ? "s" : ""), rewardTypeSpriteDict[RewardType.PassiveSpellSlot],
            delegate
            {
                GameManager._Instance.AddPassiveSpellSlots(numSlots);
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
