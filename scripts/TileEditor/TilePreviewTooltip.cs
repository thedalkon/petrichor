using Godot;
using Petrichor.scripts.GeoEditor;
using static Petrichor.scripts.Petrichor;

namespace Petrichor.scripts.TileEditor;

public partial class TilePreviewTooltip : Control
{
    public TilePreviewTooltip()
    {
        Material = ResourceLoader.Load<ShaderMaterial>("res://materials/tile_layer_mat.tres", null,
            ResourceLoader.CacheMode.Ignore);
        TextureFilter = TextureFilterEnum.Nearest;
    }
    
    public override void _Draw()
    {
        if (!GeometryEditor.IsInLevelRect() || DraggableControl.OverControl)
            return;
        
        DrawTextureRectRegion(TileEditor.CurrentTileTex, 
            new Rect2(TiledMousePos, TileEditor.CurrentTile.Size * ZoomFactor), TileEditor.CurrentTile.PreviewRect,
            TileEditor.CurrentTile.Color);
        
        DrawLine(new Vector2(TiledMousePos.X, LevelRect.Position.Y), new Vector2(TiledMousePos.X, LevelRect.End.Y), Colors.White with {A = 0.1f});
        DrawLine(new Vector2(LevelRect.Position.X, TiledMousePos.Y), new Vector2(LevelRect.End.X, TiledMousePos.Y), Colors.White with {A = 0.1f});
        DrawLine(new Vector2(TiledMousePos.X + ZoomFactor, LevelRect.Position.Y), new Vector2(TiledMousePos.X + ZoomFactor, LevelRect.End.Y), Colors.White with {A = 0.1f});
        DrawLine(new Vector2(LevelRect.Position.X, TiledMousePos.Y + ZoomFactor), new Vector2(LevelRect.End.X, TiledMousePos.Y + ZoomFactor), Colors.White with {A = 0.1f});
        
    }
}