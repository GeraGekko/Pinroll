using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public GameObject pinCubeEffectPrefab;
    public GameObject boltPrefab;
    public Transform wayLineTransform;

    public bool isPinnedOnStart;
    public bool isFat;
    public ObstacleType obstacleType;
    public enum ObstacleType
    {
        Horizontal,
        Vertical
    }

    [Header("Parameters")]
    public float moveSpeedMax;
    public float interpolationSpeedMax;
    public float interactiveMovementDistance;
    public float scaleSpeed;
    public float scaleValue;
    public float boltMovingSpeed;
    public float boltScalingSpeed;
    public float wayLineSpeed;
    private float moveSpeed;
    private float interpolationSpeed;

    private BoxCollider obstacleCollider;
    private Transform bolt;

    private Vector2 dimensions;
    private Vector3 wayLineInitialPosition;
    private Vector3 wayLineInitialScale;
    private Vector3 wayLineScaleTarget;
    private Vector3 wayLineMoveTarget;
    private Vector3 movementDirection;
    private Vector3 interactiveMovementTarget = Vector3.zero;
    private float boltScaleValue;
    private float boltInitialScale;
    private bool[] isDirectionBlocked = new bool[2];
    private int movingState; //0 - NOT MOVING; 1 - MOVING; 2 - INTERACTIVE BUMP
    private int scalingState; // 0 - NOT SCALING; 1 - SCALING TO; 2 - SCALING BACK
    private int boltMovingState; // 0 - NOT MOVING; 1 - MOVING TO OBSTACLE
    private int boltScalingState; //0 - NOT SCALING; 1 - SCALING TO; 2 - SCALING BACK; 3 - INTERACTIVE SCALE TO; 4 - INTERACTIVE SCALE BACK
    private int wayLineScalingState; //0 - NOT SCALING; 1 - SCALING TO; 2 - SCALING BACK
    private string directionTag;
    private bool isPinned;
    private bool hasBeenTapped;

    private float rayOffset = 0.1f;
    private float distanceOffset = 0.05f;

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

        if (scalingState != 0)
        {
            Scale();
        }

        if (boltMovingState != 0)
        {
            BoltMove();
        }

        if (boltScalingState != 0)
        {
            BoltScale();
        }

        if (wayLineScalingState != 0)
        {
            WayLineScale();
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

        if (other.gameObject.CompareTag("Player"))
        {
            GameController.instance.GameOver();
            return;
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            BoxCollider wallCollider = other.gameObject.GetComponent<BoxCollider>();
            if (wallCollider != null)
            {
                EndUsualMove(other.gameObject.transform, new Vector2(wallCollider.size.x, wallCollider.size.z));
            }
        }
        else
        {
            //borders
            EndUsualMove(other.gameObject.transform, new Vector2(1, 1));
        }
    }

    private void InitOnStart()
    {
        obstacleCollider = GetComponent<BoxCollider>();
        if (obstacleCollider == null)
        {
            Debug.LogError("Can't find Obstacle Collider!");
        }

        if (wayLineTransform == null)
        {
            Debug.LogError("Forgot To Attach Way Line!");
        }
        wayLineInitialPosition = wayLineTransform.localPosition;
        wayLineInitialScale = wayLineTransform.localScale;
        wayLineScaleTarget = new Vector3(0, wayLineTransform.localScale.y, wayLineTransform.localScale.z);

        if (isFat)
        {
            boltInitialScale = 1.5f;
            boltScaleValue = 1.75f;
        }
        else
        {
            boltInitialScale = 1;
            boltScaleValue = 1.25f;
        }

        if (isPinnedOnStart)
        {
            PinOnStartLogic();
        }

        CalculateObstacleDimensions();
    }

    private void PinOnStartLogic()
    {
        isPinned = true;

        bolt = Instantiate(boltPrefab, transform).transform;
        bolt.localPosition = Vector3.zero;
        bolt.localScale = new Vector3(boltInitialScale, 1, boltInitialScale);

        wayLineTransform.localPosition = new Vector3(transform.localPosition.x, wayLineTransform.localPosition.y, transform.localPosition.z);
        wayLineTransform.localScale = new Vector3(0, wayLineTransform.localScale.y, wayLineTransform.localScale.z);
    }

    private void Scale()
    {
        if (scalingState == 1)
        {
            //scale to
            transform.localScale -= new Vector3(1, 0, 1) * scaleSpeed * Time.deltaTime;
            if (transform.localScale.x <= scaleValue)
            {
                scalingState = 2;
            }
        }
        else if (scalingState == 2)
        {
            //scale back
            transform.localScale += new Vector3(1, 0, 1) * scaleSpeed * 0.75f * Time.deltaTime;
            if (transform.localScale.x >= 1)
            {
                scalingState = 0;
                transform.localScale = Vector3.one;

                if (isPinned)
                {
                    boltScalingState = 1;
                }
                else
                {
                    boltScalingState = 2;
                }

                //effect
                Instantiate(pinCubeEffectPrefab, transform);
            }
        }
    }

    private void BoltScale()
    {
        if (boltScalingState == 1)
        {
            //scale to 1
            bolt.localScale += Vector3.one * boltScalingSpeed * Time.deltaTime;
            if (bolt.localScale.x >= boltInitialScale)
            {
                bolt.localScale = new Vector3(boltInitialScale, 1, boltInitialScale);
                
                boltScalingState = 0;
                boltMovingState = 1;
            }
        }
        else if (boltScalingState == 2)
        {
            //scale to 0
            bolt.localScale -= Vector3.one * boltScalingSpeed * 0.5f * Time.deltaTime;
            if (bolt.localScale.x <= 0)
            {
                boltScalingState = 0;

                Destroy(bolt.gameObject);
                bolt = null;

                //logic
                hasBeenTapped = false;
                GameController.instance.UnpinLogic(this);
            }
        }
        else if (boltScalingState == 3)
        {
            //interactive scale to
            bolt.localScale += new Vector3(1, 0, 1) * boltScalingSpeed * 0.2f * Time.deltaTime;
            if (bolt.localScale.x >= boltScaleValue)
            {
                boltScalingState = 4;
            }
        }
        else if (boltScalingState == 4)
        {
            //interactive scale back
            bolt.localScale -= new Vector3(1, 0, 1) * boltScalingSpeed * 0.2f * Time.deltaTime;
            if (bolt.localScale.x <= boltInitialScale)
            {
                bolt.localScale = new Vector3(boltInitialScale, 1, boltInitialScale);

                boltScalingState = 0;

                //logic
                hasBeenTapped = false;
                GameController.instance.PinLogic();
            }
        }
    }

    private void WayLineScale()
    {
        if (wayLineScalingState == 1)
        {
            wayLineTransform.localScale = Vector3.MoveTowards(wayLineTransform.localScale, wayLineScaleTarget, wayLineSpeed * Time.deltaTime);
            wayLineTransform.localPosition = Vector3.MoveTowards(wayLineTransform.localPosition, wayLineMoveTarget, wayLineSpeed * 3f * Time.deltaTime);
            if (wayLineTransform.localPosition == wayLineMoveTarget && wayLineTransform.localScale == wayLineScaleTarget)
            {
                wayLineScalingState = 0;
            }
        }
        else
        {
            wayLineTransform.localScale = Vector3.MoveTowards(wayLineTransform.localScale, wayLineInitialScale, wayLineSpeed * Time.deltaTime);
            wayLineTransform.localPosition = Vector3.MoveTowards(wayLineTransform.localPosition, wayLineInitialPosition, wayLineSpeed * 3.5f * Time.deltaTime);
            if (wayLineTransform.localPosition == wayLineInitialPosition && wayLineTransform.localScale == wayLineInitialScale)
            {
                wayLineScalingState = 0;
            }
        }
    }

    private void BoltMove()
    {
        if (boltMovingState == 1)
        {
            //move to
            bolt.localPosition = Vector3.MoveTowards(bolt.localPosition, Vector3.zero, boltMovingSpeed * Time.deltaTime);
            if (bolt.localPosition == Vector3.zero)
            {
                boltMovingState = 0;
                boltScalingState = 3;
            }
        }
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
            //interactive movement to
            if (interactiveMovementTarget == Vector3.zero)
            {
                //set target
                switch (directionTag)
                {
                    case "BoardUp":
                        interactiveMovementTarget = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - interactiveMovementDistance);
                        break;
                    case "BoardDown":
                        interactiveMovementTarget = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + interactiveMovementDistance);
                        break;
                    case "BoardLeft":
                        interactiveMovementTarget = new Vector3(transform.localPosition.x + interactiveMovementDistance, transform.localPosition.y, transform.localPosition.z);
                        break;
                    case "BoardRight":
                        interactiveMovementTarget = new Vector3(transform.localPosition.x - interactiveMovementDistance, transform.localPosition.y, transform.localPosition.z);
                        break;
                    default:
                        Debug.LogError("Wrong Direction Tag!");
                        break;
                }
            }
            transform.localPosition = Vector3.Lerp(transform.localPosition, interactiveMovementTarget, moveSpeed * 0.75f * Time.deltaTime);
            if (Vector3.Distance(transform.localPosition,interactiveMovementTarget) < distanceOffset)
            {
                transform.localPosition = interactiveMovementTarget;
                interactiveMovementTarget = Vector3.zero;

                movingState = 3;
            }
        }
        else if (movingState == 3)
        {
            //interactive movement back
            if (interactiveMovementTarget == Vector3.zero)
            {
                //set target
                switch (directionTag)
                {
                    case "BoardUp":
                        interactiveMovementTarget = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + interactiveMovementDistance);
                        break;
                    case "BoardDown":
                        interactiveMovementTarget = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - interactiveMovementDistance);
                        break;
                    case "BoardLeft":
                        interactiveMovementTarget = new Vector3(transform.localPosition.x - interactiveMovementDistance, transform.localPosition.y, transform.localPosition.z);
                        break;
                    case "BoardRight":
                        interactiveMovementTarget = new Vector3(transform.localPosition.x + interactiveMovementDistance, transform.localPosition.y, transform.localPosition.z);
                        break;
                    default:
                        Debug.LogError("Wrong Direction Tag!");
                        break;
                }
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, interactiveMovementTarget, moveSpeed * 0.5f * Time.deltaTime);
            if (Vector3.Distance(transform.localPosition, interactiveMovementTarget) < distanceOffset)
            {
                transform.localPosition = interactiveMovementTarget;
                interactiveMovementTarget = Vector3.zero;

                movingState = 0;

                //logic
                GameController.instance.DecreaseMovableObjectsCounter();
            }
        }
    }

    private void EndUsualMove(Transform obstacle, Vector2 obstacleDimensions)
    {
        //set position
        Vector3 correctPosition = Vector3.zero;
        switch (directionTag)
        {
            case "BoardUp":
                correctPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, obstacle.localPosition.z - obstacleDimensions.y / 2f - dimensions.y / 2f);
                break;
            case "BoardDown":
                correctPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, obstacle.localPosition.z + obstacleDimensions.y / 2f + dimensions.y / 2f);
                break;
            case "BoardLeft":
                correctPosition = new Vector3(obstacle.localPosition.x + obstacleDimensions.x / 2f + dimensions.x / 2f, transform.localPosition.y, transform.localPosition.z);
                break;
            case "BoardRight":
                correctPosition = new Vector3(obstacle.localPosition.x - obstacleDimensions.x / 2f - dimensions.x / 2f, transform.localPosition.y, transform.localPosition.z);
                break;
            default:
                Debug.LogError("Wrong Direction Tag!");
                break;
        }
        transform.localPosition = correctPosition;
        
        movingState = 2;

        //reset collider
        obstacleCollider.center = Vector3.zero;
        obstacleCollider.size = new Vector3(obstacleCollider.size.x * 2f, obstacleCollider.size.y, obstacleCollider.size.z * 2f);
    }

    public void StartMovement(string boardTag)
    {
        //check pinned status
        if (isPinned)
        {
            GameController.instance.DecreaseMovableObjectsCounter();
            return;
        }

        //check correct direction
        directionTag = boardTag;

        if ((obstacleType == ObstacleType.Horizontal && (directionTag == "BoardUp" || directionTag == "BoardDown")) ||
            (obstacleType == ObstacleType.Vertical && (directionTag == "BoardLeft" || directionTag == "BoardRight")))
        {
            GameController.instance.DecreaseMovableObjectsCounter();
            return;
        }

        //check movability
        if (obstacleType == ObstacleType.Horizontal)
        {
            isDirectionBlocked[0] = Physics.SphereCast(transform.position, 0.1f, Vector3.left, out _, GetObstacleDimensions().x / 2f + rayOffset);
            isDirectionBlocked[1] = Physics.SphereCast(transform.position, 0.1f, Vector3.right, out _, GetObstacleDimensions().x / 2f + rayOffset);
        }
        else
        {
            isDirectionBlocked[0] = Physics.SphereCast(transform.position, 0.1f, Vector3.forward, out _, GetObstacleDimensions().y / 2f + rayOffset);
            isDirectionBlocked[1] = Physics.SphereCast(transform.position, 0.1f, Vector3.back, out _, GetObstacleDimensions().y / 2f + rayOffset);
        }
        //Debug.Log(isDirectionBlocked[0] + " - " + isDirectionBlocked[1]);

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
                if (isDirectionBlocked[0]) canMove = false;
                break;
            case "BoardRight":
                if (isDirectionBlocked[1]) canMove = false;
                break;
            default:
                canMove = false;
                break;
        }

        if (!canMove)
        {
            GameController.instance.DecreaseMovableObjectsCounter();
            return;
        }

        //cnange trigger for correct movement
        obstacleCollider.size = new Vector3(obstacleCollider.size.x / 2f, obstacleCollider.size.y, obstacleCollider.size.z / 2f);
        switch (directionTag)
        {
            case "BoardUp":
                obstacleCollider.center = new Vector3(0, 0, obstacleCollider.size.z / 2f);
                break;
            case "BoardDown":
                obstacleCollider.center = new Vector3(0, 0, -obstacleCollider.size.z / 2f);
                break;
            case "BoardLeft":
                obstacleCollider.center = new Vector3(-obstacleCollider.size.x / 2f, 0, 0);
                break;
            case "BoardRight":
                obstacleCollider.center = new Vector3(obstacleCollider.size.x / 2f, 0, 0);
                break;
            default:
                Debug.LogError("Wrong Direction Tag!");
                break;
        }

        //start moving
        movingState = 1;

        moveSpeed = 0;
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

    public void CalculateObstacleDimensions()
    {
        dimensions = new Vector2(obstacleCollider.size.x, obstacleCollider.size.z);
    }

    public Vector2 GetObstacleDimensions()
    {
        return dimensions;
    }

    public void ChangePinCondition()
    {
        if (hasBeenTapped)
        {
            return;
        }
        hasBeenTapped = true;

        if (isPinned)
        {
            //unpin
            isPinned = false;
            if (bolt == null)
            {
                Debug.LogError("Can't find Bolt!");
            }

            //wayline
            wayLineScalingState = 2;
        }
        else
        {
            //pin
            isPinned = true;
            bolt = Instantiate(boltPrefab, transform).transform;

            //wayline
            wayLineScalingState = 1;
            wayLineMoveTarget = new Vector3(transform.localPosition.x, wayLineTransform.localPosition.y, transform.localPosition.z);
        }

        scalingState = 1;
    }
}
