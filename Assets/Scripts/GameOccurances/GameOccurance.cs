using System.Collections;
using UnityEngine;

public abstract class GameOccurance : ScriptableObject
{
    private bool resolved;

    public void SetResolve(bool val)
    {
        resolved = val;
    }

    public IEnumerator RunOccurance()
    {
        Debug.Log(name + ": Beginning to Run");
        yield return GameManager._Instance.StartCoroutine(OnStart());

        Debug.Log(name + ": Waiting to Resolve");

        yield return new WaitUntil(() => resolved);
        Debug.Log(name + ": Has Been Resolved");

        yield return GameManager._Instance.StartCoroutine(OnResolve());
        Debug.Log(name + ": Has Ended");
    }

    protected abstract IEnumerator OnStart();

    protected abstract IEnumerator OnResolve();
}
