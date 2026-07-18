using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    public ulong characterId;

    public virtual void SetMaterialParams(Character c)
    {

    }

    public static GridViewStats GetGridViewStats(int lod)
    {
        switch (lod)
        {
            // 32-pixel
            case 1:
                return new GridViewStats
                {
                    charactersToShow = 112,
                    entriesPerRow = 16,
                    startingX = 0,
                    startingY = 0,
                    cardWidth = 1.5f,
                    cardHeight = 1.7f,
                    cardOffsetW = 0.1f,
                    cardOffsetH = 0.05f
                };
            // 16-pixel
            case 2:
                return new GridViewStats
                {
                    charactersToShow = 512,
                    entriesPerRow = 32,
                    startingX = 0,
                    startingY = 0,
                    cardWidth = 0.75f,
                    cardHeight = 0.75f,
                    cardOffsetW = 0.05f,
                    cardOffsetH = 0.05f
                };
            // 64-pixel
            case 0:
            default:
                return new GridViewStats
                {
                    charactersToShow = 40,
                    entriesPerRow = 10,
                    startingX = 0,
                    startingY = 0,
                    cardWidth = 2.4f,
                    cardHeight = 3f,
                    cardOffsetW = 0.24f,
                    cardOffsetH = 0.3f
                };
        }
    }
}

public struct GridViewStats
{
    public uint charactersToShow;
    public int entriesPerRow;
    public float startingX;
    public float startingY;
    public float cardWidth;
    public float cardHeight;
    public float cardOffsetW;
    public float cardOffsetH;
}
