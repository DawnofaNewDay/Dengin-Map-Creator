using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Diagnostics;
using Color = SFML.Graphics.Color;
using Font = SFML.Graphics.Font;
using Text = SFML.Graphics.Text;

namespace TileHelper;

public static class Game
{
    public static uint TileSize = 16;
    public static float Scale = 1.5f;
    public static uint TileSizePx => (uint)(TileSize * Scale);
    public static Sprite Tmap => new(new Texture("resources/Tmap.png")) 
    { 
        Scale = new Vector2f(Scale, Scale) 
    };

    private static Vector2i PickerWinSize => new((int)(Tmap.Texture.Size.X * Scale), (int)(Tmap.Texture.Size.Y * Scale));

    public static Vector2i MapSize = new((int)(50), (int)(50));
    public static int[,] Map = new int[MapSize.Y, MapSize.X];
    private static Vector2i MapWinSize => new((int)(MapSize.X * TileSizePx), (int)(MapSize.Y * TileSizePx));

    public static RenderWindow PickerWin;
    public static RenderWindow MapWin;
    public static uint FrameRate = 20;

    private static RectangleShape _pickerCursor = new(new Vector2f(TileSizePx, TileSizePx))
    {
        OutlineColor = Color.Red,
        OutlineThickness = 1 * Scale,
        FillColor = Color.Transparent
    };
    private static int SelectedTile;

    private static RectangleShape _mapCursor = new(new Vector2f(TileSizePx, TileSizePx))
    {
        OutlineColor = Color.Red,
        OutlineThickness = 1 * Scale,
        FillColor = Color.Transparent
    };

    private static Text _text = new("0", new Font("resources/font.ttf"), TileSizePx)
    {
        FillColor = Color.White
    };

    public static void Main(string[] args)
    {
        if (args.Length == 1)
            Scale = uint.Parse(args[0]);
        else if (args.Length > 1)
        {
            Scale = uint.Parse(args[0]);
            TileSize = uint.Parse(args[1]);
        }

        _text.Position = new Vector2f(PickerWinSize.X - TileSizePx * 2 + 2 * Scale, PickerWinSize.Y - TileSizePx - 2 * Scale);

        Surface pickerSurface = new();
        pickerSurface.MinimumSize = pickerSurface.MaximumSize = new Size(PickerWinSize.X, PickerWinSize.Y);

        PickerWin = new RenderWindow(pickerSurface.Handle);

        PickerWin.SetFramerateLimit(FrameRate);
        Tmap.Scale = new Vector2f(Scale, Scale);
        PickerWin.Closed += (_, _) => PickerWin.Close();
        PickerWin.MouseButtonPressed += (_, e) =>
        {
            if (e.Button == Mouse.Button.Left)
            {
                Vector2i tile = Utility.ToTilePos(new Vector2f(e.X, e.Y));
                _pickerCursor.Position = new Vector2f(tile.X * TileSizePx, tile.Y * TileSizePx);
                _text.DisplayedString = (tile.X + PickerWin.Size.X / TileSizePx * tile.Y).ToString();
                SelectedTile = (int)(tile.X + PickerWin.Size.X / TileSizePx * tile.Y);
            }
        };

        Surface mapSurface = new();
        mapSurface.MinimumSize = mapSurface.MaximumSize = new Size(MapWinSize.X, MapWinSize.Y);
        mapSurface.Left = PickerWinSize.X;
        MapWin = new RenderWindow(mapSurface.Handle);

        MapWin.SetFramerateLimit(FrameRate);
        MapWin.Closed += (_, _) => PickerWin.Close();
        MapWin.MouseButtonPressed += (_, e) =>
        {
            Vector2i tile = Utility.ToTilePos(new Vector2f(e.X, e.Y));
            if (e.Button == Mouse.Button.Left)                
                Map[tile.Y, tile.X] = SelectedTile;
            else if (e.Button == Mouse.Button.Right)
                Map[tile.Y, tile.X] = 0;
        };
        MapWin.KeyPressed += async (_, e) =>
        {
            if (e.Code == Keyboard.Key.S) Debug.WriteLine(Map);
                //await File.WriteAllLinesAsync("Map.txt", Map.ToString());
        };

        Form form = new Form();
        form.Text = "Dengin Map Creator";
        form.BackColor = System.Drawing.Color.DarkGray;
        form.MinimumSize = form.MaximumSize = new Size(PickerWinSize.X + MapWinSize.X, MapWinSize.Y > PickerWinSize.Y ? MapWinSize.Y : PickerWinSize.Y);
        form.MinimumSize = form.MaximumSize += form.Size - form.ClientSize;
        form.FormBorderStyle = FormBorderStyle.FixedSingle;
        form.MaximizeBox = false;
        form.Show();

        form.Controls.Add(pickerSurface);
        form.Controls.Add(mapSurface);

        while (form.Visible) Loop();
        PickerWin.Close();
        MapWin.Close();
    }

    private static void Loop()
    {
        GC.Collect();
        Application.DoEvents();
        PickerWin.DispatchEvents();
        PickerWin.Clear(Color.Cyan);
        PickerWin.Draw(Tmap);
        PickerWin.Draw(_pickerCursor);
        PickerWin.Draw(_text);
        PickerWin.Display();

        MapWin.DispatchEvents();
        MapWin.Clear(new Color(75, 75, 75));

        for (int i = 1; i < MapSize.X; i++)
            MapWin.Draw(new RectangleShape() { FillColor = new Color(50, 50, 50), Position = new Vector2f(i * TileSizePx - 0.5f, 0), Size = new Vector2f(1 * Scale, MapWin.Size.Y)});
        for (int i = 1; i < MapSize.Y; i++)
            MapWin.Draw(new RectangleShape() { FillColor = new Color(50, 50, 50), Position = new Vector2f(0, i * TileSizePx - 0.5f), Size = new Vector2f(MapWin.Size.X, 1 * Scale) });

        Tilemap newMap = new Tilemap(Utility.ToOneDimArray(Map), (uint)MapSize.X, (uint)MapSize.Y, new Vector2u(TileSize, TileSize));
        newMap.Scale = new Vector2f(Scale, Scale);
        MapWin.Draw(newMap);
        _mapCursor.Position = new Vector2f(Utility.ToTilePos((Vector2f)Mouse.GetPosition(MapWin)).X * TileSizePx, Utility.ToTilePos((Vector2f)Mouse.GetPosition(MapWin)).Y * TileSizePx);
        MapWin.Draw(_mapCursor);
        MapWin.Display();
    }
}

public class Surface : Control
{
    protected override void OnPaint(PaintEventArgs e)
    {
        // Do nothing
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Do nothing
    }
}