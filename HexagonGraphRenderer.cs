using UnityEngine;
using UnityEngine.UI;

// 라인차트에서 Focus 라인을 그려보자
[RequireComponent(typeof(CanvasRenderer))]
public class HexagonGraphRenderer : MaskableGraphic
{
    [SerializeField]
    private Image[] dots;                               // 육각형의 꼭짓점

    private float[] targetLength = new float[6];        // 그래프 요소별 실제 값
    private Vector3[] targetPoints = new Vector3[6];    // 값을 그래프로 표현했을 때 그려질 위치

    private Vector2 _centerPoint = Vector2.zero;        // 방사형 그래프의 중심
    private Vector3 startPoint;
    private float lineWeight = 2f;

    private float slope;
    private float yDelta;

    public void setValue(float one, float two, float three, float four, float five, float six)
    {
        targetLength[0] = one;
        targetLength[1] = two;
        targetLength[2] = three;
        targetLength[3] = four;
        targetLength[4] = five;
        targetLength[5] = six;
    }

    public void draw()
    {
        // 길이값을 위치로 바꿔준다
        for (int i = 0; i < targetLength.Length; ++i)
        {
            float theta = Mathf.PI / 6f + i * Mathf.PI / 3f;
            targetPoints[i] = new Vector3(targetLength[i] * Mathf.Cos(theta) + _centerPoint.x, targetLength[i] * Mathf.Sin(theta) + _centerPoint.y, 0f);
        }

        // 꼭짓점 찍어주기
        for (int i = 0; i < targetPoints.Length; ++i)
        {
            startPoint = targetPoints[i];
            dots[i].transform.parent.GetComponent<RectTransform>().anchoredPosition = startPoint;
        }

        SetAllDirty();
    }

    /// <summary>
    ///
    /// 그래프를 6 등분해서 한 조각씩 그리고 위치에 맞게 돌려주는 방식
    ///
    /// </summary>

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        var vertex = UIVertex.simpleVert;

        for (int i = 0; i < targetPoints.Length; ++i)
        {
            // 직선의 시작과 끝을 잡는다
            startPoint = targetPoints[i];
            Vector3 endPoint;

            if (i + 1 >= targetPoints.Length)
                endPoint = targetPoints[0];
            else
                endPoint = targetPoints[i + 1];

            // 배경을 칠한다
            vertex.color = Color.green;
            vertex.position = startPoint;
            vh.AddVert(vertex);
            vertex.position = endPoint;
            vh.AddVert(vertex);
            vertex.position = _centerPoint;
            vh.AddVert(vertex);
            vh.AddTriangle(vh.currentVertCount - 3, vh.currentVertCount - 2, vh.currentVertCount - 1);

            // 외곽선을 그린다
            vertex.color = Color.blue;
            float xDelta = 0f;

            if ((int)startPoint.x == (int)endPoint.x)
            {
                // 함수가 아닌 경우 == 기울기가 발산하는 경우
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
                    // 기울기가 0 인 경우
                    yDelta = lineWeight;
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
