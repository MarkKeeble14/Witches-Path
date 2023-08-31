using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Febucci.UI;

public enum ContentType
{
    Artifact,
    Book,
    ActiveSpell,
    PassiveSpell,
    Spell
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
    [SerializeField] private SerializableDictionary<ArtifactLabel, Rarity> artifactRarityMap = new SerializableDictionary<ArtifactLabel, Rarity>();

    [SerializeField] private PercentageMap<Rarity> artifactRarityOdds = new PercentageMap<Rarity>();
    private Dictionary<ArtifactLabel, ArtifactDisplay> artifactDisplayTracker = new Dictionary<ArtifactLabel, ArtifactDisplay>();
    private Dictionary<ArtifactLabel, Artifact> equippedArtifacts = new Dictionary<ArtifactLabel, Artifact>();
    private List<ArtifactLabel> allArtifacts;
    private int artifactIndex;
    public int NumArtifacts => equippedArtifacts.Count;

    [Header("Books")]
    [SerializeField] private Transform bookBar;
    [SerializeField] private SerializableDictionary<BookLabel, Rarity> bookRarityMap = new SerializableDictionary<BookLabel, Rarity>();
    [SerializeField] private PercentageMap<Rarity> bookRarityOdds = new PercentageMap<Rarity>();
    private Dictionary<BookLabel, BookDisplay> bookDisplayTracker = new Dictionary<BookLabel, BookDisplay>();
    private Dictionary<BookLabel, Book> equippedBooks = new Dictionary<BookLabel, Book>();
    private List<BookLabel> allBooks;
    private int bookIndex;

    [Header("Spells")]
    [SerializeField] private PercentageMap<Rarity> spellRarityOdds = new PercentageMap<Rarity>();
    [SerializeField] private List<SpellLabel> playerEquippableActiveSpells;
    [SerializeField] private List<SpellLabel> playerEquippablePassiveSpells;
    private int equippableSpellIndex;
    private List<Spell> equippedSpells = new List<Spell>();
    private Spellbook spellBook;
    public int NumSpells => spellBook.GetSpellBookEntries().Count;

    [Header("Active Spells")]
    [SerializeField] private KeyCode[] activeSpellBindings;
    [SerializeField] private ActiveSpellDisplay activeSpellDisplayPrefab;
    [SerializeField] private Transform activeSpellDisplaysList;
    private Dictionary<ActiveSpellDisplay, ActiveSpell> equippedActiveSpells = new Dictionary<ActiveSpellDisplay, ActiveSpell>();
    private List<ActiveSpellDisplay> activeSpellDisplays = new List<ActiveSpellDisplay>();
    private int numActiveSpellSlots;

    [Header("Passive Spells")]
    [SerializeField] private PassiveSpellDisplay passiveSpellDisplayPrefab;
    [SerializeField] private Transform passiveSpellDisplaysList;
    private Dictionary<PassiveSpellDisplay, PassiveSpell> equippedPassiveSpells = new Dictionary<PassiveSpellDisplay, PassiveSpell>();
    private List<PassiveSpellDisplay> passiveSpellDisplays = new List<PassiveSpellDisplay>();
    private int numPassiveSpellSlots;

    [Header("Select Spells For Combat Screen")]
    [SerializeField] private VisualSpellDisplay visualSpellDisplayPrefab;
    [SerializeField] private GameObject selectSpellsForCombatScreen;
    [SerializeField] private Transform parentSelectSpellOptionsTo;
    [SerializeField] private Button confirmSelectSpellsForCombatButton;
    [SerializeField] private TextMeshProUGUI confirmSelectSpellsForCombatText;
    [SerializeField] private TypewriterByWord selectSpellsForCombatScreenTitleTypewriter;
    private bool spellSelectionForCombatConfirmed;

    [Header("Spell Book Screen")]
    [SerializeField] private GameObject spellBookScreen;
    [SerializeField] private Transform spellBookSpawnSpellDisplaysOn;
    private List<VisualSpellDisplay> spellBookSpawnedSpellDisplays = new List<VisualSpellDisplay>();

    [Header("Select Spell Screen")]
    [SerializeField] private GameObject selectSpellScreen;
    [SerializeField] private Transform spawnSelectSpellDisplaysOn;

    [Header("Potions")]
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

    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI clothierCurrencyText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Delays")]
    [SerializeField] private float delayOnReachNode = 0.5f;

    // Callbacks
    public Action OnEnterNewRoom;
    public Action OnPlayerRecieveDamage;
    private Dictionary<MapNodeType, Action> OnEnterSpecificRoomActionMap = new Dictionary<MapNodeType, Action>();

    public int DamageFromEquipment { get; set; }
    public int DefenseFromEquipment { get; set; }
    private int manaFromEquipment;

    public bool GameOvered => GetCurrentCharacterHP() <= 0;
    public bool CanSetCurrentGameOccurance { get; private set; }

    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        CallOnGameStart();

        for (int i = 0; i < numRandomIngredientsOnStart; i++)
        {
            AddRandomPotionIngredient();
        }
    }

    public void CallOnGameStart()
    {
        // Get List of all Book Labels
        allBooks = new List<BookLabel>((BookLabel[])Enum.GetValues(typeof(BookLabel)));

        // Get List of all Artifact Labels
        allArtifacts = new List<ArtifactLabel>((ArtifactLabel[])Enum.GetValues(typeof(ArtifactLabel)));

        TryAddPersistentTokens();

        foreach (MapNodeType type in Enum.GetValues(typeof(MapNodeType)))
        {
            OnEnterSpecificRoomActionMap.Add(type, null);
        }

        // Set equipment Tool Tippables
        SetEquipmentToolTippables(equippableHats.Cast<Equipment>().ToList());
        SetEquipmentToolTippables(equippableRobes.Cast<Equipment>().ToList());
        SetEquipmentToolTippables(equippableWands.Cast<Equipment>().ToList());

        SetNewPotion();

        EquipCharacterLoadout(playerCharacter);

        CanSetCurrentGameOccurance = true;

        OnEnterNewRoom += () => spellBook.TickOutOfCombatCooldowns();
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
        int damageAmount = GetBasicAttackDamage() + DamageFromEquipment + CombatManager._Instance.GetPowerBonus(Target.Character);
        // if the amount of damage a players basic attack will do is less than zero, consider it zero. This is accounted for in the CombatManager script manually as well
        if (damageAmount >= 0)
        {
            damageText.text = damageAmount.ToString();
        }
        else
        {
            defenseText.text = 0.ToString();
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

                    if (equippableSpellIndex > playerEquippableActiveSpells.Count - 1)
                        equippableSpellIndex = 0;

                    Debug.Log("Selected: " + playerEquippableActiveSpells[equippableSpellIndex]);
                }

                // Equip new spell
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    equippableSpellIndex--;

                    if (equippableSpellIndex < 0)
                        equippableSpellIndex = playerEquippableActiveSpells.Count - 1;

                    Debug.Log("Selected: " + playerEquippableActiveSpells[equippableSpellIndex]);
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    // 
                    AddSpellToSpellBook(playerEquippableActiveSpells[equippableSpellIndex]);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
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

                    if (artifactIndex > allArtifacts.Count - 1)
                        artifactIndex = 0;

                    Debug.Log("Selected: " + allArtifacts[artifactIndex]);
                }

                // Equip new artifact
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    artifactIndex--;

                    if (artifactIndex < 0)
                        artifactIndex = allArtifacts.Count - 1;

                    Debug.Log("Selected: " + allArtifacts[artifactIndex]);
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    AddArtifact(allArtifacts[artifactIndex]);
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    RemoveArtifact(allArtifacts[artifactIndex]);
                }

                break;
            case ContentType.Book:

                // Passive Spells
                // Equip new Book
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    bookIndex++;

                    if (bookIndex > allBooks.Count - 1)
                        bookIndex = 0;

                    Debug.Log("Selected: " + allBooks[bookIndex]);
                }

                // Equip new book
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    bookIndex--;

                    if (bookIndex < 0)
                        bookIndex = allBooks.Count - 1;

                    Debug.Log("Selected: " + allBooks[bookIndex]);
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SwapBooks(bookDisplayTracker.Keys.First(), allBooks[bookIndex]);
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    RemoveBook(allBooks[bookIndex]);
                }

                break;
        }
    }

    public void TriggerRandomPassiveSpell()
    {
        if (CombatManager._Instance.InCombat)
        {
            if (equippedPassiveSpells.Count > 0)
            {
                PassiveSpell spell = RandomHelper.GetRandomFromArray(equippedPassiveSpells.Values.ToArray());
                spell.Proc(true);
                Debug.Log("Triggered: " + spell);
            }
        }
    }

    public bool EquipSpell(Spell spell)
    {
        bool didEquip;
        switch (spell)
        {
            case ActiveSpell activeSpell:
                didEquip = EquipActiveSpell(activeSpell);
                break;
            case PassiveSpell passiveSpell:
                didEquip = EquipPassiveSpell(passiveSpell);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }

        if (didEquip)
        {
            equippedSpells.Add(spell);
            spell.OnEquip();
        }

        return didEquip;
    }

    public void UnequipSpell(Spell spell)
    {
        equippedSpells.Remove(spell);
        spell.OnUnequip();
        switch (spell)
        {
            case ActiveSpell activeSpell:
                UnequipActiveSpell(activeSpell);
                break;
            case PassiveSpell passiveSpell:
                UnequipPassiveSpell(passiveSpell);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public PassiveSpellDisplay SpawnPassiveSpellDisplay()
    {
        PassiveSpellDisplay spawned = Instantiate(passiveSpellDisplayPrefab, passiveSpellDisplaysList);
        passiveSpellDisplays.Add(spawned);
        spawned.SetEmpty(true);
        return spawned;
    }

    public ActiveSpellDisplay SpawnActiveSpellDisplay()
    {
        ActiveSpellDisplay spawned = Instantiate(activeSpellDisplayPrefab, activeSpellDisplaysList);
        KeyCode keyBinding = activeSpellBindings[activeSpellDisplays.Count + equippedActiveSpells.Count];
        activeSpellDisplays.Add(spawned);
        spawned.SetKeyBinding(keyBinding);
        spawned.SetEmpty(true);
        return spawned;
    }

    private SpellDisplay GetFirstEmptySpellDisplay(List<SpellDisplay> spellDisplays)
    {
        for (int i = 0; i < spellDisplays.Count; i++)
        {
            if (spellDisplays[i].IsEmpty)
            {
                return spellDisplays[i];
            }
        }
        return null;
    }

    public bool EquipPassiveSpell(PassiveSpell spell)
    {
        // grab a display to use
        PassiveSpellDisplay spellDisplay = (PassiveSpellDisplay)GetFirstEmptySpellDisplay(passiveSpellDisplays.Cast<SpellDisplay>().ToList());

        // if there isn't any unused passive spell slots, then we can't equip this spell (something has probably gone wrong?)
        if (spellDisplay == null) return false;

        spellDisplay.SetSpell(spell);

        equippedPassiveSpells.Add(spellDisplay, spell);

        return true;
    }

    public PassiveSpellDisplay UnequipPassiveSpell(Spell spell)
    {
        PassiveSpellDisplay loaded = (PassiveSpellDisplay)spell.GetEquippedTo();

        // Manage lists
        loaded.Unset();
        equippedPassiveSpells.Remove(loaded);
        passiveSpellDisplays.Add(loaded);

        return loaded;
    }

    public bool EquipActiveSpell(ActiveSpell spell)
    {
        // grab a display to use
        ActiveSpellDisplay spellDisplay = (ActiveSpellDisplay)GetFirstEmptySpellDisplay(activeSpellDisplays.Cast<SpellDisplay>().ToList());

        // if there isn't any unused active spell slots, then we can't equip this spell (something has probably gone wrong?)
        if (spellDisplay == null) return false;

        spellDisplay.SetSpell(spell);

        // Track Lists
        equippedActiveSpells.Add(spellDisplay, spell);

        // Debug.Log("Equipped: " + newSpell);
        return true;
    }

    public ActiveSpellDisplay UnequipActiveSpell(Spell spell)
    {
        ActiveSpellDisplay loaded = (ActiveSpellDisplay)spell.GetEquippedTo();

        // Manage lists
        loaded.Unset();
        activeSpellDisplays.Add(loaded);
        equippedActiveSpells.Remove(loaded);

        return loaded;
    }

    public void ReduceActiveSpellCooldowns(int reduceBy)
    {
        foreach (KeyValuePair<ActiveSpellDisplay, ActiveSpell> kvp in equippedActiveSpells)
        {
            if (kvp.Value.OnCooldown)
            {
                kvp.Value.AlterCooldown(-reduceBy);
            }
        }
    }

    public void ResetActiveSpellCooldowns()
    {
        foreach (KeyValuePair<ActiveSpellDisplay, ActiveSpell> kvp in equippedActiveSpells)
        {
            if (kvp.Value.OnCooldown)
            {
                kvp.Value.ResetCooldown();
            }
        }
    }

    public int GetBasicAttackDamage()
    {
        return playerCharacter.GetBasicAttackDamage();
    }

    #region Artifacts
    public ArtifactLabel GetRandomOwnedArtifact()
    {
        return RandomHelper.GetRandomFromList(equippedArtifacts.Keys.ToList());
    }

    public ArtifactLabel GetRandomArtifact(bool removeFromPool = true)
    {
        ArtifactLabel artifact = GetRandomArtifactOfRarity(artifactRarityOdds.GetOption());
        if (removeFromPool)
        {
            allArtifacts.Remove(artifact);
        }
        return artifact;
    }

    public ArtifactLabel GetRandomArtifactOfRarity(Rarity r)
    {
        List<ArtifactLabel> options = new List<ArtifactLabel>();
        foreach (ArtifactLabel l in allArtifacts)
        {
            if (artifactRarityMap[l] == r)
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
            allBooks.Remove(book);
        }
        return book;
    }

    public BookLabel GetRandomBookOfRarity(Rarity r)
    {
        List<BookLabel> options = new List<BookLabel>();
        foreach (BookLabel l in allBooks)
        {
            if (bookRarityMap[l] == r)
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
        return RandomHelper.GetRandomFromList(spellBook.GetSpellBookEntries()).Spell;
    }

    public Spell GetRandomSpell(bool removeFromPool = false)
    {
        // We do not care about the Rarity
        if (RandomHelper.RandomBool()) // Active Spell
        {
            Spell spell = GetRandomSpellWithConditions(playerEquippableActiveSpells,
                spell => spell.Color == playerCharacter.GetColor() || spell.Color == SpellColor.Grey);
            if (removeFromPool)
            {
                playerEquippableActiveSpells.Remove(spell.Label);
            }
            return spell;
        }
        else // Passive Spell
        {
            Spell spell = GetRandomSpellWithConditions(playerEquippablePassiveSpells,
                spell => spell.Color == playerCharacter.GetColor() || spell.Color == SpellColor.Grey);
            if (removeFromPool)
            {
                playerEquippablePassiveSpells.Remove(spell.Label);
            }
            return spell;
        }
    }

    public Spell GetRandomSpellConsideringRarity(Rarity rarity, Func<List<SpellLabel>> getListFunc, SpellLabel defaultSpell, bool removeFromPool = false)
    {
        Spell spell = null;
        for (int i = (int)rarity; i > 0; i--)
        {
            spell = GetRandomSpellWithConditions(getListFunc(), spell => spell.Rarity == (Rarity)i
            && (spell.Color == playerCharacter.GetColor() || spell.Color == SpellColor.Grey));
            if (spell != null)
            {
                break;
            }
        }

        if (spell == null)
        {
            spell = Spell.GetSpellOfType(defaultSpell);
        }

        if (removeFromPool)
        {
            getListFunc().Remove(spell.Label);
        }
        return spell;
    }

    public Spell GetRandomSpellConsideringRarity(Rarity rarity, bool removeFromPool = false)
    {
        if (RandomHelper.RandomBool()) // Active Spell
        {
            return GetRandomSpellConsideringRarity(rarity, () => playerEquippableActiveSpells, playerCharacter.GetDefaultActiveSpell(), removeFromPool);
        }
        else // Passive Spell
        {
            return GetRandomSpellConsideringRarity(rarity, () => playerEquippablePassiveSpells, playerCharacter.GetDefaultPassiveSpell(), removeFromPool);
        }
    }

    public Spell GetRandomSpellWithConditions(List<SpellLabel> allowedOptions, Func<Spell, bool> includeConditions)
    {
        List<SpellLabel> options = new List<SpellLabel>();
        foreach (SpellLabel l in allowedOptions)
        {
            if (includeConditions(Spell.GetSpellOfType(l)))
            {
                options.Add(l);
            }
        }

        if (options.Count <= 0) return null;

        return Spell.GetSpellOfType(RandomHelper.GetRandomFromList(options));
    }

    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private EnemyInfoBlock onCombatStart;
    [SerializeField] private EnemyInfoBlock onTurnStart;
    [SerializeField] private EnemyInfoBlock combatActions;

    [System.Serializable]
    private struct EnemyInfoBlock
    {
        public GameObject Container;
        public Transform List;
    }
    [SerializeField] private EnemyInfoBlockIntentDisplay enemyIntentDisplayPrefab;
    private List<EnemyInfoBlockIntentDisplay> spawnedInfoBlockIntentDisplays = new List<EnemyInfoBlockIntentDisplay>();

    private void SetEnemyInformationText(EnemyInfoBlock infoBlock, List<EnemyAction> enemyActions)
    {
        if (enemyActions.Count <= 0)
        {
            infoBlock.Container.SetActive(false);
            return;
        }
        infoBlock.Container.SetActive(true);

        foreach (EnemyAction action in enemyActions)
        {
            List<EnemyIntent> currentIntents = action.GetEnemyIntents();
            EnemyInfoBlockIntentDisplay spawned = Instantiate(enemyIntentDisplayPrefab, infoBlock.List);
            spawned.Set(currentIntents);
            spawnedInfoBlockIntentDisplays.Add(spawned);
        }
    }

    private void ClearEnemyInfoBlockIntentDisplays()
    {
        while (spawnedInfoBlockIntentDisplays.Count > 0)
        {
            EnemyInfoBlockIntentDisplay current = spawnedInfoBlockIntentDisplays[0];
            spawnedInfoBlockIntentDisplays.RemoveAt(0);
            Destroy(current.gameObject);
        }
    }

    public IEnumerator SelectSpellsForCombat(Enemy combatEnemy)
    {
        // Set the screen to be visible
        selectSpellsForCombatScreen.SetActive(true);

        // Start Typewriter effect
        selectSpellsForCombatScreenTitleTypewriter.StartShowingText(true);

        // Set things according to the enemy so the player knows what they're fighting
        enemyNameText.text = combatEnemy.Name;
        SetEnemyInformationText(onCombatStart, combatEnemy.GetOnCombatStartActions());
        SetEnemyInformationText(onTurnStart, combatEnemy.GetOnTurnStartActions());
        SetEnemyInformationText(combatActions, combatEnemy.GetEnemyActions());

        // Get Spells
        List<Spell> selectedSpells = new List<Spell>();
        List<VisualSpellDisplay> spawnedSelections = new List<VisualSpellDisplay>();

        foreach (SpellbookEntry entry in spellBook.GetSpellBookEntries())
        {
            Spell spell = entry.Spell;

            VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, parentSelectSpellOptionsTo);
            spawnedSelections.Add(spawned);
            spawned.SetSpell(spell);
            spawned.SetAvailableState(entry.OutOfCombatCooldown);

            if (entry.OutOfCombatCooldown <= 0 && (spell.Color == SpellColor.Curse || spell.Color == SpellColor.Status))
            {
                // Curses and Statuses are Automatically Loaded and Locked
                EquipSpell(spell);
                spawned.SetSpellDisplayState(SpellDisplayState.Selected);
            }
            else
            {
                // Other Spells must be chosen
                spawned.AddOnClick(delegate
                {
                    if (selectedSpells.Contains(spell))
                    {
                        UnequipSpell(spell);
                        selectedSpells.Remove(spell);
                        spawned.SetSpellDisplayState(SpellDisplayState.Normal);
                    }
                    else
                    {
                        if (EquipSpell(spell))
                        {
                            selectedSpells.Add(spell);
                            spawned.SetSpellDisplayState(SpellDisplayState.Selected);
                        }
                    }
                });
            }
        }

        // Spawn Choices
        // To proceed, the player must click the confirm button
        // The player must equip at least one spell (provided they have any)
        while (!spellSelectionForCombatConfirmed)
        {
            // Set button text and interactable state
            // if the player has absolutely no available Spells, just allow them to proceed
            confirmSelectSpellsForCombatText.text = selectedSpells.Count + " / " + (numActiveSpellSlots + numPassiveSpellSlots);
            if (spellBook.NumSpells(SpellType.Active, SpellOutOfCombatState.Available) == 0
                && spellBook.NumSpells(SpellType.Passive, SpellOutOfCombatState.Available) == 0)
            {
                confirmSelectSpellsForCombatButton.interactable = true;
            }
            else
            {
                // Otherwise, they must have at least one Spell selected
                confirmSelectSpellsForCombatButton.interactable = selectedSpells.Count > 0;
            }

            yield return null;
        }
        spellSelectionForCombatConfirmed = false;

        // Player has made selected
        foreach (Spell spell in selectedSpells)
        {
            spellBook.SetSpellUnavailable(spell);
        }

        // Destroy spawned objects
        while (spawnedSelections.Count > 0)
        {
            VisualSpellDisplay current = spawnedSelections[0];
            spawnedSelections.RemoveAt(0);
            current.SetSpellDisplayState(SpellDisplayState.Normal);
            Destroy(current.gameObject);
        }

        ClearEnemyInfoBlockIntentDisplays();

        selectSpellsForCombatScreen.SetActive(false);
    }

    public void ConfirmSpellSelection()
    {
        spellSelectionForCombatConfirmed = true;
    }

    public void UnselectSpellsFromCombat()
    {
        while (equippedSpells.Count > 0)
        {
            Spell current = equippedSpells[0];
            equippedSpells.RemoveAt(0);
            UnequipSpell(current);
        }
    }

    public void AddSpellToSpellBook(SpellLabel spellLabel)
    {
        spellBook.AddSpell(Spell.GetSpellOfType(spellLabel));
    }

    public void AddSpellToSpellBook(Spell spell)
    {
        spellBook.AddSpell(spell);
    }

    public void RemoveSpellFromSpellBook(Spell spell)
    {
        spellBook.RemoveSpell(spell);
    }

    public void ReduceActiveSpellCDsByPercent(float normalizedPercent)
    {
        // normaliedPercent is some number between 0 and 1
        // 0 = 0%, 1 = 100%
        // .14 = 14%
        // etc
        foreach (KeyValuePair<ActiveSpellDisplay, ActiveSpell> kvp in equippedActiveSpells)
        {
            if (kvp.Value.OnCooldown)
            {
                kvp.Value.MultiplyCooldown(normalizedPercent);
            }
        }
    }

    public List<ActiveSpell> GetActiveSpells()
    {
        List<ActiveSpell> toReturn = new List<ActiveSpell>();
        foreach (KeyValuePair<ActiveSpellDisplay, ActiveSpell> kvp in equippedActiveSpells)
        {
            toReturn.Add(kvp.Value);
        }
        return toReturn;
    }

    public void AddActiveSpellSlots(int num)
    {
        for (int i = 0; i < num; i++)
        {
            SpawnActiveSpellDisplay();
        }
    }

    public void AddPassiveSpellSlots(int num)
    {
        for (int i = 0; i < num; i++)
        {
            SpawnPassiveSpellDisplay();
        }
    }

    private void RemoveSpellFromEquippableSpells(SpellLabel spellLabel)
    {
        if (playerEquippableActiveSpells.Contains(spellLabel))
        {
            playerEquippableActiveSpells.Remove(spellLabel);
        }
        else if (playerEquippablePassiveSpells.Contains(spellLabel))
        {
            playerEquippablePassiveSpells.Remove(spellLabel);
        }
    }
    #endregion


    private void EquipCharacterLoadout(Character c)
    {
        // Spawn default num spell slots
        numActiveSpellSlots = c.GetStartingActiveSpellSlots();
        AddActiveSpellSlots(numActiveSpellSlots);
        numPassiveSpellSlots = c.GetStartingPassiveSpellSlots();
        AddPassiveSpellSlots(numPassiveSpellSlots);

        // Equip starting spells
        spellBook = new Spellbook(c.GetStartingSpells());

        // Equip character equipment
        EquipEquipment(c.GetStartingRobe());
        EquipEquipment(c.GetStartingHat());
        EquipEquipment(c.GetStartingWand());
        AddBook(c.GetStartingBook());

        // Remove Owned Book from All Books
        allBooks.Remove(GetOwnedBookLabel(0));

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
        allArtifacts.Remove(label);
    }

    public void AddArtifact(ArtifactLabel type)
    {
        if (artifactDisplayTracker.ContainsKey(type))
        {
            AnimateArtifact(type);
            return;
        }

        Artifact artifact = Artifact.GetArtifactOfType(type);
        artifact.OnEquip();

        ArtifactDisplay spawned = Instantiate(artifactDisplay, artifactBar);
        spawned.SetItem(artifact);
        spawned.name = "Artifact(" + type + ")";

        artifactDisplayTracker.Add(type, spawned);
        equippedArtifacts.Add(type, artifact);
    }

    public void RemoveArtifact(ArtifactLabel type)
    {
        Artifact artifact = equippedArtifacts[type];
        artifact.OnUnequip();

        Destroy(artifactDisplayTracker[type].gameObject);
        artifactDisplayTracker.Remove(type);

        equippedArtifacts.Remove(type);
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
        Book book = equippedBooks[type];

        Destroy(bookDisplayTracker[type].gameObject);
        bookDisplayTracker.Remove(type);

        equippedBooks.Remove(type);
    }

    public void UpgradeBooks()
    {
        foreach (KeyValuePair<BookLabel, Book> b in equippedBooks)
        {
            b.Value.TryCallLevelUp(true);
        }

        ResolveCurrentEvent();
    }

    public void SwapBooks(BookLabel swappingOut, BookLabel swappingTo)
    {
        RemoveBook(swappingOut);
        AddBook(swappingTo);
        AnimateBook(swappingTo);
    }

    public void AnimateArtifact(ArtifactLabel label)
    {
        if (artifactDisplayTracker.ContainsKey(label))
        {
            artifactDisplayTracker[label].AnimateScale();
        }
    }

    public void AnimateBook(BookLabel label)
    {
        if (bookDisplayTracker.ContainsKey(label))
        {
            bookDisplayTracker[label].AnimateScale();
        }
    }

    private void PrintPotionIngredientMap()
    {
        foreach (KeyValuePair<PotionIngredientType, int> kvp in potionIngredientMap)
        {
            Debug.Log(kvp);
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
        return RandomHelper.GetRandomEnumValue<PotionIngredientType>();
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
        CanSetCurrentGameOccurance = false;
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
        currentOccurance = currentNode.GetRepresentedGameOccurance();

        // Set the new Connections to be Accessable
        setNodeTo.SetAllConnectorsState(MapNodeConnectorState.ACCESSABLE, false);
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
        MapManager._Instance.ToggleVisibility();
    }

    public void ToggleSpellBook()
    {
        spellBookScreen.SetActive(!spellBookScreen.activeInHierarchy);

        // Opening 
        if (spellBookScreen.activeInHierarchy)
        {
            foreach (SpellbookEntry entry in spellBook.GetSpellBookEntries())
            {
                Spell spell = entry.Spell;
                VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, spellBookSpawnSpellDisplaysOn);
                spawned.SetSpell(spell);
                spawned.SetAvailableState(entry.OutOfCombatCooldown);
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
                cur.SetSpellDisplayState(SpellDisplayState.Normal);
                Destroy(cur.gameObject);
            }
        }

        MapManager._Instance.ToggleVisibility();
    }

    public IEnumerator RemoveSpellSequence()
    {
        yield return StartCoroutine(SelectSpellSequence(spell => RemoveSpellFromSpellBook(spell)));
    }

    public IEnumerator SelectSpellSequence(Action<Spell> doWithSelected)
    {
        selectSpellScreen.SetActive(true);

        List<VisualSpellDisplay> spawnedDisplays = new List<VisualSpellDisplay>();
        Spell selectedSpell = null;

        // Opening 
        foreach (SpellbookEntry entry in spellBook.GetSpellBookEntries())
        {
            Spell spell = entry.Spell;
            VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, spawnSelectSpellDisplaysOn);
            spawned.SetSpell(spell);
            spawnedDisplays.Add(spawned);
            spawned.AddOnClick(() => selectedSpell = spell);
        }

        yield return new WaitUntil(() => selectedSpell != null);

        selectSpellScreen.SetActive(false);

        // Closing
        while (spawnedDisplays.Count > 0)
        {
            VisualSpellDisplay cur = spawnedDisplays[0];
            spawnedDisplays.RemoveAt(0);
            cur.SetSpellDisplayState(SpellDisplayState.Normal);
            Destroy(cur.gameObject);
        }

        doWithSelected(selectedSpell);
    }

    public bool CheckCanAfford(float amount)
    {
        return currentPlayerCurrency >= amount;
    }

    public void AlterCurrency(int amount)
    {
        // Spawn Popup Text
        PopupText spawned = Instantiate(popupTextPrefab, currencyText.transform);
        spawned.Set((amount > 0 ? "+" : "") + Utils.RoundTo(amount, 1).ToString(), Color.yellow);

        currentPlayerCurrency += amount;
    }

    public void AlterClothierCurrency(int amount)
    {
        // Spawn Popup Text
        PopupText spawned = Instantiate(popupTextPrefab, clothierCurrencyText.transform);
        spawned.Set((amount > 0 ? "+" : "") + Utils.RoundTo(amount, 1).ToString(), Color.blue);

        currentPlayerClothierCurrency += amount;
    }

    public bool AlterPlayerHP(int amount, DamageType damageType, bool spawnPopupText = true)
    {
        // Barricade Effect
        if (amount < -1 && HasArtifact(ArtifactLabel.Barricade))
        {
            amount += BalenceManager._Instance.GetValue(ArtifactLabel.Barricade, "ReductionAmount");
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
        maxPlayerHP += changeBy;
        AlterPlayerHP(changeBy, DamageType.Heal);
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

    public void AlterPlayerMana(int amount)
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

    public void SetPlayerMana(int playerMana)
    {
        currentPlayerMana = playerMana;
    }

    public void AlterManaFromEquipment(int changeBy)
    {
        manaFromEquipment += changeBy;

        maxPlayerMana = characterMaxMana + manaFromEquipment;

        AlterPlayerMana(changeBy);
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

    public bool HasArtifact(ArtifactLabel label)
    {
        return artifactDisplayTracker.ContainsKey(label);
    }

    public bool HasBook(BookLabel label)
    {
        return bookDisplayTracker.ContainsKey(label);
    }


    #region UI

    public bool CanUpgradeActiveBook => GetOwnedBook(0).CanLevelUp;

    [Header("Game Over")]
    [SerializeField] private CanvasGroup gameOverCV;
    [SerializeField] private CanvasGroup gameWonCV;
    [SerializeField] private float changeGameOverCVAlphaRate;
    [SerializeField] private float changeGameWonCVAlphaRate;
    private List<PotionIngredientListEntry> spawnedPotionIngredientListEntries = new List<PotionIngredientListEntry>();
    [SerializeField] private CanvasGroup upgradeBookButtonCV;

    public IEnumerator GameOverSequence()
    {
        gameOverCV.blocksRaycasts = true;

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(gameOverCV, 1, Time.deltaTime * changeGameOverCVAlphaRate));
    }

    public IEnumerator GameWonSequence()
    {
        gameWonCV.blocksRaycasts = true;

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
        AlterPlayerHP(Mathf.CeilToInt(maxPlayerHP * percentHP), DamageType.Heal);
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
            if (allArtifacts.Count == 0)
            {
                break;
            }

            ArtifactShopOffer offer = Instantiate(artifactShopOfferPrefab, merchantInfo.parentSpawnsTo);

            // Get artifact that will be offered
            ArtifactLabel offered = GetRandomArtifact();

            // Determine cost
            int cost = Mathf.RoundToInt(RandomHelper.RandomFloat(minMaxArtifactCostDict[artifactRarityMap[offered]]));
            offer.Set(offered, cost);
            shopArtifactList.Add(offer);
        }

        // Books
        for (int i = 0; i < numBookShopOffers; i++)
        {
            // Ran out of aartifacts
            if (allBooks.Count == 0)
            {
                break;
            }

            BookShopOffer offer = Instantiate(bookShopOfferPrefab, librarianInfo.parentSpawnsTo);

            // Get artifact that will be offered
            BookLabel offered = GetRandomBook();

            // Determine cost
            int cost = Mathf.RoundToInt(RandomHelper.RandomFloat(minMaxBookCostDict[bookRarityMap[offered]]));
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

    public bool ParseEventCondition(OptionEventGameOccurance optionEvent, string condition)
    {
        if (condition.Length == 0) return true;
        if (condition.ToLower().Equals("false")) return false;
        if (condition.ToLower().Equals("true")) return true;

        EventLabel label = optionEvent.EventLabel;
        // Debug.Log("Parsing Event Condition: " + condition);
        string[] conditionParts = CureAllStrings(condition.Split(':'));

        if (conditionParts.Length == 1)
        {
            string singleCondition = conditionParts[0];
            // Debug.Log("Single Condition: " + singleCondition);
            switch (singleCondition)
            {
                default:
                    throw new UnhandledSwitchCaseException(singleCondition);
            }
        }
        else
        {
            string conditionPart = conditionParts[0];
            string argument = conditionParts[1];

            Debug.Log("Argument Condition: " + conditionPart + ", Argument = " + argument);
            switch (conditionPart)
            {
                case "MinGoldAmount":
                    int minGoldAmount;
                    if (TryParseArgument(label, argument, out minGoldAmount))
                    {
                        return GetPlayerCurrency() > minGoldAmount;
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to Int");
                    }
                    break;
                case "MaxGoldAmount":
                    int maxGoldAmount;
                    if (TryParseArgument(label, argument, out maxGoldAmount))
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
                    if (TryParseArgument(label, argument, out minHPAmount))
                    {
                        return GetCurrentCharacterHP() > minHPAmount;
                    }
                    else
                    {
                        Debug.Log("Could not Convert Argument: " + argument + " to Int");
                    }
                    break;
                case "MaxHPAmount":
                    int maxHPAmount;
                    if (TryParseArgument(label, argument, out maxHPAmount))
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
            AlterClothierCurrency(-cost);

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
                AlterClothierCurrency(-cost);

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
