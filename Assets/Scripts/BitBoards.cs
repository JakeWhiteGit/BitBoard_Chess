using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Burst.Intrinsics;
using Unity.Burst;

public class BitBoards : MonoBehaviour
{
    // creating singleton for monobehaviour functionality in the unity editor
    public static BitBoards StaticBitBoards;

    //saves last tile a player clicked as a bitboard
    public static ulong PlayerTileSelection = 0L;

    //see pieces enum
    public static ulong[] BoardState =
    {
        0, //whites bitboard
        0, //blacks bitboard
        0, //bishops bitboard
        0, //kings bitboard
        0, //knights bitboard
        0, //pawns bitboard
        0, //queens bitboard
        0  //rooks bitboard
    };

    //caching the capturable squares of every piece type from every square on the board
    //to hopefully speed up move generation for minimax
    public static ulong[,] PawnAttacks;
    public static ulong[] KnightAttacks;
    public static ulong[] KingAttacks;
    public static ulong[] BishopAttacks;
    public static ulong[] RookAttacks;
    public static ulong[] QueenAttacks;

    public static List<ulong> WhiteMoves;
    public static List<ulong> BlackMoves;

    //constants to show a fully flipped board apart from the specified columns
    //used for checking if a move from one side wraps over to the other on the visual bitboard
    public readonly static ulong AColumn = 18374403900871474942L;
    public readonly static ulong HColumn = 9187201950435737471L;
    public readonly static ulong ABColumn = 18229723555195321596L;
    public readonly static ulong GHColumn = 4557430888798830399L;

    //value to shift bit with
    readonly ulong shiftOn = 1L;

    enum Squares
    {
        a8, b8, c8, d8, e8, f8, g8, h8,
        a7, b7, c7, d7, e7, f7, g7, h7,
        a6, b6, c6, d6, e6, f6, g6, h6,
        a5, b5, c5, d5, e5, f5, g5, h5,
        a4, b4, c4, d4, e4, f4, g4, h4,
        a3, b3, c3, d3, e3, f3, g3, h3,
        a2, b2, c2, d2, e2, f2, g2, h2,
        a1, b1, c1, d1, e1, f1, g1, h1
    }

    enum PieceColor
    {
        white, black
    }

    public enum Pieces
    {
        whites,
        blacks,
        bishops,
        kings,
        knights,
        pawns,
        queens,
        rooks
    }

    void Awake()
    {
        //ensures only one instance of this class exists
        if (StaticBitBoards != null)
        {
            GameObject.Destroy(StaticBitBoards);
        }
        else
        {
            StaticBitBoards = this;
        }
        DontDestroyOnLoad(this);

        WhiteMoves = new List<ulong>();
        BlackMoves = new List<ulong>();
    }

    void Start()
    {
    }

    public void StoreMoves()
    {
        for (int i = 0; i < 64; i++)
        {
            bool IsWhite = GetPieceColor(i) == 0;

            ulong move = CalculateSelectedMove(i, GetPieceType(i), GetPieceColor(i));

            if (move != 0 && IsWhite)
            {
                WhiteMoves.Add(move);
            }
            if(move != 0 && !IsWhite)
            {
                BlackMoves.Add(move);
            }
        }

        int count = 0;
        int totalMoves = 0;
        foreach (var move in WhiteMoves)
        {
            count++;
            PrintBitBoard(move, $"{count} white");
            totalMoves += BitCounter(move);
        }
        count = 0;
        foreach (var move in BlackMoves)
        {
            count++;
            PrintBitBoard(move, $"{count} black");
            totalMoves += BitCounter(move);
        }
        //Debug.Log($"{totalMoves} total moves");
    }

    public ulong CalculateSelectedMove(int square, int pieceType, int pieceColor)
    {
        bool isWhite = pieceColor == (int)Pieces.whites ? true : false;
        ulong teamOccupancy = isWhite ? BoardState[(int)Pieces.whites] : BoardState[(int)Pieces.blacks];
        ulong enemyOccupancy = isWhite ? BoardState[(int)Pieces.blacks] : BoardState[(int)Pieces.whites];

        if (pieceType == (int)Pieces.pawns)
           return CalculatePawnAttacks(square, isWhite, teamOccupancy, enemyOccupancy);

        if (pieceType == (int)Pieces.bishops)
            return CalculateBishopAttacks(square, teamOccupancy, enemyOccupancy);

        if (pieceType == (int)Pieces.queens)
            return CalculateQueenAttacks(square, teamOccupancy, enemyOccupancy);

        if (pieceType == (int)Pieces.kings)
            return CalculateKingAttacks(square, teamOccupancy, enemyOccupancy);

        if (pieceType == (int)Pieces.knights)
            return CalculateKnightAttacks(square, teamOccupancy, enemyOccupancy);

        if (pieceType == (int)Pieces.rooks)
            return CalculateRookAttacks(square, teamOccupancy, enemyOccupancy);

        return 0;
    }

    public int GetPieceType(int square)
    {
        int pieceType = 0;

        for (int i = 2; i < 8; i++)
        {
            if(GetBit(BoardState[i], square))
            {
                pieceType = i;
            }
        }

        return pieceType;
    }
    public int GetPieceColor(int square)
    {
        int pieceColor = 0;

        for (int i = 0; i < 2; i++)
        {
            if (GetBit(BoardState[i], square))
            {
                pieceColor = i;
            }
        }

        return pieceColor;
    }

    #region BitOperations

    //returns value of a bit at the location of square
    public bool GetBit(ulong bitBoard, int square)
        {
            return ((shiftOn << square) & bitBoard) != 0;
        }

        //sets bit to 1 at the location of square
        public ulong SetBit(ulong bitBoard, int square)
        {
            return bitBoard |= (shiftOn << square);
        }

        //sets bit to 0 at the location of square, checks if value is already 0 to stop it just flipping
        public ulong RemoveBit(ulong bitBoard, int square)
        {
            //                   is bit at square on?                  turn off         do nothing
            return bitBoard ^= ((shiftOn << square) & bitBoard) != 0 ? (shiftOn << square) : 0;
        }

        // returns the number of on bits on a bitboard Brian Kernighan's algorithm
        public int BitCounter(ulong bitBoard)
        {
            int count = 0;
            
            //discard least significant bit until bitboard is cleared 
            while (bitBoard != 0)
            {
                count++;
                bitBoard &= bitBoard - 1;
            }

            return count;
        }

        //returns the index of the least significant bit on the bitboard
        public int ReturnFirstBitIndex(ulong bitBoard)
        {
            int index = 0;

            // every bit is already off no need to waste time
            if (bitBoard == 0) { return -1; }

            //loop until we hit an on bit
            while ((bitBoard & shiftOn) == 0)
            {
                // shifts the whole bitboard by one digit at a time
                //increment index to count how many times until we hit 1
                bitBoard >>= 1;
                index++;
            }

            return index;
        }
        //returns the indeces of all 1 bits on the bitboard
        public int[] ReturnAllBitIndices(ulong bitBoard)
        {
            List<int> indices = new List<int>();

            for (int i = 0; i < 64; i++)
            {
                //if bit is on at the index of i
                if ((bitBoard & (shiftOn << i)) != 0)
                {
                    indices.Add(i);
                }
            }

            return indices.ToArray();
        }

        #endregion

    #region tests

        //gui for representing the bitboard in a human friendly way, purely for debugging and visualisation
        public void PrintBitBoard(ulong bitBoard, string name)
        {
            string[] texts = new string[64];
            string bitBoardUI = null;
            int fileCount = 0;
            int rowCount = 9;

            bitBoardUI += string.Format("{0:d}\t", $"{name}");
            bitBoardUI += Environment.NewLine;
            bitBoardUI += Environment.NewLine;
            bitBoardUI += string.Format("{0:d}\t", "");

            for (int square = 0; square < 64; square++)
            {
                //creates string of the binary value of the bitboard ulong
                if (GetBit(bitBoard, square) is true)
                {
                    texts[square] = string.Format("{0:d}\t", "1");
                }
                else
                {
                    texts[square] = string.Format("{0:d}\t", "0");
                }


                fileCount++;
                bitBoardUI += texts[square];

                //adds new line
                if (fileCount == 8)
                {
                    rowCount--;
                    bitBoardUI += string.Format("{0:d}\t", $"|  {rowCount}");
                    bitBoardUI += Environment.NewLine;
                    bitBoardUI += string.Format("{0:d}\t", $"                                                                                                                                  |");
                    bitBoardUI += Environment.NewLine;
                    bitBoardUI += string.Format("{0:d}\t", "");
                    fileCount = 0;
                }

            }

            bitBoardUI += string.Format("{0:d}\t", "____________________________________________________________________");
            bitBoardUI += Environment.NewLine;
            bitBoardUI += string.Format("{0:d}\t", "              A");
            bitBoardUI += string.Format("{0:d}\t", "B");
            bitBoardUI += string.Format("{0:d}\t", "C");
            bitBoardUI += string.Format("{0:d}\t", "D");
            bitBoardUI += string.Format("{0:d}\t", "E");
            bitBoardUI += string.Format("{0:d}\t", "F");
            bitBoardUI += string.Format("{0:d}\t", "G");
            bitBoardUI += string.Format("{0:d}\t", "H");
            bitBoardUI += Environment.NewLine;
            bitBoardUI += Environment.NewLine;
            bitBoardUI += string.Format("{0:d}\t", $"Bitboard decimal value -{bitBoard}-");
            bitBoardUI += Environment.NewLine;

            //prints bitboard to unity's console window
            Debug.Log($"{bitBoardUI}");
        }
        void TestReturnFirstBitSpeed(ulong[] bitBoards)
        {
            float startTime = Time.realtimeSinceStartup;
            int count = 0;

            for (int j = 0; j < bitBoards.Length; j++)
            {
                ReturnFirstBitIndex(bitBoards[j]);
                count++;
            }

            float endTime = Time.realtimeSinceStartup;
            float elapsedTime = endTime - startTime;

            Debug.Log($"Elapsed time in ms to return LSB {count} times: {elapsedTime}");
        }

        void TestBitCounterSpeed(ulong[] bitBoards)
        {
            float startTime = Time.realtimeSinceStartup;
            int count = 0;

            for (int j = 0; j < bitBoards.Length; j++)
            {
                BitCounter(bitBoards[j]);
                count++;
            }

            float endTime = Time.realtimeSinceStartup;
            float elapsedTime = endTime - startTime;

            Debug.Log($"Elapsed time in ms to count bits in a bitboard {count} times: {elapsedTime}");
        }

        ulong[] GenerateRandomULongs(int length)
        {
            System.Random rand = new System.Random();
            ulong[] array = new ulong[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = (ulong)rand.Next() | ((ulong)rand.Next() << 32);
            }

            return array;
        }
    #endregion

    #region initialize bitboards

    //loops over every character in the fen string and updates the relevant bitboard to contain a piece
    public void SetBitBoardsFromFen()
        {
            char[] fenLayout = FenTranslator.PiecePositions.ToCharArray();
            int square = 0;

            for (int i = 0; i < fenLayout.Length; i++)
            {
                switch (fenLayout[i])
                {
                    case 'b':
                    BoardState[(int)Pieces.blacks] |= shiftOn << square;
                    BoardState[(int)Pieces.bishops] |= shiftOn << square;
                    square++;
                        break;

                    case 'k':
                    BoardState[(int)Pieces.blacks] |= shiftOn << square;
                    BoardState[(int)Pieces.kings] |= shiftOn << square;
                    square++;
                        break;

                    case 'n':
                    BoardState[(int)Pieces.blacks] |= shiftOn << square;
                    BoardState[(int)Pieces.knights] |= shiftOn << square;
                    square++;
                        break;

                    case 'p':
                    BoardState[(int)Pieces.blacks] |= shiftOn << square;
                    BoardState[(int)Pieces.pawns] |= shiftOn << square;
                    square++;
                        break;

                    case 'q':
                    BoardState[(int)Pieces.blacks] |= shiftOn << square;
                    BoardState[(int)Pieces.queens] |= shiftOn << square;
                    square++;
                        break;

                    case 'r':
                    BoardState[(int)Pieces.blacks] |= shiftOn << square;
                    BoardState[(int)Pieces.rooks] |= shiftOn << square;
                    square++;
                        break;

                    case 'B':
                    BoardState[(int)Pieces.whites] |= shiftOn << square;
                    BoardState[(int)Pieces.bishops] |= shiftOn << square;
                    square++;
                        break;

                    case 'K':
                    BoardState[(int)Pieces.whites] |= shiftOn << square;
                    BoardState[(int)Pieces.kings] |= shiftOn << square;
                    square++;
                        break;

                    case 'N':
                    BoardState[(int)Pieces.whites] |= shiftOn << square;
                    BoardState[(int)Pieces.knights] |= shiftOn << square;
                    square++;
                        break;

                    case 'P':
                    BoardState[(int)Pieces.whites] |= shiftOn << square;
                    BoardState[(int)Pieces.pawns] |= shiftOn << square;
                    square++;
                        break;

                    case 'Q':
                    BoardState[(int)Pieces.whites] |= shiftOn << square;
                    BoardState[(int)Pieces.queens] |= shiftOn << square;
                    square++;
                        break;

                    case 'R':
                    BoardState[(int)Pieces.whites] |= shiftOn << square;
                    BoardState[(int)Pieces.rooks] |= shiftOn << square;
                    square++;
                        break;

                    case '1':
                        square += 1;
                        break;

                    case '2':
                        square += 2;
                        break;

                    case '3':
                        square += 3;
                        break;

                    case '4':
                        square += 4;
                        break;

                    case '5':
                        square += 5;
                        break;

                    case '6':
                        square += 6;
                        break;

                    case '7':
                        square += 7;
                        break;

                    case '8':
                        square += 8;
                        break;

                    case '/':
                        break;

                    default:
                    Debug.Log("invalid fen");
                        break;
                }
            }
        }
    
    /*
    public void InitializeKingAttacks()
        {
            for (int square = 0; square < 64; square++)
            {
                KingAttacks[square] = CalculateKingAttacks(square);
            }
        }
    public void InitializePawnAttacks()
        {
            for (int square = 0; square < 64; square++)
            {
                PawnAttacks[(int)PieceColor.white, square] = CalculatePawnAttacks(true, square);
                PawnAttacks[(int)PieceColor.black, square] = CalculatePawnAttacks(false, square);
            }
        }
    public void InitializeKnightAttacks()
        {
            for (int square = 0; square < 64; square++)
            {
                KnightAttacks[square] = CalculateKnightAttacks(square);
            }
        }
    public void InitializeBishopAttacks()
        {
            for (int square = 0; square < 64; square++)
            {
                BishopAttacks[square] = CalculateBishopAttacks(square);
            }
        }
    public void InitializeRookAttacks()
        {
            for (int square = 0; square < 64; square++)
            {
                RookAttacks[square] = CalculateRookAttacks(square, SetBlockedSquares();
            }
        }
    public void InitializeQueenAttacks()
        {
            for (int square = 0; square < 64; square++)
            {
                QueenAttacks[square] |= RookAttacks[square];
                QueenAttacks[square] |= BishopAttacks[square];
            }
        }
    */

    #endregion

    #region calculate attack maps
    ulong CalculateKingAttacks(int square, ulong teamOccupancy, ulong enemyOccupancy)
    {
        ulong attacks = 0L;

        ulong kingLocation = 0L;
        kingLocation = SetBit(kingLocation, square);

        if ((((kingLocation >> 8)) != 0) && (((kingLocation >> 8) & teamOccupancy) == 0)) 
            attacks |= (kingLocation >> 8);
        if ((((kingLocation >> 7) & AColumn) != 0 && (((kingLocation >> 7) & teamOccupancy) == 0)))
            attacks |= (kingLocation >> 7);
        if ((((kingLocation >> 1) & HColumn) != 0 && (((kingLocation >> 1) & teamOccupancy) == 0)))
            attacks |= (kingLocation >> 1); 
        if ((((kingLocation >> 9) & HColumn) != 0 && (((kingLocation >> 9) & teamOccupancy) == 0)))
            attacks |= (kingLocation >> 9);
        if ((((kingLocation << 8)) != 0) && (((kingLocation << 8) & teamOccupancy) == 0))
            attacks |= (kingLocation << 8);
        if ((((kingLocation << 7) & HColumn) != 0 && (((kingLocation << 7) & teamOccupancy) == 0))) 
            attacks |= (kingLocation << 7);
        if ((((kingLocation << 1) & AColumn) != 0 && (((kingLocation << 1) & teamOccupancy) == 0))) 
            attacks |= (kingLocation << 1);
        if ((((kingLocation << 9) & AColumn) != 0 && (((kingLocation << 9) & teamOccupancy) == 0))) 
            attacks |= (kingLocation << 9);

            return attacks;
    }
    ulong CalculatePawnAttacks(int square, bool isWhite, ulong teamOccupancy, ulong enemyOccupancy)
    {
        ulong attacks = 0L;

        //location to calculate attack from
        ulong pawnLocation = 0L;
        pawnLocation = SetBit(pawnLocation, square);

        //if white pawns
        if (isWhite is true)
        {
            if ((((pawnLocation >> 8)) != 0) &&
                (((pawnLocation >> 8) & teamOccupancy) == 0) &&
                (((pawnLocation >> 8) & enemyOccupancy) == 0))
                attacks |= (pawnLocation >> 8);

            //  attack is on board AND attack has no team piece AND no enemy piece AND is on rank 7
            if ((((pawnLocation >> 16)) != 0) &&
                (((pawnLocation >> 16) & teamOccupancy) == 0) &&
                (((pawnLocation >> 16) & enemyOccupancy) == 0) &&
                square / 8 == 6)
                //set attack on at square
                attacks |= (pawnLocation >> 16);

            //--Captures--
            //additional check from above to ensure diagonal moves dont wrap around the board
            if ((((pawnLocation >> 7) & AColumn) != 0) && (((pawnLocation >> 7) & enemyOccupancy) != 0))
                attacks |= (pawnLocation >> 7);
            if ((((pawnLocation >> 9) & HColumn) != 0) && (((pawnLocation >> 9) & enemyOccupancy) != 0))
                attacks |= (pawnLocation >> 9);
        }

        //if black pawns
        else
        {
            if ((((pawnLocation << 8)) != 0) &&
                (((pawnLocation << 8) & teamOccupancy) == 0) &&
                (((pawnLocation << 8) & enemyOccupancy) == 0)) 
                attacks |= (pawnLocation << 8);

            if ((((pawnLocation << 16)) != 0) &&
                (((pawnLocation << 16) & teamOccupancy) == 0) &&
                (((pawnLocation << 16) & enemyOccupancy) == 0)
                && square / 8 == 1)
                attacks |= (pawnLocation << 16);
            
            //--Captures--
            if ((((pawnLocation << 7) & HColumn) != 0) && (((pawnLocation << 7) & enemyOccupancy) != 0)) 
                attacks |= (pawnLocation << 7);
            if ((((pawnLocation << 9) & AColumn) != 0) && (((pawnLocation << 9) & enemyOccupancy) != 0)) 
                attacks |= (pawnLocation << 9);
        }

        return attacks;
    }
    ulong CalculateKnightAttacks(int square, ulong teamOccupancy, ulong enemyOccupancy)
    {
        ulong attacks = 0L;

        //location to calculate attack from
        ulong knightLocation = 0L;
        knightLocation = SetBit(knightLocation, square);

        if (((knightLocation >> 17) & HColumn) != 0 && (((knightLocation >> 17) & teamOccupancy) == 0))
            attacks |= (knightLocation >> 17);
        if (((knightLocation >> 15) & AColumn) != 0 && (((knightLocation >> 15) & teamOccupancy) == 0))
            attacks |= (knightLocation >> 15);
        if (((knightLocation >> 10) & GHColumn) != 0 && (((knightLocation >> 10) & teamOccupancy) == 0))
            attacks |= (knightLocation >> 10);
        if (((knightLocation >> 6) & ABColumn) != 0 && (((knightLocation >> 6) & teamOccupancy) == 0))
            attacks |= (knightLocation >> 6);

        if (((knightLocation << 17) & AColumn) != 0 && (((knightLocation << 17) & teamOccupancy) == 0))
            attacks |= (knightLocation << 17);
        if (((knightLocation << 15) & HColumn) != 0 && (((knightLocation << 15) & teamOccupancy) == 0))
            attacks |= (knightLocation << 15);
        if (((knightLocation << 10) & ABColumn) != 0 && (((knightLocation << 10) & teamOccupancy) == 0))
            attacks |= (knightLocation << 10);
        if (((knightLocation << 6) & GHColumn) != 0 && (((knightLocation << 6) & teamOccupancy) == 0))
            attacks |= (knightLocation << 6);

        return attacks;
    }
    ulong CalculateBishopAttacks(int square, ulong teamOccupancy, ulong enemyOccupancy)
    {
        ulong attacks = 0L;

        int rank;
        int file;

        int targetRank = square / 8;
        int targetFile = square % 8;

        //loops until we hit every diagonal tile per direction until the end of the bitboard
        for (rank = targetRank + 1, file = targetFile + 1; rank <= 7 && file <= 7; rank++, file++)
        {
            //break before setting if team, so we cant capture
            if (((shiftOn << (rank * 8 + file)) & teamOccupancy) != 0) break;
            //set bit on
            attacks |= (shiftOn << (rank * 8 + file));
            //break after setting if enemy, so we can capture
            if (((shiftOn << (rank * 8 + file)) & enemyOccupancy) != 0) break;
        }
        for (rank = targetRank - 1, file = targetFile + 1; rank >= 0 && file <= 7; rank--, file++)
        {
            if (((shiftOn << (rank * 8 + file)) & teamOccupancy) != 0) break;
            attacks |= (shiftOn << (rank * 8 + file));
            if (((shiftOn << (rank * 8 + file)) & enemyOccupancy) != 0) break;
        }
        for (rank = targetRank + 1, file = targetFile - 1; rank <= 7 && file >= 0; rank++, file--)
        {
            if (((shiftOn << (rank * 8 + file)) & teamOccupancy) != 0) break;
            attacks |= (shiftOn << (rank * 8 + file));
            if (((shiftOn << (rank * 8 + file)) & enemyOccupancy) != 0) break;
        }
        for (rank = targetRank - 1, file = targetFile - 1; rank >= 0 && file >= 0; rank--, file--)
        {
            if (((shiftOn << (rank * 8 + file)) & teamOccupancy) != 0) break;
            attacks |= (shiftOn << (rank * 8 + file));
            if (((shiftOn << (rank * 8 + file)) & enemyOccupancy) != 0) break;
        }

        return attacks;
    }
    ulong CalculateRookAttacks(int square, ulong teamOccupancy, ulong enemyOccupancy)
    {
        ulong attacks = 0L;

        int rank;
        int file;

        int targetRank = square / 8;
        int targetFile = square % 8;

        //same as above but simpler as we only have to move in straight lines
        //important to change the checks based on direction this time though(vertical, horizontal)
        for (rank = targetRank + 1; rank <= 7; rank++)
        {
            if (((shiftOn << (rank * 8 + targetFile)) & teamOccupancy) != 0) break;
            attacks |= (shiftOn << (rank * 8 + targetFile));
            if (((shiftOn << (rank * 8 + targetFile)) & enemyOccupancy) != 0) break;
        }
        for (rank = targetRank - 1; rank >= 0; rank--)
        {
            if (((shiftOn << (rank * 8 + targetFile)) & teamOccupancy) != 0) break;
            attacks |= (shiftOn << (rank * 8 + targetFile));
            if (((shiftOn << (rank * 8 + targetFile)) & enemyOccupancy) != 0) break;
        }
        for (file = targetFile + 1; file <= 7; file++)
        {
            if (((shiftOn << (targetRank * 8 + file)) & teamOccupancy) != 0) break;
            attacks |= (shiftOn << (targetRank * 8 + file));
            if (((shiftOn << (targetRank * 8 + file)) & enemyOccupancy) != 0) break;
        }
        for (file = targetFile - 1; file >= 0; file--)
        {
            if (((shiftOn << (targetRank * 8 + file)) & teamOccupancy) != 0) break;
            attacks |= (shiftOn << (targetRank * 8 + file));
            if (((shiftOn << (targetRank * 8 + file)) & enemyOccupancy) != 0) break;
        }

        return attacks;
    }
    ulong CalculateQueenAttacks(int square, ulong teamOccupancy, ulong enemyOccupancy)
    {
        ulong attacks = 0L;

        attacks |= CalculateBishopAttacks(square, teamOccupancy, enemyOccupancy);
        attacks |= CalculateRookAttacks(square, teamOccupancy, enemyOccupancy);

        return attacks;
    }
    #endregion
}


