using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BallLine
{
    public class OnClickCharacter : MonoBehaviour, UnlockAction
    {
        [Header("Reference Objects")]
        [SerializeField]
        protected GameObject lockImage;

        [SerializeField]
        protected GameObject unlockImage;

        protected Outline outLine;

        Character characterData;

        protected virtual void Awake()
        {
            characterData = GetComponent<Character>();
            outLine = GetComponent<Outline>();
        }

        protected virtual void OnEnable()
        {
            if (!characterData.IsUnlocked)
            {
                if (CoinManager.Instance.Coins >= characterData.price)
                {
                    unlockImage.SetActive(true);
                    lockImage.SetActive(false);
                }
                else
                {
                    unlockImage.SetActive(false);
                    lockImage.SetActive(true);
                }
            }
        }

        public void OnClickChange()
        {
            if (characterData.isFree || characterData.IsUnlocked)
            {
                if (ScrollViewController.CurrentSelectChar == null || ScrollViewController.CurrentSelectChar != gameObject)
                {
                    ChangeOutLine();
                    if (ScrollViewController.CurrentSelectChar != null)
                        ScrollViewController.CurrentSelectChar.GetComponent<OnClickCharacter>().ResetOutLine(GameManager.Instance.normalShadowColor, new Vector2(5, -5));
                    ScrollViewController.CurrentSelectChar = gameObject;
                    SelectCharacter();
                }
            }
            else
            {
                HandlePopUp(characterData.price, GameManager.Instance.characterMessage);
            }
        }

        public void Unlock()
        {
            bool unlockSucceeded = characterData.Unlock();
            if (unlockSucceeded)
            {
                unlockImage.SetActive(false);
                PopUpController.Instance.HidePopUp();
                SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
                OnClickChange();
            }
        }

        public void SelectCharacter()
        {
            if (CharacterManager.Instance.CurrentCharacterIndex != characterData.characterSequenceNumber)
            {
                int characterID = characterData.characterSequenceNumber;
                ObjectPooling.SharedInstance.DestroyPoolObject();
                ObjectPooling.SharedInstance.PoolingObject(characterID);
            }
            CharacterManager.Instance.CurrentCharacterIndex = characterData.characterSequenceNumber;
        }

        public void HandlePopUp(int price, string messageUnlock = null, string messageLock = null)
        {
            if (CoinManager.Instance.Coins >= price)
            {
                PopUpController.Instance.SetMassage("You want to buy this model?");
                if (messageUnlock != null)
                    PopUpController.Instance.SetMassage(messageUnlock);
                PopUpController.Instance.SetPrice(price);
                PopUpController.Instance.objectPopUp = gameObject;
                PopUpController.Instance.ShowPopUp(true);
            }
            else
            {
                PopUpController.Instance.SetMassage("You don't have enough coin");
                if (messageLock != null)
                    PopUpController.Instance.SetMassage(messageLock);
                PopUpController.Instance.SetPrice(price);
                PopUpController.Instance.ShowPopUp(false);
            }
        }

        public void ResetOutLine(Color color, Vector2 cell)
        {
            outLine.effectColor = color;
            outLine.effectDistance = cell;
        }

        public void ChangeOutLine()
        {
            outLine.effectColor = GameManager.Instance.selectShadowColor;
            outLine.effectDistance = new Vector2(5, -5);
        }
    }
}
