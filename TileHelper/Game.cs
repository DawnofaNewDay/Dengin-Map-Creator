using System.Reflection;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;
using Font = SFML.Graphics.Font;
using Text = SFML.Graphics.Text;

namespace TileHelper;

public static class Game
{
    public static uint TileSize = 16;
    public static float Scale = 2f;
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
    public static uint FrameRate = 60;

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
    
    private static bool selecting;
    private static List<int> _selectedTiles = new List<int>();
    private static Vector2i _selectedSize;
    private static Vector2i _startPos;
    private static Vector2i _endPos;


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
                _pickerCursor.Size = new Vector2f(TileSizePx, TileSizePx);
                selecting = false;
                _selectedTiles = new List<int>();
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

        Form form = new Form();
        form.Text = "Dengin Map Creator";
        form.BackColor = System.Drawing.Color.DarkGray;
        form.MinimumSize = new Size(PickerWinSize.X + MapWinSize.X, MapWinSize.Y > PickerWinSize.Y ? MapWinSize.Y : PickerWinSize.Y);
        form.MinimumSize += form.Size - form.ClientSize;
        form.FormBorderStyle = FormBorderStyle.FixedSingle;
        form.MaximizeBox = false;
        form.Show();

        ToolStripContainer container = new();
        ToolStrip file = new()
        {
            Name = "File",
            Text = "File"
        };

        file.Items.Add("Save");
        file.ItemClicked += async (_, e) =>
        {
            if (e.ClickedItem == file.Items[0])
                await File.WriteAllTextAsync("Map.txt", Utility.MapToString(Map));
        };
        container.TopToolStripPanel.Controls.Add(file);
        container.Top = PickerWinSize.Y;
        container.Size = new Size(PickerWinSize.X, (int)(30 * Scale));
        form.Controls.Add(container);

        Button button = new()
        {
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            UseVisualStyleBackColor = true,
            Text = "Save",
            Size = new Size((int)(TileSizePx * 2), (int)(TileSizePx)),
            Location = new Point(MapWinSize.X / 2 - (int)TileSizePx, MapWinSize.Y + (int)TileSizePx),
            Font = new System.Drawing.Font("Segoe UI", 9.0F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
            TabIndex = 3
        };

        form.Controls.Add(pickerSurface);
        form.Controls.Add(mapSurface);
        form.Controls.Add(button);

        while (form.Visible) Loop();
        PickerWin.Close();
        MapWin.Close();
    }

    private static void Loop()
    {
        GC.Collect();
        Application.DoEvents();
        PickerWin.DispatchEvents();

        Vector2i pickerTile = Utility.ToTilePos(new Vector2f(Mouse.GetPosition(PickerWin).X, Mouse.GetPosition(PickerWin).Y));
        if (Mouse.IsButtonPressed(Mouse.Button.Middle))
        {
            if (selecting == false)
            {
                _startPos = pickerTile;
                _pickerCursor.Position = new Vector2f(_startPos.X * TileSizePx, _startPos.Y * TileSizePx);
            }
            else
            {
                _endPos = pickerTile;
                _selectedTiles = new List<int>();
                for (int y = _startPos.Y; y <= _endPos.Y; y++)
                {
                    for (int x = _startPos.X; x <= _endPos.X; x++)
                    {
                        _selectedSize = new Vector2i(x - _startPos.X + 1, y - _startPos.Y + 1);
                        _selectedTiles.Add((int)(x + PickerWin.Size.X / TileSizePx * y));
                    }
                }
            }

            _pickerCursor.Size = new Vector2f((_endPos.X - _startPos.X + 1) * TileSizePx, (_endPos.Y - _startPos.Y + 1) * TileSizePx);
            selecting = true;
        }
        else
        {
            selecting = false;
        }

        Vector2i tile = Utility.ToTilePos(new Vector2f(Mouse.GetPosition(MapWin).X, Mouse.GetPosition(MapWin).Y));
        if (tile.X >= 0 && tile.Y >= 0 && tile.X < MapSize.X && tile.Y < MapSize.Y)
        {
            if (Mouse.IsButtonPressed(Mouse.Button.Left) && !selecting && _selectedTiles.Count > 0)
            {
                for (int y = 0; y < _selectedSize.Y; y++)
                {
                    for (int x = 0; x < _selectedSize.X; x++)
                    {
                        if (tile.X + x < MapSize.X && tile.Y + y < MapSize.Y)
                            Map[tile.Y + y, tile.X + x] = _selectedTiles[x + y * _selectedSize.X];
                    }
                }
            }
            else if (Mouse.IsButtonPressed(Mouse.Button.Left) && Keyboard.IsKeyPressed(Keyboard.Key.LShift))
                Map[tile.Y, tile.X] = Utility.RandomGrassTile();
            else if (Mouse.IsButtonPressed(Mouse.Button.Left))            
                Map[tile.Y, tile.X] = SelectedTile;
            else if (Mouse.IsButtonPressed(Mouse.Button.Right))
                Map[tile.Y, tile.X] = 0;
        }

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
        
        if (Keyboard.IsKeyPressed(Keyboard.Key.LShift)) _mapCursor.OutlineColor = Color.Green;
        else _mapCursor.OutlineColor = Color.Red;

        if (_selectedTiles.Count > 0)
        {
            _mapCursor.Size = new Vector2f(_selectedSize.X * TileSizePx, _selectedSize.Y * TileSizePx);
            _mapCursor.OutlineColor = Color.Blue;
        }
        
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