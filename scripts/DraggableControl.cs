using System.Diagnostics;
using Godot;

namespace Petrichor.scripts;

public partial class DraggableControl : Control
{
    
    [Export]
    public bool DragParent = true;

    private static DraggableControl _currentDrag = null;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("mouse_any") && GetGlobalRect().HasPoint(GetViewport().GetMousePosition()))
        {
            _currentDrag = this;
        }
        if (Input.IsActionJustReleased("mouse_any"))
        {
            _currentDrag = null;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && _currentDrag == this)
        {
            if (DragParent)
                GetParentControl().GlobalPosition += motion.Relative;
            else
                GlobalPosition += motion.Relative;
        }
    }

    public static bool IsDragging => _currentDrag != null;
}