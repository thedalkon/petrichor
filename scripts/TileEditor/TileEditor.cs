﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot;
using Petrichor.scripts.GeoEditor;
using Petrichor.scripts.Lingo;
using Vector2 = Godot.Vector2;

namespace Petrichor.scripts.TileEditor;

public partial class TileEditor : Editor
{
    public override string Controls =>
        "Left click: Place tile\n" +
        "Right click: Delete tile\n" +
        "Q: Toggle render view\n" +
        "W: Increase layer\n" +
        "S: Decrease layer\n" +
        "R: Center Camera\n";
    
    public static LingoCategory[] TileCategories;
    private static string _currentTile = "";
    public static Texture2D CurrentTileTex => GetTileTexture(_currentTile);
    public static Tile CurrentTile
    {
        get
        {
            if (!Tiles.Keys.Contains(_currentTile))
                return new Tile();
            return Tiles[_currentTile];
        }
    }

    private TilePreviewTooltip _tilePreview = new();

    private static readonly TileLayer[] Layers =
    {
        new(2),
        new(1),
        new(0)
    };

    private static readonly Dictionary<string, string> TilePaths = new();
    public static readonly Dictionary<string, Tile> Tiles = new();
    public static readonly Dictionary<string, Texture2D> TileTextures = new();

    public static string[,,] Data;
    
    public static bool RenderMode = false;
    
    public override void _Ready()
    {
        Data = new string[3, GeometryEditor.LevelSize.X, GeometryEditor.LevelSize.Y];
        Data.Fill3D("");
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        TileCategories = LingoParser.ParseFromFile(ProjectSettings.GlobalizePath("res://tiles/Init.txt"));
        Console.WriteLine(Utils.CHECK_STR + "Init.txt parsed in " + Math.Round(stopwatch.Elapsed.TotalMilliseconds) + "ms");

        Tree tilesTree = GetNode<Tree>("%TilesTree");
        
        tilesTree.CreateItem();
        tilesTree.HideRoot = true;
        
        GetNode<LineEdit>("%SearchLine").TextChanged += SetTileSearch;
        tilesTree.ItemSelected += TileSelected;
        
        AddChild(_tilePreview);
        
        foreach (var layer in Layers)
        {
            AddChild(layer);
        }

        GeometryEditor.TerrainRedrawn += RedrawTerrain;
        
        stopwatch.Restart();
        
        // Set up tiles
        foreach (LingoCategory category in TileCategories)
        {
            if (category == null)
                return;
            
            TreeItem categoryItem = GetNode<Tree>("%TilesTree").CreateItem();
            categoryItem.SetText(0, category.Name);
            categoryItem.SetCustomColor(0, category.Color);
            categoryItem.Collapsed = true;
        
            foreach (LingoPropertyList propertyList in category.Collections)
            {
                if (propertyList.GetProperty("nm") is not { } nameProperty)
                    return;

                string tileName = (string)nameProperty.Value;

                if (!Tiles.ContainsKey(tileName))
                {
                    Tile tile = new Tile(propertyList, category.Color);
                    Tiles.Add(tileName, tile);
                }
            
                string globalTilePath =
                    ProjectSettings.GlobalizePath("res://tiles/" + tileName + ".png");
            
                // Check tile textures
                if (!File.Exists(globalTilePath))
                {
                    Debug.WriteLine(Utils.WARNING_STR + "Texture file not found for tile " + tileName +
                                    ", check if the file exists and has the correct name.");
                    return;
                }
                
                ThreadedLoader.LoadFileAsync(globalTilePath);
                TilePaths.TryAdd(tileName, globalTilePath);
            
                TreeItem collectionItem = GetNode<Tree>("%TilesTree").CreateItem(categoryItem);
                collectionItem.SetText(0, tileName);
                collectionItem.SetIcon(0, ResourceLoader.Load<Texture2D>("res://textures/TreeItemIcon.png"));
                collectionItem.SetIconModulate(0, category.Color);
            }
        }
        
        Console.WriteLine(Utils.CHECK_STR + "Tiles checked in " + Math.Round(stopwatch.Elapsed.TotalMilliseconds) + "ms");
    }

    public override void _Process(double delta)
    {
        QueueRedraw();

        if (Input.IsActionPressed("mouse_left") && !DraggableControl.IsDragging && !DraggableControl.OverControl)
        {
            if (!GeometryEditor.IsInLevelRect(out Vector2I cellPos)) return;
            Data[Petrichor.CurrentLayer, cellPos.X, cellPos.Y] = CurrentTile.Name;
            RedrawTerrain(null, EventArgs.Empty);
        }
        else if (Input.IsActionPressed("mouse_right") && !DraggableControl.IsDragging && !DraggableControl.OverControl)
        {
            if (!GeometryEditor.IsInLevelRect(out Vector2I cellPos)) return;
            Data[Petrichor.CurrentLayer, cellPos.X, cellPos.Y] = "";
            RedrawTerrain(null, EventArgs.Empty);
        }
        else if (Input.IsActionJustPressed("toggle_view"))
        {
            RenderMode = !RenderMode;
            RedrawTerrain(null, EventArgs.Empty);
            foreach (var layer in Petrichor.LayerInstances)
            {
                layer.LayerChanged();
            }
        }
    }

    public override void _Draw()
    {
        DrawRect(Petrichor.LevelRect.Grow(3), Colors.White, false, 2);

        if (string.IsNullOrWhiteSpace(_currentTile))
            return;
        
        _tilePreview.QueueRedraw();
    }

    public static Texture2D GetTileTexture(string tileName)
    {
        if (string.IsNullOrWhiteSpace(tileName))
            return new Texture2D();
        
        if (TileTextures.TryGetValue(tileName, out Texture2D texture)) 
            return texture;

        TileTextures.Add(tileName, (Texture2D)ResourceLoader.Load(TilePaths[tileName]));
        return TileTextures[tileName];
    }

    private static void RedrawTerrain(object sender, EventArgs args)
    {
        Layers[2].QueueRedraw();
        Layers[1].QueueRedraw();
        Layers[0].QueueRedraw();
    }

    private void SetTileSearch(string searchTerm)
    {
        
        searchTerm = searchTerm.ToLower();
        
        TreeItem[] categoryItems = GetNode<Tree>("%TilesTree").GetRoot().GetChildren().ToArray();
        for (int i = 0; i < categoryItems.Length; i++)
        {
            TreeItem[] categoryTiles = categoryItems[i].GetChildren().ToArray();

            if (categoryItems[i].GetText(0).ToLower().Contains(searchTerm))
            {
                categoryItems[i].Visible = true;
                for (int j = 0; j < categoryTiles.Length; j++)
                {
                    categoryTiles[j].Visible = true;
                }
                continue;
            }
            
            int childrenVisible = 0;
            for (int j = 0; j < categoryTiles.Length; j++)
            {
                if (categoryTiles[j].GetText(0).ToLower().Contains(searchTerm))
                {
                    categoryTiles[j].Visible = true;
                    childrenVisible++;
                }
                else
                {
                    categoryTiles[j].Visible = false;
                }
            }

            categoryItems[i].Visible = childrenVisible > 0;
        }
    }

    private void TileSelected()
    {
        TreeItem selectedItem = GetNode<Tree>("%TilesTree").GetSelected();
        if (selectedItem.GetParent() == GetNode<Tree>("%TilesTree").GetRoot())
            return;
        
        _currentTile = selectedItem.GetText(0);
        GetNode<RichTextLabel>("%TileLabel").Text =
            "Current Tile:[color=#" + selectedItem.GetParent().GetCustomColor(0).ToHtml() + "] " + _currentTile;
    }
}

public readonly struct Tile
{
    public readonly string Name = "";
    public readonly Godot.Vector2 Size = Godot.Vector2.Zero;
    public readonly int[] Specs = Array.Empty<int>();
    public readonly int[] Specs2 = Array.Empty<int>();
    public readonly string Type = "";
    public readonly int BfTiles = 0;
    public readonly int Random = 0;
    public readonly int PointPos = 0;
    public readonly string[] Tags = Array.Empty<string>();
    public readonly Color Color = Colors.White;
    private readonly int[] _repeatLines = Array.Empty<int>();
    public int[] RepeatLines
    {
        get
        {
            return Type switch
            {
                "voxelStructRockType" => new[] { 10 },
                "box" => new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                "voxelStruct" => _repeatLines,
                _ => Array.Empty<int>()
            };
        }
    }

    public Vector2 RenderSize => (Size + Vector2.One * BfTiles * 2) * 20;
    
    public Rect2 PreviewRect => new(0, (Size.Y + BfTiles * 2) * RepeatLines.Length * 20 + 1, Size * 16);

    public Rect2[] RenderRects
    {
        get
        {
            Rect2[] rects = new Rect2[RepeatLines.Length];
            int j = 0;
            for (int i = rects.Length; i > 0; i--)
            {
                rects[j] = new Rect2(0, (i - 1) * RenderSize.Y + 1, RenderSize);
                j++;
            }
            return rects;
        }
    }
    
    public Tile(LingoPropertyList propertyList, Color color)
    {
        Color = color;
        for (int i = 0; i < propertyList.Properties.Length; i++)
        {
            LingoProperty property = propertyList.Properties[i];
            switch (property.Tag)
            {
                case "nm": Name = (string)property.Value;
                    break;
                case "sz": Size = (Vector2)property.Value;
                    break;
                case "specs": Specs = ((LingoLinearList)property.Value).ToArray<int>();
                    break;
                case "specs2":
                    Specs2 = property.Value is int ? null : ((LingoLinearList)property.Value).ToArray<int>();
                    break;
                case "tp": Type = (string)property.Value;
                    break;
                case "repeatL": _repeatLines = ((LingoLinearList)property.Value).ToArray<int>();
                    break;
                case "bfTiles": BfTiles = (int)property.Value;
                    break;
                case "rnd": Random = (int)property.Value;
                    break;
                case "ptPos": PointPos = (int)property.Value;
                    break;
                case "tags": Tags = ((LingoLinearList)property.Value).ToArray<string>();
                    break;
            }
        }
    }
}