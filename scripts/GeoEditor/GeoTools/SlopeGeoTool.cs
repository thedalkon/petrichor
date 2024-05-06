using Godot;
using Godot.Collections;

namespace Petrichor.scripts.GeoEditor.GeoTools;

public class SlopeGeoTool : GeoTool
{
    private readonly Dictionary<Vector2I, CellTypes> _slopeDirections = new()
    {
        {new Vector2I(1, 1), CellTypes.SlopeES},
        {new Vector2I(1, -1), CellTypes.SlopeNE},
        { new Vector2I(-1, 1), CellTypes.SlopeSW},
        { new Vector2I(-1, -1), CellTypes.SlopeNW},
};
    
    public override void ApplyTool(Vector2I cellPos, int layer)
    {
        if (GeometryEditor.Data[layer, cellPos.X, cellPos.Y].Type != (int)CellTypes.Solid)
            return;

        foreach (Vector2I dir in _slopeDirections.Keys)
        {
            if (GeometryEditor.GetCell(layer, cellPos + dir with { X = 0, Y = dir.Y }).Type !=
                (int)CellTypes.Air ||
                GeometryEditor.GetCell(layer, cellPos + dir with { Y = 0, X = dir.X }).Type !=
                (int)CellTypes.Air ||
                GeometryEditor.GetCell(layer, cellPos + dir with { X = 0, Y = -dir.Y }).Type ==
                (int)CellTypes.Air ||
                GeometryEditor.GetCell(layer, cellPos + dir with { Y = 0, X = -dir.X }).Type ==
                (int)CellTypes.Air) continue;
            
            CellTypes slopeType = _slopeDirections[dir];

            GeometryEditor.Layers[Petrichor.CurrentLayer].SetCellMesh(GeometryEditor.CellTextures[slopeType], cellPos.X, cellPos.Y);
            GeometryEditor.SetCell(layer, cellPos.X, cellPos.Y, slopeType);
        }
    }
}