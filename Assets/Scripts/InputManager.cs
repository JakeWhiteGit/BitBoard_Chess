using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] BitBoards bitBoards;
    [SerializeField] Board board;

    Vector2 mousePosition;
    RaycastHit2D tileUnderCursor;

    public Tile SelectedTile;
    public int TileIndex;

    int pieceType;
    int pieceColor;
    ulong legalMoves;

    void Update()
    {
        CastRays();

        if (Input.GetMouseButtonDown(0))
        {
            HighlightLegalMoves();
        }
    }

    private void HighlightLegalMoves()
    {
        for (int i = 0; i < 64; i++)
        {
            board.Tiles[i].selected = false;
        }

        //if there is a tile under the cursor return its position in the array
        if (tileUnderCursor.collider is null) return;
        SelectedTile = tileUnderCursor.collider.GetComponent<Tile>();
        TileIndex = SelectedTile.location;

        pieceType = bitBoards.GetPieceType(TileIndex);
        pieceColor = bitBoards.GetPieceColor(TileIndex);
        legalMoves = bitBoards.CalculateSelectedMove(TileIndex, pieceType, pieceColor);

        int[] legalMoveBitIndices = bitBoards.ReturnAllBitIndices(legalMoves);

        for (int i = 0; i < legalMoveBitIndices.Length; i++)
        {
            board.Tiles[legalMoveBitIndices[i]].selected = false;

            if (board.Tiles[legalMoveBitIndices[i]].location == legalMoveBitIndices[i])
            {
                board.Tiles[legalMoveBitIndices[i]].selected = true;
            }
        }
    }

    void CastRays()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        tileUnderCursor = Physics2D.Raycast(mousePosition, Vector2.zero);
    }

}
