using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace BallLine
{
    [ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
    public class EditorPathScript : MonoBehaviour {
        //public static EditorPathScript PahtInstance;
        public bool takeScreenShot = false;
        public Color rayColor = Color.white;
        public List<Transform> pathObjects = new List<Transform>();
        public Vector3[] pathPoints;
        public Material lineMaterial;
        public GameObject[] canvas;

        GameObject parent;

        Transform[] theArray;

        List<GameObject> controlPoints = new List<GameObject>();
        public Color colorLineRender = Color.white;
        public float widthLineRender = 0.2f;
        LineRenderer lineRenderer;

        //Curve 2 points
        private int curveCount = 0;
        private int layerOrder = 0;
        private int SEGMENT_COUNT = 50;

        int numberPointsCurve;

        GameManager gameManager;
        Vector3 centerPoint;
        GameObject plane;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                lineRenderer = GetComponent<LineRenderer>();
                lineMaterial.SetColor("_Color", colorLineRender);
                if (lineRenderer.sharedMaterial != lineMaterial)
                    lineRenderer.sharedMaterial = lineMaterial;
                lineRenderer.startWidth = widthLineRender;
                lineRenderer.endWidth = widthLineRender;
                theArray = new Transform[transform.childCount];
                Gizmos.color = rayColor;
                for (int i = 0; i < transform.childCount; i++)
                {
                    theArray[i] = transform.GetChild(i).gameObject.GetComponent<Transform>();
                }
                pathObjects.Clear();

                foreach (Transform pathObject in theArray)
                {
                    if (pathObject != this.transform)
                    {
                        pathObjects.Add(pathObject);
                    }
                }

                for (int i = 0; i < pathObjects.Count; i++)
                {
                    Vector3 position = pathObjects[i].position;
                    if (i > 0)
                    {
                        Transform child = pathObjects[i];
                        Transform grandChild0 = child.GetChild(0);
                        Transform grandChild1 = child.GetChild(1);
                        Vector3 previous = pathObjects[i - 1].position;
                        Gizmos.color = colorLineRender;
                        Gizmos.DrawLine(previous, position);
                        Gizmos.color = colorLineRender;
                        Gizmos.DrawWireSphere(position, 0.5f);
                        Gizmos.color = colorLineRender;
                        Gizmos.DrawLine(grandChild0.position, position);
                        Gizmos.color = colorLineRender;
                        Gizmos.DrawLine(grandChild1.position, position);
                        Gizmos.color = colorLineRender;
                        Gizmos.DrawWireSphere(grandChild0.position, 0.1f);
                        Gizmos.color = colorLineRender;
                        Gizmos.DrawWireSphere(grandChild1.position, 0.1f);
                    }
                }
                //*** Uncomment this code block to show points in the path that the ball will move to

                //for (int i = 0; i < pathPoints.Length; i++)
                //{
                //    Vector3 position = pathPoints[i];
                //    if (i > 0)
                //    {
                //        Gizmos.color = Color.yellow;
                //        Gizmos.DrawWireSphere(position, 0.3f);
                //    }
                //}
                lineRenderer.positionCount = 0;
                DrawLineRender();
            }
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                if (canvas[0] != null)
                    canvas[0].SetActive(true);
                if (canvas[1] != null)
                    canvas[1].SetActive(true);
            }
        }

        // Use this for initialization
        void Start() {
            if (!Application.isPlaying)
            {
                canvas = new GameObject[2];
                gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                canvas[0] = gameManager.characterUI;
                canvas[1] = gameManager.mainCanvas;

                if (canvas[0] != null)
                    canvas[0].SetActive(false);
                if (canvas[1] != null)
                    canvas[1].SetActive(false);
            }
            numberPointsCurve = 5;
            // update line renderer
            lineRenderer = GetComponent<LineRenderer>();
            lineMaterial.SetColor("_Color", colorLineRender);
            lineRenderer.sharedMaterial = lineMaterial;
            lineRenderer.startColor = colorLineRender;
            lineRenderer.endColor = colorLineRender;
            lineRenderer.startWidth = widthLineRender;
            lineRenderer.endWidth = widthLineRender;
            lineRenderer.positionCount = 0;
            //Draw line renderer
            DrawLineRender();
        }

        public void DrawLineRender()
        {
            int curvePointsCount = 0;
            int oldCurvePointsCount;
            List<Vector3> storePoint = new List<Vector3>();
            bool isCreateCurved = false;
            //Loop to access to every child object and get child object's position to draw the line
            for (int j = 0; j < transform.childCount; j++)
            {
                Points behindPoint = transform.GetChild(0).GetComponent<Points>();
                if(j>0)
                behindPoint = transform.GetChild(j - 1).GetComponent<Points>();
                Points currentPoint = transform.GetChild(j).GetComponent<Points>();
                Points aheadPoint = transform.GetChild(transform.childCount-1).GetComponent<Points>();
                if(j<transform.childCount-2)
                aheadPoint=transform.GetChild(j+1).GetComponent<Points>();

                if (j > 1 && !currentPoint.isCurvedWithBehindPoint)
                    transform.GetChild(j).transform.GetChild(0).transform.position = transform.GetChild(j - 1).transform.position;

                if (j < transform.childCount - 1 && !currentPoint.isCurvedWitAheadPoint)
                    transform.GetChild(j).transform.GetChild(1).transform.position = transform.GetChild(j + 1).transform.position;

                oldCurvePointsCount = curvePointsCount;
                //Add to make curved line list if stick in curve with behind box and make a curved line
                if (currentPoint.isCurvedWithBehindPoint
                    && behindPoint.isCurvedWitAheadPoint)
                {
                    GameObject endPointchild = transform.GetChild(j).gameObject;
                    GameObject endPointGrandChild = endPointchild.transform.GetChild(0).gameObject;
                    controlPoints.Add(endPointGrandChild);
                    controlPoints.Add(transform.GetChild(j).gameObject);
                    bool isSameAxis = false;

                    if (currentPoint.isAutoGenerate)
                    {
                        AutoSetVectorTangent(j, out isSameAxis);
                    }
                    if (!isSameAxis)
                    {
                        curvePointsCount++;
                        int startPosition = lineRenderer.positionCount - 1;
                        List<Vector3> curvedPoints = DrawCurve(controlPoints);
                        lineRenderer.positionCount += curvedPoints.Count - 1;
                        int count = numberPointsCurve;
                        for (int k = 0; k < curvedPoints.Count; k++)
                        {
                            if (count == numberPointsCurve)
                            {
                                storePoint.Add(curvedPoints[k]);
                                count = 0;
                            }
                            else
                                count++;
                            Vector3 localPos = gameObject.transform.InverseTransformPoint(curvedPoints[k]);
                            lineRenderer.SetPosition(startPosition, localPos);
                            startPosition++;
                        }
                        controlPoints.Clear();
                        curvedPoints.Clear();
                        isCreateCurved = true;
                    }
                    else isCreateCurved = false;
                    //If both stick in the curved with ahead and behind box
                    if (currentPoint.isCurvedWitAheadPoint && aheadPoint.isCurvedWithBehindPoint)
                    {
                        oldCurvePointsCount = curvePointsCount;
                        controlPoints.Add(transform.GetChild(j).gameObject);
                        GameObject child = transform.GetChild(j).gameObject;
                        GameObject grandChild = child.transform.GetChild(1).gameObject;
                        controlPoints.Add(grandChild);
                        curvePointsCount = 1;
                    }
                }
                //Add to make curved line list if stick in curve with ahead box
                else if (currentPoint.isCurvedWitAheadPoint && aheadPoint.isCurvedWithBehindPoint)// && !transform.GetChild(j).gameObject.GetComponent<Points>().isCurvedWithBehindPoint)
                {
                    controlPoints.Add(transform.GetChild(j).gameObject);
                    GameObject child = transform.GetChild(j).gameObject;
                    GameObject grandChild = child.transform.GetChild(1).gameObject;
                    controlPoints.Add(grandChild);
                    curvePointsCount++;
                }
                else
                {
                    isCreateCurved = false;
                    curvePointsCount = 0;
                }

                //Create straight line if not stick in the curved box
                if (curvePointsCount == 0)
                {
                    GameObject grandChildBehind;
                    GameObject grandChildAhead;
                    if (j < transform.childCount - 1)
                    {
                        if (isCreateCurved)
                        {
                            isCreateCurved = false;
                            lineRenderer.positionCount++;
                            storePoint.Add(transform.GetChild(j - 1).gameObject.transform.position);

                            grandChildAhead = transform.GetChild(j-1).gameObject.transform.GetChild(1).gameObject;
                            grandChildAhead.transform.position = transform.GetChild(j).gameObject.transform.position;

                            Vector3 localPos = gameObject.transform.InverseTransformPoint(transform.GetChild(j - 1).gameObject.transform.position);
                            lineRenderer.SetPosition(lineRenderer.positionCount - 1, localPos);
                        }
                        lineRenderer.positionCount++;
                        Vector3 localPos1 = gameObject.transform.InverseTransformPoint(transform.GetChild(j).gameObject.transform.position);
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, localPos1);
                    }
                    storePoint.Add(transform.GetChild(j).gameObject.transform.position);
                    if (j > 0)
                    {
                        grandChildBehind = transform.GetChild(j).gameObject.transform.GetChild(0).gameObject;
                        grandChildBehind.transform.position = transform.GetChild(j - 1).gameObject.transform.position;
                    }
                    if(j<transform.childCount-2)
                    {
                                                grandChildAhead = transform.GetChild(j).gameObject.transform.GetChild(1).gameObject;
                        grandChildAhead.transform.position = transform.GetChild(j+1).gameObject.transform.position;
                    }
                }
                else if (curvePointsCount == 1 && oldCurvePointsCount < 2)
                {
                    GameObject grandChildBehind;
                    GameObject grandChildAhead;

                    lineRenderer.positionCount++;
                    storePoint.Add(transform.GetChild(j - 1).gameObject.transform.position);

                    grandChildBehind = transform.GetChild(j - 1).gameObject.transform.GetChild(0).gameObject;
                    grandChildBehind.transform.position = transform.GetChild(j - 2).gameObject.transform.position;

                    grandChildAhead = transform.GetChild(j - 1).gameObject.transform.GetChild(1).gameObject;
                    grandChildAhead.transform.position = transform.GetChild(j).gameObject.transform.position;

                    Vector3 localPos = gameObject.transform.InverseTransformPoint(transform.GetChild(j - 1).gameObject.transform.position);
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, localPos);
                }
            }
            storePoint = storePoint.Distinct().ToList();
            List<Vector3> removePosition = new List<Vector3>();
            bool isFirst = true;
            Vector3 newPosition;
            Vector3 comparePosition = Vector3.zero;
            for (int i = 0; i < storePoint.Count; i++)
            {
                if (i > 0)
                {
                    newPosition = storePoint[i];
                    if (isFirst)
                    {
                        isFirst = false;
                        comparePosition = storePoint[i - 1];
                    }
                    if (Vector3.Distance(newPosition, comparePosition) < 1)
                    {
                        removePosition.Add(storePoint[i]);
                    }
                    else
                    {
                        comparePosition = newPosition;
                    }
                }
            }
            for (int i = 0; i < removePosition.Count-1; i++)
            {
                for (int j = 0; j < storePoint.Count; j++)
                {
                    if (storePoint[j] == removePosition[i])
                    {
                        storePoint.Remove(storePoint[j]);
                        break;
                    }
                }
            }
            for (int i = 0; i < storePoint.Count; i++)
            {
                if (i > 0)
                {
                    Vector3 oldPosition = storePoint[i - 1];
                    newPosition = storePoint[i];
                    if (Vector3.Distance(newPosition, oldPosition) < 1f)
                    {
                        storePoint.Remove(storePoint[i]);
                    }
                }
            }
            removePosition.Clear();

            pathPoints = new Vector3[storePoint.Count];
            pathPoints = storePoint.ToArray();
        }

        void FindErrorPoint()
        {

        }

        //Create Curved line with given points list
        public List<Vector3> DrawCurve(List<GameObject> controlCurvePoint)
        {
            List<Vector3> curvedPoints = new List<Vector3>();
            curvedPoints.Clear();
            Vector3 firstPosition = controlCurvePoint[0].transform.position;
            Vector3 endPosition = controlCurvePoint[controlCurvePoint.Count - 1].transform.position;
            lineRenderer.sortingLayerID = layerOrder;
            curveCount = (int)controlCurvePoint.Count / 3;
            curvedPoints.Add(firstPosition);
            for (int j = 0; j < curveCount; j++)
            {
                for (int i = 1; i <= SEGMENT_COUNT; i++)
                {
                    float t = i / (float)SEGMENT_COUNT;
                    int nodeIndex = j * 3;
                    Vector3 pixel = CalculateCubicBezierPoint(t, controlCurvePoint[nodeIndex].transform.position, controlCurvePoint[nodeIndex + 1].transform.position, controlCurvePoint[nodeIndex + 2].transform.position, controlCurvePoint[nodeIndex + 3].transform.position);
                    curvedPoints.Add(pixel);
                }

            }
            return curvedPoints;
        }

        //Calculate to create new curved point
        Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }

        void CalculateMidPoint(GameObject pointA,GameObject pointB,Vector3 direction1, Vector3 direction2, float distanceFromLine
            , out Vector3 directionOut1, out Vector3 directionOut2)
        {
            float errorFix = 0;
            if (distanceFromLine == 0)
                errorFix = 1f;
            directionOut1 = pointA.transform.position + direction1.normalized*(distanceFromLine+errorFix);
            directionOut2 = pointB.transform.position + direction2.normalized *( distanceFromLine+errorFix);
        }

        void AutoSetVectorTangent(int j,out bool isSameAsix)
        {
            isSameAsix = false;
            float protrusion = 0;
            protrusion = transform.GetChild(j).gameObject.GetComponent<Points>().protrusionWhenAutoGenerate;
            Vector3 positionGenerate1;
            Vector3 positionGenerate2;

            Vector3 direction1 = transform.GetChild(j - 1).gameObject.transform.position
                - transform.GetChild(j - 1).transform.GetChild(0).gameObject.transform.position;

            Vector3 direction2 = transform.GetChild(j).gameObject.transform.position
                - transform.GetChild(j).transform.GetChild(1).gameObject.transform.position;

            Vector3 directionAhead = transform.GetChild(j).gameObject.transform.position
                - transform.GetChild(j+1).gameObject.transform.position;

            Points aheadPoint = transform.GetChild(j).gameObject.GetComponent<Points>();
            Points behindPoint = transform.GetChild(j-1).gameObject.GetComponent<Points>();


            //Check if the position is parrallel with the position behind then fix this position
            if ((aheadPoint.isFixPosition || behindPoint.isFixPosition) 
                && CheckParallel(direction1, direction2) =="parallel" 
                && Mathf.Abs(direction1.magnitude - directionAhead.magnitude) > 0.5f)
            {
                Vector3 position;
                if (direction1.magnitude > direction2.magnitude && aheadPoint.isFixPosition && !aheadPoint.hasFixed)
                {
                    position = Vector3.Project(direction1, directionAhead) + transform.GetChild(j + 1).gameObject.transform.position;
                    Vector3 localPos = gameObject.transform.InverseTransformPoint(position);
                    localPos.z = 0;
                    transform.GetChild(j).gameObject.transform.localPosition = localPos;
                    transform.GetChild(j).gameObject.GetComponent<Points>().hasFixed = true;
                }
                else
                if(behindPoint.isFixPosition && !behindPoint.hasFixed)
                {
                    position = Vector3.Project(directionAhead, direction1) + transform.GetChild(j - 1).transform.GetChild(0).gameObject.transform.position;
                    Vector3 localPos = gameObject.transform.InverseTransformPoint(position);
                    localPos.z = 0;
                    transform.GetChild(j - 1).gameObject.transform.localPosition = localPos;
                    transform.GetChild(j - 1).gameObject.GetComponent<Points>().hasFixed = true;
                }
            }

            //If the line ahead and behind is same axis then not make a curve line
            if (CheckSameAxis(direction1, direction2, j))
            {
                positionGenerate1 = transform.GetChild(j).gameObject.transform.position;
                positionGenerate2 = transform.GetChild(j - 1).gameObject.transform.position;
                isSameAsix = true;
            }
            else
            {
                if(Vector2.Angle(direction1, directionAhead)>85 && Vector2.Angle(direction1, directionAhead)<95)
                {
                    if (protrusion < 1 && protrusion > -1)
                        protrusion = 1;
                }
                CalculateMidPoint(transform.GetChild(j - 1).gameObject, transform.GetChild(j).gameObject, direction1, directionAhead, protrusion, out positionGenerate1, out positionGenerate2);
            }
            Vector3 localPos1 = transform.GetChild(j - 1).transform.InverseTransformPoint(positionGenerate1);

            Vector3 localPos2 = transform.GetChild(j).transform.InverseTransformPoint(positionGenerate2);

            if (controlPoints.Count >= 4)
            {
                controlPoints[1].transform.localPosition = localPos1;
                controlPoints[2].transform.localPosition = localPos2;
            }

        }

        string CheckParallel(Vector3 direction1,Vector3 direction2)
        {
            string result = "not";
            float xVectorCompare1 = direction1.normalized.x;
            float yVectorCompare1 = direction1.normalized.y;
            float zVectorCompare1 = direction1.normalized.z;
            float xVectorCompare2 = direction2.normalized.x;
            float yVectorCompare2 = direction2.normalized.y;
            float zVectorCompare2 = direction2.normalized.z;
            if (Mathf.Abs(xVectorCompare1 - xVectorCompare2) < 0.5f && Mathf.Abs(yVectorCompare1 - yVectorCompare2) < 0.5f
                && Mathf.Abs(zVectorCompare1 - zVectorCompare2) < 0.5f)
                result = "parallel";
            else
            {
                xVectorCompare1 = Mathf.Abs(xVectorCompare1);
                xVectorCompare2 = Mathf.Abs(xVectorCompare2);
                yVectorCompare1 = Mathf.Abs(yVectorCompare1);
                yVectorCompare2 = Mathf.Abs(yVectorCompare2);
                zVectorCompare1 = Mathf.Abs(zVectorCompare1);
                zVectorCompare2 = Mathf.Abs(zVectorCompare2);
                if (Mathf.Abs(xVectorCompare1 - xVectorCompare2) < 1f && Mathf.Abs(yVectorCompare1 - yVectorCompare2) < 1f
                    && Mathf.Abs(zVectorCompare1 - zVectorCompare2) < 1)
                    result = "inverse";
            }
            return result;
        }

        bool CheckSameAxis(Vector3 direction1,Vector3 direction2,int j)
        {
            bool result = false;
            if(CheckParallel(direction1, direction2)== "inverse")
            {
                float xVectorCompare1 = transform.GetChild(j - 1).gameObject.transform.position.x;
                float yVectorCompare1 = transform.GetChild(j - 1).transform.GetChild(0).gameObject.transform.position.x;
                float xVectorCompare2 = transform.GetChild(j).gameObject.transform.position.x;
                float yVectorCompare2 = transform.GetChild(j).transform.GetChild(1).gameObject.transform.position.x;
                if (Mathf.Abs(xVectorCompare1 - yVectorCompare1) < 0.5f 
                    && Mathf.Abs(xVectorCompare2 - yVectorCompare2) < 0.5f
                    && Mathf.Abs(xVectorCompare1 - xVectorCompare2) < 0.5f)
                {
                    result = true;
                }
                else
                {
                    float xCompare1 = transform.GetChild(j - 1).gameObject.transform.position.y;
                    float yCompare1 = transform.GetChild(j - 1).transform.GetChild(0).gameObject.transform.position.y;
                    float xCompare2 = transform.GetChild(j).gameObject.transform.position.y;
                    float yCompare2 = transform.GetChild(j).transform.GetChild(1).gameObject.transform.position.y;
                    if (Mathf.Abs(xCompare1 - yCompare1) < 0.5f && Mathf.Abs(xCompare2 - yCompare2) < 0.5f
                        && Mathf.Abs(xCompare1 - xCompare2) < 0.5f)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
        // Update is called once per frame
        void Update() {
            if (!Application.isPlaying)
            {
                parent = transform.parent.gameObject;
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    if (parent.transform.GetChild(i).name == "Plane")
                        plane = parent.transform.GetChild(i).gameObject;
                }
                centerPoint = new Vector3(plane.transform.position.x, plane.transform.position.y, plane.transform.position.z - 15);
                if (plane != null && Camera.main.transform.position != centerPoint)
                {
                    centerPoint = new Vector3(plane.transform.position.x, plane.transform.position.y, plane.transform.position.z - 15);
                    Camera.main.transform.position = centerPoint;
                }
            }
            if (takeScreenShot && canvas[0] != null && canvas[1] != null)
            {
                canvas[0].SetActive(false);
                canvas[1].SetActive(false);
                ScreenCapture.CaptureScreenshot(gameManager.ScreenShootPath + gameObject.transform.parent.name + ".png");
                Debug.Log("Take screen shot: " + gameObject.transform.parent.name);
                takeScreenShot = false;
            }
        }
    }
}
