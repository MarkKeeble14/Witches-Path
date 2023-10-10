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
    Gold,
    Pelts,
    Spell,
    PotionIngredient,
    Potion
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

        List<Spell> spellChoices = new List<Spell>();
        for (int i = 0; i < NumSpellsPerChoice; i++)
        {
            Spell spellChoice = GameManager._Instance.GetRandomSpellWithConditions(spell => viableRewardFunc(spell)
            && !Spell.SpellListContainSpell(spellChoices, spell.Label));
            spellChoices.Add(spellChoice);
            spawned.AddSpellChoice(spellChoice);
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
                GameManager._Instance.SwapBooks(book);
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
        PotionIngredient ingredient = PotionIngredient.GetPotionIngredientOfType(label);
        Debug.Log("Adding Potion Ingredient Reward: " + ingredient.GetToolTipLabel());
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        GameObject spawnedToolTip = null;
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

    public void AddReward(Potion potion)
    {
        Debug.Log("Adding Potion Reward: " + potion.GetToolTipLabel());
        RewardDisplay spawned = Instantiate(simpleRewardDisplay, rewardList);
        GameObject spawnedToolTip = null;
        spawned.Set(potion.GetToolTipLabel(), rewardTypeSpriteDict[RewardType.Book],
            delegate
            {
                GameManager._Instance.AddPotion(potion);
                spawnedRewards.Remove(spawned);
                Destroy(spawned.gameObject);
            }, delegate
            {
                spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(potion, spawned.transform);
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
        spawned.Set(goldAmount.ToString() + " Gold", rewardTypeSpriteDict[RewardType.Gold],
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

    public IEnumerator ShowRewardScreen(Action onEndAction = null)
    {
        AudioManager._Instance.PlayFromSFXDict("UI_RewardOpen");

        rewardScreen.gameObject.SetActive(true);

        yield return new WaitUntil(() => resolved);
        resolved = false;

        rewardScreen.SetActive(false);

        AudioManager._Instance.PlayFromSFXDict("UI_RewardClose");

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
