using System;
using System.Diagnostics;
using Godot;
using Petrichor.scripts.GeoEditor;

namespace Petrichor.scripts.TileEditor;

public partial class TileLayer : GeoLayer
{
    private Material _tileLayerMat;
    private Material _renderMat;

    private int i = 0;
    
    public TileLayer(int layer) : base(layer)
    {
        _tileLayerMat = ResourceLoader.Load<ShaderMaterial>("res://materials/tile_layer_mat.tres", null,
            ResourceLoader.CacheMode.Ignore);
        _renderMat = ResourceLoader.Load<ShaderMaterial>("res://materials/tile_render_mat.tres"); //Set to ignore if layer shader params are added
        Material = TileEditor.RenderMode ? _renderMat : _tileLayerMat;
    }
    
    public override void LayerChanged()
    {
        if (TileEditor.RenderMode)
        {
            SelfModulate = Colors.White;
            Material = _renderMat;
        }
        else
        {
            Material = _tileLayerMat;
            float tintOffset = (Petrichor.CurrentLayer - Layer + 2) * 0.25f;
            ((ShaderMaterial)Material).SetShaderParameter("tint_offset", tintOffset);
            ((ShaderMaterial)Material).SetShaderParameter("tint_lerp", Math.Abs(tintOffset - 0.5f));
            SelfModulate = SelfModulate with {A = -Math.Abs(tintOffset - 0.5f) + 1.0f};
        }
    }

    public override void _Draw()
    {
        for (int x = 0; x < GeometryEditor.LevelSize.X; x++)
        {
            for (int y = 0; y < GeometryEditor.LevelSize.Y; y++)
            {
                // Draw Cell
                DrawTextureRect(GeometryEditor.CellTextures[(CellTypes)GeometryEditor.Data[Layer, x, y].Type],
                    new Rect2(new Vector2(x, y) * Petrichor.ZoomFactor,
                        new Vector2(Petrichor.ZoomFactor, Petrichor.ZoomFactor)), false);

                // Draw Pole Stackables
                int[] stackables = Utils.FlagsHelper.DecomposeFlag(GeometryEditor.Data[Layer, x, y].Stackables)
                    .ToArray();
                for (int i = 0; i < stackables.Length; i++)
                {
                    if (stackables[i] != (int)StackableTypes.HPole && stackables[i] != (int)StackableTypes.VPole)
                        continue;

                    DrawTextureRect(GeometryEditor.StackableTextures[(StackableTypes)stackables[i]],
                        new Rect2(new Vector2(x, y) * Petrichor.ZoomFactor,
                            new Vector2(Petrichor.ZoomFactor, Petrichor.ZoomFactor)), false);
                }
            }
        }

        for (int x = 0; x < GeometryEditor.LevelSize.X; x++)
        {
            for (int y = 0; y < GeometryEditor.LevelSize.Y; y++)
            {
                // Draw Tile
                string tileName = TileEditor.Data[Layer, x, y];
                if (!string.IsNullOrWhiteSpace(tileName) && TileEditor.Tiles.TryGetValue(tileName, out Tile tile))
                {
                    Texture2D texture = TileEditor.GetTileTexture(tileName);
                    if (TileEditor.RenderMode)
                    {
                        DrawTextureRect(texture, 
                            new Rect2((new Vector2(x, y) - Vector2.One * 2 * tile.BfTiles) * Petrichor.ZoomFactor,
                                texture.GetSize() * Petrichor.ZoomFactor / 16.0f), false, 
                            Utils.Color8((int)tile.Size.X, (int)tile.Size.Y, tile.RepeatLines.Length, tile.BfTiles)); // This somehow works
                    }
                    else
                    {
                        DrawTextureRectRegion(texture,
                            new Rect2(new Vector2(x, y) * Petrichor.ZoomFactor, tile.Size * Petrichor.ZoomFactor),
                            tile.PreviewRect, tile.Color);
                    }
                }
            }
        }
    }
}