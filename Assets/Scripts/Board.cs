using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    BoardManager boardManager;
    Dictionary<Vector3Int, PositionType> boardPositions = new();
    [SerializeField] Grid grid;
    [SerializeField] int width;
    [SerializeField] int height;

    public Grid Grid { get => grid; }
    public int Width { get => width; }
    public int Height { get => height; }


}


public enum PositionType
{
    Sea, Land, Shallow
}