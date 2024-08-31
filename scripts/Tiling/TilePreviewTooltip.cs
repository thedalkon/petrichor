using System.Diagnostics;
using Godot;
using Petrichor.scripts.Geometry;
using static Petrichor.scripts.Petrichor;

namespace Petrichor.scripts.Tiling;

public partial class TilePreviewTooltip : Control
{
    private Label _tileLabel = new();
    
    public TilePreviewTooltip()
    {
        Material = ResourceLoader.Load<ShaderMaterial>("res://materials/tile_preview_mat.tres", null,
            ResourceLoader.CacheMode.Ignore);
        TextureFilter = TextureFilterEnum.Nearest;
        ZIndex = 1;
    }

    public override void _EnterTree()
    {
        AddChild(_tileLabel);
        _tileLabel.Position = new Vector2(16, -16);
    }

    public override void _Process(double delta)
    {
        if (!GeometryEditor.IsInLevelRect() || DraggableControl.OverControl || TileEditor.CurrentTileTex == null)
        {
            Visible = false;
        }
        else
        {
            Visible = true;
            GlobalPosition = TiledMousePos;
            QueueRedraw();
        }
        
        if (!GeometryEditor.IsInLevelRect(out Vector2I cellPos))
            return;
        
        string hoveredTile = TileEditor.Data[CurrentLayer, cellPos.X, cellPos.Y];
        _tileLabel.Text = !string.IsNullOrWhiteSpace(hoveredTile) ? hoveredTile : "";
    }

    public override void _Draw()
    {   
        DrawTextureRectRegion(TileEditor.CurrentTileTex, 
            new Rect2((-(TileEditor.CurrentTile.Size * 0.5f).Ceil() + Vector2.One) * ZoomFactor, TileEditor.CurrentTile.Size * ZoomFactor), 
            TileEditor.CurrentTile.PreviewRect, TileEditor.CurrentTile.Color);

        DrawLine(new Vector2(0, LevelRect.Position.Y - GlobalPosition.Y), new Vector2(0, LevelRect.End.Y - GlobalPosition.Y), Colors.White with {A = 0.1f});
        DrawLine(new Vector2(LevelRect.Position.X - GlobalPosition.X, 0), new Vector2(LevelRect.End.X - GlobalPosition.X, 0), Colors.White with {A = 0.1f});
        DrawLine(new Vector2(ZoomFactor, LevelRect.Position.Y - GlobalPosition.Y), new Vector2(ZoomFactor, LevelRect.End.Y - GlobalPosition.Y), Colors.White with {A = 0.1f});
        DrawLine(new Vector2(LevelRect.Position.X - GlobalPosition.X, ZoomFactor), new Vector2(LevelRect.End.X - GlobalPosition.X, ZoomFactor), Colors.White with {A = 0.1f});
    }
}