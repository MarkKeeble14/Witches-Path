using UnityEngine;
using System.Collections.Generic;

public enum ScoreReason
{
    RoomCleared,
    MinorCombatCleared,
    EliteCombatCleared,
    MiniBossCombatCleared,
    BossCombatCleared,
    Perfect,
    Flawless
}

public class EndGameScoreData
{

    public string Label { get; private set; }
    public int Num { get; private set; }
    public int Score { get; private set; }
    public EndGameScoreData(string label, int num, int score)
    {
        Label = label;
        Num = num;
        Score = score;
    }
}

public class ScoreRule
{
    public string Label { get; private set; }
    public ScoreCounting ScoreCounting { get; private set; }
    public ScoreRule(string label, ScoreCounting scoreCounting)
    {
        Label = label;
        ScoreCounting = scoreCounting;
    }
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager _Instance { get; private set; }

    private Dictionary<ScoreReason, ScoreRule> scoreRuleDict = new Dictionary<ScoreReason, ScoreRule>();

    private Dictionary<ScoreReason, int> currentScoreDict = new Dictionary<ScoreReason, int>();

    // if a Key and a Value are both present in the final Current Score Dictionary, this Dictionary dicates that the Value will not be Present in the final Score 
    private Dictionary<ScoreReason, ScoreReason> overrideRules = new Dictionary<ScoreReason, ScoreReason>();

    private void Awake()
    {
        _Instance = this;

        MakeDictionaries();
    }

    private void MakeDictionaries()
    {
        // Set Score Reason Dictionary
        scoreRuleDict.Add(ScoreReason.BossCombatCleared, new ScoreRule("Bosses Slain", new IndexedScoreCounting(50, 150, 300, 500, 100)));
        scoreRuleDict.Add(ScoreReason.EliteCombatCleared, new ScoreRule("Elite Combats Cleared", new MultiplicationScoreCounting(50)));
        scoreRuleDict.Add(ScoreReason.MiniBossCombatCleared, new ScoreRule("Minibosses Slain", new MultiplicationScoreCounting(50)));
        scoreRuleDict.Add(ScoreReason.MinorCombatCleared, new ScoreRule("Minor Combats Cleared", new MultiplicationScoreCounting(10)));
        scoreRuleDict.Add(ScoreReason.RoomCleared, new ScoreRule("Rooms Cleared", new MultiplicationScoreCounting(5)));
        scoreRuleDict.Add(ScoreReason.Perfect, new ScoreRule("Perfect", new MultiplicationScoreCounting(100)));
        scoreRuleDict.Add(ScoreReason.Flawless, new ScoreRule("Flawless", new MultiplicationScoreCounting(500)));

        overrideRules.Add(ScoreReason.Flawless, ScoreReason.Perfect);

        // Initialize Current Dictionary
        foreach (ScoreReason scoreReason in scoreRuleDict.Keys)
        {
            currentScoreDict.Add(scoreReason, 0);
        }
    }

    /// <summary>
    /// Adds a Score Reason
    /// </summary>
    /// <param name="scoreReason"></param>
    /// <param name="amount"></param>
    public void AddScore(ScoreReason scoreReason, int amount = 1)
    {
        currentScoreDict[scoreReason] += amount;
    }

    private Dictionary<ScoreReason, int> CureScoreDict(Dictionary<ScoreReason, int> toCure)
    {
        Dictionary<ScoreReason, int> curedDict = new Dictionary<ScoreReason, int>(toCure);

        // foreach Value in Override Rules
        foreach (KeyValuePair<ScoreReason, ScoreReason> kvp in overrideRules)
        {
            // if Dict Contains Key and Value, remove Value
            if (curedDict.ContainsKey(kvp.Key) && curedDict.ContainsKey(kvp.Value))
            {
                curedDict.Remove(kvp.Value);
            }
        }
        return curedDict;
    }

    public List<EndGameScoreData> GetFinalScoreData()
    {
        List<EndGameScoreData> finalData = new List<EndGameScoreData>();
        foreach (KeyValuePair<ScoreReason, int> kvp in CureScoreDict(currentScoreDict))
        {
            ScoreRule scoreRule = scoreRuleDict[kvp.Key];
            finalData.Add(new EndGameScoreData(scoreRule.Label, kvp.Value, scoreRule.ScoreCounting.GetScoreAmount(kvp.Value)));
        }
        return finalData;
    }

    [ContextMenu("Print Score Dict")]
    public void PrintFinalScoreDict()
    {
        foreach (EndGameScoreData data in GetFinalScoreData())
        {
            Debug.Log(data.Label + ", (" + data.Num + ") = " + data.Score);
        }
    }

    [ContextMenu("Add Random Score")]
    public void AddRandomScore()
    {
        AddScore(RandomHelper.GetRandomEnumValue<ScoreReason>());
    }
}

[System.Serializable]
public abstract class ScoreCounting
{
    public abstract int GetScoreAmount(int amount);
}

public class IndexedScoreCounting : ScoreCounting
{
    private int[] scoreArray;

    public override int GetScoreAmount(int index)
    {
        if (index > scoreArray.Length - 1)
        {
            return scoreArray[scoreArray.Length - 1];
        }
        else
        {
            return scoreArray[index];
        }
    }

    public IndexedScoreCounting(params int[] scoreArray)
    {
        this.scoreArray = scoreArray;
    }
}

public class MultiplicationScoreCounting : ScoreCounting
{
    private int scorePer;
    public override int GetScoreAmount(int amount)
    {
        return amount * scorePer;
    }

    public MultiplicationScoreCounting(int scorePer)
    {
        this.scorePer = scorePer;
    }
}