using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour
{
    private void Start()
    {
        //logic
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("launch", 1);

        Invoke("LoadTheGame", 0.1f);
    }

    private void LoadTheGame()
    {
        SceneManager.LoadScene("Game");
    }
}
