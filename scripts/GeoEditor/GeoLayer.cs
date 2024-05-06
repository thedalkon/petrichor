using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Petrichor.scripts.TileEditor;

namespace Petrichor.scripts.GeoEditor;

public partial class GeoLayer : Layer
{
    public GeoLayer(int layer) : base(layer)
    {
        Material = ResourceLoader.Load<ShaderMaterial>("res://materials/geo_layer_mat.tres", null,
            ResourceLoader.CacheMode.Ignore);
        ZIndex = -layer - 1;
    }

    public override void LayerChanged()
    {
        float tintOffset = (Petrichor.CurrentLayer - LayerID + 2) * 0.25f;
        ((ShaderMaterial)Material).SetShaderParameter("tint_offset", tintOffset);
        ((ShaderMaterial)Material).SetShaderParameter("tint_lerp", Math.Abs(tintOffset - 0.5f));
        SelfModulate = SelfModulate with {A = -Math.Abs(tintOffset - 0.5f) + 1.0f};
    }

    public void SetCellMesh(Texture2D mmTex, int x, int y)
    {
        MultiMeshes[GeometryEditor.CellTextures[(CellTypes)GeometryEditor.Data[Petrichor.CurrentLayer, x, y].Type]]
        .Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, Utils.Tr2DZero);

        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, new Transform2D(0.0f, new Vector2(x,y) * 16.0f));
    }

    public void AddStackableMesh(Texture2D mmTex, int x, int y)
    {
        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, new Transform2D(0.0f, new Vector2(x,y) * 16.0f));
    }

    public void RemoveStackableMesh(Texture2D mmTex, int x, int y)
    {
        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, Utils.Tr2DZero);
    }
    
    // Only use if absolutely necessary
    public override void FullRedraw()
    {
        foreach (MultiMeshInstance2D mm in MultiMeshes.Values)
        {
            mm.Multimesh.VisibleInstanceCount = 0;
        }

        for (int x = 0; x < GeometryEditor.LevelSize.X; x++)
        {
            for (int y = 0; y < GeometryEditor.LevelSize.Y; y++)
            {
                // Geo Rendering
                Cell cell = GeometryEditor.Data[LayerID, x, y];
                CellTypes type = (CellTypes)cell.Type;
                MultiMesh mm = MultiMeshes[GeometryEditor.CellTextures[type]].Multimesh;
                int cellTypeIndex = mm.VisibleInstanceCount;

                mm.SetInstanceTransform2D(cellTypeIndex, new Transform2D(0.0f, new Vector2(x, y) * Petrichor.ZoomFactor));
                mm.SetInstanceColor(cellTypeIndex, Colors.White);
                mm.VisibleInstanceCount = ++cellTypeIndex;
                
                // Stackables Rendering
                int[] stackables = Utils.FlagsHelper.DecomposeFlag(cell.Stackables).ToArray();
                for (int i = 0; i < stackables.Length; i++)
                {
                    MultiMesh stackableMm = MultiMeshes[GeometryEditor.StackableTextures[(StackableTypes)stackables[i]]].Multimesh;
                    int stackableTypeCount = stackableMm.VisibleInstanceCount;

                    stackableMm.SetInstanceTransform2D(stackableTypeCount, new Transform2D(0.0f, new Vector2(x, y) * Petrichor.ZoomFactor));
                    stackableMm.SetInstanceColor(stackableTypeCount, Colors.White);
                    stackableMm.VisibleInstanceCount = ++stackableTypeCount;
                }
            }
        }
    }
}