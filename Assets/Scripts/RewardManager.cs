using System;
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
    [SerializeField] private SpellChoiceRewardDisplay spellChoiceRewardDisplay;
    [SerializeField] private Transform rewardList;
    [SerializeField] private GameObject rewardScreen;
    private List<RewardDisplay> spawnedRewards = new List<RewardDisplay>();
    public int NumOutstandingRewards => spawnedRewards.Count;

    [SerializeField] private Vector2 toolTipOffset;

    [SerializeField] private SerializableDictionary<RewardType, Sprite> rewardTypeSpriteDict = new SerializableDictionary<RewardType, Sprite>();

    private bool resolved;

    [SerializeField] private GameObject selectSpellScreen;
    [SerializeField] private Transform parentSpellChoicesTo;
    [SerializeField] private VisualSpellDisplay visualSpellDisplayPrefab;

    public int NumSpellsPerChoice { get; private set; }
    [SerializeField] private int defaultNumSpellsPerChoice = 3;
    private int maxNumSpellsPerChoice = 5;
    private bool cancelSpellRewardDecision;

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

    public void CancelSpellRewardDecision()
    {
        cancelSpellRewardDecision = true;
    }

    public IEnumerator ShowSpellRewardDecision(SpellChoiceRewardDisplay spawningFor)
    {
        // Debug.Log("Showing Spell Reward Screen");
        selectSpellScreen.SetActive(true);

        List<VisualSpellDisplay> spawnedOptions = new List<VisualSpellDisplay>();

        bool selectedSpellReward = false;
        List<Spell> choices = spawningFor.GetSpellChoices();
        for (int i = 0; i < choices.Count; i++)
        {
            Spell spell = choices[i];

            VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, parentSpellChoicesTo);
            spawnedOptions.Add(spawned);

            // Set the Spell
            spawned.SetSpell(spell);

            // Add Events
            spawned.AddOnClick(delegate
            {
                GameManager._Instance.AddSpellToSpellBook(spell);
                selectedSpellReward = true;
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
        yield return new WaitUntil(() => cancelSpellRewardDecision || selectedSpellReward);
        cancelSpellRewardDecision = false;

        // Reset
        // Destroy Options
        while (spawnedOptions.Count > 0)
        {
            VisualSpellDisplay current = spawnedOptions[0];
            spawnedOptions.RemoveAt(0);
            Destroy(current.gameObject);
        }

        if (selectedSpellReward)
        {
            spawnedRewards.Remove(spawningFor);
            Destroy(spawningFor.gameObject);
        }

        // Disable Screen
        selectSpellScreen.SetActive(false);
    }

    public void AddChooseSpellReward(Func<Spell, bool> viableRewardFunc)
    {
        // Debug.Log("Adding Choose Spell Reward: ");
        SpellChoiceRewardDisplay spawned = Instantiate(spellChoiceRewardDisplay, rewardList);

        for (int i = 0; i < NumSpellsPerChoice; i++)
        {
            spawned.AddSpellChoice(GameManager._Instance.GetRandomSpellWithConditions(viableRewardFunc));
        }

        spawned.Set("Choose a Spell Tome", rewardTypeSpriteDict[RewardType.Spell],
            delegate
            {
                StartCoroutine(ShowSpellRewardDecision(spawned));
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
        Artifact artifact = Artifact.GetArtifactOfType(label);
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
        Book book = Book.GetBookOfType(label);
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
        PotionIngredient ingredient = PotionIngredient.GetPotionIngredientOfType(label);
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

    public void AddGoldReward(int goldAmount)
    {
        if (goldAmount <= 0)
        {
            return;
        }

        Debug.Log("Adding Currency Reward: " + goldAmount);

        // Lucky Coin Effect
        if (GameManager._Instance.HasArtifact(ArtifactLabel.LuckyCoin))
        {
            goldAmount = Mathf.CeilToInt(goldAmount * (LuckyCoin.CurrencyMultiplier / 100));
            GameManager._Instance.AnimateArtifact(ArtifactLabel.LuckyCoin);
        }

        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        spawned.Set(goldAmount.ToString() + " Gold", rewardTypeSpriteDict[RewardType.Currency],
            delegate
            {
                GameManager._Instance.AlterGold(goldAmount);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, null, null);
        spawnedRewards.Add(spawned);
    }

    public void AddPeltsReward(int currencyAmount)
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
                GameManager._Instance.AlterPelts(currencyAmount);
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

    public IEnumerator ShowRewardScreen(Action onEndAction = null)
    {
        rewardScreen.gameObject.SetActive(true);

        yield return new WaitUntil(() => resolved);
        resolved = false;

        rewardScreen.SetActive(false);

        // Call any Actions on End if Specified to
        onEndAction?.Invoke();
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

    public void AddRandomArtifactReward(int num = 1)
    {
        for (int i = 0; i < num; i++)
            AddReward(GameManager._Instance.GetRandomArtifact());
    }

    public void AddRandomPotionIngredientReward(int num = 1)
    {
        for (int i = 0; i < num; i++)
            AddReward(GameManager._Instance.GetRandomPotionIngredient());
    }

    public void AddRandomBookReward(int num = 1)
    {
        for (int i = 0; i < num; i++)
            AddReward(GameManager._Instance.GetRandomBook());
    }
}
