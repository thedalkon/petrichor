using Godot;

namespace Petrichor.scripts.GeoEditor.GeoTools;

public class EraseAllGeoTool : GeoTool
{
    public override void ApplyTool(Vector2I cellPos, int layer)
    {
        GeoEditor.GeometryEditor.Data[layer, cellPos.X, cellPos.Y].Type = 0;
        GeoEditor.GeometryEditor.Data[layer, cellPos.X, cellPos.Y].Stackables = 0;
        GeometryEditor.RedrawTerrain();
    }
}