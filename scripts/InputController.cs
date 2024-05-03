using System.Diagnostics;
using Godot;

namespace Petrichor.scripts;

public partial class InputController : Node
{
    private bool _usingController;
    private Vector2 _joyMousePos;
    
    public override void _Ready()
    {
        _joyMousePos = GetViewport().GetMousePosition();
        if (Input.GetConnectedJoypads().Count > 0)
        {
            _usingController = true;
        }
    }

    public override void _Process(double delta)
    {
        if (!_usingController)
            return;
        
        Vector2 joystickMotion = new Vector2(Input.GetJoyAxis(0, JoyAxis.LeftX), Input.GetJoyAxis(0, JoyAxis.LeftY));

        if (joystickMotion.Length() < 0.2f)
            return;
        
        _joyMousePos += joystickMotion;
        Input.WarpMouse(_joyMousePos);
    }
}