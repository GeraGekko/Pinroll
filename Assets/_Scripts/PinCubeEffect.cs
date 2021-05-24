using UnityEngine;

public class PinCubeEffect : MonoBehaviour
{
    public float speed;

    private void Start()
    {
        //get scale
        Vector3 parentScale = gameObject.transform.parent.gameObject.GetComponent<BoxCollider>().size;
        transform.localScale = new Vector3(parentScale.x, 1, parentScale.z);
    }

    private void Update()
    {
        transform.localScale += Vector3.one * speed * Time.deltaTime;
    }

    public void DestroyOnEnd()
    {
        Destroy(gameObject);
    }
}
