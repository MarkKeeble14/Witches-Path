using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager _Instance { get; private set; }
    private MapNodeUI currentNode;
    private GameOccurance currentOccurance;

    [SerializeField] private Character playerCharacter;
    private float maxPlayerHP;
    private float currentPlayerHP;
    private float maxPlayerMana;
    private float currentPlayerMana;
    private float currentPlayerCurrency;
    private Robe playerEquippedRobe;
    private Hat playerEquippedHat;
    private Wand playerEquippedWand;
    private List<Artifact> playerArtifacts = new List<Artifact>();

    private string persistentTokensKey = "PersistentTokens";

    private Dictionary<ArtifactLabel, ArtifactIcon> artifactDisplayTracker = new Dictionary<ArtifactLabel, ArtifactIcon>();

    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;

    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Transform artifactBar;

    [SerializeField] private ActiveSpellDisplay[] activeSpellDisplays = new ActiveSpellDisplay[3];
    private Dictionary<SpellLabel, SpellDisplay> loadedSpellDisplays = new Dictionary<SpellLabel, SpellDisplay>();
    private Dictionary<ActiveSpellDisplay, ActiveSpell> equippedActiveSpells = new Dictionary<ActiveSpellDisplay, ActiveSpell>();
    private Dictionary<PassiveSpellDisplay, PassiveSpell> equippedPassiveSpells = new Dictionary<PassiveSpellDisplay, PassiveSpell>();
    [SerializeField] private PassiveSpellDisplay[] passiveSpellDisplays = new PassiveSpellDisplay[2];
    [SerializeField] private KeyCode[] activeSpellBindings = new KeyCode[3];

    public Action OnEnterNewRoom;
    public Action OnPlayerRecieveDamage;

    private Dictionary<MapNodeType, Action> OnEnterSpecificRoomActionMap = new Dictionary<MapNodeType, Action>();

    [Header("Prefabs")]
    [SerializeField] private ArtifactIcon artifactDisplay;
    [SerializeField] private PopupText popupTextPrefab;

    [Header("Test")]
    [Header("Artifacts")]
    [SerializeField] private List<ArtifactLabel> testArtifacts;

    [Header("Passive Spells")]
    [SerializeField] private List<SpellLabel> equippablePassiveSpells = new List<SpellLabel>();
    private int equippablePassiveSpellIndex = 0;

    [Header("Active Spells")]
    [SerializeField] private List<SpellLabel> equippableActiveSpells = new List<SpellLabel>();
    private int equippableActiveSpellIndex = 0;

    [SerializeField] private SerializableDictionary<DamageSource, Color> damageSourceColorDict = new SerializableDictionary<DamageSource, Color>();


    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        TryAddPersistentTokens();

        foreach (MapNodeType type in Enum.GetValues(typeof(MapNodeType)))
        {
            OnEnterSpecificRoomActionMap.Add(type, null);
        }

        LoadMap();
        StartCoroutine(Begin());
        EquipCharacterLoadout(playerCharacter);
    }

    private void Update()
    {
        hpText.text = Mathf.RoundToInt(currentPlayerHP).ToString() + "/" + Mathf.RoundToInt(maxPlayerHP).ToString();
        manaText.text = Mathf.RoundToInt(currentPlayerMana).ToString() + "/" + Mathf.RoundToInt(maxPlayerMana).ToString();
        currencyText.text = Mathf.RoundToInt(currentPlayerCurrency).ToString();

        // Testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (testArtifacts.Count > 0)
            {
                ArtifactLabel a = testArtifacts[0];
                testArtifacts.RemoveAt(0);
                AddArtifact(a);
            }
        }

        // Testing
        // Active Spells
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Equip new spell
            equippableActiveSpellIndex++;

            if (equippableActiveSpellIndex > equippableActiveSpells.Count - 1)
                equippableActiveSpellIndex = 0;

            Debug.Log("Selected: " + equippableActiveSpells[equippableActiveSpellIndex]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            EquipActiveSpell(equippableActiveSpells[equippableActiveSpellIndex]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UnequipActiveSpell(equippableActiveSpells[equippableActiveSpellIndex]);
        }

        // Passive Spells
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // Equip new spell
            equippablePassiveSpellIndex++;

            if (equippablePassiveSpellIndex > equippablePassiveSpells.Count - 1)
                equippablePassiveSpellIndex = 0;

            Debug.Log("Selected: " + equippablePassiveSpells[equippablePassiveSpellIndex]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            EquipPassiveSpell(equippablePassiveSpells[equippablePassiveSpellIndex]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            UnequipPassiveSpell(equippablePassiveSpells[equippablePassiveSpellIndex]);
        }

        // Only allow for spell casts while in combat
        if (CombatManager._Instance.InCombat)
        {
            for (int i = 0; i < activeSpellBindings.Length; i++)
            {
                if (Input.GetKeyDown(activeSpellBindings[i]))
                {
                    ActiveSpell spellToCast = activeSpellDisplays[i].GetSpell();
                    Debug.Log("Attempting to Cast: " + spellToCast);
                    if (spellToCast.CanCast)
                    {
                        Debug.Log("Casting: " + spellToCast);
                        spellToCast.Cast();
                    }
                    else
                    {
                        Debug.Log("Can't Cast: " + spellToCast);
                        if (spellToCast.OnCooldown)
                        {
                            Debug.Log("Spell: " + spellToCast + " Cooling Down: " + spellToCast.CooldownTimer);
                        }
                        if (!spellToCast.HasMana)
                        {
                            Debug.Log("Not Enough Mana to Cast Spell: " + spellToCast);
                        }
                    }
                }
            }
        }
    }

    public void EquipPassiveSpell(SpellLabel label)
    {
        for (int i = 0; i < passiveSpellDisplays.Length; i++)
        {
            if (passiveSpellDisplays[i].IsAvailable)
            {
                PassiveSpell newSpell = (PassiveSpell)GetSpellOfType(label);
                newSpell.OnEquip();

                equippedPassiveSpells.Add(passiveSpellDisplays[i], newSpell);
                passiveSpellDisplays[i].SetPassiveSpell(newSpell);
                loadedSpellDisplays.Add(label, passiveSpellDisplays[i]);

                Debug.Log("Equipped: " + newSpell);

                return;
            }
        }

        Debug.Log("No Empty Slot to Equip: " + label.ToString());
    }

    public void UnequipPassiveSpell(SpellLabel label)
    {
        if (!loadedSpellDisplays.ContainsKey(label))
        {
            Debug.Log("Spell: " + label + ", not Currency Equipped");
            return;
        }

        SpellDisplay loaded = loadedSpellDisplays[label];

        equippedPassiveSpells.Remove((PassiveSpellDisplay)loaded);
        loaded.Unset();
        loadedSpellDisplays.Remove(label);

        Debug.Log("Unequipped: " + label.ToString());
    }

    public void EquipActiveSpell(SpellLabel label)
    {
        for (int i = 0; i < activeSpellDisplays.Length; i++)
        {
            if (activeSpellDisplays[i].IsAvailable)
            {
                ActiveSpell newSpell = (ActiveSpell)GetSpellOfType(label);
                newSpell.OnEquip();

                equippedActiveSpells.Add(activeSpellDisplays[i], newSpell);
                activeSpellDisplays[i].SetActiveSpell(newSpell, activeSpellBindings[i]);
                loadedSpellDisplays.Add(label, activeSpellDisplays[i]);

                Debug.Log("Equipped: " + newSpell);

                return;
            }
        }

        Debug.Log("No Empty Slot to Equip: " + label.ToString());
    }

    public void UnequipActiveSpell(SpellLabel label)
    {
        if (!loadedSpellDisplays.ContainsKey(label))
        {
            Debug.Log("Spell: " + label + ", not Currency Equipped");
            return;
        }

        SpellDisplay loaded = loadedSpellDisplays[label];

        equippedActiveSpells.Remove((ActiveSpellDisplay)loaded);
        loaded.Unset();
        loadedSpellDisplays.Remove(label);

        Debug.Log("Unequipped: " + label.ToString());
    }

    public void TickActiveSpellCooldowns(float tickAmount)
    {
        foreach (KeyValuePair<ActiveSpellDisplay, ActiveSpell> kvp in equippedActiveSpells)
        {
            if (kvp.Value.OnCooldown)
            {
                kvp.Value.AlterCooldown(-tickAmount);
            }
        }
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

    private void EquipCharacterLoadout(Character c)
    {
        maxPlayerHP = c.GetMaxHP();
        maxPlayerMana = c.GetMaxMana();
        currentPlayerCurrency = c.GetStartingCurrency();
        currentPlayerHP = c.GetStartingHP();
        currentPlayerMana = c.GetStartingMana();
        EquipRobe(c.GetStartingRobe());
        EquipHat(c.GetStartingHat());
        EquipWand(c.GetStartingWand());
    }

    private void EquipRobe(Robe robe)
    {
        playerEquippedRobe = robe;
        EquipEquipment(robe);

    }
    private void EquipHat(Hat hat)
    {
        playerEquippedHat = hat;
        EquipEquipment(hat);

    }

    private void EquipWand(Wand wand)
    {
        playerEquippedWand = wand;
        EquipEquipment(wand);
    }


    private void EquipEquipment(Equipment e)
    {
        // Figure out what to do here
    }

    private Spell GetSpellOfType(SpellLabel label)
    {
        switch (label)
        {
            case SpellLabel.BattleTrance:
                return new BattleTrance();
            case SpellLabel.BloodTrade:
                return new BloodTrade();
            case SpellLabel.Cripple:
                return new Cripple();
            case SpellLabel.CrushJoints:
                return new CrushJoints();
            case SpellLabel.Electrifry:
                return new Electrifry();
            case SpellLabel.Excite:
                return new Excite();
            case SpellLabel.ExposedFlesh:
                return new ExposedFlesh();
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
            case SpellLabel.Reverberations:
                return new Reverberations();
            case SpellLabel.Shock:
                return new Shock();
            case SpellLabel.Singe:
                return new Singe();
            case SpellLabel.StaticField:
                return new StaticField();
            case SpellLabel.Toxify:
                return new Toxify();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public Artifact GetArtifactOfType(ArtifactLabel label)
    {
        switch (label)
        {
            case ArtifactLabel.BankCard:
                return new BankCard();
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
            case ArtifactLabel.SheriffsEye:
                return new SheriffsEye();
            case ArtifactLabel.SmokeBomb:
                return new SmokeBomb();
            case ArtifactLabel.SmokeShroud:
                return new SmokeShroud();
            case ArtifactLabel.GreedyHands:
                return new GreedyHands();
            case ArtifactLabel.VoodooDoll:
                return new VoodooDoll();
            case ArtifactLabel.ZedsScalpel:
                return new ZedsScalpel();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public void AddArtifact(ArtifactLabel type)
    {
        Artifact artifact = GetArtifactOfType(type);
        artifact.OnEquip();
        playerArtifacts.Add(artifact);

        ArtifactIcon spawned = Instantiate(artifactDisplay, artifactBar);
        spawned.name = "Artifact(" + type + ")";
        spawned.SetSprite(artifact.GetSprite());
        spawned.SetText(Utils.SplitOnCapitalLetters(type.ToString()));

        artifactDisplayTracker.Add(artifact.GetLabel(), spawned);

        // Somehow evaluate what artifact must do and add it's effect
    }

    public void AnimateArtifact(ArtifactLabel type)
    {
        if (artifactDisplayTracker.ContainsKey(type))
        {
            artifactDisplayTracker[type].AnimateScale();
        }
    }

    internal void GameOver()
    {
        throw new NotImplementedException();
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
        Debug.Log("Loading Map");
        MapManager._Instance.Generate();
    }

    private IEnumerator Begin()
    {
        MapManager._Instance.NextRow();

        while (true)
        {
            yield return new WaitUntil(() => currentOccurance != null);
            currentOccurance.SetResolve(false);

            OnEnterNewRoom?.Invoke();
            OnEnterSpecificRoomActionMap[currentOccurance.Type]?.Invoke();

            yield return StartCoroutine(currentOccurance.RunOccurance());

            // if beat boss, break the loop as the level is done
            if (currentOccurance.Type == MapNodeType.BOSS)
            {
                break;
            }

            // Reset current occurance
            currentNode.SetMapNodeState(MapNodeState.COMPLETED);
            currentOccurance = null;

            // move to next room
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

    public void AlterCurrency(float amount)
    {
        currentPlayerCurrency += amount;
    }

    public bool AlterPlayerHP(float amount, DamageSource damageSource, bool spawnPopupText = true)
    {
        // Barricade Effect
        if (amount <= -1 && HasArtifact(ArtifactLabel.Barricade))
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
            PopupText spawned = Instantiate(popupTextPrefab, hpText.transform.position, Quaternion.identity);
            spawned.Set(Utils.RoundTo(amount, 1).ToString(), GetColorByDamageSource(damageSource));
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

    public void AlterPlayerMana(float amount)
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

    public Color GetColorByDamageSource(DamageSource damageSource)
    {
        return damageSourceColorDict[damageSource];
    }

    public float GetCurrentCharacterHP()
    {
        return currentPlayerHP;
    }

    public float GetMaxPlayerHP()
    {
        return maxPlayerHP;
    }

    public float GetCurrentPlayerMana()
    {
        return currentPlayerMana;
    }

    public float GetMaxPlayerMana()
    {
        return maxPlayerMana;
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

    public void AnimateSpell(SpellLabel label)
    {
        loadedSpellDisplays[label].AnimateScale();
    }
}
