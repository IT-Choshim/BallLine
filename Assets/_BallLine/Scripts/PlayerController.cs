using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BallLine
{
    public class PlayerController : MonoBehaviour
    {
        public static event System.Action PlayerDied;
        public static event System.Action Collision;
        public static event System.Action EndCollision;
        public static event System.Action StartMoveback;
        public static event System.Action BeginNextMoveBack;
        public static event System.Action FinishMoveBack;
        public static event System.Action MoveWhenDie;
        public static event System.Action SpeedUp;
        public static event System.Action FinishSpeedUp;
        public static event System.Action StartMoveToward;

        public GameObject boom;
        public GameObject trail;
        public GameObject shootBall = null;
        public GameObject hitPoint;
        public GameObject plane = null;
        public GameObject level = null;
        public GameObject path;
        public GameObject clone;
        public GameObject addScore;
        //***uncomment to use pass level when enough score feature

        //public Image scoreBar;
        //float currentScore = 0;

        LayerMask layer = -1;

        public Transform planePosition;

        public Material lineRenderMaterial;

        public bool shoot;
        public bool isDie;
        public bool isPlay;
        public bool isSpeedUp = false;
        public bool isBengin = false;
        public bool nextLevel = false;
        public bool hasEarnCoin = false;

        public ParticleSystem scoreEffect;
        public ParticleSystem explosionEffect;

        public int comboScore;
        public int waitMoveBackCount;
        public int newRange;
        public int scoreRange;

        public float levelScore;
        public float createBallSpeed;

        private LineRenderer line;

        GameObject previousColorBall = null;
        GameObject currentBall = null;
        GameObject scoreCanvas = null;

        Transform shootPosition;
        Transform spawnPosition;

        Vector3 centerPosition;

        float speed;
        float coroutineTime;

        int comboBallCount = 1;
        int oldBallNumber = 100;
        int i = 0;

        bool hasStop;
        bool isFirstBall;
        bool isFirstShootBall = true;
        bool isCreateBall;
        bool canShoot = true;
        bool isScaleUp = false;

        void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        void Start()
        {
            // Setup
            instantiateNewLevel(LevelManager.Instance.CurrentLevelIndex);
            SetBackGround(BackGroundManager.Instance.CurrentBackGroundIndex);
            line = GetComponent<LineRenderer>();
            line.SetPosition(0, shootPosition.position);
        }

        public void SetBackGround(int backGroundID)
        {
            plane.GetComponent<SpriteRenderer>().material = BackGroundManager.Instance.backGrounds[backGroundID].GetComponent<Image>().material;
            plane.GetComponent<SpriteRenderer>().sprite = BackGroundManager.Instance.backGrounds[backGroundID].GetComponent<Image>().sprite;
        }

        public void instantiateNewLevel(int levelID)
        {
            if (level != null)
                Destroy(level);
            level = (GameObject)Instantiate(LevelManager.Instance.levelTests[levelID].level, planePosition.position, Quaternion.Euler(0, 0, 0));
            //***uncomment the cobe below to use pass level when enough score feature

            //Level levelComponent = level.GetComponent<Level>();
            //levelScore = levelComponent.scoreToPass;

            for (int i = 0; i < level.transform.childCount; i++)
            {
                if (level.transform.GetChild(i).gameObject.name == "Plane")
                    plane = level.transform.GetChild(i).gameObject;
            }
            centerPosition = new Vector3(plane.transform.position.x, plane.transform.position.y, plane.transform.position.z - GameManager.Instance.distanceFromCamera);
            SetupNewLevel(level);
            SetBackGround(BackGroundManager.Instance.CurrentBackGroundIndex);
        }

        //When new level has load setup spawn position,shoot position,path to move follow
        public void SetupNewLevel(GameObject levelMap)
        {
            shootPosition = levelMap.transform.GetChild(0);

            for (int i = 0; i < levelMap.transform.childCount; i++)
            {
                GameObject gameObject = levelMap.transform.GetChild(i).gameObject;
                if (gameObject.tag.Equals("Path"))
                {
                    path = gameObject;
                    break;
                }
            }

            //Loop to find spawn ball position from childs of this path
            for (int i = 0; i < path.transform.childCount; i++)
            {
                GameObject point = path.transform.GetChild(i).gameObject;
                if (point.tag == "SpawnPosition")
                {
                    spawnPosition = point.transform;
                    break;
                }
            }
        }

        //Start new coroutine to create a new loop to create ball
        void StartSpawnBall()
        {
            StartCoroutine(SpawnBall());
        }

        // Update is called once per frame
        void Update()
        {
            if (plane != null && centerPosition != plane.transform.position)
                centerPosition = new Vector3(plane.transform.position.x, plane.transform.position.y, plane.transform.position.z - GameManager.Instance.distanceFromCamera);
            // Activities that take place every frame
            if (plane != null && Camera.main.transform.position != centerPosition)
                Camera.main.transform.position = centerPosition;
            if (isPlay && !isDie)
            {
                if ((shootBall != null) && Input.GetMouseButtonUp(0) && canShoot && !isScaleUp)
                {
                    ShootTheBall();
                }
                if ((shootBall != null) && !shoot && Input.GetMouseButton(0) && canShoot && !isScaleUp)
                {
                    AimToShoot();
                    //setTrajectoryPoints(shootBall.transform.position, Quaternion.LookRotation(Vector3.forward, targetdir), distance, true);
                }
            }
        }

        private void FixedUpdate()
        {
            if (shoot && shootBall != null)
            {
                shootBall.GetComponent<Rigidbody>().transform.position += shootBall.transform.up * speed * Time.fixedDeltaTime;
            }
        }

        //*if use feature pass level when enough score just uncomment this code block

        //public void CheckScore()
        //{
        //    currentScore = ScoreManager.Instance.Score;
        //    Debug.Log(currentScore+"  "+levelScore +" score " + currentScore / levelScore);
        //    ScoreBar.fillAmount = currentScore / levelScore;
        //    if (currentScore >= levelScore && !nextLevel)
        //    {
        //        if (LevelManager.Instance.CurrentLevelIndex < LevelManager.Instance.Levels.Length - 1)
        //        {
        //            nextLevel = true;
        //            LevelManager.Instance.CurrentLevelIndex += 1;
        //            GameManager.Instance.RestartGame();
        //        }
        //    }
        //}

        void AimToShoot()
        {
            line.enabled = true;
            Vector3 mousePositionVector3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            mousePositionVector3 = Camera.main.ScreenToWorldPoint(mousePositionVector3);
            Vector3 targetdir = shootBall.transform.position - mousePositionVector3;
            shootBall.transform.rotation = Quaternion.LookRotation(Vector3.forward, -targetdir);
            //Ray ray = new Ray(shootBall.transform.position + shootBall.transform.up * 0.75f, shootBall.transform.up);
            RaycastHit hit;
            if (Physics.SphereCast(shootBall.transform.position, 0.25f, shootBall.transform.up, out hit, 30, layer, QueryTriggerInteraction.UseGlobal))
            {
                if (hit.transform.gameObject.tag == "Plane")
                {
                    line.positionCount = 3;
                    DrawPredictedReflectionPattern(shootBall.transform.position + shootBall.transform.up * 0.75f, shootBall.transform.up, 2, false);
                }
                else
                {
                    line.positionCount = 2;
                    DrawPredictedReflectionPattern(shootBall.transform.position + shootBall.transform.up * 0.75f, shootBall.transform.up, 1, false);
                }
            }
        }

        void ShootTheBall()
        {
            hitPoint.SetActive(false);
            line.enabled = false;
            shoot = true;
            trail.SetActive(true);
        }
        //Draw a reflection line of  aim line
        private void DrawPredictedReflectionPattern(Vector3 position, Vector3 direction, int reflectionsRemaining, bool hasReflect)
        {
            if (reflectionsRemaining == 0)
            {
                i = 0;
                return;
            }

            Vector3 startingPosition = position;

            if (line.positionCount == 2 || hasReflect)
            {
                RaycastHit hit;
                if (Physics.SphereCast(position, 0.25f, direction, out hit, 30, layer, QueryTriggerInteraction.UseGlobal))
                {
                    if (hit.transform.gameObject.tag == "Plane")
                    {
                        //line.SetVertexCount(3);
                        direction = Vector3.Reflect(direction, hit.normal);
                    }
                    //else
                    //{
                    //    reflectionsRemaining--;
                    //    line.SetVertexCount(2);
                    //}
                    position = hit.point;
                }
                else
                {
                    position += direction * 50;
                }
            }
            else
            {
                hasReflect = true;
                Ray ray = new Ray(position, direction);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 50))
                {
                    if (hit.transform.gameObject.tag == "Plane")
                    {
                        //line.SetVertexCount(3);
                        direction = Vector3.Reflect(direction, hit.normal);
                    }
                    //else
                    //{
                    //    reflectionsRemaining--;
                    //    line.SetVertexCount(2);
                    //}
                    position = hit.point;
                }
                else
                {
                    position += direction * 50;
                }
            }
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawLine(startingPosition, position);
            line.SetPosition(i, new Vector3(startingPosition.x, startingPosition.y, startingPosition.z + 1));
            Vector3 newPos = position + (shootPosition.position - position).normalized * 0.8f;
            newPos.z = newPos.z + 1;
            line.SetPosition(i + 1, newPos);
            hitPoint.SetActive(true);
            hitPoint.transform.up = shootBall.transform.up;
            hitPoint.transform.position = position + (shootPosition.position - position).normalized * 0.5f;// - (hitPoint.transform.position + hit.transform.position).normalized * 5f;
            i++;
            DrawPredictedReflectionPattern(position, direction, reflectionsRemaining - 1, hasReflect);
        }

        int count = 0;

        IEnumerator SpawnBall()
        {
            var startTime = Time.time;
            float runTime = createBallSpeed;
            float timePast = 0;
            if (hasStop)
            {
                runTime = createBallSpeed - coroutineTime;
                hasStop = false;
            }

            while (Time.time < startTime + runTime)
            {
                coroutineTime = timePast + Time.deltaTime;
                timePast += Time.deltaTime;
                yield return null;
            }
            count++;
            CreateBall();
//            if(count<4)
            StartCoroutine(SpawnBall());
        }

        void CreateBall()
        {
            int ballNumber = Random.Range(0, 3);
            string ballTag = "RedBall";
            switch (ballNumber)
            {
                case 0:
                    ballTag = "RedBall";
                    break;
                case 1:
                    ballTag = "GreenBall";
                    break;
                case 2:
                    ballTag = "YellowBall";
                    break;
            }
            GameObject _ball = ObjectPooling.SharedInstance.GetPooledObjectByTag(ballTag);//(GameObject)Instantiate(ball[ballNumber], spawnPosition.position, Quaternion.Euler(0, 0, 0));
            BallController ballComponent = _ball.GetComponent<BallController>();
            _ball.transform.position = spawnPosition.position;
            _ball.transform.rotation = Quaternion.Euler(0, 0, 0);
            _ball.SetActive(true);
            ballComponent.Init();

            ballComponent.move = true;
            ballComponent.pathName = path;
            ballComponent.orderOfBall = 1;
            if (isSpeedUp && isBengin)
            {
                ballComponent.speed = GameManager.Instance.speedUp;// GameManager.Instance.moveBallSpeed;
            }
            else
            {
                ballComponent.speed = GameManager.Instance.moveBallSpeed;
            }
            if (oldBallNumber == ballNumber)
            {
                comboBallCount += 1;
                ballComponent.previousColorBall = previousColorBall;
            }
            else
            {
                previousColorBall = currentBall;
                if (comboBallCount > 1)
                {
                    currentBall.GetComponent<BallController>().SetRangeColorBallForward(comboBallCount);
                    currentBall.GetComponent<BallController>().SetOrderOfBallForward(comboBallCount);
                    comboBallCount = 1;
                }
                if (currentBall != null)
                    currentBall.GetComponent<BallController>().SetOldColorBall(_ball);
                ballComponent.previousColorBall = previousColorBall;
            }

            if (!isFirstBall)
            {
                currentBall.GetComponent<BallController>().oldBall = _ball;
            }
            else
            {
                ballComponent.isLeader = true;
                isFirstBall = false;
            }

            oldBallNumber = ballNumber;
            ballComponent.previousBall = currentBall;
            currentBall = _ball;
            //if (!isSpeedUp && lastSpeedUp)
            //{
            //    lastSpeedUp = false;
            //    ballComponent.FixSpacingForward();
            //}
        }


        public void CreateShootBall()
        {
            int ballNumber = Random.Range(0, 3);
            string ballTag = "RedBall";
            switch (ballNumber)
            {
                case 0:
                    ballTag = "RedBall";
                    //hitPoint.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                    break;
                case 1:
                    ballTag = "GreenBall";
                    //hitPoint.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                    break;
                case 2:
                    ballTag = "YellowBall";
                    //hitPoint.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
                    break;
            }
            shootBall = ObjectPooling.SharedInstance.GetPooledObjectByTag(ballTag);
            ;//(GameObject)Instantiate(ball[ballNumber], shootPosition.position, Quaternion.Euler(0, 0, 0));
            BallController shootBallComponent = shootBall.GetComponent<BallController>();
            shootBall.transform.position = shootPosition.position;
            hitPoint.GetComponent<SpriteRenderer>().sprite = shootBall.GetComponent<SpriteRenderer>().sprite;
            //shootBall.AddComponent<InterpolatedTransformUpdater>();
            //shootBall.AddComponent<InterpolatedTransform>();
            shootBall.transform.rotation = Quaternion.Euler(0, 0, 0);
            shootBall.SetActive(true);
            shootBallComponent.Init();
            //shootBall.GetComponent<Rigidbody>().isKinematic = false;
            shootBall.GetComponent<Collider>().isTrigger = false;
            shootBall.GetComponent<Rigidbody>().isKinematic = false;
            shootBallComponent.isShootBall = true;
            shootBallComponent.isInterrupted = true;
            shootBallComponent.sameColorRange = 1;
            shootBallComponent.orderOfBall = 1;
            trail.transform.SetParent(shootBall.transform);
            trail.transform.localPosition = Vector3.zero;
            if (!isFirstShootBall)
            {
                Vector3 scale = shootBall.transform.localScale;
                scale.Set(0.1f, 0.1f, 0.1f);
                shootBall.transform.localScale = scale;
                StartCoroutine(ScaleUpShootBall(shootBall));
            }
            else
            {
                isFirstShootBall = false;
            }
        }

        public void CreateBoomBall()
        {
            if (shootBall != null)
            {
                trail.transform.SetParent(null);
                shootBall.gameObject.SetActive(false);
                shootBall.GetComponent<BallController>().ResetBall();
                shootBall = null;
            }
            shootBall = (GameObject)Instantiate(boom, shootPosition.position, Quaternion.Euler(0, 0, 0));
            hitPoint.GetComponent<SpriteRenderer>().sprite = shootBall.GetComponent<SpriteRenderer>().sprite;
            shootBall.GetComponent<Collider>().isTrigger = false;
            shootBall.GetComponent<Rigidbody>().isKinematic = false;
            BallController shootBallComponent = shootBall.GetComponent<BallController>();
            shootBallComponent.isBoomBall = true;
            shootBallComponent.isShootBall = true;
            shootBallComponent.isInterrupted = true;
            shootBallComponent.sameColorRange = 1;
            shootBallComponent.orderOfBall = 1;
            trail.transform.SetParent(shootBall.transform);
            trail.transform.localPosition = Vector3.zero;
        }

        // Listens to changes in game state
        void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                Time.fixedDeltaTime = 1.0f / 120;
                isFirstBall = true;
                createBallSpeed = (0.05f / GameManager.Instance.speedUp);
                GameManager.Instance.GetComponent<GameManager>().SpeedUp();
                StartSpawnBall();
                speed = GameManager.Instance.shootBallSpeed;
                // Do whatever necessary when a new game starts
            }      
        }



        IEnumerator ScaleUpShootBall(GameObject ball)
        {
            var startTime = Time.time;
            float runTime = 0.1f;
            float timePast = 0;
            var originalScale = new Vector3(0.1f, 0.1f, 0.1f);
            Vector3 targetScale = new Vector3(1, 1, 1);
            Vector3 scaleUp = Vector3.zero;

            while (Time.time < startTime + runTime)
            {
                isScaleUp = true;
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                ball.transform.localScale = Vector3.Lerp(originalScale, targetScale, factor);
                //ball.transform.localScale = scale;
                yield return null;
            }
            ball.transform.localScale = targetScale;
            ball.GetComponent<BallController>().scaleMagnitude = ball.GetComponent<SpriteRenderer>().bounds.extents.x * transform.lossyScale.x * 2;
            isScaleUp = false;
        }

        public void collision()
        {
            if (!isDie)
            {
                canShoot = false;
                trail.transform.SetParent(null);
                trail.SetActive(false);
                hasStop = true;
                StopAllCoroutines();
                if (scoreCanvas != null)
                {
                    scoreCanvas.transform.SetParent(null);
                    Destroy(scoreCanvas);
                }
                // Fire event
                if (Collision != null)
                    Collision();
            }
        }

        public void AddScore(Vector3 position, int score)
        {
            GameObject addScoreCanvas = (GameObject)Instantiate(addScore, position, Quaternion.Euler(Camera.main.transform.eulerAngles.x, 0, 0));
            addScoreCanvas.transform.GetComponentInChildren<Text>().text = "+" + score.ToString();
            StartCoroutine(MoveAndFade(addScoreCanvas));

        }

        IEnumerator MoveAndFade(GameObject canvas)
        {
            var startTime = Time.time;
            float runTime = 1.5f;
            float timePast = 0;
            var oriPos = canvas.transform.localPosition;
            if (scoreCanvas != null)
                Destroy(scoreCanvas);
            scoreCanvas = canvas;
            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                if (canvas != null)
                {
                    canvas.transform.localPosition = oriPos + new Vector3(0, factor * 1.0f, 0);
                    canvas.GetComponent<CanvasGroup>().alpha = 1 - factor;
                }
                yield return null;
            }
            if (canvas != null)
            {
                canvas.transform.SetParent(null);
                Destroy(canvas);
            }

        }

        public void BeginMoveBack()
        {
            if (StartMoveback != null)
                StartMoveback();
        }

        public void EndTrigger()
        {
            if (!isDie)
            {
                StartCoroutine(RaiseEndCollisinEvent());
            }
        }

        IEnumerator RaiseEndCollisinEvent()
        {
            yield return new WaitForSeconds(0.01f);
            if (EndCollision != null)
                EndCollision();
        }

        public void onEndCollision()
        {
            if (!isDie)
            {
                canShoot = true;
                StartSpawnBall();
                // Fire event
                StartCoroutine(RaiseEndCollisinEvent());
            }
        }


        public void StopMoveBack()
        {
            if (FinishMoveBack != null)
            {
                FinishMoveBack();
            }
        }

        public void EndMoveBack()
        {
            if (BeginNextMoveBack != null)
            {
                BeginNextMoveBack();
            }
            else
                onEndCollision();
        }

        // Calls this when the player dies and game over
        public void Die()
        {
            if (!isDie)
            {
                isDie = true;
                hitPoint.SetActive(false);

                if (shootBall != null)
                {
                    shootBall.GetComponent<BallController>().ResetBall();
                    shootBall.SetActive(false);
                }

                line.enabled = false;
                isPlay = false;
                StopAllCoroutines();
                if (scoreCanvas != null)
                {
                    scoreCanvas.transform.SetParent(null);
                    Destroy(scoreCanvas);
                }
                if (MoveWhenDie != null)
                    MoveWhenDie();
                GameManager.Instance.GetComponent<GameManager>().SpeedUp();
                // Fire event

            }
        }

        public void StartMoveForward()
        {
            if (StartMoveToward != null)
                StartMoveToward();
        }

        public void StartSpeedUp()
        {
            if (SpeedUp != null)
                SpeedUp();
        }

        public void EndSpeedUp()
        {
            if (FinishSpeedUp != null)
                FinishSpeedUp();
        }

        public void RaiseEventDie()
        {
            if (PlayerDied != null)
            {
                PlayerDied();
            }
        }
    }
}