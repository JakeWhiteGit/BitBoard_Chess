using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] BitBoards bitBoards;

    // Pieces same as FEN
    [SerializeField] GameObject B, K, N, P, Q, R, b, k, n, p, q, r;

    public Tile[] Tiles;
    public GameObject[] TilesObject;
    public List<GameObject> Pieces = new List<GameObject>();

    void Start()
    {

    }

    public void GenerateBoard()
    {
        Tiles = new Tile[64];
        TilesObject = new GameObject[64];
        int square = 0;
        
        for (int row = 7; row >= 0; row--)
        {
            for (int column = 0; column < 8; column++)
            {
                bool isEven = (column + row) % 2 != 0;
                Vector2 position = new Vector2(-3.5f + column, -3.5f + row);

                TilesObject[square] = InstantiateTile(position, isEven, square);
                Tiles[square] = TilesObject[square].GetComponent<Tile>();
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

    //method needs to be called twice for each colour
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
        int[] pieceIndeces = bitBoards.ReturnAllBitIndices(bitBoards.BoardState[isWhite ? 0 : 1]);

        //goes through the indeces of all pieces on the board
        foreach (var pieceType in pieceTypes)
        {
            int[] indexes = bitBoards.ReturnAllBitIndices(bitBoards.BoardState[pieceType.Key]);
            instantiatePiece(pieceIndeces, indexes, pieceType.Value);
        }
    }

    //checking to see if i have a match between color and piece index to place a piece
    void instantiatePiece(int[] pieceColorIndices, int[] pieceTypeIndices, GameObject piecePrefab)
    {
        //loop through both the array of all color locations and all type values
        for (int i = 0; i < pieceColorIndices.Length; i++)
        {
            for (int j = 0; j < pieceTypeIndices.Length; j++)
            {
                if (pieceColorIndices[i] == pieceTypeIndices[j])
                {
                   Pieces.Add(Instantiate(piecePrefab, Tiles[pieceColorIndices[i]].transform.position, Quaternion.identity, Tiles[pieceColorIndices[i]].transform));
                }
            }
        }
    }
}
