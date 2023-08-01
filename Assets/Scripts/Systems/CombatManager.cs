using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using TMPro;

public enum Target
{
    Character,
    Enemy
}

public enum AfflictionType
{
    Emboldened,
    Weakened,
    Vulnerable,
    Guarded,
    Bandaged,
    Retribution,
    Prepared,
    Parry,
    Echo,
    Blight,
    Burn,
    Paralyzed,
}

public enum AfflictionSetType
{
    Activations,
    Duration
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager _Instance { get; private set; }

    private void Awake()
    {
        _Instance = this;
    }

    [Header("Objects")]
    [SerializeField] private GameObject Circle; // Circle Object

    [SerializeField] private bool enableSliders = true;

    const int SPAWN = -100; // Spawn coordinates for objects

    private double timer = 0; // Main song timer
    [SerializeField] private int apprRate = 600; // Approach rate (in ms)
    private int delayPos = 0; // Delay song position

    private int noteCount = 0; // Notes played counter
    private int objCount = 0; // Spawned objects counter

    private List<GameObject> circleList; // Circles List
    private static string[] lineParams; // Object Parameters

    // Audio stuff
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    // Other stuff
    [SerializeField] private LayerMask noteLayer;
    [SerializeField] private GameObject cursorTrail;
    [SerializeField] private Image background;
    private Vector3 mousePosition;
    private Ray mainRay;
    private RaycastHit mainHit;

    private AudioClip hitSound;
    private AudioClip missSound;
    private Enemy currentEnemy;
    private float currentEnemyHP;
    private float maxEnemyHP;

    [Header("Settings")]
    [SerializeField] private bool playSFXOnMiss;

    [SerializeField] private TextMeshProUGUI characterHP;
    [SerializeField] private TextMeshProUGUI enemyHP;
    [SerializeField] private Image characterCombatSprite;
    [SerializeField] private Image enemyCombatSprite;

    public Action OnPlayerAttack;
    public Action OnEnemyAttack;
    public Action OnCombatStart;
    public Dictionary<Action, float> OnCombatStartDelayedActionMap = new Dictionary<Action, float>();
    public Dictionary<Action, RepeatData> OnCombatStartRepeatedActionMap = new Dictionary<Action, RepeatData>();
    public Action OnCombatEnd;
    public Action OnPassiveSpellProc;
    public Action OnActiveSpellActivated;
    public Action OnCharacterGainAffliction;
    public Action OnCharacterLoseAffliction;
    public Action OnEnemyGainAffliction;
    public Action OnEnemyLoseAffliction;

    private Dictionary<AfflictionType, Affliction> characterAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private Dictionary<AfflictionType, Affliction> enemyAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private List<AfflictionType> toClearFromAffMap = new List<AfflictionType>();

    private List<Coroutine> onStartCombatCoroutines = new List<Coroutine>();

    public void AddAffliction(AfflictionType type, float num, AfflictionSetType setType, Target target)
    {
        switch (target)
        {
            case Target.Character:

                if (type == AfflictionType.Weakened && GameManager._Instance.HasArtifact(ArtifactLabel.SpecialSpinich))
                {
                    return;
                }

                if (type == AfflictionType.Vulnerable && GameManager._Instance.HasArtifact(ArtifactLabel.HolyShield))
                {
                    return;
                }

                switch (setType)
                {
                    case AfflictionSetType.Activations:
                        if (AddAffliction(type, (int)num, characterAfflictionMap))
                        {
                            // Character was given new Affliction
                            OnCharacterGainAffliction?.Invoke();
                        }
                        break;
                    case AfflictionSetType.Duration:
                        if (AddAffliction(type, num, characterAfflictionMap))
                        {
                            // Character was given new Affliction
                            OnCharacterGainAffliction?.Invoke();
                        }
                        break;
                }
                break;
            case Target.Enemy:
                switch (setType)
                {
                    case AfflictionSetType.Activations:
                        if (AddAffliction(type, (int)num, enemyAfflictionMap))
                        {
                            // Enemy was given new Affliction
                            OnEnemyGainAffliction?.Invoke();
                        }
                        break;
                    case AfflictionSetType.Duration:
                        if (AddAffliction(type, num, enemyAfflictionMap))
                        {
                            // Enemy was given new Affliction
                            OnEnemyGainAffliction?.Invoke();
                        }
                        break;
                }
                break;
        }
    }

    private bool AddAffliction(AfflictionType type, float duration, Dictionary<AfflictionType, Affliction> map)
    {
        //  Affliction Map already contains
        if (map.ContainsKey(type))
        {
            map[type].AlterDuration(duration);
            return false;
        }
        else
        {
            Affliction aff = new Affliction();
            aff.SetDuration(duration);
            map.Add(type, aff);
            return true;
        }
    }

    private bool AddAffliction(AfflictionType type, int activations, Dictionary<AfflictionType, Affliction> map)
    {
        // Affliction Map already contains
        if (map.ContainsKey(type))
        {
            map[type].AlterActivations(activations);
            return false;
        }
        else
        {
            Affliction aff = new Affliction();
            aff.SetActivations(activations);
            map.Add(type, aff);
            return true;
        }
    }

    private void UpdateAfflictionMaps()
    {
        UpdateAfflictionMap(characterAfflictionMap, Target.Character);
        UpdateAfflictionMap(enemyAfflictionMap, Target.Enemy);
    }


    private void ClearAfflictionMaps()
    {
        enemyAfflictionMap.Clear();
        characterAfflictionMap.Clear();
    }

    private void ResetCombat()
    {
        ClearAfflictionMaps();

        foreach (Coroutine c in onStartCombatCoroutines)
        {
            StopCoroutine(c);
        }
        onStartCombatCoroutines.Clear();

        musicSource.Stop();
        musicSource.time = 0;
        objCount = 0;
        noteCount = 0;

        // Destroy Circles
        while (circleList.Count > 0)
        {
            GameObject circle = circleList[0];
            circleList.RemoveAt(0);
            Destroy(circle);
        }
    }

    private void UpdateAfflictionMap(Dictionary<AfflictionType, Affliction> map, Target target)
    {
        toClearFromAffMap.Clear();
        foreach (KeyValuePair<AfflictionType, Affliction> kvp in map)
        {
            Affliction a = kvp.Value;

            if (a.TickAway)
                a.AlterDuration(-Time.deltaTime);

            if (a.CanBeCleared)
            {
                toClearFromAffMap.Add(kvp.Key);
            }
        }

        foreach (AfflictionType toClear in toClearFromAffMap)
        {
            map.Remove(toClear);
            switch (target)
            {
                case Target.Character:
                    OnCharacterLoseAffliction?.Invoke();
                    break;
                case Target.Enemy:
                    OnEnemyLoseAffliction?.Invoke();
                    break;
            }
        }
    }

    public void ReduceActiveSpellCDsByPercent(float normalizedPercent)
    {
        // normaliedPercent is some number between 0 and 1
        // 0 = 0%, 1 = 100%
        // .14 = 14%
        // etc
    }

    public void TriggerRandomPassiveSpell()
    {

    }

    public void ReleaseHalfLitFirework()
    {

    }

    private void Start()
    {
        circleList = new List<GameObject>();
    }

    public void OnNoteHit()
    {
        sfxSource.PlayOneShot(hitSound);
        noteCount++;

        PlayerAttack();
    }

    public void OnNoteMiss()
    {
        if (playSFXOnMiss)
            sfxSource.PlayOneShot(missSound);
        noteCount++;

        EnemyAttack();
    }

    public void AltarEnemyHP(float amount)
    {
        if (GameManager._Instance.HasArtifact(ArtifactLabel.DoctorsReport) && amount > ArtifactManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "MustBeOver"))
        {
            AddAffliction(AfflictionType.Bandaged, ArtifactManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "StackAmount"), characterAfflictionMap);
            GameManager._Instance.AnimateArtifact(ArtifactLabel.DoctorsReport);
        }

        if (currentEnemyHP + amount > maxEnemyHP)
        {
            currentEnemyHP = maxEnemyHP;
        }
        else
        {
            currentEnemyHP += amount;
            if (currentEnemyHP <= 0)
            {
                // Enemy Died
                CallOnEndCombat();
            }
        }
    }

    private void PlayerAttack()
    {
        // Simple way of attacking for now
        AltarEnemyHP(-GameManager._Instance.GetCharacter().GetBasicAttackDamage());

        OnPlayerAttack?.Invoke();
    }

    private void EnemyAttack()
    {
        // Simple way of attacking for now
        if (!GameManager._Instance.AlterPlayerHP(-currentEnemy.GetBasicAttackDamage()))
        {
            // Player Died
            GameManager._Instance.GameOver();
        }
        else
        {
            OnEnemyAttack?.Invoke();
        }
    }

    public double GetTimer()
    {
        return timer;
    }

    public int GetApprRate()
    {
        return apprRate;
    }

    void ReadCircles(string path)
    {
        StreamReader reader = new StreamReader(path);
        string line;

        // Skip to [HitObjects] part
        while (true)
        {
            if (reader.ReadLine() == "[HitObjects]")
                break;
        }

        int TotalLines = 0;

        // Count all lines
        while (!reader.EndOfStream)
        {
            reader.ReadLine();
            TotalLines++;
        }

        reader.Close();

        reader = new StreamReader(path);

        // Skip to [HitObjects] part again
        while (true)
        {
            if (reader.ReadLine() == "[HitObjects]")
                break;
        }

        // Sort objects on load
        int ForeOrder = TotalLines + 2; // Sort foreground layer
        int BackOrder = TotalLines + 1; // Sort background layer
        int ApproachOrder = TotalLines; // Sort approach circles layer

        // Some crazy Z axis modifications for sorting
        string TotalLinesStr = "0.";
        for (int i = 3; i > TotalLines.ToString().Length; i--)
            TotalLinesStr += "0";
        TotalLinesStr += TotalLines.ToString();
        float Z_Index = -(float.Parse(TotalLinesStr));

        while (!reader.EndOfStream)
        {
            // Uncomment to skip sliders
            if (!enableSliders)
            {
                while (true)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (!line.Contains("|"))
                            break;
                    }
                    else
                        break;
                }
            }

            line = reader.ReadLine();
            if (line == null)
                break;

            lineParams = line.Split(','); // Line parameters (X&Y axis, time position)

            // Sorting configuration
            GameObject circleObject = Instantiate(Circle, new Vector2(SPAWN, SPAWN), Quaternion.identity);
            Circle circle = circleObject.GetComponent<Circle>();
            circle.GetFore().sortingOrder = ForeOrder;
            circle.GetBack().sortingOrder = BackOrder;
            circle.GetAppr().sortingOrder = ApproachOrder;
            circleObject.transform.localPosition += new Vector3((float)circleObject.transform.localPosition.x, (float)circleObject.transform.localPosition.y, (float)Z_Index);
            circleObject.transform.SetAsFirstSibling();
            ForeOrder--; BackOrder--; ApproachOrder--; Z_Index += 0.01f;

            int FlipY = 384 - int.Parse(lineParams[1]); // Flip Y axis

            int AdjustedX = Mathf.RoundToInt(Screen.height * 1.333333f); // Aspect Ratio

            // Padding
            float slices = 8f;
            float paddingX = AdjustedX / slices;
            float paddingY = Screen.height / slices;

            // Resolution set
            float newRangeX = ((AdjustedX - paddingX) - paddingX);
            float newValueX = ((int.Parse(lineParams[0]) * newRangeX) / 512f) + paddingX + ((Screen.width - AdjustedX) / 2f);
            float newRangeY = Screen.height;
            float newValueY = ((FlipY * newRangeY) / 512f) + paddingY;

            Vector3 mainPos = Camera.main.ScreenToWorldPoint(new Vector3(newValueX, newValueY, 0)); // Convert from screen position to world position
            Circle mainCircle = circleObject.GetComponent<Circle>();

            mainCircle.Set(mainPos.x, mainPos.y, circleObject.transform.position.z, int.Parse(lineParams[2]) - apprRate);

            circleList.Add(circleObject);
        }
    }

    private IEnumerator UpdateRoutine()
    {
        while (true)
        {
            // Show Enemy HP
            enemyHP.text = currentEnemyHP + "/" + maxEnemyHP;
            characterHP.text = GameManager._Instance.GetCurrentCharacterHP() + "/" + GameManager._Instance.GetMaxPlayerHP();
            UpdateAfflictionMaps();

            if (currentEnemyHP <= 0 || GameManager._Instance.GetCurrentCharacterHP() <= 0)
            {
                break;
            }

            timer = (musicSource.time * 1000); // Convert timer
            delayPos = (circleList[objCount].GetComponent<Circle>().posA);
            mainRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Spawn object
            if (timer >= delayPos)
            {
                circleList[objCount].GetComponent<Circle>().Spawn();
                objCount++;
            }

            // Check if cursor is over object
            if (Physics.Raycast(mainRay, out mainHit))
            {
                if (LayerMaskHelper.IsInLayerMask(mainHit.collider.gameObject, noteLayer) && timer >= mainHit.collider.gameObject.GetComponent<Circle>().posA + apprRate)
                {
                    mainHit.collider.gameObject.GetComponent<Circle>().Got();
                    mainHit.collider.enabled = false;
                    noteCount++;

                }
            }

            // Cursor trail movement
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorTrail.transform.position = new Vector3(mousePosition.x, mousePosition.y, -9);

            yield return null;
        }
    }

    private void CallOnStartCombat()
    {
        OnCombatStart?.Invoke();

        foreach (KeyValuePair<Action, float> kvp in OnCombatStartDelayedActionMap)
        {
            Debug.Log("Starting: " + kvp.Key + ", Delay = " + kvp.Value);
            onStartCombatCoroutines.Add(StartCoroutine(Utils.CallActionAfterDelay(kvp.Key, kvp.Value)));
        }

        foreach (KeyValuePair<Action, RepeatData> kvp in OnCombatStartRepeatedActionMap)
        {
            Debug.Log("Starting: " + kvp.Key + ", Repetitions = " + kvp.Value.Repetitions + ", Delay = " + kvp.Value.Delay);
            for (int i = 1; i <= kvp.Value.Repetitions; i++)
            {
                onStartCombatCoroutines.Add(StartCoroutine(Utils.CallActionAfterDelay(kvp.Key, kvp.Value.Delay * i)));
            }
        }
    }

    private void CallOnEndCombat()
    {
        OnCombatEnd?.Invoke();
    }

    public void AddOnCombatStartDelayedAction(Action a, float delay)
    {
        Debug.Log("Added: " + a + ", Delay = " + delay);
        OnCombatStartDelayedActionMap.Add(a, delay);
    }

    public void RemoveOnCombatStartDelayedAction(Action a)
    {
        OnCombatStartDelayedActionMap.Remove(a);
    }

    public void AddOnCombatStartRepeatedAction(Action a, RepeatData data)
    {
        Debug.Log("Starting: " + a + ", Repetitions = " + data.Repetitions + ", Delay = " + data.Delay);
        OnCombatStartRepeatedActionMap.Add(a, data);
    }

    public void RemoveOnCombatStartRepeatedAction(Action a)
    {
        OnCombatStartRepeatedActionMap.Remove(a);
    }

    public IEnumerator StartCombat(Combat combat)
    {
        Debug.Log("Combat Started: " + combat);

        // Set Up Combat
        musicSource.clip = combat.MainMusic;
        ReadCircles(AssetDatabase.GetAssetPath(combat.MapFile));
        hitSound = combat.HitSound;
        missSound = combat.MissSound;

        currentEnemy = combat.Enemy;
        maxEnemyHP = currentEnemy.GetMaxHP();
        currentEnemyHP = maxEnemyHP;
        enemyCombatSprite.sprite = currentEnemy.GetCombatSprite();
        characterCombatSprite.sprite = GameManager._Instance.GetCharacter().GetCombatSprite();

        CallOnStartCombat();

        musicSource.Play();

        yield return StartCoroutine(UpdateRoutine());

        // Reset
        ResetCombat();

        GameManager._Instance.ResolveCurrentEvent();
        Debug.Log("Combat Completed: " + combat);
    }
}
