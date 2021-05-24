using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Testing Platform")]
    public bool isEditor;

    [Header("Prefabs")]
    public GameObject tutorialMenu1Prefab;
    public GameObject tutorialMenu2Prefab;
    public GameObject pandaEmoji;

    [Header("UI")]
    public Transform canvas;
    public GameObject startMenuPanel;
    public GameObject topMenuPanel;
    public GameObject gameMenuPanel;
    public GameObject finishMenuPanel;
    public Text globalLevelText;
    public Image hapticImage;
    public Sprite[] hapticSprites;

    [Header("Timings")]
    public float gameOverDelay;
    public float finishDelay;

    [Header("Level Logic")]
    public GameObject[] levelBoards;
    public Vector3[] cameraStartLevelPositions;
    private int globalLevel;
    private int level;
    private int gems = 0;

    //Scripts
    private ObstacleController[] obstacles;
    private BoardMovement board;
    private PlayerController player;
    private RingColor ring;

    //Input
    private Vector2 startInputPosition;
    private Vector2 currentInputPosition;
    private float swipeRange = 50;
    private bool isRegisteringSwipe;

    //Logic
    private GameObject tutorialMenu1;
    private GameObject tutorialMenu2;
    public static int movableObjectsCounter;
    private int registerInputState = 1; //0 - NOT REGISTERING; 1 - REGISTERING ALL; 2 - REGISTERING ONLY TAPS
    private bool hasGameStarted = false;
    private bool isHapticEnabled;

    //Singleton
    public static GameController instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        GetCorrectLevel();

        //haptic
        isHapticEnabled = true;
        if (PlayerPrefs.HasKey("haptic"))
        {
            if (PlayerPrefs.GetInt("haptic") == 0)
            {
                HapticButton();
            }
        }
        else
        {
            PlayerPrefs.SetInt("haptic", 1);
        }
    }

    private void Start()
    {
        InitOnStart();
    }

    private void Update()
    {
        if (registerInputState != 0)
        {
            PlayerInput();
        }
    }

    private void GetCorrectLevel()
    {
        if (PlayerPrefs.HasKey("globalLevel"))
        {
            globalLevel = PlayerPrefs.GetInt("globalLevel");
        }
        else
        {
            globalLevel = 1;
        }
        //UI
        globalLevelText.text = "Level " + globalLevel;

        if (globalLevel <= 15)
        {
            level = globalLevel;
        }
        else
        {
            string levelChain = PlayerPrefs.GetString("levelChain");
            level = int.Parse(levelChain.Split('.')[0]);
        }
    }

    public void SetCorrectLevel()
    {
        globalLevel++;
        PlayerPrefs.SetInt("globalLevel", globalLevel);

        if (globalLevel < 16)
        {
            return;
        }

        if (PlayerPrefs.HasKey("levelChain"))
        {
            string levelChain = PlayerPrefs.GetString("levelChain");
            string[] levels = levelChain.Split('.');
            string newLevelChain = "";

            if (levels.Length == 1)
            {
                newLevelChain = GenerateLevelChain();
            }
            else
            {
                for (int i = 1; i < levels.Length; i++)
                {
                    newLevelChain += levels[i];
                    if (i != levels.Length - 1)
                    {
                        newLevelChain += ".";
                    }
                }
            }

            PlayerPrefs.SetString("levelChain", newLevelChain);
        }
        else
        {
            PlayerPrefs.SetString("levelChain", GenerateLevelChain());
        }
    }

    private string GenerateLevelChain()
    {
        string chain = "";
        int[] levels = new int[13];

        for (int i = 0; i < levels.Length; i++)
        {
            levels[i] = i + 3;
        }

        for (int i = 0; i < levels.Length; i++)
        {
            int randIndex = Random.Range(0, levels.Length);
            int temp = levels[i];
            levels[i] = levels[randIndex];
            levels[randIndex] = temp;
        }

        for (int i = 0; i < levels.Length; i++)
        {
            chain += levels[i];
            if (i != levels.Length - 1)
            {
                chain += ".";
            }
        }

        return chain;
    }

    private void InitOnStart()
    {
        Instantiate(levelBoards[level - 1]);

        board = FindObjectOfType<BoardMovement>();
        if (board == null)
        {
            Debug.LogError("Can't find Board!");
        }

        player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("Can't find Player!");
        }

        ring = FindObjectOfType<RingColor>();
        if (ring == null)
        {
            Debug.LogError("Can't find Ring!");
        }

        obstacles = FindObjectsOfType<ObstacleController>();
        Camera.main.transform.position = cameraStartLevelPositions[(level - 1) / 5];

        //UI
        if (globalLevel == 1)
        {
            tutorialMenu1 = Instantiate(tutorialMenu1Prefab, canvas);
        }
        else if (globalLevel != 1 && PlayerPrefs.GetInt("launch") == 1)
        {
            startMenuPanel.SetActive(true);
            topMenuPanel.SetActive(true);
            PlayerPrefs.SetInt("launch", 0);
        }
        else if (PlayerPrefs.GetInt("launch") == 0)
        {
            gameMenuPanel.SetActive(true);
        }
    }

    private void PlayerInput()
    {
        if (isEditor)
        {
            //pc
            if (Input.anyKeyDown && registerInputState == 1)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    if (!board.gameObject.CompareTag("BoardUp"))
                    {
                        board.gameObject.tag = "BoardUp";
                        SendSignalsToMovableObjects(false);
                    }
                    else
                    {
                        SendSignalsToMovableObjects(true);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    if (!board.gameObject.CompareTag("BoardDown"))
                    {
                        board.gameObject.tag = "BoardDown";
                        SendSignalsToMovableObjects(false);
                    }
                    else
                    {
                        SendSignalsToMovableObjects(true);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    if (!board.gameObject.CompareTag("BoardLeft"))
                    {
                        board.gameObject.tag = "BoardLeft";
                        SendSignalsToMovableObjects(false);
                    }
                    else
                    {
                        SendSignalsToMovableObjects(true);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    if (!board.gameObject.CompareTag("BoardRight"))
                    {
                        board.gameObject.tag = "BoardRight";
                        SendSignalsToMovableObjects(false);
                    }
                    else
                    {
                        SendSignalsToMovableObjects(true);
                    }
                }

                if (!hasGameStarted)
                {
                    hasGameStarted = true;

                    if (tutorialMenu1 != null && tutorialMenu1.activeSelf)
                    {
                        tutorialMenu1.SetActive(false);
                    }

                    if (globalLevel == 2 && tutorialMenu2 == null)
                    {
                        tutorialMenu2 = Instantiate(tutorialMenu2Prefab, canvas);
                    }

                    if (startMenuPanel.activeSelf)
                    {
                        ActionsOnGameStart();
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0) && (registerInputState == 1 || registerInputState == 2))
            {
                TapRegister(Camera.main.ScreenPointToRay(Input.mousePosition));
            }
        }
        else
        {
            //mobile
            if (Input.touchCount > 0)
            {
                Touch currentTouch = Input.GetTouch(0);
                if (currentTouch.phase == TouchPhase.Began && registerInputState == 1)
                {
                    isRegisteringSwipe = true;
                    startInputPosition = currentTouch.position;
                    currentInputPosition = startInputPosition;
                }
                else if (currentTouch.phase == TouchPhase.Stationary)
                {
                    if (currentTouch.position != startInputPosition)
                    {
                        isRegisteringSwipe = true;
                        startInputPosition = currentTouch.position;
                        currentInputPosition = startInputPosition;
                    }
                }
                else if (currentTouch.phase == TouchPhase.Moved && isRegisteringSwipe && registerInputState == 1)
                {
                    currentInputPosition = currentTouch.position;
                    Vector2 touchDistance = currentInputPosition - startInputPosition;

                    if (touchDistance.y > swipeRange)
                    {
                        isRegisteringSwipe = false;

                        if (!board.gameObject.CompareTag("BoardUp"))
                        {
                            board.gameObject.tag = "BoardUp";
                            SendSignalsToMovableObjects(false);
                        }
                        else
                        {
                            SendSignalsToMovableObjects(true);
                        }
                    }
                    else if (touchDistance.y < -swipeRange)
                    {
                        isRegisteringSwipe = false;

                        if (!board.gameObject.CompareTag("BoardDown"))
                        {
                            board.gameObject.tag = "BoardDown";
                            SendSignalsToMovableObjects(false);
                        }
                        else
                        {
                            SendSignalsToMovableObjects(true);
                        }
                    }
                    else if (touchDistance.x < -swipeRange)
                    {
                        isRegisteringSwipe = false;

                        if (!board.gameObject.CompareTag("BoardLeft"))
                        {
                            board.gameObject.tag = "BoardLeft";
                            SendSignalsToMovableObjects(false);
                        }
                        else
                        {
                            SendSignalsToMovableObjects(true);
                        }
                    }
                    else if (touchDistance.x > swipeRange)
                    {
                        isRegisteringSwipe = false;

                        if (!board.gameObject.CompareTag("BoardRight"))
                        {
                            board.gameObject.tag = "BoardRight";
                            SendSignalsToMovableObjects(false);
                        }
                        else
                        {
                            SendSignalsToMovableObjects(true);
                        }
                    }
                }
                else if (currentTouch.phase == TouchPhase.Ended && (registerInputState == 1 || registerInputState == 2))
                {
                    if (isRegisteringSwipe)
                    {
                        TapRegister(Camera.main.ScreenPointToRay(currentTouch.position));
                    }
                }

                if (!hasGameStarted && !isRegisteringSwipe)
                {
                    hasGameStarted = true;

                    if (tutorialMenu1 != null && tutorialMenu1.activeSelf)
                    {
                        tutorialMenu1.SetActive(false);
                    }

                    if (globalLevel == 2 && tutorialMenu2 == null)
                    {
                        tutorialMenu2 = Instantiate(tutorialMenu2Prefab, canvas);
                    }

                    if (startMenuPanel.activeSelf)
                    {
                        ActionsOnGameStart();
                    }
                }
            }
        }
    }

    private void ActionsOnGameStart()
    {
        startMenuPanel.GetComponent<Animator>().SetTrigger("off");
        topMenuPanel.GetComponent<Animator>().SetTrigger("off");
        gameMenuPanel.SetActive(true);
    }

    private void TapRegister(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            ObstacleController oc = hit.collider.gameObject.GetComponent<ObstacleController>();
            if (oc != null)
            {
                registerInputState = 2;
                oc.ChangePinCondition();
            }

            //ui
            if (globalLevel == 2)
            {
                tutorialMenu2.SetActive(false);
            }
        }
    }

    private void SendSignalsToMovableObjects(bool onlyBoard)
    {
        registerInputState = 0;

        if (onlyBoard)
        {
            movableObjectsCounter = 1;
            board.SwipeRepeat();
        }
        else
        {
            movableObjectsCounter = 2 + obstacles.Length; //board + player + obstacles (if any)

            board.StartMovement();
            player.StartMovement(board.gameObject.tag);
            if (obstacles.Length != 0)
            {
                foreach (ObstacleController obstacle in obstacles)
                {
                    obstacle.StartMovement(board.gameObject.tag);
                }
            }
        }
    }

    private void DelayedActionsOnGameOver()
    {
        gameMenuPanel.GetComponent<Animator>().SetTrigger("over");
    }

    private void DelayedActionsOnFinish()
    {
        finishMenuPanel.SetActive(true);
        topMenuPanel.SetActive(true);
    }

    public void DecreaseMovableObjectsCounter()
    {
        movableObjectsCounter--;
        if (movableObjectsCounter == 0)
        {
            registerInputState = 1;
        }
    }

    public void UnpinLogic(ObstacleController obstacle)
    {
        if (board.CompareTag("BoardCenter"))
        {
            registerInputState = 1;
        }
        else
        {
            registerInputState = 2;
        }

        movableObjectsCounter++;
        obstacle.StartMovement(board.tag);
    }

    public void PinLogic()
    {
        registerInputState = 1;
    }

    public void Finish()
    {
        ring.Finish();

        //UI
        Instantiate(pandaEmoji, canvas);
        gameMenuPanel.GetComponent<Animator>().SetTrigger("off");
        Invoke("DelayedActionsOnFinish", finishDelay);
        if (globalLevel == 1)
        {
            PlayerPrefs.SetInt("launch", 0);
        }
    }

    public void GameOver()
    {
        player.GameOver();
        registerInputState = 0;

        Invoke("DelayedActionsOnGameOver", gameOverDelay);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void AddGem()
    {
        gems++;
    }

    public int GetGem()
    {
        return gems;
    }

    public void SetOffTopMenu()
    {
        topMenuPanel.GetComponent<Animator>().SetTrigger("off");
    }

    public void HapticButton()
    {
        if (isHapticEnabled)
        {
            //turn off
            isHapticEnabled = false;
            PlayerPrefs.SetInt("haptic", 0);

            hapticImage.sprite = hapticSprites[0];
        }
        else
        {
            //turn on
            isHapticEnabled = true;
            PlayerPrefs.SetInt("haptic", 1);

            hapticImage.sprite = hapticSprites[1];
        }
    }
}
