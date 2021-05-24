using UnityEngine;
using UnityEngine.UI;

public class PandaEmoji : MonoBehaviour
{
    public Sprite[] images;

    private void Start()
    {
        int randIndex = Random.Range(0, images.Length);
        GetComponent<Image>().sprite = images[randIndex];
    }

    public void DestroyOnAnimationEnd()
    {
        Destroy(gameObject);
    }
}
