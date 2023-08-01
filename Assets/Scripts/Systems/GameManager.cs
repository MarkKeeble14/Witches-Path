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
    private float currentPlayerCurrency;
    private Robe playerEquippedRobe;
    private Hat playerEquippedHat;
    private Wand playerEquippedWand;
    private List<Artifact> playerArtifacts = new List<Artifact>();

    private string persistentTokensKey = "PersistentTokens";

    private Dictionary<ArtifactLabel, ArtifactDisplay> artifactDisplayTracker = new Dictionary<ArtifactLabel, ArtifactDisplay>();

    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Transform artifactBar;

    public Action OnEnterNewRoom;
    public Action OnPlayerRecieveDamage;

    private Dictionary<MapNodeType, Action> OnEnterSpecificRoomActionMap = new Dictionary<MapNodeType, Action>();
    [SerializeField] private List<ArtifactLabel> testArtifacts;

    [Header("Prefabs")]
    [SerializeField] private ArtifactDisplay artifactDisplay;

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
        currencyText.text = Mathf.RoundToInt(currentPlayerCurrency).ToString();

        // Testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (testArtifacts.Count > 0)
            {
                ArtifactLabel a = testArtifacts[0];
                testArtifacts.RemoveAt(0);
                AddArtifact(a);
            }
        }
    }


    private void EquipCharacterLoadout(Character c)
    {
        maxPlayerHP = c.GetMaxHP();
        currentPlayerCurrency = c.GetStartingCurrency();
        currentPlayerHP = c.GetStartingHP();
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

        ArtifactDisplay spawned = Instantiate(artifactDisplay, artifactBar);
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

    public bool AlterPlayerHP(float amount)
    {
        // Barricade Effect
        if (amount <= -1 && HasArtifact(ArtifactLabel.Barricade))
        {
            amount += ArtifactManager._Instance.GetValue(ArtifactLabel.Barricade, "ReductionAmount");
            AnimateArtifact(ArtifactLabel.Barricade);
        }

        // If amount is still less than 0 (i.e., a negative number), the player is taking damage
        if (amount < 0)
        {
            OnPlayerRecieveDamage?.Invoke();
        }

        if (currentPlayerHP + amount > maxPlayerHP)
        {
            currentPlayerHP = maxPlayerHP;
        }
        else
        {
            currentPlayerHP += amount;
        }
        if (currentPlayerHP > 0)
        {
            return true;
        }
        return false;
    }

    public Character GetCharacter()
    {
        return playerCharacter;
    }

    public float GetCurrentCharacterHP()
    {
        return currentPlayerHP;
    }

    public float GetMaxPlayerHP()
    {
        return maxPlayerHP;
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
}
