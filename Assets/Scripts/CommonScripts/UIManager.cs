using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private bool paused;

    public static UIManager _Instance { get; set; }

    private int UILayer = 5;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private Transform parentToolTipsTo;

    [Header("Prefabs")]
    [SerializeField] private ToolTipList toolTipList;
    [SerializeField] private ToolTip toolTipPrefab;

    [SerializeField] private int toolTipWidth;
    [SerializeField] private int toolTipHeight;
    [SerializeField] private float toolTipSpacing;
    [SerializeField] private float offset;

    private void Awake()
    {
        _Instance = this;
    }

    public void Restart()
    {
        TransitionManager._Instance.FadeOut(delegate
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    private void TogglePauseState()
    {
        if (paused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        paused = true;

        Time.timeScale = 0;

        pauseScreen.SetActive(true);
    }

    public void Unpause()
    {
        paused = false;

        Time.timeScale = 1;

        pauseScreen.SetActive(false);
    }

    public void GoToMainMenu()
    {
        GoToLevel(0);
    }

    public void Exit()
    {
        TransitionManager._Instance.FadeOut(() => Application.Quit());
    }

    public void GoToLevel(int buildIndex)
    {
        TransitionManager._Instance.FadeOut(delegate
        {
            if (paused)
                Unpause();
            SceneManager.LoadScene(buildIndex);
        });
    }

    public void GoToLevel(string sceneName)
    {
        TransitionManager._Instance.FadeOut(delegate
        {
            if (paused)
                Unpause();
            SceneManager.LoadScene(sceneName);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseState();
        }
    }

    public GameObject SpawnToolTipsForBook(Book book, Transform transform)
    {
        return SpawnToolTips(book, transform, "On Use: \n");
    }

    public GameObject SpawnToolTips(PowerupItem item, Transform transform, string toolTipTextPrefix = "")
    {
        ToolTipKeyword[] additionalKeyWords = item.Keywords;
        AfflictionType[] additionalAfflictionToolTips = item.AfflictionKeywords;
        int numToolTips = additionalKeyWords.Length + additionalAfflictionToolTips.Length + 1;

        ToolTipList list = SpawnToolTipList(transform, numToolTips);
        Transform vLayout = list.GetVerticalLayoutGroup().transform;
        SpawnToolTip(toolTipTextPrefix + item.ToolTipText, vLayout);

        // Spawn ToolTips for Affliction Keywords
        for (int i = 0; i < additionalAfflictionToolTips.Length; i++)
        {
            AfflictionType affliction = additionalAfflictionToolTips[i];
            SpawnToolTip(affliction + ": " + CombatManager._Instance.GetAfflictionOfType(affliction).ToolTipText, vLayout);
        }

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < additionalKeyWords.Length; i++)
        {
            ToolTipKeyword keyword = additionalKeyWords[i];
            SpawnToolTip(keyword + ": " + GameManager._Instance.GetKeyWordText(keyword), vLayout);
        }

        return list.gameObject;
    }

    public GameObject SpawnToolTips(Affliction affliction, Transform transform, string toolTipTextPrefix = "")
    {
        ToolTipKeyword[] additionalKeyWords = affliction.Keywords;

        int numToolTips = additionalKeyWords.Length + 1;

        ToolTipList list = SpawnToolTipList(transform, numToolTips);
        Transform vLayout = list.GetVerticalLayoutGroup().transform;
        SpawnToolTip(toolTipTextPrefix + affliction.ToolTipText, vLayout);

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < additionalKeyWords.Length; i++)
        {
            ToolTipKeyword keyword = additionalKeyWords[i];
            SpawnToolTip(keyword + ": " + GameManager._Instance.GetKeyWordText(keyword), vLayout);
        }

        return list.gameObject;
    }

    public GameObject SpawnToolTips(Spell spell, Transform transform)
    {
        ToolTipKeyword[] additionalKeyWords = spell.Keywords;
        AfflictionType[] additionalAfflictionToolTips = spell.AfflictionKeywords;
        int numToolTips = additionalKeyWords.Length + additionalAfflictionToolTips.Length + 1;
        Debug.Log(numToolTips);

        ToolTipList list = SpawnToolTipList(transform, numToolTips);
        Transform vLayout = list.GetVerticalLayoutGroup().transform;
        SpawnToolTip(spell.ToolTipText, vLayout);

        // Spawn ToolTips for Affliction Keywords
        for (int i = 0; i < additionalAfflictionToolTips.Length; i++)
        {
            AfflictionType affliction = additionalAfflictionToolTips[i];
            SpawnToolTip(affliction + ": " + CombatManager._Instance.GetAfflictionOfType(affliction).ToolTipText, vLayout);
        }

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < additionalKeyWords.Length; i++)
        {
            ToolTipKeyword keyword = additionalKeyWords[i];
            SpawnToolTip(keyword + ": " + GameManager._Instance.GetKeyWordText(keyword), vLayout);
        }

        return list.gameObject;
    }

    public GameObject SpawnToolTips(Equipment e, Transform transform)
    {
        ToolTipKeyword[] additionalKeyWords = e.Keywords;
        List<SpellLabel> equipmentSpells = e.ComesWithSpells;
        List<AfflictionType> additionalAfflictionKeywords = new List<AfflictionType>();
        for (int i = 0; i < equipmentSpells.Count; i++)
        {
            Spell spell = GameManager._Instance.GetSpellOfType(equipmentSpells[i]);
            AfflictionType[] additionalAfflictionToolTips = spell.AfflictionKeywords;
            foreach (AfflictionType aff in additionalAfflictionToolTips)
            {
                if (!additionalAfflictionKeywords.Contains(aff))
                {
                    additionalAfflictionKeywords.Add(aff);
                }
            }
        }
        int numToolTips = additionalKeyWords.Length + equipmentSpells.Count + additionalAfflictionKeywords.Count + 1;

        ToolTipList list = SpawnToolTipList(transform, numToolTips);
        Transform vLayout = list.GetVerticalLayoutGroup().transform;
        SpawnToolTip(e.ToolTipText, vLayout);

        // Spawns ToolTips for Spells
        for (int i = 0; i < equipmentSpells.Count; i++)
        {
            SpellLabel label = equipmentSpells[i];
            Spell spell = GameManager._Instance.GetSpellOfType(equipmentSpells[i]);

            SpawnToolTip(label + ": " + GameManager._Instance.GetSpellOfType(label).ToolTipText, vLayout);

            // Spawn ToolTips for Afflictions within Spells
            AfflictionType[] additionalAfflictionToolTips = spell.AfflictionKeywords;
            foreach (AfflictionType aff in additionalAfflictionToolTips)
            {
                if (additionalAfflictionKeywords.Contains(aff))
                {
                    SpawnToolTip(aff + ": " + CombatManager._Instance.GetAfflictionOfType(aff).ToolTipText, vLayout);
                }
            }
        }

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < additionalKeyWords.Length; i++)
        {
            ToolTipKeyword keyword = additionalKeyWords[i];
            SpawnToolTip(keyword + " : " + GameManager._Instance.GetKeyWordText(keyword), vLayout);
        }

        return list.gameObject;
    }

    private ToolTipList SpawnToolTipList(Transform spawningFor, int numToolTips)
    {
        RectTransform spawningForRect = spawningFor.GetComponent<RectTransform>();
        float spawningForWidth = spawningForRect.sizeDelta.x;
        float spawningForHeight = spawningForRect.sizeDelta.y;

        // Determine where on the screen the mouse is
        int horizontalSlice = 0;
        int verticalSlice = 0;

        // Get Either Left, Middle, or Right of Screen
        if (Input.mousePosition.x < Screen.width / 3)
        {
            // Left
            horizontalSlice = 0;
        }
        else if (Input.mousePosition.x >= Screen.width / 3 && Input.mousePosition.x < (Screen.width / 3) * 2)
        {
            // Middle
            horizontalSlice = 1;
        }
        else
        {
            // Right
            horizontalSlice = 2;
        }

        // Get Either Top or Bottom of Screen
        if (Input.mousePosition.y > Screen.height / 2)
        {
            // Top
            verticalSlice = 0;
        }
        else
        {
            // Bottom
            verticalSlice = 1;
        }

        // Spawn Tool Tip List
        ToolTipList spawned = Instantiate(toolTipList, spawningFor);
        RectTransform listRect = spawned.GetRect();

        // Calculate List Height
        float listHeight = (numToolTips * toolTipHeight) + ((numToolTips - 1) * toolTipSpacing);
        listRect.sizeDelta = new Vector2(toolTipWidth, listHeight);

        // Set vList Spacing
        VerticalLayoutGroup vLayout = spawned.GetVerticalLayoutGroup();
        vLayout.spacing = toolTipSpacing;

        // Depending on where the mouse is, splitting the screen up into two rows of 3 (so six sections total), we set the Layout Groups Child Alignment Property
        // Top Left = Upper Left
        // Top Middle = Upper Center
        // Top Right = Upper Right
        // Bottom Left = Lower Left
        // Bottom Middle = Lower Center
        // Bottom Right = Lower Right

        // Upper Left -> (-parentHeight / 2) (-numToolTips * toolTipHeight) (-numToolTIps - 1 * toolTipSpacing) (-offsetY)

        float verticalOffset = (spawningForHeight / 2) + (listHeight / 2) + this.offset;
        float horizontalOffset = (toolTipWidth - spawningForWidth) / 2;
        switch (verticalSlice)
        {
            case 0:
                if (horizontalSlice == 0)
                {
                    // Upper Left
                    vLayout.childAlignment = TextAnchor.UpperLeft;
                    verticalOffset = -verticalOffset;
                }
                else if (horizontalSlice == 1)
                {
                    // Upper Center
                    vLayout.childAlignment = TextAnchor.UpperCenter;
                    verticalOffset = -verticalOffset;
                    horizontalOffset = 0;
                }
                else
                {
                    // Upper Right
                    vLayout.childAlignment = TextAnchor.UpperRight;
                    horizontalOffset = -horizontalOffset;
                    verticalOffset = -verticalOffset;
                }
                break;
            case 1:
                if (horizontalSlice == 0)
                {
                    // Upper Left
                    vLayout.childAlignment = TextAnchor.LowerLeft;
                }
                else if (horizontalSlice == 1)
                {
                    // Lower Center
                    vLayout.childAlignment = TextAnchor.LowerCenter;
                    horizontalOffset = 0;
                }
                else
                {
                    // Lower Right
                    vLayout.childAlignment = TextAnchor.LowerRight;
                    horizontalOffset = -horizontalOffset;
                }
                break;
        }
        // Set Position
        Vector3 offset = new Vector3(horizontalOffset, verticalOffset, 0);
        listRect.localPosition = offset;
        // Debug.Log("X: " + horizontalSlice + ", Y: " + verticalSlice);
        // Debug.Log(spawningFor.position + ", Horizontal Offset: " + horizontalOffset + ", Vertical Offset: " + verticalOffset + ", Final Position: " + listRect.position);

        return spawned;
    }

    private GameObject SpawnToolTip(string text, Transform transform)
    {
        ToolTip spawned = Instantiate(toolTipPrefab, transform);
        RectTransform spawnedRect = spawned.GetComponent<RectTransform>();
        spawnedRect.sizeDelta = new Vector2(spawnedRect.sizeDelta.x, toolTipHeight);
        spawned.Set(text);
        return spawned.gameObject;
    }

    public void SavePlayerPrefs()
    {
        PlayerPrefs.Save();
    }

    [ContextMenu("ClearPlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
