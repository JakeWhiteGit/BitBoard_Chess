using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    Vector2 mousePosition;
    RaycastHit2D tileUnderCursor;

    public int selectedTile;

    void Update()
    {
        CastRays();

        if (Input.GetMouseButtonDown(0))
        {
            //if there is a tile under the cursor return its position in the array
            if (tileUnderCursor.collider is null) return;
            selectedTile = tileUnderCursor.collider.GetComponent<Tile>().location;

            //resets the current bitboard representing last selected tile
            BitBoards.PlayerTileSelection = 0L;
            BitBoards.PlayerTileSelection = BitBoards.StaticBitBoards.SetBit(BitBoards.PlayerTileSelection, selectedTile);

            //fire event on tile to process logic when selected
        }
    }

    void CastRays()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        tileUnderCursor = Physics2D.Raycast(mousePosition, Vector2.zero);
    }

}
