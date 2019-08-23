using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BallLine
{
    public class OnClickLevel : OnClickBackGround,UnlockAction
    {
        public Level levelData;

        protected override void OnEnable()
        {
            if (levelData != null && !levelData.IsUnlocked)
            {
                if (CoinManager.Instance.Coins >= levelData.price)
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
            if (levelData.isFree || levelData.IsUnlocked)
            {
                if (ScrollViewController.CurrentSelectLevel == null || ScrollViewController.CurrentSelectLevel != gameObject)
                {
                    ChangeOutLine();
                    if (ScrollViewController.CurrentSelectLevel != null)
                        ScrollViewController.CurrentSelectLevel.GetComponent<OnClickLevel>().ResetOutLine(GameManager.Instance.normalShadowColor);
                    ScrollViewController.CurrentSelectLevel = gameObject;
                    SelectLevel();
                }
            }
            else
            {
                HandlePopUp(levelData.price,GameManager.Instance.levelMessage);
            }
        }

        new public void Unlock()
        {
            bool unlockSucceeded = levelData.Unlock();
            if (unlockSucceeded)
            {
                GameObject unlockImage = transform.GetChild(1).gameObject;
                unlockImage.SetActive(false);
                PopUpController.Instance.HidePopUp();
                SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
                OnClickChange();
            }
        }

        public void SelectLevel()
        {
            if (LevelManager.Instance.CurrentLevelIndex != levelData.levelSequenceNumber)
            {
                int levelID = levelData.levelSequenceNumber;
                GameManager.Instance.playerController.instantiateNewLevel(levelID);
            }
            LevelManager.Instance.CurrentLevelIndex = levelData.levelSequenceNumber;
        }
    }
}
