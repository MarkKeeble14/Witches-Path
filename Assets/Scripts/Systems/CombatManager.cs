using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using TMPro;

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
    private int ObjCount = 0; // Spawned objects counter

    private List<GameObject> circleList; // Circles List
    private static string[] lineParams; // Object Parameters

    // Audio stuff
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    // Other stuff
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

    private void PlayerAttack()
    {
        // Simple way of attacking for now
        currentEnemyHP -= GameManager._Instance.GetCharacter().GetBasicAttackDamage();
    }


    private void EnemyAttack()
    {
        // Simple way of attacking for now
        GameManager._Instance.AlterPlayerHP(-currentEnemy.GetBasicAttackDamage());
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


    private void GameStart()
    {
        Application.targetFrameRate = -1; // Unlimited framerate
        musicSource.Play();
        StartCoroutine(UpdateRoutine()); // Using coroutine instead of Update()
    }

    private IEnumerator UpdateRoutine()
    {
        while (true)
        {
            // Show Enemy HP
            enemyHP.text = currentEnemyHP + "/" + maxEnemyHP;
            characterHP.text = GameManager._Instance.GetCurrentCharacterHP() + "/" + GameManager._Instance.GetMaxPlayerHP();

            timer = (musicSource.time * 1000); // Convert timer
            delayPos = (circleList[ObjCount].GetComponent<Circle>().posA);
            mainRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Spawn object
            if (timer >= delayPos)
            {
                circleList[ObjCount].GetComponent<Circle>().Spawn();
                ObjCount++;

            }

            // Check if cursor is over object
            if (Physics.Raycast(mainRay, out mainHit))
            {
                if (mainHit.collider.name == "Circle(Clone)" && timer >= mainHit.collider.gameObject.GetComponent<Circle>().posA + apprRate)
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

        GameStart();

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        GameManager._Instance.ResolveCurrentEvent();
        Debug.Log("Combat Completed: " + combat);
    }

    public IEnumerator StartBossCombat(BossCombat combat)
    {
        Debug.Log("Boss Combat Started: " + combat);

        musicSource.clip = combat.MainMusic;
        ReadCircles(AssetDatabase.GetAssetPath(combat.MapFile));
        hitSound = combat.HitSound;
        missSound = combat.MissSound;

        GameStart();

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        GameManager._Instance.ResolveCurrentEvent();
        Debug.Log("Combat Completed: " + combat);
    }
}
