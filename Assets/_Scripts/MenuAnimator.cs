using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
    public void Disappear()
    {
        gameObject.SetActive(false);
    }

    public void GameOver()
    {
        GameController.instance.LoadGame();
    }
}
