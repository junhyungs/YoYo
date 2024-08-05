
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public bool isWall;
    public PathNode ParentNode;
    public int PosX, PosZ;
    public int MoveCost_G;
    public int HeuristicCost_H;
    public int TotalCost_F;

    public int TotalCost
    {
        get
        {
            return MoveCost_G + HeuristicCost_H;
        }
    }

    public PathNode(bool isWall, int posX, int posZ)
    {
        this.isWall = isWall;
        PosX = posX;
        PosZ = posZ;
    }

    
}


public class PathFind : MonoBehaviour
{
    private Vector3 _destination;
    private int _range = 3;
    private Vector2Int _bottomLeft, _topRight, _startPos, _endPos;
    private List<PathNode> _finalNode;
    private PathNode[,] _grid;

    [Header("Digonal")]
    [SerializeReference] private bool _isDigonal;

    [Header("CrossConer")]
    [SerializeField] private bool _isCrossConer;

    private PathNode _startNode, _endNode, _currentNode;

    private int _gridSizeX, _gridSizeZ;

    private const int CostStraight = 10;
    private const int CostDiagonal = 14;

    private List<PathNode> _openList, _closedList;

    public void Clear()
    {
        _finalNode.Clear();
    }

    private void SetInitializePos(Vector3 destination)
    {
        float minPosX, maxPosX, minPosZ, maxPosZ;

        minPosX = transform.position.x;
        maxPosX = destination.x;
        minPosZ = transform.position.z;
        maxPosZ = destination.z;

        if(transform.position.x > destination.x)
        {
            float temp = minPosX;
            minPosX = maxPosX;
            maxPosX = temp;
        }

        if(transform.position.z > destination.z)
        {
            float temp = minPosZ;
            minPosZ = maxPosZ;
            maxPosZ = temp;
        }

        minPosX -= _range;
        minPosZ -= _range;
        maxPosX += _range;
        maxPosZ += _range;

        _bottomLeft = new Vector2Int((int)minPosX, (int)minPosZ);
        _topRight = new Vector2Int((int)maxPosX, (int)maxPosZ);
        _startPos = new Vector2Int((int)transform.position.x, (int)transform.position.z);
        _endPos = new Vector2Int((int)destination.x, (int)destination.z);

    }

    public List<PathNode> PathFinding(Vector3 destination)
    {
        _destination = destination;

        SetInitializePos(destination);

        _gridSizeX = _topRight.x - _bottomLeft.x + 1;
        _gridSizeZ = _topRight.y - _bottomLeft.y + 1;
        

        _grid = new PathNode[_gridSizeX, _gridSizeZ];

        for(int i = 0; i < _gridSizeX; i++)
        {
            for(int j = 0; j < _gridSizeZ; j++)
            {
                bool isObstacle = false;

                foreach (Collider coll in Physics.OverlapSphere(new Vector3(i + _bottomLeft.x, 0, j + _bottomLeft.y), 0.49f))
                {
                    if(coll.gameObject.layer == LayerMask.NameToLayer("Wall"))
                    {
                        isObstacle = true;
                    }
                }

                _grid[i, j] = new PathNode(isObstacle, i + _bottomLeft.x, j + _bottomLeft.y);
            }
        }

        _startNode = _grid[_startPos.x - _bottomLeft.x, _startPos.y - _bottomLeft.y];
        _endNode = _grid[_endPos.x - _bottomLeft.x, _endPos.y - _bottomLeft.y];

        _openList = new List<PathNode>();
        _closedList = new List<PathNode>();
        _finalNode = new List<PathNode>();

        _openList.Add(_startNode);

        while(_openList.Count > 0)
        {
            _currentNode = _openList[0];

            for(int i = 0; i < _openList.Count; i++)
            {
                if (_openList[i].TotalCost <= _currentNode.TotalCost
                    && _openList[i].HeuristicCost_H < _openList[i].HeuristicCost_H)
                {
                    _currentNode = _openList[i];
                }

                _openList.Remove(_currentNode);
                _closedList.Add(_currentNode);

                if(_currentNode == _endNode)
                {
                    PathNode targetNode = _endNode;

                    while(targetNode != _startNode)
                    {
                        _finalNode.Add(targetNode);
                        targetNode = targetNode.ParentNode;
                    }

                    _finalNode.Add(_startNode);
                    _finalNode.Reverse();
                    return _finalNode;
                }

                if (_isDigonal)
                {
                    OpenListAdd(_currentNode.PosX + 1, _currentNode.PosZ - 1);
                    OpenListAdd(_currentNode.PosX - 1, _currentNode.PosZ + 1);
                    OpenListAdd(_currentNode.PosX + 1, _currentNode.PosZ + 1);
                    OpenListAdd(_currentNode.PosX - 1, _currentNode.PosZ - 1);
                }

                OpenListAdd(_currentNode.PosX + 1, _currentNode.PosZ);
                OpenListAdd(_currentNode.PosX - 1, _currentNode.PosZ);
                OpenListAdd(_currentNode.PosX, _currentNode.PosZ + 1);
                OpenListAdd(_currentNode.PosX, _currentNode.PosZ - 1);
            }
        }

        return _finalNode;
    }
    
    private void OpenListAdd(int checkPosX, int checkPosZ)
    {
        if (checkPosX >= _bottomLeft.x && checkPosX < _topRight.x + 1
            && checkPosZ >= _bottomLeft.y && checkPosZ < _topRight.y + 1
            && !_grid[checkPosX - _bottomLeft.x, checkPosZ - _bottomLeft.y].isWall
            && !_closedList.Contains(_grid[checkPosX - _bottomLeft.x, checkPosZ - _bottomLeft.y]))
        {
            if (_isDigonal)
            {
                if (_grid[_currentNode.PosX - _bottomLeft.x, checkPosZ - _bottomLeft.y].isWall &&
                    _grid[checkPosX - _bottomLeft.x, _currentNode.PosZ - _bottomLeft.y].isWall)
                {
                    return;
                }
            }

            if (_isCrossConer)
            {
                if (_grid[_currentNode.PosX - _bottomLeft.x, checkPosZ - _bottomLeft.y].isWall ||
                    _grid[checkPosX - _bottomLeft.x, _currentNode.PosZ - _bottomLeft.y].isWall)
                {
                    return;
                }
            }

            PathNode neighborNode = _grid[checkPosX - _bottomLeft.x, checkPosZ - _bottomLeft.y];

            int moveCost = _currentNode.MoveCost_G +
                (_currentNode.PosX - checkPosX == 0 || _currentNode.PosZ - checkPosZ == 0 ? CostStraight : CostDiagonal);

            if(moveCost < neighborNode.MoveCost_G || !_openList.Contains(neighborNode))
            {
                neighborNode.MoveCost_G = moveCost;
                neighborNode.HeuristicCost_H = (
                    Mathf.Abs(neighborNode.PosX - _endNode.PosX) +
                    Mathf.Abs(neighborNode.PosZ - _endNode.PosZ)) * CostStraight;

                neighborNode.ParentNode = _currentNode;

                _openList.Add(neighborNode);    
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_grid == null)
            return;

        Gizmos.color = Color.green;
        foreach (var node in _grid)
        {
            if (node != null && node.isWall)
            {
                Gizmos.DrawSphere(new Vector3(node.PosX, 0, node.PosZ), 0.2f);
            }
        }

        Gizmos.color = Color.red;
        if (_finalNode != null)
        {
            foreach (var node in _finalNode)
            {
                Gizmos.DrawSphere(new Vector3(node.PosX, 0, node.PosZ), 0.2f);
            }
        }

        if(_finalNode == null)
        {
            Debug.Log("finalNode is null");
        }
    }


}
