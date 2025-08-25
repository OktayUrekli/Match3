using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public enum PieceType // gride yerleþtirilebilecek obje tipleri
    {
        EMPTY, 
        NORMAL, // etkileþime geçilebilen boje
        BUBBLE, // engel
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOW,
        COUNT,
    };

    [System.Serializable]
    public struct PiecePrefab // gride yerleþtirilecek objelerin iç yapýsý
    {
        public PieceType type; 
        public GameObject prefab;
    }

    [System.Serializable]
    public struct PiecePosition
    {
        public PieceType type;
        public int x;
        public int y;
    }

    public int xDim; // geniþlik
    public int yDim; // yükseklik
    public float fillTime; // objelerin yukarýdan aþaðýya düþmesini animasyonlanlaþtýrmak için gerekli olan deðiþken

    public Level level;

    public PiecePrefab[] piecePrefabs; // yerleþtirlecek objeleri tutar
    public GameObject backgroundPrefab; // gridi oluþtururken kullanýlacak boje prefabý

    public PiecePosition[] initialPieces;

    Dictionary<PieceType, GameObject> piecePrefabDict;
    GamePiece[,] pieces; // oyun içinde oluþturulan objelerin listesini tutuyor
    
    bool inverse=false;

    GamePiece pressedPiece; // yer deðiþtirme için üzerine basýlan obje -- ilk konum
    GamePiece enteredPiece; // ilk konuma geçirilmesi istenen ikinci obje -- son konum

    bool gameOver=false;

    bool isFilling=false;
    public bool IsFilling  { get { return isFilling; } }

    void Awake()
    {

        piecePrefabDict = new Dictionary<PieceType, GameObject>();

        // yerleþtirilebilecek objeler dictionary ye kaydediliyor
        for (int i = 0; i < piecePrefabs.Length; i++) 
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type)) // eðer dictionary de bu tipte obje yoksa 
            {   // row column cleaner parçalarýnýn görsel deðiþimi özel olarak yapýlmalý
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab); // bu parçayý dictionary ye ekle
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

        pieces = new GamePiece[xDim, yDim];

        for (int i = 0; i < initialPieces.Length; i++)
        {
            if (initialPieces[i].x>=0 && initialPieces[i].x<xDim &&
                initialPieces[i].y >= 0 && initialPieces[i].y < yDim)
            {
                SpawnNewPiece(initialPieces[i].x, initialPieces[i].y, initialPieces[i].type);
            }

        }


        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x,y]==null)
                {
                    SpawnNewPiece(x, y, PieceType.EMPTY); // bütün grid boþ objeler ile dolduruluyor.
                }
            }
        }

        

        StartCoroutine(Fill()); // baþka objelerle doldurma iþlemi baþlýyor

    }

    public IEnumerator Fill()
    {
        bool needsRefill = true;
        isFilling = true;

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

        isFilling = false;
    }

    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = yDim - 2; y >= 0; y--) // en aþaðýdaki konum =yDim-1 => 8 -- bu konumdaki parça hareket edemeyeceði için bir üstteki parçadan kontrol etmeye baþlýyor
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;
                if (inverse)
                {
                    x = xDim - 1 - loopX; 
                }

                GamePiece piece = pieces[x, y];

                if (piece.IsMovable()) // parça hareket edebilen bir parça ise 
                {
                    GamePiece pieceBelow = pieces[x, y + 1]; // bir birim aþaðýsýndaki parça 
                    if (pieceBelow.Type == PieceType.EMPTY) // bir birim aþaðýsýndaki parça boþ ise 
                    {
                        Destroy(pieceBelow.gameObject); // boþ parçayý yok et
                        piece.MovableComponent.Move(x, y+1,fillTime); // hareket edebilen parçayý bir birim aþaðý kaydýr
                        pieces[x, y + 1] = piece; 
                        SpawnNewPiece(x, y, PieceType.EMPTY); // kayan parçanýn eski konumunu boþ olarak iþaretlemek için oraya yeni bir boþ parça oluþturuluyor
                        movedPiece = true;
                    }
                    else // eðer bir birim aþaðýdaki parça boþ deðil ise alt çaprazdaki parça kontrol edilecek.
                    {
                        for (int diag = 0; diag <= 1; diag++)
                        {
                            if (diag!=0)
                            {
                                int diagX = x + diag; // sað taraf

                                if (inverse)// inverse baþlangýçta false olduðu için önce sað alt kontrol edilir 
                                {
                                    diagX = x - diag; // sol taraf
                                }

                                if (diagX>=0 && diagX<xDim) // konum sýnýrlar içerisinde ise 
                                {
                                    GamePiece diagonalPiece = pieces[diagX, y + 1];  // diagX sað sol belirler -- sað/sol alt çaprazdaki parça alýnýyor
                                    if (diagonalPiece.Type==PieceType.EMPTY)  // alt çapraz konumu boþ ise 
                                    {
                                        bool hasPieceAbove = true;  
                                        for (int aboveY = y; aboveY >= 0; aboveY--)  
                                        {
                                            GamePiece pieceAbove = pieces[diagX,aboveY]; // bir birim yandaki obje kontrol ediliyor
                                            if (pieceAbove.IsMovable()) // eðer hareket edebilen bir parça ise 
                                            {
                                                break; // alt çaprazý doldurma görevi bu parçanýndýr.
                                            }
                                            else if (!pieceAbove.IsMovable() && pieceAbove.Type!=PieceType.EMPTY) // eðer hareket edemez ve boþ deðil ise yani engel ise 
                                            {
                                                hasPieceAbove = false; // boþ olan alt çapraz konumunun üstünde orayý doldurabilecek hareket edebilen bir obje yok olarak iþaretleniyor
                                                break;
                                            }
                                        }

                                        if (!hasPieceAbove) // eðer bir birim yukarýda boþ konumu doldurabilecek parça yok ise
                                        {
                                            Destroy(diagonalPiece.gameObject); // alt çaprazdaki boþ parça yok ediliyor
                                            piece.MovableComponent.Move(diagX,y+1,fillTime); 
                                            pieces[diagX, y + 1] = piece; // dolu parça boþ konuma getirilir
                                            SpawnNewPiece(x, y, PieceType.EMPTY); // dolu parçanýn eski konumu boþ olarak iþaretlenir
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
        for (int x = 0; x < xDim; x++) // satýrýn boþ olan konumlarý normal parçayla dolduruluyor
        {   // baþlangýçta tüm grid boþ olduðu için önce bir satýr hareket edebilen parça oluþturulacak sonra aþaðý kaydýrma iþlemi gerçekleþtirilecek 
            GamePiece pieceBelow = pieces[x, 0];
            if (pieceBelow.Type==PieceType.EMPTY)
            {
                Destroy(pieceBelow.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);

                newPiece.transform.parent=transform;
                
                pieces[x, 0]=newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                pieces[x, 0].MovableComponent.Move(x, 0,fillTime); // oluþturulan normla türdeki parça bir birim aþaðý kaydýrýlýyor
                pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors)); // oluþturulan parçanýn rengi rasgele olarak atanýyor
                movedPiece = true;
            }
        }

        return movedPiece;
    }
    
    public Vector2 GetWorldPosition(int x,int y)
    {
        return new Vector2(transform.position.x - xDim / 2.0f + x,
            transform.position.y + yDim / 2.0f - y
            );

        // aþaðýdaki kod 9*9 grid için yazýlmýþtý
        //return new Vector2(transform.position.x - 4 + x,
        //    transform.position.y + 4- y
        //    );
    }

    public GamePiece SpawnNewPiece(int x,int y,PieceType type) // istenilen konumda istenilen türde parça oluþturan metot
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type],GetWorldPosition(x,y),Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x,y]= newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x,y,this,type); // oluþturulan parçaya bilgileri atanýyor 

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
        if (gameOver)
        {
            return;
        }

        if (piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y]=piece2 ;
            pieces[piece2.X, piece2.Y] = piece1;

            if (GetMatch(piece1, piece2.X, piece2.Y) !=null || GetMatch(piece2, piece1.X, piece1.Y)!=null
               || piece1.Type==PieceType.RAINBOW || piece2.Type == PieceType.RAINBOW  ) // uygun eþleþme var ise yer deðiþtirme gerçekleþir 
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                if (piece1.Type== PieceType.RAINBOW && piece1.IsClearable() && piece2.IsColored())
                {
                    ClearColorPiece clearColor = piece1.GetComponent<ClearColorPiece>();
                    if (clearColor ) 
                    {
                        clearColor.Color=piece2.ColorComponent.Color;
                    }
                    ClearPiece(piece1.X , piece1.Y);
                }

                if (piece2.Type == PieceType.RAINBOW && piece2.IsClearable() && piece1.IsColored())
                {
                    ClearColorPiece clearColor = piece2.GetComponent<ClearColorPiece>();
                    if (clearColor)
                    {
                        clearColor.Color = piece1.ColorComponent.Color;
                    }
                    ClearPiece(piece2.X, piece2.Y);
                }

                ClearAllValidMatches();

                if (piece1.Type==PieceType.ROW_CLEAR || piece1.Type == PieceType.COLUMN_CLEAR)
                {
                    ClearPiece(piece1.X, piece1.Y);
                }

                if (piece2.Type == PieceType.ROW_CLEAR || piece2.Type == PieceType.COLUMN_CLEAR)
                {
                    ClearPiece(piece2.X, piece2.Y);
                }

                pressedPiece = null;
                enteredPiece = null;

                StartCoroutine(Fill());
                level.OnMove();
            }
            else // uygun eþleþme yok ise parçalar eski konumuna getirilir
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
            ColorPiece.ColorType color = piece.ColorComponent.Color; // parçanýn türü alýnýyor
            List<GamePiece> horizontalPieces= new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // önce yatay eþleþme kotrol ediliyor
            #region Yatay eþleþme kontrolü
            horizontalPieces.Add(piece); 
            for (int dir = 0; dir <= 1; dir++) // yatayda arama yapýlacak 0=sol arama indexi  1=sað arama indexi
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;
                    if (dir==0)//sol
                    {
                        x = newX - xOffset;
                    }
                    else // sað
                    {
                        x= newX + xOffset;
                    }

                    if (x<0 || x>=xDim) // grid sýnýrlarý dýþýnda ise 
                    {
                        break;
                    }

                    if (pieces[x, newY].IsColored() && pieces[x, newY].ColorComponent.Color == color) // eðer ayný türden parça ise 
                    {
                        horizontalPieces.Add(pieces[x, newY]); 
                    }
                    else // farklý tür ise 
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

            // T veya L þeklindeki eþleþme için dikey arayýþa geçiþ
            if (horizontalPieces.Count >= 3) 
            {
                for (int i = 0; i < horizontalPieces.Count; i++)  // ayný türde olan tüm parçalarýn aþaðý ve yukarýsýndaki parçalar da kontrol ediliyor 
                { 
                    for (int dir = 0; dir <= 1; dir++) // üst alt arama için index
                    {
                        for (int yOffset = 1; yOffset < yDim; yOffset++)
                        {
                            int y;
                            if (dir == 0)//üst 
                            {
                                y = newY - yOffset;
                            }
                            else // alt
                            {
                                y = newY + yOffset;
                            }

                            if (y < 0 || y >= yDim) // grid sýnýrlarý dýþýnda ise 
                            {
                                break;
                            }

                            if (pieces[horizontalPieces[i].X, y].IsColored() && pieces[horizontalPieces[i].X, y].ColorComponent.Color == color) // eðer ayný türden parça ise 
                            {
                                verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
                            }
                            else // farklý tür ise 
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

            // eðer yatay eþleþme bulunamadýysa
            #region Dikey eþleþme kontrolü
            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);
            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;
                    if (dir == 0)//üst 
                    {
                        y = newY - yOffset;
                    }
                    else // alt
                    {
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim) // grid sýnýrlarý dýþýnda ise 
                    {
                        break;
                    }

                    if (pieces[newX, y].IsColored() && pieces[newX, y].ColorComponent.Color == color) // eðer ayný türden parça ise 
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else // farklý tür ise 
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

            // T veya L þeklindeki eþleþme için yatay arayýþa geçiþ

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
                            else // sað
                            {
                                x = newY + xOffset;
                            }

                            if (x < 0 || x >= xDim) // grid sýnýrlarý dýþýnda ise 
                            {
                                break;
                            }

                            if (pieces[x,verticalPieces[i].Y].IsColored() && pieces[x, verticalPieces[i].Y].ColorComponent.Color == color) // eðer ayný türden parça ise 
                            {
                                horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
                            }
                            else // farklý tür ise 
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

        return null; // eðer eþleþme yoksa
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
                        PieceType specialPieceType = PieceType.COUNT;
                        GamePiece randomPiece = match[Random.Range(0,match.Count)];
                        int specialPieceX=randomPiece.X;
                        int specialPieceY=randomPiece.Y;

                        if (match.Count==4)
                        {
                            if (pressedPiece==null || enteredPiece== null)
                            {
                                specialPieceType = (PieceType)Random.Range((int)PieceType.ROW_CLEAR, (int)PieceType.COLUMN_CLEAR);
                            }
                            else if (pressedPiece.Y== enteredPiece.Y)
                            {
                                specialPieceType = PieceType.ROW_CLEAR;
                            }
                            else
                            {
                                specialPieceType = PieceType.COLUMN_CLEAR;
                            }
                        }
                        else if (match.Count >= 5)
                        {
                            specialPieceType = PieceType.RAINBOW;
                        }



                        for (int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;
                                if (match[i]==pressedPiece || match[i]==enteredPiece)
                                {
                                    specialPieceX = match[i].X;
                                    specialPieceY = match[i].Y;
                                }

                            }
                        }

                        if (specialPieceType!=PieceType.COUNT)
                        {
                            Destroy(pieces[specialPieceX, specialPieceY]);
                            GamePiece newPiece=SpawnNewPiece(specialPieceX,specialPieceY,specialPieceType);
                            if ((specialPieceType==PieceType.ROW_CLEAR|| specialPieceType== PieceType.COLUMN_CLEAR)
                                && newPiece.IsColored() && match[0].IsColored())
                            {
                                newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                            }
                            else if (specialPieceType == PieceType.RAINBOW && newPiece.IsColored())
                            {
                                newPiece.ColorComponent.SetColor(ColorPiece.ColorType.ANY);
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
        if (pieces[x,y].IsClearable()&& !pieces[x,y].ClearableComponent.IsBeingCleared) // eðer parça silinebilir ve silinmemiþ ise
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);
            ClearObstacle(x, y);
            return true;

        }
        return false; 
    }

    public void ClearObstacle(int x, int y) 
    {
        for (int adjacentX=x-1; adjacentX<=x+1;adjacentX++)
        {
            if (adjacentX!=x && adjacentX>0 && adjacentX< xDim)
            {
                if (pieces[adjacentX,y].Type==PieceType.BUBBLE && pieces[adjacentX,y].IsClearable())
                {
                    pieces[adjacentX, y].ClearableComponent.Clear();
                    SpawnNewPiece(adjacentX, y,PieceType.EMPTY);
                }
            }
        }

        for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
        {
            if (adjacentY != y && adjacentY > 0 && adjacentY < yDim)
            {
                if (pieces[x, adjacentY].Type == PieceType.BUBBLE && pieces[x,adjacentY].IsClearable())
                {
                    pieces[x, adjacentY].ClearableComponent.Clear();
                    SpawnNewPiece(x,adjacentY, PieceType.EMPTY);
                }
            }
        }
    }

    public void ClearRow(int row)
    {
        for (int x = 0; x < xDim; x++)
        {
            ClearPiece(x, row);
        }
    }

    public void ClearColumn(int column)
    {
        for (int y = 0; y < yDim; y++)
        {
            ClearPiece(column,y);
        }
    }

    public void ClearColor(ColorPiece.ColorType color)
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x,y].IsColored()&& pieces[x,y].ColorComponent.Color==color
                   || color==ColorPiece.ColorType.ANY )
                {
                    ClearPiece(x,y);
                }
            }
        }
    }

    public void GameOver()
    {
        gameOver = true;
    }

    public List<GamePiece> GetPiecesOfType(PieceType type)
    {
        List<GamePiece> piecesOfType= new List<GamePiece>();
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x,y].Type==type)
                {
                    piecesOfType.Add(pieces[x, y]);
                }
            }
        }

        return piecesOfType;
    }

}
