using Godot;

namespace Petrichor.scripts.Geometry.GeoTools;

public class SetCellGeoTool : GeoTool
{
    private readonly CellTypes _cellType;

    public SetCellGeoTool(CellTypes cellType)
    {
        _cellType = cellType;
    }
    
    public override void ApplyTool(Vector2I cellPos, int layer)
    {
        if (GeometryEditor.GetCell(layer, cellPos).Type == (int)_cellType)
            return;
        
        GeometryEditor.Layers[Petrichor.CurrentLayer].SetCellMesh(GeometryEditor.CellTextures[_cellType], cellPos.X, cellPos.Y);
        GeometryEditor.SetCell(layer, cellPos.X, cellPos.Y, _cellType);
    }
}