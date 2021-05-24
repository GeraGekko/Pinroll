using UnityEngine;

public class Destroyer : MonoBehaviour
{
    public float timeToDestroy;

    private void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }
}
