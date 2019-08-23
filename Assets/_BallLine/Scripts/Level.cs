using UnityEngine;

namespace BallLine
{
    public class Level : MonoBehaviour
    {
        public int levelSequenceNumber;
        public string levelName;
        public int price;
        public bool isFree = false;
        //***uncomment the cobe below to use pass level when enough score feature

        //public int scoreToPass = 200;

        public bool IsUnlocked
        {
            get
            {
                return (isFree || PlayerPrefs.GetInt(levelName, 0) == 1);
            }
        }

        void Awake()
        {
            levelName = levelName.ToUpper();
        }

        public bool Unlock(bool isDefault=false)
        {
            if (IsUnlocked)
                return true;
            if(isDefault)
            {
                PlayerPrefs.SetInt(levelName, 1);
                PlayerPrefs.Save();

                return true;
            }
            if (CoinManager.Instance.Coins >= price)
            {
                PlayerPrefs.SetInt(levelName, 1);
                PlayerPrefs.Save();
                CoinManager.Instance.RemoveCoins(price);

                return true;
            }

            return false;
        }
    }
}