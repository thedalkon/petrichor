using System;
using System.Diagnostics;
using Godot;
using Petrichor.scripts.Geometry;

namespace Petrichor.scripts.Tiling;

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
        ZIndex -= layer;
    }

    public TileLayer()
    {
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

    public void SetTileMesh(Texture2D mmTex, ref Tile tile, int x, int y)
    {
        Texture2D tileTex = TileEditor.GetTileTexture(TileEditor.Data[Petrichor.CurrentLayer, x, y]);
        if (tileTex != null && MultiMeshes.TryGetValue(tileTex, out MultiMeshInstance2D mm))
        {
            mm.Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, Utils.Tr2DZero);
            ChangeInstances(mm, -1);
        }
        
        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, 
            new Transform2D(0.0f, (new Vector2(x,y) - (tile.Size * 0.5f).Ceil() + Vector2.One) * 16.0f));
        
        ChangeInstances(MultiMeshes[mmTex], 1);
        CheckVisibility(MultiMeshes[mmTex]);
    }

    public void RemoveTileMesh(Texture2D mmTex, int x, int y)
    {
        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, Utils.Tr2DZero);
        ChangeInstances(MultiMeshes[mmTex], -1);
        CheckVisibility(MultiMeshes[mmTex]);
    }

    public override void ReloadMultimesh()
    {
        foreach (MultiMeshInstance2D mm in MultiMeshes.Values)
        {
            CheckVisibility(mm);
        }
    }

    // Only use if absolutely necessary
    public override void FullRedraw()
    {
        foreach (MultiMeshInstance2D mm in MultiMeshes.Values)
        {
            for (int i = 0; i < mm.Multimesh.InstanceCount; i++)
            {
                mm.Multimesh.SetInstanceTransform2D(i, Utils.Tr2DZero);
            }
        }

        for (int x = 0; x < GeometryEditor.LevelSize.X; x++)
        {
            for (int y = 0; y < GeometryEditor.LevelSize.Y; y++)
            {
                // Tiles Rendering
                string tileName = TileEditor.Data[LayerID, x, y];
                if (string.IsNullOrWhiteSpace(tileName) || !TileEditor.Tiles.TryGetValue(tileName, out Tile tile))
                    continue;

                Texture2D texture = TileEditor.GetTileTexture(tileName);
                if (!MultiMeshes.TryGetValue(texture, out var mesh))
                {
                    Debug.WriteLine(Utils.ERROR_STR + $"Tile {tileName} not in multimesh cache, report this bug.");
                    continue;
                }
                    
                MultiMesh tileMm = mesh.Multimesh;
                int tileIndex = x * GeometryEditor.LevelSize.Y + y;

                if (TileEditor.RenderMode)
                {
                    tileMm.SetInstanceTransform2D(tileIndex, new Transform2D(0.0f, 
                        (new Vector2(x, y) - Vector2.One * 2 * tile.BfTiles + (tile.Size * 0.5f).Ceil()  + Vector2.One) * Petrichor.ZoomFactor));
                }
                else
                {
                    tileMm.SetInstanceTransform2D(tileIndex, new Transform2D(0.0f, 
                        (new Vector2(x, y) - (tile.Size * 0.5f).Ceil() + Vector2.One) * Petrichor.ZoomFactor));
                }
                ChangeInstances(mesh, 1);
                mesh.Show();
            }
        }
    }
}