using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BallLine
{
    public interface UnlockAction
    {
        void Unlock();
    }
    public class PopUpController : MonoBehaviour
    {
        public static PopUpController Instance { get; private set; }
        public Text priceText;
        public Text messageText;
        public GameObject btnUnlock;
        public GameObject btnBlock;
        public GameObject objectPopUp;
        void Awake()
        {
            if (Instance != null)
                DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
        private void Start()
        {
            HidePopUp();
        }

        public void SetMassage(string message)
        {
            messageText.text = message;
        }
        public void SetPrice(int price)
        {
            priceText.text = price.ToString();
        }
        public void ShowPopUp(bool canUnlock)
        {
            if (canUnlock)
            {
                btnUnlock.SetActive(true);
                btnBlock.SetActive(false);
            }
            else
            {
                btnUnlock.SetActive(false);
                btnBlock.SetActive(true);
            }
            gameObject.SetActive(true);
        }

        public void Unlock()
        {
            objectPopUp.GetComponent<UnlockAction>().Unlock();
        }

        public void HidePopUp()
        {
            gameObject.SetActive(false);
        }
    }
}
