using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject gemEffect;
    public GameObject pandaEffect;

    public float scaleValue;
    public float scaleSpeed;
    public float moveSpeedMax;
    public float rotationSpeedMax;
    public float interpolationSpeedMax;
    public Vector3[] rotatingTargets;
    private float moveSpeed;
    private float rotationSpeed;
    private float interpolationSpeed;

    private SphereCollider playerCollider;
    private Transform ball;

    private int movingState; //0 - NOT MOVING; 1 - USUAL MOVE; 2 - FINISH MOVE; 3 - FALLING
    private int rotatingState; //0 - NOT ROTATING; 1 - ROTATING ON MOVING; 2 - ROTATING ON HIT; 3 - ROTATING ON WIN; 4 - ROTATING ON IDLE
    private int scalingState; //0 - NOT SCALING; 1 - SCALING TO; 2 - SCALING BACK
    private bool[] isDirectionBlocked = new bool[4]; //0 - UP; 1 - DOWN; 2 - LEFT; 3 - RIGHT

    private Quaternion currentRotatingTarget = Quaternion.identity;
    private Vector3 targetToMove;
    private Vector3 movementDirection;
    private Vector3 positionBeforeScaling = Vector3.zero;
    private string directionTag;
    private float angleOffset = 0.05f;
    private float distanceOffset = 0.05f;
    private float rayOffset = 0.05f;

    private void Start()
    {
        InitOnStart();
    }

    private void Update()
    {
        if (movingState != 0 && movingState != 1)
        {
            Move();
        }

        if (rotatingState != 0)
        {
            Rotate();
        }

        if (scalingState != 0)
        {
            Scale();
        }
    }

    private void FixedUpdate()
    {
        if (movingState == 1)
        {
            Move();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (movingState != 1)
        {
            return;
        }

        if (other.gameObject.CompareTag("WinHole"))
        {
            StartFinishMove(other.gameObject.transform);
        }
        else if (other.gameObject.CompareTag("Gem"))
        {
            Destroy(other.gameObject);
            GameController.instance.AddGem();
            //effect
            Instantiate(gemEffect, other.gameObject.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            BoxCollider wallCollider = other.gameObject.GetComponent<BoxCollider>();
            if (wallCollider != null)
            {
                EndUsualMove(other.gameObject.transform, new Vector2(wallCollider.size.x, wallCollider.size.z));
            }
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            ObstacleController oc = other.gameObject.GetComponent<ObstacleController>();
            if (oc != null)
            {
                EndUsualMove(other.gameObject.transform, oc.GetObstacleDimensions());
            }
        }
        else if (other.gameObject.CompareTag("Hole"))
        {
            movingState = 3;
        }
        else
        {
            //borders
            EndUsualMove(other.gameObject.transform, new Vector2(1, 1));
        }
    }

    private void InitOnStart()
    {
        playerCollider = GetComponent<SphereCollider>();
        if (playerCollider == null)
        {
            Debug.LogError("Can't find Player Collider!");
        }

        if (ball == null)
        {
            ball = GetComponentInChildren<MeshRenderer>().gameObject.transform;
            if (ball == null)
            {
                Debug.LogError("Can't find Ball!");
            }
        }
    }

    private void FinishLogic()
    {
        rotatingState = 3;
        
        //reset collider
        playerCollider.center = Vector3.zero;
        playerCollider.radius *= 2;
        
        GameController.instance.Finish();
    }

    private void StartFinishMove(Transform hole)
    {
        movingState = 2;
        targetToMove = new Vector3(hole.localPosition.x, -transform.localScale.y / 4f, hole.localPosition.z);
    }

    private void EndUsualMove(Transform obstacle, Vector2 obstacleDimensions)
    {
        movingState = 0;
        rotatingState = 2;
        scalingState = 1;

        //set position
        Vector3 correctPosition = Vector3.zero;
        switch (directionTag)
        {
            case "BoardUp":
                correctPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, obstacle.localPosition.z - obstacleDimensions.y / 2f - transform.localScale.z / 2f);
                break;
            case "BoardDown":
                correctPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, obstacle.localPosition.z + obstacleDimensions.y / 2f + transform.localScale.z / 2f);
                break;
            case "BoardLeft":
                correctPosition = new Vector3(obstacle.localPosition.x + obstacleDimensions.x / 2f + transform.localScale.x / 2f, transform.localPosition.y, transform.localPosition.z);
                break;
            case "BoardRight":
                correctPosition = new Vector3(obstacle.localPosition.x - obstacleDimensions.x / 2f - transform.localScale.x / 2f, transform.localPosition.y, transform.localPosition.z);
                break;
            default:
                Debug.LogError("Wrong Direction Tag!");
                break;
        }
        transform.localPosition = correctPosition;

        //reset collider
        playerCollider.center = Vector3.zero;
        playerCollider.radius *= 2;

        //effect
        Instantiate(pandaEffect, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
    }

    private void Move()
    {
        if (movingState == 1)
        {
            transform.Translate(movementDirection * moveSpeed * Time.deltaTime);
            moveSpeed = Mathf.Lerp(moveSpeed, moveSpeedMax, interpolationSpeed * Time.deltaTime);
            interpolationSpeed = Mathf.Lerp(interpolationSpeed, interpolationSpeedMax, 15f * Time.deltaTime);
        }
        else if (movingState == 2)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetToMove, moveSpeed * 0.8f * Time.deltaTime);
            if (Vector3.Distance(transform.localPosition, targetToMove) < distanceOffset)
            {
                movingState = 0;
                transform.localPosition = targetToMove;

                FinishLogic();
            }
        }
        else if (movingState == 3)
        {
            transform.Translate(new Vector3(movementDirection.x, -1f, movementDirection.z) * moveSpeed * Time.deltaTime);
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, scaleSpeed * Time.deltaTime);
            if (transform.localScale == Vector3.zero)
            {
                movingState = 0;
                GameController.instance.GameOver();
            }
        }
    }

    private void Rotate()
    {
        if (rotatingState == 1)
        {
            switch (directionTag)
            {
                case "BoardUp":
                    ball.Rotate(Vector3.right * rotationSpeed * 100f * Time.deltaTime, Space.World);
                    break;
                case "BoardDown":
                    ball.Rotate(Vector3.left * rotationSpeed * 100f * Time.deltaTime, Space.World);
                    break;
                case "BoardLeft":
                    ball.Rotate(Vector3.forward * rotationSpeed * 100f * Time.deltaTime, Space.World);
                    break;
                case "BoardRight":
                    ball.Rotate(Vector3.back * rotationSpeed * 100f * Time.deltaTime, Space.World);
                    break;
                default:
                    Debug.LogError("Wrong Direction Tag!");
                    break;
            }
            rotationSpeed = Mathf.Lerp(rotationSpeed, rotationSpeedMax, interpolationSpeed * 0.5f * Time.deltaTime);
        }
        else if (rotatingState == 2 || rotatingState == 3)
        {
            if (rotatingState == 3 && currentRotatingTarget == Quaternion.identity)
            {
                currentRotatingTarget = Quaternion.Euler(rotatingTargets[5]);
            }
            else if (currentRotatingTarget == Quaternion.identity)
            {
                switch (directionTag)
                {
                    case "BoardUp":
                        currentRotatingTarget = Quaternion.Euler(rotatingTargets[1]);
                        break;
                    case "BoardDown":
                        currentRotatingTarget = Quaternion.Euler(rotatingTargets[2]);
                        break;
                    case "BoardLeft":
                        currentRotatingTarget = Quaternion.Euler(rotatingTargets[3]);
                        break;
                    case "BoardRight":
                        currentRotatingTarget = Quaternion.Euler(rotatingTargets[4]);
                        break;
                    default:
                        Debug.LogError("Wrong Direction Tag!");
                        break;
                }
            }

            ball.localRotation = Quaternion.Slerp(ball.localRotation, currentRotatingTarget, rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(ball.localRotation, currentRotatingTarget) < angleOffset)
            {
                ball.localRotation = currentRotatingTarget;
                currentRotatingTarget = Quaternion.identity;
                rotatingState = 0;
            }
        }
    }

    private void Scale()
    {
        if (scalingState == 1)
        {
            //remember position before scaling
            if (positionBeforeScaling == Vector3.zero)
            {
                positionBeforeScaling = transform.localPosition;
            }

            //scale to
            if (directionTag == "BoardUp" || directionTag == "BoardDown")
            {
                transform.localScale -= new Vector3(0, 0, scaleSpeed * Time.deltaTime);
                if (transform.localScale.z <= scaleValue)
                {
                    scalingState = 2;
                }

                if (directionTag == "BoardUp")
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + transform.localScale.z / 2f), scaleSpeed * Time.deltaTime);
                }
                else
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - transform.localScale.z / 2f), scaleSpeed * Time.deltaTime);
                }
            }
            else
            {
                transform.localScale -= new Vector3(scaleSpeed * Time.deltaTime, 0, 0);
                if (transform.localScale.x <= scaleValue)
                {
                    scalingState = 2;
                }

                if (directionTag == "BoardLeft")
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x - transform.localScale.x / 2f, transform.localPosition.y, transform.localPosition.z), scaleSpeed * Time.deltaTime);
                }
                else
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x + transform.localScale.x / 2f, transform.localPosition.y, transform.localPosition.z), scaleSpeed * Time.deltaTime);
                }
            }
        }
        else
        {
            //scale back
            if (directionTag == "BoardUp" || directionTag == "BoardDown")
            {
                transform.localScale += new Vector3(0, 0, scaleSpeed * Time.deltaTime);
                if (transform.localScale.z >= transform.localScale.x)
                {
                    scalingState = 0;
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.x);

                    transform.localPosition = positionBeforeScaling;
                    positionBeforeScaling = Vector3.zero;

                    //logic
                    GameController.instance.DecreaseMovableObjectsCounter();
                    return;
                }

                if (directionTag == "BoardUp")
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - transform.localScale.z / 2f), scaleSpeed * Time.deltaTime);
                }
                else
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + transform.localScale.z / 2f), scaleSpeed * Time.deltaTime);
                }
            }
            else
            {
                transform.localScale += new Vector3(scaleSpeed * Time.deltaTime, 0, 0);
                if (transform.localScale.x >= transform.localScale.z)
                {
                    scalingState = 0;
                    transform.localScale = new Vector3(transform.localScale.z, transform.localScale.y, transform.localScale.z);

                    transform.localPosition = positionBeforeScaling;
                    positionBeforeScaling = Vector3.zero;

                    //logic
                    GameController.instance.DecreaseMovableObjectsCounter();
                    return;
                }

                if (directionTag == "BoardLeft")
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x + transform.localScale.x / 2f, transform.localPosition.y, transform.localPosition.z), scaleSpeed * Time.deltaTime);
                }
                else
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x - transform.localScale.x / 2f, transform.localPosition.y, transform.localPosition.z), scaleSpeed * Time.deltaTime);
                }
            }
        }
    }

    public void StartMovement(string boardTag)
    {
        //reset targets
        currentRotatingTarget = Quaternion.identity;

        //check movability
        isDirectionBlocked[0] = Physics.Raycast(transform.position, Vector3.forward, transform.localScale.x / 2f + rayOffset);
        isDirectionBlocked[1] = Physics.Raycast(transform.position, Vector3.back, transform.localScale.x / 2f + rayOffset);
        isDirectionBlocked[2] = Physics.Raycast(transform.position, Vector3.left, transform.localScale.x / 2f + rayOffset);
        isDirectionBlocked[3] = Physics.Raycast(transform.position, Vector3.right, transform.localScale.x / 2f + rayOffset);

        directionTag = boardTag;

        bool canMove = true;
        switch (directionTag)
        {
            case "BoardUp":
                if (isDirectionBlocked[0]) canMove = false;
                break;
            case "BoardDown":
                if (isDirectionBlocked[1]) canMove = false;
                break;
            case "BoardLeft":
                if (isDirectionBlocked[2]) canMove = false;
                break;
            case "BoardRight":
                if (isDirectionBlocked[3]) canMove = false;
                break;
            default:
                Debug.LogError("Wrong Direction Tag!");
                break;
        }
        if (!canMove)
        {
            rotatingState = 2;

            GameController.instance.DecreaseMovableObjectsCounter();
            return;
        }

        //cnange trigger for correct movement
        playerCollider.radius /= 2f;
        switch (directionTag)
        {
            case "BoardUp":
                playerCollider.center = new Vector3(0, 0, playerCollider.radius);
                break;
            case "BoardDown":
                playerCollider.center = new Vector3(0, 0, -playerCollider.radius);
                break;
            case "BoardLeft":
                playerCollider.center = new Vector3(-playerCollider.radius, 0, 0);
                break;
            case "BoardRight":
                playerCollider.center = new Vector3(playerCollider.radius, 0, 0);
                break;
            default:
                Debug.LogError("Wrong Direction Tag!");
                break;
        }

        //start moving
        movingState = 1;
        rotatingState = 1;

        moveSpeed = 0;
        rotationSpeed = 0;
        interpolationSpeed = 0;

        switch (directionTag)
        {
            case "BoardUp":
                movementDirection = Vector3.forward;
                break;
            case "BoardDown":
                movementDirection = Vector3.back;
                break;
            case "BoardLeft":
                movementDirection = Vector3.left;
                break;
            case "BoardRight":
                movementDirection = Vector3.right;
                break;
            default:
                Debug.LogError("Wrong Direction Tag!");
                break;
        }
    }

    public void GameOver()
    {
        gameObject.SetActive(false);
    }

    public void SetPlayerOnLevelStart(Vector3 startPos, Vector3 starRot)
    {
        transform.localPosition = startPos;
        if (ball == null)
        {
            ball = GetComponentInChildren<MeshRenderer>().gameObject.transform;
        }
        ball.localRotation = Quaternion.Euler(starRot);
    }
}
