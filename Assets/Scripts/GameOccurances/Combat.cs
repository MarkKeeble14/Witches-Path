using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public abstract class Combat : GameOccurance
{
    [Header("Map")]
    [SerializeField] private DefaultAsset mapFile; // Map file (.osu format), attach from editor
    public DefaultAsset MapFile { get => mapFile; }
    [SerializeField] private AudioClip mainMusic; // Music file, attach from editor
    public AudioClip MainMusic { get => mainMusic; }
    [SerializeField] private AudioClip hitSound; // Hit sound
    public AudioClip HitSound { get => hitSound; }
    [SerializeField] private AudioClip missSound; // Hit sound
    public AudioClip MissSound { get => missSound; }

    [SerializeField] private Enemy enemy;
    public Enemy Enemy { get => enemy; }

    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");
        yield return GameManager._Instance.StartCoroutine(CombatManager._Instance.StartCombat(this));
    }
}
