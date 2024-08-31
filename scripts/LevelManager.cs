using System.IO;
using Petrichor.scripts.Tiling;
using Petrichor.scripts.Geometry;
using Petrichor.scripts.Lingo;
using System.Diagnostics;
using Godot;

namespace Petrichor.scripts;

public static class LevelManager
{
    public static void ImportLevel(string path)
    {
        if (!File.Exists(path))
        {
            Debug.WriteLine(Utils.ERROR_STR + $"Level file not found at path {path}");
            return;
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string[] levelLines = File.ReadAllLines(path);

        LingoLinearList geo = LingoLinearList.FromString(levelLines[0]);
        LingoLinearList tiles = (LingoLinearList)LingoPropertyList.FromString(levelLines[1]).GetProperty("tlMatrix").Value;
        int width = geo.Length;
        int height = ((LingoLinearList)geo[0]).Length;
        NewLevel(width, height);

        for (int x = 0; x < width; x++)
        {
            LingoLinearList geoX = (LingoLinearList)geo[x];
            LingoLinearList tilesX = (LingoLinearList)tiles[x];
            for (int y = 0; y < height; y++)
            {
                LingoLinearList geoY = (LingoLinearList)geoX[y];
                LingoLinearList tilesY = (LingoLinearList)tilesX[y];
                for (int layer = 0; layer < 3; layer++)
                {
                    LingoLinearList cellList = (LingoLinearList)geoY[layer];
                    LingoPropertyList tileList = (LingoPropertyList)tilesY[layer];
                    
                    // Geo
                    Cell newCell = new Cell((int)cellList[0], 
                    ((LingoLinearList)cellList[1]).Length > 0 ? ((LingoLinearList)cellList[1]).ToArray<int>().StackablesToFlag() : 0);
                    GeometryEditor.Data[layer, x, y] = newCell;
                    
                    // Tiles
                    if ((string)tileList.GetProperty("tp").Value == "tileHead")
                    {
                        string tileName = (string)((LingoLinearList)tileList.GetProperty("data").Value)[1];
                        if (!TileEditor.Tiles.TryGetValue(tileName, out Tile tile))
                        {
                            Debug.WriteLine(Utils.WARNING_STR + $"Missing texture for {tileName}, make sure your tile directory contains this tile and that it is properly formatted.");
                            Texture2D missingTex = ResourceLoader.Load<Texture2D>("res://textures/MissingTexture.png",
                                "Texture2D", ResourceLoader.CacheMode.Ignore);
                            Tile missingTile = new (LingoPropertyList.FromString(
                                $"[#nm:\"{tileName}\", #sz:point(1,1), #specs:[1], #specs2:0, #tp:\"voxelStruct\", #repeatL:[1, 1, 1, 1], #bfTiles:0, #rnd:1, #ptPos:0, #tags:[]]"
                            ), Colors.Magenta);
                            TileEditor.Tiles.Add(tileName, missingTile);
                            TileEditor.TileTextures.Add(tileName, missingTex);
                            TileEditor.Layers[layer].AddMultiMeshType(missingTex, Colors.Magenta, Utils.Color8(0, 1, 0, 255), true);
                        }
                        else if (TileEditor.GetTileTexture(tileName) is { } tileTex && !TileEditor.Layers[layer].MultiMeshes.ContainsKey(tileTex))
                        {
                            TileEditor.Layers[layer].AddMultiMeshType(tileTex, tile.Color,
                                Utils.Color8(tile.Size.X, tile.Size.Y, tile.RepeatLines.Length, tile.BfTiles));
                        }
                        TileEditor.Data[layer, x, y] = tileName;
                    }
                }
            }
        }
        
        foreach(Layer layer in Petrichor.LayerInstances)
        {
            layer.ReloadMultimesh();
            layer.FullRedraw();
        }
        Debug.WriteLine(Utils.INFO_STR + $"Level {path.GetFile()} Loaded in {stopwatch.ElapsedMilliseconds}ms");
    }

    public static void NewLevel(int width, int height)
    {
        GeometryEditor.LevelSize = new Vector2I(width, height);
        GeometryEditor.Data = new Cell[3, width, height];
        TileEditor.Data = new string[3, width, height];
        Petrichor.Singleton.CenterCamera();
    }
}