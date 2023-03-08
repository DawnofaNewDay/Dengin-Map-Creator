using SFML.System;

namespace TileHelper;

public static class Utility
{
    public static Vector2i ToTilePos(Vector2f pos)
    {
        return new Vector2i((int)Math.Floor(pos.X / Game.TileSizePx), (int)Math.Floor(pos.Y / Game.TileSizePx));
    }

    public static int[] ToOneDimArray(int[,] twoDimArray)
    {
        int[] oneDimArray = new int[twoDimArray.Length];
        int i = 0;
        foreach (int element in twoDimArray)
        {
            oneDimArray[i] = element;
            i++;
        }
        return oneDimArray;
    }

    public static string MapToString(int[,] Map)
    {
        return "wait";
    }
}