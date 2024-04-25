using System;
using Godot;

namespace Petrichor.scripts.GeoEditor.GeoTools;

public class SelectRectGeoTool : GeoTool
{
    Vector2I _startCell = Vector2I.Zero;
    Vector2I _endCell = Vector2I.Zero;
    Vector2I _rectSize = Vector2I.One;
    private bool _selecting = false;
    
    public override void ApplyTool(Vector2I cellPos, int layer)
    {
        if (Input.IsActionJustPressed("mouse_any"))
        {
            _startCell = cellPos;
            _selecting = true;
            GeoEditor.GeometryEditor.GeoTooltip.OverrideTooltip = true;
        }

        if (!_selecting) return;
        
        _endCell = cellPos;
        GeoEditor.GeometryEditor.GeoTooltip.TooltipRect = new Rect2((Vector2)_startCell * Petrichor.ZoomFactor + Petrichor.CameraPos,
            (Vector2)(_endCell - _startCell) * Petrichor.ZoomFactor + Vector2.One * Petrichor.ZoomFactor);
        _rectSize = (Vector2I)(GeoEditor.GeometryEditor.GeoTooltip.TooltipRect.Size / Petrichor.ZoomFactor);
        
        GeoEditor.GeometryEditor.GeoTooltip.TooltipText =
            "x: " + _rectSize.X + " y: " + _rectSize.Y;
    }

    public override void EndApplyTool(Vector2I cellPos, int layer)
    {
        _selecting = false;
        if (_rectSize.X == 0 || _rectSize.Y == 0) return;
        
        Vector2I topLeft = _startCell + _rectSize with
        {
            X = Math.Min(_rectSize.X, 0),
            Y = Math.Min(_rectSize.Y, 0)
        };

        Vector2 absRectSize = _rectSize.Abs();
        
        string clipboardCopy = "PTCHR;" + absRectSize.X + ";" + absRectSize.Y + ";";

        for (int x = 0; x < absRectSize.X; x++)
        {
            for (int y = 0; y < absRectSize.Y; y++)
            {
                clipboardCopy += GeoEditor.GeometryEditor.Data[layer, topLeft.X + x, topLeft.Y + y].ToString() + ";";
            }
        }
        
        DisplayServer.ClipboardSet(clipboardCopy);
        
        GeoEditor.GeometryEditor.GeoTooltip.TooltipText = "Selection copied to clipboard";
    }
}