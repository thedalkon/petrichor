using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Petrichor.scripts.GeoEditor;

namespace Petrichor.scripts.TileEditor;

public partial class TileLayer : Layer
{
    private Material _tileLayerMat;
    private Material _renderMat;

    public TileLayer(int layer) : base(layer)
    {
        _tileLayerMat = ResourceLoader.Load<ShaderMaterial>("res://materials/tile_layer_mat.tres", null,
            ResourceLoader.CacheMode.Ignore);
        _renderMat = ResourceLoader.Load<ShaderMaterial>("res://materials/tile_render_mat.tres"); //Set to ignore if layer shader params are added
        Material = TileEditor.RenderMode ? _renderMat : _tileLayerMat;
        ZIndex = -layer;
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
            float tintOffset = (Petrichor.CurrentLayer - LayerID + 2) * 0.25f;
            ((ShaderMaterial)Material).SetShaderParameter("tint_offset", tintOffset);
            ((ShaderMaterial)Material).SetShaderParameter("tint_lerp", Math.Abs(tintOffset - 0.5f));
            SelfModulate = SelfModulate with {A = -Math.Abs(tintOffset - 0.5f) + 1.0f};
        }
    }

    public void SetTileMesh(Texture2D mmTex, int x, int y)
    {
        Texture2D tileTex = TileEditor.GetTileTexture(TileEditor.Data[Petrichor.CurrentLayer, x, y]);
        if (tileTex != null)
            MultiMeshes[tileTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, Utils.Tr2DZero);
        
        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, new Transform2D(0.0f, new Vector2(x,y) * 16.0f));
    }

    public void RemoveTileMesh(Texture2D mmTex, int x, int y)
    {
        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, Utils.Tr2DZero);
    }

    // Only use if absolutely necessary
    public override void FullRedraw()
    {
        foreach (MultiMeshInstance2D mm in MultiMeshes.Values)
        {
            mm.Multimesh.VisibleInstanceCount = 0;
        }

        for (int x = 0; x < GeometryEditor.LevelSize.X; x++)
        {
            for (int y = 0; y < GeometryEditor.LevelSize.Y; y++)
            {
                // Geo Rendering
                Cell cell = GeometryEditor.Data[LayerID, x, y];
                CellTypes type = (CellTypes)cell.Type;
                MultiMesh mm = MultiMeshes[GeometryEditor.CellTextures[type]].Multimesh;
                int cellIndex = mm.VisibleInstanceCount;

                mm.SetInstanceTransform2D(cellIndex, new Transform2D(0.0f, new Vector2(x, y) * Petrichor.ZoomFactor));
                mm.SetInstanceColor(cellIndex, Colors.White);
                mm.VisibleInstanceCount = ++cellIndex;
                
                // Pole Stackables Rendering
                int[] stackables = Utils.FlagsHelper.DecomposeFlag(cell.Stackables).ToArray();
                for (int i = 0; i < stackables.Length; i++)
                {
                    if (stackables[i] != (int)StackableTypes.HPole || stackables[i] != (int)StackableTypes.VPole)
                        continue;

                    MultiMesh stackableMm = MultiMeshes[GeometryEditor.StackableTextures[(StackableTypes)stackables[i]]].Multimesh;
                    int stackableIndex = stackableMm.VisibleInstanceCount;

                    stackableMm.SetInstanceTransform2D(stackableIndex, new Transform2D(0.0f, new Vector2(x, y) * Petrichor.ZoomFactor));
                    stackableMm.SetInstanceColor(stackableIndex, Colors.White);
                    stackableMm.VisibleInstanceCount = ++stackableIndex;
                }

                // Tiles Rendering
                string tileName = TileEditor.Data[LayerID, x, y];
                if (string.IsNullOrWhiteSpace(tileName) || !TileEditor.Tiles.TryGetValue(tileName, out Tile tile))
                    continue;

                Texture2D texture = TileEditor.GetTileTexture(tileName);
                MultiMesh tileMm = MultiMeshes[texture].Multimesh;
                int tileIndex = tileMm.VisibleInstanceCount;

                if (TileEditor.RenderMode)
                {
                    tileMm.SetInstanceTransform2D(tileIndex, new Transform2D(0.0f, (new Vector2(x, y) - Vector2.One * 2 * tile.BfTiles) * Petrichor.ZoomFactor));
                    tileMm.SetInstanceColor(tileIndex, Utils.Color8((int)tile.Size.X, (int)tile.Size.Y, tile.RepeatLines.Length, tile.BfTiles));
                    //texture.GetSize() * Petrichor.ZoomFactor / 16.0f
                }
                else
                {
                    tileMm.SetInstanceTransform2D(tileIndex, new Transform2D(0.0f, new Vector2(x, y) * Petrichor.ZoomFactor));
                    tileMm.SetInstanceColor(tileIndex, Colors.White);
                }

                tileMm.VisibleInstanceCount = ++tileIndex;
            }
        }
    }
}