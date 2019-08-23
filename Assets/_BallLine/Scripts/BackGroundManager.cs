using UnityEngine;
using System.Collections;

namespace BallLine
{
    public class BackGroundManager : MonoBehaviour
    {
        public static BackGroundManager Instance;

        public static readonly string CURRENT_BackGround_KEY = "SGLIB_CURRENT_BackGround";

        public int CurrentBackGroundIndex
        {
            get
            {
                return PlayerPrefs.GetInt(CURRENT_BackGround_KEY, 0);
            }
            set
            {
                PlayerPrefs.SetInt(CURRENT_BackGround_KEY, value);
                PlayerPrefs.Save();
            }
        }
        public GameObject[] backGroundFrame;
        [HideInInspector]
        public GameObject[] backGrounds;

        void Awake()
        {
            backGrounds = new GameObject[backGroundFrame.Length];
            for(int i=0;i<backGroundFrame.Length;i++)
            {
                backGrounds[i] = backGroundFrame[i].transform.GetChild(0).gameObject;
            }
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}