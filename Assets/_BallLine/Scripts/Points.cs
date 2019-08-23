using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BallLine
{
    public class Points : MonoBehaviour
    {

        public bool isCurvedWitAheadPoint;
        public bool isCurvedWithBehindPoint;

        public bool isAutoGenerate;
        public bool isFixPosition;

        public float protrusionWhenAutoGenerate;

        public bool hasFixed = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
