using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int score;

    int x;
    int y;

    public int X {
        get { return x; }
        set
        {
            if (IsMovable())
            {
                x = value;
            } }
    }

    public int Y { 
        get { return y; }
        set
        {
            if (IsMovable())
            {
                y = value;
            }
        }
    }


    Grid.PieceType type;
    public Grid.PieceType Type { get { return type; } }

    Grid grid;
    public Grid GridRef { get { return grid; } }

    MovablePiece movableComponent;
    public MovablePiece MovableComponent { 
        get { return movableComponent; } 
    }

    ColorPiece colorComponent;
    public ColorPiece ColorComponent
    {
        get { return colorComponent; }
    }

    ClearablePiece clearableComponent;
    public ClearablePiece ClearableComponent
    {
        get { return clearableComponent; }
    }

    private void Awake()
    {
        movableComponent=GetComponent<MovablePiece>();
        colorComponent=GetComponent<ColorPiece>();
        clearableComponent = GetComponent<ClearablePiece>();
    }

    public void Init(int _x,int _y,Grid _grid,Grid.PieceType _type)
    {
        x = _x; 
        y = _y;
        grid = _grid;
        type = _type;
    }

    private void OnMouseEnter()
    {
        grid.EnterPiece(this);
    }

    private void OnMouseDown()
    {
        grid.PressPiece(this);
    }

    private void OnMouseUp()
    {
        grid.ReleasePiece();
    }

    public bool IsMovable()
    {
        return movableComponent!=null; // boþ deðilse hareket edebilir demek ve true döner
    }

    public bool IsColored()
    {
        return colorComponent != null; 
    }

    public bool IsClearable()
    {
        return clearableComponent != null; 
    }
}
