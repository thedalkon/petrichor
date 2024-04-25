using Godot;

namespace Petrichor.scripts.GeoEditor.GeoTools;

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
            adding = !GeoEditor.GeometryEditor.HasStackable(layer, cellPos.X, cellPos.Y, _stackableType);
        }
        
        if (adding)
            GeoEditor.GeometryEditor.AddStackable(layer, cellPos.X, cellPos.Y, _stackableType);
        else 
            GeoEditor.GeometryEditor.RemoveStackable(layer, cellPos.X, cellPos.Y, _stackableType);
        
        GeometryEditor.RedrawTerrain();
    }
}