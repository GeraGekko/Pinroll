using UnityEngine;

public class RingColor : MonoBehaviour
{
    public MeshRenderer ring;

    [Header("Colors")]
    public Color secondColor;
    public Color colorOnFinish;

    public float colorSpeed;
    public float changeDelayInSeconds;

    private Color firstColor;
    private Color targetColor;

    [Range(0f,1f)]
    private float colorState;

    private bool isChanging = true;

    private void Start()
    {
        firstColor = ring.material.color;
        targetColor = secondColor;
    }

    private void Update()
    {
        if (isChanging)
        {
            ColorChange();
        }
    }

    private void ColorChange()
    {
        ring.material.color = Color.Lerp(ring.material.color, targetColor, colorSpeed * Time.deltaTime);
        
        colorState = Mathf.Lerp(colorState, 1f, colorSpeed * Time.deltaTime);
        if (colorState >= 0.95f)
        {
            ring.material.color = targetColor;
            if (targetColor == colorOnFinish)
            {
                isChanging = false;
                return;
            }
            else if (targetColor == firstColor)
            {
                targetColor = secondColor;
            }
            else
            {
                targetColor = firstColor;
            }

            isChanging = false;
            colorState = 0f;
            Invoke("StartColorChange", changeDelayInSeconds);
        }
    }

    private void StartColorChange()
    {
        isChanging = true;
    }

    public void Finish()
    {
        targetColor = colorOnFinish;
        StartColorChange();
    }
}
