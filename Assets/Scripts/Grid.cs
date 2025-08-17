using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        BUBBLE,
        COUNT,
    };

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }

    public int xDim;
    public int yDim;
    public float fillTime;

    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;

    Dictionary<PieceType, GameObject> piecePrefabDict;
    GamePiece[,] pieces;
    
    bool inverse=false;

    GamePiece pressedPiece;
    GamePiece enteredPiece;

    void Start()
    {

        piecePrefabDict = new Dictionary<PieceType, GameObject>();

        for (int i = 0; i < piecePrefabs.Length; i++) 
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        // grid oluþturan iç içe döngü
        // grid sol üst köþeden oluþturulmaya baþlanýyor ve önce sütun sonra satýr þeklinde gidiyor.
        for (int x = 0; x < xDim; x++) 
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background=(GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent= transform; // background objesini grid managerýn child i yapýyoruz 
            }
        }

        pieces=new GamePiece[xDim,yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnNewPiece(x, y,PieceType.EMPTY);
                //GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], Vector3.zero, Quaternion.identity);
                //newPiece.name = "Piece(" + x + " " + y + ")";
                //newPiece.transform.parent = transform;

                //pieces[x, y] = newPiece.GetComponent<GamePiece>();
                //pieces[x, y].Init(x, y, this, PieceType.NORMAL);

                //if (pieces[x, y].IsMovable())
                //{
                //    pieces[x, y].MovableComponent.Move(x, y);
                //}

                //if (pieces[x, y].IsColored())
                //{
                //    pieces[x, y].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, y].ColorComponent.NumColors));
                //}
            }
        }

        Destroy(pieces[4, 4].gameObject);
        SpawnNewPiece(4, 4,PieceType.BUBBLE);


        Destroy(pieces[1, 3].gameObject);
        SpawnNewPiece(1, 3, PieceType.BUBBLE);


        Destroy(pieces[6, 4].gameObject);
        SpawnNewPiece(6, 4, PieceType.BUBBLE);

        Destroy(pieces[3, 7].gameObject);
        SpawnNewPiece(3, 7, PieceType.BUBBLE);

        StartCoroutine(Fill());

    }

    public IEnumerator Fill()
    {
        while (FillStep()) 
        {
            inverse = !inverse;
            yield return new WaitForSeconds(fillTime);
        }

        
    }

    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;
                if (inverse)
                {
                    x = xDim - 1 - loopX; 
                }

                GamePiece piece = pieces[x, y];

                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = pieces[x, y + 1];
                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(x, y+1,fillTime);
                        pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                    else
                    {
                        for (int diag = 0; diag <= 1; diag++)
                        {
                            if (diag!=0)
                            {
                                int diagX = x + diag;

                                if (inverse)
                                {
                                    diagX = x - diag;
                                }

                                if (diagX>=0 && diagX<xDim)
                                {
                                    GamePiece diagonalPiece = pieces[diagX, y + 1];
                                    if (diagonalPiece.Type==PieceType.EMPTY)
                                    {
                                        bool hasPieceAbove = true;
                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GamePiece pieceAbove = pieces[diagX,aboveY];
                                            if (pieceAbove.IsMovable())
                                            {
                                                break;
                                            }
                                            else if (!pieceAbove.IsMovable() && pieceAbove.Type!=PieceType.EMPTY)
                                            {
                                                hasPieceAbove = false;
                                                break;
                                            }
                                        }

                                        if (!hasPieceAbove)
                                        {
                                            Destroy(diagonalPiece.gameObject);
                                            piece.MovableComponent.Move(diagX,y+1,fillTime);
                                            pieces[diagX, y + 1] = piece;
                                            SpawnNewPiece(x, y, PieceType.EMPTY);
                                            movedPiece = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
//----------------------------------------------------------------------------------------
        for (int x = 0; x < xDim; x++)
        {
            GamePiece pieceBelow = pieces[x, 0];
            if (pieceBelow.Type==PieceType.EMPTY)
            {
                Destroy(pieceBelow.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);

                newPiece.transform.parent=transform;
                
                pieces[x, 0]=newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                pieces[x, 0].MovableComponent.Move(x, 0,fillTime);
                pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors));
                movedPiece = true;
            }
        }

        return movedPiece;
    }
    
    public Vector2 GetWorldPosition(int x,int y)
    {
        //return new Vector2 (transform.position.x-xDim/2.0f+x,
        //    transform.position.y+yDim/2.0f-y
        //    );
        return new Vector2(transform.position.x - 4 + x,
            transform.position.y + 4- y
            );
    }

    public GamePiece SpawnNewPiece(int x,int y,PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type],GetWorldPosition(x,y),Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x,y]= newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x,y,this,type);

        return pieces[x, y];
    }


    public bool IsAdjacent(GamePiece piece1,GamePiece piece2)
    {
         return (piece1.X == piece2.X && (int)Mathf.Abs(piece2.Y - piece1.Y)==1
            || piece1.Y == piece2.Y && (int)Mathf.Abs(piece2.X - piece1.X) == 1
            );
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y]=piece2 ;
            pieces[piece2.X, piece2.Y] = piece1;

            int piece1X = piece1.X;
            int piece1Y = piece1.Y;
            piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);

            piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

        }
    }

    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }

    public void EnterPiece(GamePiece piece)
    {
        enteredPiece = piece;
    }

    public void ReleasePiece() 
    {
        if (IsAdjacent(pressedPiece,enteredPiece))
        {
            SwapPieces(pressedPiece, enteredPiece);
        }
    }

}
