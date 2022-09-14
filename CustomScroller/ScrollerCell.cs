using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ScrollerCell : MonoBehaviour
{
    [SerializeField]
    private RectTransform rect_panel;
    [SerializeField]
    private RectTransform rect_icon;
    [SerializeField]
    private RectTransform rect_text;
    [SerializeField]
    private GameObject go_highlight;

    private bool _highlightOn = false;
    private float _animationDuration = 0.3f;

    private Vector2 _panelNormalSize = new Vector2(184f, 232f);
    private Vector2 _panelPointedSize = new Vector2(202f, 256f);

    private Vector2 _iconNormalSize = new Vector2(88f, 88f);
    private Vector2 _iconPointedSize = new Vector2(100f, 100f);

    private Vector2 _textPositionDiff = new Vector2(0f, 4f);

    /// <summary>
    /// 선택된 셀 애니메이션 처리 (셀도 커지고 글씨도 커지고 이미지도 커진다~!)
    /// </summary>
    public void pointOn()
    {
        _highlightOn = true;
        DOTween.To(() => rect_panel.sizeDelta, x => rect_panel.sizeDelta = x, _panelPointedSize, _animationDuration);
        DOTween.To(() => rect_icon.sizeDelta, x => rect_icon.sizeDelta = x, _iconPointedSize, _animationDuration);
        DOTween.To(() => rect_icon.gameObject.GetComponent<Image>().color, x => rect_icon.gameObject.GetComponent<Image>().color = x, ColorChart.primary_300, _animationDuration);
        DOTween.To(() => rect_text.anchoredPosition, x => rect_text.anchoredPosition = x, rect_text.anchoredPosition - _textPositionDiff, _animationDuration);

        // 선택되었다면 배경 하이라이트
        Invoke("highlightOn", _animationDuration);
    }

    private void highlightOn()
    {
        if(_highlightOn)
        {
            go_highlight.SetActive(true);
        }
    }

    /// <summary>
    /// 애니메이션 없이 바로 선택된 상태로 전환
    /// </summary>
    public void pointOnImmediately()
    {
        _highlightOn = true;
        go_highlight.SetActive(true);

        rect_panel.sizeDelta = _panelPointedSize;
        rect_icon.sizeDelta = _iconPointedSize;
        rect_icon.GetComponent<Image>().color = ColorChart.primary_300;
        rect_text.anchoredPosition -= _textPositionDiff;
    }

    /// <summary>
    /// 선택 풀린 셀 일반 상태로 돌아가는 처리
    /// </summary>
    public void pointOff()
    {
        _highlightOn = false;
        go_highlight.SetActive(false);

        DOTween.To(() => rect_icon.sizeDelta, x => rect_icon.sizeDelta = x, _iconPointedSize, _animationDuration);
        DOTween.To(() => rect_icon.gameObject.GetComponent<Image>().color, x => rect_icon.gameObject.GetComponent<Image>().color = x, ColorChart.gray_400, _animationDuration);
        DOTween.To(() => rect_panel.sizeDelta, x => rect_panel.sizeDelta = x, _panelNormalSize, _animationDuration);
        DOTween.To(() => rect_text.anchoredPosition, x => rect_text.anchoredPosition = x, rect_text.anchoredPosition + _textPositionDiff, _animationDuration);

    }

    /// <summary>
    /// 애니메이션 없이 바로 일반 상태로 전환
    /// </summary>
    public void pointOffImmediately()
    {
        _highlightOn = false;
        go_highlight.SetActive(false);

        rect_panel.sizeDelta = _panelNormalSize;
        rect_icon.sizeDelta = _iconNormalSize;
        rect_icon.gameObject.GetComponent<Image>().color = ColorChart.gray_400;
        rect_text.anchoredPosition += _textPositionDiff;
    }
}
