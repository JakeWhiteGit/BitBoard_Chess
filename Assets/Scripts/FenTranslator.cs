using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenTranslator : MonoBehaviour
{
    //fen for default chess board start
    [SerializeField] string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
   
    public static FenTranslator StaticFenTranslator;

    public static string PiecePositions;
    public static string ActiveTurn;
    public static string CastlingAvailable;
    public static string EnpassantTile;
    public static string HalfTurns;
    public static string WholeTurns;

    string[] fenPartitions;

    void Awake()
    {
        //fen = GenerateRandomFen();
        Debug.Log($"FEN: '{fen}'");

        //ensures only one instance of this class exists
        if (StaticFenTranslator != null)
        {
            GameObject.Destroy(StaticFenTranslator);
        }
        else
        {
            StaticFenTranslator = this;
        }
        DontDestroyOnLoad(this);

        fenPartitions = fen.Split(" ");
        PiecePositions = fenPartitions[0];
        ActiveTurn = fenPartitions[1];
        CastlingAvailable = fenPartitions[2];
        EnpassantTile = fenPartitions[3];
        HalfTurns = fenPartitions[4];
        WholeTurns = fenPartitions[5];

        ParseFenTurn();
    }

    void Start()
    {
    }

    void ParseFenTurn()
    {
        if (fenPartitions[1] == "w")
        {
            GameManager.IsWhitesTurn = true;
        }
        else
        {
            GameManager.IsWhitesTurn = false;
        }
    }

    public string GenerateRandomFen()
    {

        char[] pieces = { 'K', 'Q', 'R', 'B', 'N', 'P', 'k', 'q', 'r', 'b', 'n', 'p' };

        // Create an empty FEN string
        string fen = "";
        int spaceCount = 0;

        // Loop through each square on the board
        for (int i = 0; i < 64; i++)
        {
            // Randomly select a piece or an empty square
            char piece = (Random.value < 0.75f) ? '1' : pieces[Random.Range(0, pieces.Length)];

            if (piece == '1')
            {
                spaceCount++;
            }
            else
            {
                //if we have 1s to add to the fen
                if (spaceCount > 0)
                {
                    fen += spaceCount.ToString();
                    spaceCount = 0;
                }
                fen += piece;
            }

            // Add a slash after every 8th square
            if ((i + 1) % 8 == 0 && i != 63)
            {
                if (spaceCount > 0)
                {
                    fen += spaceCount.ToString();
                    spaceCount = 0;
                }
                fen += '/';
            }
        }

        // Add the remaining parts of the FEN string
        fen += " w - - 0 1";

        return fen;
    }
}
