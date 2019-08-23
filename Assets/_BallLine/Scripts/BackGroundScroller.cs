using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BallLine
{
    public class BackGroundScroller : MonoBehaviour
    {
        public static BackGroundScroller Instance { get; private set; }

        [Header("Scroller Config")]
        public GameObject BackGroundScrollerCamera = null;
        public float minScale = 1f;
        public float maxScale = 1.5f;
        public float BackGroundSpace = 3f;
        public float moveForwardAmount = 2f;
        public float swipeThresholdX = 5f;
        public float swipeThresholdY = 30f;
        public float rotateSpeed = 30f;
        public float snapTime = 0.3f;
        public float resetRotateSpeed = 180f;
        public ScrollerStyle scrollerStyle = ScrollerStyle.Line;
        public float BackGroundScrollerRadius = 100f;
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

        List<GameObject> listbackGround = new List<GameObject>();
        GameObject currentbackGround;
        GameObject lastCurrentbackGround;
        IEnumerator rotateCoroutine;
        Vector3 startPos;
        Vector3 endPos;
        float startTime;
        float endTime;
        bool isCurrentbackGroundRotating = false;
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
            if (currentbackGround)
            {
                StartRotateCurrentbackGround();
            }
        }

        float backGroundAngleSpace = 1f;
        float currentAngle = 0f;
        // Use this for initialization
        void Start()
        {
            //PlayerPrefs.DeleteAll();
            lockColor.a = 0;    // need this for later setting material colors to work

            int currentbackGroundIndex = BackGroundManager.Instance.CurrentBackGroundIndex;
            currentbackGroundIndex = Mathf.Clamp(currentbackGroundIndex, 0, BackGroundManager.Instance.backGrounds.Length - 1);
            centerPoint = transform.TransformPoint(centerPoint);

            switch (scrollerStyle)
            {
                case ScrollerStyle.Line:
                    BackGroundScrollerCamera.GetComponent<Camera>().orthographic = true;
                    break;
                case ScrollerStyle.Circle:
                    BackGroundScrollerCamera.GetComponent<Camera>().orthographic = false;
                    break;
                default:
                    break;
            }

            backGroundAngleSpace = Mathf.PI * 2 / BackGroundManager.Instance.backGrounds.Length;
            currentAngle = currentbackGroundIndex * backGroundAngleSpace;
            for (int i = 0; i < BackGroundManager.Instance.backGrounds.Length; i++)
            {
                int deltaIndex = i - currentbackGroundIndex;

                GameObject backGround = (GameObject)Instantiate(BackGroundManager.Instance.backGrounds[i], centerPoint, Quaternion.Euler(0,0,0));
                backGround.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                BackGround charData = backGround.GetComponent<BackGround>();
                charData.backGroundSequenceNumber = i;
                listbackGround.Add(backGround);
                backGround.transform.localScale = originalScale;
                //backGround.transform.position = centerPoint + new Vector3(deltaIndex * backGroundSpace, 0, 0);

                // Set color based on locking status
                //Renderer charRdr = backGround.GetComponentInChildren<Renderer>();

                //if (charData.IsUnlocked)
                //    charRdr.material.SetColor("_Color", Color.white);
                //else
                //    charRdr.material.SetColor("_Color", lockColor);

                // Set as child of this object
                backGround.transform.parent = transform;
                switch (scrollerStyle)
                {
                    case ScrollerStyle.Line:
                        backGround.transform.localPosition += new Vector3(deltaIndex * BackGroundSpace, 0, 0);
                        break;
                    case ScrollerStyle.Circle:
                        backGround.transform.localPosition = transform.InverseTransformPoint(centerPoint) + new Vector3(Mathf.Sin(-currentAngle + i * backGroundAngleSpace), 0, -Mathf.Cos(-currentAngle + i * backGroundAngleSpace)) * BackGroundScrollerRadius;
                        break;
                    default:
                        break;
                }
                // Set layer for camera culling
                backGround.gameObject.layer = LayerMask.NameToLayer("BackGroundSelectionUI");
                //GameObject child = backGround.gameObject.transform.GetChild(0).gameObject;
                //child.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("BackGroundSelectionUI");
                //if (backGround.gameObject.transform.childCount > 0)
                //{
                //    for (int j = 0; j < backGround.gameObject.transform.childCount; j++)
                //        backGround.gameObject.transform.GetChild(j).gameObject.layer = LayerMask.NameToLayer("BackGroundSelectionUI");
                //}
            }

            // Highlight current backGround
            currentbackGround = listbackGround[currentbackGroundIndex];
            switch (scrollerStyle)
            {
                case ScrollerStyle.Line:
                    currentbackGround.transform.localScale = maxScale * originalScale;
                    currentbackGround.transform.localPosition += moveForwardAmount * Vector3.forward;
                    break;
                case ScrollerStyle.Circle:
                    currentbackGround.transform.localScale = maxScale * originalScale;
                    break;
                default:
                    break;
            }

            lastCurrentbackGround = null;
            StartRotateCurrentbackGround();
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
                    if (isCurrentbackGroundRotating)
                        StopRotateCurrentbackGround(true);

                    float speed = deltaX / (endTime - startTime);
                    Vector3 dir = (startPos.x - endPos.x < 0) ? Vector3.right : Vector3.left;
                    Vector3 moveVector = dir * (speed / 10) * scrollSpeedFactor * Time.fixedDeltaTime;
                    currentAngle -= moveVector.x * backGroundAngleSpace / 5;
                    if (currentAngle > Mathf.PI * 2)
                    {
                        currentAngle -= Mathf.PI * 2;
                    }
                    else if (currentAngle < 0)
                    {
                        currentAngle += Mathf.PI * 2;
                    }
                    // Move and scale the children
                    for (int i = 0; i < listbackGround.Count; i++)
                    {
                        switch (scrollerStyle)
                        {
                            case ScrollerStyle.Line:
                                MoveAndScale(listbackGround[i].transform, moveVector);
                                break;
                            case ScrollerStyle.Circle:
                                MoveAndScaleCircleVer(listbackGround[i].transform, moveVector, i);
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
                    // Store the last currentbackGround
                    lastCurrentbackGround = currentbackGround;

                    // Update current backGround to the one nearest to center point
                    switch (scrollerStyle)
                    {
                        case ScrollerStyle.Line:
                            currentbackGround = FindbackGroundNearestToCenter();
                            // Snap
                            float snapDistance = transform.InverseTransformPoint(centerPoint).x - currentbackGround.transform.localPosition.x;
                            StartCoroutine(SnapAndRotate(snapDistance));
                            break;
                        case ScrollerStyle.Circle:
                            currentbackGround = FindbackGroundNearestToCenterCircleVer();
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
            BackGround charData = currentbackGround.GetComponent<BackGround>();

            if (!charData.isFree)
            {
                priceText.gameObject.SetActive(true);
                priceText.text = charData.price.ToString();
            }
            else
            {
                priceText.gameObject.SetActive(false);
            }

            if (currentbackGround != lastCurrentbackGround)
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

        public void ShowBackGroundFromCamera()
        {
            Camera selectionCamera = BackGroundScrollerCamera.GetComponent<Camera>();
            selectionCamera.cullingMask = 2048;
        }

        void MoveAndScale(Transform tf, Vector3 moveVector)
        {
            // Move
            tf.localPosition += moveVector;

            // Scale and move forward according to distance from current position to center position
            float d = Mathf.Abs(tf.localPosition.x - transform.InverseTransformPoint(centerPoint).x);
            if (d < (BackGroundSpace / 2))
            {
                float factor = 1 - d / (BackGroundSpace / 2);
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
            tf.localPosition = transform.InverseTransformPoint(centerPoint) + new Vector3(Mathf.Sin(-currentAngle + index * backGroundAngleSpace), 0, -Mathf.Cos(-currentAngle + index * backGroundAngleSpace)) * BackGroundScrollerRadius;
            //Scale and move forward according to distance from current position to center position
            float d = Mathf.Abs(Vector3.Angle(Vector3.back, (tf.localPosition - transform.InverseTransformPoint(centerPoint)).normalized) * Mathf.Deg2Rad);
            if (d < (backGroundAngleSpace / 2))
            {
                float factor = 1 - d / (backGroundAngleSpace / 2);
                float scaleFactor = Mathf.Lerp(minScale, maxScale, factor);
                tf.localScale = scaleFactor * originalScale;
            }
            else
            {
                tf.localScale = minScale * originalScale;
            }
        }

        GameObject FindbackGroundNearestToCenter()
        {
            float min = -1;
            GameObject nearestObj = null;

            for (int i = 0; i < listbackGround.Count; i++)
            {
                float d = Mathf.Abs((listbackGround[i].transform.position - centerPoint).magnitude);
                if (d < min || min < 0)
                {
                    min = d;
                    nearestObj = listbackGround[i];
                }
            }

            return nearestObj;
        }

        GameObject FindbackGroundNearestToCenterCircleVer()
        {
            GameObject nearestObj = null;

            int neareastObjIndex = Mathf.RoundToInt(currentAngle / backGroundAngleSpace);
            if (neareastObjIndex < 0)
            {
                neareastObjIndex = listbackGround.Count - 1;
            }
            else if (neareastObjIndex > listbackGround.Count - 1)
            {
                neareastObjIndex = 0;
            }
            nearestObj = listbackGround[neareastObjIndex];
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
                for (int i = 0; i < listbackGround.Count; i++)
                {
                    MoveAndScale(listbackGround[i].transform, moveVector);
                }

                movedDistance += d;
                yield return null;
            }

            if (currentbackGround != lastCurrentbackGround || !isCurrentbackGroundRotating)
            {
                // Stop rotating the last current backGround
                StopRotateCurrentbackGround();

                // Now rotate the new current backGround
                StartRotateCurrentbackGround();
            }
        }

        IEnumerator SnapAndRotateCircleVer()
        {
            float nextAngle = Mathf.RoundToInt(currentAngle / backGroundAngleSpace) * backGroundAngleSpace;
            SoundManager.Instance.PlaySound(SoundManager.Instance.tick);
            while (Mathf.Abs(currentAngle - nextAngle) > 0.01f)
            {
                Vector3 moveVector = new Vector3((nextAngle - currentAngle) / snapTime * 10 * Time.fixedDeltaTime, 0, 0);
                currentAngle += moveVector.x * backGroundAngleSpace;
                if (currentAngle > Mathf.PI * 2)
                {
                    currentAngle -= Mathf.PI * 2;
                }
                else if (currentAngle < 0)
                {
                    currentAngle += Mathf.PI * 2;
                }
                for (int i = 0; i < listbackGround.Count; i++)
                {
                    MoveAndScaleCircleVer(listbackGround[i].transform, moveVector, i);
                }
                yield return null;
            }
            if (currentbackGround != lastCurrentbackGround || !isCurrentbackGroundRotating)
            {
                // Stop rotating the last current backGround
                StopRotateCurrentbackGround();

                // Now rotate the new current backGround
                StartRotateCurrentbackGround();
            }
        }

        void StartRotateCurrentbackGround()
        {
            StopRotateCurrentbackGround(false);   // stop previous rotation if any
            rotateCoroutine = CRRotatebackGround(currentbackGround.transform);
            StartCoroutine(rotateCoroutine);
            isCurrentbackGroundRotating = true;
        }

        void StopRotateCurrentbackGround(bool resetRotation = false)
        {
            if (rotateCoroutine != null)
            {
                StopCoroutine(rotateCoroutine);
            }

            isCurrentbackGroundRotating = false;

            if (resetRotation)
                StartCoroutine(CRResetbackGroundRotation(currentbackGround.transform));
        }

        IEnumerator CRRotatebackGround(Transform charTf)
        {
            while (true)
            {
                charTf.Rotate(new Vector3(0, rotateSpeed * Time.fixedDeltaTime, 0));
                yield return null;
            }
        }

        IEnumerator CRResetbackGroundRotation(Transform charTf)
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

        public void UnlockbackGround()
        {
            bool unlockSucceeded = currentbackGround.GetComponent<BackGround>().Unlock(false);
            if (unlockSucceeded)
            {
                //currentbackGround.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                unlockButton.gameObject.SetActive(false);
                selectButon.gameObject.SetActive(true);

                SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
            }
        }

        public void SelectbackGround()
        {
            if (currentbackGround.GetComponent<BackGround>().backGroundSequenceNumber != BackGroundManager.Instance.CurrentBackGroundIndex)
            {
                int backGroundID = currentbackGround.GetComponent<BackGround>().backGroundSequenceNumber;
                GameManager.Instance.playerController.SetBackGround(backGroundID);
            }
            BackGroundManager.Instance.CurrentBackGroundIndex = currentbackGround.GetComponent<BackGround>().backGroundSequenceNumber;
        }
    }
}