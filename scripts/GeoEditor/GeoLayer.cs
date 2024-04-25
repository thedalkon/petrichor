using System;
using Godot;

namespace Petrichor.scripts.GeoEditor;

public partial class GeoLayer : Control
{
    private readonly int _layer;

    public GeoLayer(int layer)
    {
        _layer = layer;
        Material = ResourceLoader.Load<ShaderMaterial>("res://materials/geo_layer_mat.tres", null,
            ResourceLoader.CacheMode.Ignore);
        Petrichor.GeoLayerInstances.Add(this);
        ZIndex = -1;
        LayerChanged();
    }

    public void LayerChanged()
    {
        float tintOffset = (Petrichor.CurrentLayer - _layer + 2) * 0.25f;
        ((ShaderMaterial)Material).SetShaderParameter("tint_offset", tintOffset);
        ((ShaderMaterial)Material).SetShaderParameter("tint_lerp", Math.Abs(tintOffset - 0.5f));
        SelfModulate = SelfModulate with {A = -Math.Abs(tintOffset - 0.5f) + 1.0f};
    }
    
    public override void _Draw()
    {
        for (int x = 0; x < GeometryEditor.LevelSize.X; x++)
        {
            for (int y = 0; y < GeometryEditor.LevelSize.Y; y++)
            {
                // Draw Cell
                DrawTextureRect(GeometryEditor.CellTextures[(CellTypes)GeometryEditor.Data[_layer, x, y].Type],
                    new Rect2(new Vector2(x,y) * Petrichor.ZoomFactor, new Vector2(Petrichor.ZoomFactor, Petrichor.ZoomFactor)), false);
                
                // Draw Stackables
                int[] stackables = Utils.FlagsHelper.DecomposeFlag(GeometryEditor.Data[_layer, x, y].Stackables).ToArray();
                foreach (int t in stackables)
                {
                    DrawTextureRect(GeometryEditor.StackableTextures[(StackableTypes)t], 
                        new Rect2(new Vector2(x,y) * Petrichor.ZoomFactor, 
                            new Vector2(Petrichor.ZoomFactor, Petrichor.ZoomFactor)), false);
                }
            }
        }
    }
}