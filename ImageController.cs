using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
///
/// 위치(사각형 이미지의 무게중심)와 오른쪽 아래 점을 이용해 이미지의 크기와 회전을 조절하는 스크립트
/// 위치의 x, y, 오른쪽 아래의 x, y 총 네 개의 float 만 알면 나머지는 전부 알아서 계산한다
///
/// 레퍼런스 : 카카오톡 프로필 꾸미기
///
/// </summary>


public static class EditState
{
    // 보드 위에 있으면 편집 가능, 아니면 편집 불가능
    public static int OnBoard = 0;
    public static int NotOnBoard = 1;
}


public class ImageController : MonoBehaviour
{
    // 초기화를 위해 저장해두는 값들
    private class SetupValue
    {
        public int state;
        public int id;
        public int index;
        public Vector2 position;
        public Vector2 rightBottom;
    }

    private SetupValue setupValue;
    private Camera _renderCamera;

    #region objects

    [SerializeField]
    private GameObject go_boarder;              // 여기에 editingHandle 이 있다
    [SerializeField]
    private RectTransform rect_targetImage;     // pivot 0.5 0.5
    [SerializeField]
    private RawImage img_image;

    private Vector2 _imageSize = new Vector2(72f, 72f);      // 임의로 잡은 초기 크기
    private int _editState = EditState.NotOnBoard;

    public int orderIndex;
    public int ID;

    #endregion

    /* ============== from server ============== */
    public Vector2 currentPosition;
    public Vector2 rightBottomVertex;
    /* ========================================= */

    // 크기와 회전을 바꾸는 변수들
    private Vector2 _touchPosError;
    private Vector2 _currentRotVector;
    private float _sin;              // 로테와 무관한 이미지 고유 사인값
    private float _cos;              // 로테와 무관한 이미지 고유 코사인값
    private float _thetaInDeg;       // in degree... 모든 이미지의 스케일을 고정한다면 고정 값으로 넣어줄 수 있을 것 같다!! == 삼각비 계산 덜 해도 됨!!

    // 위치를 바꾸는 변수들
    private Vector2 _refTouchPos;

    #region touch input

    public void onPointerDownEditingHandle(Vector2 touchPos)
    {
        _touchPosError = touchPos;
        _touchPosError -= currentPosition;
        _touchPosError -= _currentRotVector;
    }

    public void onDragEditingHandle(Vector2 touchPos)
    {
        rightBottomVertex = touchPos;
        rightBottomVertex -= _touchPosError;
        _currentRotVector = rightBottomVertex - currentPosition;
        setCurrentScaleRotation();
    }

    public void onPointerDownDragging(Vector2 startPos)
    {
        _refTouchPos = startPos;
    }

    public void onDragImage(Vector2 dragPos)
    {
        rect_targetImage.anchoredPosition += dragPos - _refTouchPos;
        rightBottomVertex += dragPos - _refTouchPos;
        currentPosition = rect_targetImage.anchoredPosition;
        _refTouchPos = dragPos;
    }

    #endregion

    public RectTransform getStickerRect()
    {
        return rect_targetImage;
    }

    public Vector2 getStickerSize()
    {
        return rect_targetImage.sizeDelta;
    }

    public int getState()
    {
        return _editState;
    }

    public bool isEditMode()
    {
        return go_boarder.activeSelf;
    }

    public void onClickClose()
    {
        if (_editState == EditState.OnBoard)
            switchMode(EditState.NotOnBoard, null, null);
    }

    /// <summary>
    /// 인벤토리에 있는 이미지를 보드에 올린다
    /// </summary>
    public void onClickInventoryImage()
    {
        if(_editState == EditState.NotOnBoard)
            switchMode(EditState.OnBoard, null, null);
    }

    public void enterEditMode(bool enter)
    {
        go_boarder.SetActive(enter);

        if(enter)
        {
            this.gameObject.transform.SetAsLastSibling();
        }
        else
        {
            this.gameObject.transform.SetSiblingIndex(orderIndex);
        }
    }

    #region set state

    /// <summary>
    /// 처음에 보드에 올라올 때 위치 및 로테이션 세팅 (초깃값 설정)
    /// </summary>
    private void setPositionOnBoard(Vector2 currentPos, Vector2 rightBottom)
    {
        // 엥커 세팅
        rect_targetImage.anchorMax = Vector2.one * 0.5f;
        rect_targetImage.anchorMin = Vector2.one * 0.5f;

        currentPosition = currentPos;
        rightBottomVertex = rightBottom;

        if (_imageSize.y > 0 && _imageSize.x > 0)
        {
            // 원본 이미지를 기준으로, 한 번만 구해 놓으면 되는 값들
            float imageDiameter = getDiameter(_imageSize.x, _imageSize.y);
            _cos = _imageSize.x / imageDiameter;
            _sin = _imageSize.y / imageDiameter;
            _thetaInDeg = getAngleInDeg(_imageSize.y, _imageSize.x);

            // 현재 상태 세팅
            _currentRotVector = rightBottomVertex - currentPosition;
            rect_targetImage.anchoredPosition = currentPosition;
            setCurrentScaleRotation();
        }
        else
            Debug.LogError("image size cannot be less than or equal to zero");
    }

    public void switchMode(int state, Vector2? position, Vector2? rightBottom)
    {
        if(state == EditState.OnBoard)
        {
            // (여기에서 보드 위에 실제로 올리는 처리를 주고,,)

            if(position.HasValue && rightBottom.HasValue)
                setPositionOnBoard(position.Value, rightBottom.Value);
            else
                // 초깃값이 하나라도 없으면 중앙에 회전 0 으로 설정해서 올린다
                setPositionOnBoard(new Vector2(0f, 110f), new Vector2(_imageSize.x * 0.5f, 110f - _imageSize.y * 0.5f));
        }
        else if(state == EditState.NotOnBoard)
        {
            // (여기에서 다시 인벤토리로 내리는 처리를 해 준다)
        }

        _editState = state;
    }

    // 처음에 setup 했던 값으로 초기화
    public void reset()
    {
        if(setupValue.state == EditState.NotOnBoard)
        {
            if (_editState == EditState.OnBoard)
            {
                // 원래 보드에 없었는데 올라갔다,, 다시 내려오자
                switchMode(EditState.NotOnBoard, null, null);
            }
        }
        else if(setupValue.state == EditState.OnBoard)
        {
            if(_editState == EditState.NotOnBoard)
            {
                // 원래 보드에 있었는데 내려갔다,, 올라가자
                switchMode(EditState.OnBoard, setupValue.position, setupValue.rightBottom);
                orderIndex = setupValue.index;
            }
        }
    }

    #endregion

    /// <summary>
    ///
    /// 처음에 생성할 때 한 번만 실행되는 함수
    /// 처음에 보드에 있을지 인벤토리에 있을지, 보드에 있다면 몇 번째 어느 위치에 어느 로테이션을 있을지 설정
    ///
    /// </summary>

    public void setup(int state, int id, int index = 0, float posX = 0, float posY = 0, float rightBottomX = 0, float rightBottomY = 0)
    {
        setupValue = new SetupValue();

        setupValue.id = ID = id;
        setupValue.state = _editState = state;

        if (state == EditState.OnBoard)
        {
            setupValue.index = orderIndex = index;

            // (보드에 올리는 설정 하기)

            Vector2 pos = new Vector2(posX, posY);
            Vector2 rightBottom = new Vector2(rightBottomX, rightBottomY);
            setPositionOnBoard(pos, rightBottom);
            setupValue.position = pos;
            setupValue.rightBottom = rightBottom;
        }
        else if (state == EditState.NotOnBoard)
        {
            // (인벤토리에 넣어주는 설정 하기)

            go_boarder.gameObject.SetActive(false);
        }
    }

    #region calculation

    private float getDiameter(float x, float y)
    {
        return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
    }

    private float getAngleInDeg(float y, float x)
    {
        return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
    }

    private void setCurrentScaleRotation()
    {
        float diameter = 2f * getDiameter(_currentRotVector.x, _currentRotVector.y);
        rect_targetImage.sizeDelta = new Vector2(diameter * _cos, diameter * _sin);
        rect_targetImage.rotation = Quaternion.AngleAxis(getAngleInDeg(_currentRotVector.y, _currentRotVector.x) + _thetaInDeg, Vector3.forward);
    }

    #endregion
}
