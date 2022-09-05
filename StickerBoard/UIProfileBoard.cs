using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Festa.Client.Module.UI;
using Festa.Client.ViewModel;
using Festa.Client.Module;
using Festa.Client.RefData;
using Festa.Client.Module.Net;
using UnityEngine.UI;
using DG.Tweening;
using Festa.Client.Module.MsgPack;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Festa.Client
{
    public class UIProfileBoard : UISingletonPanel<UIProfileBoard>
    {
        public static class EditMode
        {
            public static int view = 0;
            public static int edit = 1;
        }
        private int _editMode = EditMode.view;

        // 스티커의 기준
        public RectTransform rect_StickerRoot;
        public RectTransform rect_bottomsheetContent;

        [SerializeField]
        private CanvasGroup can_stickerBoard;
        [SerializeField]
        private GameObject go_editButton;
        [SerializeField]
        private GameObject go_blankPageLowerText;
        [SerializeField]
        private TMP_Text txt_boardTitle;
        [SerializeField]
        private GameObject stickerPrefab;
        [SerializeField]
        private GameObject go_blankPage;
        [SerializeField]
        private CanvasGroup can_defaultSheet;
        [SerializeField]
        private SwipeDownPanel[] _editSheets;   // 수정모드의 위아래 시트; 0 위 1 아래
        public SwipeDownPanel[] EditPanel
        {
            get { return _editSheets; }
        }

        public Sprite[] testStickers;   // 테스트용 임시
        public int[] testIDs = { 512, 513, 514, 515, 901, 902, 903, 402, 403, 404, 405, 406, 407, 408, 409 };

        public List<StickerSizeController> allStickers = new List<StickerSizeController>();         // 가지고 있는 모든 스티커들
        public List<StickerSizeController> boardStickers = new List<StickerSizeController>();       // 지금 눈에 보이는 보드 위 스티커들

        private StickerSizeController _pickedSticker = null;
        private bool _draggingSticker = false;
        private bool _rotatingSticker = false;

#if UNITY_EDITOR
        private InputModule_PC _inputModule;
#else
        private InputModule_Mobile _inputModule;
#endif

        //private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        private ClientProfileCache _profileCache;
        private ClientNetwork Network => ClientMain.instance.getNetwork();

        private int JSONDATA_VERSION = 1;

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

#if UNITY_EDITOR
            _inputModule = InputModule_PC.create();
#else
            _inputModule = InputModule_Mobile.create();
#endif
        }

        public void setClientProfile(ClientProfileCache cache, bool isMine)
        {
            _profileCache = cache;
            go_editButton.SetActive(isMine);
            go_blankPageLowerText.SetActive(isMine);
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
        {
            base.open(param, transitionType, closeType);

            // 기본 모드
            resetEditMode();

            setStickerBoard();

            // 스티커 없다면!
            if (boardStickers.Count == 0)
                go_blankPage.SetActive(true);
            else
                go_blankPage.SetActive(false);

            txt_boardTitle.text = GlobalRefDataContainer.getStringCollection().getFormat("board.title", 0, _profileCache.Profile.name);
        }

        public override void update()
        {
            base.update();

            if (_editMode == EditMode.edit && _inputModule.isTouchDown())
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(_editSheets[1].getTargetPanelRect(), _inputModule.getTouchPosition(), Camera.main))
                    return;

                // 혹시 조절기를 눌렀니??
                if (_pickedSticker != null && _pickedSticker.isEditMode())
                {
                    if (isControllerClicked(_pickedSticker, _inputModule.getTouchPosition()))
                    {
                        // 크기 조절
                        _rotatingSticker = true;
                        Vector2 touchPosition;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_StickerRoot, _inputModule.getTouchPosition(), Camera.main, out touchPosition);
                        touchPosition += new Vector2(375f * 0.5f, 812f * 0.5f);
                        _pickedSticker.onPointerDownEditingHandle(touchPosition);
                    }
                }

                // 스티커를 눌렀구나!!
                if (_rotatingSticker == false)
                {
                    var currentPicked = getPickedSticker(_inputModule.getTouchPosition());
                    if (currentPicked == null || currentPicked.getState() == StickerState.NotOnBoard)
                    {
                        // 아니야ㅜ
                        if (_pickedSticker != null)
                        {
                            _pickedSticker.enterEditMode(false);
                            _pickedSticker = null;
                        }
                        return;
                    }

                    if (_pickedSticker != null)
                    {
                        if (_pickedSticker != currentPicked)
                        {
                            // 이번에 고른 애가 전에 고른 애랑 달라
                            if (_pickedSticker != null && _pickedSticker.isEditMode())
                                _pickedSticker.enterEditMode(false);
                        }
                    }

                    currentPicked.enterEditMode(true);
                    _pickedSticker = currentPicked;

                    // 드래그
                    _draggingSticker = true;
                    Vector2 dragPosition;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_StickerRoot, _inputModule.getTouchPosition(), Camera.main, out dragPosition);
                    _pickedSticker.onPointerDownDragging(dragPosition);
                    return;
                }
            }
            else if (_inputModule.isTouchDrag() && _pickedSticker != null)
            {
                Vector2 dragPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_StickerRoot, _inputModule.getTouchPosition(), Camera.main, out dragPosition);

                if (_draggingSticker == true)
                {
                    // 드래그
                    _pickedSticker.onDragImage(dragPosition);
                }
                else if(_rotatingSticker == true)
                {
                    // rotate
                    dragPosition += new Vector2(375f * 0.5f, 812f * 0.5f);
                    _pickedSticker.onDragEditingHandle(dragPosition);
                }
            }
            else if (_inputModule.isTouchUp())
            {
                _draggingSticker = false;
                _rotatingSticker = false;
            }
        }

        private StickerSizeController getPickedSticker(Vector2 touchPos)
        {
            for (int i = boardStickers.Count - 1; i >= 0; --i)
            {
                var sticker = boardStickers[i];
                if (RectTransformUtility.RectangleContainsScreenPoint(sticker.getStickerRect(), touchPos, Camera.main))
                    return sticker;
            }
/*            foreach (StickerSizeController sticker in boardChildren)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(sticker.getStickerRect(), touchPos, Camera.main))
                    return sticker;
            }*/

            foreach (StickerSizeController sticker in allStickers)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(sticker.getStickerRect(), touchPos, Camera.main))
                    return sticker;
            }

            return null;
        }

        private bool isControllerClicked(StickerSizeController sticker, Vector2 touchPos)
        {
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_StickerRoot, touchPos, Camera.main, out localPosition);

            float distance = (sticker.rightBottomVertex - localPosition).magnitude;
            return distance <= 18.0f;

            //Vector2 targetPosition;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_StickerRoot, sticker.rightBottomVertex, Camera.main, out targetPosition);
            //targetPosition += new Vector2(375f * 0.5f, 812f * 0.5f);

            //// 적당히 가로세로 36 이라고 하고
            //Vector2 halfControllerSize = new Vector2(36f, 36f) * 0.5f;
            //if (touchPosition.x > targetPosition.x - halfControllerSize.x
            //    && touchPosition.x < targetPosition.x + halfControllerSize.x
            //    && touchPosition.y > targetPosition.y - halfControllerSize.y
            //    && touchPosition.y < targetPosition.y + halfControllerSize.y)
            //    return true;

            //return false;
        }

        private void setStickerBoard()
        {
            // 2022.08.11 이강희 스티커가 아직 구성되지 않는 유저가 있음
            if( _profileCache.Sticker.StickerBoard == null)
			{
                return;
			}

            // 스티커 초기 세팅
            // 일단 테스트용,, 다 긁어오기

            var allStickerMap = GlobalRefDataContainer.getInstance().getMap<RefSticker>();

            // 2022.07.27 저장된 데이터에서 스티커 보드 구성
            // 일단 저장된 애들 올리기
            List<int> savedIDs = buildboardStickers(_profileCache.Sticker.StickerBoard.getJsonData());

            allStickers.Clear();

            // 테스트용 임시
            int count = 0;

            foreach(var sticker in allStickerMap)
            {
                var value = sticker.Value as RefSticker;

                // 보드에 있는 애는 패스!
                if (savedIDs.Contains(value.id))
                {
                    // === 임시 ===

                    if(count < testStickers.Length)
                    {
                        int stickerIndex = savedIDs.IndexOf(value.id);
                        boardStickers[stickerIndex].setImage(testStickers[count].texture);
                        count++;
                    }

                    // ============
                    continue;
                }

                StickerSizeController controller;
                if (allStickers.Count < rect_bottomsheetContent.childCount)
                {
                    // 이미 있는 건 다시 안 만든당
                    controller = rect_bottomsheetContent.transform.GetChild(allStickers.Count).GetComponent<StickerSizeController>();
                    controller.gameObject.SetActive(true);
                }
                else
                {
                    GameObject stickerObj = Instantiate(stickerPrefab, rect_StickerRoot);
                    controller = stickerObj.GetComponent<StickerSizeController>();
                }

                controller.setup(StickerState.NotOnBoard, value.id);

                // === 임시 ===

                if (count < testStickers.Length)
                {
                    controller.setImage(testStickers[count].texture);
                    count++;
                }

                // ============

                allStickers.Add(controller);
            }

            // 혹시 컨트롤러가 남아 있다면 비활
            for (int j = allStickers.Count; j < rect_bottomsheetContent.childCount; ++j)
            {
                rect_bottomsheetContent.GetChild(j).gameObject.SetActive(false);
            }
        }

        private void resetEditMode()
        {
            can_defaultSheet.alpha = 1f;
            _editSheets[0].swipePanel(false);
            _editSheets[1].swipePanel(false);
        }

        public void orderStickers()
        {
            for (int i = 0; i < boardStickers.Count; i++)
            {
                boardStickers[i].orderIndex = i;
            }
        }

        public void switchEditMode(int mode)
        {
            if (mode == EditMode.view)
            {
                // 스티커 없다면!
                if (boardStickers == null)
                    go_blankPage.SetActive(true);
                else
                    go_blankPage.SetActive(false);

                // 기본 모드
                showHideDefaultPanel(true);
                _editSheets[0].swipePanel(false);
                _editSheets[1].swipePanel(false);
            }
            else if (mode == EditMode.edit)
            {
                // 수정 모드
                go_blankPage.SetActive(false);
                showHideDefaultPanel(false);
                _editSheets[0].swipePanel(true);
                _editSheets[1].swipePanel(true);
            }

            _editMode = mode;
        }

        public void onClickOpenEdit()
        {
            switchEditMode(EditMode.edit);
        }

        public void onClickCanelEdit()
        {
            foreach (StickerSizeController controller in allStickers)
            {
                controller.reset();
            }

            // 혹시 너무 적으면 다 안돌아가니까,, 전부 위치 잡은 다음에 돌리기
            foreach(StickerSizeController sticker in boardStickers)
            {
                sticker.gameObject.transform.SetSiblingIndex(sticker.orderIndex);
            }

            switchEditMode(EditMode.view);
        }

        public void onClickSaveEdit()
        {
            JsonObject jsonData = buildSaveData();

            // 내 프로필에 저장 (로컬)
            _profileCache.Sticker.StickerBoard.setData(jsonData);

            // 서버에 저장 (응답을 기다릴 필요는 없어 보이는데..)
            saveStickerToServer(_profileCache.Sticker.StickerBoard.data);

            switchEditMode(EditMode.view);
        }

        private JsonObject buildSaveData()
        {
            JsonObject stickersJson = new JsonObject();
            stickersJson.put("version", JSONDATA_VERSION); // 나중을 위해

            // 순서를 보장하기 위해 JsonArray사용
            JsonArray stickerList = new JsonArray();
            stickersJson.put("list", stickerList);

            for (int i = 0; i < boardStickers.Count; ++i)
            {
                JsonObject sticker = new JsonObject();

                // 1: 이미지 스티커   2 : 텍스트 ??
                sticker.put("type", 1);
                sticker.put("id", boardStickers[i].ID); // RefSticker.ID

                JsonArray vertices = new JsonArray();
                vertices.add((double)boardStickers[i].currentPosition.x);
                vertices.add((double)boardStickers[i].currentPosition.y);
                vertices.add((double)boardStickers[i].rightBottomVertex.x);
                vertices.add((double)boardStickers[i].rightBottomVertex.y);

                sticker.put("vertices", vertices);

                stickerList.add(sticker);
            }

#if UNITY_EDITOR
            Debug.Log(stickersJson.encode());
#endif

            return stickersJson;
        }

        private List<int> buildboardStickers(JsonObject jsonData)
        {
#if UNITY_EDITOR
            Debug.Log(jsonData.encode());
#endif
            boardStickers.Clear();
            List<int> stickersOnBoard = new List<int>();

            int version = jsonData.getInteger("version");
            JsonArray stickerList = jsonData.getJsonArray("list");

            int currentStickers = rect_StickerRoot.transform.childCount;

            for (int i = 0; i < stickerList.size(); ++i)
            {
                JsonObject sticker = stickerList.getJsonObject(i);

                int type = sticker.getInteger("type");
                int id = sticker.getInteger("id");

                JsonArray vertices = sticker.getJsonArray("vertices");
                float position_x = (float)vertices.getDouble(0);
                float position_y = (float)vertices.getDouble(1);
                float rightBottomVertex_x = (float)vertices.getDouble(2);
                float rightBottomVertex_y = (float)vertices.getDouble(3);

                // 만들기!!
                StickerSizeController controller;
                if (i < currentStickers)
                {
                    // 이미 있는 건 다시 안 만든당
                    controller = rect_StickerRoot.transform.GetChild(i).GetComponent<StickerSizeController>();
                    controller.gameObject.SetActive(true);
                }
                else
                {
                    GameObject stickerObj = Instantiate(stickerPrefab, rect_StickerRoot);
                    controller = stickerObj.GetComponent<StickerSizeController>();
                }

                controller.setup(StickerState.OnBoard, id, i, position_x, position_y, rightBottomVertex_x, rightBottomVertex_y);
                boardStickers.Add(controller);
                stickersOnBoard.Add(id);
            }

            // 혹시 컨트롤러가 남아 있다면 비활
            // 필요없을것같기도하구,,
            for (int j = stickerList.size(); j < currentStickers; ++j)
            {
                rect_StickerRoot.GetChild(j).gameObject.SetActive(false);
            }

            return stickersOnBoard;
        }

        private void showHideDefaultPanel(bool show)
        {
            can_stickerBoard.interactable = !show;
            can_stickerBoard.blocksRaycasts = !show;
            DOTween.To(() => can_defaultSheet.alpha, x => can_defaultSheet.alpha = x, show ? 1f : 0f, 0.5f);
        }

        public void onClickBackNavigation()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
        }

        private void saveStickerToServer(BlobData data)
		{
            MapPacket req = Network.createReq(CSMessageID.Account.SaveStickerBoardReq);
            req.put("data", data);

            Network.call(req, ack => { 
            });
		}
    }
}