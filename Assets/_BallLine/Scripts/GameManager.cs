using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

namespace BallLine
{
    public enum GameState
    {
        Prepare,
        Playing,
        Paused,
        PreGameOver,
        GameOver
    }

    public enum CoinEarned
    {
        OnceCombo,
        EachCombo,
        IncreaseEachCombo
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event System.Action<GameState, GameState> GameStateChanged;
        [Header("Gameplay Config")]

        [SerializeField]
        private string screenShootPath = "Assets/_BallLine/ScreenShot/";
        public string ScreenShootPath
        {
            get { return screenShootPath; }
        }

        [Range(0f, 1f)]
        public float coinFrequency = 0.1f;

        public Color selectShadowColor = Color.green;
        public Color normalShadowColor = Color.black;
        public Color selectTextColor = Color.yellow;
        public Color normalTextColor = Color.white;

        public float moveBallSpeed=1;

        public float shootBallSpeed = 0.1f;

        public int createBombBallAtComboScore=0;

        public float bombBallExplosionRadius = 2.5f;

        public float speedUp = 1;

        public float distanceFromCamera=15.8911f;

        public float moveBackSpeed = 30;

        public float timeSpeedUp=0.5f;

        public CoinEarned monetizationOptions = CoinEarned.OnceCombo;

        public int earnCoinAtComboScore = 1;

        public int coinEarned=1;
        [Range(1,100)]
        public int amountCoinIncrease=0;

        public string backgroundMessage = "Do you want to unlock this background?";

        public string levelMessage = "Do you want to unlock this level?";

        public string characterMessage = "Do you want to unlock this model?";

        private static bool isRestart;

        public GameState GameState
        {
            get
            {
                return _gameState;
            }
            private set
            {
                if (value != _gameState)
                {
                    GameState oldState = _gameState;
                    _gameState = value;

                    if (GameStateChanged != null)
                        GameStateChanged(_gameState, oldState);
                }
            }
        }

        public static int GameCount
        {
            get { return _gameCount; }
            private set { _gameCount = value; }
        }

        private static int _gameCount = 0;

        [Header("Set the target frame rate for this game")]
        [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
        public int targetFrameRate = 30;

        [Header("Current game state")]
        [SerializeField]
        private GameState _gameState = GameState.Prepare;

        // List of public variables referencing other objects
        [Header("Object References")]
        public PlayerController playerController;
        public GameObject mainCanvas;
        public GameObject characterUI;

        void OnEnable()
        {
            PlayerController.PlayerDied += PlayerController_PlayerDied;
        }

        void OnDisable()
        {
            PlayerController.PlayerDied -= PlayerController_PlayerDied;
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(Instance.gameObject);
                Instance = this;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Use this for initialization
        void Start()
        {
            // Initial setup
            Application.targetFrameRate = targetFrameRate;
            ScoreManager.Instance.Reset();

            PrepareGame();
        }

        // Update is called once per frame
        void Update()
        {
        }

        // Listens to the event when player dies and call GameOver
        void PlayerController_PlayerDied()
        {
            GameOver();
        }

        // Make initial setup and preparations before the game can be played
        public void PrepareGame()
        {
            GameState = GameState.Prepare;

            // Automatically start the game if this is a restart.
            if (isRestart)
            {
                isRestart = false;
                StartGame();
            }
        }

        // A new game official starts
        public void StartGame()
        {
            GameState = GameState.Playing;
            if (SoundManager.Instance.background != null)
            {
                SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
            }
        }

        // Called when the player died
        public void GameOver()
        {
            if (SoundManager.Instance.background != null)
            {
                SoundManager.Instance.StopMusic();
            }

            SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
            GameState = GameState.GameOver;
            GameCount++;

            // Add other game over actions here if necessary
        }

        // Start a new game
        public void RestartGame(float delay = 0)
        {
            isRestart = true;
            StartCoroutine(CRRestartGame(delay));
        }

        IEnumerator CRRestartGame(float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void HidePlayer()
        {
            if (playerController != null)
                playerController.gameObject.SetActive(false);
        }

        public void ShowPlayer()
        {
            if (playerController != null)
                playerController.gameObject.SetActive(true);
        }

        void StopSpeedUp()
        {
            if (playerController.GetComponent<PlayerController>().isDie)
            {
                playerController.GetComponent<PlayerController>().RaiseEventDie();
            }
            else
            {
                StartCoroutine(Delay());
                playerController.isSpeedUp = false;
                playerController.isBengin = false;
                playerController.EndSpeedUp();
                playerController.CreateShootBall();
                playerController.isPlay = true;
            }

        }

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.1f);
            playerController.createBallSpeed = (0.97f / GameManager.Instance.moveBallSpeed);
        }

        public void SpeedUp()
        {
            if (playerController.GetComponent<PlayerController>().isDie)
            {
                playerController.StartSpeedUp();
                Invoke("StopSpeedUp", timeSpeedUp * Time.timeScale*0.5f);
            }
            else
            {
                //Time.timeScale = speedUp;
                playerController.isBengin = true;
                playerController.isSpeedUp = true;
                Invoke("StopSpeedUp", timeSpeedUp * Time.timeScale);
            }

        }
    }
}