using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Febucci.UI;
using DG.Tweening;

public enum ContentType
{
    Artifact,
    Book,
    Spell,
}

public class GameManager : MonoBehaviour
{
    public static GameManager _Instance { get; private set; }
    private MapNodeUI currentNode;

    private GameOccurance currentOccurance;

    [Header("Character")]
    [SerializeField] private Character playerCharacter;
    private int maxPlayerHP;
    private int currentPlayerHP;
    private int maxPlayerMana;
    private int characterMaxMana;
    private int currentPlayerMana;
    private int currentPlayerCurrency;
    private int currentPlayerClothierCurrency;
    private Robe playerEquippedRobe;
    private Hat playerEquippedHat;
    private Wand playerEquippedWand;

    private string persistentTokensKey = "PersistentTokens";

    [Header("Artifacts")]
    [SerializeField] private Transform artifactBar;
    [SerializeField] private PercentageMap<Rarity> artifactRarityOdds = new PercentageMap<Rarity>();
    private Dictionary<Artifact, ArtifactDisplay> equippedArtifacts = new Dictionary<Artifact, ArtifactDisplay>();
    private List<ArtifactLabel> awardableArtifacts = new List<ArtifactLabel>();
    [SerializeField] private List<ArtifactLabel> unawardableArtifacts = new List<ArtifactLabel>();
    public int NumArtifacts => equippedArtifacts.Count;

    [Header("Books")]
    [SerializeField] private Transform bookBar;
    [SerializeField] private PercentageMap<Rarity> bookRarityOdds = new PercentageMap<Rarity>();
    private Dictionary<BookLabel, BookDisplay> bookDisplayTracker = new Dictionary<BookLabel, BookDisplay>();
    private Dictionary<BookLabel, Book> equippedBooks = new Dictionary<BookLabel, Book>();
    private List<BookLabel> awardableBooks = new List<BookLabel>();
    [SerializeField] private List<BookLabel> unawardableBooks = new List<BookLabel>();

    [Header("Spells")]
    [SerializeField] private List<SpellLabel> unviableSpellRewards;
    private List<SpellLabel> viableSpellRewards = new List<SpellLabel>();
    private int equippableSpellIndex;
    private Spellbook spellBook;
    public int NumSpells => spellBook.GetSpellBookEntries().Count;

    [Header("Select Spells For Combat Screen")]
    [SerializeField] private VisualSpellDisplay visualSpellDisplayPrefab;
    [SerializeField] private GameObject selectSpellsForCombatScreen;
    [SerializeField] private Transform parentSelectSpellOptionsTo;
    [SerializeField] private Button confirmSelectSpellsForCombatButton;
    [SerializeField] private Image confirmSelectSpellsForCombatImage;
    [SerializeField] private TextMeshProUGUI confirmSelectSpellsForCombatText;
    [SerializeField] private TextMeshProUGUI selectSpellsForCombatTitleText;
    [SerializeField] private TypewriterByWord selectSpellsForCombatScreenTitleTypewriter;
    private bool spellSelectionForCombatConfirmed;
    [SerializeField] private string allowedToFightTitleText = "Enter Combat";
    [SerializeField] private string unallowedToFightTitleText = "Select Your Spells";

    [Header("Spell Book Screen")]
    [SerializeField] private GameObject spellBookScreen;
    [SerializeField] private Transform spellBookSpawnSpellDisplaysOn;
    private List<VisualSpellDisplay> spellBookSpawnedSpellDisplays = new List<VisualSpellDisplay>();

    [SerializeField] private GameObject spellPileScreen;

    [Header("Select Spell Screen")]
    [SerializeField] private GameObject selectSpellScreen;
    [SerializeField] private Transform spawnSelectSpellDisplaysOn;

    [Header("Potions")]
    [SerializeField] private PercentageMap<PotionIngredientCategory> potionIngredientCatagoryOdds = new PercentageMap<PotionIngredientCategory>();
    [SerializeField]
    private SerializableDictionary<PotionIngredientCategory, PercentageMap<PotionIngredientType>> potionIngredientRandomOddsOfTypeMap =
        new SerializableDictionary<PotionIngredientCategory, PercentageMap<PotionIngredientType>>();
    private Dictionary<PotionIngredientType, int> potionIngredientMap = new Dictionary<PotionIngredientType, int>();
    private Potion currentPotion;
    private List<Potion> availablePotions = new List<Potion>();
    public int NumPotions => availablePotions.Count;

    [Header("Equipment")]
    [SerializeField] private List<Hat> equippableHats = new List<Hat>();
    [SerializeField] private List<Robe> equippableRobes = new List<Robe>();
    [SerializeField] private List<Wand> equippableWands = new List<Wand>();

    [Header("Prefabs")]
    [SerializeField] private ArtifactDisplay artifactDisplay;
    [SerializeField] private BookDisplay bookDisplay;
    [SerializeField] private PopupText popupTextPrefab;

    [Header("Test")]
    [SerializeField] private ContentType currentlyTesting;
    [SerializeField] private int numRandomIngredientsOnStart;
    [SerializeField] private int numRandomCursesOnStart;
    private List<BookLabel> allBook;
    private List<ArtifactLabel> allArtifact;
    private int artifactIndex;
    private int bookIndex;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI clothierCurrencyText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI spellbookSizeText;
    [SerializeField] private OnTextChangedEventCaller manaTextEventCaller;

    [Header("Delays")]
    [SerializeField] private float delayOnReachNode = 0.5f;

    Pile<Spell> prevCombatDrawPile = new Pile<Spell>();
    private int combatPileSize = 10;

    // Callbacks
    public Action OnEnterNewRoom;
    public Action OnPlayerRecieveDamage;
    private Dictionary<MapNodeType, Action> OnEnterSpecificRoomActionMap = new Dictionary<MapNodeType, Action>();

    public bool OverlaidUIOpen => potionIngredientListScreen.activeInHierarchy || spellBookScreen.activeInHierarchy || spellPileScreen.activeInHierarchy;

    public int DamageFromEquipment { get; set; }
    public int DefenseFromEquipment { get; set; }
    private int manaFromEquipment;

    public bool GameOvered => GetCurrentCharacterHP() <= 0;
    public bool CanSetCurrentGameOccurance { get; set; }

    // Spell Reward Offers
    public Func<Spell, bool> AcceptSpellRewardFunc => spell => !spellRewardsMustMatchCharacterColor || (spellRewardsMustMatchCharacterColor && spell.Color == GetCharacterColor());
    private bool spellRewardsMustMatchCharacterColor = true;

    private List<PotionIngredientListEntry> spawnedPotionIngredientListEntries = new List<PotionIngredientListEntry>();

    [SerializeField] private CanvasGroup upgradeBookButtonCV;

    [SerializeField] private GameObject showPileScreen;

    internal void AlterCombatPileSize(int changeBy)
    {
        if (combatPileSize + changeBy < 1)
        {
            combatPileSize = 1;
        }
        else
        {
            combatPileSize += changeBy;
        }
    }

    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        CallOnGameStart();

        // Test
        for (int i = 0; i < numRandomIngredientsOnStart; i++)
        {
            AddRandomPotionIngredient();
        }
        // Test
        for (int i = 0; i < numRandomCursesOnStart; i++)
        {
            AddSpellToSpellBook(GetRandomSpellWithConditions(spell => spell.Color == SpellColor.Curse));
        }
    }

    public void CallOnGameStart()
    {
        // Get List of all Books & Artifacts for Testing
        allBook = new List<BookLabel>((BookLabel[])Enum.GetValues(typeof(BookLabel)));
        allArtifact = new List<ArtifactLabel>((ArtifactLabel[])Enum.GetValues(typeof(ArtifactLabel)));

        // Get List of all Book Labels
        foreach (BookLabel label in Enum.GetValues(typeof(BookLabel)))
        {
            if (!unawardableBooks.Contains(label))
            {
                awardableBooks.Add(label);
            }
        }

        // Get List of all Artifact Labels
        foreach (ArtifactLabel label in Enum.GetValues(typeof(ArtifactLabel)))
        {
            if (!unawardableArtifacts.Contains(label))
            {
                awardableArtifacts.Add(label);
            }
        }

        // Setup Potion Base Map
        List<PotionIngredient> potionBaseIngredients = PotionIngredient.GetPotionIngredientsMatchingFunc(ingredient => ingredient.Category == PotionIngredientCategory.Base);
        int equalOdds = Mathf.FloorToInt(100 / potionBaseIngredients.Count);
        foreach (PotionIngredient ingredient in potionBaseIngredients)
        {
            potionIngredientRandomOddsOfTypeMap[PotionIngredientCategory.Base].AddOption(new SerializableKeyValuePair<PotionIngredientType, int>(ingredient.Type, equalOdds));
        }

        TryAddPersistentTokens();

        // Setup On Specific Room Action Map
        foreach (MapNodeType type in Enum.GetValues(typeof(MapNodeType)))
        {
            OnEnterSpecificRoomActionMap.Add(type, null);
        }

        // Setup Viable Spell Rewards
        foreach (SpellLabel label in Enum.GetValues(typeof(SpellLabel)))
        {
            if (!unviableSpellRewards.Contains(label))
                viableSpellRewards.Add(label);
        }


        // Set equipment Tool Tippables
        SetEquipmentToolTippables(equippableHats.Cast<Equipment>().ToList());
        SetEquipmentToolTippables(equippableRobes.Cast<Equipment>().ToList());
        SetEquipmentToolTippables(equippableWands.Cast<Equipment>().ToList());

        SetNewPotion();

        EquipCharacterLoadout(playerCharacter);

        InitializeDeck();

        CanSetCurrentGameOccurance = true;
    }

    private void SetEquipmentToolTippables(List<Equipment> equipment)
    {
        foreach (Equipment e in equipment)
        {
            e.PrepEquipment();
        }
    }

    private void Update()
    {
        // Disable the upgrade book button if book cannot be upgraded
        if (CanUpgradeActiveBook)
        {
            upgradeBookButtonCV.alpha = 1;
            upgradeBookButtonCV.interactable = true;
        }
        else
        {
            upgradeBookButtonCV.alpha = 0.5f;
            upgradeBookButtonCV.interactable = false;
        }

        // Set Info Texts
        hpText.text = Mathf.RoundToInt(currentPlayerHP).ToString() + "/" + Mathf.RoundToInt(maxPlayerHP).ToString();
        manaText.text = Mathf.RoundToInt(currentPlayerMana).ToString() + "/" + Mathf.RoundToInt(maxPlayerMana).ToString();
        currencyText.text = Mathf.RoundToInt(currentPlayerCurrency).ToString();
        clothierCurrencyText.text = Mathf.RoundToInt(currentPlayerClothierCurrency).ToString();

        spellbookSizeText.text = spellBook.GetNumSpellsMatchingCondition(spell => true).ToString();

        // Set Timer Text
        int hours = TimeSpan.FromSeconds(Time.realtimeSinceStartup).Hours;
        int minutes = TimeSpan.FromSeconds(Time.realtimeSinceStartup).Minutes;
        int seconds = TimeSpan.FromSeconds(Time.realtimeSinceStartup).Seconds;
        if (hours > 0)
        {
            timerText.text = string.Format("{0:0}:{0:0}:{1:00}", hours, minutes, seconds);
        }
        else
        {
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }

        // Guard Against Negatives
        // The players basic attacks will deal their basic attack damage + DamageFromEquipment + Bonus Damage from their Power Affliction (if applicable)
        int damageAmount = GetBasicAttackDamage() + DamageFromEquipment + CombatManager._Instance.GetPowerBonus(Combatent.Character);
        // if the amount of damage a players basic attack will do is less than zero, consider it zero. This is accounted for in the CombatManager script manually as well
        if (damageAmount >= 0)
        {
            damageText.text = damageAmount.ToString();
        }
        else
        {
            damageText.text = 0.ToString();
        }

        // Guard Against Negatives
        // if Defense from equipment is less than zero, we'll consider it zero. This is accounted for in the CombatManager script manually as well
        if (DefenseFromEquipment >= 0)
        {
            defenseText.text = Mathf.RoundToInt(DefenseFromEquipment).ToString();
        }
        else
        {
            defenseText.text = 0.ToString();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int v = (int)currentlyTesting;
            if (v + 1 >= Enum.GetNames(typeof(ContentType)).Length)
            {
                v = 0;
            }
            else
            {
                v += 1;
            }
            currentlyTesting = (ContentType)v;
            Debug.Log("Now Testing: " + currentlyTesting);
        }

        // Testing
        switch (currentlyTesting)
        {
            case ContentType.Spell:

                // Active Spells
                // Equip new spell
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    equippableSpellIndex++;

                    if (equippableSpellIndex > viableSpellRewards.Count - 1)
                        equippableSpellIndex = 0;

                    Debug.Log("Selected: " + viableSpellRewards[equippableSpellIndex]);
                }

                // Equip new spell
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    equippableSpellIndex--;

                    if (equippableSpellIndex < 0)
                        equippableSpellIndex = viableSpellRewards.Count - 1;

                    Debug.Log("Selected: " + viableSpellRewards[equippableSpellIndex]);
                }

                if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    // 
                    AddSpellToSpellBook(viableSpellRewards[equippableSpellIndex]);
                }
                if (Input.GetKeyDown(KeyCode.KeypadPeriod))
                {
                    //
                }

                break;
            case ContentType.Artifact:

                // Artifacts
                // Equip new Artifact
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    artifactIndex++;

                    if (artifactIndex > allArtifact.Count - 1)
                        artifactIndex = 0;

                    Debug.Log("Selected: " + allArtifact[artifactIndex]);
                }

                // Equip new artifact
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    artifactIndex--;

                    if (artifactIndex < 0)
                        artifactIndex = allArtifact.Count - 1;

                    Debug.Log("Selected: " + allArtifact[artifactIndex]);
                }

                if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    AddArtifact(allArtifact[artifactIndex]);
                }

                if (Input.GetKeyDown(KeyCode.KeypadPeriod))
                {
                    // RemoveArtifact(allArtifact[artifactIndex]);
                }

                break;
            case ContentType.Book:

                // Books
                // Equip new Book
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    bookIndex++;

                    if (bookIndex > allBook.Count - 1)
                        bookIndex = 0;

                    Debug.Log("Selected: " + allBook[bookIndex]);
                }

                // Equip new book
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    bookIndex--;

                    if (bookIndex < 0)
                        bookIndex = allBook.Count - 1;

                    Debug.Log("Selected: " + allBook[bookIndex]);
                }

                if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SwapBooks(bookDisplayTracker.Keys.First(), allBook[bookIndex]);
                }

                if (Input.GetKeyDown(KeyCode.KeypadPeriod))
                {
                    RemoveBook(allBook[bookIndex]);
                }

                break;
        }
    }

    public int GetBasicAttackDamage()
    {
        return playerCharacter.GetBasicAttackDamage();
    }

    #region Artifacts
    public Artifact GetRandomOwnedArtifact()
    {
        return RandomHelper.GetRandomFromList(equippedArtifacts.Keys.ToList());
    }

    public ArtifactLabel GetRandomArtifact(bool removeFromPool = true)
    {
        ArtifactLabel artifact = GetRandomArtifactOfRarity(artifactRarityOdds.GetOption());
        if (removeFromPool)
        {
            awardableArtifacts.Remove(artifact);
        }
        return artifact;
    }

    public ArtifactLabel GetRandomArtifactOfRarity(Rarity r)
    {
        List<ArtifactLabel> options = new List<ArtifactLabel>();
        foreach (ArtifactLabel l in awardableArtifacts)
        {
            if (Artifact.GetArtifactOfType(l).Rarity == r)
            {
                options.Add(l);
            }
        }

        // No Artifacts Remaining
        if (options.Count == 0)
        {
            // Attempt to instead return an Artifact of the rarity underneath the asked for
            switch (r)
            {
                case Rarity.Basic:
                    return ArtifactLabel.Crown;
                case Rarity.Common:
                    return GetRandomArtifactOfRarity(Rarity.Basic);
                case Rarity.Uncommon:
                    return GetRandomArtifactOfRarity(Rarity.Common);
                case Rarity.Rare:
                    return GetRandomArtifactOfRarity(Rarity.Uncommon);
                case Rarity.Boss:
                    return GetRandomArtifactOfRarity(Rarity.Rare);
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }


        return RandomHelper.GetRandomFromList(options);
    }

    public void AddRandomArtifact()
    {
        AddArtifact(GetRandomArtifact());
    }
    #endregion

    #region Book
    public void AlterAllBookCharge(int alterBy)
    {
        // Alter Book Charge by
        foreach (KeyValuePair<BookLabel, Book> kvp in equippedBooks)
        {
            kvp.Value.AlterCharge(alterBy);
        }
    }

    [ContextMenu("Fill All Book Charges")]
    public void FillAllBookCharges()
    {
        // Alter Book Charge by
        foreach (KeyValuePair<BookLabel, Book> kvp in equippedBooks)
        {
            kvp.Value.AlterCharge(kvp.Value.MaxCharge);
        }
    }

    public void AlterBookCharge(BookLabel label, int alterBy)
    {
        equippedBooks[label].AlterCharge(alterBy);
    }

    public BookLabel GetRandomOwnedBook()
    {
        return RandomHelper.GetRandomFromList(equippedBooks.Keys.ToList());
    }

    public BookLabel GetOwnedBookLabel(int index)
    {
        return equippedBooks.Keys.ToList()[index];
    }

    public Book GetOwnedBook(int index)
    {
        return equippedBooks.Values.ToList()[index];
    }

    public BookLabel GetRandomBook(bool removeFromPool = true)
    {
        BookLabel book = GetRandomBookOfRarity(bookRarityOdds.GetOption());
        if (removeFromPool)
        {
            awardableBooks.Remove(book);
        }
        return book;
    }

    public BookLabel GetRandomBookOfRarity(Rarity r)
    {
        List<BookLabel> options = new List<BookLabel>();
        foreach (BookLabel l in awardableBooks)
        {
            if (Book.GetBookOfType(l).Rarity == r)
            {
                options.Add(l);
            }
        }

        // No Books Remaining
        // No Artifacts Remaining
        if (options.Count == 0)
        {
            switch (r)
            {
                case Rarity.Basic:
                    return BookLabel.WitchesTravelGuide;
                case Rarity.Common:
                    return GetRandomBookOfRarity(Rarity.Basic);
                case Rarity.Uncommon:
                    return GetRandomBookOfRarity(Rarity.Common);
                case Rarity.Rare:
                    return GetRandomBookOfRarity(Rarity.Uncommon);
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }

        return RandomHelper.GetRandomFromList(options);
    }

    public void AddRandomBook()
    {
        AddBook(GetRandomBook());
    }

    #endregion

    #region Spells

    public Spell GetRandomOwnedSpell()
    {
        return RandomHelper.GetRandomFromList(spellBook.GetSpellBookEntries());
    }

    public Spell GetRandomSpell(Func<Spell, bool> includeConditions, bool removeFromPool = false)
    {
        return GetRandomSpellWithConditions(spell => includeConditions(spell), removeFromPool);
    }

    public SpellColor GetCharacterColor()
    {
        return playerCharacter.GetColor();
    }

    public Spell GetRandomSpellWithConditions(Func<Spell, bool> includeConditions, bool removeFromPool = false)
    {
        List<SpellLabel> options = new List<SpellLabel>();
        foreach (SpellLabel label in viableSpellRewards)
        {
            Spell spell = Spell.GetSpellOfType(label);
            if (includeConditions(spell))
            {
                // Debug.Log(spell + ", Passed Conditions");
                options.Add(label);
            }
        }

        // Default
        if (options.Count <= 0)
        {
            return Spell.GetSpellOfType(playerCharacter.GetDefaultSpell());
        }

        SpellLabel chosenSpellLabel = RandomHelper.GetRandomFromList(options);
        if (removeFromPool)
        {
            viableSpellRewards.Remove(chosenSpellLabel);
        }
        return Spell.GetSpellOfType(chosenSpellLabel);
    }

    [Header("Before Combat Information")]
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI enemyAdditionalInfoText;
    [SerializeField] private EnemyInfoBlock onCombatStart;
    [SerializeField] private EnemyInfoBlock onTurnStart;
    [SerializeField] private EnemyInfoBlock onTurnEnd;
    [SerializeField] private EnemyInfoBlock combatActions;

    [System.Serializable]
    private struct EnemyInfoBlock
    {
        public GameObject Container;
        public Transform List;
    }
    [SerializeField] private EnemyActionInfoBlockDisplay enemyIntentDisplayPrefab;
    private List<EnemyActionInfoBlockDisplay> spawnedInfoBlockIntentDisplays = new List<EnemyActionInfoBlockDisplay>();

    private void SetEnemyInformationText(EnemyInfoBlock infoBlock, List<EnemyAction> enemyActions)
    {
        if (enemyActions.Count <= 0)
        {
            infoBlock.Container.SetActive(false);
            return;
        }
        infoBlock.Container.SetActive(true);

        List<Spell> spellsToDisplay = new List<Spell>();
        List<SpellLabel> addedSpells = new List<SpellLabel>();
        foreach (EnemyAction action in enemyActions)
        {
            List<Spell> actionSpells = action.GetActionSpells();

            foreach (Spell spell in actionSpells)
            {
                if (!addedSpells.Contains(spell.Label))
                {
                    addedSpells.Add(spell.Label);
                    spellsToDisplay.Add(spell);
                }
            }
        }

        foreach (Spell spell in spellsToDisplay)
        {
            EnemyActionInfoBlockDisplay spawned = Instantiate(enemyIntentDisplayPrefab, infoBlock.List);
            spawned.Set(spell);
            spawnedInfoBlockIntentDisplays.Add(spawned);
        }
    }

    private void ClearEnemyInfoBlockIntentDisplays()
    {
        while (spawnedInfoBlockIntentDisplays.Count > 0)
        {
            EnemyActionInfoBlockDisplay current = spawnedInfoBlockIntentDisplays[0];
            spawnedInfoBlockIntentDisplays.RemoveAt(0);
            Destroy(current.gameObject);
        }
    }


    private void InitializeDeck()
    {
        Pile<Spell> starterDeck = new Pile<Spell>();

        foreach (Spell spell in spellBook.GetSpellBookEntries())
        {
            starterDeck.Add(spell);
        }

        CombatManager._Instance.SetSpellPiles(starterDeck);
        prevCombatDrawPile = starterDeck;
    }

    public IEnumerator SelectSpellsForCombat(Enemy combatEnemy)
    {
        // Set the screen to be visible
        selectSpellsForCombatScreen.SetActive(true);

        // Start Typewriter effect
        selectSpellsForCombatScreenTitleTypewriter.StartShowingText(true);

        // Set things according to the enemy so the player knows what they're fighting
        enemyNameText.text = combatEnemy.Name;
        if (combatEnemy.AdditionalInfoText.Length > 0)
        {
            enemyAdditionalInfoText.gameObject.SetActive(true);
            enemyAdditionalInfoText.text = combatEnemy.AdditionalInfoText;
        }
        else
        {
            enemyAdditionalInfoText.gameObject.SetActive(false);
        }
        SetEnemyInformationText(onCombatStart, combatEnemy.GetOnCombatStartActions());
        SetEnemyInformationText(onTurnStart, combatEnemy.GetOnTurnStartActions());
        SetEnemyInformationText(onTurnEnd, combatEnemy.GetOnTurnEndActions());
        SetEnemyInformationText(combatActions, combatEnemy.GetEnemyActions());

        // Get Spells
        List<VisualSpellDisplay> spawnedSelections = new List<VisualSpellDisplay>();
        int numAvailableSpells = spellBook.GetNumSpellsMatchingCondition(spell => true);

        Pile<Spell> newCombatDrawPile = new Pile<Spell>();

        foreach (Spell spell in spellBook.GetSpellBookEntries())
        {
            VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, parentSelectSpellOptionsTo);
            spawnedSelections.Add(spawned);
            spawned.SetSpell(spell);

            // if the player has less spells available than the pile size, then we just select all for them but do not give them the option to deselect 
            if (numAvailableSpells <= combatPileSize)
            {
                newCombatDrawPile.Add(spell);
                spawned.SetSpellDisplayState(SpellDisplayState.Locked);
                spawned.GetCanvasGroup().blocksRaycasts = false;
                continue;
            }
            else if (prevCombatDrawPile.Contains(spell)) // Automatically Load previously selected Spells BUT ALSO we'll be allowing them to be de-selected
            {
                // Automatically Select
                newCombatDrawPile.Add(spell);
                spawned.SetSpellDisplayState(SpellDisplayState.Selected);

            }
            else if (spell.Color == SpellColor.Curse || spell.Color == SpellColor.Status)
            {
                // Automatically Select
                // Curses and Statuses are Automatically Loaded and Locked
                newCombatDrawPile.Add(spell);
                spawned.SetSpellDisplayState(SpellDisplayState.Locked);
                spawned.GetCanvasGroup().blocksRaycasts = false;
                continue;
            }

            // Add Callbacks to all Spells that have not been already handled
            spawned.AddOnClick(delegate
            {
                if (newCombatDrawPile.Contains(spell))
                {
                    newCombatDrawPile.Remove(spell);
                    spawned.SetSpellDisplayState(SpellDisplayState.Normal);
                }
                else
                {
                    newCombatDrawPile.Add(spell);
                    spawned.SetSpellDisplayState(SpellDisplayState.Selected);
                }
            });
        }

        // Spawn Choices
        // To proceed, the player must click the confirm button
        // The player must equip at least one spell (provided they have any)
        bool prevAllowed = false;
        bool allowed = false;
        confirmSelectSpellsForCombatButton.interactable = false;
        List<Tween> confirmButtonTweens = new List<Tween>();
        selectSpellsForCombatTitleText.text = unallowedToFightTitleText;
        while (!spellSelectionForCombatConfirmed)
        {
            // Set button text and interactable state
            // if the player has absolutely no available Spells, just allow them to proceed
            confirmSelectSpellsForCombatText.text = newCombatDrawPile.Count + " / " + combatPileSize;

            if (numAvailableSpells == 0)
            {
                allowed = true;
            }
            else if (numAvailableSpells < combatPileSize)
            {
                // if the player has less spells than is maximally allowed, they must equip then all
                allowed = newCombatDrawPile.Count == numAvailableSpells;
            }
            else
            {
                // Otherwise, they must choose which to take and which to leave
                allowed = newCombatDrawPile.Count == combatPileSize;
            }

            // Essentially Checks if a Change in Allowed State has Occurred & will only set the text then
            // Hopefully stops the Typewriter from never displaying text
            if (prevAllowed != allowed)
            {
                confirmSelectSpellsForCombatButton.interactable = allowed;

                // if going from unallowed to allowed, start tweens
                if (prevAllowed == false)
                {
                    // Change color
                    confirmButtonTweens.Add(confirmSelectSpellsForCombatImage.DOColor(Color.red, 1));
                    // Make shake
                    confirmButtonTweens.Add(confirmSelectSpellsForCombatButton.transform.DOShakePosition(1, 3, 10, 90, false, true).SetLoops(-1));
                }
                else // going from allowed to unallowed, kill tweens
                {
                    // Kill Tweens
                    while (confirmButtonTweens.Count > 0)
                    {
                        Tween t = confirmButtonTweens[0];
                        confirmButtonTweens.RemoveAt(0);
                        t.Kill();
                    }
                    // And reset color
                    confirmSelectSpellsForCombatImage.DOColor(Color.white, 1);
                }

                selectSpellsForCombatTitleText.text = allowed ? allowedToFightTitleText : unallowedToFightTitleText;
            }
            prevAllowed = allowed;

            yield return null;
        }

        // Ensure any remaining Tweens have been Killed
        while (confirmButtonTweens.Count > 0)
        {
            Tween t = confirmButtonTweens[0];
            confirmButtonTweens.RemoveAt(0);
            t.Kill();
        }
        // and reset color
        confirmSelectSpellsForCombatImage.DOColor(Color.white, 1);

        // Player has made selection
        spellSelectionForCombatConfirmed = false;

        // Set each spell to be on Out of Combat Cooldown
        // For now, testing not doing this
        // combatDrawPile.ActOnEachSpellInPile(spell => spellBook.SetSpellUnavailable(spell));

        // Destroy spawned objects
        while (spawnedSelections.Count > 0)
        {
            VisualSpellDisplay current = spawnedSelections[0];
            spawnedSelections.RemoveAt(0);
            Destroy(current.gameObject);
        }

        ClearEnemyInfoBlockIntentDisplays();

        selectSpellsForCombatScreen.SetActive(false);

        CombatManager._Instance.SetSpellPiles(newCombatDrawPile);
        prevCombatDrawPile = newCombatDrawPile;
    }

    public void ConfirmSpellSelection()
    {
        spellSelectionForCombatConfirmed = true;
    }

    public void AddSpellToSpellBook(SpellLabel spellLabel)
    {
        AddSpellToSpellBook(Spell.GetSpellOfType(spellLabel));
    }

    public void AddSpellToSpellBook(Spell spell)
    {
        spellBook.AddSpell(spell);
        StartCoroutine(ShowAddSpellSequence(spell));
    }

    public void RemoveSpellFromSpellBook(Spell spell)
    {
        spellBook.RemoveSpell(spell);
        StartCoroutine(ShowRemoveSpellSequence(spell));
    }
    #endregion

    private void EquipCharacterLoadout(Character c)
    {
        // Spawn default num spell slots
        combatPileSize = c.GetStartingCombatPileSize();
        CombatManager._Instance.SetHandSize(c.GetStartingHandSize());

        // Equip starting spells
        spellBook = new Spellbook(c.GetStartingSpells());

        // Equip character equipment
        EquipEquipment(c.GetStartingRobe());
        EquipEquipment(c.GetStartingHat());
        EquipEquipment(c.GetStartingWand());
        AddBook(c.GetStartingBook());

        // Remove Owned Book from Awardable Books
        awardableBooks.Remove(GetOwnedBookLabel(0));

        // Set player stats
        maxPlayerHP = c.GetMaxHP();
        characterMaxMana = c.GetMaxMana();
        maxPlayerMana = characterMaxMana + manaFromEquipment;
        currentPlayerCurrency = c.GetStartingCurrency();
        currentPlayerClothierCurrency = c.GetStartingClothierCurrency();
        currentPlayerHP = c.GetStartingHP();
        currentPlayerMana = maxPlayerMana;
    }

    private void EquipRobe(Robe robe)
    {
        // Unequip old
        if (playerEquippedRobe != null)
            UnequipEquipment(playerEquippedRobe);

        // Equip new
        playerEquippedRobe = robe;
        robe.OnEquip();
    }

    private void EquipHat(Hat hat)
    {
        // Unequip old
        if (playerEquippedHat != null)
            UnequipEquipment(playerEquippedHat);

        // Equip new
        playerEquippedHat = hat;
        hat.OnEquip();
    }

    private void EquipWand(Wand wand)
    {
        // Unequip old
        if (playerEquippedWand != null)
            UnequipEquipment(playerEquippedWand);

        // Equip new
        playerEquippedWand = wand;
        wand.OnEquip();
    }

    public void EquipEquipment(Equipment e)
    {
        switch (e)
        {
            case Robe robe:
                EquipRobe(robe);
                break;
            case Hat hat:
                EquipHat(hat);
                break;
            case Wand wand:
                EquipWand(wand);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public Equipment GetEquippedEquipmentOfSameType(Equipment e)
    {
        switch (e)
        {
            case Robe robe:
                return playerEquippedRobe;
            case Hat hat:
                return playerEquippedHat;
            case Wand wand:
                return playerEquippedWand;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    private void UnequipEquipment(Equipment e)
    {
        e.OnUnequip();
        switch (e)
        {
            case Robe robe:
                playerEquippedRobe = null;
                break;
            case Hat hat:
                playerEquippedHat = null;
                break;
            case Wand wand:
                playerEquippedWand = null;
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public void RemoveArtifactFromPool(ArtifactLabel label)
    {
        awardableArtifacts.Remove(label);
    }

    public void AddArtifact(ArtifactLabel type)
    {
        Artifact artifact = Artifact.GetArtifactOfType(type);
        AddArtifact(artifact);
    }

    public void AddArtifact(Artifact artifact)
    {
        artifact.OnEquip();

        ArtifactDisplay spawned = Instantiate(artifactDisplay, artifactBar);
        spawned.SetItem(artifact);
        spawned.name = "Artifact(" + artifact.Name + ")";

        equippedArtifacts.Add(artifact, spawned);
    }

    public void RemoveArtifact(ArtifactLabel label)
    {
        foreach (KeyValuePair<Artifact, ArtifactDisplay> artifactDisplay in equippedArtifacts)
        {
            if (artifactDisplay.Key.GetLabel() == label)
            {
                RemoveArtifact(artifactDisplay.Key);
                break;
            }
        }
    }

    public void RemoveArtifact(Artifact artifact)
    {
        artifact.OnUnequip();

        Destroy(equippedArtifacts[artifact].gameObject);
        equippedArtifacts.Remove(artifact);
    }

    public void AddBook(BookLabel type)
    {
        Book book = Book.GetBookOfType(type);

        BookDisplay spawned = Instantiate(bookDisplay, bookBar);
        spawned.SetItem(book);
        spawned.name = "Book(" + type + ")";

        bookDisplayTracker.Add(book.GetLabel(), spawned);
        equippedBooks.Add(type, book);
    }

    public void RemoveBook(BookLabel type)
    {
        Destroy(bookDisplayTracker[type].gameObject);
        bookDisplayTracker.Remove(type);

        equippedBooks.Remove(type);
    }

    [ContextMenu("Upgrade Books")]
    public void UpgradeBooks()
    {
        foreach (KeyValuePair<BookLabel, Book> b in equippedBooks)
        {
            b.Value.TryCallLevelUp(true);
        }
    }

    public void SwapBooks(BookLabel swappingOut, BookLabel swappingTo)
    {
        RemoveBook(swappingOut);
        AddBook(swappingTo);
        AnimateBook(swappingTo);
    }

    public void AnimateArtifact(ArtifactLabel artifactLabel)
    {
        foreach (KeyValuePair<Artifact, ArtifactDisplay> kvp in equippedArtifacts)
        {
            if (kvp.Key.GetLabel() == artifactLabel)
            {
                kvp.Value.AnimateScale();
            }
        }
    }

    public void AnimateArtifact(Artifact artifact)
    {
        if (equippedArtifacts.ContainsKey(artifact))
        {
            equippedArtifacts[artifact].AnimateScale();
        }
    }

    public void AnimateBook(BookLabel label)
    {
        if (bookDisplayTracker.ContainsKey(label))
        {
            bookDisplayTracker[label].AnimateScale();
        }
    }

    public bool AddPotionIngredient(PotionIngredientType ingredient)
    {
        bool r;
        if (potionIngredientMap.ContainsKey(ingredient))
        {
            potionIngredientMap[ingredient] = potionIngredientMap[ingredient] + 1;
            r = false;
        }
        else
        {
            potionIngredientMap.Add(ingredient, 1);
            r = true;
        }

        if (brewPotionScreenOpen)
        {
            ReSpawnPotionIngredientToBrewList(ingredient);
        }

        return r;
    }

    public bool RemovePotionIngredient(PotionIngredientType ingredient)
    {
        if (potionIngredientMap.ContainsKey(ingredient))
        {
            int numIngredient = potionIngredientMap[ingredient];
            if (numIngredient == 1)
            {
                potionIngredientMap.Remove(ingredient);
                return true;
            }
            else
            {
                potionIngredientMap[ingredient] = numIngredient - 1;
                return false;
            }
        }
        else
        {
            Debug.Log("Potion Ingredient: " + ingredient + ", Not Found in Map");
            return false;
        }
    }

    public PotionIngredientType GetRandomPotionIngredient()
    {
        return potionIngredientRandomOddsOfTypeMap[potionIngredientCatagoryOdds.GetOption()].GetOption();
    }

    public void AddRandomPotionIngredient()
    {
        AddPotionIngredient(GetRandomPotionIngredient());
    }

    private void TryAddPersistentTokens()
    {
        if (!PlayerPrefs.HasKey(persistentTokensKey))
        {
            PlayerPrefs.SetInt(persistentTokensKey, 0);
            PlayerPrefs.Save();
        }
    }

    public int GetNumPersistentTokens()
    {
        return PlayerPrefs.GetInt(persistentTokensKey);
    }

    public void AlterPersistentTokens(int amount)
    {
        PlayerPrefs.SetInt(persistentTokensKey, PlayerPrefs.GetInt(persistentTokensKey) + amount);
        PlayerPrefs.Save();
    }

    public IEnumerator StageLoop()
    {
        bool shouldBreak = false;
        while (!shouldBreak)
        {
            yield return new WaitUntil(() => currentOccurance != null);
            currentOccurance.SetResolve(false);

            // Only Clear Rewards Screen once we've fully advanced from the room
            RewardManager._Instance.ClearRewardsScreen();
            MapManager._Instance.Hide();

            OnEnterNewRoom?.Invoke();
            OnEnterSpecificRoomActionMap[currentOccurance.Type]?.Invoke();

            yield return StartCoroutine(currentOccurance.RunOccurance());

            // if we've game overed, break the loop as the level is done
            if (GameOvered)
            {
                shouldBreak = true;
            }

            // if beat boss, break the loop as the level is done
            if (currentOccurance.Type == MapNodeType.Boss)
            {
                shouldBreak = true;
            }

            // Reset current occurance
            currentNode.SetMapNodeState(MapNodeState.COMPLETED);
            currentOccurance = null;
            CanSetCurrentGameOccurance = true;

            // Set the new Connections to be Accessable
            currentNode.SetAllConnectorsState(MapNodeConnectorState.ACCESSABLE, false);

            ScoreManager._Instance.AddScore(ScoreReason.RoomCleared);

            if (!shouldBreak)
            {
                // move to next room
                MapManager._Instance.UnlockNext(currentNode);
                MapManager._Instance.Show();
            }
            else
            {
                break;
            }
        }

        // Determine what Happens Next
        if (GameOvered)
        {
            // Game Over
            Debug.Log("Level Ended With: Game Over");
            StartCoroutine(GameOverSequence());
        }
        else
        {
            // Beat Stage
            Debug.Log("Level Ended With: Beat Stage");
            if (MapManager._Instance.HasNextStage)
            {
                MapManager._Instance.Clear();

                string nextStage = MapManager._Instance.GetNextStage();
                Debug.Log("Loading Next Stage: " + nextStage);

                SceneManager.LoadScene(nextStage);
            }
            else
            {
                Debug.Log("No more Stages: Game Won");
                yield return StartCoroutine(GameWonSequence());
            }
        }
    }

    public IEnumerator SetCurrentGameOccurance(MapNodeUI setNodeTo)
    {
        if (currentNode != null)
        {
            // Set the current Connection to be Traversed
            NodeConnection connection = currentNode.SetConnectionState(currentNode, setNodeTo, MapNodeConnectorState.TRAVERSED);
            // Set the previous Connections to be Unaccessable
            currentNode.SetAllConnectorsState(MapNodeConnectorState.UNACCESSABLE, true);

            float delayBetweenSegments = MapManager._Instance.GetBuildConnectorDelay();
            yield return StartCoroutine(currentNode.SetConnectionColors(connection, delayBetweenSegments));
            yield return new WaitForSeconds(delayOnReachNode);
        }
        else
        {
            yield return new WaitForSeconds(delayOnReachNode);
        }

        currentNode = setNodeTo;
        currentOccurance = currentNode.GetSetTo();
    }

    public GameOccurance GetCurrentGameOccurance()
    {
        return currentOccurance;
    }

    [ContextMenu("ResolveCurrentEvent")]
    public void ResolveCurrentEvent()
    {
        currentOccurance.SetResolve(true);
    }

    public void ToggleMap()
    {
        if (!CanSetCurrentGameOccurance)
        {
            MapManager._Instance.ToggleVisibility();
        }
    }

    public void ToggleSpellBook()
    {
        spellBookScreen.SetActive(!spellBookScreen.activeInHierarchy);

        // Opening 
        if (spellBookScreen.activeInHierarchy)
        {
            foreach (Spell spell in spellBook.GetSpellBookEntries())
            {
                VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, spellBookSpawnSpellDisplaysOn);
                spawned.SetSpell(spell);
                spellBookSpawnedSpellDisplays.Add(spawned);
            }
        }
        else
        {
            // Closing
            while (spellBookSpawnedSpellDisplays.Count > 0)
            {
                VisualSpellDisplay cur = spellBookSpawnedSpellDisplays[0];
                spellBookSpawnedSpellDisplays.RemoveAt(0);
                Destroy(cur.gameObject);
            }
        }

        MapManager._Instance.ToggleVisibility();
    }

    public Spellbook GetSpellbook()
    {
        return spellBook;
    }

    public void RemoveRandomSpellOfColor(SpellColor ofColor)
    {
        RemoveRandomSpellsMatchingConditions(spell => spell.Color == ofColor);
    }

    public void RemoveRandomSpellsMatchingConditions(Func<Spell, bool> removeCondition)
    {
        // Make list of all spells matching condition
        List<Spell> toRemove = new List<Spell>();
        foreach (Spell entry in spellBook.GetSpellBookEntries())
        {
            if (removeCondition(entry))
            {
                toRemove.Add(entry);
            }
        }

        if (toRemove.Count <= 0) return;

        // Remove a random one from the list
        RemoveSpellFromSpellBook(RandomHelper.GetRandomFromList(toRemove));
    }

    public void RemoveAllSpellsOfColor(SpellColor ofColor)
    {
        RemoveAllSpellsMatchingConditions(spell => spell.Color == ofColor);
    }

    public void RemoveAllSpellsMatchingConditions(Func<Spell, bool> removeCondition)
    {
        // Make list of all spells matching condition
        List<Spell> toRemove = new List<Spell>();
        foreach (Spell entry in spellBook.GetSpellBookEntries())
        {
            if (removeCondition(entry))
            {
                toRemove.Add(entry);
            }
        }

        // Remove all from the list
        foreach (Spell spell in toRemove)
        {
            RemoveSpellFromSpellBook(spell);
        }
    }

    private bool interruptSpellSequence;
    [SerializeField] private GameObject interruptSpellSequenceButton;
    public void InterrupSpellSequence()
    {
        interruptSpellSequence = true;
    }

    public IEnumerator RemoveSpellSequence(Func<Spell, bool> viableSpellFunc, int numSpells = 1, Action onCompleteAction = null)
    {
        yield return StartCoroutine(SelectSpellSequence(spell => RemoveSpellFromSpellBook(spell), numSpells, viableSpellFunc, onCompleteAction));
    }

    public IEnumerator TransformSpellSequence(Func<Spell, bool> viableSpellFunc, int numSpells = 1, Action onCompleteAction = null)
    {
        yield return StartCoroutine(SelectSpellSequence(spell =>
        {
            RemoveSpellFromSpellBook(spell);
            AddSpellToSpellBook(GetRandomSpellWithConditions(newSpell => AcceptSpellRewardFunc(newSpell) && newSpell.Rarity == spell.Rarity));
        }, numSpells, viableSpellFunc, onCompleteAction));
    }

    public IEnumerator DuplicateSpellSequence(Func<Spell, bool> viableSpellFunc, int numSpells = 1, Action onCompleteAction = null)
    {
        yield return StartCoroutine(SelectSpellSequence(spell =>
        {
            AddSpellToSpellBook(spell);
        }, numSpells, viableSpellFunc, onCompleteAction));
    }

    public IEnumerator TradeSpellSequence(Spell newSpell, Func<Spell, bool> viableSpellFunc, Action onCompleteAction = null)
    {
        yield return StartCoroutine(SelectSpellSequence(spell =>
        {
            RemoveSpellFromSpellBook(spell);
            AddSpellToSpellBook(newSpell);
        }, 1, viableSpellFunc, onCompleteAction));
    }

    public IEnumerator SelectSpellSequence(Action<Spell> doWithSelected, int numToSelect, Func<Spell, bool> viableSpell, Action onComplete)
    {
        yield return StartCoroutine(SelectSpellSequence(doWithSelected, numToSelect, viableSpell, onComplete, false));
    }

    private IEnumerator SelectSpellSequence(Action<Spell> doWithSelected, int numToSelect, Func<Spell, bool> viableSpell, Action onComplete, bool allowInterrupt)
    {
        if (allowInterrupt)
        {
            interruptSpellSequenceButton.SetActive(true);
        }

        selectSpellScreen.SetActive(true);

        List<VisualSpellDisplay> spawnedDisplays = new List<VisualSpellDisplay>();

        List<Spell> selectedSpells = new List<Spell>();

        // Opening 
        foreach (Spell spell in spellBook.GetSpellBookEntries())
        {
            if (!viableSpell(spell))
            {
                continue;
            }

            VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, spawnSelectSpellDisplaysOn);
            spawned.SetSpell(spell);
            spawnedDisplays.Add(spawned);
            spawned.AddOnClick(delegate
            {
                if (selectedSpells.Contains(spell))
                {
                    // Deselect
                    // Select
                    selectedSpells.Remove(spell);
                    if (numToSelect > 1)
                    {
                        spawned.SetSpellDisplayState(SpellDisplayState.Normal);
                    }
                }
                else
                {
                    // Select
                    selectedSpells.Add(spell);
                    if (numToSelect > 1)
                    {
                        spawned.SetSpellDisplayState(SpellDisplayState.Selected);
                    }
                }
            });
        }

        yield return new WaitUntil(() => selectedSpells.Count >= numToSelect || interruptSpellSequence);

        selectSpellScreen.SetActive(false);

        // Closing
        while (spawnedDisplays.Count > 0)
        {
            VisualSpellDisplay cur = spawnedDisplays[0];
            spawnedDisplays.RemoveAt(0);
            Destroy(cur.gameObject);
        }

        if (interruptSpellSequence)
        {
            interruptSpellSequence = false;
            interruptSpellSequenceButton.SetActive(false);
            yield break;
        }

        foreach (Spell spell in selectedSpells)
        {
            doWithSelected(spell);
        }
        onComplete?.Invoke();
    }

    public void IncreaseStat(BaseStat stat, int incBy)
    {
        switch (stat)
        {
            case BaseStat.Damage:
                DamageFromEquipment += incBy;
                return;
            case BaseStat.Defense:
                DefenseFromEquipment += incBy;
                return;
            case BaseStat.Mana:
                AlterManaFromEquipment(incBy);
                return;
        }
    }

    public void AlterGold(int amount)
    {
        // Spawn Popup Text
        PopupText spawned = Instantiate(popupTextPrefab, currencyText.transform);
        spawned.Set((amount > 0 ? "+" : "") + Utils.RoundTo(amount, 1).ToString(), Color.yellow);

        currentPlayerCurrency += amount;
    }

    public void AlterPelts(int amount)
    {
        // Spawn Popup Text
        PopupText spawned = Instantiate(popupTextPrefab, clothierCurrencyText.transform);
        spawned.Set((amount > 0 ? "+" : "") + Utils.RoundTo(amount, 1).ToString(), Color.blue);

        currentPlayerClothierCurrency += amount;
    }

    public bool AlterPlayerCurrentHP(int amount, DamageType damageType, bool spawnPopupText = true)
    {
        // Barricade Effect
        if (amount < -1 && HasArtifact(ArtifactLabel.Barricade))
        {
            amount += Barricade.ReductionAmount;
            AnimateArtifact(ArtifactLabel.Barricade);
        }

        // If amount is still less than 0 (i.e., a negative number), the player is taking damage
        if (amount < 0)
        {
            OnPlayerRecieveDamage?.Invoke();
        }

        if (spawnPopupText)
        {
            PopupText spawned = Instantiate(popupTextPrefab, hpText.transform);
            spawned.Set((amount > 0 ? "+" : "") + Utils.RoundTo(amount, 1).ToString(), UIManager._Instance.GetDamageTypeColor(damageType));
        }

        if (currentPlayerHP + amount > maxPlayerHP)
        {
            currentPlayerHP = maxPlayerHP;
        }
        else if (currentPlayerHP + amount < 0)
        {
            currentPlayerHP = 0;
        }
        else
        {
            currentPlayerHP += amount;
        }

        // Return true if the player is still alive, return false if not
        if (currentPlayerHP > 0)
        {
            return true;
        }
        return false;
    }

    public void AlterPlayerMaxHP(int changeBy)
    {
        // Change max HP
        maxPlayerHP += changeBy;

        // if gaining max hp, gain that same amount of current hp
        if (changeBy > 0)
        {
            AlterPlayerCurrentHP(changeBy, DamageType.Heal);
        }
        else if (changeBy < 0 && currentPlayerHP > maxPlayerHP)
        {
            // if losing max hp AND the player has lost enough max hp to the point where their current hp is no higher than their max hp, even that out
            currentPlayerHP = maxPlayerHP;
        }

    }

    public int GetCurrentCharacterHP()
    {
        return currentPlayerHP;
    }

    public int GetMaxPlayerHP()
    {
        return maxPlayerHP;
    }

    public int GetPlayerCurrency()
    {
        return currentPlayerCurrency;
    }

    public int GetCurrentPlayerMana()
    {
        return currentPlayerMana;
    }

    public int GetMaxPlayerMana()
    {
        return maxPlayerMana;
    }

    public void AlterPlayerCurrentMana(int amount)
    {
        if (currentPlayerMana + amount > maxPlayerMana)
        {
            currentPlayerMana = maxPlayerMana;
        }
        else if (currentPlayerMana + amount < 0)
        {
            currentPlayerMana = 0;
        }
        else
        {
            currentPlayerMana += amount;
        }
    }

    public void SetCurrentPlayerMana(int playerMana)
    {
        currentPlayerMana = playerMana;
    }

    public void AlterManaFromEquipment(int changeBy)
    {
        manaFromEquipment += changeBy;

        maxPlayerMana = characterMaxMana + manaFromEquipment;

        AlterPlayerCurrentMana(changeBy);
    }

    public int GetManaPerTurn()
    {
        return maxPlayerMana - playerCharacter.GetManaPerTurnSubFromMax();
    }

    public Character GetCharacter()
    {
        return playerCharacter;
    }

    public void AddOnEnterSpecificRoomAction(MapNodeType type, Action a)
    {
        OnEnterSpecificRoomActionMap[type] += a;
    }

    public void RemoveOnEnterSpecificRoomAction(MapNodeType type, Action a)
    {
        OnEnterSpecificRoomActionMap[type] -= a;
    }

    public bool HasArtifact(ArtifactLabel artifactLabel)
    {
        foreach (KeyValuePair<Artifact, ArtifactDisplay> kvp in equippedArtifacts)
        {
            if (kvp.Key.GetLabel() == artifactLabel)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasBook(BookLabel label)
    {
        return bookDisplayTracker.ContainsKey(label);
    }


    #region UI

    public bool CanUpgradeActiveBook => GetOwnedBook(0).CanLevelUp;

    public void PopManaText()
    {
        manaTextEventCaller.Force();
    }

    [Header("Game Over")]
    [SerializeField] private CanvasGroup gameOverCV;
    [SerializeField] private CanvasGroup gameWonCV;
    [SerializeField] private CanvasGroup endGameScoreCV;
    [SerializeField] private float changeEndGameScoreCVAlphaRate;
    [SerializeField] private float changeGameOverCVAlphaRate;
    [SerializeField] private float changeGameWonCVAlphaRate;

    [SerializeField] private EndGameScoreDisplay endGameScoreDisplay;
    [SerializeField] private TextMeshProUGUI endGameFinalScoreText;
    [SerializeField] private Transform endGameSpawnScoreDisplaysOn;
    [SerializeField] private float betweenScoresDelay;

    public IEnumerator GameOverSequence()
    {
        gameOverCV.blocksRaycasts = true;

        StartCoroutine(ShowScoreSequence());

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(gameOverCV, 1, Time.deltaTime * changeGameOverCVAlphaRate));
    }

    private IEnumerator ShowScoreSequence()
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(endGameScoreCV, 1, Time.deltaTime * changeEndGameScoreCVAlphaRate));

        endGameScoreCV.blocksRaycasts = true;

        int finalScore = 0;
        foreach (EndGameScoreData data in ScoreManager._Instance.GetFinalScoreData())
        {
            if (data.Num == 0) continue;
            EndGameScoreDisplay spawned = Instantiate(endGameScoreDisplay, endGameSpawnScoreDisplaysOn);
            spawned.Set(data);
            finalScore += data.Score;
            endGameFinalScoreText.text = "Score: " + finalScore.ToString();

            yield return new WaitForSeconds(betweenScoresDelay);
        }
    }


    public IEnumerator GameWonSequence()
    {
        gameWonCV.blocksRaycasts = true;

        StartCoroutine(ShowScoreSequence());

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(gameWonCV, 1, Time.deltaTime * changeGameWonCVAlphaRate));
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePotionIngredientListScreen()
    {
        potionIngredientListScreen.SetActive(!potionIngredientListScreen.activeInHierarchy);

        // Opening 
        if (potionIngredientListScreen.activeInHierarchy)
        {
            foreach (KeyValuePair<PotionIngredientType, int> kvp in potionIngredientMap)
            {
                PotionIngredientListEntry spawned = Instantiate(potionIngredientListEntryPrefab, potionIngredientListParent);
                spawned.Set(kvp.Key, kvp.Value, false);
                spawnedPotionIngredientListEntries.Add(spawned);
            }
        }
        else
        {
            // Closing
            while (spawnedPotionIngredientListEntries.Count > 0)
            {
                PotionIngredientListEntry cur = spawnedPotionIngredientListEntries[0];
                spawnedPotionIngredientListEntries.RemoveAt(0);
                Destroy(cur.gameObject);
            }
        }
    }

    private void SwitchUIScreens(GameObject[] turnOn, GameObject[] turnOff)
    {
        foreach (GameObject obj in turnOn)
        {
            obj.SetActive(true);
        }
        foreach (GameObject obj in turnOff)
        {
            obj.SetActive(false);
        }
    }

    [Header("Spellbook Change Sequences")]
    [SerializeField] private float spellbookChangeFadeOutRate = 1;
    [SerializeField] private float spellbookChangeFadeInRate = 1;
    [SerializeField] private float spellbookChangeScaleRate = 1;
    [SerializeField] private float spellbookChangeDelayBeforeFadingOut;
    [SerializeField] private Vector2 addSpellDisplaySizeDelta = new Vector2(300, 400);
    [SerializeField] private Transform spellBookChangeDisplayContainer;
    public IEnumerator ShowAddSpellSequence(Spell spell)
    {
        // Spawn Visual
        VisualSpellDisplay spellDisplay = Instantiate(visualSpellDisplayPrefab, spellBookChangeDisplayContainer);
        spellDisplay.SetSpell(spell);
        RectTransform spellDisplayRect = spellDisplay.transform as RectTransform;
        spellDisplayRect.sizeDelta = addSpellDisplaySizeDelta;
        CanvasGroup spellDisplayCV = spellDisplay.GetCanvasGroup();

        spellDisplayCV.blocksRaycasts = false;
        spellDisplay.SetCVLocked(true);
        spellDisplay.SetScaleLocked(true);

        // Scale Up
        Coroutine changeScale = StartCoroutine(Utils.MoveTowardsScale(spellDisplayRect, spellDisplayRect.localScale * 2, spellbookChangeScaleRate));

        // Slowly Fade Visual In
        spellDisplayCV.alpha = 0;
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(spellDisplayCV, 1, spellbookChangeFadeInRate));

        // Wait
        yield return new WaitForSeconds(spellbookChangeDelayBeforeFadingOut);

        // Slowly Fade Visual Out
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(spellDisplayCV, 0, spellbookChangeFadeOutRate));

        // Stop Scaling
        StopCoroutine(changeScale);

        // Destroy Visual
        Destroy(spellDisplayCV.gameObject);
    }

    public IEnumerator ShowRemoveSpellSequence(Spell spell)
    {
        // Spawn Visual
        VisualSpellDisplay spellDisplay = Instantiate(visualSpellDisplayPrefab, spellBookChangeDisplayContainer);
        spellDisplay.SetSpell(spell);
        spellDisplay.SetSpellDisplayState(SpellDisplayState.Selected);
        RectTransform spellDisplayRect = spellDisplay.transform as RectTransform;
        spellDisplayRect.sizeDelta = addSpellDisplaySizeDelta;
        CanvasGroup spellDisplayCV = spellDisplay.GetCanvasGroup();
        spellDisplayCV.alpha = 0;
        spellDisplayCV.blocksRaycasts = false;
        spellDisplay.SetCVLocked(true);
        spellDisplay.SetScaleLocked(true);

        // Scale Down
        Coroutine changeScale = StartCoroutine(Utils.MoveTowardsScale(spellDisplayRect, Vector3.zero, spellbookChangeScaleRate));

        // Slowly Fade Visual In
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(spellDisplayCV, 1, spellbookChangeFadeInRate));

        // Wait
        yield return new WaitForSeconds(spellbookChangeDelayBeforeFadingOut);

        // Slowly Fade Visual Out
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(spellDisplayCV, 0, spellbookChangeFadeOutRate));

        // Stop Scaling
        StopCoroutine(changeScale);

        // Destroy Visual
        Destroy(spellDisplayCV.gameObject);
    }

    #endregion

    #region Campfire

    [Header("Potion Screen")]
    [SerializeField] private PotionIngredientListEntry potionIngredientListEntryPrefab;
    [SerializeField] private Transform potionIngredientListParent;
    [SerializeField] private GameObject potionIngredientListScreen;
    [SerializeField] private Transform potionDisplayList;
    [SerializeField] private PotionDisplay potionDisplayPrefab;

    // Brew Screen
    [SerializeField] private GameObject[] turnOnForBrewPotionScreen;
    [SerializeField] private GameObject[] turnOffForBrewPotionScreen;
    [SerializeField] private Transform brewPotionScreenInventoryPotionIngredientList;
    [SerializeField] private BrewingPotionDisplay brewingPotionDisplay;
    private List<PotionIngredientListEntry> spawnedBrewPotionIngredientListEntries = new List<PotionIngredientListEntry>();

    private Dictionary<Potion, PotionDisplay> spawnedPotionDisplays = new Dictionary<Potion, PotionDisplay>();
    private bool brewPotionScreenOpen;

    public void RestAtCampfire()
    {
        float value = BalenceManager._Instance.GetValue(MapNodeType.Campfire, "HealPercent");
        float percentHP = value / 100;
        AlterPlayerCurrentHP(Mathf.CeilToInt(maxPlayerHP * percentHP), DamageType.Heal);
    }

    public void ReSpawnPotionIngredientToBrewList(PotionIngredientType type)
    {
        foreach (PotionIngredientListEntry entry in spawnedBrewPotionIngredientListEntries)
        {
            if (entry.Type == type)
            {
                entry.UpdateQuantity(potionIngredientMap[type]);
                return;
            }
        }

        SpawnPotionIngredientEntry(type, 1);
    }

    private void SpawnPotionIngredientEntry(PotionIngredientType type, int num)
    {
        PotionIngredientListEntry cur = Instantiate(potionIngredientListEntryPrefab, brewPotionScreenInventoryPotionIngredientList);
        cur.Set(type, num, true);
        spawnedBrewPotionIngredientListEntries.Add(cur);
        cur.AddOnPressAction(delegate
        {
            // Get an instance of the ingredient
            PotionIngredient ingredient = PotionIngredient.GetPotionIngredientOfType(type);

            // Onlly allow the click if we can actually add the ingredient to the pot
            if (currentPotion.HasComponentOfType(ingredient)) return;

            // Add the ingredient to the potion
            currentPotion.AddIngredient(ingredient);

            // Remove it from the players inventory
            if (RemovePotionIngredient(type))
            {
                // Removing it from the inventory removed the last ingredient
                // Remove the spawned list entry and destroy the game object
                spawnedBrewPotionIngredientListEntries.Remove(cur);
                Destroy(cur.gameObject);
                cur.DestroyToolTip();
            }
            else
            {
                cur.UpdateQuantity(potionIngredientMap[type]);
            }
        });
    }

    private void FillBrewPotionIngredientList()
    {
        foreach (KeyValuePair<PotionIngredientType, int> entry in potionIngredientMap)
        {
            SpawnPotionIngredientEntry(entry.Key, entry.Value);
        }
    }

    private void ClearBrewPotionIngredientList()
    {
        while (spawnedBrewPotionIngredientListEntries.Count > 0)
        {
            PotionIngredientListEntry cur = spawnedBrewPotionIngredientListEntries[0];
            spawnedBrewPotionIngredientListEntries.RemoveAt(0);
            Destroy(cur.gameObject);
        }

        currentPotion.ClearPotionBase();
        currentPotion.ClearPotionPotency();
        currentPotion.ClearPotionTargeter();
    }

    public void OpenBrewPotionScreen()
    {
        SwitchUIScreens(turnOnForBrewPotionScreen, turnOffForBrewPotionScreen);

        FillBrewPotionIngredientList();

        brewPotionScreenOpen = true;
    }


    public void CloseBrewPotionScreen()
    {
        SwitchUIScreens(turnOffForBrewPotionScreen, turnOnForBrewPotionScreen);

        // Re-add ingredients put into potion but not actually combined
        if (currentPotion.CurPotionBaseIngredient != null)
        {
            AddPotionIngredient(currentPotion.CurPotionBaseIngredient.Type);
        }
        if (currentPotion.CurPotionPotencyIngredient != null)
        {
            AddPotionIngredient(currentPotion.CurPotionPotencyIngredient.Type);
        }
        if (currentPotion.CurPotionTargeterIngredient != null)
        {
            AddPotionIngredient(currentPotion.CurPotionTargeterIngredient.Type);
        }

        // Destroy list entries
        ClearBrewPotionIngredientList();

        brewPotionScreenOpen = false;
    }

    public void BrewPotion()
    {
        if (currentPotion.ReadyForBrew)
        {
            // Brew the potion
            currentPotion.Brew();

            // Add the potion
            AddPotion(currentPotion);

            // Reset 
            SetNewPotion();
        }
    }

    public void SetNewPotion()
    {
        currentPotion = new Potion();
        brewingPotionDisplay.SetPotion(currentPotion);
    }

    public void AddPotion(Potion p)
    {
        availablePotions.Add(p);

        // Spawn UI
        PotionDisplay spawned = Instantiate(potionDisplayPrefab, potionDisplayList);
        spawned.Set(p);
        p.SetPotionIcon(spawned);

        spawnedPotionDisplays.Add(p, spawned);
    }


    public void RemovePotion(Potion p)
    {
        availablePotions.Remove(p);

        PotionDisplay d = spawnedPotionDisplays[p];
        spawnedPotionDisplays.Remove(p);
        Destroy(d.gameObject);
    }

    public void UsePotion(Potion p)
    {
        p.Use();
        RemovePotion(p);
    }

    [ContextMenu("UseFirstPotion")]
    public void UseFirstPotion()
    {
        if (availablePotions.Count <= 0) return;
        Potion p = availablePotions[0];
        UsePotion(p);
    }

    #endregion

    #region Tavern

    [System.Serializable]
    private class TavernScreenInformation
    {
        public GameObject[] turnOnForScreen;
        public Transform parentSpawnsTo;
    }

    private enum TavernScreen
    {
        Innkeeper,
        ClothierDefault,
        ClothierWares,
        Merchant,
        Librarian,
        Default
    }

    [Header("Tavern")]
    [SerializeField] private int numPotionIngredientShopOffers;
    [SerializeField] private int numArtifactShopOffers;
    [SerializeField] private int numEquipmentShopOffers;
    [SerializeField] private int numBookShopOffers;

    [SerializeField] private EquipmentShopOffer equipmentShopOfferPrefab;
    [SerializeField] private ArtifactShopOffer artifactShopOfferPrefab;
    [SerializeField] private IngredientShopOffer ingredientShopOfferPrefab;
    [SerializeField] private BookShopOffer bookShopOfferPrefab;

    private List<ArtifactShopOffer> shopArtifactList = new List<ArtifactShopOffer>();
    private List<IngredientShopOffer> shopIngredientList = new List<IngredientShopOffer>();
    private List<EquipmentShopOffer> shopEquipmentList = new List<EquipmentShopOffer>();
    private List<BookShopOffer> shopBookList = new List<BookShopOffer>();

    [SerializeField] private SerializableDictionary<TavernScreen, TavernScreenInformation> tavernScreens = new SerializableDictionary<TavernScreen, TavernScreenInformation>();

    [SerializeField] private SerializableDictionary<Rarity, Vector2> minMaxArtifactCostDict = new SerializableDictionary<Rarity, Vector2>();
    [SerializeField] private SerializableDictionary<Rarity, Vector2> minMaxEquipmentCostDict = new SerializableDictionary<Rarity, Vector2>();
    [SerializeField] private SerializableDictionary<Rarity, Vector2> minMaxBookCostDict = new SerializableDictionary<Rarity, Vector2>();
    [SerializeField] private Vector2 minMaxIngredientCost;

    public void MultiplyCosts(float multBy)
    {
        foreach (ShopOffer offer in shopArtifactList)
        {
            offer.MultiplyCost(multBy);
        }

        foreach (ShopOffer offer in shopIngredientList)
        {
            offer.MultiplyCost(multBy);
        }

        foreach (ShopOffer offer in shopEquipmentList)
        {
            offer.MultiplyCost(multBy);
        }

        foreach (ShopOffer offer in shopBookList)
        {
            offer.MultiplyCost(multBy);
        }
    }

    private TavernScreen ParseStringIntoTavernScreen(string str)
    {
        TavernScreen screen;
        Enum.TryParse<TavernScreen>(str, out screen);
        return screen;
    }

    public void OpenTavernScreen(string screen)
    {
        foreach (GameObject obj in tavernScreens[ParseStringIntoTavernScreen(screen)].turnOnForScreen)
        {
            obj.SetActive(true);
        }
    }

    public void CloseTavernScreen(string screen)
    {
        foreach (GameObject obj in tavernScreens[ParseStringIntoTavernScreen(screen)].turnOnForScreen)
        {
            obj.SetActive(false);
        }
    }

    public void LoadShop()
    {
        // Spawn Offers
        TavernScreenInformation innkeeperInfo = tavernScreens[TavernScreen.Innkeeper];
        TavernScreenInformation merchantInfo = tavernScreens[TavernScreen.Merchant];
        TavernScreenInformation clothierWaresInfo = tavernScreens[TavernScreen.ClothierWares];
        TavernScreenInformation librarianInfo = tavernScreens[TavernScreen.Librarian];

        // Artifacts
        for (int i = 0; i < numArtifactShopOffers; i++)
        {
            // Ran out of aartifacts
            if (awardableArtifacts.Count == 0)
            {
                break;
            }

            ArtifactShopOffer offer = Instantiate(artifactShopOfferPrefab, merchantInfo.parentSpawnsTo);

            // Get artifact that will be offered
            Artifact offered = Artifact.GetArtifactOfType(GetRandomArtifact());

            // Determine cost
            int cost = Mathf.RoundToInt(RandomHelper.RandomFloat(minMaxArtifactCostDict[offered.Rarity]));
            offer.Set(offered, cost);
            shopArtifactList.Add(offer);
        }

        // Books
        for (int i = 0; i < numBookShopOffers; i++)
        {
            // Ran out of aartifacts
            if (awardableBooks.Count == 0)
            {
                break;
            }

            BookShopOffer offer = Instantiate(bookShopOfferPrefab, librarianInfo.parentSpawnsTo);

            // Get artifact that will be offered
            Book offered = Book.GetBookOfType(GetRandomBook());

            // Determine cost
            int cost = Mathf.RoundToInt(RandomHelper.RandomFloat(minMaxBookCostDict[offered.Rarity]));
            offer.Set(offered, cost);
            shopBookList.Add(offer);
        }


        // Potion Ingredients
        for (int i = 0; i < numPotionIngredientShopOffers; i++)
        {
            IngredientShopOffer offer = Instantiate(ingredientShopOfferPrefab, innkeeperInfo.parentSpawnsTo);
            offer.Set(GetRandomPotionIngredient(), RandomHelper.RandomIntExclusive(minMaxIngredientCost));
            shopIngredientList.Add(offer);
        }

        // Equipment
        for (int i = 0; i < numEquipmentShopOffers; i++)
        {
            EquipmentShopOffer offer = Instantiate(equipmentShopOfferPrefab, clothierWaresInfo.parentSpawnsTo);
            Equipment oferred = GetRandomEquipment();
            offer.Set(oferred, RandomHelper.RandomIntExclusive(minMaxEquipmentCostDict[oferred.GetRarity()]));
            shopEquipmentList.Add(offer);
        }
    }

    private Equipment GetRandomEquipment()
    {
        List<Equipment> allEquipment = new List<Equipment>();

        // Add all Equipment
        allEquipment.AddRange(equippableWands);
        allEquipment.AddRange(equippableRobes);
        allEquipment.AddRange(equippableHats);

        // Remove Currently Equipped Equipment
        allEquipment.Remove(playerEquippedWand);
        allEquipment.Remove(playerEquippedRobe);
        allEquipment.Remove(playerEquippedHat);

        return RandomHelper.GetRandomFromList(allEquipment);
    }

    public void ClearShop()
    {
        while (shopArtifactList.Count > 0)
        {
            ArtifactShopOffer offer = shopArtifactList[0];
            shopArtifactList.RemoveAt(0);
            Destroy(offer.gameObject);
        }

        while (shopIngredientList.Count > 0)
        {
            IngredientShopOffer offer = shopIngredientList[0];
            shopIngredientList.RemoveAt(0);
            Destroy(offer.gameObject);
        }

        while (shopEquipmentList.Count > 0)
        {
            EquipmentShopOffer offer = shopEquipmentList[0];
            shopEquipmentList.RemoveAt(0);
            Destroy(offer.gameObject);
        }

        while (shopBookList.Count > 0)
        {
            BookShopOffer offer = shopBookList[0];
            shopBookList.RemoveAt(0);
            Destroy(offer.gameObject);
        }
    }

    #region Merchant

    #endregion

    #region Innkeeper

    #endregion

    #endregion

    #region Event 

    public string[] CureAllStrings(string[] arr)
    {
        string[] res = new string[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            res[i] = arr[i];
        }
        return res;
    }

    public bool ParseEventCondition(EventLabel eventLabel, string condition)
    {
        if (condition.Length == 0) return true;
        if (condition.ToLower().Equals("false")) return false;
        if (condition.ToLower().Equals("true")) return true;

        // Debug.Log("Parsing Event Condition: " + condition);
        string[] conditionParts = CureAllStrings(condition.Split(':'));

        if (conditionParts.Length == 1)
        {
            string singleCondition = conditionParts[0];
            // Debug.Log("Single Condition: " + singleCondition);
            switch (singleCondition)
            {
                case "BookCanBeUpgraded":
                    return GetOwnedBook(0).CanLevelUp;
                default:
                    throw new UnhandledSwitchCaseException(singleCondition);
            }
        }
        else
        {
            string conditionPart = conditionParts[0];
            string argument = conditionParts[1];

            // Debug.Log("Argument Condition: " + conditionPart + ", Argument = " + argument);
            switch (conditionPart)
            {
                case "MinGoldAmount":
                    int minGoldAmount;
                    if (TryParseArgument(eventLabel, argument, out minGoldAmount))
                    {
                        return GetPlayerCurrency() >= minGoldAmount;
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to Int");
                    }
                    break;
                case "MaxGoldAmount":
                    int maxGoldAmount;
                    if (TryParseArgument(eventLabel, argument, out maxGoldAmount))
                    {
                        return GetPlayerCurrency() < maxGoldAmount;
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to Int");
                    }
                    break;
                case "MinHPAmount":
                    int minHPAmount;
                    if (TryParseArgument(eventLabel, argument, out minHPAmount))
                    {
                        return GetCurrentCharacterHP() >= minHPAmount;
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to Int");
                    }
                    break;
                case "MaxHPAmount":
                    int maxHPAmount;
                    if (TryParseArgument(eventLabel, argument, out maxHPAmount))
                    {
                        return GetCurrentCharacterHP() < maxHPAmount;
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to Int");
                    }
                    break;
                case "HasArtifact":
                    ArtifactLabel artifactLabel;
                    if (Enum.TryParse<ArtifactLabel>(argument, out artifactLabel))
                    {
                        return HasArtifact(artifactLabel);
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to ArtifactLabel");
                    }
                    break;
                case "HasBook":
                    BookLabel bookLabel;
                    if (Enum.TryParse<BookLabel>(argument, out bookLabel))
                    {
                        return HasBook(bookLabel);
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to BookLabel");
                    }
                    break;
                case "MinSpells":
                    int minSpells;
                    if (TryParseArgument(eventLabel, argument, out minSpells))
                    {
                        return spellBook.GetNumSpellsMatchingCondition(spell => true) >= minSpells;
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to Int");
                    }
                    break;
                case "MinCurses":
                    int minCurses;
                    if (TryParseArgument(eventLabel, argument, out minCurses))
                    {
                        return spellBook.GetNumSpellsMatchingCondition(spell => spell.Color == SpellColor.Curse) >= minCurses;
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to Int");
                    }
                    break;
                default:
                    throw new UnhandledSwitchCaseException(conditionPart + ", " + argument);
            }
        }
        return false;
    }

    private int GetNumPotionIngredients()
    {
        int r = 0;
        foreach (KeyValuePair<PotionIngredientType, int> kvp in potionIngredientMap)
        {
            r += kvp.Value;
        }
        return r;
    }

    private PotionIngredientType GetRandomOwnedPotionIngredient()
    {
        return RandomHelper.GetRandomFromList(potionIngredientMap.Keys.ToList());
    }

    public Potion GetRandomOwnedPotion()
    {
        if (availablePotions.Count > 0)
            return RandomHelper.GetRandomFromList(availablePotions);
        return null;
    }

    private bool TryParseArgument(EventLabel label, string s, out int v)
    {
        if (s[0] == '{' && s[s.Length - 1] == '}')
        {
            string param = s.Substring(1, s.Length - 2);
            Debug.Log(s + ", " + param);

            if (BalenceManager._Instance.EventHasValue(label, param))
            {
                v = BalenceManager._Instance.GetValue(label, param);
                // Debug.Log("Parsed Argument: " + v);
                return true;
            }
            else
            {
                // Debug.Log("Failed to Parse Argument");
                v = Utils.StandardSentinalValue;
                return false;
            }
        }
        else
        {
            if (int.TryParse(s, out v))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    #endregion

    #region Clothier

    [Header("Clothier")]
    [Header("Select Equipment Screen")]
    [SerializeField] private GameObject selectEquipmentScreen;
    [SerializeField] private SelectEquipmentButton selectEquipmentButtonPrefab;
    [SerializeField] private Transform selectEquipmentList;
    private List<SelectEquipmentButton> spawnedSelectEquipmentButtons = new List<SelectEquipmentButton>();

    [Header("Select Stat Screen")]
    [SerializeField] private GameObject selectStatScreen;
    [SerializeField] private SelectStatButton selectStatButtonPrefab;
    [SerializeField] private Transform selectStatList;
    private List<SelectStatButton> spawnedSelectStatButtons = new List<SelectStatButton>();
    private SelectStatButton pressedSelectStatButton;

    private Equipment selectedEquipment;
    private bool hasSelectedStat;
    private BaseStat selectedStat;

    private bool exitSelectEquipmentScreen;
    private bool exitSelectStatScreen;

    public void OpenSelectEquipmentScreen(string label, EquipmentSequence inSequence)
    {
        selectEquipmentScreen.SetActive(true);

        SelectEquipmentButton spawnedForHat = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForHat.Set(playerEquippedHat, () => selectedEquipment = playerEquippedHat);

        SelectEquipmentButton spawnedForRobe = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForRobe.Set(playerEquippedRobe, () => selectedEquipment = playerEquippedRobe);

        SelectEquipmentButton spawnedForWand = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForWand.Set(playerEquippedWand, () => selectedEquipment = playerEquippedWand);

        if (inSequence == EquipmentSequence.Reforge)
        {
            Debug.Log("Showing Costs: " + label);
            spawnedForHat.ShowCost(label, inSequence);
            spawnedForRobe.ShowCost(label, inSequence);
            spawnedForWand.ShowCost(label, inSequence);
        }

        spawnedSelectEquipmentButtons.Add(spawnedForHat);
        spawnedSelectEquipmentButtons.Add(spawnedForRobe);
        spawnedSelectEquipmentButtons.Add(spawnedForWand);
    }

    private void CloseSelectEquipmentScreen()
    {
        while (spawnedSelectEquipmentButtons.Count > 0)
        {
            SelectEquipmentButton b = spawnedSelectEquipmentButtons[0];
            spawnedSelectEquipmentButtons.RemoveAt(0);
            Destroy(b.gameObject);
        }

        selectEquipmentScreen.SetActive(false);
    }

    public void OpenSelectStatScreen(bool showCost, string label)
    {
        selectStatScreen.SetActive(true);

        foreach (BaseStat stat in Enum.GetValues(typeof(BaseStat)))
        {
            SelectStatButton spawned = Instantiate(selectStatButtonPrefab, selectStatList);
            spawned.Set(stat.ToString(), delegate
            {
                selectedStat = stat;
                hasSelectedStat = true;
                pressedSelectStatButton = spawned;
            });

            if (showCost)
            {
                spawned.ShowCost(selectedEquipment, label);
            }

            spawnedSelectStatButtons.Add(spawned);
        }
    }

    private void CloseSelectStatScreen()
    {
        while (spawnedSelectStatButtons.Count > 0)
        {
            SelectButton b = spawnedSelectStatButtons[0];
            spawnedSelectStatButtons.RemoveAt(0);
            Destroy(b.gameObject);
        }

        selectStatScreen.SetActive(false);
    }

    public void ReforgeEquipmentSelected()
    {
        StartCoroutine(ReforgeEquipmentSequence());
    }

    private IEnumerator ReforgeEquipmentSequence()
    {
        OpenSelectEquipmentScreen("Reforge", EquipmentSequence.Reforge);

        while (!exitSelectEquipmentScreen)
        {
            yield return new WaitUntil(() => selectedEquipment != null || exitSelectEquipmentScreen);

            if (exitSelectEquipmentScreen)
            {
                selectedEquipment = null;
                break;
            }

            // No Moneys
            int cost = selectedEquipment.GetCostToReforge();
            if (currentPlayerClothierCurrency < cost)
            {
                selectedEquipment = null;
                continue;
            }

            // Use Currency
            AlterPelts(-cost);

            // Reforge
            selectedEquipment.Reforge();

            // Reset
            selectedEquipment = null;
        }

        exitSelectEquipmentScreen = false;

        CloseSelectEquipmentScreen();
    }

    public void StrengthenEquipmentSelected()
    {
        StartCoroutine(StrengthenEquipmentSequence());
    }

    private IEnumerator StrengthenEquipmentSequence()
    {
        OpenSelectEquipmentScreen("", EquipmentSequence.Strengthen);

        while (!exitSelectEquipmentScreen)
        {
            yield return new WaitUntil(() => selectedEquipment != null || exitSelectEquipmentScreen);

            if (exitSelectEquipmentScreen)
            {
                selectedEquipment = null;
                break;
            }

            OpenSelectStatScreen(true, "Strengthen");

            while (!exitSelectStatScreen)
            {
                yield return new WaitUntil(() => hasSelectedStat || exitSelectStatScreen);

                if (exitSelectStatScreen)
                {
                    selectedEquipment = null;
                    hasSelectedStat = false;
                    CloseSelectStatScreen();
                    break;
                }

                // No Moneys
                int cost = selectedEquipment.GetCostToStrengthen();
                if (currentPlayerClothierCurrency < cost)
                {
                    hasSelectedStat = false;
                    continue;
                }

                // Use Currency
                AlterPelts(-cost);

                // Reforge
                selectedEquipment.Strengthen(selectedStat, 1);

                // Reset
                hasSelectedStat = false;

                // Reset
                pressedSelectStatButton.ReupToolTip();
                pressedSelectStatButton = null;
            }

            selectedEquipment = null;
            exitSelectStatScreen = false;
        }

        exitSelectEquipmentScreen = false;

        CloseSelectEquipmentScreen();
    }

    public void ExitSelectEquipmentScreen()
    {
        exitSelectEquipmentScreen = true;
    }

    public void ExitSelectStatScreen()
    {
        exitSelectStatScreen = true;
    }

    #endregion
}
