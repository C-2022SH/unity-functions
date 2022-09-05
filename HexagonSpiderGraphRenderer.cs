using AwesomeCharts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Festa.Client.RefData;
using TMPro;

namespace Festa.Client
{
    // 라인차트에서 Focus 라인을 그려보자
    [RequireComponent(typeof(CanvasRenderer))]
    public class HexagonSpiderGraphRenderer : LineSegmentsRenderer
    {
        [SerializeField]
        private TMP_Text[] levelLabels;
        [SerializeField]
        private Image[] dots;
        [SerializeField]
        private RectTransform rect_label;
        [SerializeField]
        private TMP_Text txt_label;
        [SerializeField]
        private TMP_Text txt_title;
        [SerializeField]
        private TMP_Text txt_topDesc;
        [SerializeField]
        private Color[] statColors;     // 일단은 이렇게 해놓고 뭔가 좀 확정되면 차트에 저장해 놓자!!
        [SerializeField]
        private Color[] dotColors;

        private float[] targetLength = new float[6];
        private Vector3[] targetPoints = new Vector3[6];
        private float _labelOffsetY = -7f;

        private Vector2 _centerPoint = new Vector2(0f, -151f);
        private Vector3 startPoint;
        private float lineWeight = 2f;

        private float slope;
        private float yDelta;

        private Color _graphColor;
        private Color _dotColor;

        public void setScore(float exp, float fit, float emo, float iint, float ins, float soc)
        {
            targetLength[0] = exp;
            targetLength[1] = fit;
            targetLength[2] = emo;
            targetLength[3] = iint;
            targetLength[4] = ins;
            targetLength[5] = soc;
        }

        public void setLevel(float exp, float fit, float emo, float iint, float ins, float soc)
        {
            levelLabels[0].text = exp.ToString();
            levelLabels[1].text = fit.ToString();
            levelLabels[2].text = emo.ToString();
            levelLabels[3].text = iint.ToString();
            levelLabels[4].text = ins.ToString();
            levelLabels[5].text = soc.ToString();
        }

        public void draw()
        {
            rect_label.gameObject.SetActive(false);

            // 차트 만들기
            // 그래프 상 최댓값은 150
            int renderMax = 150;
            int top = 0;

            // 최댓값 구하기
            for (int i = 0; i < targetLength.Length; ++i)
            {
                if (targetLength[top] < targetLength[i])
                    top = i;
            }

            var sc = GlobalRefDataContainer.getStringCollection();
            float threshold = float.Parse(sc.get("lifestat.graphMaxRef", 0));

            if(targetLength[top] > threshold)
            {
                threshold = targetLength[top];
            }

            // 길이값을 위치로 바꿔준다~~
            for (int i = 0; i < targetLength.Length; ++i)
            {
                float normalizedLength = targetLength[i] * (renderMax / threshold);

                float theta = Mathf.PI / 6f + i * Mathf.PI / 3f;
                targetPoints[i] = new Vector3(normalizedLength * Mathf.Cos(theta) + _centerPoint.x, normalizedLength * Mathf.Sin(theta) + _centerPoint.y, 0f);
            }

            _graphColor = statColors[top];
            _dotColor = dotColors[top];
            string level = sc.get("lifestat.itemLevel", top);
            txt_title.text = sc.getFormat("lifestat.title", 0, level);
            txt_topDesc.text = sc.get("lifestat.desc", top);

			for (int i = 0; i < targetPoints.Length; ++i)
            {
				// 꼭짓점 찍기
				dots[i].color = _dotColor;
				dots[i].transform.parent.GetComponent<RectTransform>().anchoredPosition = targetPoints[i];

			}

			SetAllDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var vertex = UIVertex.simpleVert;

            for (int i = 0; i < targetPoints.Length; ++i)
            {
                // 포인트 잡기!
                startPoint = targetPoints[i];
                Vector3 endPoint;

                if (i + 1 >= targetPoints.Length)
                    endPoint = targetPoints[0];
                else
                    endPoint = targetPoints[i + 1];

                // 배경 칠하기
                Color withAlpha = _graphColor;
                withAlpha.a = 0.4f;
                vertex.color = withAlpha;

                vertex.position = startPoint;
                vh.AddVert(vertex);
                vertex.position = endPoint;
                vh.AddVert(vertex);
                vertex.position = _centerPoint;
                vh.AddVert(vertex);
                vh.AddTriangle(vh.currentVertCount - 3, vh.currentVertCount - 2, vh.currentVertCount - 1);

                // 아웃라인 잡기
                vertex.color = _graphColor;
                float xDelta = 0f;

                if ((int)startPoint.x == (int)endPoint.x)
                {
                    // 함수가 아닐 수도 있나?? 그럼 어떡할래~~
                    // 그냥 대충 그려도 된다는 뜻~~~
                    xDelta = lineWeight * 0.5f;

                    // 위에 삼각형
                    vertex.position = new Vector3(startPoint.x + xDelta, startPoint.y, 0.0f);
                    vh.AddVert(vertex);
                    vertex.position = new Vector3(startPoint.x - xDelta, startPoint.y, 0.0f);
                    vh.AddVert(vertex);

                    // 밑에 삼각형
                    vertex.position = new Vector3(endPoint.x + xDelta, endPoint.y, 0.0f);
                    vh.AddVert(vertex);
                    vertex.position = new Vector3(endPoint.x - xDelta, endPoint.y, 0.0f);
                    vh.AddVert(vertex);
                }
                else
                {
                    slope = (startPoint.y - endPoint.y) / (startPoint.x - endPoint.x);

                    if (slope == 0)
                    {
                        // 0 이 될 수도 있나?? 그럼 어떡할래~~
                        // 그냥 2 겠지~~
                        yDelta = 2f;
                    }
                    else
                    {
                        float theta = Mathf.Atan(slope);
                        float cos = Mathf.Cos(theta);
                        if (cos != 0)
                            yDelta = lineWeight / cos;

                        xDelta = lineWeight * 0.5f * Mathf.Sin(theta);
                    }

                    // 위에 삼각형
                    vertex.position = new Vector3(startPoint.x + xDelta, getLowerY(startPoint.x + xDelta), 0.0f);
                    vh.AddVert(vertex);
                    vertex.position = new Vector3(startPoint.x - xDelta, getUpperY(startPoint.x - xDelta), 0.0f);
                    vh.AddVert(vertex);

                    // 밑에 삼각형
                    vertex.position = new Vector3(endPoint.x + xDelta, getLowerY(endPoint.x + xDelta), 0.0f);
                    vh.AddVert(vertex);
                    vertex.position = new Vector3(endPoint.x - xDelta, getUpperY(endPoint.x - xDelta), 0.0f);
                    vh.AddVert(vertex);
                }

                vh.AddTriangle(vh.currentVertCount - 4, vh.currentVertCount - 3, vh.currentVertCount - 2);
                vh.AddTriangle(vh.currentVertCount - 3, vh.currentVertCount - 2, vh.currentVertCount - 1);
            }
        }

        #region get vertex y position

        private float getY(float targetX, float slope, Vector2 refPoint, float yDelta = 0f)
        {
            return slope * (targetX - refPoint.x) + refPoint.y + yDelta;
        }

        private float getLowerY(float x)
        {
            return getY(x, slope, startPoint, -yDelta * 0.5f);
        }

        private float getUpperY(float x)
        {
            return getY(x, slope, startPoint, yDelta * 0.5f);
        }

        #endregion
    }
}