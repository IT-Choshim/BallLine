using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BallLine
{
    public class LevelScroller : MonoBehaviour
    {
        public static LevelScroller Instance { get; private set; }

        [Header("Scroller Config")]
        public GameObject LevelScrollerCamera = null;
        public float minScale = 1f;
        public float maxScale = 1.5f;
        public float levelSpace = 3f;
        public float moveForwardAmount = 2f;
        public float swipeThresholdX = 5f;
        public float swipeThresholdY = 30f;
        public float rotateSpeed = 30f;
        public float snapTime = 0.3f;
        public float resetRotateSpeed = 180f;
        public ScrollerStyle scrollerStyle = ScrollerStyle.Line;
        public float LevelScrollerRadius = 100f;
        [Range(0.1f, 1f)]
        public float scrollSpeedFactor = 0.25f;
        public Vector3 centerPoint;
        public Vector3 originalScale = Vector3.one;
        public Vector3 originalRotation = Vector3.zero;

        [Header("Object References")]
        public Text totalCoins;
        public Text priceText;
        public Image priceImg;
        public Button selectButon;
        public Button unlockButton;
        public Button lockButton;
        public Color lockColor = Color.black;

        List<GameObject> listLevel = new List<GameObject>();
        GameObject currentLevel;
        GameObject lastCurrentLevel;
        IEnumerator rotateCoroutine;
        Vector3 startPos;
        Vector3 endPos;
        float startTime;
        float endTime;
        bool isCurrentLevelRotating = false;
        bool hasMoved = false;

        public enum ScrollerStyle
        {
            Line,
            Circle
        }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;
        }

        private void OnEnable()
        {
            if (currentLevel)
            {
                StartRotateCurrentLevel();
            }
        }

        float levelAngleSpace = 1f;
        float currentAngle = 0f;
        // Use this for initialization
        void Start()
        {
            //PlayerPrefs.DeleteAll();
            lockColor.a = 0;    // need this for later setting material colors to work

            int currentLevelIndex = LevelManager.Instance.CurrentLevelIndex;
            currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, LevelManager.Instance.levelTests.Length - 1);
            centerPoint = transform.TransformPoint(centerPoint);

            switch (scrollerStyle)
            {
                case ScrollerStyle.Line:
                    LevelScrollerCamera.GetComponent<Camera>().orthographic = true;
                    break;
                case ScrollerStyle.Circle:
                    LevelScrollerCamera.GetComponent<Camera>().orthographic = false;
                    break;
                default:
                    break;
            }

            levelAngleSpace = Mathf.PI * 2 / LevelManager.Instance.levelTests.Length;
            currentAngle = currentLevelIndex * levelAngleSpace;
            for (int i = 0; i < LevelManager.Instance.levelTests.Length; i++)
            {
                int deltaIndex = i - currentLevelIndex;

                GameObject Level = (GameObject)Instantiate(LevelManager.Instance.levelTests[i].level, centerPoint, Quaternion.Euler(0, originalRotation.y, originalRotation.z));
                if (Level.transform.childCount > 0)
                {
                    for (int j = 0; j < Level.transform.childCount; j++)
                    {
                        if (Level.transform.GetChild(j).name == "Plane")
                        {
                            Level.transform.GetChild(j).GetComponent<SpriteRenderer>().sprite = BackGroundManager.Instance.backGrounds[1].GetComponent<Image>().sprite;
                            Level.transform.GetChild(j).GetComponent<SpriteRenderer>().material = BackGroundManager.Instance.backGrounds[1].GetComponent<Image>().material;
                        }
                    }
                }
                Vector3 scale = Level.transform.localScale;
                scale.Set(0.5f, 0.5f, 0.5f);
                Level.transform.localScale = scale;
                Level charData = Level.GetComponent<Level>();
                charData.levelSequenceNumber = i;
                listLevel.Add(Level);
                Level.transform.localScale = originalScale;
                //Level.transform.position = centerPoint + new Vector3(deltaIndex * LevelSpace, 0, 0);

                // Set color based on locking status
                //Renderer charRdr = Level.GetComponentInChildren<Renderer>();

                //if (charData.IsUnlocked)
                //    charRdr.material.SetColor("_Color", Color.white);
                //else
                //    charRdr.material.SetColor("_Color", lockColor);

                // Set as child of this object
                Level.transform.parent = transform;
                switch (scrollerStyle)
                {
                    case ScrollerStyle.Line:
                        Level.transform.localPosition += new Vector3(deltaIndex * levelSpace, 0, 0);
                        break;
                    case ScrollerStyle.Circle:
                        Level.transform.localPosition = transform.InverseTransformPoint(centerPoint) + new Vector3(Mathf.Sin(-currentAngle + i * levelAngleSpace), 0, -Mathf.Cos(-currentAngle + i * levelAngleSpace)) * LevelScrollerRadius;
                        break;
                    default:
                        break;
                }
                // Set layer for camera culling
                Level.gameObject.layer = LayerMask.NameToLayer("LevelSelectionUI");
                GameObject child = Level.gameObject.transform.GetChild(0).gameObject;
                if(child.transform.childCount>0)
                    child.transform.GetChild(0).gameObject.layer= LayerMask.NameToLayer("LevelSelectionUI"); 
                if (Level.gameObject.transform.childCount > 0)
                {
                    for (int j = 0; j < Level.gameObject.transform.childCount; j++)
                        Level.gameObject.transform.GetChild(j).gameObject.layer = LayerMask.NameToLayer("LevelSelectionUI");
                }
            }

            // Highlight current Level
            currentLevel = listLevel[currentLevelIndex];
            switch (scrollerStyle)
            {
                case ScrollerStyle.Line:
                    currentLevel.transform.localScale = maxScale * originalScale;
                    currentLevel.transform.localPosition += moveForwardAmount * Vector3.forward;
                    break;
                case ScrollerStyle.Circle:
                    currentLevel.transform.localScale = maxScale * originalScale;
                    break;
                default:
                    break;
            }

            lastCurrentLevel = null;
            StartRotateCurrentLevel();
        }

        // Update is called once per frame
        void Update()
        {
            #region Scrolling
            // Do the scrolling stuff
            if (Input.GetMouseButtonDown(0))    // first touch
            {
                startPos = Input.mousePosition;
                startTime = Time.time;
                hasMoved = false;
            }
            else if (Input.GetMouseButton(0))   // touch stays
            {
                endPos = Input.mousePosition;
                endTime = Time.time;

                float deltaX = Mathf.Abs(startPos.x - endPos.x);
                //float deltaY = Mathf.Abs(startPos.y - endPos.y);

                if (deltaX >= swipeThresholdX)
                {
                    hasMoved = true;
                    if (isCurrentLevelRotating)
                        StopRotateCurrentLevel(true);

                    float speed = deltaX / (endTime - startTime);
                    Vector3 dir = (startPos.x - endPos.x < 0) ? Vector3.right : Vector3.left;
                    Vector3 moveVector = dir * (speed / 10) * scrollSpeedFactor * Time.fixedDeltaTime;
                    currentAngle -= moveVector.x * levelAngleSpace / 5;
                    if (currentAngle > Mathf.PI * 2)
                    {
                        currentAngle -= Mathf.PI * 2;
                    }
                    else if (currentAngle < 0)
                    {
                        currentAngle += Mathf.PI * 2;
                    }
                    // Move and scale the children
                    for (int i = 0; i < listLevel.Count; i++)
                    {
                        switch (scrollerStyle)
                        {
                            case ScrollerStyle.Line:
                                MoveAndScale(listLevel[i].transform, moveVector);
                                break;
                            case ScrollerStyle.Circle:
                                MoveAndScaleCircleVer(listLevel[i].transform, moveVector, i);
                                break;
                            default:
                                break;
                        }

                    }

                    // Update for next step
                    startPos = endPos;
                    startTime = endTime;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (hasMoved)
                {
                    // Store the last currentLevel
                    lastCurrentLevel = currentLevel;

                    // Update current Level to the one nearest to center point
                    switch (scrollerStyle)
                    {
                        case ScrollerStyle.Line:
                            currentLevel = FindLevelNearestToCenter();
                            // Snap
                            float snapDistance = transform.InverseTransformPoint(centerPoint).x - currentLevel.transform.localPosition.x;
                            StartCoroutine(SnapAndRotate(snapDistance));
                            break;
                        case ScrollerStyle.Circle:
                            currentLevel = FindLevelNearestToCenterCircleVer();
                            StartCoroutine(SnapAndRotateCircleVer());
                            break;
                        default:
                            break;
                    }
                }
            }

            #endregion

            // Update UI
            totalCoins.text = CoinManager.Instance.Coins.ToString();
            Level charData = currentLevel.GetComponent<Level>();

            if (!charData.isFree )//&& LevelManager.Instance.CurrentLevelIndex<charData.levelSequenceNumber)
            {
                priceText.gameObject.SetActive(true);
                priceText.text = charData.price.ToString();
            }
            else
            {
                //charData.isFree = true;
                priceText.gameObject.SetActive(false);
            }

            if (currentLevel != lastCurrentLevel)
            {
                if (charData.IsUnlocked)
                {
                    unlockButton.gameObject.SetActive(false);
                    lockButton.gameObject.SetActive(false);
                    selectButon.gameObject.SetActive(true);
                }
                else
                {
                    selectButon.gameObject.SetActive(false);
                    if (CoinManager.Instance.Coins >= charData.price)
                    {
                        unlockButton.gameObject.SetActive(true);
                        lockButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        unlockButton.gameObject.SetActive(false);
                        lockButton.gameObject.SetActive(true);
                    }
                }
            }
        }

        public void ShowLevelFromCamera()
        {
            Camera selectionCamera = LevelScrollerCamera.GetComponent<Camera>();
            selectionCamera.cullingMask = 1024;
        }

        void MoveAndScale(Transform tf, Vector3 moveVector)
        {
            // Move
            tf.localPosition += moveVector;

            // Scale and move forward according to distance from current position to center position
            float d = Mathf.Abs(tf.localPosition.x - transform.InverseTransformPoint(centerPoint).x);
            if (d < (levelSpace / 2))
            {
                float factor = 1 - d / (levelSpace / 2);
                float scaleFactor = Mathf.Lerp(minScale, maxScale, factor);
                tf.localScale = scaleFactor * originalScale;

                float fd = Mathf.Lerp(0, moveForwardAmount, factor);
                Vector3 pos = tf.localPosition;
                pos.z = transform.InverseTransformPoint(centerPoint).z + fd;
                tf.localPosition = pos;
            }
            else
            {
                tf.localScale = minScale * originalScale;
                Vector3 pos = tf.localPosition;
                pos.z = transform.InverseTransformPoint(centerPoint).z;
                tf.localPosition = pos;
            }
        }

        void MoveAndScaleCircleVer(Transform tf, Vector3 moveVector, int index)
        {
            // Move
            tf.localPosition = transform.InverseTransformPoint(centerPoint) + new Vector3(Mathf.Sin(-currentAngle + index * levelAngleSpace), 0, -Mathf.Cos(-currentAngle + index * levelAngleSpace)) * LevelScrollerRadius;
            //Scale and move forward according to distance from current position to center position
            float d = Mathf.Abs(Vector3.Angle(Vector3.back, (tf.localPosition - transform.InverseTransformPoint(centerPoint)).normalized) * Mathf.Deg2Rad);
            if (d < (levelAngleSpace / 2))
            {
                float factor = 1 - d / (levelAngleSpace / 2);
                float scaleFactor = Mathf.Lerp(minScale, maxScale, factor);
                tf.localScale = scaleFactor * originalScale;
            }
            else
            {
                tf.localScale = minScale * originalScale;
            }
        }

        GameObject FindLevelNearestToCenter()
        {
            float min = -1;
            GameObject nearestObj = null;

            for (int i = 0; i < listLevel.Count; i++)
            {
                float d = Mathf.Abs((listLevel[i].transform.position - centerPoint).magnitude);
                if (d < min || min < 0)
                {
                    min = d;
                    nearestObj = listLevel[i];
                }
            }

            return nearestObj;
        }

        GameObject FindLevelNearestToCenterCircleVer()
        {
            GameObject nearestObj = null;

            int neareastObjIndex = Mathf.RoundToInt(currentAngle / levelAngleSpace);
            if (neareastObjIndex < 0)
            {
                neareastObjIndex = listLevel.Count - 1;
            }
            else if (neareastObjIndex > listLevel.Count - 1)
            {
                neareastObjIndex = 0;
            }
            nearestObj = listLevel[neareastObjIndex];
            return nearestObj;
        }


        IEnumerator SnapAndRotate(float snapDistance)
        {
            float snapDistanceAbs = Mathf.Abs(snapDistance);
            float snapSpeed = snapDistanceAbs / snapTime;
            float sign = snapDistance / snapDistanceAbs;
            float movedDistance = 0;

            SoundManager.Instance.PlaySound(SoundManager.Instance.tick);

            while (Mathf.Abs(movedDistance) < snapDistanceAbs)
            {
                float d = sign * snapSpeed * Time.fixedDeltaTime;
                float remainedDistance = Mathf.Abs(snapDistanceAbs - Mathf.Abs(movedDistance));
                d = Mathf.Clamp(d, -remainedDistance, remainedDistance);

                Vector3 moveVector = new Vector3(d, 0, 0);
                for (int i = 0; i < listLevel.Count; i++)
                {
                    MoveAndScale(listLevel[i].transform, moveVector);
                }

                movedDistance += d;
                yield return null;
            }

            if (currentLevel != lastCurrentLevel || !isCurrentLevelRotating)
            {
                // Stop rotating the last current Level
                StopRotateCurrentLevel();

                // Now rotate the new current Level
                StartRotateCurrentLevel();
            }
        }

        IEnumerator SnapAndRotateCircleVer()
        {
            float nextAngle = Mathf.RoundToInt(currentAngle / levelAngleSpace) * levelAngleSpace;
            SoundManager.Instance.PlaySound(SoundManager.Instance.tick);
            while (Mathf.Abs(currentAngle - nextAngle) > 0.01f)
            {
                Vector3 moveVector = new Vector3((nextAngle - currentAngle) / snapTime * 10 * Time.fixedDeltaTime, 0, 0);
                currentAngle += moveVector.x * levelAngleSpace;
                if (currentAngle > Mathf.PI * 2)
                {
                    currentAngle -= Mathf.PI * 2;
                }
                else if (currentAngle < 0)
                {
                    currentAngle += Mathf.PI * 2;
                }
                for (int i = 0; i < listLevel.Count; i++)
                {
                    MoveAndScaleCircleVer(listLevel[i].transform, moveVector, i);
                }
                yield return null;
            }
            if (currentLevel != lastCurrentLevel || !isCurrentLevelRotating)
            {
                // Stop rotating the last current Level
                StopRotateCurrentLevel();

                // Now rotate the new current Level
                StartRotateCurrentLevel();
            }
        }

        void StartRotateCurrentLevel()
        {
            StopRotateCurrentLevel(false);   // stop previous rotation if any
            rotateCoroutine = CRRotateLevel(currentLevel.transform);
            StartCoroutine(rotateCoroutine);
            isCurrentLevelRotating = true;
        }

        void StopRotateCurrentLevel(bool resetRotation = false)
        {
            if (rotateCoroutine != null)
            {
                StopCoroutine(rotateCoroutine);
            }

            isCurrentLevelRotating = false;

            if (resetRotation)
                StartCoroutine(CRResetLevelRotation(currentLevel.transform));
        }

        IEnumerator CRRotateLevel(Transform charTf)
        {
            while (true)
            {
                charTf.Rotate(new Vector3(0, rotateSpeed * Time.fixedDeltaTime, 0));
                yield return null;
            }
        }

        IEnumerator CRResetLevelRotation(Transform charTf)
        {
            Vector3 startRotation = charTf.rotation.eulerAngles;
            Vector3 endRotation = originalRotation;
            float timePast = 0;
            float rotateAngle = Mathf.Abs(endRotation.y - startRotation.y);
            float rotateTime = rotateAngle / resetRotateSpeed;

            while (timePast < rotateTime)
            {
                timePast += Time.fixedDeltaTime;
                Vector3 rotation = Vector3.Lerp(startRotation, endRotation, timePast / rotateTime);
                charTf.rotation = Quaternion.Euler(rotation);
                yield return null;
            }
        }

        public void UnlockLevel()
        {
            bool unlockSucceeded = currentLevel.GetComponent<Level>().Unlock();
            if (unlockSucceeded)
            {
                //currentLevel.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                unlockButton.gameObject.SetActive(false);
                selectButon.gameObject.SetActive(true);

                SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
            }
        }

        public void SelectLevel()
        {
            if (currentLevel.GetComponent<Level>().levelSequenceNumber != LevelManager.Instance.CurrentLevelIndex)
            {
                int levelID = currentLevel.GetComponent<Level>().levelSequenceNumber;
                GameManager.Instance.playerController.instantiateNewLevel(levelID);
            }
            LevelManager.Instance.CurrentLevelIndex = currentLevel.GetComponent<Level>().levelSequenceNumber;
        }
    }
}