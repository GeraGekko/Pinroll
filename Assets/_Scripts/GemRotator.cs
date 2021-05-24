using UnityEngine;

public class GemRotator : MonoBehaviour
{
    public float speed;

    private void Update()
    {
        transform.Rotate(Vector3.up * speed * 50f * Time.deltaTime);
    }
}
