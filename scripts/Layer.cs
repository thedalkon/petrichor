using System;
using System.Collections.Generic;
using Godot;
using Petrichor.scripts.GeoEditor;

namespace Petrichor.scripts;

public abstract partial class Layer : Node2D
{
    public readonly int LayerID;
    public readonly Dictionary<Texture2D, MultiMeshInstance2D> MultiMeshes = new();

    public Layer(int layer)
    {
        LayerID = layer;
        TextureFilter = TextureFilterEnum.Nearest;
        Petrichor.LayerInstances.Add(this);
    }

    public override void _Ready()
    {
        LayerChanged();
    }

    public MultiMeshInstance2D AddMultiMeshType(Texture2D texture, Color? modulate = null, Color? data = null)
    {
        Vector2 texSize = texture.GetSize();
        MultiMeshInstance2D mm = new MultiMeshInstance2D
        {
            Texture = texture,
            UseParentMaterial = true,
            Multimesh = new MultiMesh()
            {
                TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
                Mesh = new QuadMesh() { Size = texSize * new Vector2(1, -1), CenterOffset = new Vector3(texSize.X, texSize.Y, 0) * 0.5f },
                UseCustomData = data.HasValue,
                InstanceCount = GeometryEditor.LevelSize.Size(),
            },
            Modulate = modulate.HasValue ? modulate.Value : Colors.White
        };

        if (data.HasValue)
        {
            for (int i = 0; i < mm.Multimesh.InstanceCount; i++)
            {
                mm.Multimesh.SetInstanceCustomData(i, data.Value);
            }
        }

        AddChild(mm);
        MultiMeshes.Add(texture, mm);
        return mm;
    }

    public abstract void LayerChanged();

    public abstract void FullRedraw();
}