using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGameHelper : MonoBehaviour
{
    public static RestartGameHelper _Instance { get; private set; }

    [SerializeField] private string sceneToLoad;

    private void Start()
    {
        _Instance = this;
    }

    public void Restart()
    {
        Destroy(GameObject.Find("Systems"));
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadScene(string scene)
    {
        Destroy(GameObject.Find("Systems"));
        SceneManager.LoadScene(scene);
    }

    public void LoadScene(int buildIndex)
    {
        Destroy(GameObject.Find("Systems"));
        SceneManager.LoadScene(buildIndex);
    }
}