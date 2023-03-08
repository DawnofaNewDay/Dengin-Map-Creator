using SFML.Graphics;
using SFML.System;

namespace TileHelper;

public class Tilemap : Transformable, Drawable
{
    public uint Width;
    public uint Height;

    private VertexArray _vertices = new();
    private Texture _tilemap = new("resources/Tmap.png");

    public Tilemap(int[] map, uint width, uint height, Vector2u tileSize)
    {
        Width = width;
        Height = height;
        _vertices.PrimitiveType = PrimitiveType.Quads;
        _vertices.Resize(width * height * 4);

        for (uint i = 0; i < width; ++i)
            for (uint j = 0; j < height; ++j)
            {
                int tileNumber = map[i + j * width];

                float tu = tileNumber % (_tilemap.Size.X / tileSize.X);
                float tv = tileNumber / (_tilemap.Size.X / tileSize.X);

                uint index = (i + j * width) * 4;
                _vertices[index + 0] = new Vertex(
                    new Vector2f(i * tileSize.X, j * tileSize.Y),
                    new Vector2f(tu * tileSize.X, tv * tileSize.Y));
                _vertices[index + 1] = new Vertex(
                    new Vector2f((i + 1) * tileSize.X, j * tileSize.Y),
                    new Vector2f((tu + 1) * tileSize.X, tv * tileSize.Y));
                _vertices[index + 2] = new Vertex(
                    new Vector2f((i + 1) * tileSize.X, (j + 1) * tileSize.Y),
                    new Vector2f((tu + 1) * tileSize.X, (tv + 1) * tileSize.Y));
                _vertices[index + 3] = new Vertex(
                    new Vector2f(i * tileSize.X, (j + 1) * tileSize.Y),
                    new Vector2f(tu * tileSize.X, (tv + 1) * tileSize.Y));
            }
    }

    public void Draw(RenderTarget target, RenderStates states)
    {
        states.Transform = Transform;
        states.Texture = _tilemap;
        target.Draw(_vertices, states);
    }
}
