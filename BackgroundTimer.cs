using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Festa.Client
{
    public class ResendCodeTimer : MonoBehaviour
    {
        [SerializeField]
        private Button btn_resend;
        [SerializeField]
        private TMP_Text txt_resendCode;
        [SerializeField]
        private Image img_underline;

        private int _countSec;

        private DateTime _targetTime;
        private int _timeLeft;
        private bool _isCounting;

        public void setCountTime(int time)
        {
            _countSec = time;
        }

        public void turnOnTimer()
        {
            _targetTime = DateTime.Now.AddSeconds(_countSec);
            _isCounting = true;

            txt_resendCode.color = ColorChart.gray_400;
            img_underline.color = ColorChart.gray_400;

            btn_resend.interactable = false;
        }

        public bool isCounting()
        {
            return _isCounting;
        }

        public int getTimeLeft()
        {
            return _timeLeft;
        }

        private void Update()
        {
            if (_isCounting)
            {
                TimeSpan timeDiff = _targetTime - DateTime.Now;
                int diffSec = timeDiff.Seconds;

                if (diffSec > 0)
                {
                    _timeLeft = diffSec;
                }
                else
                {
                    _timeLeft = 0;
                    _isCounting = false;

                    txt_resendCode.color = ColorChart.gray_700;
                    img_underline.color = ColorChart.gray_700;

                    btn_resend.interactable = true;
                }
            }
        }
    }
}
