using UnityEngine;
using System.Collections.Generic;
public class Node
{
    /*
     이 노드가 벽인지 
    부모노드 
    x, y 좌표값
    f  h+g
    h  추정값 즉 가로 세로 장애물을 무시하여 목표까지의 거리
    g  시작으로 부터 이동했던 거리 
     */
    public bool isWall;
    public Node Parentnode;
    public int x, y;
    public int G;
    public int H;

    public int F
    {
        get
        {
            return G + H;
        }
    }
    public Node(bool iswall, int x, int y)
    {
        this.isWall = iswall;
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return "x : " + x + ",y : " + y;
    }

}
public class Astar : MonoBehaviour
{
    GameObject destination;

    public int range = 2;

    public Vector2Int bottomLeft, topRight, start_Pos, end_Pos;

    public List<Node> final_node;
    public Node[,] nodeArray;

    //대각선을 이용 할것인지 경우 // false일 시 직선으로만 이동
    public bool AllowDigonal = true;
    //코너를 가로질러 가지 않을 경우 이동중 수직 수평 장애물이 있는 지 판단 //false일 시 코너에 닿으면서 지나감
    public bool DontCrossCorner = false;

    Node startNode, endNode, curNode;

    private int sizeX, sizeY;

    const int CostStraight = 10;
    const int CostDiagonal = 14;

    int index = 0;

    List<Node> OpenList, ClosedList;
    public void SetGizmoIndex(int index)
    {
        this.index = index;
    }
    public void Clear()
    {
        final_node.Clear();
    }

    public void SetPosInit() //a* 알고리즘을 시작할 그리드의 범위를 정의한다.
    {
        float minX, maxX, minZ, maxZ;
        minX = transform.position.x;
        maxX = destination.transform.position.x;
        minZ = transform.position.z;
        maxZ = destination.transform.position.z;

        if (transform.position.x > destination.transform.position.x) //큰값, 작은값을 맞춰주기 위한 스왑
        {
            float temp = minX;
            minX = maxX;
            maxX = temp;
        }
        if (transform.position.z > destination.transform.position.z) //큰값, 작은값을 맞춰주기 위한 스왑
        {
            float temp = minZ;
            minZ = maxZ;
            maxZ = temp;
        }

        //시작부터 도착까지 range만큼 넓은 범위로 검색
        minX -= range; //범위를 더 넓게 잡기 위해서 작은값에는 -를 큰 값에는 +를 해준다.
        minZ -= range;
        maxX += range;
        maxZ += range;

        bottomLeft = new Vector2Int((int)minX, (int)minZ);//탐색할 그리드의 왼쪽 하단 좌표값
        topRight = new Vector2Int((int)maxX, (int)maxZ); //탐색할 그리드의 오른쪽 상단 좌표값
        start_Pos = new Vector2Int((int)transform.position.x, (int)transform.position.z); //시작 포지션
        end_Pos = new Vector2Int((int)destination.transform.position.x, (int)destination.transform.position.z); //목표한 지점 (end_Pos)
    }

    public List<Node> AStar(GameObject targetObject)
    {
        destination = targetObject; //목적지 받아옴
        SetPosInit(); //그리드 정의
        sizeX = topRight.x - bottomLeft.x + 1; //가로(X)에 몇개의 노드가 있는지 계산
        sizeY = topRight.y - bottomLeft.y + 1; //세로(Z)에 몇개의 노드가 있는지 계산
        //예를 들어 BottomLeft가 (0,0)이고 TopRight가 (4,4)라고 가정할 때 실제 노드의 개수는 0,1,2,3,4로 5개지만
        // 4 - 0 = 4로 4개의 노드, 즉 1개가 누락되기 때문에 이것을 보완하기 위해 + 1을 해줘야함.
        nodeArray = new Node[sizeX, sizeY]; //그리드의 사이즈를 토대로 노드 array 정의
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++) //그리드안에 있는 모든 좌표(노드)에 0.49f만큼의 오버랩 스피어를 생성하여 장애물이 있는지 없는지 판단한다.
            {                               //장애물이 있다면 충돌하여 true가 담기고 없다면 false를 담는다. array가 [5,5]라면 (0,0), (0,1), (0,2)...순으로 모두 검사.
                bool isObstacle = false;
                
                foreach (Collider col in Physics.OverlapSphere(new Vector3(i + bottomLeft.x, 0, j + bottomLeft.y), 0.49f))
                {
                    if (col.gameObject.layer.Equals(LayerMask.NameToLayer("Border")))
                    {
                        isObstacle = true;
                    }
                }
                nodeArray[i, j] = new Node(isObstacle, i + bottomLeft.x, j + bottomLeft.y);
            }
        }

        //i + bottomLeft.x, 0, j + bottomLeft.y 그리드 내의 상대적 위치를 전역 좌표계로 변환하기 위해 i + bottomLeft.x와 같이 계산한다.
        //그냥 bottomLeft.x, 0, bottomLeft.y를 하게 되면 계속 이 위치만 검사하게 됨. 따라서 정확하게 각 노드의 위치를 검사하기 위해 i+, j+를 해줘야한다.
        //EX) bottomLeft.x가 (10, 0, 10)이라고 가정할 때 그냥 bottomLeft.x만 넣으면 계속 10,0,10에 대한 검사만 하기 때문에 i+, j+를 통해 (i + 10, 0, j + 10)로 다음 위치를 검사할 수 있다.

        //Node 초기화, 시작지점 끝지점 넣어주기
        startNode = nodeArray[start_Pos.x - bottomLeft.x, start_Pos.y - bottomLeft.y];
        endNode = nodeArray[end_Pos.x - bottomLeft.x, end_Pos.y - bottomLeft.y];
        //그리드안에서 각 지점의 상대적인 위치를 넣어준다. 예를 들어 나의 위치가 (12, 0, 14)이고 bottomLeft가 (10, 0, 10)이라면 start_Pos.x - bottomLeft.x, start_Pos.y - bottomLeft.y
        // -> 12 - 10 = 2, 14 - 10 = 4 로 그리드에서 나의 위치는 [2,4]가 된다. 즉, 그리드안에서 시작지점과 종료지점 좌표를 얻기 위해서 다음과 같이 초기화 작업을 해준다.


        //List 초기화
        OpenList = new List<Node>();
        ClosedList = new List<Node>();
        final_node = new List<Node>();

        OpenList.Add(startNode);

        while (OpenList.Count > 0)
        {
            curNode = OpenList[0]; //가장 적합한 노드를 찾기위해 0번지로 설정. 이후 for문에서 인덱스 값을 변화시킨다.
            for (int i = 0; i < OpenList.Count; i++)
            {
                //열린 리스트중 가장 F가 작고 -> F최종 비용
                //F가 같다면 H가 작은 것을 현재 노드로 설정 -> 가상 최종비용
                if (OpenList[i].F <= curNode.F &&
                    OpenList[i].H < curNode.H)
                {
                    curNode = OpenList[i];
                }
                //열린 리스트에서 닫힌 리스트로 옮기기
                OpenList.Remove(curNode);
                ClosedList.Add(curNode);

                //노드가 목적지에 도착했을 때
                if (curNode == endNode)// 현재 노드가 목적지(end Node)에 도달했는지 여부를 확인한다.
                {
                    Node targetnode = endNode; //시작 노드는 목적지에서 시작지점으로 거슬러 올라가며 경로를 탐색한다.
                    while (targetnode != startNode) //현재 노드가 시작지점 노드가 아닐때 까지 while문을 반복하며 거슬러 올라간다.
                    {
                        final_node.Add(targetnode); //경로에 있는 노드를 최종 노드 리스트에 추가.
                        targetnode = targetnode.Parentnode; //현재 노드의 부모 노드로 이동.
                    }
                    final_node.Add(startNode); //루프가 끝나고 시작지점도 최종 노드 리스트에 추가해준다.
                    final_node.Reverse(); // 목적지부터 거슬러 올라가며 노드를 추가했기 때문에 이것을 반전시켜 시작지점부터 종료지점까지 올바른 경로를 반환할 수 있도록 한다.
                    return final_node; //최종 노드리스트 리턴.
                }
                if (AllowDigonal) // 도착하지 않았을 시 계속해서 계산, 주변노드를 리스트에 넣기
                {
                    //대각선으로 움직이는 cost 계산
                    // ↗↖↙↘
                    openListAdd(curNode.x + 1, curNode.y - 1); //오른쪽 하단
                    openListAdd(curNode.x - 1, curNode.y + 1); //왼쪽 상단
                    openListAdd(curNode.x + 1, curNode.y + 1); //오른쪽 상단
                    openListAdd(curNode.x - 1, curNode.y - 1); //왼쪽 하단
                }
                //직선으로 움직이는 cost 계산
                // ↑ → ↓ ←
                openListAdd(curNode.x + 1, curNode.y);
                openListAdd(curNode.x - 1, curNode.y);
                openListAdd(curNode.x, curNode.y + 1);
                openListAdd(curNode.x, curNode.y - 1);
            }

        }
        return final_node;

    }

    public void openListAdd(int checkX, int checkY)
    {
        /*
            상하좌우 범위를 벗어나지 않고,
            벽도 아니면서
            닫힌리스트에 없어야 한다.
         */

        //아래 if는 현재 좌표가 그리드 내에서 유효한 범위에 있는지 여부를 검사한다
        //checkX는 bottomLeft.X 보다 크거나 같아야하고 topRight.x + 1보다 작아야한다.
        //이것을 예를 들자면 bottomLeft가 (10,10) topRight가 (14,14)라고 할 때
        //이 설정에서 그리드의 X는 [10,11,12,13,14]가 되고 최소한 10보다는 크거나 같아야하며, 15보다는 작아야 설정된 그리드의 x에서 유효한 범위에 있는것이다.

        if (checkX >= bottomLeft.x && checkX < topRight.x + 1//x가 bottomleft와 top right안에 있고 벗어나면 예측범위 밖
            && checkY >= bottomLeft.y && checkY < topRight.y + 1 //y도 마찬가지로 범위 확인
            && !nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y].isWall //벽인지 확인
            && !ClosedList.Contains(nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y])//닫힌 리스트, 이미 계산된 노드인지 판단하여 중복연산을 방지한다.
            )
        {
            if (AllowDigonal)//대각선 허용 시
            {
                if (nodeArray[curNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall &&
                    nodeArray[checkX - bottomLeft.x, curNode.y - bottomLeft.y].isWall)
                {
                    return;
                }
            }
            //코너를 가로질러 가지 않을 시 (이동 중 수직 수펑 장애물이 있으면 안됨.)
            if (DontCrossCorner)
            {
                if (nodeArray[curNode.x - bottomLeft.x, checkY - bottomLeft.y].isWall ||
                   nodeArray[checkX - bottomLeft.x, curNode.y - bottomLeft.y].isWall)
                {
                    return;
                }
            }
            //check하는 노드를 이웃 노드에 넣고 직선은 10 대각선 14
            Node neightdorNode =
                nodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];//검사하려는 인접 노드를 그리드에서의 위치를 찾기 위해 전역 좌표계에서 그리드 좌표로 변환한다.checkX - bottomLeft.x, checkY - bottomLeft.y
            int movecost = curNode.G +
                (curNode.x - checkX == 0 || curNode.y - checkY == 0 ? CostStraight : CostDiagonal);
            //이동 비용을 계산한다. curNode.x - checkX == 0이면 대각선 이동,curNode.y - checkY == 0은 직선이동 하나라도 true라면 대각선 이동 비용을 사용한다.

            //이동비용이 이웃노드 G보다 작거나, 또는 열린 리스트에 이웃노드가 없다면
            if (movecost < neightdorNode.G || !OpenList.Contains(neightdorNode))
            {
                //G H parentnode를 설정후 열린 리스트에 추가
                neightdorNode.G = movecost;
                neightdorNode.H = (
                    Mathf.Abs(neightdorNode.x - endNode.x) +
                    Mathf.Abs(neightdorNode.y - endNode.y)) * CostStraight;

                neightdorNode.Parentnode = curNode;//부모 노드를 설정하면서 경로를 역추적한다. 

                OpenList.Add(neightdorNode);
            }
        }
    }
    private void OnDrawGizmos()
    {
        //씬 뷰의 Debug용도로 그림을 그릴 떄 사용합니다. 
        if (final_node != null)
        {
            for (int i = index; i < final_node.Count - 1; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(new Vector3(final_node[i].x, 0, final_node[i].y),
                    new Vector3(final_node[i + 1].x, 0, final_node[i + 1].y));
            }
        }
    }
}