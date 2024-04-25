using Godot;

namespace Petrichor.scripts.GeoEditor.GeoTools;

public class SetCellGeoTool : GeoTool
{
    private readonly CellTypes _cellType;

    public SetCellGeoTool(CellTypes cellType)
    {
        _cellType = cellType;
    }
    
    public override void ApplyTool(Vector2I cellPos, int layer)
    {
        GeometryEditor.SetCell(layer, cellPos.X, cellPos.Y, _cellType);
        GeometryEditor.RedrawTerrain();
    }
}