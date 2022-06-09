using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DeleteScrollTest : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // pivot 0.5, 0.5

    [SerializeField]
    private RectTransform rect_scrollView;
    [SerializeField]
    private RectTransform rect_content;
    [SerializeField]
    private RectTransform rect_trash;
    [SerializeField]
    private Image backgroundColor;
    [SerializeField]
    private float dragToDeleteThreshold;
    [SerializeField]
    private float showDeleteButtonThreshold;

    private bool _allowDragging;
    private bool _isDragging;
    private float _beginX;
    private float _prevContentX = 32f;

    public bool isInsideRect(PointerEventData pos)
    {
        if (pos.position.y - Screen.height * 0.5f >= rect_scrollView.anchoredPosition.y - rect_scrollView.rect.height * 0.5f
            && pos.position.y - Screen.height * 0.5f <= rect_scrollView.anchoredPosition.y + rect_scrollView.rect.height * 0.5f
            && pos.position.x - Screen.width * 0.5f >= rect_scrollView.anchoredPosition.x - rect_scrollView.rect.width * 0.5f
            && pos.position.x - Screen.width * 0.5f <= rect_scrollView.anchoredPosition.x + rect_scrollView.rect.width * 0.5f)
            return true;
        else
            return false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isInsideRect(eventData))
        {
            if (_allowDragging)
            {
                _isDragging = true;
                _beginX = eventData.position.x;
                Debug.Log("dragging started");
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging && _allowDragging)
        {
            Vector2 contentPos = rect_content.anchoredPosition;
            contentPos.x = eventData.position.x - _beginX + _prevContentX;
            rect_content.anchoredPosition = contentPos;

            Debug.Log("on dragging");
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDragging && _allowDragging)
        {
            if (rect_content.anchoredPosition.x <= dragToDeleteThreshold)
            {
                // 밀어서 삭제
                setContentPos(-407f, 0.4f);         // 여기 나중에 계산해서 숫자 넣자~~
                Debug.Log("item deleted");
            }
            else if (rect_content.anchoredPosition.x <= showDeleteButtonThreshold)
            {
                // 삭제버튼 표시
                setContentPos(-rect_trash.rect.width, 0.2f);
                _prevContentX = -rect_trash.rect.width;
            }
            else
            {
                // 원상복귀
                setContentPos(0f, 0.3f);
                _prevContentX = 0f;
            }

            _isDragging = false;
            Debug.Log("dragging finished");
        }
    }

    private void setContentPos(float desX, float duration = 0f)
    {
        DOTween.To(() => rect_content.anchoredPosition, x => rect_content.anchoredPosition = x, new Vector2(desX, 0f), duration);
    }

    private void Start()
    {
        setContentPos(rect_trash.rect.width);
    }

    private void Update()
    {
        float currentContentRectX = rect_content.anchoredPosition.x;
        // content 위치 세팅
        if (currentContentRectX > 0f)
        {
            // 오른쪽으로 스와이프 == 안됨!
            _allowDragging = false;
            setContentPos(0f);
        }
        else
        {
            _allowDragging = true;
        }

        // 휴지통!
        if (rect_content.anchoredPosition.x <= -64.5f && rect_content.anchoredPosition.x >= -222.5f)
        {
            float moveDelta = (rect_trash.rect.width + rect_content.rect.width) * 0.5f + rect_content.anchoredPosition.x;
            rect_trash.anchoredPosition = new Vector2(moveDelta, 0f);
        }

        // 배경색
        // 일단 리니어하게 바뀌는 걸로
        if (currentContentRectX <= 0f && currentContentRectX >= -rect_content.rect.width)
        {
            // 점점 선명
            Color back = backgroundColor.color;
            back.a = -currentContentRectX / rect_trash.rect.width;
            backgroundColor.color = back;
        }

        if (currentContentRectX <= -rect_content.rect.width * 0.5f)
        {
            // 점점 투명
            Color back = backgroundColor.color;
            back.a = (1f + (currentContentRectX / rect_content.rect.width)) * 2f;
            backgroundColor.color = back;
        }
    }
}
