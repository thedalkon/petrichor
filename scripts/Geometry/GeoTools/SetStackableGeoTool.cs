using Godot;

namespace Petrichor.scripts.Geometry.GeoTools;

public class SetStackableGeoTool : GeoTool
{
    private readonly StackableTypes _stackableType;
    private bool adding = true;

    public SetStackableGeoTool(StackableTypes stackableType)
    {
        _stackableType = stackableType;
    }
    
    public override void ApplyTool(Vector2I cellPos, int layer)
    {
        if (Input.IsActionJustPressed("mouse_any"))
        {
            adding = !GeometryEditor.HasStackable(layer, cellPos.X, cellPos.Y, _stackableType);
        }
        
        if (adding)
        {
            GeometryEditor.AddStackable(layer, cellPos.X, cellPos.Y, _stackableType);
            GeometryEditor.Layers[Petrichor.CurrentLayer].AddStackableMesh(GeometryEditor.StackableTextures[_stackableType], cellPos.X, cellPos.Y);
        }
        else 
        {
            GeometryEditor.RemoveStackable(layer, cellPos.X, cellPos.Y, _stackableType);
            GeometryEditor.Layers[Petrichor.CurrentLayer].RemoveStackableMesh(GeometryEditor.StackableTextures[_stackableType], cellPos.X, cellPos.Y);
        }
        
        
    }
}