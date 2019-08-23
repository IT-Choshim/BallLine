using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BallLine
{
    //Sort a List by current way point Id
    public class Sort : IComparer<GameObject>
    {
        int IComparer<GameObject>.Compare(GameObject _objA, GameObject _objB)
        {
            int t1 = _objA.GetComponent<BallController>().currentWayPointID;
            int t2 = _objB.GetComponent<BallController>().currentWayPointID;
            return t1.CompareTo(t2);
        }
    }

    public class BallController : MonoBehaviour
    {
        public List<GameObject> notExplosiveBallAhead;
        public List<GameObject> notExplosiveBallBehind;
        public EditorPathScript pathFollow;
        public GameObject triggerObecjt = null;
        public int currentWayPointID = 0;
        public float reachDistance;
        public float rotationSpeed = 5.0f;
        public GameObject pathName;
        public bool move;
        public bool isShootBall;
        public GameObject previousBall = null;
        public GameObject oldBall = null;
        public GameObject previousColorBall = null;
        public GameObject oldColorBall;
        public int sameColorRange;
        public int orderOfBall;
        public float currentAngle;

        GameObject triggerObject=null;

        public bool hasMakeConnection;

        public bool hasConnection=false;

        public bool isNodeBall;

        public bool isMoveToward=false;

        public bool isLeader=false;

        bool isBehind = false;
        GameObject connectObject;

        public float speed;
        bool destroy;
        bool hasCollision;
        public bool moveBack;
        float distance;
        public GameObject collisionObject;

        public bool isInterrupted;

        public bool isMoveBack;
        public bool isNewRange;
        int newRange;
        Rigidbody ballRigidbody;

        public int range;

        public bool isBoomBall;
        public bool isExplosive = false;
        public bool die;
        public float curAngle;

        ParticleSystem scoreEffect;
        ParticleSystem explosionEffect;

        Vector3 position1;
        Vector3 position2;

        public bool isMovingToPosition;
        public int orderMoveBack;
        public bool isMovingForward;
        public bool isMovingBack;

        public float scaleMagnitude=0;
        public float originalDistance = 0;
        public float currentDistance=0;

        int errorCount = 0;
        //Reset the ball to default setting
        public void ResetBall()
        {
            if (isLeader && oldBall != null)
                oldBall.GetComponent<BallController>().isLeader = true;
            isLeader = false;
            originalDistance = 0;
            isBehind = false;
            connectObject = null;
            hasConnection = false;
            hasMakeConnection = false;
            isMoveToward = false;
            if (previousBall != null)
                previousBall.GetComponent<BallController>().oldBall = null;
            if (oldBall != null)
                oldBall.GetComponent<BallController>().previousBall = null;
            if (oldColorBall != null && oldColorBall.GetComponent<BallController>().previousColorBall != null && oldColorBall.GetComponent<BallController>().previousColorBall == gameObject)
                oldColorBall.GetComponent<BallController>().previousColorBall = null;
            if (previousColorBall != null && previousColorBall.GetComponent<BallController>().oldColorBall != null && previousColorBall.GetComponent<BallController>().oldColorBall == gameObject)
                previousColorBall.GetComponent<BallController>().oldColorBall = null;
            StopAllCoroutines();
            isMovingToPosition=false;
            orderMoveBack = 0;
            isMovingForward = false;
            isMovingBack = false;
            isExplosive = false;
            die = false;
            pathFollow = null;
            currentWayPointID = 2;
            reachDistance = 0;
            rotationSpeed = 5.0f;
            pathName = null;
            move = false;
            isShootBall = false;
            previousBall = null;
            oldBall = null;
            previousColorBall = null;
            oldColorBall = null;
            sameColorRange = 0;
            orderOfBall = 0;
            isNodeBall = false;
            speed = 0;
            destroy = false;
            hasCollision = false;
            moveBack = false;
            distance = 0;
            collisionObject = null;
            isInterrupted = false;
            isMoveBack = false;
            isNewRange = false;
            newRange = 0;
            isBoomBall = false;
            scoreEffect = null;
            explosionEffect = null;
        }

        void OnEnable()
        {
            PlayerController.MoveWhenDie += MoveWhenDie;
            PlayerController.PlayerDied += PlayerDie;
            PlayerController.Collision += HandleEnterTrigger;
            PlayerController.EndCollision += HandleEndTrigger;
            PlayerController.SpeedUp += SpeedUp;
            PlayerController.FinishSpeedUp += FinishSpeedUp;
            PlayerController.StartMoveToward += StartMoveToWard;
        }

        void OnDisable()
        {
            PlayerController.SpeedUp -= SpeedUp;
            PlayerController.FinishSpeedUp -= FinishSpeedUp;
            PlayerController.MoveWhenDie -= MoveWhenDie;
            PlayerController.PlayerDied -= PlayerDie;
            PlayerController.Collision -= HandleEnterTrigger;
            PlayerController.EndCollision -= HandleEndTrigger;
            PlayerController.StartMoveToward -= StartMoveToWard;
        }

        void StartMoveToWard()
        {
            if(isMoveToward)
                StartCoroutine(MoveToward(1));
        }

        void SpeedUp()
        {
            if(Time.timeScale==1)
            speed = 10;
        }

        void FinishSpeedUp()
        {
            speed = GameManager.Instance.moveBallSpeed;
        }

        private void Awake()
        {
        }

        // Use this for initialization
        void Start()
        {
            notExplosiveBallAhead = new List<GameObject>();
            Init();
        }

        //Initial setting
        public void Init()
        {
            if(!isBoomBall)
                scaleMagnitude = (GetComponent<SpriteRenderer>().bounds.extents.x) * transform.lossyScale.x * 2.0f;
            if(isShootBall)
            reachDistance = 0;
            currentWayPointID = 1;
            scoreEffect = GameManager.Instance.playerController.scoreEffect;
            explosionEffect = GameManager.Instance.playerController.explosionEffect;
            ballRigidbody = gameObject.GetComponent<Rigidbody>();
            sameColorRange = 1;
            if (!isShootBall)
            {
                pathName = GameManager.Instance.playerController.path;
                pathFollow = pathName.GetComponent<EditorPathScript>();
            }
            if (!isBoomBall)
            {
                Vector3 dir = (pathFollow.pathPoints[2] - pathFollow.pathPoints[1]).normalized;
            }
        }

        void MoveWhenDie()
        {
            die = true;
        }

        //Reflect when collision with wall
        void OnCollisionEnter(Collision bla)
        {
            if (SoundManager.Instance.hitWallSound != null)
                SoundManager.Instance.PlaySound(SoundManager.Instance.hitWallSound);
            foreach (ContactPoint contact in bla.contacts)
            {
                transform.up = Vector3.Reflect(transform.up, contact.normal);
                return;
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            ballRigidbody.isKinematic = true;
            gameObject.GetComponent<Collider>().isTrigger = true;
        }

        //Eplosive when this ball is a boom ball
        void BlowUp(Vector3 center, float radius)
        {
            if (SoundManager.Instance.explosionSound != null)
                SoundManager.Instance.PlaySound(SoundManager.Instance.explosionSound);
            CreateExplosionEffect(transform.position, radius);
            GameManager.Instance.playerController.collision();
            Collider[] hitColliders = Physics.OverlapSphere(center, radius);
            int i = 0;
            int j = 0;
            int score = 0;
            GameObject aheadBall=null;
            GameObject behindBall=null;
            GameObject aheadColorBall = null;
            GameObject behindColorBall = null;

            //Mark all the balls in the bursting radius
            while (j < hitColliders.Length)
            {
                if (hitColliders[j].gameObject != gameObject && hitColliders[j].gameObject.tag != "Plane" && hitColliders[j].gameObject.tag !="Disable")
                {
                    hitColliders[j].gameObject.GetComponent<BallController>().isExplosive = true;
                }
                j++;
            }

            int k = 0;
            //Find the ball next to the burst radius and add to notExplosiveBall list
            while (k < hitColliders.Length)
            {
                if (hitColliders[k].gameObject != gameObject && hitColliders[k].gameObject.tag != "Plane" && hitColliders[k].gameObject.tag != "Disable")
                {
                    aheadBall = hitColliders[k].gameObject.GetComponent<BallController>().previousBall;
                    aheadColorBall = hitColliders[k].gameObject.GetComponent<BallController>().previousColorBall;

                    behindBall = hitColliders[k].gameObject.GetComponent<BallController>().oldBall;
                    behindColorBall = hitColliders[k].gameObject.GetComponent<BallController>().oldColorBall;

                    //Find the ball not explosion ahead
                    if (aheadBall != null && !aheadBall.GetComponent<BallController>().isExplosive)
                    {
                        if (notExplosiveBallAhead != null)
                        {
                            if (!Checkduplication(aheadBall, notExplosiveBallAhead))
                                notExplosiveBallAhead.Add(aheadBall);
                        }else
                            notExplosiveBallAhead.Add(aheadBall);
                    }
                    else if (aheadBall == null && aheadColorBall!=null && !aheadColorBall.GetComponent<BallController>().isExplosive)
                    {
                        if (notExplosiveBallAhead != null)
                        {
                            if (!Checkduplication(aheadColorBall, notExplosiveBallAhead))
                                notExplosiveBallAhead.Add(aheadColorBall);
                        }
                        else
                            notExplosiveBallAhead.Add(aheadColorBall);
                    }

                    //Find the ball not explosion behind
                    if (behindBall != null && !behindBall.GetComponent<BallController>().isExplosive)
                    {
                        if (notExplosiveBallBehind != null)
                        {
                            if (!Checkduplication(behindBall, notExplosiveBallBehind))
                                notExplosiveBallBehind.Add(behindBall);
                        }else
                            notExplosiveBallBehind.Add(behindBall);
                    }
                    else if (aheadBall == null && aheadColorBall != null && !aheadColorBall.GetComponent<BallController>().isExplosive)
                    {
                        if (notExplosiveBallBehind != null)
                        {
                            if (!Checkduplication(behindColorBall, notExplosiveBallBehind))
                                notExplosiveBallBehind.Add(behindColorBall);
                        }else
                            notExplosiveBallBehind.Add(behindColorBall);
                    }
                }
                k++;
            }

            //If there are balls next to the burst radius make connect between them with their old color ball
            if (notExplosiveBallAhead != null)
            {
                notExplosiveBallAhead.Sort((IComparer<GameObject>)new Sort());
                foreach (GameObject ball in notExplosiveBallAhead)
                {
                    BallController ballComponent = ball.GetComponent<BallController>();

                    ballComponent.CountSameColorRangeForward(1);
                    ballComponent.SetInterrupted(true,false);
                    if (ballComponent.oldBall != null)
                    {
                        GameObject old = ballComponent.oldBall;
                        old.GetComponent<BallController>().GetOldColorBallAfterBlowUp(ball);
                    }
                    else
                    {
                        GameObject old = ballComponent.oldColorBall;
                        old.GetComponent<BallController>().GetOldColorBallAfterBlowUp(ball);
                    }
                }
            }

            if (notExplosiveBallBehind != null)
            {
                foreach (GameObject ball in notExplosiveBallBehind)
                {
                    ball.GetComponent<BallController>().CountSameColorRangeBackWard(1);
                }
            }

            //Destroy ball in explosion radius
            while (i < hitColliders.Length)
            {
                if (hitColliders[i].gameObject != gameObject && hitColliders[i].gameObject.tag != "Plane" && hitColliders[i].gameObject.tag != "Disable")
                {
                    aheadBall = hitColliders[i].gameObject.GetComponent<BallController>().previousBall;
                    behindBall = hitColliders[i].gameObject.GetComponent<BallController>().oldBall;

                    if (aheadBall != null)
                        aheadBall.GetComponent<BallController>().oldBall = null;
                    if (behindBall != null)
                        behindBall.GetComponent<BallController>().previousBall = null;

                    hitColliders[i].gameObject.GetComponent<BallController>().ResetBall();
                    hitColliders[i].gameObject.SetActive(false);
                }
                    i++;
                score++;
            }
            //Pull back the ball if there is the same color ball
            bool haveMoveBack=false;
            int moveBackCount = 1;
            bool firstMoveBack = true;
            if (notExplosiveBallAhead != null)
            {
                for (int g=( notExplosiveBallAhead.Count-1); g>=0;g--)
                {
                    GameObject oldBall = notExplosiveBallAhead[g].gameObject.GetComponent<BallController>().oldColorBall;
                    if (oldBall!=null && notExplosiveBallAhead[g].gameObject.tag == oldBall.tag)
                    {
                        if (firstMoveBack)
                        {
                            notExplosiveBallAhead[g].gameObject.GetComponent<BallController>().isMoveBack = true;
                            notExplosiveBallAhead[g].gameObject.GetComponent<BallController>().MoveBack(true);
                            firstMoveBack = false;
                        }
                        else
                        {
                            notExplosiveBallAhead[g].gameObject.GetComponent<BallController>().WaitMoveBack(true,moveBackCount-1);
                        }
                        moveBackCount++;
                        haveMoveBack = true;
                    }
                }
                GameManager.Instance.playerController.waitMoveBackCount=moveBackCount;
                if (!haveMoveBack)
                    GameManager.Instance.playerController.onEndCollision();
            }
            else
                GameManager.Instance.playerController.onEndCollision();

            if (score > 0)
            {
                if(SoundManager.Instance.score!=null)
                    SoundManager.Instance.PlaySound(SoundManager.Instance.score);
                ScoreManager.Instance.AddScore(score);
                GameManager.Instance.playerController.AddScore(gameObject.transform.position, score);
            }
            GameManager.Instance.playerController.shoot = false;
            GameManager.Instance.playerController.shootBall = null;
            GameManager.Instance.playerController.trail.transform.SetParent(null);
            GameManager.Instance.playerController.trail.SetActive(false);
            GameManager.Instance.playerController.CreateShootBall();
            Destroy(gameObject);
        }

        bool Checkduplication(GameObject objectCheck,List<GameObject> listCheck)
        {
            foreach(GameObject objectToCheck in listCheck)
            {
                if(objectCheck==objectToCheck)
                {
                    return true;
                }
            }
            return false;
        }


        public GameObject BallAccess(bool isAheadBall,int orderFromThisBall,GameObject startBall)
        {
            GameObject ball = null;
            GameObject currentBall = startBall;

            if (isAheadBall)
            {
                while (orderFromThisBall > 0)
                {
                    if (currentBall.GetComponent<BallController>().previousBall != null)
                    {
                        ball = currentBall.GetComponent<BallController>().previousBall;
                        currentBall = ball;
                    }
                    else
                        if (currentBall.GetComponent<BallController>().previousColorBall != null)
                    {
                        ball = currentBall.GetComponent<BallController>().previousColorBall;
                        currentBall = ball;
                    }
                    else
                    {
                        ball = null;
                        break;
                    }

                    orderFromThisBall--;
                }
            }
            else
                while (orderFromThisBall > 0)
                {
                    if (currentBall.GetComponent<BallController>().oldBall != null)
                    {
                        ball = currentBall.GetComponent<BallController>().oldBall;
                        currentBall = ball;
                    }
                    else
                        if (currentBall.GetComponent<BallController>().oldColorBall != null)
                    {
                        ball = currentBall.GetComponent<BallController>().oldColorBall;
                        currentBall = ball;
                    }
                    else
                    {
                        ball = null;
                        break;
                    }

                    orderFromThisBall--;
                }
            return ball;
        }

        //Find old color ball after explosion by access one by one to old ball
        public void GetOldColorBallAfterBlowUp(GameObject aheadBlowUpBall)
        {
            //If this ball is explosive find next old ball
            if (isExplosive && oldBall != null)
                oldBall.GetComponent<BallController>().GetOldColorBallAfterBlowUp(aheadBlowUpBall);
            else
            //If next old ball do not exist and old colo ball is explosive find by next old ball of old color ball
            if (isExplosive && oldBall == null && oldColorBall != null && oldColorBall.GetComponent<BallController>().isExplosive)
                oldColorBall.GetComponent<BallController>().GetOldColorBallAfterBlowUp(aheadBlowUpBall);
            //If next old ball do not exist but old color ball is not explosive finish find old color ball
            else if (isExplosive && oldBall == null && oldColorBall != null && !oldColorBall.GetComponent<BallController>().isExplosive)
            {
                if (aheadBlowUpBall.tag == oldColorBall.tag)
                {
                    aheadBlowUpBall.GetComponent<BallController>().oldColorBall = oldColorBall;
                    oldColorBall.GetComponent<BallController>().previousColorBall = aheadBlowUpBall;
                }else
                {
                    aheadBlowUpBall.GetComponent<BallController>().SetOldColorBall(oldColorBall);
                    oldColorBall.GetComponent<BallController>().SetPreviousColorBall(aheadBlowUpBall);
                }
            }
            else if (!isExplosive)
            {
                if (aheadBlowUpBall.tag == gameObject.tag)
                {
                    aheadBlowUpBall.GetComponent<BallController>().oldColorBall = gameObject;
                    previousColorBall = aheadBlowUpBall;
                }else
                {
                    aheadBlowUpBall.GetComponent<BallController>().SetOldColorBall(gameObject);
                    SetPreviousColorBall(aheadBlowUpBall);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            triggerObecjt = other.gameObject;
            BallController otherComponent=other.gameObject.GetComponent<BallController>();
            ballRigidbody.isKinematic = true;
            gameObject.GetComponent<Collider>().isTrigger = true;

            //destroy ball after move to die position
            if(other.gameObject.tag=="Disable")
            {
                if (isLeader && oldBall != null)
                    oldBall.GetComponent<BallController>().isLeader = true;
                if (gameObject.activeSelf)
                {
                    ResetBall();
                    gameObject.SetActive(false);
                }
            }      

            //destroy ball when trigger with wall
            if (isShootBall && other.gameObject.tag == "Plane")
            {
                if (SoundManager.Instance.hitWallSound != null)
                    SoundManager.Instance.PlaySound(SoundManager.Instance.hitWallSound);
                GameManager.Instance.playerController.trail.transform.SetParent(null);
                GameManager.Instance.playerController.trail.SetActive(false);
                GameManager.Instance.playerController.shoot = false;
                GameManager.Instance.playerController.shootBall = null;
                GameManager.Instance.playerController.CreateShootBall();
                ResetBall();
                gameObject.SetActive(false);
            }

            //Blow up the boom when collision with other ball
            if (isBoomBall && other.gameObject.tag == "Plane" && !hasCollision)
            {
                BlowUp(gameObject.transform.position, GameManager.Instance.bombBallExplosionRadius);
                hasCollision = true;
            }

            if (isBoomBall && !hasCollision && other.gameObject.tag != "Disable")
            {
                BlowUp(gameObject.transform.position, GameManager.Instance.bombBallExplosionRadius);
                hasCollision = true;
            }

            //Trigger with ball and move this ball into the path
            if (other.gameObject.tag != "Disable" && !isBoomBall && isShootBall 
                && !hasCollision && other.gameObject.tag != "Plane" 
                && !otherComponent.isBoomBall)
            {
                if (SoundManager.Instance.hitBallSound != null)
                    SoundManager.Instance.PlaySound(SoundManager.Instance.hitBallSound);
                //Compare the distance from behind and ahead to add the ball to behind or ahead of this ball
                float oldDistance=0;
                float previousDistance = 0;
                Vector3 comparePosition = Vector3.zero;
                if (otherComponent.previousBall != null)
                    comparePosition = gameObject.transform.position + gameObject.transform.up * GameManager.Instance.shootBallSpeed * Time.deltaTime * 0.25f;
                else
                    comparePosition = gameObject.transform.position;
                oldDistance = Vector3.Distance(comparePosition, Vector3.MoveTowards(other.gameObject.transform.position, pathFollow.pathPoints[otherComponent.currentWayPointID-1], (1.5f / 15) * 10));
                previousDistance = Vector3.Distance(comparePosition, Vector3.MoveTowards(other.gameObject.transform.position, pathFollow.pathPoints[otherComponent.currentWayPointID], (1.5f / 15) * 10));

                if ((previousDistance < oldDistance)) //||(other.gameObject.GetComponent<BallController>().previousBall == null && previousDistance > oldDistance * 0.9f))
                    if (otherComponent.previousBall != null)
                    {
                        AddBallToPath(otherComponent.previousBall, false);
                    }
                    else
                    {
                        AddBallToPath(other.gameObject, true);
                    }
                else
                {
                    AddBallToPath(other.gameObject, false);
                }
                triggerObject = other.gameObject;
                hasCollision = true;
            } 
            //Push the ball a head if this ball is moving and the ball ahead is interrupted
            if (other.gameObject.tag != "Disable" && !isBoomBall && other.gameObject.tag != "Plane" 
                && move
                && !isMovingToPosition && !otherComponent.isMovingToPosition
                && otherComponent.isInterrupted && !isInterrupted && !isShootBall 
                && !otherComponent.isShootBall && previousBall == null 
                && otherComponent.oldBall == null 
                && !otherComponent.isMoveBack 
                && !otherComponent.moveBack 
                && !otherComponent.isBoomBall
                && !isMovingForward)
            {
                PushPreviousBall(other);
            }
            
            //Stop this ball from moving back when collision with the behind ball
            if (other.gameObject.tag != "Disable" && !isBoomBall && other.gameObject.tag != "Plane" 
                && oldBall == null && isMoveBack && !isMovingToPosition
                && otherComponent.previousBall == null 
                && other.gameObject.tag == gameObject.tag 
                && !otherComponent.isBoomBall
                && !otherComponent.moveBack
                && !otherComponent.isMovingBack)
            {
                //GameManager.Instance.playerController.capsule.transform.position = other.gameObject.transform.position;
                StopPullBallBack(other);
            }

            //when this ball is moving to position in path and collision with the ball behind then make connect between them
            if (other.gameObject.tag != "Disable" && other.gameObject.tag != "Plane" && hasCollision 
                && other!=triggerObject && hasCollision
                && isMovingToPosition && isInterrupted && oldBall == null
                && otherComponent.previousBall == null && previousBall != null
                && other.gameObject != previousBall
                && other.gameObject != oldBall
                && !otherComponent.isShootBall)
            {
                hasConnection = true;
                isBehind = false;
                connectObject=other.gameObject;
            }

            //when this ball is moving forward cause has a ball add to this path, then collision with the interrupted ball ahead and make connect between them
            if (other.gameObject.tag != "Disable" && other.gameObject.tag != "Plane"
                && (isMovingForward|| 
                    (!isMoveBack && !isMovingBack && !isMovingForward 
                    && !move && other.gameObject == previousColorBall 
                    && gameObject == other.gameObject.GetComponent<BallController>().oldColorBall))
                && otherComponent.oldBall == null
                && !otherComponent.isMovingToPosition
                && !otherComponent.isMovingForward
                && !otherComponent.isMovingBack && !otherComponent.isMoveBack
                && previousBall == null
                && !otherComponent.isShootBall)
            {
                if (!isMovingToPosition && (other.gameObject == previousColorBall || gameObject == other.gameObject.GetComponent<BallController>().oldColorBall))
                {
                    hasConnection = true;
                    MakeConnectBetweenBall(gameObject, other.gameObject, true,false);
                    otherComponent.hasMakeConnection = true;
                }
            }
            //When move to position in the path and collision with the ball ahead then fix position and make connection
            if (other.gameObject.tag != "Disable" && other.gameObject.tag != "Plane"
                && isMovingToPosition
                && other.gameObject!=triggerObject
                && otherComponent.isInterrupted
                && otherComponent.oldBall == null
                && previousBall == null && hasCollision && oldBall!=null
                && !otherComponent.isShootBall)
            {
                if (isMovingToPosition && !isMovingForward)
                {
                    hasConnection = true;
                    isBehind = true;
                    connectObject = other.gameObject;
                    isInterrupted = oldBall.GetComponent<BallController>().isInterrupted;
                }
            }

        }


        //public void FixSpacingForward()
        //{
        //    if (oldBall != null)
        //        FixSpacingBetweenBalls();
        //    if (previousBall != null)
        //        previousBall.GetComponent<BallController>().FixSpacingForward();
        //}

        //Make Connect between two balls and destroy it when there are 3 balls same color
        public void MakeConnectBetweenBall(GameObject behindBall,GameObject aheadBall,bool checkDestroy,bool moveAfterCheck)
        {
            BallController aheadComponent = aheadBall.GetComponent<BallController>();
            BallController behindComponent = behindBall.GetComponent<BallController>();
            GameObject old = behindComponent.oldBall;
            if(old!=null)
                aheadComponent.SetInterrupted(old.GetComponent<BallController>().isInterrupted,false);
            else
                aheadComponent.SetInterrupted(behindComponent.isInterrupted,false);

            behindComponent.previousBall = aheadBall;
            aheadComponent.oldBall = behindBall;

            if (behindBall.tag != aheadBall.tag)
            {
                behindComponent.SetPreviousColorBall(aheadBall);
                aheadComponent.SetOldColorBall(behindBall);
                if (moveAfterCheck)
                    GameManager.Instance.playerController.onEndCollision();
            } else
            {
                aheadComponent.SetOldColorBall(behindComponent.oldColorBall);
                behindComponent.SetPreviousColorBall(aheadComponent.previousColorBall);
                if (((aheadComponent.sameColorRange + behindComponent.sameColorRange) >= 3) && checkDestroy)
                {
                    GameManager.Instance.playerController.scoreRange = aheadComponent.sameColorRange + behindComponent.sameColorRange;
                    //behindComponent.destroy = true;
                    if (aheadComponent.isMovingToPosition)
                    {
                        aheadComponent.destroy = true;
                        aheadComponent.collisionObject = aheadBall;
                    }
                    else
                        if (behindComponent.isMovingToPosition)
                    {
                        behindComponent.destroy = true;
                        behindComponent.collisionObject = behindBall;
                    }
                    else
                    {
                        aheadComponent.isNodeBall = true;
                        aheadComponent.DestroySameColorBall(false);
                    }
                }
                else
                {
                    behindComponent.ResetColorRange();
                    if (moveAfterCheck)
                        GameManager.Instance.playerController.onEndCollision();
                }
            }
        }

        //Create paticle effect
        void CreateScoreEffect(Vector3 position,Quaternion rotation,int range)
        {
            position.z -= 1;
            var score = scoreEffect.shape;
            score.scale = new Vector3(scaleMagnitude * range, scoreEffect.shape.scale.y, scoreEffect.shape.scale.z);
            //Vector3 angle = rotation.eulerAngles() ;
            //score.rotation = angle;
            scoreEffect.transform.position = position;
            //Color scoreColor = scoreEffect.GetComponent<Renderer>().material.GetColor("_Color");
            Material scoreMaterial= scoreEffect.GetComponent<Renderer>().material;
            Sprite scoreTexture =gameObject.GetComponent<SpriteRenderer>().sprite;
            scoreMaterial.SetTexture("_MainTex", scoreTexture.texture);
            scoreEffect.Play();
        }

        void CreateExplosionEffect(Vector3 position, float radius)
        {
            position.z -= 1;
            var explosionShape = explosionEffect.shape;
            explosionShape.radius = radius;
            explosionEffect.transform.position = position;
            //Color scoreColor = gameObject.GetComponent<Renderer>().material.GetColor("_Color");
            //scoreEffect.GetComponent<Renderer>().material.SetColor("_Color", scoreColor);
            explosionEffect.Play();
        }

        //Push the ball ahead to move and make the connect
        void PushPreviousBall(Collider other)
        {
            GameManager.Instance.playerController.hasEarnCoin = false;
            GameManager.Instance.playerController.comboScore = 0;
            isLeader = false;
            BallController otherComponent = other.gameObject.GetComponent<BallController>();
            if (otherComponent.currentWayPointID < currentWayPointID)
            {
                otherComponent.currentWayPointID=currentWayPointID;
                if (otherComponent.currentWayPointID > (pathFollow.pathPoints.Length - 1))
                {
                    otherComponent.currentWayPointID = pathFollow.pathPoints.Length - 1;
                }
            }
            MakeConnectBetweenBall(gameObject, other.gameObject, false, false);
            StartCoroutine(Example(otherComponent));
            
            GameManager.Instance.playerController.EndTrigger();
        }
        IEnumerator Example(BallController otherComponent)
        {
            yield return new WaitForEndOfFrame();
            otherComponent.FixBeforeMove(true, gameObject.transform.position, currentWayPointID);
        }
        //If collision with the same ball color then stop this ball from moving back and make the connect
        void StopPullBallBack(Collider other)
        {
            BallController otherComponent = other.gameObject.GetComponent<BallController>();
            isMoveBack = false;
            MoveBack(false);
            isMovingBack = false;
            GameManager.Instance.playerController.StopMoveBack();
            SetInterrupted(otherComponent.isInterrupted,false);
            oldBall = other.gameObject;
            otherComponent.previousBall = gameObject;

            otherComponent.SetPreviousColorBall(previousColorBall);
            SetOldColorBall(otherComponent.oldColorBall);
            GameManager.Instance.playerController.waitMoveBackCount--;
            if (GameManager.Instance.playerController.waitMoveBackCount <= 0)
            {
                GameManager.Instance.playerController.waitMoveBackCount = 0;
            }

            newRange = other.gameObject.GetComponent<BallController>().sameColorRange + sameColorRange;
            //Check if the new same color range is larger than 3 if yes then continue destroy the same color ball
            if (newRange >= 3)
            {
                destroy = true;
                isNodeBall = true;
                GameManager.Instance.playerController.newRange = newRange;
                GameManager.Instance.playerController.comboScore++;
                int combo = GameManager.Instance.playerController.comboScore;
                bool hasEarnCoin=GameManager.Instance.playerController.hasEarnCoin;
                if (combo >= GameManager.Instance.earnCoinAtComboScore)
                {
                    CoinEarned earnCoinOption = GameManager.Instance.monetizationOptions;
                    switch (earnCoinOption) {
                        case CoinEarned.OnceCombo:
                            if (!hasEarnCoin)
                            {
                                GameManager.Instance.playerController.hasEarnCoin = true;
                                if (SoundManager.Instance.coin != null)
                                    SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
                                CoinManager.Instance.AddCoins(GameManager.Instance.coinEarned);
                            }
                            break;
                        case CoinEarned.EachCombo:
                            if (SoundManager.Instance.coin != null)
                                SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
                            CoinManager.Instance.AddCoins(GameManager.Instance.coinEarned);
                            break;
                        case CoinEarned.IncreaseEachCombo:
                            int multi = combo* GameManager.Instance.amountCoinIncrease;
                            if(GameManager.Instance.earnCoinAtComboScore==1)
                                multi=GameManager.Instance.amountCoinIncrease * (combo - GameManager.Instance.earnCoinAtComboScore);
                            if (SoundManager.Instance.coin != null)
                                SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
                            CoinManager.Instance.AddCoins(GameManager.Instance.coinEarned*multi);
                            break;
                    }

                }
                if (combo >= GameManager.Instance.createBombBallAtComboScore)
                    GameManager.Instance.playerController.CreateBoomBall();
                DestroySameColorBall(true);
                int score = combo * 10;
                if (score <= 0)
                    score = 10;
                GameManager.Instance.playerController.scoreRange = newRange;
                if (SoundManager.Instance.score != null)
                    SoundManager.Instance.PlaySound(SoundManager.Instance.score);
                ScoreManager.Instance.AddScore(score);
                GameManager.Instance.playerController.AddScore(gameObject.transform.position,score);
                //***uncomment the cobe below to use pass level when enough score feature

                //GameManager.Instance.playerController.CheckScore();
            }
            //if not then call event EndMoveBack to start move forward or make the other ball start move back
            else
            {
                FixBeforeMove(true, other.gameObject.transform.position, otherComponent.currentWayPointID);
                GameManager.Instance.playerController.EndMoveBack();
                GameManager.Instance.playerController.comboScore = 0;
                ResetColorRange();
            }
        }

        //Subscribe or unsubscribe event to know if this ball will move back or not
        public void WaitMoveBack(bool isWait,int order)
        {
            if (isWait)
            {
                orderMoveBack = order;
                PlayerController.BeginNextMoveBack +=BeginNextMoveBack ;
            }else
            {
                PlayerController.BeginNextMoveBack -= BeginNextMoveBack;
            }
        }

        //Stop moving back if the ball is moving back and event is raise
        void StopMoveBack()
        {
                PlayerController.FinishMoveBack -= StopMoveBack;
                PlayerController.StartMoveback -= BeginMoveBack;
                isMovingBack = false;
                currentWayPointID++;
                if (pathFollow!=null && pathFollow.pathPoints!=null && currentWayPointID > pathFollow.pathPoints.Length - 1)
                    currentWayPointID = pathFollow.pathPoints.Length - 1;
        }

        //If event is raise oderMoveBack will subtract by one to know if this is the turn for this ball to move back 
        void BeginNextMoveBack()
        {
            orderMoveBack--;
            if (orderMoveBack <= 0)
            {
                PlayerController.BeginNextMoveBack -= BeginNextMoveBack;
                if (oldColorBall != null && oldColorBall.tag == gameObject.tag)
                {
                    GameManager.Instance.playerController.waitMoveBackCount++;
                    isMoveBack = true;
                    MoveBack(true);
                }
            }
        }

        public Vector3 CalculateDistanceBehind(GameObject PositionToCalculate)
        {
            Vector3 result = Vector3.zero;
            BallController thisComponent = PositionToCalculate.GetComponent<BallController>();
            float distanceBehind = Vector3.Distance(PositionToCalculate.transform.position, pathFollow.pathPoints[thisComponent.currentWayPointID-1]);
            if (distanceBehind >= scaleMagnitude * 0.5f)
            {
                Vector3 direction = pathFollow.pathPoints[thisComponent.currentWayPointID-1] - pathFollow.pathPoints[thisComponent.currentWayPointID];
                currentWayPointID = thisComponent.currentWayPointID;
                result = PositionToCalculate.transform.TransformPoint(direction.normalized * scaleMagnitude);
            }
            else
            {
                Vector3 direction = pathFollow.pathPoints[thisComponent.currentWayPointID -2] - pathFollow.pathPoints[thisComponent.currentWayPointID-1];
                currentWayPointID = thisComponent.currentWayPointID - 1;
                result = pathFollow.pathPoints[thisComponent.currentWayPointID-1] + direction.normalized * (scaleMagnitude - distance);
            }
            return result;
        }

        //Calculate to find position a head of this ball if this ball's ahead ball is null
        public Vector3 CalculateDistanceAhead(Vector3 PositionToCalculate,int currentwaypoin,out int Waypoint)
        {
            Vector3 result=Vector3.zero;
            //BallController thisComponent = PositionToCalculate.GetComponent<BallController>();
            float distanceAhead = Vector3.Distance(PositionToCalculate, pathFollow.pathPoints[currentwaypoin]);
            float angle = 0;
            if(currentwaypoin < pathFollow.pathPoints.Length-1)
                angle = Vector2.Angle(pathFollow.pathPoints[currentwaypoin] - PositionToCalculate, pathFollow.pathPoints[currentwaypoin + 1] - pathFollow.pathPoints[currentwaypoin]);
            Waypoint = 0;
            if (distanceAhead>=scaleMagnitude && (angle<=85 || angle>=95))
            {
                Vector3 direction = pathFollow.pathPoints[currentwaypoin] - PositionToCalculate;
                Waypoint = currentwaypoin;
                result=PositionToCalculate+(direction.normalized * scaleMagnitude);
            }else
            if(distanceAhead == scaleMagnitude && (angle <= 85 || angle >= 95))
            {
                result = pathFollow.pathPoints[currentwaypoin];
                Waypoint = currentwaypoin + 1;
            }
            else
            if(angle <= 85 || angle >= 95)
            {
                Vector3 direction = pathFollow.pathPoints[currentwaypoin + 1] - pathFollow.pathPoints[currentwaypoin];
                Waypoint = currentwaypoin + 1;
                result= pathFollow.pathPoints[currentwaypoin] +direction.normalized * (scaleMagnitude- distanceAhead);
            }

            if (distanceAhead > scaleMagnitude && angle > 85 && angle < 95)
            {
                Vector3 direction = pathFollow.pathPoints[currentwaypoin] - PositionToCalculate;
                Waypoint = currentwaypoin;
                result = PositionToCalculate+(direction.normalized * scaleMagnitude);
            }else
            if (distanceAhead == scaleMagnitude && angle > 85 && angle < 95)
            {
                result = pathFollow.pathPoints[currentwaypoin];
                Waypoint = currentwaypoin + 1;
            }
            else
            if (angle > 85 && angle < 95)
            {
                Vector3 direction = pathFollow.pathPoints[currentwaypoin + 1] - pathFollow.pathPoints[currentwaypoin];
                Waypoint = currentwaypoin + 1;
                result = pathFollow.pathPoints[currentwaypoin] + direction.normalized * (scaleMagnitude - distanceAhead);
            }
            return result;
        }

        //add this ball to the moving path
        void AddBallToPath(GameObject other,bool isPreviousNull)
        {
            BallController otherBallComponent = other.gameObject.GetComponent<BallController>();

            if (otherBallComponent.isInterrupted)
            {
                isInterrupted = true;
            }
            GameManager.Instance.playerController.collision();
            //Move the ball into path
            if (otherBallComponent.oldBall != null && !isPreviousNull)
            {
                otherBallComponent.MoveFoward(1);
                currentWayPointID = otherBallComponent.currentWayPointID;
                StartCoroutine(MoveToPosition((other.gameObject.transform.position - otherBallComponent.oldBall.transform.position).normalized * 1 + other.gameObject.GetComponent<BallController>().oldBall.transform.position));
            }
            else if(!isPreviousNull && otherBallComponent.oldBall == null)
            {
                float distance = Vector3.Distance(otherBallComponent.oldColorBall.transform.position
                    ,other.gameObject.transform.position);
                if (distance > scaleMagnitude*2)
                {
                    //currentWayPointID = otherBallComponent.behindWayPoint;
                    StartCoroutine(MoveToPosition(CalculateDistanceBehind(other)));//otherBallComponent.behindPosition.transform.position));//Vector3.MoveTowards(other.transform.position,pathFollow.pathPoints[other.GetComponent<BallController>().currentWayPointID - 1], (1.5f / 15) *10)));
                }else
                {
                    //currentWayPointID = oldColorComponent.aheadWayPoint;
                    StartCoroutine(MoveToPosition(CalculateDistanceAhead(otherBallComponent.oldColorBall.transform.position
                        , otherBallComponent.oldColorBall.GetComponent<BallController>().currentWayPointID
                        , out currentWayPointID)));//oldColorComponent.aheadPosition.transform.position));//Vector3.MoveTowards(other.transform.position,pathFollow.pathPoints[other.GetComponent<BallController>().currentWayPointID - 1], (1.5f / 15) *10)));
                }
            }
            if (isPreviousNull)
            {
                //StartCoroutine( MoveToward(aheadPosition));
                StartCoroutine(MoveToPosition(CalculateDistanceAhead(other.transform.position,other.GetComponent<BallController>().currentWayPointID,out currentWayPointID)));//otherBallComponent.aheadPosition.transform.position));
            }

            //Set connect to the older ball in path
            GameObject backwardBall;
            if (isPreviousNull)
                backwardBall = other.gameObject;
            else
                backwardBall = otherBallComponent.oldBall;

            //Setup to destroy same color ahead balls after move to path
            if (otherBallComponent.sameColorRange >= 2 && other.gameObject.tag == gameObject.tag)
            {
                GameManager.Instance.playerController.scoreRange = otherBallComponent.sameColorRange + 1;
                destroy = true;
                collisionObject = other.gameObject;
            }
            //or same color behind balls
            else if (backwardBall != null && backwardBall.GetComponent<BallController>().sameColorRange >= 2 && backwardBall.tag == gameObject.tag)
            {
                GameManager.Instance.playerController.scoreRange = backwardBall.GetComponent<BallController>().sameColorRange + 1;
                destroy = true;
                collisionObject = backwardBall;
            }
            //Setup if not destroy same color balls
            else
            {
                if (!isPreviousNull)
                {
                    if (otherBallComponent.oldBall != null)
                    {
                        var older = otherBallComponent.oldBall;
                        oldBall = older;
                        older.GetComponent<BallController>().previousBall = gameObject;
                    }

                    previousBall = other.gameObject;
                    other.gameObject.GetComponent<BallController>().oldBall = gameObject;

                }
                else
                {
                    oldBall = other.gameObject;
                    otherBallComponent.previousBall = gameObject;
                    previousBall = null;
                }
                    speed = GameManager.Instance.moveBallSpeed;
                    pathName = GameManager.Instance.playerController.path;
                    pathFollow = pathName.GetComponent<EditorPathScript>();

                    isShootBall = false;

                //Set same color ball range when split the same color ball chain
                if (previousBall != null && oldBall != null)

                    if (otherBallComponent.sameColorRange > 1 && previousBall.tag != gameObject.tag && oldBall.tag != gameObject.tag && previousBall.tag == oldBall.tag)
                    {
                        if (oldBall != null)
                        {
                            oldBall.GetComponent<BallController>().SetRangeColorBallBackward(other.gameObject.GetComponent<BallController>().sameColorRange - other.gameObject.GetComponent<BallController>().orderOfBall);
                            oldBall.GetComponent<BallController>().SetOrderOfBallBackward(other.gameObject.GetComponent<BallController>().sameColorRange - other.gameObject.GetComponent<BallController>().orderOfBall, (other.gameObject.GetComponent<BallController>().sameColorRange - other.gameObject.GetComponent<BallController>().orderOfBall) - 1);
                        }
                        previousBall.GetComponent<BallController>().SetRangeColorBallForward(other.gameObject.GetComponent<BallController>().orderOfBall);
                        previousBall.GetComponent<BallController>().SetOrderOfBallForward(other.gameObject.GetComponent<BallController>().orderOfBall);
                    }

                //Set up the connect between old ball and previous ball if the ball ahead is same color as this color
                if (previousBall != null && previousBall.tag == gameObject.tag)
                {
                    previousColorBall = previousBall.GetComponent<BallController>().previousColorBall;
                    oldColorBall = previousBall.GetComponent<BallController>().oldColorBall;
                    if (oldBall != null)
                        oldBall.GetComponent<BallController>().SetPreviousColorBall(gameObject);
                    else 
                        oldColorBall.GetComponent<BallController>().SetPreviousColorBall(gameObject);
                }
                // or the ball behind is same color as this color
                else
                if (oldBall != null && oldBall.tag == gameObject.tag)
                {
                    previousColorBall = oldBall.GetComponent<BallController>().previousColorBall;
                    oldColorBall = oldBall.GetComponent<BallController>().oldColorBall;
                    if (previousBall != null)
                        previousBall.GetComponent<BallController>().SetOldColorBall(gameObject);
                    else
                    if (previousColorBall != null)
                        previousColorBall.GetComponent<BallController>().SetOldColorBall(gameObject);
                }
                //or the ball behind and ahead neither same color as this color
                else
                {
                    //Set connect between this ball and the ball ahead if the ball ahead is exist 
                    if (previousBall != null)
                    {
                        previousColorBall = previousBall;
                        if(oldBall==null)
                        {
                            oldColorBall = previousBall.GetComponent<BallController>().oldColorBall;
                            oldColorBall.GetComponent<BallController>().SetPreviousColorBall(gameObject);
                        }
                        previousBall.GetComponent<BallController>().oldColorBall = gameObject;
                    }
                    //Set connect between this ball and the ball behind if the ball behind is exist
                    if (oldBall != null)
                    {
                        oldColorBall = oldBall;
                        if (previousBall == null )
                        {
                            previousColorBall = oldBall.GetComponent<BallController>().previousColorBall;
                            if(previousColorBall != null)
                            previousColorBall.GetComponent<BallController>().SetOldColorBall(gameObject);
                        }
                        oldBall.GetComponent<BallController>().SetPreviousColorBall (gameObject);
                    }
                }

                //Increase same color ball range if there is a ball same color with this ball
                ResetColorRange();
            }
            GameManager.Instance.playerController.shoot = false;
            GameManager.Instance.playerController.shootBall = null;
            GameManager.Instance.playerController.CreateShootBall();
        }

        //Set range same color ball increase by one if there is a same color ball next to this ball
        void ResetColorRange()
        {
            if (previousBall != null && previousBall.tag == gameObject.tag)
            {
                sameColorRange++;
                previousBall.GetComponent<BallController>().sameColorRange++;
                SetOrderOfBallForward(sameColorRange);
            }
            else
            if (oldBall != null && oldBall.tag == gameObject.tag)
            {
                sameColorRange++;
                oldBall.GetComponent<BallController>().sameColorRange++;
                SetOrderOfBallBackward(sameColorRange, sameColorRange - 1);
            }
        }

        //fix position before move to avoid jerk
        public void FixBeforeMove(bool isFixThisBall,Vector3 positionFix,int waypointID)
        {
            Vector3 fixPosition = Vector3.zero;
            int outPutWayPoint = 0;
                fixPosition = CalculateDistanceAhead(positionFix
                        , waypointID, out outPutWayPoint);
                transform.position = fixPosition;
                currentWayPointID = outPutWayPoint;
            transform.position = Vector3.MoveTowards(transform.position, pathFollow.pathPoints[currentWayPointID],Time.deltaTime*speed);
            if (previousBall != null)
                previousBall.GetComponent<BallController>().FixBeforeMove(true,fixPosition,outPutWayPoint);
            else
                transform.position = Vector3.MoveTowards(transform.position, pathFollow.pathPoints[currentWayPointID], Time.deltaTime * speed);
        }

        //Move this ball to position in the path
        IEnumerator MoveToPosition(Vector3 position)
        {
            var startTime = Time.time;
            float runTime = 0.11f;
            float timePast = 0;
            var originalRotate = gameObject.transform.rotation;

            while (Time.time < startTime + runTime)
            {
                isMovingToPosition = true;
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                transform.rotation = Quaternion.Lerp(originalRotate, Quaternion.Euler(0,0,0), factor);
                transform.position = Vector3.Lerp(transform.position, position, factor);
                yield return null;
            }
            transform.position = position;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            isMovingToPosition = false;
            if (destroy)
            {
                if (SoundManager.Instance.score != null)
                    SoundManager.Instance.PlaySound(SoundManager.Instance.score);
                ScoreManager.Instance.AddScore(GameManager.Instance.playerController.scoreRange);
                GameManager.Instance.playerController.AddScore(gameObject.transform.position, GameManager.Instance.playerController.scoreRange);
                //***uncomment the cobe below to use pass level when enough score feature

                //GameManager.Instance.playerController.CheckScore();
                collisionObject.GetComponent<BallController>().isNodeBall = true;
                collisionObject.GetComponent<BallController>().DestroySameColorBall(false);
                if (collisionObject != gameObject)
                {
                    if (previousBall != null)
                        previousBall.GetComponent<BallController>().oldBall = null;
                    if (oldBall != null)
                        oldBall.GetComponent<BallController>().previousBall = null;
                    ResetBall();
                    gameObject.SetActive(false);
                }
            }
            else
            {
                if (hasConnection)
                {
                    if (isBehind)
                    {
                        MakeConnectBetweenBall(gameObject, connectObject, true, true);
                        if(connectObject!=null && connectObject.activeSelf)
                        connectObject.GetComponent<BallController>().FixBeforeMove(true,gameObject.transform.position,currentWayPointID);
                    }
                    else
                    {
                        MakeConnectBetweenBall(connectObject, gameObject, true, true);
                        if(gameObject.activeSelf)
                        FixBeforeMove(true,connectObject.transform.position,connectObject.GetComponent<BallController>().currentWayPointID);
                    }

                    isBehind = false;
                    connectObject = null;
                }else
                {
                    if(previousBall!=null)
                    previousBall.GetComponent<BallController>().FixBeforeMove(true, gameObject.transform.position, currentWayPointID);
                }
                if (oldBall != null && !oldBall.GetComponent<BallController>().isInterrupted)
                    isInterrupted = false;
                if (!hasConnection && oldColorBall != null && oldColorBall.tag == gameObject.tag && oldBall == null && oldColorBall.GetComponent<BallController>().previousBall == null && isInterrupted && previousColorBall != null && previousBall != null && Vector3.Distance(oldColorBall.transform.position, gameObject.transform.position) > 1f)
                {
                    isMoveBack = true;
                    MoveBack(true);
                    GameManager.Instance.playerController.waitMoveBackCount++;
                }
                else if (!hasConnection && previousColorBall != null && previousColorBall.tag == gameObject.tag && previousBall == null && previousColorBall.GetComponent<BallController>().oldBall == null && previousColorBall.GetComponent<BallController>().isInterrupted)
                {
                    previousColorBall.GetComponent<BallController>().isMoveBack = true;
                    previousColorBall.GetComponent<BallController>().MoveBack(true);
                    GameManager.Instance.playerController.waitMoveBackCount++;
                }
                else
                {
                    if(!hasConnection)
                        GameManager.Instance.playerController.onEndCollision();
                }
                if(hasConnection)
                    hasConnection = false;
            }
        }

        //Set information about previous color ball,old color ball for the ball ahead or behind
        public void SetPreviousColorBall(GameObject ball)
        {
            previousColorBall = ball;
            if (oldBall != null && oldBall.tag == gameObject.tag && ball!=null)
                oldBall.GetComponent<BallController>().SetPreviousColorBall(ball);
        }

        public void SetOldColorBall(GameObject ball)
        {
            oldColorBall = ball;
            if (previousBall != null && previousBall.tag == gameObject.tag && ball != null)
                previousBall.GetComponent<BallController>().SetOldColorBall(ball);
        }

        // Update is called once per frame
        void Update()
        {
            if (!isShootBall)
            {
                if (previousBall == null)
                    if (previousColorBall != null && (!previousColorBall.activeSelf || (previousColorBall.activeSelf && previousColorBall.GetComponent<BallController>().move)))
                        SetPreviousColorBall(null);
                   
                if (currentWayPointID >= pathFollow.pathPoints.Length-1 && !die)
                {
                    GameManager.Instance.playerController.Die();
                }
                if ((move && !isMovingBack && isLeader) || (die && isLeader))
                {
                        Move();
                }
                else
                if (isMovingBack && !move && isMoveBack && !die)
                {
                        MoveBackLeader();
                }
            }
        }

        //Move the leader ball and balls behind
        public void Move()
        {
            Vector3 previousPosition = Vector3.zero;
            GameObject previousCurrentBall=oldBall;
            GameObject currentBall = gameObject;
            Vector3 direction;
            if (oldBall != null)
            {
                previousPosition = oldBall.transform.position;
                previousCurrentBall = oldBall;
            }

            transform.position = Vector3.MoveTowards(transform.position
                , pathFollow.pathPoints[currentWayPointID], Time.deltaTime * speed);
            Vector3 currentPosition = transform.position;

            IncreaseWayPoint(gameObject, currentWayPointID);

            if (oldBall != null)
            {
                float distanceFix = 0;
                int wayPointId = currentWayPointID;
                while (previousCurrentBall != null)
                {
                    int tempID = wayPointId - 1;
                    if (tempID < 0)
                        tempID = 0;
                    distanceFix = Vector3.Distance(currentBall.transform.position, pathFollow.pathPoints[tempID]);

                    if (distanceFix >= scaleMagnitude)
                    {
                        previousCurrentBall.GetComponent<BallController>().currentWayPointID = wayPointId;
                    }
                    else
                    {
                        previousCurrentBall.GetComponent<BallController>().currentWayPointID = wayPointId - 1;
                    }

                    if (distanceFix >= scaleMagnitude * 0.5f && previousCurrentBall.GetComponent<BallController>().currentWayPointID == currentBall.GetComponent<BallController>().currentWayPointID)
                    {
                        direction = (pathFollow.pathPoints[wayPointId - 1] - currentPosition).normalized;
                        previousPosition = previousCurrentBall.transform.position;
                        previousCurrentBall.transform.position = currentBall.transform.TransformPoint(direction * scaleMagnitude);
                    }
                    else
                    {
                        int oldID = wayPointId - 2;
                        int newID = wayPointId - 1;
                        if (oldID < 0)
                            oldID = 0;
                        if (newID < 0)
                            newID = 0;
                        direction = (pathFollow.pathPoints[oldID] - pathFollow.pathPoints[newID]).normalized;
                        previousCurrentBall.transform.position = pathFollow.pathPoints[newID] + direction * (scaleMagnitude - distanceFix);
                    }

                    currentPosition = previousCurrentBall.transform.position;
                    currentBall = previousCurrentBall;
                    previousCurrentBall = currentBall.GetComponent<BallController>().oldBall;
                    wayPointId = currentBall.GetComponent<BallController>().currentWayPointID;
                }
            }
        }

        //Move the last ball behind backward
        public void MoveBackLeader()
        {
            Vector3 previousPosition = Vector3.zero;
            if (previousBall!=null)
            previousPosition = previousBall.transform.position;
            GameObject previousCurrentBall = previousBall;
            GameObject currentBall = gameObject;            
            Vector3 direction;

            transform.position = Vector3.MoveTowards(transform.position, pathFollow.pathPoints[currentWayPointID], Time.deltaTime * GameManager.Instance.moveBackSpeed);
            Vector3 currentPosition = transform.position;

            DecreaseWayPoint(gameObject, currentWayPointID);

            float distanceFix = 0;
            int wayPointId = currentWayPointID;

            //loop to move the balls behind the leader ball
            while (previousCurrentBall != null)
            {
                int tempID = wayPointId + 1;
                if (tempID >pathFollow.pathPoints.Length-1)
                    tempID = pathFollow.pathPoints.Length - 1;
                distanceFix = Vector3.Distance(currentBall.transform.position, pathFollow.pathPoints[tempID]);

                if (distanceFix >= scaleMagnitude)
                {
                    previousCurrentBall.GetComponent<BallController>().currentWayPointID = wayPointId;
                }
                else
                {
                    previousCurrentBall.GetComponent<BallController>().currentWayPointID = wayPointId +1;
                }

                if (distanceFix >= scaleMagnitude * 0.5f && previousCurrentBall.GetComponent<BallController>().currentWayPointID == currentBall.GetComponent<BallController>().currentWayPointID)
                {
                    direction = (pathFollow.pathPoints[wayPointId + 1] - currentPosition).normalized;
                    previousPosition = previousCurrentBall.transform.position;
                    previousCurrentBall.transform.position = currentBall.transform.TransformPoint(direction * scaleMagnitude);
                }
                else
                {
                    int oldID = wayPointId + 2;
                    int newID = wayPointId + 1;
                    if (oldID < 0)
                        oldID = 0;
                    if (newID < 0)
                        newID = 0;
                    direction = (pathFollow.pathPoints[oldID] - pathFollow.pathPoints[newID]).normalized;
                    previousCurrentBall.transform.position = pathFollow.pathPoints[newID] + direction * (scaleMagnitude - distanceFix);
                }
                currentPosition = previousCurrentBall.transform.position;
                
                currentBall = previousCurrentBall;
                previousCurrentBall = currentBall.GetComponent<BallController>().previousBall;

                wayPointId = currentBall.GetComponent<BallController>().currentWayPointID;
            }
        }

        public void FixedUpdate()
        {
            if (!isShootBall)
            {
                if(die && transform.position.z>GameManager.Instance.playerController.plane.transform.position.z)
                {
                    isLeader = true;
                    if ((oldBall == null || !oldBall.activeSelf) && (previousBall == null || !previousBall.activeSelf))
                    {
                        ResetBall();
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        //Count the balls of the same color ahead
        public void CountSameColorRangeForward(int startCount)
        {
            if (previousBall != null && previousBall.tag == gameObject.tag)
                previousBall.GetComponent<BallController>().CountSameColorRangeForward(startCount + 1);
            else
            {
                SetOrderOfBallBackward(startCount, startCount - 1);
                SetRangeColorBallBackward(startCount);
            }
        }

        //Count the balls of the same color back ward
        public void CountSameColorRangeBackWard(int startCount)
        {
            if (oldBall != null && oldBall.tag == gameObject.tag)
                oldBall.GetComponent<BallController>().CountSameColorRangeBackWard(startCount + 1);
            else
            {
                SetOrderOfBallForward(startCount);
                SetRangeColorBallForward(startCount);
            }
        }

        //Set information about the ball is interrupted or not
        public void SetInterrupted(bool interrupted,bool isSetMove)
        {
            isInterrupted = interrupted;
            if (isSetMove && !isInterrupted)
                move = true;
            if (previousBall != null)
            {
                previousBall.GetComponent<BallController>().SetInterrupted(interrupted,isSetMove);
            }else
            {
                if (isSetMove)
                    isLeader = true;
            }
        }

        //Set the range for the ahead same color balls
        public void SetRangeColorBallForward(int range)
        {
            sameColorRange = range;
            if (previousBall != null)
            {

                if (previousBall.tag == gameObject.tag)
                {
                    previousBall.GetComponent<BallController>().SetRangeColorBallForward(range);
                }
            }
        }

        //Set the order for the same color balls ahead
        public void SetOrderOfBallForward(int orderRange)
        {
            orderOfBall = orderRange;
            if (previousBall != null)
            {

                if (previousBall.tag == gameObject.tag)
                {
                    previousBall.GetComponent<BallController>().SetOrderOfBallForward(orderRange - 1);
                }
            }

        }

        //Set the order for the same color balls behind
        public void SetOrderOfBallBackward(int order, int orderCount)
        {
            orderOfBall = order - orderCount;
            if (oldBall != null)
            {

                if (oldBall.tag == gameObject.tag)
                {
                    oldBall.GetComponent<BallController>().SetOrderOfBallBackward(order, orderCount - 1);
                }
            }

        }

        public void SetRangeColorBallBackward(int range)
        {
            sameColorRange = range;
            if (oldBall != null)
            {

                if (oldBall.tag == gameObject.tag)
                {
                    oldBall.GetComponent<BallController>().SetRangeColorBallBackward(range);
                }
            }
        }

        //Start destroy same color balls ahead or behind
        public void DestroySameColorBall(bool callNextMoveBack)
        {
            Vector3 rotation;
            if (currentWayPointID < pathFollow.pathPoints.Length - 1)
            {
                rotation = pathFollow.pathPoints[currentWayPointID] - pathFollow.pathPoints[currentWayPointID - 1];
                CreateScoreEffect(gameObject.transform.position, Quaternion.LookRotation(rotation), sameColorRange);
            }
            DestroyForward();
            DestroyBackward(callNextMoveBack);
            if (SoundManager.Instance.destroyBallSound != null)
                SoundManager.Instance.PlaySound(SoundManager.Instance.destroyBallSound);
            if (previousBall != null)
                previousBall.GetComponent<BallController>().oldBall = null;
            if (oldBall != null)
                oldBall.GetComponent<BallController>().previousBall = null;
            ResetBall();
            gameObject.SetActive(false);

        }

        //Destroy the balls ahead
        void DestroyForward()
        {
            BallController previousComponent = null;
            if(previousBall!=null)
                previousComponent = previousBall.GetComponent<BallController>();

            if (previousBall != null && previousBall.tag != gameObject.tag)
            {
                previousComponent.SetInterrupted(true,false);
            }else
            if(previousBall != null)
            {
                previousComponent.SetOldColorBall( oldColorBall);
            }
            if (previousBall != null && previousBall.tag == gameObject.tag)
            {
                previousComponent.DestroyForward();
            }

            if (!isNodeBall)
            {
                if (previousBall != null)
                    previousComponent.oldBall = null;
                if (oldBall != null)
                    oldBall.GetComponent<BallController>().previousBall = null;
                ResetBall();
                gameObject.SetActive(false);
            }
        }

        //Destroy the balls behind
        void DestroyBackward(bool callNextMoveBack)
        {
            BallController previousColorComponent=null;
            if (previousColorBall!=null)
                previousColorComponent = previousColorBall.GetComponent<BallController>();

            BallController oldBallComponent=null;
            if(oldBall!=null)
                oldBallComponent = oldBall.GetComponent<BallController>();

            bool isBack = false;
            //Check if this is the last ball to destroy then check if this old ball and the previous ball is the same color to pull it backward
            if ((oldBall == null || (oldBall != null && oldBall.tag != gameObject.tag))&& oldColorBall != null 
                && previousColorBall != null && previousColorBall != null 
                && previousColorBall.tag == oldColorBall.tag && previousColorBall.activeSelf
                && previousColorComponent.currentWayPointID >= oldColorBall.GetComponent<BallController>().currentWayPointID)
            {
                previousColorComponent.move = false;
                previousColorComponent.isMoveBack = true;
                previousColorComponent.MoveBack(true);
                isBack = true;
                GameManager.Instance.playerController.waitMoveBackCount++;
            }
            else
            //or the old ball and previous ball not same color then make connect between them
            if ((oldBall == null || (oldBall != null && oldBall.tag != gameObject.tag)) && previousColorBall != null && oldColorBall!=null && previousColorBall.tag != oldColorBall.tag)
            {
                previousColorComponent.SetOldColorBall(oldColorBall);
                oldColorBall.GetComponent<BallController>().SetPreviousColorBall(previousColorBall);
            }
            //or the previous ball is not exist destroy the connect between old ball and this ball
            else if ((oldBall != null && oldBall.tag != gameObject.tag) && previousColorBall == null)
                oldBallComponent.SetPreviousColorBall(null);
            
            //if still in the same color chain continue destroy backward  
            if (oldBall != null && oldBall.tag == gameObject.tag)
            {
                oldBallComponent.DestroyBackward(callNextMoveBack);
            }
            else
             if (!isBack)
            {
                if (callNextMoveBack)
                {
                    GameManager.Instance.playerController.EndMoveBack();
                }else
                GameManager.Instance.playerController.onEndCollision();
            }

            if (!isNodeBall)
            {
                ResetBall();
                gameObject.SetActive(false);
            }
        }

        //Start move back when raise event
        void BeginMoveBack()
        {
            isMovingBack = true;
        }

        //Pull back if there is same color ball
        public void MoveBack(bool isMove)
        {

            moveBack = isMove;
            if (isMove)
            {
                PlayerController.StartMoveback += BeginMoveBack;
                PlayerController.FinishMoveBack += StopMoveBack;
                currentWayPointID--;
                if (currentWayPointID <= 0)
                    currentWayPointID = 0;
                StopAllCoroutines();
                if (isMovingForward)
                {
                    isMovingForward = false;
                    isMoveToward = false;
                    //StartCoroutine(WaitMakeConnect());
                }
                if (!gameObject.activeSelf)
                    GameManager.Instance.playerController.onEndCollision();
            }
            //check if this is the last ball,if not coutinue to the next ball
            if (previousBall != null)
                previousBall.GetComponent<BallController>().MoveBack(isMove);
            else
            //if this is the last ball raise event,balls subscribe event will move back 
                GameManager.Instance.playerController.BeginMoveBack();

        }

        //Move forward to create a spacing for new ball add to path
        public void MoveFoward(float lenght)
        {
            isMoveToward = true;
            GameObject previous = null;
            previous = previousBall;
            while (previous != null)
            {
                previous.GetComponent<BallController>().isMoveToward=true;
                previous=previous.GetComponent<BallController>().previousBall;
            }
                GameManager.Instance.playerController.StartMoveForward();
        }

        //Stop moving ball when player die
        void PlayerDie()
        {
            StopAllCoroutines();
            die = true;
            speed = GameManager.Instance.speedUp;
            move = true;
            isMoveBack = false;
            isMovingBack = false;
            if (previousBall == null || !previousBall.activeSelf)
                isLeader = true;
            if(isShootBall)
            {
                ResetBall();
                gameObject.SetActive(false);
            }
        }

        //On trigger enter stop all ball
        void HandleEnterTrigger()
        {
            if (die)
                move = true;
            else
                move = false;
        }

        //Make ball move again if not interrupted
        void HandleEndTrigger()
        {
            if (die)
                move = true;
            else
            {
                if (isInterrupted && oldBall != null && !oldBall.GetComponent<BallController>().isInterrupted)
                    SetInterrupted(false, true);
                if (!isInterrupted || (hasMakeConnection && isInterrupted && oldBall != null && oldBall.activeSelf
                    && !oldBall.GetComponent<BallController>().isInterrupted))
                {
                    if (previousBall == null || !previousBall.activeSelf)
                        isLeader = true;
                    else
                        isLeader = false;
                    move = true;
                    if (hasMakeConnection && isInterrupted && !oldBall.GetComponent<BallController>().isInterrupted)
                    {
                        SetBallMove();
                    }
                }
                else
                if ((hasMakeConnection && isInterrupted && oldBall != null && oldBall.GetComponent<BallController>().isInterrupted) || (isInterrupted && oldBall == null) || (oldBall != null && isInterrupted && oldBall.GetComponent<BallController>().isInterrupted))
                {
                    move = false;
                    if (isLeader)
                        isLeader = false;
                }
            }
        }

        public void SetBallMove()
        {
            move = true;
            if (previousBall != null)
                previousBall.GetComponent<BallController>().SetBallMove();
        }

        //Check if reach distance ahead then increase currentWayPointId by one
        void IncreaseWayPoint( GameObject objectToMove,int wayPointID)
        {
            float distance = Vector3.Distance(pathFollow.pathPoints[wayPointID], objectToMove.transform.position);
            if (distance <= reachDistance)
            {
                wayPointID++;
                if (wayPointID > (pathFollow.pathPoints.Length - 1))
                {
                    errorCount++;
                    wayPointID = pathFollow.pathPoints.Length - 1;
                    if (errorCount >= 4)
                        ResetBall();
                    gameObject.SetActive(false);
                }
            }
            if (gameObject == objectToMove)
                currentWayPointID = wayPointID;
        }

        //Check if reach distance behindthen decrease currentWayPointId by one
        void DecreaseWayPoint(GameObject objectToMove, int wayPointID)
        {
            float distance = Vector3.Distance(pathFollow.pathPoints[wayPointID], objectToMove.transform.position);
            if (distance <= reachDistance)
            {
                wayPointID--;
                if (wayPointID <0)
                {
                    wayPointID = 0;
                }
            }
            if (gameObject == objectToMove)
                currentWayPointID = wayPointID;
        }

        //Move ball forward with a distance  equal a ball size
        IEnumerator MoveToward(float lenght)
        {
            var startTime = Time.time;
            float runTime = 0.09f * lenght;
            float timePast = 0;
            int waypoint = 0;
            bool isFix = false;
            Vector3 targetposition=Vector3.zero;
            if (previousBall == null)
                targetposition = CalculateDistanceAhead(gameObject.transform.position, currentWayPointID, out waypoint);
            else
            {
                targetposition = previousBall.transform.position;
                waypoint = previousBall.GetComponent<BallController>().currentWayPointID;
            }
            while (Time.time < startTime + runTime)
            {
                isMovingForward = true;
                timePast += Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, pathFollow.pathPoints[currentWayPointID], 2 * 5 * Time.deltaTime);
                IncreaseWayPoint(gameObject, currentWayPointID);
                yield return null;
            }
            currentWayPointID = waypoint;
            transform.position = targetposition;
            if (hasConnection && !isFix)
            {
                isFix = true;
                BallController previousBallComponent = previousBall.GetComponent<BallController>();
                hasConnection = false;
                previousBallComponent.FixBeforeMove(true, targetposition, currentWayPointID);
                previousBallComponent.hasConnection = false;
                previousBallComponent.hasMakeConnection = false;
            }
            isMovingForward = false;
            isMoveToward = false;
        }
    }
}
