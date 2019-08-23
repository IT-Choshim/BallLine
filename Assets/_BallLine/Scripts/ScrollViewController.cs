using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BallLine
{
    public class ScrollViewController : MonoBehaviour
    {
        [Header("Reference Objects")]
        public Text coinText;
        Text coinComponent;
        public static GameObject CurrentSelectChar;
        public static GameObject CurrentSelectBackgr;
        public static GameObject CurrentSelectLevel;
        public GameObject imagePrefab;

        [Header("Congfig")]
        [SerializeField]
        private Vector2 ballBoardSize = new Vector2(250,260);

        [SerializeField]
        private Vector2 backgroundBoardSize = new Vector2(3 * 70, 4 * 70);

        [SerializeField]
        private Vector2 levelBoardSize = new Vector2(9 * 23, 18 * 23);
        // Use this for initialization
        private void OnEnable()
        {
            CoinManager.CoinsUpdated += UpdateGridUI;
        }

        private void OnDisable()
        {
            CoinManager.CoinsUpdated -= UpdateGridUI;
        }

        void UpdateGridUI(int x)
        {

        }

        void Start()
        {
            coinComponent = coinText.GetComponent<Text>();
            coinComponent.text = CoinManager.Instance.Coins.ToString();
            GridLayoutCharacter();
            GridLayoutBackGround();
            GridLayoutLevel();
            ClearScrollViewByTag("BackGround");
            ClearScrollViewByTag("Level");
        }

        public void ClearScrollViewByTag(string tag)
        {
            if(transform.childCount>0)
                for(int i=0;i<transform.childCount;i++)
                {
                    if (transform.GetChild(i).tag == tag)
                        transform.GetChild(i).gameObject.SetActive(false);
                }
        }

        public void ShowScrollViewByTag(string tag)
        {
            gameObject.transform.parent.transform.parent.GetComponent<ScrollRect>().verticalScrollbar.value = 1;
            GridLayoutGroup gridLayout=gameObject.GetComponent<GridLayoutGroup>();
            switch (tag)
            {
                case "BackGround":
                    gridLayout.cellSize = new Vector2(backgroundBoardSize.x, backgroundBoardSize.y);
                    gridLayout.spacing =new Vector2(25, 75);
                    break;
                case "Character":
                    gridLayout.cellSize = new Vector2(ballBoardSize.x, ballBoardSize.y);
                    gridLayout.spacing = new Vector2(25, 75);
                    break;
                case "Level":
                    gridLayout.cellSize = new Vector2(levelBoardSize.x, levelBoardSize.y);
                    gridLayout.spacing = new Vector2(25, 75);
                    break;
            }
            if (transform.childCount > 0)
                for (int i = 0; i < transform.childCount; i++)
                {
                    if(transform.GetChild(i).tag==tag)
                        transform.GetChild(i).gameObject.SetActive(true);
                }
        }

        public void GridLayoutBackGround()
        {
            for (int i = 0; i < BackGroundManager.Instance.backGroundFrame.Length; i++)
            {
                GameObject backgroundFrame = (GameObject)Instantiate(BackGroundManager.Instance.backGroundFrame[i], transform);
                GameObject background = backgroundFrame.transform.GetChild(0).gameObject;
                backgroundFrame.tag = "BackGround";
                BackGround backgroundData = background.GetComponent<BackGround>();
                GameObject lockImage = background.transform.GetChild(0).gameObject;
                GameObject unlockImage = background.transform.GetChild(1).gameObject;
                if (i == 0)
                    backgroundData.Unlock(true);
                if (i == BackGroundManager.Instance.CurrentBackGroundIndex)
                {
                    CurrentSelectBackgr = background;
                    background.GetComponent<OnClickBackGround>().ChangeOutLine();
                }
                if (backgroundData.isFree || backgroundData.IsUnlocked)
                {
                    lockImage.SetActive(false);
                    unlockImage.SetActive(false);
                }
                else
                {
                    if (CoinManager.Instance.Coins >= backgroundData.price)
                    {
                        lockImage.SetActive(false);
                        unlockImage.SetActive(true);
                    }
                    else
                    {
                        lockImage.SetActive(true);
                        unlockImage.SetActive(false);
                    }
                }
                backgroundData.backGroundSequenceNumber = i;
            }
        }

        void GridLayoutCharacter()
        {
            gameObject.GetComponent<GridLayoutGroup>().cellSize = new Vector2(ballBoardSize.x, ballBoardSize.y);
            for (int i = 0; i < CharacterManager.Instance.characters.Length; i++)
            {
                GameObject character = (GameObject)Instantiate(CharacterManager.Instance.characters[i], transform);
                character.tag = "Character";
                Character characterData = character.GetComponent<Character>();
                GameObject lockImage = character.transform.GetChild(3).gameObject;
                GameObject unlockImage = character.transform.GetChild(4).gameObject;
                if (i == 0)
                    characterData.Unlock(true);
                if (i == CharacterManager.Instance.CurrentCharacterIndex)
                {
                    CurrentSelectChar = character;
                    character.GetComponent<OnClickCharacter>().ChangeOutLine();
                }
                if (characterData.isFree || characterData.IsUnlocked)
                {
                    lockImage.SetActive(false);
                    unlockImage.SetActive(false);
                }
                else
                {
                    if (CoinManager.Instance.Coins >= characterData.price)
                    {
                        lockImage.SetActive(false);
                        unlockImage.SetActive(true);
                    }
                    else
                    {
                        lockImage.SetActive(true);
                        unlockImage.SetActive(false);
                    }
                }
                characterData.characterSequenceNumber = i;
                for(int j=0;j<3;j++)
                {
                    GameObject newObject = ObjectPooling.SharedInstance.poolObject[i].itemsToPool[j].objectToPool;
                    GameObject child = character.transform.GetChild(j).gameObject;
                    child.GetComponent<Image>().sprite = newObject.GetComponent<SpriteRenderer>().sprite;
                }
            }
        }

        void GridLayoutLevel()
        {
            for (int i = 0; i < LevelManager.Instance.levelTests.Length; i++)
            {
                GameObject levelButtonFrame = (GameObject)Instantiate(imagePrefab, transform);
                GameObject levelButton = levelButtonFrame.transform.GetChild(0).gameObject;
                levelButtonFrame.tag = "Level";
                Level levelData = LevelManager.Instance.levelTests[i].level.GetComponent<Level>();
                levelButton.GetComponent<Image>().sprite = LevelManager.Instance.levelTests[i].levelImage;
                levelButton.GetComponent<OnClickLevel>().levelData = levelData;
                GameObject lockImage = levelButton.transform.GetChild(0).gameObject;
                GameObject unlockImage = levelButton.transform.GetChild(1).gameObject;
                if (i == 0)
                    levelData.Unlock(true);
                if (i == LevelManager.Instance.CurrentLevelIndex)
                {
                    CurrentSelectLevel = levelButton;
                    levelButton.GetComponent<OnClickLevel>().ChangeOutLine();
                }
                if (levelData.isFree || levelData.IsUnlocked)
                {
                    lockImage.SetActive(false);
                    unlockImage.SetActive(false);
                }
                else
                {
                    if (CoinManager.Instance.Coins >= levelData.price)
                    {
                        lockImage.SetActive(false);
                        unlockImage.SetActive(true);
                    }
                    else
                    {
                        lockImage.SetActive(true);
                        unlockImage.SetActive(false);
                    }
                }
                levelData.levelSequenceNumber = i;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (gameObject.activeSelf)
            {
                int newCoin = 0;
                int.TryParse(coinComponent.text, out newCoin);
                if (CoinManager.Instance.Coins != newCoin)
                    coinComponent.text = CoinManager.Instance.Coins.ToString();
            }
        }
    }
}