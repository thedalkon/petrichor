using Godot;
using static Petrichor.scripts.Geometry.GeometryEditor;
using static Petrichor.scripts.Petrichor;

namespace Petrichor.scripts;

public partial class Tooltip : Control
{
    public bool OverrideTooltip;
    public Rect2 TooltipRect;
    public new string TooltipText;

    public override void _Draw()
    {
        if (!OverrideTooltip)
        {
            DrawRect(new Rect2(TiledMousePos, new Vector2(ZoomFactor, ZoomFactor)), Colors.Red, false, 2);
		    
            DrawLine(new Vector2(TiledMousePos.X, LevelRect.Position.Y), new Vector2(TiledMousePos.X, LevelRect.End.Y), Colors.White with {A = 0.1f});
            DrawLine(new Vector2(LevelRect.Position.X, TiledMousePos.Y), new Vector2(LevelRect.End.X, TiledMousePos.Y), Colors.White with {A = 0.1f});
            DrawLine(new Vector2(TiledMousePos.X + ZoomFactor, LevelRect.Position.Y), new Vector2(TiledMousePos.X + ZoomFactor, LevelRect.End.Y), Colors.White with {A = 0.1f});
            DrawLine(new Vector2(LevelRect.Position.X, TiledMousePos.Y + ZoomFactor), new Vector2(LevelRect.End.X, TiledMousePos.Y + ZoomFactor), Colors.White with {A = 0.1f});
            
            DrawString(Utils.CozzetteFont,  TiledMousePos + new Vector2(ZoomFactor + 4, -4),
                "x:" + Mathf.Floor(RelativeMousePos.X / ZoomFactor) + " y:" + Mathf.Floor(RelativeMousePos.Y / ZoomFactor),
                HorizontalAlignment.Left, -1F, 13, Colors.White);
        }
        else
        {
            DrawRect(TooltipRect, Colors.Red, false, 2);
            
            DrawLine(new Vector2(TooltipRect.Position.X, LevelRect.Position.Y), new Vector2(TooltipRect.Position.X, LevelRect.End.Y), Colors.White with {A = 0.1f});
            DrawLine(new Vector2(LevelRect.Position.X, TooltipRect.Position.Y), new Vector2(LevelRect.End.X, TooltipRect.Position.Y), Colors.White with {A = 0.1f});
            DrawLine(new Vector2(TooltipRect.End.X, LevelRect.Position.Y), new Vector2(TooltipRect.End.X, LevelRect.End.Y), Colors.White with {A = 0.1f});
            DrawLine(new Vector2(LevelRect.Position.X, TooltipRect.End.Y), new Vector2(LevelRect.End.X, TooltipRect.End.Y), Colors.White with {A = 0.1f});
            
            DrawString(Utils.CozzetteFont,  TiledMousePos + new Vector2(ZoomFactor + 4, -4), TooltipText,
                HorizontalAlignment.Left, -1F, 13, Colors.White);
        }
    }
}