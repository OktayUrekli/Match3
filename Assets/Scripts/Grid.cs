using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public enum PieceType // gride yerle�tirilebilecek obje tipleri
    {
        EMPTY, 
        NORMAL, // etkile�ime ge�ilebilen boje
        BUBBLE, // engel
        COUNT,
    };

    [System.Serializable]
    public struct PiecePrefab // gride yerle�tirilecek objelerin i� yap�s�
    {
        public PieceType type; 
        public GameObject prefab;
    }

    public int xDim; // geni�lik
    public int yDim; // y�kseklik
    public float fillTime; // objelerin yukar�dan a�a��ya d��mesini animasyonlanla�t�rmak i�in gerekli olan de�i�ken

    public PiecePrefab[] piecePrefabs; // yerle�tirlecek objeleri tutar
    public GameObject backgroundPrefab; // gridi olu�tururken kullan�lacak boje prefab�

    Dictionary<PieceType, GameObject> piecePrefabDict;
    GamePiece[,] pieces; // oyun i�inde olu�turulan objelerin listesini tutuyor
    
    bool inverse=false;

    GamePiece pressedPiece; // yer de�i�tirme i�in �zerine bas�lan obje -- ilk konum
    GamePiece enteredPiece; // ilk konuma ge�irilmesi istenen ikinci obje -- son konum

    void Start()
    {

        piecePrefabDict = new Dictionary<PieceType, GameObject>();

        // yerle�tirilebilecek objeler dictionary ye kaydediliyor
        for (int i = 0; i < piecePrefabs.Length; i++) 
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type)) // e�er dictionary de bu tipte obje yoksa 
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab); // bu par�ay� dictionary ye ekle
            }
        }

        // grid olu�turan i� i�e d�ng�
        // grid sol �st k��eden olu�turulmaya ba�lan�yor ve �nce s�tun sonra sat�r �eklinde gidiyor.
        for (int x = 0; x < xDim; x++) 
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background=(GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent= transform; // background objesini grid manager�n child i yap�yoruz 
            }
        }

        pieces=new GamePiece[xDim,yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnNewPiece(x, y,PieceType.EMPTY); // b�t�n grid bo� objeler ile dolduruluyor.
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

        // ge�ici olarak gride engeller eklendi
        Destroy(pieces[4, 4].gameObject);
        SpawnNewPiece(4, 4,PieceType.BUBBLE);


        Destroy(pieces[1, 3].gameObject);
        SpawnNewPiece(1, 3, PieceType.BUBBLE);


        Destroy(pieces[6, 4].gameObject);
        SpawnNewPiece(6, 4, PieceType.BUBBLE);

        Destroy(pieces[3, 7].gameObject);
        SpawnNewPiece(3, 7, PieceType.BUBBLE);

        StartCoroutine(Fill()); // ba�ka objelerle doldurma i�lemi ba�l�yor

    }

    public IEnumerator Fill()
    {
        bool needsRefill = true;

        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }
            needsRefill = ClearAllValidMatches();
        }
    }

    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = yDim - 2; y >= 0; y--) // en a�a��daki konum =yDim-1 => 8 -- bu konumdaki par�a hareket edemeyece�i i�in bir �stteki par�adan kontrol etmeye ba�l�yor
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;
                if (inverse)
                {
                    x = xDim - 1 - loopX; 
                }

                GamePiece piece = pieces[x, y];

                if (piece.IsMovable()) // par�a hareket edebilen bir par�a ise 
                {
                    GamePiece pieceBelow = pieces[x, y + 1]; // bir birim a�a��s�ndaki par�a 
                    if (pieceBelow.Type == PieceType.EMPTY) // bir birim a�a��s�ndaki par�a bo� ise 
                    {
                        Destroy(pieceBelow.gameObject); // bo� par�ay� yok et
                        piece.MovableComponent.Move(x, y+1,fillTime); // hareket edebilen par�ay� bir birim a�a�� kayd�r
                        pieces[x, y + 1] = piece; 
                        SpawnNewPiece(x, y, PieceType.EMPTY); // kayan par�an�n eski konumunu bo� olarak i�aretlemek i�in oraya yeni bir bo� par�a olu�turuluyor
                        movedPiece = true;
                    }
                    else // e�er bir birim a�a��daki par�a bo� de�il ise alt �aprazdaki par�a kontrol edilecek.
                    {
                        for (int diag = 0; diag <= 1; diag++)
                        {
                            if (diag!=0)
                            {
                                int diagX = x + diag; // sa� taraf

                                if (inverse)// inverse ba�lang��ta false oldu�u i�in �nce sa� alt kontrol edilir 
                                {
                                    diagX = x - diag; // sol taraf
                                }

                                if (diagX>=0 && diagX<xDim) // konum s�n�rlar i�erisinde ise 
                                {
                                    GamePiece diagonalPiece = pieces[diagX, y + 1];  // diagX sa� sol belirler -- sa�/sol alt �aprazdaki par�a al�n�yor
                                    if (diagonalPiece.Type==PieceType.EMPTY)  // alt �apraz konumu bo� ise 
                                    {
                                        bool hasPieceAbove = true;  
                                        for (int aboveY = y; aboveY >= 0; aboveY--)  
                                        {
                                            GamePiece pieceAbove = pieces[diagX,aboveY]; // bir birim yandaki obje kontrol ediliyor
                                            if (pieceAbove.IsMovable()) // e�er hareket edebilen bir par�a ise 
                                            {
                                                break; // alt �apraz� doldurma g�revi bu par�an�nd�r.
                                            }
                                            else if (!pieceAbove.IsMovable() && pieceAbove.Type!=PieceType.EMPTY) // e�er hareket edemez ve bo� de�il ise yani engel ise 
                                            {
                                                hasPieceAbove = false; // bo� olan alt �apraz konumunun �st�nde oray� doldurabilecek hareket edebilen bir obje yok olarak i�aretleniyor
                                                break;
                                            }
                                        }

                                        if (!hasPieceAbove) // e�er bir birim yukar�da bo� konumu doldurabilecek par�a yok ise
                                        {
                                            Destroy(diagonalPiece.gameObject); // alt �aprazdaki bo� par�a yok ediliyor
                                            piece.MovableComponent.Move(diagX,y+1,fillTime); 
                                            pieces[diagX, y + 1] = piece; // dolu par�a bo� konuma getirilir
                                            SpawnNewPiece(x, y, PieceType.EMPTY); // dolu par�an�n eski konumu bo� olarak i�aretlenir
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
        for (int x = 0; x < xDim; x++) // sat�r�n bo� olan konumlar� normal par�ayla dolduruluyor
        {   // ba�lang��ta t�m grid bo� oldu�u i�in �nce bir sat�r hareket edebilen par�a olu�turulacak sonra a�a�� kayd�rma i�lemi ger�ekle�tirilecek 
            GamePiece pieceBelow = pieces[x, 0];
            if (pieceBelow.Type==PieceType.EMPTY)
            {
                Destroy(pieceBelow.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);

                newPiece.transform.parent=transform;
                
                pieces[x, 0]=newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                pieces[x, 0].MovableComponent.Move(x, 0,fillTime); // olu�turulan normla t�rdeki par�a bir birim a�a�� kayd�r�l�yor
                pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors)); // olu�turulan par�an�n rengi rasgele olarak atan�yor
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

    public GamePiece SpawnNewPiece(int x,int y,PieceType type) // istenilen konumda istenilen t�rde par�a olu�turan metot
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type],GetWorldPosition(x,y),Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x,y]= newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x,y,this,type); // olu�turulan par�aya bilgileri atan�yor 

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

            if (GetMatch(piece1, piece2.X, piece2.Y) !=null || GetMatch(piece2, piece1.X, piece1.Y)!=null) // uygun e�le�me var ise yer de�i�tirme ger�ekle�ir 
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();
                StartCoroutine(Fill());
            }
            else // uygun e�le�me yok ise par�alar eski konumuna getirilir
            {
                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;
            }


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

    public List<GamePiece> GetMatch(GamePiece piece,int newX,int newY)
    {
        if (piece.IsColored())
        {
            ColorPiece.ColorType color = piece.ColorComponent.Color; // par�an�n t�r� al�n�yor
            List<GamePiece> horizontalPieces= new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // �nce yatay e�le�me kotrol ediliyor
            #region Yatay e�le�me kontrol�
            horizontalPieces.Add(piece); 
            for (int dir = 0; dir <= 1; dir++) // yatayda arama yap�lacak 0=sol arama indexi  1=sa� arama indexi
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;
                    if (dir==0)//sol
                    {
                        x = newX - xOffset;
                    }
                    else // sa�
                    {
                        x= newX + xOffset;
                    }

                    if (x<0 || x>=xDim) // grid s�n�rlar� d���nda ise 
                    {
                        break;
                    }

                    if (pieces[x, newY].IsColored() && pieces[x, newY].ColorComponent.Color == color) // e�er ayn� t�rden par�a ise 
                    {
                        horizontalPieces.Add(pieces[x, newY]); 
                    }
                    else // farkl� t�r ise 
                    {
                        break;
                    }

                }
            }

            if (horizontalPieces.Count>=3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            // T veya L �eklindeki e�le�me i�in dikey aray��a ge�i�
            if (horizontalPieces.Count >= 3) 
            {
                for (int i = 0; i < horizontalPieces.Count; i++)  // ayn� t�rde olan t�m par�alar�n a�a�� ve yukar�s�ndaki par�alar da kontrol ediliyor 
                { 
                    for (int dir = 0; dir <= 1; dir++) // �st alt arama i�in index
                    {
                        for (int yOffset = 1; yOffset < yDim; yOffset++)
                        {
                            int y;
                            if (dir == 0)//�st 
                            {
                                y = newY - yOffset;
                            }
                            else // alt
                            {
                                y = newY + yOffset;
                            }

                            if (y < 0 || y >= yDim) // grid s�n�rlar� d���nda ise 
                            {
                                break;
                            }

                            if (pieces[horizontalPieces[i].X, y].IsColored() && pieces[horizontalPieces[i].X, y].ColorComponent.Color == color) // e�er ayn� t�rden par�a ise 
                            {
                                verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
                            }
                            else // farkl� t�r ise 
                            {
                                break;
                            }

                        }
                    }

                    if (verticalPieces.Count<2)
                    {
                        verticalPieces.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < verticalPieces.Count; j++)
                        {
                            matchingPieces.Add (verticalPieces[j]);
                        }
                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
            #endregion

            // e�er yatay e�le�me bulunamad�ysa
            #region Dikey e�le�me kontrol�
            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);
            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;
                    if (dir == 0)//�st 
                    {
                        y = newY - yOffset;
                    }
                    else // alt
                    {
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim) // grid s�n�rlar� d���nda ise 
                    {
                        break;
                    }

                    if (pieces[newX, y].IsColored() && pieces[newX, y].ColorComponent.Color == color) // e�er ayn� t�rden par�a ise 
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else // farkl� t�r ise 
                    {
                        break;
                    }

                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            // T veya L �eklindeki e�le�me i�in yatay aray��a ge�i�

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int xOffset = 1; xOffset < xDim; xOffset++)
                        {
                            int x;
                            if (dir == 0)//sol 
                            {
                                x = newY - xOffset;
                            }
                            else // sa�
                            {
                                x = newY + xOffset;
                            }

                            if (x < 0 || x >= xDim) // grid s�n�rlar� d���nda ise 
                            {
                                break;
                            }

                            if (pieces[x,verticalPieces[i].Y].IsColored() && pieces[x, verticalPieces[i].Y].ColorComponent.Color == color) // e�er ayn� t�rden par�a ise 
                            {
                                horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
                            }
                            else // farkl� t�r ise 
                            {
                                break;
                            }

                        }
                    }

                    if (horizontalPieces.Count < 2)
                    {
                        horizontalPieces.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < horizontalPieces.Count; j++)
                        {
                            matchingPieces.Add(horizontalPieces[j]);
                        }
                        break;
                    }
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
            #endregion


            
        }

        return null; // e�er e�le�me yoksa
    }

    public bool ClearAllValidMatches()
    {
        bool needsRefill=false;
        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x,y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);

                    if (match != null) 
                    {
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;
                            }
                        }
                    }
                }
            }
        }

        return needsRefill;
    }

    public bool ClearPiece(int x, int y) 
    {
        if (pieces[x,y].IsClearable()&& !pieces[x,y].ClearableComponent.IsBeingCleared) // e�er par�a silinebilir ve silinmemi� ise
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);
            return true;

        }
        return false; 
    }

}
