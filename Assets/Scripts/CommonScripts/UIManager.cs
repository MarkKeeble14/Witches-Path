using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct SpellColorInfo
{
    [SerializeField] private Color color;
    [SerializeField] private Color textColor;

    public Color Color => color;
    public Color TextColor => textColor;
}

[System.Serializable]
public struct UISectionInformation
{
    [SerializeField] private string text;
    [SerializeField] private Sprite icon;
    [SerializeField] private Color color;

    public string Text { get => text; }
    public Sprite Icon { get => icon; }
    public Color Color { get => color; }
}

public struct ToolTippableComparisonData
{
    public string AdditionalLabel;
    public ToolTippable ToolTippable;

    public ToolTippableComparisonData(string additionalLabel, ToolTippable toolTippable)
    {
        AdditionalLabel = additionalLabel;
        ToolTippable = toolTippable;
    }
}

public interface ToolTippable
{
    public List<AfflictionType> GetAfflictionKeyWords();

    public List<ToolTipKeyword> GetGeneralKeyWords();

    public List<ToolTippable> GetOtherToolTippables();

    public string GetToolTipLabel();

    public string GetToolTipText();
}

public enum ToolTipKeyword
{
    Affliction,
    Ward,
    Heal,
    Gold,
    Charge,
    Health,
    Mana,
    Damage,
    Defense,
    Pelts,
    PotionBase,
    PotionTargeter,
    PotionPotency,
    PotionAugmenter,
    Multiplier
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
    [SerializeField] private ConfirmPotionToolTip confirmPotionToolTipPrefab;
    [SerializeField] private VisualSpellDisplay spellToolTipPrefab;

    [Header("Tool Tips")]
    [SerializeField] private int toolTipWidth;
    [SerializeField] private int toolTipHeight;

    [SerializeField] private float toolTipSpacing;
    [SerializeField] private float offset;

    [SerializeField] private float confirmPotionUseToolTipWidth = 250;
    [SerializeField] private float confirmPotionUseToolTipHeight = 100;

    [SerializeField] private float spellToolTipWidth = 250;
    [SerializeField] private float spellToolTipHeight = 300;

    [SerializeField] SerializableDictionary<TextDecorationLabel, Color> textDecorationColorMap = new SerializableDictionary<TextDecorationLabel, Color>();

    [SerializeField] private SerializableDictionary<string, string> keyWordData = new SerializableDictionary<string, string>();
    private List<string> afflictionTypes = new List<string>();

    private List<string> numericalSuffixes => new List<string>() { "st", "nd", "rd", "th" };

    [SerializeField] private SerializableDictionary<UISection, UISectionInformation> UISectionMap = new SerializableDictionary<UISection, UISectionInformation>();

    [SerializeField] private SerializableDictionary<DamageType, Color> damageTypeColorMap = new SerializableDictionary<DamageType, Color>();

    [SerializeField] private SerializableDictionary<PotionIngredientCategory, Sprite> potionIngredientCategorySpriteMap = new SerializableDictionary<PotionIngredientCategory, Sprite>();

    [SerializeField] private SerializableDictionary<Rarity, Color> rarityColorMap = new SerializableDictionary<Rarity, Color>();

    [SerializeField] private SerializableDictionary<SpellColor, SpellColorInfo> spellColorMap = new SerializableDictionary<SpellColor, SpellColorInfo>();

    [SerializeField] private SerializableDictionary<AfflictionType, Sprite> afflictionIconDict = new SerializableDictionary<AfflictionType, Sprite>();

    [SerializeField] private Transform canvas;
    public Transform Canvas => canvas;

    [SerializeField] private SerializableDictionary<string, Color> effectTextColors = new SerializableDictionary<string, Color>();

    [SerializeField] private SerializableDictionary<string, string[]> multiTokenAfflictions = new SerializableDictionary<string, string[]>();
    [SerializeField] private SerializableDictionary<string, string[]> multiTokenKeywords = new SerializableDictionary<string, string[]>();

    public Color GetEffectTextColor(string key)
    {
        return effectTextColors[key];
    }

    public SpellColorInfo GetSpellColor(SpellColor color)
    {
        return spellColorMap[color];
    }

    public Color GetRarityColor(Rarity r)
    {
        return rarityColorMap[r];
    }

    public Sprite GetPotionIngredientCategorySprite(PotionIngredientCategory category)
    {
        return potionIngredientCategorySpriteMap[category];
    }

    public Color GetDamageTypeColor(DamageType type)
    {
        return damageTypeColorMap[type];
    }

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

            // Add space
            if (i > 0)
            {
                res += " ";
            }

            // Token is a Keyword
            if (keyWordData.ContainsKey(token))
            {
                res += DecorateTextWithAppropriateColor(TextDecorationLabel.Keyword, token);
                continue;
            }

            // Token is an Keyword with Multiple Tokens
            if (multiTokenKeywords.ContainsKey(token))
            {
                string[] multiTokens = multiTokenKeywords[token];
                bool valid = true;
                for (int p = 0; p < multiTokens.Length; p++)
                {
                    if (i + p > toolTipTextTokens.Length - 1)
                    {
                        valid = false;
                        break;
                    }
                    if (toolTipTextTokens[i + p] != multiTokens[p])
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    string fullText = "";
                    for (int p = 0; p < multiTokens.Length; p++)
                    {
                        fullText += multiTokens[p];
                        if (p < multiTokens.Length - 1)
                        {
                            fullText += " ";
                        }
                    }
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Keyword, fullText);
                    i += multiTokens.Length - 1;
                    continue;
                }
            }

            // Token is an Affliction
            if (afflictionTypes.Contains(token) && !multiTokenAfflictions.ContainsKey(token))
            {
                res += DecorateTextWithAppropriateColor(TextDecorationLabel.Affliction, token);
                continue;
            }

            // Token is an Affliction with Multiple Tokens
            if (multiTokenAfflictions.ContainsKey(token))
            {
                string[] multiTokens = multiTokenAfflictions[token];
                bool valid = true;
                for (int p = 0; p < multiTokens.Length; p++)
                {
                    if (i + p > toolTipTextTokens.Length - 1)
                    {
                        valid = false;
                        break;
                    }
                    if (toolTipTextTokens[i + p] != multiTokens[p])
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    string fullText = "";
                    for (int p = 0; p < multiTokens.Length; p++)
                    {
                        fullText += multiTokens[p];
                        if (p < multiTokens.Length - 1)
                        {
                            fullText += " ";
                        }
                    }
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Affliction, fullText);
                    i += multiTokens.Length - 1;
                    continue;
                }
            }

            // Token has a comma at the end which might interfere with determining if it's a keyword or not
            if (token.Length > 1 && token[token.Length - 1].Equals(','))
            {
                string sub = token.Substring(0, token.Length - 1);

                // Token was in fact a Key Word
                if (keyWordData.ContainsKey(sub))
                {
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Keyword, sub) + ",";
                    continue;

                }

                // Token was in fact an Affliction
                if (afflictionTypes.Contains(sub))
                {
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Affliction, sub) + ",";
                    continue;
                }
            }

            // Token is a Number
            if (int.TryParse(token, out number))
            {
                res += DecorateTextWithAppropriateColor(TextDecorationLabel.Number, number.ToString());
                continue;
            }

            // Token is a Number but with a Percent (%)
            if (token.Length > 1 && token[token.Length - 1] == '%')
            {
                string sub = token.Substring(0, token.Length - 1);
                if (int.TryParse(sub, out number))
                {
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Number, token);
                    continue;
                }
            }

            // Token is a Number with a Comma (,) or Period (.)
            if (token.Length > 1 && (token[token.Length - 1] == ',' || token[token.Length - 1] == '.'))
            {
                string sub = token.Substring(0, token.Length - 1);
                if (int.TryParse(sub, out number))
                {
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Number, number.ToString()) + token[token.Length - 1];
                    continue;
                }
            }

            // Token is a Number with some numerical suffix (st, nd, rd, th)
            if (token.Length > 2 && numericalSuffixes.Contains(token.Substring(token.Length - 2, 2)))
            {
                string suffix = token.Substring(token.Length - 2, 2);
                string sub = token.Substring(0, token.Length - 2);
                if (int.TryParse(sub, out number))
                {
                    res += DecorateTextWithAppropriateColor(TextDecorationLabel.Number, number.ToString() + suffix);
                    continue;
                }
            }

            res += token;
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

    public GameObject SpawnSpellToolTip(Spell spell, Transform spawningOn)
    {
        // Spawn the ToolTipList object that will house all of our other tooltips
        ToolTipList list = SpawnToolTipList(spawningOn, 1, 1, true, spellToolTipWidth);
        Transform vLayout = list.GetVerticalLayoutGroup(0).transform;
        GameObject spawned = SpawnToolTip(spellToolTipPrefab.gameObject, vLayout, true, spellToolTipHeight);
        VisualSpellDisplay display = spawned.GetComponent<VisualSpellDisplay>();
        display.SetSpell(spell);
        display.SetScaleLocked(true);
        display.SetSpellDisplayState(SpellDisplayState.ToolTip);
        return list.gameObject;
    }

    public GameObject SpawnConfirmPotionToolTip(Potion potion, Transform spawningOn)
    {
        // Spawn the ToolTipList object that will house all of our other tooltips
        ToolTipList list = SpawnToolTipList(spawningOn, 1, 1, true, confirmPotionUseToolTipWidth);
        Transform vLayout = list.GetVerticalLayoutGroup(0).transform;
        GameObject spawned = SpawnToolTip(confirmPotionToolTipPrefab.gameObject, vLayout, true, confirmPotionUseToolTipHeight);
        spawned.GetComponent<ConfirmPotionToolTip>().Set(potion, spawningOn, list.gameObject);
        return list.gameObject;
    }

    public GameObject SpawnOnlyAfflictionAndKeywordsToolTips(ToolTippable spawningFor, Transform spawningOn)
    {
        // Determine how many tool tips we're spawning
        List<ToolTipKeyword> generalKeywords = spawningFor.GetGeneralKeyWords();
        List<AfflictionType> afflictionKeywords = spawningFor.GetAfflictionKeyWords();
        int numToolTips = generalKeywords.Count + afflictionKeywords.Count;

        // Spawn the ToolTipList object that will house all of our other tooltips
        ToolTipList list = SpawnToolTipList(spawningOn, numToolTips, 1);
        Transform vLayout = list.GetVerticalLayoutGroup(0).transform;

        // Spawn ToolTips for Affliction Keywords
        for (int i = 0; i < generalKeywords.Count; i++)
        {
            ToolTipKeyword keyword = generalKeywords[i];
            SpawnToolTip(HighlightKeywords(keyword.ToString()), GetKeyWordText(keyword.ToString()), vLayout);
        }

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < afflictionKeywords.Count; i++)
        {
            AfflictionType keyword = afflictionKeywords[i];
            SpawnToolTip(HighlightKeywords(Utils.SplitOnCapitalLetters(keyword.ToString())), Affliction.GetAfflictionOfType(keyword).GetToolTipText(), vLayout);
        }

        return list.gameObject;
    }

    public GameObject SpawnEqualListingToolTips(List<ToolTippable> listings, Transform spawningOn)
    {
        // Spawn the ToolTipList object that will house all of our other tooltips
        ToolTipList list = SpawnToolTipList(spawningOn, listings.Count, 1);
        Transform vLayout = list.GetVerticalLayoutGroup(0).transform;

        // Spawn ToolTips for Additional ToolTippables
        for (int i = 0; i < listings.Count; i++)
        {
            ToolTippable tt = listings[i];
            SpawnToolTip(tt.GetToolTipLabel(), tt.GetToolTipText(), vLayout);
        }

        return list.gameObject;
    }

    public GameObject SpawnGenericToolTips(ToolTippable spawningFor, Transform spawningOn)
    {
        // Determine how many tool tips we're spawning
        List<ToolTipKeyword> generalKeywords = spawningFor.GetGeneralKeyWords();
        List<AfflictionType> afflictionKeywords = spawningFor.GetAfflictionKeyWords();
        List<ToolTippable> otherToolTippables = spawningFor.GetOtherToolTippables();
        int numToolTips = generalKeywords.Count + afflictionKeywords.Count + otherToolTippables.Count + 1;

        // Spawn the ToolTipList object that will house all of our other tooltips
        ToolTipList list = SpawnToolTipList(spawningOn, numToolTips, 1);
        Transform vLayout = list.GetVerticalLayoutGroup(0).transform;
        SpawnToolTip(spawningFor.GetToolTipLabel(), spawningFor.GetToolTipText(), vLayout);

        // Spawn ToolTips for Additional ToolTippables
        for (int i = 0; i < otherToolTippables.Count; i++)
        {
            ToolTippable tt = otherToolTippables[i];
            SpawnToolTip(tt.GetToolTipLabel(), tt.GetToolTipText(), vLayout);
        }

        // Spawn ToolTips for Affliction Keywords
        for (int i = 0; i < generalKeywords.Count; i++)
        {
            ToolTipKeyword keyword = generalKeywords[i];
            SpawnToolTip(HighlightKeywords(keyword.ToString()), GetKeyWordText(keyword.ToString()), vLayout);
        }

        // Spawn ToolTips for Additional Keywords
        for (int i = 0; i < afflictionKeywords.Count; i++)
        {
            AfflictionType keyword = afflictionKeywords[i];
            SpawnToolTip(HighlightKeywords(Utils.SplitOnCapitalLetters(keyword.ToString())), Affliction.GetAfflictionOfType(keyword).GetToolTipText(), vLayout);
        }

        return list.gameObject;
    }

    public GameObject SpawnComparisonToolTips(ToolTippableComparisonData[] spawningFor, Transform spawningOn)
    {
        int numToolTips = 0;
        // Determine how many tool tips we're spawning
        for (int i = 0; i < spawningFor.Length; i++)
        {
            ToolTippable tt = spawningFor[i].ToolTippable;
            List<ToolTipKeyword> generalKeywords = tt.GetGeneralKeyWords();
            List<AfflictionType> afflictionKeywords = tt.GetAfflictionKeyWords();
            List<ToolTippable> otherToolTippables = tt.GetOtherToolTippables();
            int ttNumToolTips = generalKeywords.Count + afflictionKeywords.Count + otherToolTippables.Count + 1;
            if (ttNumToolTips > numToolTips)
                numToolTips = ttNumToolTips;
        }

        // Spawn the ToolTipList object that will house all of our other tooltips
        ToolTipList list = SpawnToolTipList(spawningOn, numToolTips, spawningFor.Length);

        for (int i = 0; i < spawningFor.Length; i++)
        {
            ToolTippable tt = spawningFor[i].ToolTippable;
            List<ToolTipKeyword> generalKeywords = tt.GetGeneralKeyWords();
            List<AfflictionType> afflictionKeywords = tt.GetAfflictionKeyWords();
            List<ToolTippable> otherToolTippables = tt.GetOtherToolTippables();
            Transform vLayout = list.GetVerticalLayoutGroup(i).transform;
            SpawnToolTip(spawningFor[i].AdditionalLabel + tt.GetToolTipLabel(), tt.GetToolTipText(), vLayout);

            // Spawn ToolTips for Other ToolTippables
            for (int p = 0; p < otherToolTippables.Count; p++)
            {
                ToolTippable otherTt = otherToolTippables[p];
                SpawnToolTip(otherTt.GetToolTipLabel(), otherTt.GetToolTipText(), vLayout);
            }

            // Spawn ToolTips for Affliction Keywords
            for (int p = 0; p < generalKeywords.Count; p++)
            {
                ToolTipKeyword keyword = generalKeywords[p];
                SpawnToolTip(HighlightKeywords(keyword.ToString()), GetKeyWordText(keyword.ToString()), vLayout);
            }

            // Spawn ToolTips for Additional Keywords
            for (int p = 0; p < afflictionKeywords.Count; p++)
            {
                AfflictionType keyword = afflictionKeywords[p];
                SpawnToolTip(HighlightKeywords(Utils.SplitOnCapitalLetters(keyword.ToString())), Affliction.GetAfflictionOfType(keyword).GetToolTipText(), vLayout);
            }
        }

        return list.gameObject;
    }

    private ToolTipList SpawnToolTipList(Transform spawningFor, int numToolTips, int numLists, bool overrideWidth = false, float overridenWidth = 0)
    {
        RectTransform spawningForRect = spawningFor.GetComponent<RectTransform>();
        float spawningForWidth = spawningForRect.sizeDelta.x;
        float spawningForHeight = spawningForRect.sizeDelta.y;

        // Determine where on the screen the mouse is
        int horizontalSlice = 0;
        int verticalSlice = 0;
        Vector2 posFrom = spawningFor.position;

        // Get Either Left, Middle, or Right of Screen
        if (posFrom.x < Screen.width / 3)
        {
            // Left
            horizontalSlice = 0;
        }
        else if (posFrom.x >= Screen.width / 3 && posFrom.x < (Screen.width / 3) * 2)
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
        if (posFrom.y > Screen.height / 2)
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
        HorizontalLayoutGroup hLayout = spawned.GetHorizontalLayoutGroup();
        hLayout.spacing = toolTipSpacing;

        // Calculate List Height
        float listHeight = (numToolTips * toolTipHeight) + ((numToolTips - 1) * toolTipSpacing);

        float listWidth;
        if (overrideWidth)
        {
            listWidth = overridenWidth;
        }
        else
        {
            listWidth = ((numLists * toolTipWidth) + ((numLists - 1) * toolTipSpacing));
        }
        listRect.sizeDelta = new Vector2(listWidth, listHeight);

        // Depending on where the mouse is, splitting the screen up into two rows of 3 (so six sections total), we set the Layout Groups Child Alignment Property
        // Top Left = Upper Left
        // Top Middle = Upper Center
        // Top Right = Upper Right
        // Bottom Left = Lower Left
        // Bottom Middle = Lower Center
        // Bottom Right = Lower Right

        // Upper Left -> (-parentHeight / 2) (-numToolTips * toolTipHeight) (-numToolTIps - 1 * toolTipSpacing) (-offsetY)

        float verticalOffset = (spawningForHeight / 2) + (listHeight / 2) + this.offset;

        float horizontalOffset;
        if (overrideWidth)
        {
            horizontalOffset = (overridenWidth - spawningForWidth) / 2;
        }
        else
        {
            horizontalOffset = (toolTipWidth - spawningForWidth) / 2;
        }

        switch (verticalSlice)
        {
            case 0:
                if (horizontalSlice == 0)
                {
                    // Upper Left
                    hLayout.childAlignment = TextAnchor.UpperLeft;
                    verticalOffset = -verticalOffset;
                }
                else if (horizontalSlice == 1)
                {
                    // Upper Center
                    hLayout.childAlignment = TextAnchor.UpperCenter;
                    verticalOffset = -verticalOffset;
                    horizontalOffset = 0;
                }
                else
                {
                    // Upper Right
                    hLayout.childAlignment = TextAnchor.UpperRight;
                    verticalOffset = -verticalOffset;
                    horizontalOffset = -horizontalOffset;
                }
                break;
            case 1:
                if (horizontalSlice == 0)
                {
                    // Lower Left
                    hLayout.childAlignment = TextAnchor.LowerLeft;
                }
                else if (horizontalSlice == 1)
                {
                    // Lower Center
                    hLayout.childAlignment = TextAnchor.LowerCenter;
                    horizontalOffset = 0;
                }
                else
                {
                    // Lower Right
                    hLayout.childAlignment = TextAnchor.LowerRight;
                    horizontalOffset = -horizontalOffset;
                }
                break;
        }

        // Set Position
        Vector3 offset = new Vector3(horizontalOffset, verticalOffset, 0);
        listRect.localPosition = offset;
        // Debug.Log("X: " + horizontalSlice + ", Y: " + verticalSlice);
        // Debug.Log(spawningFor.position + ", Horizontal Offset: " + horizontalOffset + ", Vertical Offset: " + verticalOffset + ", Final Position: " + listRect.position);

        for (int i = 0; i < numLists; i++)
        {
            VerticalLayoutGroup list = spawned.SpawnList();

            // Set vList Spacing
            list.spacing = toolTipSpacing;

            RectTransform currentRect = list.GetComponent<RectTransform>();
            Vector2 currentSizeDelta = currentRect.sizeDelta;
            currentRect.sizeDelta = new Vector2(toolTipWidth, currentSizeDelta.y);
        }

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

    private GameObject SpawnToolTip(GameObject obj, Transform transform, bool overrideHeight = false, float overridenHeight = 0)
    {
        GameObject spawned = Instantiate(obj, transform);
        RectTransform spawnedRect = spawned.GetComponent<RectTransform>();
        if (overrideHeight)
        {
            spawnedRect.sizeDelta = new Vector2(spawnedRect.sizeDelta.x, overridenHeight);
        }
        else
        {
            spawnedRect.sizeDelta = new Vector2(spawnedRect.sizeDelta.x, toolTipHeight);
        }

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

    public UISectionInformation GetUISectionInformation(UISection uiSection)
    {
        return UISectionMap[uiSection];
    }

    public Sprite GetAfflictionIcon(AfflictionType type)
    {
        return afflictionIconDict[type];
    }
}
