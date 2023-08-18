using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

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

    [Header("Books")]
    [SerializeField] private Transform bookBar;
    [SerializeField] private SerializableDictionary<BookLabel, Rarity> bookRarityMap = new SerializableDictionary<BookLabel, Rarity>();
    [SerializeField] private PercentageMap<Rarity> bookRarityOdds = new PercentageMap<Rarity>();
    private Dictionary<BookLabel, BookDisplay> bookDisplayTracker = new Dictionary<BookLabel, BookDisplay>();
    private Dictionary<BookLabel, Book> equippedBooks = new Dictionary<BookLabel, Book>();
    private List<BookLabel> allBooks;
    private int bookIndex;

    [Header("Spells")]
    [SerializeField] private SerializableDictionary<SpellLabel, Rarity> spellRarityMap = new SerializableDictionary<SpellLabel, Rarity>();
    [SerializeField] private PercentageMap<Rarity> spellRarityOdds = new PercentageMap<Rarity>();
    private List<SpellLabel> allSpells;
    private Dictionary<SpellLabel, SpellDisplay> loadedSpellDisplays = new Dictionary<SpellLabel, SpellDisplay>();
    private int equippableSpellIndex;

    [Header("Active Spells")]
    [SerializeField] private KeyCode[] activeSpellBindings;
    [SerializeField] private ActiveSpellDisplay activeSpellDisplayPrefab;
    [SerializeField] private Transform activeSpellDisplaysList;
    private List<ActiveSpellDisplay> activeSpellDisplays = new List<ActiveSpellDisplay>();
    private Dictionary<ActiveSpellDisplay, ActiveSpell> equippedActiveSpells = new Dictionary<ActiveSpellDisplay, ActiveSpell>();
    private List<ActiveSpellDisplay> unusedActiveSpellDisplays = new List<ActiveSpellDisplay>();
    private int numActiveSpellSlots;

    [Header("Passive Spells")]
    [SerializeField] private PassiveSpellDisplay passiveSpellDisplayPrefab;
    [SerializeField] private Transform passiveSpellDisplaysList;
    private Dictionary<PassiveSpellDisplay, PassiveSpell> equippedPassiveSpells = new Dictionary<PassiveSpellDisplay, PassiveSpell>();
    private List<PassiveSpellDisplay> unusedPassiveSpellDisplays = new List<PassiveSpellDisplay>();
    private int numPassiveSpellSlots;


    [Header("Potions")]
    private Dictionary<PotionIngredient, int> potionIngredientMap = new Dictionary<PotionIngredient, int>();

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

    [Header("Visual")]
    [SerializeField] private SerializableDictionary<DamageType, Color> damageSourceColorDict = new SerializableDictionary<DamageType, Color>();

    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI clothierCurrencyText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI defenseText;

    // Callbacks
    public Action OnEnterNewRoom;
    public Action OnPlayerRecieveDamage;
    private Dictionary<MapNodeType, Action> OnEnterSpecificRoomActionMap = new Dictionary<MapNodeType, Action>();

    public int DamageFromEquipment { get; set; }
    public int DefenseFromEquipment { get; set; }
    private int manaFromEquipment;


    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        // Get List of all Book Labels
        allBooks = new List<BookLabel>((BookLabel[])Enum.GetValues(typeof(BookLabel)));

        // Get List of all Artifact Labels
        allArtifacts = new List<ArtifactLabel>((ArtifactLabel[])Enum.GetValues(typeof(ArtifactLabel)));

        // Get List of all Spell Labels
        allSpells = new List<SpellLabel>((SpellLabel[])Enum.GetValues(typeof(SpellLabel)));

        TryAddPersistentTokens();

        foreach (MapNodeType type in Enum.GetValues(typeof(MapNodeType)))
        {
            OnEnterSpecificRoomActionMap.Add(type, null);
        }

        // Set equipment Tool Tippables
        SetEquipmentToolTippables(equippableHats.Cast<Equipment>().ToList());
        SetEquipmentToolTippables(equippableRobes.Cast<Equipment>().ToList());
        SetEquipmentToolTippables(equippableWands.Cast<Equipment>().ToList());

        LoadMap();
        StartCoroutine(GameLoop());
        EquipCharacterLoadout(playerCharacter);
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
        hpText.text = Mathf.RoundToInt(currentPlayerHP).ToString() + "/" + Mathf.RoundToInt(maxPlayerHP).ToString();
        manaText.text = Mathf.RoundToInt(currentPlayerMana).ToString() + "/" + Mathf.RoundToInt(maxPlayerMana).ToString();
        currencyText.text = Mathf.RoundToInt(currentPlayerCurrency).ToString();
        clothierCurrencyText.text = Mathf.RoundToInt(currentPlayerClothierCurrency).ToString();

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

                    if (equippableSpellIndex > allSpells.Count - 1)
                        equippableSpellIndex = 0;

                    Debug.Log("Selected: " + allSpells[equippableSpellIndex]);
                }

                // Equip new spell
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    equippableSpellIndex--;

                    if (equippableSpellIndex < 0)
                        equippableSpellIndex = allSpells.Count - 1;

                    Debug.Log("Selected: " + allSpells[equippableSpellIndex]);
                }

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    EquipSpell(allSpells[equippableSpellIndex]);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    EquipSpell(allSpells[equippableSpellIndex]);
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

    // Spell Swap Sequence
    [SerializeField] private GameObject[] turnOnForSwapSpellSequence;
    [SerializeField] private VisualSpellDisplay visualSpellDisplayPrefab;
    [SerializeField] private Transform spawnVisualSpellDisplayPrefabOn;
    private bool skipSpellSwap;
    private Spell spellToSwap;

    public void SkipSpellSwap()
    {
        skipSpellSwap = true;
    }

    public void SetSpellToSwap(Spell spell)
    {
        spellToSwap = spell;
    }

    public IEnumerator StartSwapSpellSequnce(SpellLabel label)
    {
        Spell spell = GetSpellOfType(label);
        switch (spell)
        {
            case ActiveSpell:
                yield return StartCoroutine(SwapSpellSequence(ContentType.ActiveSpell, label));
                break;
            case PassiveSpell:
                yield return StartCoroutine(SwapSpellSequence(ContentType.PassiveSpell, label));
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    private IEnumerator SwapSpellSequence(ContentType requiredSpellType, SpellLabel newSpell)
    {
        // First check to see if theres an extra slot to use
        switch (requiredSpellType)
        {
            case ContentType.ActiveSpell:
                // if there is an extra spell display to use
                if (unusedActiveSpellDisplays.Count > 0)
                {
                    // Just use it
                    EquipActiveSpell(newSpell);
                    yield break;
                }
                break;
            case ContentType.PassiveSpell:
                // if there is an extra spell display to use
                if (unusedPassiveSpellDisplays.Count > 0)
                {
                    // Just use it
                    EquipPassiveSpell(newSpell);
                    yield break;
                }
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }

        // Enable extra UI
        foreach (GameObject obj in turnOnForSwapSpellSequence)
        {
            obj.SetActive(true);
        }

        // Spawn Visual Spell Display to Give Player Info
        VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, spawnVisualSpellDisplayPrefabOn);
        spawned.SetSpell(GetSpellOfType(newSpell));

        // Tell all displays of the specified type that the swap sequence IS happening 
        switch (requiredSpellType)
        {
            case ContentType.ActiveSpell:
                foreach (KeyValuePair<ActiveSpellDisplay, ActiveSpell> spell in equippedActiveSpells)
                {
                    // Tell Spell Display new on click action
                    spell.Key.SetSpellDisplayState(SpellDisplayState.SwapSequence);
                }
                break;
            case ContentType.PassiveSpell:
                foreach (KeyValuePair<PassiveSpellDisplay, PassiveSpell> spell in equippedPassiveSpells)
                {
                    // Tell Spell Display new on click action
                    spell.Key.SetSpellDisplayState(SpellDisplayState.SwapSequence);
                }
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }

        // Wait until either a spell has been selected to be swapped or the player has elected to reject the swap
        yield return new WaitUntil(() => skipSpellSwap || spellToSwap != null);

        // Re-tell all spell displays to go back to normal
        switch (requiredSpellType)
        {
            case ContentType.ActiveSpell:
                foreach (KeyValuePair<ActiveSpellDisplay, ActiveSpell> spell in equippedActiveSpells)
                {
                    // Tell Spell Display new on click action
                    spell.Key.SetSpellDisplayState(SpellDisplayState.Normal);
                }
                break;
            case ContentType.PassiveSpell:
                foreach (KeyValuePair<PassiveSpellDisplay, PassiveSpell> spell in equippedPassiveSpells)
                {
                    // Tell Spell Display new on click action
                    spell.Key.SetSpellDisplayState(SpellDisplayState.Normal);
                }
                break;
        }

        // Player has decided not to reject the swap and has selected a spell to swap out
        if (!skipSpellSwap)
        {
            // Swap out the selected Spells
            switch (requiredSpellType)
            {
                case ContentType.ActiveSpell:
                    ActiveSpellDisplay activeDisplay = UnequipActiveSpell(spellToSwap.Label);
                    unusedActiveSpellDisplays.Remove(activeDisplay);
                    EquipActiveSpell(newSpell, activeDisplay);
                    break;
                case ContentType.PassiveSpell:
                    PassiveSpellDisplay passiveDisplay = UnequipPassiveSpell(spellToSwap.Label);
                    unusedPassiveSpellDisplays.Remove(passiveDisplay);
                    EquipPassiveSpell(newSpell, passiveDisplay);
                    break;
            }
        }

        // Destroy previously spawned visual spell display
        Destroy(spawned.gameObject);

        // Disable the screens used for this sequence
        foreach (GameObject obj in turnOnForSwapSpellSequence)
        {
            obj.SetActive(false);
        }

        // Reset for next time
        spellToSwap = null;
        skipSpellSwap = false;
    }

    public void EquipSpell(SpellLabel label)
    {
        Spell spell = GetSpellOfType(label);
        switch (spell)
        {
            case ActiveSpell:
                EquipActiveSpell(label);
                break;
            case PassiveSpell:
                EquipPassiveSpell(label);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public PassiveSpellDisplay SpawnPassiveSpellDisplay()
    {
        PassiveSpellDisplay spawned = Instantiate(passiveSpellDisplayPrefab, passiveSpellDisplaysList);
        unusedPassiveSpellDisplays.Add(spawned);
        spawned.SetEmpty(true);
        return spawned;
    }

    public ActiveSpellDisplay SpawnActiveSpellDisplay()
    {
        ActiveSpellDisplay spawned = Instantiate(activeSpellDisplayPrefab, activeSpellDisplaysList);
        KeyCode keyBinding = activeSpellBindings[unusedActiveSpellDisplays.Count + equippedActiveSpells.Count];
        unusedActiveSpellDisplays.Add(spawned);
        spawned.SetKeyBinding(keyBinding);
        spawned.SetEmpty(true);
        return spawned;
    }

    public void EquipPassiveSpell(SpellLabel label, PassiveSpellDisplay spellDisplay = null)
    {
        // if no spell display to equip into is specified
        if (spellDisplay == null)
        {
            // if there is an extra spell display to use
            if (unusedPassiveSpellDisplays.Count > 0)
            {
                // grab that display to use
                spellDisplay = unusedPassiveSpellDisplays[0];
                unusedPassiveSpellDisplays.RemoveAt(0);
            }
            else
            {
                // if there isn't, then we can't equip this spell and something has probably gone wrong
                return;
            }
        }

        spellDisplay.SetEmpty(false);
        PassiveSpell newSpell = (PassiveSpell)GetSpellOfType(label);
        newSpell.OnEquip();

        spellDisplay.SetPassiveSpell(newSpell);
        equippedPassiveSpells.Add(spellDisplay, newSpell);
        loadedSpellDisplays.Add(label, spellDisplay);

        // Debug.Log("Equipped: " + newSpell);
    }

    public PassiveSpellDisplay UnequipPassiveSpell(SpellLabel label)
    {
        if (!loadedSpellDisplays.ContainsKey(label))
        {
            Debug.Log("Spell: " + label + ", not Currency Equipped");
            return null;
        }

        PassiveSpellDisplay loaded = (PassiveSpellDisplay)loadedSpellDisplays[label];

        // Manage lists
        loaded.Unset();
        equippedPassiveSpells.Remove(loaded);
        loadedSpellDisplays.Remove(label);

        // if the number of equipped passive spells + the number of unused passive spell displays is over the number of allowed spell slots, destroy the extra one
        // otherwise, it remains simply unset
        if (equippedPassiveSpells.Count + unusedPassiveSpellDisplays.Count > numPassiveSpellSlots)
        {
            Destroy(loaded.gameObject);
        }
        else
        {
            unusedPassiveSpellDisplays.Add(loaded);
        }

        return loaded;

        // Debug.Log("Unequipped: " + label.ToString());
    }

    public void EquipActiveSpell(SpellLabel label, ActiveSpellDisplay spellDisplay = null)
    {
        // if no spell display to equip into is specified
        if (spellDisplay == null)
        {
            // if there is an extra spell display to use
            if (unusedActiveSpellDisplays.Count > 0)
            {
                // grab that display to use
                spellDisplay = unusedActiveSpellDisplays[0];
                unusedActiveSpellDisplays.RemoveAt(0);
            }
            else
            {
                // if there isn't, then we can't equip this spell and something has probably gone wrong
                return;
            }
        }

        spellDisplay.SetEmpty(false);
        ActiveSpell newSpell = (ActiveSpell)GetSpellOfType(label);
        newSpell.OnEquip();

        spellDisplay.SetActiveSpell(newSpell);
        equippedActiveSpells.Add(spellDisplay, newSpell);
        loadedSpellDisplays.Add(label, spellDisplay);
        activeSpellDisplays.Add(spellDisplay);

        // Debug.Log("Equipped: " + newSpell);
    }

    public ActiveSpellDisplay UnequipActiveSpell(SpellLabel label)
    {
        if (!loadedSpellDisplays.ContainsKey(label))
        {
            Debug.Log("Spell: " + label + ", not Currency Equipped");
            return null;
        }

        ActiveSpellDisplay loaded = (ActiveSpellDisplay)loadedSpellDisplays[label];

        // if the number of equipped active spells + the number of unused active spell displays is over the number of allowed spell slots, destroy the extra one
        // otherwise, it remains simply unset
        if (equippedActiveSpells.Count + unusedActiveSpellDisplays.Count > numActiveSpellSlots)
        {
            Destroy(loaded.gameObject);
        }
        else
        {
            // Manage lists
            loaded.Unset();
            unusedActiveSpellDisplays.Add(loaded);
        }

        equippedActiveSpells.Remove(loaded);
        loadedSpellDisplays.Remove(label);
        activeSpellDisplays.Remove(loaded);

        // Debug.Log("Unequipped: " + label.ToString());
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

    private void EquipCharacterLoadout(Character c)
    {
        // Spawn default num spell slots
        numActiveSpellSlots = c.GetStartingActiveSpellSlots();
        numPassiveSpellSlots = c.GetStartingPassiveSpellSlots();
        for (int i = 0; i < numActiveSpellSlots; i++)
        {
            SpawnActiveSpellDisplay();
        }
        for (int i = 0; i < numPassiveSpellSlots; i++)
        {
            SpawnPassiveSpellDisplay();
        }

        // Equip starting spells
        foreach (SpellLabel spellLabel in c.GetStartingSpells())
        {
            EquipSpell(spellLabel);
            allSpells.Remove(spellLabel);
        }

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

    public Spell GetSpellOfType(SpellLabel label)
    {
        switch (label)
        {
            case SpellLabel.BattleTrance:
                return new BattleTrance();
            case SpellLabel.TradeBlood:
                return new TradeBlood();
            case SpellLabel.Cripple:
                return new Cripple();
            case SpellLabel.CrushJoints:
                return new CrushJoints();
            case SpellLabel.Electrifry:
                return new Electrifry();
            case SpellLabel.Excite:
                return new Excite();
            case SpellLabel.ExposeFlesh:
                return new ExposeFlesh();
            case SpellLabel.Fireball:
                return new Fireball();
            case SpellLabel.Flurry:
                return new Flurry();
            case SpellLabel.Forethought:
                return new Forethought();
            case SpellLabel.ImpartialAid:
                return new ImpartialAid();
            case SpellLabel.Inferno:
                return new Inferno();
            case SpellLabel.Jarkai:
                return new Jarkai();
            case SpellLabel.MagicRain:
                return new MagicRain();
            case SpellLabel.Overexcite:
                return new Overexcite();
            case SpellLabel.Plague:
                return new Plague();
            case SpellLabel.PoisonTips:
                return new PoisonTips();
            case SpellLabel.Reverberate:
                return new Reverberate();
            case SpellLabel.Shock:
                return new Shock();
            case SpellLabel.Singe:
                return new Singe();
            case SpellLabel.StaticField:
                return new StaticField();
            case SpellLabel.Toxify:
                return new Toxify();
            case SpellLabel.WitchesWill:
                return new WitchesWill();
            case SpellLabel.WitchesWard:
                return new WitchesWard();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public Artifact GetArtifactOfType(ArtifactLabel label)
    {
        switch (label)
        {
            case ArtifactLabel.VIPCard:
                return new VIPCard();
            case ArtifactLabel.Barricade:
                return new Barricade();
            case ArtifactLabel.BlueMantis:
                return new BlueMantis();
            case ArtifactLabel.CanyonChunk:
                return new CanyonChunk();
            case ArtifactLabel.DoctorsReport:
                return new DoctorsReport();
            case ArtifactLabel.SpecialSpinich:
                return new SpecialSpinach();
            case ArtifactLabel.HalfLitFirework:
                return new HalfLitFirework();
            case ArtifactLabel.HealthInsurance:
                return new HealthInsurance();
            case ArtifactLabel.HolyShield:
                return new HolyShield();
            case ArtifactLabel.InvertedPolaroid:
                return new InvertedPolaroid();
            case ArtifactLabel.LooseTrigger:
                return new LooseTrigger();
            case ArtifactLabel.BoldInvestments:
                return new BoldInvestments();
            case ArtifactLabel.MedicineKit:
                return new MedicineKit();
            case ArtifactLabel.MolatovCocktail:
                return new MolatovCocktail();
            case ArtifactLabel.Plaguebringer:
                return new Plaguebringer();
            case ArtifactLabel.RustyCannon:
                return new RustyCannon();
            case ArtifactLabel.SmokeShroud:
                return new SmokeShroud();
            case ArtifactLabel.GreedyHands:
                return new GreedyHands();
            case ArtifactLabel.VoodooDoll:
                return new VoodooDoll();
            case ArtifactLabel.ZedsScalpel:
                return new ZedsScalpel();
            case ArtifactLabel.BarbariansBlade:
                return new BarbariansBlade();
            case ArtifactLabel.BlackPrism:
                return new BlackPrism();
            case ArtifactLabel.Boulder:
                return new Boulder();
            case ArtifactLabel.CaveMural:
                return new CaveMural();
            case ArtifactLabel.CheapStopwatch:
                return new CheapStopwatch();
            case ArtifactLabel.HiredHand:
                return new HiredHand();
            case ArtifactLabel.LizardSkinSilk:
                return new LizardSkinSilk();
            case ArtifactLabel.LuckyCoin:
                return new LuckyCoin();
            case ArtifactLabel.Telescope:
                return new Telescope();
            case ArtifactLabel.Crown:
                return new Crown();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public Book GetBookOfType(BookLabel label)
    {
        switch (label)
        {
            case BookLabel.WitchesTravelGuide:
                return new WitchesTravelGuide();
            case BookLabel.MedicalNovella:
                return new MedicalNovella();
            case BookLabel.MerchantsManual:
                return new MerchantsManual();
            case BookLabel.BusinessTextbook:
                return new BusinessTextbook();
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

        Artifact artifact = GetArtifactOfType(type);
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
        Book book = GetBookOfType(type);

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
        foreach (KeyValuePair<PotionIngredient, int> kvp in potionIngredientMap)
        {
            Debug.Log(kvp);
        }
    }

    public void AddPotionIngredient(PotionIngredient ingredient)
    {
        if (potionIngredientMap.ContainsKey(ingredient))
        {
            potionIngredientMap[ingredient] = potionIngredientMap[ingredient] + 1;

            // Update entry in UI
            potionIngredientUIList[ingredient].UpdateQuantity(potionIngredientMap[ingredient]);
        }
        else
        {
            potionIngredientMap.Add(ingredient, 1);

            PotionIngredientListEntry spawned = Instantiate(potionIngredientListEntryPrefab, potionIngredientListParent);
            potionIngredientUIList.Add(ingredient, spawned);
            spawned.Set(ingredient, 1);
        }
    }

    public void RemovePotionIngredient(PotionIngredient ingredient)
    {
        if (potionIngredientMap.ContainsKey(ingredient))
        {
            int numIngredient = potionIngredientMap[ingredient];
            if (numIngredient == 1)
            {
                potionIngredientMap.Remove(ingredient);

                PotionIngredientListEntry ui = potionIngredientUIList[ingredient];
                Destroy(ui.gameObject);
                potionIngredientUIList.Remove(ingredient);
            }
            else
            {
                potionIngredientMap[ingredient] = numIngredient - 1;
                potionIngredientUIList[ingredient].UpdateQuantity(potionIngredientMap[ingredient]);
            }
        }
        else
        {
            throw new Exception();
        }
    }

    public PotionIngredient GetRandomPotionIngredient()
    {
        return RandomHelper.GetRandomEnumValue<PotionIngredient>();
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

    public void LoadMap()
    {
        MapManager._Instance.Generate();
    }

    private IEnumerator GameLoop()
    {
        MapManager._Instance.NextRow();

        while (true)
        {
            yield return new WaitUntil(() => currentOccurance != null);
            currentOccurance.SetResolve(false);

            MapManager._Instance.Hide();

            OnEnterNewRoom?.Invoke();
            OnEnterSpecificRoomActionMap[currentOccurance.Type]?.Invoke();

            yield return StartCoroutine(currentOccurance.RunOccurance());

            // if beat boss, break the loop as the level is done
            if (currentOccurance.Type == MapNodeType.Boss)
            {
                break;
            }

            // Reset current occurance
            currentNode.SetMapNodeState(MapNodeState.COMPLETED);
            currentOccurance = null;

            // move to next room
            MapManager._Instance.Show();

            MapManager._Instance.NextRow();
        }

        Debug.Log("Level Ended");
    }

    [ContextMenu("ResolveCurrentEvent")]
    public void ResolveCurrentEvent()
    {
        currentOccurance.SetResolve(true);
    }

    public void SetCurrentGameOccurance(MapNodeUI setNodeTo)
    {
        currentNode = setNodeTo;
        currentOccurance = currentNode.GetRepresentedGameOccurance();
    }

    public GameOccurance GetCurrentGameOccurance()
    {
        return currentOccurance;
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

    public bool AlterPlayerHP(int amount, DamageType damageSource, bool spawnPopupText = true)
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
            spawned.Set((amount > 0 ? "+" : "") + Utils.RoundTo(amount, 1).ToString(), GetColorByDamageSource(damageSource));
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

    public Color GetColorByDamageSource(DamageType damageSource)
    {
        return damageSourceColorDict[damageSource];
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
    public void AnimateSpell(SpellLabel label)
    {
        loadedSpellDisplays[label].AnimateScale();
    }

    [Header("Game Over")]
    [SerializeField] private CanvasGroup gameOverCV;
    [SerializeField] private float changeGameOverCVAlphaRate;

    public IEnumerator GameOverSequence()
    {
        gameOverCV.blocksRaycasts = true;

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(gameOverCV, 1, Time.deltaTime * changeGameOverCVAlphaRate));
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePotionIngredientListScreen()
    {
        potionIngredientListScreen.SetActive(!potionIngredientListScreen.activeInHierarchy);
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
    private Dictionary<PotionIngredient, PotionIngredientListEntry> potionIngredientUIList = new Dictionary<PotionIngredient, PotionIngredientListEntry>();
    [SerializeField] private PotionIngredientListEntry potionIngredientListEntryPrefab;
    [SerializeField] private Transform potionIngredientListParent;
    [SerializeField] private GameObject potionIngredientListScreen;
    [SerializeField] private GameObject[] turnOnForBrewPotionScreen;
    [SerializeField] private GameObject[] turnOffForBrewPotionScreen;

    public void RestAtCampfire()
    {
        float value = BalenceManager._Instance.GetValue(MapNodeType.Campfire, "HealPercent");
        float percentHP = value / 100;
        AlterPlayerHP(Mathf.CeilToInt(maxPlayerHP * percentHP), DamageType.Heal);
        ResolveCurrentEvent();
    }

    public void OpenBrewPotionScreen()
    {
        SwitchUIScreens(turnOnForBrewPotionScreen, turnOffForBrewPotionScreen);
    }

    public void CloseBrewPotionScreen()
    {
        SwitchUIScreens(turnOffForBrewPotionScreen, turnOnForBrewPotionScreen);
    }

    public void ChoosePotion()
    {
        // Add potion somehow

        // Close brew potion screen 
        CloseBrewPotionScreen();
        ResolveCurrentEvent();
    }

    #endregion

    #region Tavern

    [Header("Tavern")]
    [SerializeField] private int numPotionIngredientShopOffers;
    [SerializeField] private int numArtifactShopOffers;
    [SerializeField] private int numEquipmentShopOffers;

    [SerializeField] private EquipmentShopOffer equipmentShopOfferPrefab;
    [SerializeField] private ArtifactShopOffer artifactShopOfferPrefab;
    [SerializeField] private IngredientShopOffer ingredientShopOfferPrefab;

    private List<ArtifactShopOffer> shopArtifactList = new List<ArtifactShopOffer>();
    private List<IngredientShopOffer> shopIngredientList = new List<IngredientShopOffer>();
    private List<EquipmentShopOffer> shopEquipmentList = new List<EquipmentShopOffer>();

    [SerializeField] private SerializableDictionary<TavernScreen, TavernScreenInformation> tavernScreens = new SerializableDictionary<TavernScreen, TavernScreenInformation>();
    [SerializeField] private SerializableDictionary<Rarity, Vector2> minMaxArtifactCostDict = new SerializableDictionary<Rarity, Vector2>();
    [SerializeField] private SerializableDictionary<Rarity, Vector2> minMaxEquipmentCostDict = new SerializableDictionary<Rarity, Vector2>();
    [SerializeField] private Vector2 minMaxIngredientCost;

    [SerializeField] private GameObject[] turnOnForBrowseWaresScreen;

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
    }

    public void OpenBrowseWaresScreen()
    {
        foreach (GameObject obj in turnOnForBrowseWaresScreen)
        {
            obj.SetActive(true);
        }
    }

    public void CloseBrowseWaresScreen()
    {
        foreach (GameObject obj in turnOnForBrowseWaresScreen)
        {
            obj.SetActive(false);
        }
    }

    [System.Serializable]
    private class TavernScreenInformation
    {
        public GameObject[] turnOnForScreen;
        public Transform parentSpawnsTo;
    }

    private enum TavernScreen
    {
        Innkeeper,
        Clothier,
        Merchant
    }

    public void OpenScreen(int index)
    {
        foreach (GameObject obj in tavernScreens[(TavernScreen)index].turnOnForScreen)
        {
            obj.SetActive(true);
        }
    }

    public void CloseScreen(int index)
    {
        foreach (GameObject obj in tavernScreens[(TavernScreen)index].turnOnForScreen)
        {
            obj.SetActive(false);
        }
    }

    public void LoadShop()
    {
        // Spawn Offers
        TavernScreenInformation innkeeperInfo = tavernScreens[TavernScreen.Innkeeper];
        TavernScreenInformation merchantInfo = tavernScreens[TavernScreen.Merchant];
        TavernScreenInformation clothierInfo = tavernScreens[TavernScreen.Clothier];

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
            offer.Set(offered, cost, null);
            shopArtifactList.Add(offer);
        }

        // Potion Ingredients
        for (int i = 0; i < numPotionIngredientShopOffers; i++)
        {
            IngredientShopOffer offer = Instantiate(ingredientShopOfferPrefab, innkeeperInfo.parentSpawnsTo);
            offer.Set(GetRandomPotionIngredient(), RandomHelper.RandomIntExclusive(minMaxIngredientCost), null);
            shopIngredientList.Add(offer);
        }

        // Equipment
        for (int i = 0; i < numEquipmentShopOffers; i++)
        {
            EquipmentShopOffer offer = Instantiate(equipmentShopOfferPrefab, clothierInfo.parentSpawnsTo);
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
    }

    #region Runemaiden

    // Reroll Rune
    public void RerollRune(Equipment e)
    {

    }

    // Upgrade Rune
    public void UpgradeRune(Equipment e)
    {

    }

    #endregion

    #region Merchant

    #endregion

    #region Innkeeper

    #endregion

    #endregion

    #region Event 

    private string[] CureAllStrings(string[] arr)
    {
        string[] res = new string[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            res[i] = arr[i];
        }
        return res;
    }

    public IEnumerator ParseEventEffect(OptionEvent optionEvent, string effects)
    {
        if (effects.Length == 0) yield break;
        EventLabel label = optionEvent.EventLabel;
        bool hasRewards = false;

        Debug.Log("Parsing Event Effect: " + effects);
        string[] commands = CureAllStrings(effects.Split(';'));
        Debug.Log("NumCommands: " + commands.Length);

        foreach (string command in commands)
        {
            string[] commandParts = CureAllStrings(command.Split(':'));
            Debug.Log("Command: " + command + ", NumParts = " + commandParts.Length);
            if (commandParts.Length == 1)
            {
                string singleCommand = commandParts[0];
                Debug.Log("Single Command: " + singleCommand);
                switch (singleCommand)
                {
                    case "AddRandomArtifact":
                        RewardManager._Instance.AddReward(GetRandomArtifact());
                        hasRewards = true;
                        break;
                    case "AddRandomBook":
                        RewardManager._Instance.AddReward(GetRandomBook());
                        hasRewards = true;
                        break;
                    case "RemoveRandomArtifact":
                        if (equippedArtifacts.Count > 0)
                            RemoveArtifact(GetRandomOwnedArtifact());
                        break;
                    case "RemoveRandomBook":
                        if (equippedBooks.Count > 0)
                            RemoveBook(GetRandomOwnedBook());
                        break;
                    case "AddRandomPotionIngredient":
                        RewardManager._Instance.AddReward(GetRandomPotionIngredient());
                        hasRewards = true;
                        break;
                    case "RemoveRandomPotionIngredient":
                        if (GetNumPotionIngredients() > 0)
                            RemovePotionIngredient(GetRandomOwnedPotionIngredient());
                        break;
                    default:
                        throw new UnhandledSwitchCaseException(singleCommand);
                }
            }
            else
            {
                string commandPart = commandParts[0];
                string argument = commandParts[1];

                Debug.Log("Argument Command: " + commandPart + ", Argument = " + argument);
                switch (commandPart)
                {
                    case "AlterGold":
                        int goldAmount;
                        if (TryParseArgument(label, argument, out goldAmount))
                        {
                            if (goldAmount > 0)
                            {
                                RewardManager._Instance.AddCurrencyReward((int)goldAmount);
                                hasRewards = true;
                            }
                            else
                            {
                                AlterCurrency(goldAmount);
                            }
                        }
                        else
                        {
                            Debug.Log("Could not Convert Argument: " + argument + " to Int");
                        }
                        break;
                    case "AlterHP":
                        int hpAmount;
                        if (TryParseArgument(label, argument, out hpAmount))
                        {
                            AlterPlayerHP(hpAmount, DamageType.Default);
                        }
                        else
                        {
                            Debug.Log("Could not Convert Argument: " + argument + " to Int");
                        }
                        break;
                    case "RemoveArtifact":
                        ArtifactLabel removedArtifact;
                        if (Enum.TryParse<ArtifactLabel>(argument, out removedArtifact))
                        {
                            RemoveArtifact(removedArtifact);
                        }
                        else
                        {
                            Debug.Log("Could not Convert Argument: " + argument + " to ArtifactLabel");
                        }
                        break;
                    case "RemoveBook":
                        BookLabel removedBook;
                        if (Enum.TryParse<BookLabel>(argument, out removedBook))
                        {
                            RemoveBook(removedBook);
                        }
                        else
                        {
                            Debug.Log("Could not Convert Argument: " + argument + " to BookLabel");
                        }
                        break;
                    case "AddArtifact":
                        ArtifactLabel addedArtifact;
                        if (Enum.TryParse<ArtifactLabel>(argument, out addedArtifact))
                        {
                            RewardManager._Instance.AddReward(addedArtifact);
                            hasRewards = true;

                        }
                        else
                        {
                            Debug.Log("Could not Convert Argument: " + argument + " to ArtifactLabel");
                        }
                        break;
                    case "AddBook":
                        BookLabel addedBook;
                        if (Enum.TryParse<BookLabel>(argument, out addedBook))
                        {
                            RewardManager._Instance.AddReward(addedBook);
                            hasRewards = true;
                        }
                        else
                        {
                            Debug.Log("Could not Convert Argument: " + argument + " to BookLabel");
                        }
                        break;
                    case "AddRandomPotionIngredient":
                        int numIngredients;
                        if (TryParseArgument(label, argument, out numIngredients))
                        {
                            for (int i = 0; i < numIngredients; i++)
                            {
                                RewardManager._Instance.AddReward(GetRandomPotionIngredient());
                            }
                            hasRewards = true;
                        }
                        else
                        {
                            Debug.Log("Could not Convert Argument: " + argument + " to Int");
                        }
                        break;
                    case "ChainEvent":
                        int index;
                        if (TryParseArgument(label, argument, out index))
                        {
                            EventManager._Instance.SetChainingEvents(true);
                            EventManager._Instance.SetOutcome(null);
                            StartCoroutine(EventManager._Instance.StartOptionEvent(optionEvent.GetChainEvent(index)));
                        }
                        else
                        {
                            Debug.Log("Could not Convert Argument: " + argument + " to Int");
                        }
                        break;
                    default:
                        throw new UnhandledSwitchCaseException(commandPart + ", " + argument);
                }
            }
        }

        if (hasRewards)
        {
            yield return StartCoroutine(RewardManager._Instance.ShowRewardScreen());
        }
        else
        {
            yield return null;
        }
    }

    public bool ParseEventCondition(OptionEvent optionEvent, string condition)
    {
        if (condition.Length == 0) return true;
        if (condition.ToLower().Equals("false")) return false;
        if (condition.ToLower().Equals("true")) return true;

        EventLabel label = optionEvent.EventLabel;
        Debug.Log("Parsing Event Condition: " + condition);
        string[] conditionParts = CureAllStrings(condition.Split(':'));

        if (conditionParts.Length == 1)
        {
            string singleCondition = conditionParts[0];
            Debug.Log("Single Condition: " + singleCondition);
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
                default:
                    throw new UnhandledSwitchCaseException(conditionPart + ", " + argument);
            }
        }
        return false;
    }

    private int GetNumPotionIngredients()
    {
        int r = 0;
        foreach (KeyValuePair<PotionIngredient, int> kvp in potionIngredientMap)
        {
            r += kvp.Value;
        }
        return r;
    }

    private PotionIngredient GetRandomOwnedPotionIngredient()
    {
        return RandomHelper.GetRandomFromList(potionIngredientMap.Keys.ToList());
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
                Debug.Log("Parsed Argument: " + v);
                return true;
            }
            else
            {
                Debug.Log("Failed to Parse Argument");
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
            return ArtifactLabel.Crown;
        }


        return RandomHelper.GetRandomFromList(options);
    }

    public void AddRandomArtifact()
    {
        AddArtifact(GetRandomArtifact());
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
        if (options.Count == 0)
        {
            return BookLabel.WitchesTravelGuide;
        }

        return RandomHelper.GetRandomFromList(options);
    }

    // BrEAk

    public SpellLabel GetRandomSpell(bool removeFromPool = true)
    {
        SpellLabel spell = GetRandomSpellOfRarity(spellRarityOdds.GetOption());
        if (removeFromPool)
        {
            allSpells.Remove(spell);
        }
        return spell;
    }

    public SpellLabel GetRandomSpellOfRarity(Rarity r)
    {
        List<SpellLabel> options = new List<SpellLabel>();
        foreach (SpellLabel l in allSpells)
        {
            if (spellRarityMap[l] == r)
            {
                options.Add(l);
            }
        }

        // No Books Remaining
        if (options.Count == 0)
        {
            return SpellLabel.WitchesWill;
        }

        return RandomHelper.GetRandomFromList(options);
    }

    public void AddRandomBook()
    {
        AddBook(GetRandomBook());
    }

    #endregion

    #region Library 

    [Header("Swap Book Screen")]
    [SerializeField] private GameObject[] turnOnForSwapBookScreen;
    [SerializeField] private GameObject[] turnOffForSwapBookScreen;
    [SerializeField] private Transform swapBookOptionList;
    [SerializeField] private SwapBookButton swapBookButtonPrefab;
    private List<SwapBookButton> spawnedSwapBookButtons = new List<SwapBookButton>();

    private BookLabel libraryBookOffer;

    public void UpgradeBooks()
    {
        foreach (KeyValuePair<BookLabel, Book> b in equippedBooks)
        {
            b.Value.TryCallLevelUp(true);
        }

        ResolveCurrentEvent();
    }

    public void OpenSwapBookScreen()
    {
        SwitchUIScreens(turnOnForSwapBookScreen, turnOffForSwapBookScreen);

        SwapBookButton spawned = Instantiate(swapBookButtonPrefab, swapBookOptionList);
        libraryBookOffer = GetRandomBook();
        spawned.Set(libraryBookOffer, null);
        spawnedSwapBookButtons.Add(spawned);
    }

    public void CloseSwapBookScreen()
    {
        SwitchUIScreens(turnOffForSwapBookScreen, turnOnForSwapBookScreen);

        while (spawnedSwapBookButtons.Count > 0)
        {
            SwapBookButton spawned = spawnedSwapBookButtons[0];
            spawnedSwapBookButtons.RemoveAt(0);
            Destroy(spawned.gameObject);
        }
    }

    public void SwapBooks(BookLabel swappingOut, BookLabel swappingTo)
    {
        RemoveBook(swappingOut);
        AddBook(swappingTo);
        AnimateBook(swappingTo);
    }

    public void RejectBookSwap()
    {
        CloseSwapBookScreen();
        ResolveCurrentEvent();
    }

    public void AcceptBookSwap()
    {
        SwapBooks(GetRandomOwnedBook(), libraryBookOffer);
        CloseSwapBookScreen();
        ResolveCurrentEvent();
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

    public void OpenSelectEquipmentScreen(bool showCost, string label)
    {
        selectEquipmentScreen.SetActive(true);

        SelectEquipmentButton spawnedForHat = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForHat.Set(playerEquippedHat, () => selectedEquipment = playerEquippedHat);

        SelectEquipmentButton spawnedForRobe = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForRobe.Set(playerEquippedRobe, () => selectedEquipment = playerEquippedRobe);

        SelectEquipmentButton spawnedForWand = Instantiate(selectEquipmentButtonPrefab, selectEquipmentList);
        spawnedForWand.Set(playerEquippedWand, () => selectedEquipment = playerEquippedWand);

        if (showCost)
        {
            spawnedForHat.ShowCost(1, label);
            spawnedForRobe.ShowCost(1, label);
            spawnedForWand.ShowCost(1, label);
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
        OpenSelectEquipmentScreen(true, "Refroge");

        while (!exitSelectEquipmentScreen)
        {
            yield return new WaitUntil(() => selectedEquipment != null || exitSelectEquipmentScreen);

            if (exitSelectEquipmentScreen)
            {
                selectedEquipment = null;
                break;
            }

            // No Moneys
            if (currentPlayerClothierCurrency <= 0)
            {
                selectedEquipment = null;
                continue;
            }

            // Reforge
            selectedEquipment.Reforge();

            // Use Currency
            AlterClothierCurrency(-1);

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
        OpenSelectEquipmentScreen(false, "");

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
                if (currentPlayerClothierCurrency < selectedEquipment.GetCostToBoost())
                {
                    hasSelectedStat = false;
                    continue;
                }

                // Use Currency
                AlterClothierCurrency(-selectedEquipment.GetCostToBoost());

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
