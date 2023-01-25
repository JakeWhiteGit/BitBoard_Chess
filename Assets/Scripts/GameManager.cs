using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Board board;
    [SerializeField] BitBoards bitBoards;

    public Camera cam;
    public static GameManager StaticGameManager;
    public static bool PlayerColor;
    public static int[,] EnPassant;
    public static bool Turn;

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

        ChoosePlayerColor();
       // RotateBoard();
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

    public void RotateBoard()
    {
        float zRotation = PlayerColor ? 0f : 180f;
        cam.transform.rotation = new Quaternion(0, 0, zRotation, 0);
    }
}
