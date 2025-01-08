using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Watermelon
{
    public class UnlockCanvas : MonoBehaviour
    {
        [SerializeField] Canvas canvas;

        [Space]
        [SerializeField] CanvasGroup countdownFade;
        [SerializeField] TMP_Text countdownText;

        [Space]
        [SerializeField] CanvasGroup unlockButtonFade;
        [SerializeField] Button unlockButton;

        public bool IsVisible { get => canvas.enabled = true; set => canvas.enabled = value; }

        public bool IsCountdownVisible { get; private set; } = false;
        public bool IsButtonVisible { get; private set; }

        public void Init(UnityAction onButtonClicked)
        {
            unlockButton.onClick.AddListener(onButtonClicked);
        }

        public void UpdateCountdown(TimeSpan timeSpan)
        {
            if(timeSpan.TotalHours > 1)
            {
                countdownText.text = string.Format("{0}h:{1:00}m", timeSpan.Hours, timeSpan.Minutes);
            } else if(timeSpan.TotalMinutes > 1)
            {
                countdownText.text = string.Format("{0}m:{1:00}s", timeSpan.Minutes, timeSpan.Seconds);
            } else
            {
                countdownText.text = string.Format("0m:{0:00}s", timeSpan.Seconds);
            }
        }

        #region Show Countdown

        public void ShowCountdown(bool instantly = false)
        {
            if (instantly)
            {
                ShowCountdownInstantly();
            }
            else
            {
                ShowCountdownAnimated();
            }
        }

        private void ShowCountdownInstantly()
        {
            if (IsButtonVisible)
            {
                unlockButtonFade.alpha = 0;
                unlockButtonFade.gameObject.SetActive(false);

                IsButtonVisible = false;
            }

            if (!IsCountdownVisible)
            {
                countdownFade.gameObject.SetActive(true);
                countdownFade.alpha = 1;

                IsCountdownVisible = true;
            }
        }

        private void ShowCountdownAnimated()
        {
            if (IsButtonVisible)
            {
                unlockButtonFade.DOFade(0, 0.2f).OnComplete(() =>
                {
                    unlockButtonFade.gameObject.SetActive(false);

                    IsButtonVisible = false;

                    ShowCountdownAnimation();
                });
            }
            else
            {
                ShowCountdownAnimation();
            }
        }

        private void ShowCountdownAnimation()
        {
            if (!IsCountdownVisible)
            {
                countdownFade.gameObject.SetActive(true);

                countdownFade.alpha = 0;
                countdownFade.DOFade(1, 0.2f);

                IsCountdownVisible = true;
            }
        }

        #endregion

        #region ShowButton

        public void ShowButton(bool instantly = false)
        {
            if (instantly)
            {
                ShowButtonInstantly();
            } else
            {
                ShowButtonAnimated();
            }
        }

        private void ShowButtonInstantly()
        {
            if (IsCountdownVisible)
            {
                countdownFade.alpha = 0;
                countdownFade.gameObject.SetActive(false);

                IsCountdownVisible = false;
            }

            if (!IsButtonVisible)
            {
                unlockButtonFade.gameObject.SetActive(true);
                unlockButtonFade.alpha = 1;

                IsButtonVisible = true;
            }
        }

        private void ShowButtonAnimated()
        {
            if (IsCountdownVisible)
            {
                countdownFade.DOFade(0, 0.2f).OnComplete(() =>
                {
                    countdownFade.gameObject.SetActive(false);

                    IsCountdownVisible = false;

                    ShowButtonAnimation();
                });
            }
            else
            {
                ShowButtonAnimation();
            }
        }

        private void ShowButtonAnimation()
        {
            if (!IsButtonVisible)
            {
                unlockButtonFade.gameObject.SetActive(true);

                unlockButtonFade.alpha = 0;
                unlockButtonFade.DOFade(1, 0.2f);

                IsButtonVisible = true;
            }
        }

        #endregion
    }
}