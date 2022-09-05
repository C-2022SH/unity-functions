using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Festa.Client.Module;
using UnityEngine.UI;
using Festa.Client;
using UnityEngine.EventSystems;

public static class StickerState
{
    public static int OnBoard = 0;
    public static int NotOnBoard = 1;
}

public class StickerSizeController : MonoBehaviour
{
    /// <summary>
    /// 변경 값을 저장하지 않는 경우를 대비해
    /// 초기 설정 값을 저장하는 클래스
    /// </summary>
    private class SetupValue
    {
        public int state;
        public int id;
        public int index;
        public Vector2 position;
        public Vector2 rightBottom;
    }

    private SetupValue _setupValue;

    #region objects

    [SerializeField]
    private GameObject go_boarder;
    [SerializeField]
    private RectTransform rect_targetImage;     // pivot 0.5 0.5
    [SerializeField]
    private RawImage img_image;

    private RectTransform rect_root;
    private RectTransform rect_bottomsheetContent;
    private Vector2 _imageSize = new Vector2(72f, 72f);
    private int _stickerState = StickerState.NotOnBoard;

    public int orderIndex;
    public int ID;

    #endregion

    /* ============== from server ============== */
    public Vector2 currentPosition;
    public Vector2 rightBottomVertex;
    /* ========================================= */

    // 크기 바꾸고 회전 바꾸고~~
    private Vector2 _touchPosError;
    private Vector2 _currentRotVector;
    private float _sin;              // 로테와 무관한 이미지 고유 사인값
    private float _cos;              // 로테와 무관한 이미지 고유 코사인값
    private float _thetaInDeg;       // in degree... 모든 이미지의 스케일을 고정한다면 고정 값으로 넣어줄 수 있을 것 같다!! == 삼각비 계산 덜 해도 됨!!

    // 위치 바꾸고~~
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
        return _stickerState;
    }

    public bool isEditMode()
    {
        return go_boarder.activeSelf;
    }

    public void onClickClose()
    {
        if (_stickerState == StickerState.OnBoard)
            switchMode(StickerState.NotOnBoard);
    }

    public void onClickWaitingSticker()
    {
        if (_stickerState == StickerState.NotOnBoard)
            switchMode(StickerState.OnBoard);
    }

    public void enterEditMode(bool enter)
    {
        go_boarder.SetActive(enter);

        if (enter)
        {
            this.gameObject.transform.SetAsLastSibling();
        }
        else
        {
            this.gameObject.transform.SetSiblingIndex(orderIndex);
        }
    }

    #region set state

    private void setPositionOnBoard(Vector2 currentPos, Vector2 rightBottom)
    {
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
            Debug.LogError("image size is less than or equal to zero");
    }

    public void switchMode(int state)
    {
        if (state == StickerState.OnBoard)
        {
            UIProfileBoard.getInstance().boardStickers.Add(this);
            UIProfileBoard.getInstance().orderStickers();

            this.gameObject.transform.SetParent(rect_root);
            setPositionOnBoard(new Vector2(0f, 110f), new Vector2(_imageSize.x * 0.5f, 110f - _imageSize.y * 0.5f));
            UIProfileBoard.getInstance().EditPanel[1].swipePanel(false);
            //enterEditMode(true);
        }
        else if (state == StickerState.NotOnBoard)
        {
            UIProfileBoard.getInstance().boardStickers.Remove(this);
            this.gameObject.transform.SetParent(rect_bottomsheetContent);
            go_boarder.gameObject.SetActive(false);
        }

        _stickerState = state;
    }

    // 처음에 setup 했던 값으로 초기화
    public void reset()
    {
        if (_setupValue.state == StickerState.NotOnBoard)
        {
            if (_stickerState == StickerState.NotOnBoard)
            {
                // 잘 있네!
                return;
            }
            else if (_stickerState == StickerState.OnBoard)
            {
                // 원래 보드에 없었는데 올라감,, 다시 내려오자
                switchMode(StickerState.NotOnBoard);
            }
        }
        else if (_setupValue.state == StickerState.OnBoard)
        {
            if (_stickerState == StickerState.OnBoard)
            {

            }
            else if (_stickerState == StickerState.NotOnBoard)
            {
                this.gameObject.transform.SetParent(rect_root);
                _stickerState = _setupValue.state;
                UIProfileBoard.getInstance().boardStickers.Add(this);
            }

            orderIndex = _setupValue.index;
            setPositionOnBoard(_setupValue.position, _setupValue.rightBottom);
        }
    }

    #endregion

    // 테스트용 임시 함수
    public void setImage(Texture tex)
    {
        img_image.texture = tex;
    }

    // 처음에 생성할 때 한 번만 실행되는 함수!!
    public void setup(int state, int id, int index = 0, float posX = 0, float posY = 0, float rightBottomX = 0, float rightBottomY = 0, RectTransform root = null)
    {
        _setupValue = new SetupValue();
        go_boarder.gameObject.SetActive(false);
        if (root == null)
            rect_root = UIProfileBoard.getInstance().rect_StickerRoot;
        else
            rect_root = root;

        rect_bottomsheetContent = UIProfileBoard.getInstance().rect_bottomsheetContent;
        _setupValue.id = ID = id;

        if (state == StickerState.OnBoard)
        {
            _setupValue.index = orderIndex = index;
            if (this.transform.parent != rect_root)
                this.transform.SetParent(rect_root);


            Vector2 pos = new Vector2(posX, posY);
            Vector2 rightBottom = new Vector2(rightBottomX, rightBottomY);
            setPositionOnBoard(pos, rightBottom);
            _setupValue.position = pos;
            _setupValue.rightBottom = rightBottom;
        }
        else if (state == StickerState.NotOnBoard)
        {
            if (this.transform.parent != rect_bottomsheetContent)
                this.gameObject.transform.SetParent(rect_bottomsheetContent);

            go_boarder.gameObject.SetActive(false);
        }

        _setupValue.state = _stickerState = state;
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