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

public enum OverlaidUIType
{
    SelectSpellScreen,
    PotionIngredientScreen,
    SpellbookScreen,
}

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
    [SerializeField] private PercentageMap<Rarity> bookRarityOdds = new PercentageMap<Rarity>();
    [SerializeField] private List<BookLabel> unawardableBooks = new List<BookLabel>();
    [SerializeField] private BookDisplay bookDisplay;
    private Book equippedBook;
    private List<BookLabel> awardableBooks = new List<BookLabel>();

    [Header("Spells")]
    [SerializeField] private List<SpellLabel> unviableSpellRewards;
    [SerializeField] private PercentageMap<Rarity> spellRarityOdds = new PercentageMap<Rarity>();
    private List<SpellLabel> viableSpellRewards = new List<SpellLabel>();
    private int equippableSpellIndex;
    public Pile<Spell> Spellbook { get; private set; }
    public int NumSpells => Spellbook.Count;

    [SerializeField] private VisualSpellDisplay visualSpellDisplayPrefab;

    [Header("Spell Book Screen")]
    [SerializeField] private Transform spellBookSpawnSpellDisplaysOn;
    private List<VisualSpellDisplay> spellBookSpawnedSpellDisplays = new List<VisualSpellDisplay>();

    [Header("Select Spell Screen")]
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

    [Header("Prefabs")]
    [SerializeField] private ArtifactDisplay artifactDisplay;
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
    private float timer;

    [Header("Delays")]
    [SerializeField] private float delayOnReachNode = 0.5f;
    private int combatPileSize = 10;

    // Callbacks
    public Action OnEnterNewRoom;
    public Action OnPlayerRecieveDamage;
    private Dictionary<MapNodeType, Action> OnEnterSpecificRoomActionMap = new Dictionary<MapNodeType, Action>();

    public bool OverlaidUIOpen => activeOverlaidUIs.Count > 0;
    [SerializeField] private SerializableDictionary<OverlaidUIType, GameObject> overlaidUIDict = new SerializableDictionary<OverlaidUIType, GameObject>();
    private List<OverlaidUIType> activeOverlaidUIs = new List<OverlaidUIType>();

    public void OpenOverlayUI(OverlaidUIType type)
    {
        if (activeOverlaidUIs.Count > 0)
        {
            overlaidUIDict[activeOverlaidUIs[activeOverlaidUIs.Count - 1]].SetActive(false);
        }

        if (activeOverlaidUIs.Contains(type))
        {
            activeOverlaidUIs.Remove(type);
        }

        overlaidUIDict[type].SetActive(true);
        activeOverlaidUIs.Add(type);
    }

    public void CloseOverlayUI(OverlaidUIType type)
    {
        overlaidUIDict[type].SetActive(false);
        activeOverlaidUIs.Remove(type);

        if (activeOverlaidUIs.Count > 0)
        {
            overlaidUIDict[activeOverlaidUIs[activeOverlaidUIs.Count - 1]].SetActive(true);
        }
    }

    public int DamageFromEquipment { get; set; }
    public int DefenseFromEquipment { get; set; }
    private int manaFromEquipment;

    public bool GameOvered => GetCurrentCharacterHP() <= 0;
    public bool CanSetCurrentGameOccurance { get; set; }

    // Spell Reward Offers
    private bool spellRewardsMustMatchCharacterColor = true;
    public Func<Spell, bool> AcceptSpellRewardFunc =>
        spell => (!spellRewardsMustMatchCharacterColor || (spellRewardsMustMatchCharacterColor && spell.Color == GetCharacterColor()))
            && (spell.Rarity == spellRarityOdds.GetOption());

    private List<PotionIngredientListEntry> spawnedPotionIngredientListEntries = new List<PotionIngredientListEntry>();

    [SerializeField] private CanvasGroup upgradeBookButtonCV;

    [SerializeField] private GameObject showPileScreen;

    [Header("Spellbook Change Sequences")]
    [SerializeField] private float spellbookChangeFadeOutRate = 1;
    [SerializeField] private float spellbookChangeFadeInRate = 1;
    [SerializeField] private float spellbookChangeScaleRate = 1;
    [SerializeField] private float spellbookChangeDelayBeforeFadingOut;
    [SerializeField] private Vector2 addSpellDisplaySizeDelta = new Vector2(300, 400);
    [SerializeField] private Transform spellBookChangeDisplayContainer;

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
        timer = 0;
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

    private bool HasBook(BookLabel label)
    {
        return equippedBook.GetLabel() == label;
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
            if (unviableSpellRewards.Contains(label)) continue;
            viableSpellRewards.Add(label);
        }

        SetNewPotion();

        EquipCharacterLoadout(playerCharacter);

        CanSetCurrentGameOccurance = true;
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

        spellbookSizeText.text = Spellbook.Count.ToString();

        removeSpellCostText.text = costToRemoveSpell.ToString();
        upgradeSpellCostText.text = costToUpgradeSpell.ToString();
        UpgradeSpellButton.interactable = canUpgradeSpell;

        // Set Timer Text
        timer += Time.unscaledDeltaTime;
        int hours = TimeSpan.FromSeconds(timer).Hours;
        int minutes = TimeSpan.FromSeconds(timer).Minutes;
        int seconds = TimeSpan.FromSeconds(timer).Seconds;
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
                    EquipBook(Book.GetBookOfType(allBook[bookIndex]));
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
    public void AlterBookCharge(int alterBy)
    {
        // Alter Book Charge by
        equippedBook.AlterCharge(alterBy);
    }

    [ContextMenu("Fill All Book Charges")]
    public void FillBookCharge()
    {
        equippedBook.AlterCharge(equippedBook.MaxCharge);
    }

    public Book GetEquippedBook()
    {
        return equippedBook;
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

    #endregion

    #region Spells

    public Spell GetRandomOwnedSpell()
    {
        return RandomHelper.GetRandomFromList(Spellbook.GetSpells());
    }

    public Spell GetRandomSpell(bool removeFromPool = false)
    {
        return GetRandomSpellWithConditions(AcceptSpellRewardFunc, removeFromPool);
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

    public void AddSpellToSpellBook(SpellLabel spellLabel)
    {
        AddSpellToSpellBook(Spell.GetSpellOfType(spellLabel));
    }

    public void AddSpellToSpellBook(Spell spell)
    {
        Spellbook.Add(spell);
        StartCoroutine(ShowAddSpellSequence(spell));
    }

    public void RemoveSpellFromSpellBook(Spell spell)
    {
        Spellbook.Remove(spell);
        StartCoroutine(ShowRemoveSpellSequence(spell));
    }
    #endregion

    private void EquipCharacterLoadout(Character c)
    {
        // Spawn default num spell slots
        combatPileSize = c.GetStartingCombatPileSize();
        CombatManager._Instance.SetHandSize(c.GetStartingHandSize());

        // Equip starting spells
        Spellbook = new Pile<Spell>();
        foreach (SpellLabel label in c.GetStartingSpells())
        {
            Spellbook.Add(Spell.GetSpellOfType(label));
        }

        // Equip starting artifacts
        foreach (ArtifactLabel label in c.GetStartingArtifacts())
        {
            AddArtifact(label);
        }

        // Equip character equipment
        EquipEquipment(c.GetStartingRobe());
        EquipEquipment(c.GetStartingHat());
        EquipEquipment(c.GetStartingWand());
        EquipBook(Book.GetBookOfType(c.GetStartingBook()));

        // Remove Owned Book from Awardable Books
        awardableBooks.Remove(equippedBook.GetLabel());

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
        e.PrepEquipment();
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

    public void EquipBook(Book book)
    {
        equippedBook = book;
        bookDisplay.SetItem(book);
        bookDisplay.name = "Book(" + book.GetLabel() + ")";
    }

    [ContextMenu("Upgrade Books")]
    public void UpgradeBook()
    {
        equippedBook.TryCallLevelUp(true);
    }

    public void SwapBooks(Book newBook)
    {
        EquipBook(newBook);
        AnimateBook();
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

    public void AnimateBook()
    {
        bookDisplay.AnimateScale();
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
        if (overlaidUIDict[OverlaidUIType.SpellbookScreen].activeInHierarchy)
        {
            CloseOverlayUI(OverlaidUIType.SpellbookScreen);

            AudioManager._Instance.PlayFromSFXDict("UI_SpellbookClose");

            // Closing
            while (spellBookSpawnedSpellDisplays.Count > 0)
            {
                VisualSpellDisplay cur = spellBookSpawnedSpellDisplays[0];
                spellBookSpawnedSpellDisplays.RemoveAt(0);
                Destroy(cur.gameObject);
            }
        }
        else // Opening 
        {
            OpenOverlayUI(OverlaidUIType.SpellbookScreen);

            AudioManager._Instance.PlayFromSFXDict("UI_SpellbookOpen");

            foreach (Spell spell in Spellbook.GetSpells())
            {
                VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, spellBookSpawnSpellDisplaysOn);
                spawned.SetSpell(spell);
                spellBookSpawnedSpellDisplays.Add(spawned);
            }
        }
    }

    public void RemoveRandomSpellOfColor(SpellColor ofColor)
    {
        RemoveRandomSpellsMatchingConditions(spell => spell.Color == ofColor);
    }

    public void RemoveRandomSpellsMatchingConditions(Func<Spell, bool> removeCondition)
    {
        // Make list of all spells matching condition
        List<Spell> toRemove = new List<Spell>();
        foreach (Spell entry in Spellbook.GetSpells())
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
        foreach (Spell entry in Spellbook.GetSpells())
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

    public void TavernRemoveSpellSequence()
    {
        if (costToRemoveSpell > GetPlayerCurrency())
        {
            return;
        }

        StartCoroutine(RemoveSpellSequence(spell => true, 1, delegate
        {
            if (costToRemoveSpell > GetPlayerCurrency())
            {
                return;
            }

            AlterGold(-costToRemoveSpell);
            costToRemoveSpell += increaseCostToRemoveSpellOnPurchase;
        }, true));
    }

    public void TavernUpgradeSpellSequence()
    {
        if (costToUpgradeSpell > GetPlayerCurrency())
        {
            return;
        }
        // 
        canUpgradeSpell = false;
    }

    public IEnumerator RemoveSpellSequence(Func<Spell, bool> viableSpellFunc, int numSpells = 1, Action onCompleteAction = null, bool allowInterrupt = false)
    {
        yield return StartCoroutine(SelectSpellSequence(spell => RemoveSpellFromSpellBook(spell), numSpells, viableSpellFunc, onCompleteAction, allowInterrupt));
    }

    public IEnumerator TransformSpellSequence(Func<Spell, bool> viableSpellFunc, int numSpells = 1, Action onCompleteAction = null)
    {
        yield return StartCoroutine(SelectSpellSequence(spell =>
        {
            RemoveSpellFromSpellBook(spell);
            AddSpellToSpellBook(GetRandomSpellWithConditions(newSpell => AcceptSpellRewardFunc(newSpell)));
        }, numSpells, viableSpellFunc, onCompleteAction));
    }

    public IEnumerator DuplicateSpellSequence(Func<Spell, bool> viableSpellFunc, int numSpells = 1, Action onCompleteAction = null)
    {
        yield return StartCoroutine(SelectSpellSequence(spell =>
        {
            AddSpellToSpellBook(Spell.GetSpellOfType(spell.Label));
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

        OpenOverlayUI(OverlaidUIType.SelectSpellScreen);

        List<VisualSpellDisplay> spawnedDisplays = new List<VisualSpellDisplay>();

        List<Spell> selectedSpells = new List<Spell>();

        // Opening 
        foreach (Spell spell in Spellbook.GetSpells())
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

        CloseOverlayUI(OverlaidUIType.SelectSpellScreen);

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
        if (amount > 0)
        {
            AudioManager._Instance.PlayFromSFXDict("Gold_GainGold");
        }
        else if (amount < 0)
        {
            AudioManager._Instance.PlayFromSFXDict("Gold_LoseGold");
        }

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

    public void AlterPlayerCurrentMana(int amount, bool canExceedCap = false)
    {
        if (!canExceedCap && currentPlayerMana + amount > maxPlayerMana)
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


    #region UI

    public bool CanUpgradeActiveBook => GetEquippedBook().CanLevelUp;


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
        endGameFinalScoreText.text = "Score: " + finalScore.ToString();
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

    public void TogglePotionIngredientListScreen()
    {
        if (overlaidUIDict[OverlaidUIType.PotionIngredientScreen].activeInHierarchy)
        {
            CloseOverlayUI(OverlaidUIType.PotionIngredientScreen);

            AudioManager._Instance.PlayFromSFXDict("UI_PotionIngredientsClose");

            // Closing
            while (spawnedPotionIngredientListEntries.Count > 0)
            {
                PotionIngredientListEntry cur = spawnedPotionIngredientListEntries[0];
                spawnedPotionIngredientListEntries.RemoveAt(0);
                Destroy(cur.gameObject);
            }
        }
        else // Opening 
        {
            OpenOverlayUI(OverlaidUIType.PotionIngredientScreen);

            AudioManager._Instance.PlayFromSFXDict("UI_PotionIngredientsOpen");

            foreach (KeyValuePair<PotionIngredientType, int> kvp in potionIngredientMap)
            {
                PotionIngredientListEntry spawned = Instantiate(potionIngredientListEntryPrefab, potionIngredientListParent);
                spawned.Set(kvp.Key, kvp.Value, false);
                spawnedPotionIngredientListEntries.Add(spawned);
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
    // [SerializeField] private GameObject potionIngredientListScreen;
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
        AudioManager._Instance.PlayFromSFXDict("Campfire_Rest");

        float value = BalenceManager._Instance.GetValue(MapNodeType.Campfire, "HealPercent");
        float percentHP = value / 100;
        AlterPlayerCurrentHP(Mathf.CeilToInt(maxPlayerHP * percentHP), DamageType.Heal);
    }

    public void StudyAtCampfire()
    {
        AudioManager._Instance.PlayFromSFXDict("Campfire_Study");

        UpgradeBook();
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

            AudioManager._Instance.PlayFromSFXDict("Campfire_AddToBrew");

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
    [SerializeField] private TavernItemInfo artifactOffers;
    [SerializeField] private TavernItemInfo bookOffers;
    [SerializeField] private TavernItemInfo spellOffers;
    [SerializeField] private TavernItemInfo ingredientOffers;

    private List<ShopOffer> spawnedShopOffers = new List<ShopOffer>();

    [SerializeField] private SerializableDictionary<Rarity, Vector2> minMaxArtifactCostDict = new SerializableDictionary<Rarity, Vector2>();
    [SerializeField] private SerializableDictionary<Rarity, Vector2> minMaxBookCostDict = new SerializableDictionary<Rarity, Vector2>();
    [SerializeField] private SerializableDictionary<Rarity, Vector2> minMaxSpellCostDict = new SerializableDictionary<Rarity, Vector2>();
    [SerializeField] private Vector2 minMaxIngredientCost;
    [SerializeField] private int costToRemoveSpell = 75;
    [SerializeField] private int increaseCostToRemoveSpellOnPurchase;
    [SerializeField] private int costToUpgradeSpell = 50;
    private bool canUpgradeSpell = true;
    [SerializeField] private Button UpgradeSpellButton;
    [SerializeField] private TextMeshProUGUI removeSpellCostText;
    [SerializeField] private TextMeshProUGUI upgradeSpellCostText;

    [System.Serializable]
    public class TavernItemInfo
    {
        public int NumOffers;
        public Transform SpawnOffersOn;
        public ShopOffer Prefab;
    }

    public void MultiplyCosts(float multBy)
    {
        foreach (ShopOffer offer in spawnedShopOffers)
        {
            offer.MultiplyCost(multBy);
        }
    }

    public void LoadShop()
    {
        // Artifacts
        for (int i = 0; i < artifactOffers.NumOffers; i++)
        {
            // Ran out of artifacts
            if (awardableArtifacts.Count == 0)
            {
                break;
            }

            ArtifactShopOffer offer = Instantiate((ArtifactShopOffer)artifactOffers.Prefab, artifactOffers.SpawnOffersOn);

            // Get artifact that will be offered
            Artifact offered = Artifact.GetArtifactOfType(GetRandomArtifact());

            // Determine cost
            int cost = Mathf.RoundToInt(RandomHelper.RandomFloat(minMaxArtifactCostDict[offered.Rarity]));
            offer.Set(offered, cost);
            spawnedShopOffers.Add(offer);
        }

        // Books
        for (int i = 0; i < bookOffers.NumOffers; i++)
        {
            // Ran out of books
            if (awardableBooks.Count == 0)
            {
                break;
            }

            BookShopOffer offer = Instantiate((BookShopOffer)bookOffers.Prefab, bookOffers.SpawnOffersOn);

            // Get book that will be offered
            Book offered = Book.GetBookOfType(GetRandomBook());

            // Determine cost
            int cost = Mathf.RoundToInt(RandomHelper.RandomFloat(minMaxBookCostDict[offered.Rarity]));
            offer.Set(offered, cost);
            spawnedShopOffers.Add(offer);
        }

        // Spells
        List<Spell> chosenSpells = new List<Spell>();
        for (int i = 0; i < spellOffers.NumOffers; i++)
        {
            SpellShopOffer offer = Instantiate((SpellShopOffer)spellOffers.Prefab, spellOffers.SpawnOffersOn);
            Spell offered = GetRandomSpellWithConditions(spell => AcceptSpellRewardFunc(spell)
                && !Spell.SpellListContainSpell(chosenSpells, spell.Label));
            chosenSpells.Add(offered);
            int cost = Mathf.RoundToInt(RandomHelper.RandomFloat(minMaxSpellCostDict[offered.Rarity]));
            offer.Set(offered, cost);
            spawnedShopOffers.Add(offer);
        }

        // Ingredients
        for (int i = 0; i < ingredientOffers.NumOffers; i++)
        {
            IngredientShopOffer offer = Instantiate((IngredientShopOffer)ingredientOffers.Prefab, ingredientOffers.SpawnOffersOn);
            PotionIngredient offered = PotionIngredient.GetPotionIngredientOfType(GetRandomPotionIngredient());
            int cost = RandomHelper.RandomIntExclusive(minMaxIngredientCost);
            offer.Set(offered, cost);
            spawnedShopOffers.Add(offer);
        }
    }

    public void ClearShop()
    {
        while (spawnedShopOffers.Count > 0)
        {
            ShopOffer o = spawnedShopOffers[0];
            Destroy(o.gameObject);
            spawnedShopOffers.RemoveAt(0);
        }
        canUpgradeSpell = true;
    }

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
                    return GetEquippedBook().CanLevelUp;
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
                        return Spellbook.GetNumEntriesMatching(spell => true) >= minSpells;
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
                        return Spellbook.GetNumEntriesMatching(spell => spell.Color == SpellColor.Curse) >= minCurses;
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

    public void OpenSelectEquipmentScreen()
    {
        selectEquipmentScreen.SetActive(true);

        SelectEquipmentButton spawnedForHat = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForHat.Set(playerEquippedHat, () => selectedEquipment = playerEquippedHat);

        SelectEquipmentButton spawnedForRobe = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForRobe.Set(playerEquippedRobe, () => selectedEquipment = playerEquippedRobe);

        SelectEquipmentButton spawnedForWand = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForWand.Set(playerEquippedWand, () => selectedEquipment = playerEquippedWand);

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

    public void StrengthenEquipmentSelected()
    {
        StartCoroutine(StrengthenEquipmentSequence());
    }

    private IEnumerator StrengthenEquipmentSequence()
    {
        OpenSelectEquipmentScreen();

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

                AudioManager._Instance.PlayFromSFXDict("Campfire_StrengthenEquipment");

                // Use Currency
                AlterPelts(-cost);

                // Strengthen
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
}
