using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;

    // Pieces same as FEN
    [SerializeField] GameObject B, K, N, P, Q, R, b, k, n, p, q, r;

    GameObject[] tiles;

    void Start()
    {

    }


    public void GenerateBoard()
    {
        tiles = new GameObject[64];
        int square = 0;
        
        for (int row = 7; row >= 0; row--)
        {
            for (int column = 0; column < 8; column++)
            {
                bool isEven = (column + row) % 2 != 0;
                Vector2 position = new Vector2(-3.5f + column, -3.5f + row);

                tiles[square] = InstantiateTile(position, isEven, square);
                square++;
            }
        }
    }
    GameObject InstantiateTile(Vector2 position, bool isEven, int square)
    {
        GameObject tile;
        SpriteRenderer spriteRenderer;

        tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
        spriteRenderer = tile.GetComponent<SpriteRenderer>();
        if (spriteRenderer is null) { return tile; }
        spriteRenderer.color = isEven ? Color.white : Color.grey;

        tile.GetComponent<Tile>().location = square;

        return tile;
    }

    public void PlacePieces(bool isWhite)
    {
        Dictionary<int, GameObject> pieceTypes = new Dictionary<int, GameObject>()
        {
            {2, isWhite ? B : b},
            {3, isWhite ? K : k},
            {4, isWhite ? N : n},
            {5, isWhite ? P : p},
            {6, isWhite ? Q : q},
            {7, isWhite ? R : r},
        };

        //this is the index of every piece color
        int[] pieceIndeces = BitBoards.StaticBitBoards.ReturnAllBitIndices(BitBoards.BoardState[isWhite ? 0 : 1]);

        //goes through the indeces of all pieces on the board
        foreach (var pieceType in pieceTypes)
        {
            int[] indexes = BitBoards.StaticBitBoards.ReturnAllBitIndices(BitBoards.BoardState[pieceType.Key]);
            instantiatePiece(pieceIndeces, indexes, pieceType.Value);
        }
    }

    //checking to see if i have a match between color and piece index to place a piece
    void instantiatePiece(int[] indices1, int[] indices2, GameObject piece)
    {
        for (int i = 0; i < indices1.Length; i++)
        {
            for (int j = 0; j < indices2.Length; j++)
            {
                if (indices1[i] == indices2[j])
                {
                    Instantiate(piece, tiles[indices1[i]].transform.position, Quaternion.identity, tiles[indices1[i]].transform);
                }
            }
        }
    }
}
