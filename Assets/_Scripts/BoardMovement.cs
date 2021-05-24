using UnityEngine;

public class BoardMovement : MonoBehaviour
{
    private float boardSpeed = 10;
    //private float lightSpeed = 12.5f;
    private float boardAngle = 15;
    private float fakeAngle = 5;
    //private float lightAngle = 0;
    private float offsetAngle = 0.1f;
    private bool hasRespondedToGameController;

    //private Light sceneLight;
    
    //private Quaternion lightTarget = Quaternion.identity;
    private Quaternion target = Quaternion.identity;
    
    private BoardState boardState = BoardState.Stopped;
    private enum BoardState
    {
        Stopped,
        MovingUp,
        MovingDown,
        MovingLeft,
        MovingRight,
        MovingCenter,
        MovingBeyond
    }

    private void Start()
    {
        //sceneLight = FindObjectOfType<Light>();
        //if (sceneLight == null)
        //{
        //    Debug.LogError("Can't find Directional Light!");
        //}
    }

    private void Update()
    {
        if (boardState != BoardState.Stopped && boardState != BoardState.MovingBeyond)
        {
            MainMovement();
        }
        else if (boardState == BoardState.MovingBeyond)
        {
            BeyondMovement();
        }
    }

    private void MainMovement()
    {
        //set rotation target
        if (target == Quaternion.identity)
        {
            switch (boardState)
            {
                //case moving center missing!!!
                case BoardState.MovingUp:
                    target = Quaternion.Euler(boardAngle, transform.rotation.eulerAngles.y, 0);
                    //lightTarget = Quaternion.Euler(40, -lightAngle, 0);
                    break;
                case BoardState.MovingDown:
                    target = Quaternion.Euler(-boardAngle, transform.rotation.eulerAngles.y, 0);
                    //lightTarget = Quaternion.Euler(40, -lightAngle, 0);
                    break;
                case BoardState.MovingLeft:
                    target = Quaternion.Euler(0, transform.rotation.eulerAngles.y, boardAngle);
                    //lightTarget = Quaternion.Euler(40, 0, 0);
                    break;
                case BoardState.MovingRight:
                    target = Quaternion.Euler(0, transform.rotation.eulerAngles.y, -boardAngle);
                    //lightTarget = Quaternion.Euler(40, 0, 0);
                    break;
                default:
                    Debug.LogError("Wrong BoardState!");
                    break;
            }
        }

        //light rotation
        //if (sceneLight.transform.rotation != lightTarget)
        //{
        //    sceneLight.transform.rotation = Quaternion.Slerp(sceneLight.transform.rotation, lightTarget, lightSpeed * Time.deltaTime);
        //}
        //if (Quaternion.Angle(sceneLight.transform.rotation, lightTarget) < offsetAngle)
        //{
        //    sceneLight.transform.rotation = lightTarget;
        //}

        //board rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target, boardSpeed * Time.deltaTime);
        if (!hasRespondedToGameController && Quaternion.Angle(transform.rotation, target) < offsetAngle * 10f)
        {
            GameController.instance.DecreaseMovableObjectsCounter();
            hasRespondedToGameController = true;
        }
        if (Quaternion.Angle(transform.rotation, target) < offsetAngle)
        {
            transform.rotation = target;
            target = Quaternion.identity;

            boardState = BoardState.Stopped;
        }
    }

    private void BeyondMovement()
    {
        //set rotation target
        if (target == Quaternion.identity)
        {
            if (CompareTag("BoardUp"))
            {
                target = Quaternion.Euler(transform.rotation.eulerAngles.x + fakeAngle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            }
            else if (CompareTag("BoardDown"))
            {
                target = Quaternion.Euler(transform.rotation.eulerAngles.x - fakeAngle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            }
            else if (CompareTag("BoardLeft"))
            {
                target = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + fakeAngle);
            }
            else if (CompareTag("BoardRight"))
            {
                target = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z - fakeAngle);
            }
            else
            {
                Debug.LogError("Wrong Tag!");
            }
        }

        //short beyond rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target, boardSpeed * 2f * Time.deltaTime);
        if (Quaternion.Angle(transform.rotation, target) < offsetAngle)
        {
            transform.rotation = target;
            target = Quaternion.identity;

            SetNewBoardState();
        }
    }

    private void SetNewBoardState()
    {
        hasRespondedToGameController = false;
        
        target = Quaternion.identity;
        //lightTarget = Quaternion.identity;

        switch (tag)
        {
            case "BoardUp":
                boardState = BoardState.MovingUp;
                break;
            case "BoardDown":
                boardState = BoardState.MovingDown;
                break;
            case "BoardLeft":
                boardState = BoardState.MovingLeft;
                break;
            case "BoardRight":
                boardState = BoardState.MovingRight;
                break;
            default:
                Debug.LogError("Wrong BoardState!");
                break;
        }
    }

    public void StartMovement()
    {
        SetNewBoardState();
    }

    public void SwipeRepeat()
    {
        boardState = BoardState.MovingBeyond;
    }
}
