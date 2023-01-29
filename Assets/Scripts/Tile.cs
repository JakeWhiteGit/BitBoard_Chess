using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int location;

    SpriteRenderer spriteRenderer;
    Color defaultColor;

    public bool selected;

    ulong legalMoves;
    [SerializeField] BitBoards bitBoards;
    [SerializeField] Board board;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bitBoards = FindObjectOfType<BitBoards>();
        board = FindObjectOfType<Board>();
    }
    void Start()
    {
        defaultColor = spriteRenderer.color;
    }
    void Update()
    {
        if(selected is true)
        {
            spriteRenderer.color = Color.green;
        }
        else 
        {
            spriteRenderer.color = defaultColor;
        }
    }
    void OnMouseDown()
    {
        if (selected && GameManager.PieceSelected)
        {
            GameManager.StaticGameManager.MakeMove(location,
                                                    GameManager.TileSelected,
                                                    GameManager.PieceSelectedType,
                                                    GameManager.PieceSelectedColor );
        }
        else
        {
            GameManager.PieceSelected = false;
            GameManager.StaticGameManager.ClearLegalMoves();
            HighlightLegalMoves();
        }
    }
    private void HighlightLegalMoves()
    {
        //if there is a tile under the cursor return
        GameManager.TileSelected = location;

        if (!GameManager.PieceSelected)
        {
            GameManager.PieceSelected = true;
            //get all legal moves for tile index if any
            GameManager.PieceSelectedType = bitBoards.GetPieceType(GameManager.TileSelected);
            GameManager.PieceSelectedColor = bitBoards.GetPieceColor(GameManager.TileSelected);
            legalMoves = bitBoards.CalculateSelectedMove(GameManager.TileSelected, GameManager.PieceSelectedType, GameManager.PieceSelectedColor);

            //pull every individual legal move and put into array
            int[] legalMoveIndices = bitBoards.ReturnAllBitIndices(legalMoves);

            for (int i = 0; i < legalMoveIndices.Length; i++)
            {
                board.Tiles[legalMoveIndices[i]].selected = true;
            }
        }

    }
}
 