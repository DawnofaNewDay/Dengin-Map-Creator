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
        string output = "{\n";

        for (int y = 0; y < Game.MapSize.Y; y++)
        {
            output += "    {";
            for (int x = 0; x < Game.MapSize.X; x++)
            {
                if (x != Game.MapSize.X - 1) output += $"{Map[y,x]}, ";
                else output += $"{Map[y, x]}";
            }
            output += "},\n";
        }
        output += "};";

        return output;
    }

    public static int RandomGrassTile()
    {
        int[] grassTiles = { 6, 29, 30, 50 };
        return grassTiles[new Random().Next(0, grassTiles.Length)];
    }
}