using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicNumberGeneration : MonoBehaviour
{
    /*

    public static ulong[] Random;

    
    uint seed = 1804289383;
    
    
    public static readonly ulong[] BishopOccupancyMaskCount =
    {
        6, 5, 5, 5, 5, 5, 5, 6,
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5,
        6, 5, 5, 5, 5, 5, 5, 6
    };
    public static readonly ulong[] RookOccupancyMaskCount =
    {
        12, 11, 11, 11, 11, 11, 11, 12,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        12, 11, 11, 11, 11, 11, 11, 12,
    };
    ulong FindMagicNumber(int square, int relevantBits, bool bishop)
    {
        ulong[] occupancies = new ulong[4096];
        ulong[] attacks = new ulong[4096];
        ulong[] usedAttacks = new ulong[4096];
        ulong attackMask = bishop == true ? CalculateBishopAttacks(square) : CalculateRookAttacks(square);

        int occupancyIndeces = 1 << relevantBits;

        for (int i = 0; i < occupancyIndeces; i++)
        {
            occupancies[i] = SetOccupancy(i, relevantBits, attackMask);

            attacks[i] = bishop ? CalculateBishopAttacksRunTime(square, occupancies[i]) :
                                  CalculateRookAttacksRunTime(square, occupancies[i]);
        }


        for (int i = 0; i < 100000000; i++)
        {
            ulong magicNumber = MagicNumberCandidate();

            if ((Convert.ToUInt64(BitCounter(attackMask * magicNumber)) & 0xFF00000000000000) < 16) continue;

            Array.Clear(usedAttacks, 0, usedAttacks.Length);

            int index;
            bool fail = false;

            for (index = 0; !fail && index < occupancyIndeces; index++)
            {
                int magicIndex = Convert.ToInt32(occupancies[index] * magicNumber) >> (64 - relevantBits);

                if (usedAttacks[magicIndex] == 0)
                {
                    usedAttacks[magicIndex] = attacks[index];
                }
                else if (usedAttacks[magicIndex] != attacks[index])
                {
                    fail = true;
                }
            }
            if (fail is false)
            {
                return magicNumber;
            }
        }

        Debug.Log("magic number fails");
        return 0;
    }

    #region RandomMagicGeneration

    //creating my own pseudo random to ensure results
    //of random numbers are same cross platform/architecture
    uint RandomNumberGen32()
    {
        uint number = seed;
        number ^= number << 13;
        number ^= number >> 17;
        number ^= number << 5;

        seed = number;

        return number;
    }

    ulong RandomNumberGen64()
    {
        ushort slicer = 0xFFFF;
        ulong n1, n2, n3, n4;
        n1 = (RandomNumberGen32()) & slicer;
        n2 = (RandomNumberGen32()) & slicer;
        n3 = (RandomNumberGen32()) & slicer;
        n4 = (RandomNumberGen32()) & slicer;

        return n1 | (n2 << 16) | (n3 << 32) | (n4 << 48);
    }

    ulong MagicNumberCandidate()
    {

        ulong value = RandomNumberGen64() & RandomNumberGen64() & RandomNumberGen64();
        return value;
    }
    ulong CalculateBishopAttacks(int square)
    {
        ulong attacks = 0L;

        int rank;
        int file;

        int targetRank = square / 8;
        int targetFile = square % 8;

        for (rank = targetRank + 1, file = targetFile + 1; rank <= 6 && file <= 6; rank++, file++)
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
        for (rank = targetRank - 1; rank >= 1; rank--)
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
    */
}
