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
    private Dictionary<string, Image> artifactDisplayTracker = new Dictionary<string, Image>();

    [Header("References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Transform artifactBar;

    private Action onSuccessfulNoteHit;
    private Action onFailedNoteHit;
    private Action onRecievedDamage;
    private Action onDealtDamage;
    private Action onSongStart;
    private Action onSongEnd;

    [Header("Prefabs")]
    [SerializeField] private Image artifactDisplay;

    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        TryAddPersistentTokens();

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
                Artifact a = testArtifacts[0];
                testArtifacts.RemoveAt(0);
                AddArtifact(a);
            }
        }
    }

    [SerializeField] private List<Artifact> testArtifacts;

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

    public void AddArtifact(Artifact artifact)
    {
        playerArtifacts.Add(artifact);

        Image spawned = Instantiate(artifactDisplay, artifactBar);
        spawned.sprite = artifact.GetSprite();

        artifactDisplayTracker.Add(artifact.GetIdentifier(), spawned);

        // Somehow evaluate what artifact must do and add it's effect
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

    public void AlterCurrency(float amount)
    {
        currentPlayerCurrency += amount;
    }

    public void AlterPlayerHP(float amount)
    {
        currentPlayerHP += amount;
    }
}
