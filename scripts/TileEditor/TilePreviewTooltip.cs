using System.Diagnostics;
using Godot;
using Petrichor.scripts.GeoEditor;
using static Petrichor.scripts.Petrichor;

namespace Petrichor.scripts.TileEditor;

public partial class TilePreviewTooltip : Control
{
    public TilePreviewTooltip()
    {
        Material = ResourceLoader.Load<ShaderMaterial>("res://materials/tile_preview_mat.tres", null,
            ResourceLoader.CacheMode.Ignore);
        TextureFilter = TextureFilterEnum.Nearest;
        ZIndex = 0;
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
    }

    public override void _Draw()
    {   
        DrawTextureRectRegion(TileEditor.CurrentTileTex, 
            new Rect2(Vector2.Up, TileEditor.CurrentTile.Size * ZoomFactor), TileEditor.CurrentTile.PreviewRect, TileEditor.CurrentTile.Color);

        DrawLine(new Vector2(0, LevelRect.Position.Y - GlobalPosition.Y), new Vector2(0, LevelRect.End.Y - GlobalPosition.Y), Colors.Red with {A = 0.1f});
        DrawLine(new Vector2(LevelRect.Position.X - GlobalPosition.X, 0), new Vector2(LevelRect.End.X - GlobalPosition.X, 0), Colors.Red with {A = 0.1f});
        DrawLine(new Vector2(ZoomFactor, LevelRect.Position.Y - GlobalPosition.Y), new Vector2(ZoomFactor, LevelRect.End.Y - GlobalPosition.Y), Colors.Red with {A = 0.1f});
        DrawLine(new Vector2(LevelRect.Position.X - GlobalPosition.X, ZoomFactor), new Vector2(LevelRect.End.X - GlobalPosition.X, ZoomFactor), Colors.Red with {A = 0.1f});
    }
}