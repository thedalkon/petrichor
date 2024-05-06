using Godot;

namespace Petrichor.scripts.GeoEditor.GeoTools;

public class EraseAllGeoTool : GeoTool
{
    public override void ApplyTool(Vector2I cellPos, int layer)
    {
        Cell cell = GeometryEditor.GetCell(layer, cellPos);
        if (cell.Type == (int)CellTypes.Air && cell.Stackables == 0)
            return;

        GeometryEditor.Layers[Petrichor.CurrentLayer].SetCellMesh(GeometryEditor.CellTextures[CellTypes.Air], cellPos.X, cellPos.Y);
        
        GeometryEditor.Data[layer, cellPos.X, cellPos.Y].Type = 0;
        GeometryEditor.Data[layer, cellPos.X, cellPos.Y].Stackables = 0;

        int[] stackables = Utils.FlagsHelper.DecomposeFlag(cell.Stackables).ToArray();
        foreach (int stackable in stackables)
        {
            GeometryEditor.Layers[Petrichor.CurrentLayer].RemoveStackableMesh(GeometryEditor.StackableTextures[(StackableTypes)stackable], cellPos.X, cellPos.Y);
        }
    }
}