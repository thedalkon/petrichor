using System;
using Godot;

namespace Petrichor.scripts.Geometry;

public partial class GeoLayer : Layer
{
    public GeoLayer(int layer) : base(layer)
    {
        Material = ResourceLoader.Load<ShaderMaterial>("res://materials/geo_layer_mat.tres", null,
            ResourceLoader.CacheMode.Ignore);
        ZIndex -= layer;
    }

    public GeoLayer()
    {
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
        MultiMeshInstance2D lastMm =
            MultiMeshes[GeometryEditor.CellTextures[(CellTypes)GeometryEditor.Data[Petrichor.CurrentLayer, x, y].Type]];
        lastMm.Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, Utils.Tr2DZero);
        ChangeInstances(lastMm, -1);

        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, new Transform2D(0.0f, new Vector2(x,y) * 16.0f));
        ChangeInstances(MultiMeshes[mmTex], 1);
        CheckVisibility(MultiMeshes[mmTex]);
    }

    public void AddStackableMesh(Texture2D mmTex, int x, int y)
    {
        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, new Transform2D(0.0f, new Vector2(x,y) * 16.0f));
        
        ChangeInstances(MultiMeshes[mmTex], 1);
        CheckVisibility(MultiMeshes[mmTex]);
    }

    public void RemoveStackableMesh(Texture2D mmTex, int x, int y)
    {
        MultiMeshes[mmTex].Multimesh.SetInstanceTransform2D(x * GeometryEditor.LevelSize.Y + y, Utils.Tr2DZero);
        
        ChangeInstances(MultiMeshes[mmTex], -1);
        CheckVisibility(MultiMeshes[mmTex]);
    }

    public override void ReloadMultimesh()
    {
        foreach (MultiMeshInstance2D mm in MultiMeshes.Values)
        {
            mm.QueueFree();
        }
        MultiMeshes.Clear();

        foreach (Texture2D i in GeometryEditor.CellTextures.Values)
        {
            AddMultiMeshType(i);
        }
        foreach (Texture2D i in GeometryEditor.StackableTextures.Values)
        {
            AddMultiMeshType(i);
        }
        
        foreach (MultiMeshInstance2D mm in MultiMeshes.Values)
        {
            CheckVisibility(mm);
        }
    }
    
    // Only use if absolutely necessary
    public override void FullRedraw()
    {
        foreach (MultiMeshInstance2D mm in MultiMeshes.Values)
        {
            for (int i = 0; i < mm.Multimesh.InstanceCount; i++)
            {
                mm.Multimesh.SetInstanceTransform2D(i, Utils.Tr2DZero);
            }
        }

        for (int x = 0; x < GeometryEditor.LevelSize.X; x++)
        {
            for (int y = 0; y < GeometryEditor.LevelSize.Y; y++)
            {
                // Geo Rendering
                Cell cell = GeometryEditor.Data[LayerID, x, y];
                CellTypes type = (CellTypes)cell.Type;
                MultiMesh mm = MultiMeshes[GeometryEditor.CellTextures[type]].Multimesh;
                int cellTypeIndex = x * GeometryEditor.LevelSize.Y + y;

                mm.SetInstanceTransform2D(cellTypeIndex, new Transform2D(0.0f, new Vector2(x, y) * Petrichor.ZoomFactor));
                
                ChangeInstances(MultiMeshes[GeometryEditor.CellTextures[type]], 1);
                MultiMeshes[GeometryEditor.CellTextures[type]].Show();
                
                // Stackables Rendering
                int[] stackables = Utils.FlagsHelper.DecomposeFlag(cell.Stackables).ToArray();
                for (int i = 0; i < stackables.Length; i++)
                {
                    MultiMesh stackableMm = MultiMeshes[GeometryEditor.StackableTextures[(StackableTypes)stackables[i]]].Multimesh;
                    int stackableTypeCount = x * GeometryEditor.LevelSize.Y + y;

                    stackableMm.SetInstanceTransform2D(stackableTypeCount, new Transform2D(0.0f, new Vector2(x, y) * Petrichor.ZoomFactor));
                    ChangeInstances(MultiMeshes[GeometryEditor.StackableTextures[(StackableTypes)stackables[i]]], 1);
                    MultiMeshes[GeometryEditor.StackableTextures[(StackableTypes)stackables[i]]].Show();
                }
            }
        }
    }
}