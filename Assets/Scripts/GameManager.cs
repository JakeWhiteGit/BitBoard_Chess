using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager StaticGameManager;

    [SerializeField] Board board;
    [SerializeField] BitBoards bitBoards;

    public Camera cam;
    public static bool PlayerColor;
    public static int[] EnPassant;
    public static bool IsWhitesTurn;

    public static bool PieceSelected = false;
    public static int PieceSelectedType = -1;
    public static int PieceSelectedColor = -1;
    public static int TileSelected = -1;

    void Awake()
    {
        //ensures only one instance of this class exists
        if(StaticGameManager != null)
        {
            GameObject.Destroy(StaticGameManager);
        }
        else
        {
            StaticGameManager = this;
        }
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        InitializeBitBoards();

        InitializeBoardGUI();

      //bitBoards.StoreMoves();

        ChoosePlayerColor();
       // RotateCamera();
    }
    public void MakeMove(int squareTo, int squareFrom, int pieceType, int pieceColor)
    {
        Debug.Log("makemove");
        bitBoards.BoardState[pieceType] = bitBoards.RemoveBit(bitBoards.BoardState[pieceType], squareFrom);
        bitBoards.BoardState[pieceColor] = bitBoards.RemoveBit(bitBoards.BoardState[pieceColor], squareFrom);

        for (int i = 0; i < 2; i++)
        {
            bitBoards.BoardState[i] = bitBoards.RemoveBit(bitBoards.BoardState[i], squareFrom);
        }
        for (int i = 2; i < 8; i++)
        {
            bitBoards.BoardState[i] = bitBoards.RemoveBit(bitBoards.BoardState[i], squareFrom);
        }

        bitBoards.BoardState[pieceType] = bitBoards.SetBit(bitBoards.BoardState[pieceType], squareTo);
        bitBoards.BoardState[pieceColor] = bitBoards.SetBit(bitBoards.BoardState[pieceColor], squareTo);



        foreach (var piece in board.Pieces)
        {
            Destroy(piece);
        }
        
        board.PlacePieces(true);
        board.PlacePieces(false);

        ClearLegalMoves();
    }

    public void ClearLegalMoves()
    {
        for (int i = 0; i < 64; i++)
        {
            board.Tiles[i].selected = false;
        }
        PieceSelectedType = -1;
        PieceSelectedColor = -1;
    }

    private void InitializeBoardGUI()
    {
        board.GenerateBoard();
        board.PlacePieces(true);
        board.PlacePieces(false);
    }

    private void InitializeBitBoards()
    {
        bitBoards.SetBitBoardsFromFen();
    /*
        bitBoards.InitializePawnAttacks();
        bitBoards.InitializeKnightAttacks();
        bitBoards.InitializeKingAttacks();
        bitBoards.InitializeBishopAttacks();
        bitBoards.InitializeRookAttacks();
        bitBoards.InitializeQueenAttacks();
    */
    }

    private void ChoosePlayerColor()
    {
        float value = Random.Range(0.0f, 1.0f);
        if(value > 0.5f)
        {
            PlayerColor = true;
        }
        else
        {
            PlayerColor = false;
        }
    }

    public void RotateCamera()
    {
        float zRotation = PlayerColor ? 0f : 180f;
        cam.transform.rotation = new Quaternion(0, 0, zRotation, 0);
    }
}
