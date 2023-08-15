using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ToolTipKeyword
{
    Affliction,
    Ward,
    Heal,
    Gold
}

public enum TextDecorationLabel
{
    Keyword,
    Affliction,
    Number
}

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

    [Header("Tool Tips")]
    [SerializeField] private int toolTipWidth;
    [SerializeField] private int toolTipHeight;
    [SerializeField] private float toolTipSpacing;
    [SerializeField] private float offset;

    [SerializeField] SerializableDictionary<TextDecorationLabel, Color> textDecorationColorMap = new SerializableDictionary<TextDecorationLabel, Color>();

    [SerializeField] private SerializableDictionary<string, string> keyWordData = new SerializableDictionary<string, string>();
    private List<string> afflictionTypes = new List<string>();

    public string GetKeyWordText(string keyword)
    {
        return keyWordData[keyword];
    }

    // Will Highlight text if it is an...
    // Affliction
    // Keyword
    // Number (Percent or Int)
    public string HighlightKeywords(string toolTipText)
    {
        string res = "";
        string[] toolTipTextTokens = toolTipText.Split(' ');
        for (int i = 0; i < toolTipTextTokens.Length; i++)
        {
            string token = toolTipTextTokens[i];

            int number;
            if (keyWordData.ContainsKey(token))
            {
                res += DecorateTextWithAppropriateColor(TextDecorationLabel.Keyword, token);
            }
            else if (afflictionTypes.Contains(token))
            {
                res += DecorateTextWithAppropriateColor(TextDecorationLabel.Affliction, token);
            }
            else if (int.TryParse(token, out number))
            {
                res += DecorateTextWithAppropriateColor(TextDecorationLabel.Number, number.ToString());
            }
            else if (token[token.Length - 1] == '%')
            {
                string sub = token.Substring(0, token.Length - 1);
                if (int.TryParse(sub, out number))
                {
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Number, token);
                }
                else
                {
                    res += token;
                }
            }
            else if (token[token.Length - 1] == ',')
            {
                string sub = token.Substring(0, token.Length - 1);
                if (int.TryParse(sub, out number))
                {
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Number, number.ToString()) + ",";
                }
                else
                {
                    res += token;
                }
            }
            else
            {
                res += token;
            }

            if (i < toolTipTextTokens.Length - 1)
            {
                res += " ";
            }
        }
        return res;
    }

    private string DecorateTextWithAppropriateColor(TextDecorationLabel label, string text)
    {
        return "<#" + ColorUtility.ToHtmlStringRGB(textDecorationColorMap[label]) + ">" + text + "</color>";
    }

    private void Awake()
    {
        _Instance = this;

        // Populate List
        foreach (AfflictionType type in Enum.GetValues(typeof(AfflictionType)))
        {
            afflictionTypes.Add(type.ToString());
        }
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
        return SpawnToolTips(book, transform, "On Use: ");
    }

    public GameObject SpawnToolTips(PowerupItem item, Transform transform, string toolTipTextPrefix = "")
    {
        ToolTipKeyword[] additionalKeyWords = item.Keywords;
        AfflictionType[] additionalAfflictionToolTips = item.AfflictionKeywords;
        int numToolTips = additionalKeyWords.Length + additionalAfflictionToolTips.Length + 1;

        ToolTipList list = SpawnToolTipList(transform, numToolTips);
        Transform vLayout = list.GetVerticalLayoutGroup().transform;
        SpawnToolTip(item.Name, toolTipTextPrefix + item.GetToolTipText(), vLayout);

        // Spawn ToolTips for Affliction Keywords
        for (int i = 0; i < additionalAfflictionToolTips.Length; i++)
        {
            AfflictionType affliction = additionalAfflictionToolTips[i];
            SpawnToolTip(affliction.ToString(), CombatManager._Instance.GetAfflictionOfType(affliction).GetToolTipText(), vLayout);
        }

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < additionalKeyWords.Length; i++)
        {
            ToolTipKeyword keyword = additionalKeyWords[i];
            SpawnToolTip(keyword.ToString(), GetKeyWordText(keyword.ToString()), vLayout);
        }

        return list.gameObject;
    }

    public GameObject SpawnToolTips(Affliction affliction, Transform transform)
    {
        ToolTipKeyword[] additionalKeyWords = affliction.Keywords;

        int numToolTips = additionalKeyWords.Length + 1;

        ToolTipList list = SpawnToolTipList(transform, numToolTips);
        Transform vLayout = list.GetVerticalLayoutGroup().transform;
        SpawnToolTip(affliction.ToString(), affliction.GetToolTipText(), vLayout);

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < additionalKeyWords.Length; i++)
        {
            ToolTipKeyword keyword = additionalKeyWords[i];
            SpawnToolTip(keyword.ToString(), GetKeyWordText(keyword.ToString()), vLayout);
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
        SpawnToolTip(spell.ToString(), spell.GetToolTipText(), vLayout);

        // Spawn ToolTips for Affliction Keywords
        for (int i = 0; i < additionalAfflictionToolTips.Length; i++)
        {
            AfflictionType affliction = additionalAfflictionToolTips[i];
            SpawnToolTip(affliction.ToString(), CombatManager._Instance.GetAfflictionOfType(affliction).GetToolTipText(), vLayout);
        }

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < additionalKeyWords.Length; i++)
        {
            ToolTipKeyword keyword = additionalKeyWords[i];
            SpawnToolTip(keyword.ToString(), GetKeyWordText(keyword.ToString()), vLayout);
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
        SpawnToolTip("", e.ToolTipText, vLayout);

        // Spawns ToolTips for Spells
        for (int i = 0; i < equipmentSpells.Count; i++)
        {
            SpellLabel label = equipmentSpells[i];
            Spell spell = GameManager._Instance.GetSpellOfType(equipmentSpells[i]);

            SpawnToolTip(label.ToString(), GameManager._Instance.GetSpellOfType(label).GetToolTipText(), vLayout);

            // Spawn ToolTips for Afflictions within Spells
            AfflictionType[] additionalAfflictionToolTips = spell.AfflictionKeywords;
            foreach (AfflictionType aff in additionalAfflictionToolTips)
            {
                if (additionalAfflictionKeywords.Contains(aff))
                {
                    SpawnToolTip(aff.ToString(), CombatManager._Instance.GetAfflictionOfType(aff).GetToolTipText(), vLayout);
                }
            }
        }

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < additionalKeyWords.Length; i++)
        {
            ToolTipKeyword keyword = additionalKeyWords[i];
            SpawnToolTip(keyword.ToString(), GetKeyWordText(keyword.ToString()), vLayout);
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

    private GameObject SpawnToolTip(string label, string content, Transform transform)
    {
        ToolTip spawned = Instantiate(toolTipPrefab, transform);
        RectTransform spawnedRect = spawned.GetComponent<RectTransform>();
        spawnedRect.sizeDelta = new Vector2(spawnedRect.sizeDelta.x, toolTipHeight);
        spawned.Set(label, content);
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
