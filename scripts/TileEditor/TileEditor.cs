using System;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using Petrichor.scripts.GeoEditor;
using Petrichor.scripts.Lingo;

namespace Petrichor.scripts.TileEditor;

public partial class TileEditor : Editor
{
    public override string Controls =>
        "W: Increase layer\n" +
        "S: Decrease layer\n" +
        "R: Center Camera\n";
    
    public static LingoCategory[] TileCategories;

    private static readonly GeoLayer[] Layers =
    {
        new(2),
        new(1),
        new(0)
    };
    
    public override void _Ready()
    {
        Stopwatch tileStopwatch = Stopwatch.StartNew();
        TileCategories = LingoParser.ParseFromFile(ProjectSettings.GlobalizePath("res://tiles/Init.txt"));
        Console.WriteLine(Utils.CHECK_STR + "Tiles parsed in " + tileStopwatch.Elapsed.Milliseconds + "ms");

        GetNode<Tree>("%TilesTree").CreateItem();
        GetNode<Tree>("%TilesTree").HideRoot = true;
        GetNode<LineEdit>("%SearchLine").TextChanged += SetTileSearch;
        
        foreach (var layer in Layers)
        {
            AddChild(layer);
        }

        GeometryEditor.TerrainRedrawn += RedrawTerrain;
        
        // Set up tile tree
        for (int i = 0; i < TileCategories.Length; i++)
        {
            if (TileCategories[i] == null)
                continue;
            
            TreeItem categoryItem = GetNode<Tree>("%TilesTree").CreateItem();
            categoryItem.SetText(0, TileCategories[i].Name);
            categoryItem.SetCustomColor(0, TileCategories[i].Color);
            categoryItem.Collapsed = true;

            for (int j = 0; j < TileCategories[i].Collections.Length; j++)
            {
                TreeItem collectionItem = GetNode<Tree>("%TilesTree").CreateItem(categoryItem);
                if (TileCategories[i].Collections[j].GetProperty("nm") is not { } nameProperty)
                    return;
                collectionItem.SetText(0, (string)nameProperty.Value);
                collectionItem.SetIcon(0, ResourceLoader.Load<Texture2D>("res://textures/TreeItemIcon.png"));
                collectionItem.SetIconModulate(0, TileCategories[i].Color);
            }
        }
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(Petrichor.LevelRect.Grow(3), Colors.White, false, 2);
    }

    private static void RedrawTerrain(object sender, EventArgs args)
    {
        Layers[2].QueueRedraw();
        Layers[1].QueueRedraw();
        Layers[0].QueueRedraw();
    }

    private void SetTileSearch(string searchTerm)
    {
        Array<TreeItem> categoryItems = GetNode<Tree>("%TilesTree").GetRoot().GetChildren();
        for (int i = 0; i < categoryItems.Count; i++)
        {
            Array<TreeItem> categoryTiles = categoryItems[i].GetChildren();

            if (categoryItems[i].GetText(0).Contains(searchTerm))
            {
                categoryItems[i].Visible = true;
                for (int j = 0; j < categoryTiles.Count; j++)
                {
                    categoryTiles[j].Visible = true;
                }
                continue;
            }
            
            int childrenVisible = 0;
            for (int j = 0; j < categoryTiles.Count; j++)
            {
                if (categoryTiles[j].GetText(0).Contains(searchTerm))
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
}