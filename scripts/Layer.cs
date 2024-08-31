using System;
using System.Collections.Generic;
using Godot;
using Petrichor.scripts.Geometry;

namespace Petrichor.scripts;

public abstract partial class Layer : Node2D
{
    protected readonly int LayerID;
    public readonly Dictionary<Texture2D, MultiMeshInstance2D> MultiMeshes = new();

    protected Layer(int layer)
    {
        LayerID = layer;
        TextureFilter = TextureFilterEnum.Nearest;
        Petrichor.LayerInstances.Add(this);
        ZIndex = -10;
    }

    protected Layer()
    {
    }

    public override void _Ready()
    {
        LayerChanged();
    }

    public MultiMeshInstance2D AddMultiMeshType(Texture2D texture, Color? modulate = null, Color? data = null, bool missing = false)
    {
        Vector2 texSize = texture.GetSize();
        MultiMeshInstance2D mm = new MultiMeshInstance2D
        {
            Texture = texture,
            UseParentMaterial = !missing,
            Multimesh = new MultiMesh()
            {
                TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
                Mesh = new QuadMesh() { Size = texSize * new Vector2(1, -1), CenterOffset = new Vector3(texSize.X, texSize.Y, 0) * 0.5f},
                UseCustomData = data.HasValue,
                CustomAabb = new Aabb(Vector3.Zero, new Vector3(GeometryEditor.LevelSize.X, GeometryEditor.LevelSize.Y, 0) * 16.0f),
                InstanceCount = GeometryEditor.LevelSize.Size(),
            },
            Modulate = modulate ?? Colors.White,
            Visible = false
        };
        mm.SetMeta("visible_instances", 0);

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

    protected void CheckVisibility(MultiMeshInstance2D mm)
    {
        mm.Visible = !((int)mm.GetMeta("visible_instances") <= 0);
    }
    
    protected void ChangeInstances(MultiMeshInstance2D mm, int change)
    {
        mm.SetMeta("visible_instances", (int)mm.GetMeta("visible_instances") + change);
    }

    public abstract void LayerChanged();
    public abstract void ReloadMultimesh();
    public abstract void FullRedraw();
}