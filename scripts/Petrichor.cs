using System;
using System.Collections.Generic;
using Godot;
using Petrichor.scripts.GeoEditor;

namespace Petrichor.scripts;

public partial class Petrichor : Control
{
    private Timer _oneSecTimer;

    private static Dictionary<string, Editor> _editors;

    public static Editor CurrentEditor;
    
    public static List<GeoLayer> GeoLayerInstances = new();

    // ReSharper disable once InconsistentNaming
    public static float ZoomFactor = 16.0f;
    public static Rect2 LevelRect;
    public static int CurrentLayer;
    
    private static Vector2 _cameraPos = Vector2.Zero;
    public static Vector2 CameraPos
    {
        get => _cameraPos;
        set
        {
            _cameraPos = value;
            foreach (GeoLayer layer in GeoLayerInstances)
            {
                layer.GlobalPosition = CameraPos;
            }
        }
    }
    
    public override void _Ready()
    {
        DisplayServer.WindowSetTitle("Petrichor");
        _oneSecTimer = new Timer() { OneShot = false, Autostart = true };
        AddChild(_oneSecTimer);
        _oneSecTimer.Timeout += UpdatePerformanceLabels;

        _editors = new Dictionary<string, Editor>
        {
            {"open_geo_editor", GetNode<Editor>("GeoEditor")},
            {"open_tile_editor", GetNode<Editor>("TileEditor")},
        };
        
        CameraPos = GetViewport().GetVisibleRect().GetCenter() - (Vector2)GeometryEditor.LevelSize * ZoomFactor * 0.5f;
        LevelRect = new Rect2(CameraPos, (Vector2)GeometryEditor.LevelSize * ZoomFactor);
    }

    public override void _Process(double delta)
    {
        // Changing Editors
        foreach (string action in _editors.Keys)
        {
            if (Input.IsActionJustPressed(action))
            {
                SetEditor(action);
            }
        }
        
        // Changing layers
        if (Input.IsActionJustPressed("next_layer"))
        {
            CurrentLayer += 1;
            if (CurrentLayer >= 3) CurrentLayer = 0;
            //GetNode<Label>("%LayerLabel").Text = "Layer: " + (CurrentLayer + 1);
            foreach (var layer in GeoLayerInstances)
            {
                layer.LayerChanged();
            }
        }
        else if (Input.IsActionJustPressed("last_layer"))
        {
            CurrentLayer -= 1;
            if (CurrentLayer < 0) CurrentLayer = 2;
            foreach (var layer in GeoLayerInstances)
            {
                layer.LayerChanged();
            }
        }

        if (Input.IsActionJustPressed("reset_camera"))
        {
            ZoomFactor = 16.0f;
            CameraPos = GetViewport().GetVisibleRect().GetCenter() - (Vector2)GeometryEditor.LevelSize * ZoomFactor * 0.5f;
            GeometryEditor.RedrawTerrain();
            LevelRect = new Rect2(CameraPos, (Vector2)GeometryEditor.LevelSize * ZoomFactor);
        }

        if (Input.IsActionJustPressed("scroll_up"))
        {
            ZoomFactor = Mathf.Clamp(ZoomFactor + 1.0f, 0.0f, Mathf.Inf);
            GeometryEditor.RedrawTerrain();
            LevelRect = new Rect2(CameraPos, (Vector2)GeometryEditor.LevelSize * ZoomFactor);
            GetNode<Label>("%ZoomLabel").Text = "Zoom: " + Math.Round(ZoomFactor / 16.0f * 100.0f, 3) + "%"; 
        }
        if (Input.IsActionJustPressed("scroll_down"))
        {
            ZoomFactor = Mathf.Clamp(ZoomFactor - 1.0f, 0.0f, Mathf.Inf);
            GeometryEditor.RedrawTerrain();
            LevelRect = new Rect2(CameraPos, (Vector2)GeometryEditor.LevelSize * ZoomFactor);
            GetNode<Label>("%ZoomLabel").Text = "Zoom: " + Math.Round(ZoomFactor / 16.0f * 100.0f, 3) + "%";
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("ui_drag") && @event is InputEventMouseMotion motionEvent)
        {
            CameraPos += motionEvent.Relative;
            LevelRect = new Rect2(CameraPos, (Vector2)GeometryEditor.LevelSize * ZoomFactor);
        }
    }
    
    private void SetEditor(string action)
    {
        foreach (Editor editor in _editors.Values)
        {
            editor.Hide();
            editor.ProcessMode = ProcessModeEnum.Disabled;
        }

        CurrentEditor = _editors[action];
        CurrentEditor.Show();
        CurrentEditor.ProcessMode = ProcessModeEnum.Inherit;
        GetNode<Label>("%ControlsLabel").Text = CurrentEditor.Controls;
        DisplayServer.WindowSetTitle("Petrichor (" + CurrentEditor.Name + ")");
    }
    
    private void UpdatePerformanceLabels()
    {
        float ramUsage = MathF.Round(OS.GetStaticMemoryUsage() * 0.000001f, 2);
        GetNode<Label>("%PerformanceLabel").Text = "RAM: " + ramUsage + "MB";
    }
}