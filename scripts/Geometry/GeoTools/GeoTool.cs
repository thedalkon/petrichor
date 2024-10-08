﻿using Godot;

namespace Petrichor.scripts.Geometry.GeoTools;

public abstract class GeoTool
{
    public abstract void ApplyTool(Vector2I cellPos, int layer);

    public virtual void EndApplyTool(Vector2I cellPos, int layer) {}
}