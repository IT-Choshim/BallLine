using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BallLine
{
    public class OnClickBackGround :OnClickCharacter,UnlockAction  {
        BackGround backgroundData;

        protected override void Awake()
        {
            backgroundData = gameObject.GetComponent<BackGround>();
        }

        protected override void OnEnable()
        {
            if (!backgroundData.IsUnlocked)
            {
                if (CoinManager.Instance.Coins >= backgroundData.price)
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

        new public void OnClickChange()
        {
            if (backgroundData.isFree || backgroundData.IsUnlocked)
            {
                if (ScrollViewController.CurrentSelectBackgr == null || ScrollViewController.CurrentSelectBackgr != gameObject)
                {
                    ChangeOutLine();
                    if (ScrollViewController.CurrentSelectBackgr != null)
                        ScrollViewController.CurrentSelectBackgr.GetComponent<OnClickBackGround>().ResetOutLine(GameManager.Instance.normalShadowColor);
                    ScrollViewController.CurrentSelectBackgr = gameObject;
                    SelectbackGround();
                }
            }
            else
            {
                HandlePopUp(backgroundData.price,GameManager.Instance.backgroundMessage);
            }
        }

        new public void Unlock()
        {
            bool unlockSucceeded = backgroundData.Unlock(false);
            if (unlockSucceeded)
            {
                unlockImage.SetActive(false);
                PopUpController.Instance.HidePopUp();
                SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
                OnClickChange();
            }
        }

        public void SelectbackGround()
        {
            if (BackGroundManager.Instance.CurrentBackGroundIndex != backgroundData.backGroundSequenceNumber)
            {
                int backGroundID = backgroundData.backGroundSequenceNumber;
                GameManager.Instance.playerController.SetBackGround(backGroundID);
            }
            BackGroundManager.Instance.CurrentBackGroundIndex = backgroundData.backGroundSequenceNumber;
        }

        public void ResetOutLine(Color color)
        {
            transform.parent.GetComponent<Image>().color = color;
        }

        new public void ChangeOutLine()
        {
            transform.parent.GetComponent<Image>().color = GameManager.Instance.selectShadowColor;
        }
    }
}
