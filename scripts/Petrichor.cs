﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot;
using Petrichor.scripts.GeoEditor;

namespace Petrichor.scripts;

public partial class Petrichor : Control
{
    public static UserData UserData = new();
    private string _userDataPath = OS.GetUserDataDir() + "/userdata.data";
    
    private Timer _oneSecTimer;

    private static Dictionary<string, Editor> _editors;

    public static Editor CurrentEditor;
    
    public static List<Layer> LayerInstances = new();

    // ReSharper disable once InconsistentNaming
    private static float _zoomFactor = 16.0f;
    public static float ZoomFactor
    {
        get => _zoomFactor;
        set
        {
            _zoomFactor = value;
            foreach (Layer layer in LayerInstances)
            {
                layer.Scale = Vector2.One * _zoomFactor / 16.0f;
            }
        }
    }

    private static Rect2 _levelRect;
    public static Rect2 LevelRect
    {
        get => _levelRect;
        private set
        {
            _levelRect = value;
            GridRect.GlobalPosition = value.Position;
            GridRect.SetDeferred("size", value.Size);
            ((ShaderMaterial)GridRect.Material).SetShaderParameter("grid_size", value.Size);
            ((ShaderMaterial)GridRect.Material).SetShaderParameter("cell_size", ZoomFactor);
        }
    }

    public static int CurrentLayer;
    
    public static Vector2 MousePos = Vector2.Zero;
    public static Vector2 TiledMousePos = Vector2.Zero;
    public static Vector2 RelativeMousePos = Vector2.Zero;
    
    private static Vector2 _cameraPos = Vector2.Zero;
    
    public static Vector2 CameraPos
    {
        get => _cameraPos;
        private set
        {
            _cameraPos = value;
            foreach (Layer layer in LayerInstances)
            {
                layer.GlobalPosition = CameraPos;
            }
            GridRect.GlobalPosition = value;
        }
    }

    public static ColorRect GridRect;

    public override void _EnterTree()
    {
        if (!File.Exists(_userDataPath))
        {
            Debug.WriteLine(Utils.INFO_STR + "No user data found, creating new.");
            return;
        }
        
        Debug.WriteLine(Utils.INFO_STR + "Loading user data.");
        UserData = UserData.Load(_userDataPath);
        Debug.WriteLine(Utils.INFO_STR + "User data loaded.");
    }

    public override void _ExitTree()
    {
        Debug.WriteLine(Utils.INFO_STR + "Saving user data.");
        UserData.Save(_userDataPath);
    }

    public override void _Ready()
    {
        DisplayServer.WindowSetTitle("Petrichor");
        _oneSecTimer = new Timer() { OneShot = false, Autostart = true };
        AddChild(_oneSecTimer);
        _oneSecTimer.Timeout += UpdatePerformanceLabels;
        GridRect = GetNode<ColorRect>("%GridRect");

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
        MousePos = GetViewport().GetMousePosition();
        TiledMousePos = ((MousePos - new Vector2(ZoomFactor, ZoomFactor) - CameraPos.PosMod(ZoomFactor)) / 
                          ZoomFactor).Ceil() * ZoomFactor + CameraPos.PosMod(ZoomFactor);
        RelativeMousePos = MousePos - CameraPos;
        
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
            GetNode<Label>("%LayerLabel").Text = "Layer: " + (CurrentLayer + 1);
            foreach (var layer in LayerInstances)
            {
                layer.LayerChanged();
            }
        }
        else if (Input.IsActionJustPressed("last_layer"))
        {
            CurrentLayer -= 1;
            if (CurrentLayer < 0) CurrentLayer = 2;
            GetNode<Label>("%LayerLabel").Text = "Layer: " + (CurrentLayer + 1);
            foreach (var layer in LayerInstances)
            {
                layer.LayerChanged();
            }
        }

        if (Input.IsActionJustPressed("reset_camera"))
        {
            ZoomFactor = 16.0f;
            CameraPos = GetViewport().GetVisibleRect().GetCenter() - (Vector2)GeometryEditor.LevelSize * ZoomFactor * 0.5f;
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

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("scroll_up"))
        {
            ZoomFactor = Mathf.Clamp(ZoomFactor + 1.0f, 0.0f, Mathf.Inf);
            LevelRect = new Rect2(CameraPos, (Vector2)GeometryEditor.LevelSize * ZoomFactor);
            GetNode<Label>("%ZoomLabel").Text = "Zoom: " + Math.Round(ZoomFactor / 16.0f * 100.0f, 3) + "%"; 
        }
        if (Input.IsActionJustPressed("scroll_down"))
        {
            ZoomFactor = Mathf.Clamp(ZoomFactor - 1.0f, 0.0f, Mathf.Inf);
            LevelRect = new Rect2(CameraPos, (Vector2)GeometryEditor.LevelSize * ZoomFactor);
            GetNode<Label>("%ZoomLabel").Text = "Zoom: " + Math.Round(ZoomFactor / 16.0f * 100.0f, 3) + "%";
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
        GetNode<Label>("%PerformanceLabel").Text = 
            "RAM: " + ramUsage + "MB\n" +
            "FPS: " + Engine.GetFramesPerSecond() + "\n" +
            "Draw Calls: " + Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
    }
}