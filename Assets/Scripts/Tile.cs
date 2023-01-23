using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int location;

    SpriteRenderer spriteRenderer;
    Color defaultColor;

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
        bool selected = location == BitBoards.StaticBitBoards.ReturnFirstBitIndex(BitBoards.PlayerTileSelection) ? true : false;
        
        if(selected is true)
        {
            spriteRenderer.color = Color.green;
        }
        else 
        {
            spriteRenderer.color = defaultColor;
        }
    }
}
 