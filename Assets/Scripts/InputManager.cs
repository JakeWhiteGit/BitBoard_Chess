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

    Tile SelectedTile;

    ulong legalMoves;

    void Update()
    {
        CastRays();

        if (Input.GetMouseButtonDown(0))
        {
                HighlightLegalMoves();
            Debug.Log($"{GameManager.PieceSelected}");
        }
        if (Input.GetMouseButtonDown(1))
        {
            GameManager.StaticGameManager.ClearLegalMoves();
        }
    }

    private void HighlightLegalMoves()
    {
        //if there is a tile under the cursor return
        if (tileUnderCursor.collider is null) return;
        SelectedTile = tileUnderCursor.collider.GetComponent<Tile>();
        GameManager.TileSelected = SelectedTile.location;

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

    void CastRays()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        tileUnderCursor = Physics2D.Raycast(mousePosition, Vector2.zero);
    }

}
