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
        string output = $"{Game.MapSize.X}-{Game.MapSize.Y}-";
        int[] MapArray = new int[Game.MapSize.X * Game.MapSize.Y];

        for (int y = 0; y < Game.MapSize.Y; y++)
        {
            for (int x = 0; x < Game.MapSize.X; x++)
            {
                MapArray[x + y * Game.MapSize.X] = Map[y, x];
            }
        }

        output += string.Join(".", MapArray);
        return output;
    }

    public static int RandomGrassTile()
    {
        int[] grassTiles = { 6, 29, 30, 50 };
        return grassTiles[new Random().Next(0, grassTiles.Length)];
    }
}