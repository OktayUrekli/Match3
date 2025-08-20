using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public enum PieceType // gride yerleþtirilebilecek obje tipleri
    {
        EMPTY, 
        NORMAL, // etkileþime geçilebilen boje
        BUBBLE, // engel
        COUNT,
    };

    [System.Serializable]
    public struct PiecePrefab // gride yerleþtirilecek objelerin iç yapýsý
    {
        public PieceType type; 
        public GameObject prefab;
    }

    public int xDim; // geniþlik
    public int yDim; // yükseklik
    public float fillTime; // objelerin yukarýdan aþaðýya düþmesini animasyonlanlaþtýrmak için gerekli olan deðiþken

    public PiecePrefab[] piecePrefabs; // yerleþtirlecek objeleri tutar
    public GameObject backgroundPrefab; // gridi oluþtururken kullanýlacak boje prefabý

    Dictionary<PieceType, GameObject> piecePrefabDict;
    GamePiece[,] pieces; // oyun içinde oluþturulan objelerin listesini tutuyor
    
    bool inverse=false;

    GamePiece pressedPiece; // yer deðiþtirme için üzerine basýlan obje -- ilk konum
    GamePiece enteredPiece; // ilk konuma geçirilmesi istenen ikinci obje -- son konum

    void Start()
    {

        piecePrefabDict = new Dictionary<PieceType, GameObject>();

        // yerleþtirilebilecek objeler dictionary ye kaydediliyor
        for (int i = 0; i < piecePrefabs.Length; i++) 
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type)) // eðer dictionary de bu tipte obje yoksa 
            {
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

        pieces=new GamePiece[xDim,yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnNewPiece(x, y,PieceType.EMPTY); // bütün grid boþ objeler ile dolduruluyor.
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

        // geçici olarak gride engeller eklendi
        Destroy(pieces[4, 4].gameObject);
        SpawnNewPiece(4, 4,PieceType.BUBBLE);


        Destroy(pieces[1, 3].gameObject);
        SpawnNewPiece(1, 3, PieceType.BUBBLE);


        Destroy(pieces[6, 4].gameObject);
        SpawnNewPiece(6, 4, PieceType.BUBBLE);

        Destroy(pieces[3, 7].gameObject);
        SpawnNewPiece(3, 7, PieceType.BUBBLE);

        StartCoroutine(Fill()); // baþka objelerle doldurma iþlemi baþlýyor

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
        //return new Vector2 (transform.position.x-xDim/2.0f+x,
        //    transform.position.y+yDim/2.0f-y
        //    );
        return new Vector2(transform.position.x - 4 + x,
            transform.position.y + 4- y
            );
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
        if (piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y]=piece2 ;
            pieces[piece2.X, piece2.Y] = piece1;

            if (GetMatch(piece1, piece2.X, piece2.Y) !=null || GetMatch(piece2, piece1.X, piece1.Y)!=null) // uygun eþleþme var ise yer deðiþtirme gerçekleþir 
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();
                StartCoroutine(Fill());
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
        if (pieces[x,y].IsClearable()&& !pieces[x,y].ClearableComponent.IsBeingCleared) // eðer parça silinebilir ve silinmemiþ ise
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);
            return true;

        }
        return false; 
    }

}
