using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int location;

    SpriteRenderer spriteRenderer;
    Color defaultColor;

    public bool selected;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();    
    }
    void Start()
    {
        defaultColor = spriteRenderer.color;
    }
    void Update()
    {
        if(selected is true)
        {
            spriteRenderer.color = Color.green;
        }
        else 
        {
            spriteRenderer.color = defaultColor;
        }
    }
    void OnMouseDown()
    {
        if (selected && GameManager.PieceSelected)
        {
            Debug.Log(" tile makemove");
            GameManager.StaticGameManager.MakeMove(location,
                                                    GameManager.TileSelected,
                                                    GameManager.PieceSelectedType,
                                                    GameManager.PieceSelectedColor );
        }
        else
        {
            GameManager.StaticGameManager.ClearLegalMoves();
            GameManager.PieceSelected = false;
        }
    }
}
 