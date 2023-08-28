using System;
using System.Collections.Generic;

public class EnemyAction
{
    private List<EnemyIntent> enemyIntents = new List<EnemyIntent>();
    private Action onActivate;

    // Constructor
    public EnemyAction(List<EnemyIntent> intents, Action onActivate)
    {
        AddEnemyIntents(intents);
        this.onActivate += onActivate;
    }

    // Call the Callback
    public void CallOnActivate()
    {
        onActivate?.Invoke();
    }

    // Getter
    public List<EnemyIntent> GetEnemyIntents()
    {
        return enemyIntents;
    }

    // Function to Add an IEnumerable of Intents
    public void AddEnemyIntents(IEnumerable<EnemyIntent> intents)
    {
        foreach (EnemyIntent intent in intents)
        {
            AddEnemyIntent(intent);
        }
    }

    // Function to Add a single Intent
    public void AddEnemyIntent(EnemyIntent intent)
    {
        enemyIntents.Add(intent);
    }

    public bool HasIntentType(IntentType intentType)
    {
        foreach (EnemyIntent intent in enemyIntents)
        {
            if (intent.Type == intentType) return true;
        }
        return false;
    }

    public EnemyIntent GetIntentOfType(IntentType intentType)
    {
        foreach (EnemyIntent intent in enemyIntents)
        {
            if (intent.Type == intentType) return intent;
        }
        return null;
    }
}
