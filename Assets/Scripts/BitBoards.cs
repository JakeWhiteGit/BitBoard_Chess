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

    public static ulong[] BoardState;

    //ulong types binary values being used to represent piece locations
    public static ulong White = 0L,
                        Black = 0L,
                        Bishops = 0L,
                        Kings = 0L,
                        Knights = 0L,
                        Pawns = 0L,
                        Queens = 0L,
                        Rooks = 0L;

    //caching the capturable squares of every piece type from every square on the board
    //to hopefully speed up move generation for minimax
    public static ulong[,] PawnAttacks;
    public static ulong[] KnightAttacks;
    public static ulong[] KingAttacks;
    public static ulong[] BishopAttacks;
    public static ulong[] RookAttacks;
    public static ulong[] QueenAttacks;

    public static ulong[] Random;


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

        //pawn attacks is a 2d array as the direction differs based on piece color
        PawnAttacks = new ulong[2, 64];
        KnightAttacks = new ulong[64];
        KingAttacks = new ulong[64];
        BishopAttacks = new ulong[64];
        RookAttacks = new ulong[64];
        QueenAttacks = new ulong[64];

        BoardState = new ulong[8];
    }

    void Start()
    {
        ulong attackMask = RookAttacks[(int)Squares.a1];
        PrintBitBoard(attackMask, "attack mask");

        for (int i = 0; i < 4096; i++)
        {
            PrintBitBoard(SetOccupancy(i, BitCounter(attackMask), attackMask), "BLEH");
        }
    }

    #region new
    ulong SetOccupancy(int index, int bitsInMask, UInt64 attackMask)
    {
        ulong occupancy = 0L;

        for (int count = 0; count < bitsInMask; count++)
        {
            // returns location of the first bit that is on in a bitboard
            int square = ReturnFirstBitIndex(attackMask);
            attackMask = RemoveBit(attackMask, square);

            if ((index & (1 << count)) != 0)
            {
                //set occupancy table bit on at square
                occupancy |= (shiftOn << square);
            }
        }
        return occupancy;
    }
    #endregion

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
        while((bitBoard & shiftOn) == 0)
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
            if ((bitBoard &(shiftOn << i)) != 0)
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
                    Black = SetBit(Black, square);
                    Bishops = SetBit(Bishops, square);
                    square++;
                    break;

                case 'k':
                    Black = SetBit(Black, square);
                    Kings = SetBit(Kings, square);
                    square++;
                    break;

                case 'n':
                    Black = SetBit(Black, square);
                    Knights = SetBit(Knights, square);
                    square++;
                    break;

                case 'p':
                    Black = SetBit(Black, square);
                    Pawns = SetBit(Pawns, square);
                    square++;
                    break;

                case 'q':
                    Black = SetBit(Black, square);
                    Queens = SetBit(Queens, square);
                    square++;
                    break;

                case 'r':
                    Black = SetBit(Black, square);
                    Rooks = SetBit(Rooks, square);
                    square++;
                    break;

                case 'B':
                    White = SetBit(White, square);
                    Bishops = SetBit(Bishops, square);
                    square++;
                    break;

                case 'K':
                    White = SetBit(White, square);
                    Kings = SetBit(Kings, square);
                    square++;
                    break;

                case 'N':
                    White = SetBit(White, square);
                    Knights = SetBit(Knights, square);
                    square++;
                    break;

                case 'P':
                    White = SetBit(White, square);
                    Pawns = SetBit(Pawns, square);
                    square++;
                    break;

                case 'Q':
                    White = SetBit(White, square);
                    Queens = SetBit(Queens, square);
                    square++;
                    break;

                case 'R':
                    White = SetBit(White, square);
                    Rooks = SetBit(Rooks, square);
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

                default:
                    break;
            }

            //adds all of the bitboards into one array to use easier later
            BoardState[(int)Pieces.whites] = White;
            BoardState[(int)Pieces.blacks] = Black;
            BoardState[(int)Pieces.bishops] = Bishops;
            BoardState[(int)Pieces.kings] = Kings;
            BoardState[(int)Pieces.knights] = Knights;
            BoardState[(int)Pieces.pawns] = Pawns;
            BoardState[(int)Pieces.queens] = Queens;
            BoardState[(int)Pieces.rooks] = Rooks;
        }
    }

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
            PawnAttacks[(int)PieceColor.white, square] = CalculatePawnAttacks(PieceColor.white, square);
            PawnAttacks[(int)PieceColor.black, square] = CalculatePawnAttacks(PieceColor.black, square);
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
            RookAttacks[square] = CalculateRookAttacks(square);
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

    ulong CalculateKingAttacks(int square)
    {
        ulong attacks = 0L;

        //location to calculate attack from
        ulong bitBoard = 0L;
        bitBoard = SetBit(bitBoard, square);

        if (((bitBoard >> 8)) != 0) attacks |= (bitBoard >> 8);
        if (((bitBoard >> 7) & AColumn) != 0) attacks |= (bitBoard >> 7);
        if (((bitBoard >> 1) & HColumn) != 0) attacks |= (bitBoard >> 1);
        if (((bitBoard >> 9) & HColumn) != 0) attacks |= (bitBoard >> 9);

        if (((bitBoard << 8)) != 0) attacks |= (bitBoard << 8);
        if (((bitBoard << 7) & HColumn) != 0) attacks |= (bitBoard << 7);
        if (((bitBoard << 1) & AColumn) != 0) attacks |= (bitBoard << 1);
        if (((bitBoard << 9) & AColumn) != 0) attacks |= (bitBoard << 9);

        return attacks;
    }
    ulong CalculatePawnAttacks(PieceColor color, int square)
    {
        ulong attacks = 0L;

        //location to calculate attack from
        ulong bitBoard = 0L;
        bitBoard = SetBit(bitBoard, square);

        //if white pawns
        if (color == 0)
        {
            //check to see if the north east attack would land on the A column 
            //for this to be possible it would mean the attack would have wrapped around
            if (((bitBoard >> 7) & AColumn) != 0) attacks |= (bitBoard >> 7);
            if (((bitBoard >> 9) & HColumn) != 0) attacks |= (bitBoard >> 9);
        }
        //if black pawns
        else
        {
            if (((bitBoard << 7) & HColumn) != 0) attacks |= (bitBoard << 7);
            if (((bitBoard << 9) & AColumn) != 0) attacks |= (bitBoard << 9);
        }

        return attacks;
    }
    ulong CalculateKnightAttacks(int square)
    {
        ulong attacks = 0L;

        //location to calculate attack from
        ulong bitBoard = 0L;
        bitBoard = SetBit(bitBoard, square);

        if (((bitBoard >> 17) & HColumn) != 0) attacks |= (bitBoard >> 17);
        if (((bitBoard >> 15) & AColumn) != 0) attacks |= (bitBoard >> 15);
        if (((bitBoard >> 10) & GHColumn) != 0) attacks |= (bitBoard >> 10);
        if (((bitBoard >> 6) & ABColumn) != 0) attacks |= (bitBoard >> 6);

        if (((bitBoard << 17) & AColumn) != 0) attacks |= (bitBoard << 17);
        if (((bitBoard << 15) & HColumn) != 0) attacks |= (bitBoard << 15);
        if (((bitBoard << 10) & ABColumn) != 0) attacks |= (bitBoard << 10);
        if (((bitBoard << 6) & GHColumn) != 0) attacks |= (bitBoard << 6);


        return attacks;
    }
    ulong CalculateBishopAttacks(int square)
    {
        ulong attacks = 0L;

        int rank;
        int file;

        int targetRank = square / 8;
        int targetFile = square % 8;

        for (rank = targetRank +1, file = targetFile + 1; rank <= 6 && file <= 6 ; rank++, file++)
        {
            attacks |= (shiftOn << (rank * 8 + file));
        }
        for (rank = targetRank - 1, file = targetFile + 1; rank >= 1 && file <= 6; rank--, file++)
        {
            attacks |= (shiftOn << (rank * 8 + file));
        }
        for (rank = targetRank + 1, file = targetFile - 1; rank <= 6 && file >= 1; rank++, file--)
        {
            attacks |= (shiftOn << (rank * 8 + file));
        }
        for (rank = targetRank - 1, file = targetFile - 1; rank >= 1 && file >= 1; rank--, file--)
        {
            attacks |= (shiftOn << (rank * 8 + file));
        }

        return attacks;
    }
    ulong CalculateRookAttacks(int square)
    {
        ulong attacks = 0L;

        int rank;
        int file;

        int targetRank = square / 8;
        int targetFile = square % 8;

        for (rank = targetRank + 1; rank <= 6; rank++)
        {
            attacks |= (shiftOn << (rank * 8 + targetFile));
        }
        for (rank = targetRank -1; rank >= 1; rank--)
        {
            attacks |= (shiftOn << (rank * 8 + targetFile));
        }
        for (file = targetFile + 1; file <= 6; file++)
        {
            attacks |= (shiftOn << (targetRank * 8 + file));
        }
        for (file = targetFile - 1; file >= 1; file--)
        {
            attacks |= (shiftOn << (targetRank * 8 + file));
        }

        return attacks;
    }
}
