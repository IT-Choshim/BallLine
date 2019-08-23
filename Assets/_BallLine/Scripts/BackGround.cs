using UnityEngine;

namespace BallLine
{
    public class BackGround : MonoBehaviour
    {
        public int backGroundSequenceNumber;
        public string backGroundName;
        public int price;
        public bool isFree = false;

        public bool IsUnlocked
        {
            get
            {
                return (isFree || PlayerPrefs.GetInt(backGroundName, 0) == 1);
            }
        }

        void Awake()
        {
            backGroundName = backGroundName.ToUpper();
        }

        public bool Unlock(bool isDefault)
        {
            if (IsUnlocked)
                return true;
            if(isDefault)
            {
                PlayerPrefs.SetInt(backGroundName, 1);
                PlayerPrefs.Save();

                return true;
            }

            if (CoinManager.Instance.Coins >= price)
            {
                PlayerPrefs.SetInt(backGroundName, 1);
                PlayerPrefs.Save();
                CoinManager.Instance.RemoveCoins(price);

                return true;
            }

            return false;
        }
    }
}