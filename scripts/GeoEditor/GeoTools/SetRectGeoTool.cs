using System;
using Godot;

namespace Petrichor.scripts.GeoEditor.GeoTools;

public class SetRectGeoTool : GeoTool
{
    Vector2I _startCell = Vector2I.Zero;
    Vector2I _endCell = Vector2I.Zero;
    Vector2I _rectSize = Vector2I.One;
    private readonly CellTypes _cellType;

    public SetRectGeoTool(CellTypes cellType)
    {
        _cellType = cellType;
    }
    
    public override void ApplyTool(Vector2I cellPos, int layer)
    {
        if (Input.IsActionJustPressed("mouse_any"))
        {
            _startCell = cellPos;
            GeometryEditor.GeoTooltip.OverrideTooltip = true;
        }

        _endCell = cellPos;

        GeometryEditor.GeoTooltip.TooltipRect = new Rect2((Vector2)_startCell * Petrichor.ZoomFactor + Petrichor.CameraPos,
            (Vector2)(_endCell - _startCell) * Petrichor.ZoomFactor + Vector2.One * Petrichor.ZoomFactor);

        _rectSize = (Vector2I)(GeometryEditor.GeoTooltip.TooltipRect.Size / Petrichor.ZoomFactor);
        
        GeometryEditor.GeoTooltip.TooltipText =
            "x: " + _rectSize.X + " y: " + _rectSize.Y;
    }

    public override void EndApplyTool(Vector2I cellPos, int layer)
    {
        GeometryEditor.GeoTooltip.OverrideTooltip = false;
        if (_rectSize.X == 0 || _rectSize.Y == 0) return;

        Vector2I growSign = _rectSize.Sign();
        if (growSign.X == -1) _startCell.X -= 1;
        if (growSign.Y == -1) _startCell.Y -= 1;
        
        for (int x = 0; x < Math.Abs(_rectSize.X); x++)
        {
            for (int y = 0; y < Math.Abs(_rectSize.Y); y++)
            {
                GeometryEditor.Data[layer, _startCell.X + x * growSign.X, _startCell.Y + y * growSign.Y].Type = (int)_cellType;
            }
        }
        
        GeometryEditor.RedrawTerrain();
    }
}