using UnityEngine;
using UnityEngine.UI;

public class FinishMenu : MonoBehaviour
{
    public GameObject medalEffect;

    public Button[] buttons;
    public Sprite[] medalsSprites;
    public Image[] medals;

    public RectTransform sliderTransform;
    public RectTransform gemTransform;
    public Image progressBar;
    public Text gemsText;
    public float gemScaleValue;
    public float progressBarSpeed;
    public float gemSpeed;

    private float finalSliderPosition = 230f;
    private float percentToFill;
    private float percentPerGem;
    private float percentToAddGem;

    private int collectedGems;
    private int allGems;
    private int currentGem;

    private int medalState = 0; //0 - NONE; 1 - BRONZE; 2 - SILVER; 3 - GOLD
    private int gemScalingState = 0; //0 - NOT SCALING; 1 - SCALE TO; 2 - SCALE BACK
    private bool isButtonActive = false;

    private void Update()
    {
        progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, percentToFill, progressBarSpeed * 0.1f * Time.deltaTime);
        sliderTransform.localPosition = new Vector3(progressBar.fillAmount * finalSliderPosition, sliderTransform.localPosition.y, sliderTransform.localPosition.z);

        //medals
        if (progressBar.fillAmount >= 0.135f && medalState == 0)
        {
            //bronze
            medals[0].sprite = medalsSprites[0];
            medalState = 1;
            Instantiate(medalEffect, medals[0].gameObject.transform);

            if (!isButtonActive && percentToFill < 0.465f)
            {
                buttons[0].interactable = true;
                buttons[1].interactable = true;

                isButtonActive = true;
            }
        }
        else if (progressBar.fillAmount >= 0.465f && medalState == 1)
        {
            //silver
            medals[1].sprite = medalsSprites[1];
            medalState = 2;
            Instantiate(medalEffect, medals[1].gameObject.transform);

            if (!isButtonActive && percentToFill < 0.865f)
            {
                buttons[0].interactable = true;
                buttons[1].interactable = true;

                isButtonActive = true;
            }
        }
        else if (progressBar.fillAmount >= 0.865f && medalState == 2)
        {
            //gold
            medals[2].sprite = medalsSprites[2];
            medalState = 3;
            Instantiate(medalEffect, medals[2].gameObject.transform);

            if (!isButtonActive)
            {
                buttons[0].interactable = true;
                buttons[1].interactable = true;

                isButtonActive = true;
            }
        }

        if (progressBar.fillAmount > percentToAddGem)
        {
            if (currentGem < collectedGems)
            {
                currentGem++;
                gemsText.text = currentGem.ToString();

                if (currentGem == collectedGems)
                {
                    progressBarSpeed *= 1.35f;
                }

                gemScalingState = 1;
            }
            percentToAddGem += percentPerGem - 0.1f;
        }

        if (gemScalingState != 0)
        {
            if (gemScalingState == 1)
            {
                gemTransform.localScale += new Vector3(1, 1, 0) * gemSpeed * Time.deltaTime;
                if (gemTransform.localScale.x >= gemScaleValue)
                {
                    gemScalingState = 2;
                }
            }
            else if (gemScalingState == 2)
            {
                gemTransform.localScale -= new Vector3(1, 1, 0) * gemSpeed * Time.deltaTime;
                if (gemTransform.localScale.x <= 1)
                {
                    gemScalingState = 0;
                    gemTransform.localScale = Vector3.one;
                }
            }
        }
    }

    public void StartReward()
    {
        collectedGems = GameController.instance.GetGem();
        allGems = collectedGems + GameObject.FindGameObjectsWithTag("Gem").Length;

        percentPerGem = (float)1 / allGems;
        percentToFill = percentPerGem * collectedGems;
        percentToAddGem = percentPerGem - 0.1f;

        if (collectedGems == 0)
        {
            percentToFill = 0.15f;
        }
    }

    public void NextButton()
    {
        GameController.instance.SetCorrectLevel();
        GameController.instance.SetOffTopMenu();
        GetComponent<Animator>().SetTrigger("off");

        buttons[0].interactable = false;
        buttons[1].interactable = false;
    }

    public void RetryButton()
    {
        GetComponent<Animator>().SetTrigger("off");
        GameController.instance.SetOffTopMenu();

        buttons[0].interactable = false;
        buttons[1].interactable = false;
    }
}
