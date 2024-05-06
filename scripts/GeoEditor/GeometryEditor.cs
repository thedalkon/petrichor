using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;
using Petrichor.scripts.GeoEditor.GeoTools;

namespace Petrichor.scripts.GeoEditor;

public partial class GeometryEditor : Editor
{
	public static Vector2I LevelSize = new(48, 35);
	public static int CurrentTool = 0;
	public static int CurrentSecondaryTool = 1;
	private static float ZoomFactor => Petrichor.ZoomFactor; 

	public static Cell[,,] Data;
	
	public static event EventHandler ChangedTool;

	public override string Controls => 
		"Left click: Use / set primary tool\n" +
		"Right click: Use / set secondary tool\n" +
		"W: Increase layer\n" +
		"S: Decrease layer\n" +
		"R: Center Camera\n" +
		"Ctrl + V: Paste selection";
	
	public static readonly Dictionary<CellTypes, Texture2D> CellTextures = new()
	{
	    {CellTypes.Air, (Texture2D)ResourceLoader.Load("res://textures/Air.png")},
	    {CellTypes.Solid, (Texture2D)ResourceLoader.Load("res://textures/Solid.png")},
	    {CellTypes.Platform, (Texture2D)ResourceLoader.Load("res://textures/Platform.png")},
	    {CellTypes.SlopeES, (Texture2D)ResourceLoader.Load("res://textures/SlopeES.png")},
	    {CellTypes.SlopeNE, (Texture2D)ResourceLoader.Load("res://textures/SlopeNE.png")},
	    {CellTypes.SlopeNW, (Texture2D)ResourceLoader.Load("res://textures/SlopeNW.png")},
	    {CellTypes.SlopeSW, (Texture2D)ResourceLoader.Load("res://textures/SlopeSW.png")},
	};

	public static readonly Dictionary<StackableTypes, Texture2D> StackableTextures = new()
	{
		{ StackableTypes.HPole , (Texture2D)ResourceLoader.Load("res://textures/HPole.png")},
		{ StackableTypes.VPole , (Texture2D)ResourceLoader.Load("res://textures/VPole.png")},
		{ StackableTypes.BatHive , (Texture2D)ResourceLoader.Load("res://textures/BatHive.png")},
		{ StackableTypes.ShortcutEnt , (Texture2D)ResourceLoader.Load("res://textures/ShortcutEntrance.png")},
		{ StackableTypes.ShortcutPath , (Texture2D)ResourceLoader.Load("res://textures/ShortcutPath.png")},
		{ StackableTypes.RoomEnt , (Texture2D)ResourceLoader.Load("res://textures/RoomEntrance.png")},
		{ StackableTypes.LizardDen , (Texture2D)ResourceLoader.Load("res://textures/LizardDen.png")},
		{ StackableTypes.Spear , (Texture2D)ResourceLoader.Load("res://textures/Spear.png")},
		{ StackableTypes.Rock , (Texture2D)ResourceLoader.Load("res://textures/Rock.png")},
		{ StackableTypes.CrackedTerr , (Texture2D)ResourceLoader.Load("res://textures/CrackedTerrain.png")},
		{ StackableTypes.ForbidBat , (Texture2D)ResourceLoader.Load("res://textures/ForbidBat.png")},
		{ StackableTypes.GarbageHole , (Texture2D)ResourceLoader.Load("res://textures/GarbageHole.png")},
		{ StackableTypes.Waterfall , (Texture2D)ResourceLoader.Load("res://textures/Waterfall.png")},
		{ StackableTypes.CreatureShortcut , (Texture2D)ResourceLoader.Load("res://textures/CreatureShortcut.png")},
		{ StackableTypes.Wormgrass , (Texture2D)ResourceLoader.Load("res://textures/Wormgrass.png")},
		{ StackableTypes.ScavHole , (Texture2D)ResourceLoader.Load("res://textures/ScavengerDen.png")},
	};

	private static readonly GeoTool[] Tools = {
		new SetCellGeoTool(CellTypes.Solid),
		new SetCellGeoTool(CellTypes.Air),
		new SlopeGeoTool(),
		new SetStackableGeoTool(StackableTypes.VPole),
		new SetStackableGeoTool(StackableTypes.HPole),
		new SetRectGeoTool(CellTypes.Solid),
		new SetRectGeoTool(CellTypes.Air),
		new EraseAllGeoTool(),
		new SetStackableGeoTool(StackableTypes.ShortcutEnt),
		new SetStackableGeoTool(StackableTypes.ShortcutPath),
		new SetStackableGeoTool(StackableTypes.RoomEnt),
		new SetStackableGeoTool(StackableTypes.LizardDen),
		new SetStackableGeoTool(StackableTypes.Spear),
		new SetStackableGeoTool(StackableTypes.Rock),
		new SetStackableGeoTool(StackableTypes.CrackedTerr),
		new SetStackableGeoTool(StackableTypes.ForbidBat),
		new SetStackableGeoTool(StackableTypes.BatHive),
		new SetStackableGeoTool(StackableTypes.GarbageHole),
		new SetStackableGeoTool(StackableTypes.Waterfall),
		new SetStackableGeoTool(StackableTypes.CreatureShortcut),
		new SetStackableGeoTool(StackableTypes.Wormgrass),
		new SetStackableGeoTool(StackableTypes.ScavHole),
		new SetCellGeoTool(CellTypes.Platform),
		new SelectRectGeoTool(),
	};

	public static readonly GeoLayer[] Layers =
	{
		new(0),
		new(1),
		new(2)
	};

	public static readonly Tooltip GeoTooltip = new();
	
	public override void _Ready()
	{
		Data = new Cell[3, LevelSize.X, LevelSize.Y];
		ToolChanged();
		foreach (var layer in Layers)
		{
			AddChild(layer);
			foreach (Texture2D i in CellTextures.Values)
			{
				layer.AddMultiMeshType(i);
			}
			foreach (Texture2D i in StackableTextures.Values)
			{
				layer.AddMultiMeshType(i);
			}
		}
		AddChild(GeoTooltip);
	}

	public override void _Process(double delta)
	{
		QueueRedraw();
		
		// Tool usage
		if (Input.IsActionPressed("mouse_left") && !DraggableControl.IsDragging)
		{
			if (!IsInLevelRect(out var cellPos)) return;
			Tools[CurrentTool].ApplyTool(cellPos, Petrichor.CurrentLayer);
		} 
		else if (Input.IsActionPressed("mouse_right") && !DraggableControl.IsDragging)
		{
			if (!IsInLevelRect(out var cellPos)) return;
			Tools[CurrentSecondaryTool].ApplyTool(cellPos, Petrichor.CurrentLayer);
		}
		if (Input.IsActionJustReleased("mouse_left") && !DraggableControl.IsDragging)
		{
			if (!IsInLevelRect(out var cellPos)) return;
			Tools[CurrentTool].EndApplyTool(cellPos, Petrichor.CurrentLayer);
		} 
		else if (Input.IsActionJustReleased("mouse_right") && !DraggableControl.IsDragging)
		{
			if (!IsInLevelRect(out var cellPos)) return;
			Tools[CurrentSecondaryTool].EndApplyTool(cellPos, Petrichor.CurrentLayer);
		}

		if (Input.IsActionJustPressed("paste_clipboard"))
		{
			PasteClipboard();
		}
	}

	public override void _Draw()
	{
		DrawRect(Petrichor.LevelRect, Colors.White, false, 2);
		
		// Cursor tooltips
		if (IsInLevelRect())
		{
			GeoTooltip.QueueRedraw();
		}
		
		GeoTooltip.Visible = IsInLevelRect();
	}

	public static void SetCell(int layer, int x, int y, CellTypes cellType)
	{
		Data[layer, x, y].Type = (int)cellType;
	}
	
	public static void SetCell(int layer, Vector2I pos, CellTypes cellType)
	{
		Data[layer, pos.X, pos.Y].Type = (int)cellType;
	}

	public static Cell GetCell(int layer, int x, int y)
	{
		return Data[layer, x, y];
	}
	
	public static Cell GetCell(int layer, Vector2I pos)
	{
		return Data[layer, pos.X, pos.Y];
	}
	
	public static void AddStackable(int layer, int x, int y, StackableTypes stackableType)
	{
		Data[layer, x, y].Stackables |= (int)stackableType;
	}

	private static void SetStackable(int layer, int x, int y, int stackables)
	{
		Data[layer, x, y].Stackables = stackables;
	}
	
	public static bool HasStackable(int layer, int x, int y, StackableTypes stackableType)
	{
		return (Data[layer, x, y].Stackables & (int)stackableType) == (int)stackableType;
	}
	
	public static void RemoveStackable(int layer, int x, int y, StackableTypes stackableType)
	{
		Data[layer, x, y].Stackables &= ~(int)stackableType;
	}

	private static void PasteClipboard()
	{
		string clipboard = DisplayServer.ClipboardGet();
		if (!clipboard.StartsWith("PTCHR")) return;
		
		Vector2I mousePos = (Vector2I)(Petrichor.RelativeMousePos / ZoomFactor).Floor();
		if (!IsInLevelRect(mousePos)) return;
		
		string[] entries = clipboard.Split(';', StringSplitOptions.RemoveEmptyEntries);
		if (entries.Length <= 3) return;
		Vector2I copySize = new Vector2I(entries[1].ToInt(), entries[2].ToInt());
		int currentEntry = 3;
		
		for (int x = 0; x < copySize.X; x++)
		{
			for (int y = 0; y < copySize.Y; y++)
			{
				if (currentEntry >= entries.Length) return;
				Vector2I cellPos = mousePos + new Vector2I(x, y);
				if (!IsInLevelRect(cellPos))
				{
					currentEntry++;
					continue;
				}
				
				string[] splitCell = entries[currentEntry].Split(',');
				int cellType = splitCell[0].ToInt();
				int stackables = splitCell[1].ToInt();

				Data[Petrichor.CurrentLayer, cellPos.X, cellPos.Y].Type = cellType;
				Data[Petrichor.CurrentLayer, cellPos.X, cellPos.Y].Stackables = stackables;
				currentEntry++;
			}
		}

		Layers[Petrichor.CurrentLayer].FullRedraw();
	}

	public static bool IsInLevelRect(out Vector2I cellPos)
	{
		cellPos = (Vector2I)(Petrichor.RelativeMousePos / ZoomFactor).Floor();
		return !(cellPos.X < 0 || cellPos.X >= LevelSize.X || cellPos.Y < 0 || cellPos.Y >= LevelSize.Y);
	}
	
	public static bool IsInLevelRect()
	{
		Vector2I cellPos = (Vector2I)(Petrichor.RelativeMousePos / ZoomFactor).Floor();
		return !(cellPos.X < 0 || cellPos.X >= LevelSize.X || cellPos.Y < 0 || cellPos.Y >= LevelSize.Y);
	}
	
	public static bool IsInLevelRect(Vector2I cellPos)
	{
		return !(cellPos.X < 0 || cellPos.X >= LevelSize.X || cellPos.Y < 0 || cellPos.Y >= LevelSize.Y);
	}

	public static void ToolChanged()
	{
		if (ChangedTool != null) 
			ChangedTool.Invoke(null, EventArgs.Empty);
		GeoTooltip.OverrideTooltip = false;
	}
}

public struct Cell
{
	public bool Equals(Cell other)
	{
		return Type == other.Type && Stackables == other.Stackables;
	}

	public override bool Equals(object obj)
	{
		return obj is Cell other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type, Stackables);
	}

	public int Type;
	public int Stackables;

	public Cell(int type = 0, int stackables = 0)
	{
		Type = type;
		Stackables = stackables;
	}
	
	public new string ToString()
	{
		string ret = Type + "," + Stackables;
		return ret;
	}
	
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public string ToStringRW()
	{
		string ret = Type + ",[";
		int[] flags = Utils.FlagsHelper.DecomposeFlag(Stackables).ToArray();
		for (int i = 0; i < flags.Length; i++)
		{
			ret += flags[i];
			if (i != flags.Length - 1)
				ret += ",";
		}
		ret += "]";
		return ret;
	}

	public static bool operator ==(Cell a, Cell b) => a.Type == b.Type && a.Stackables == b.Stackables;
	public static bool operator !=(Cell a, Cell b) => a.Type != b.Type || a.Stackables != b.Stackables;
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum CellTypes
{
	Air = 0,
	Solid = 1,
	SlopeNE = 2,
	SlopeNW = 3,
	SlopeES = 4,
	SlopeSW = 5,
	Platform = 6,
	ShortcutEnt = 7, //TODO: Add shortcut entrance tool
	Glass = 9
}

[Flags]
public enum StackableTypes
{
	HPole = 1<<0,
	VPole = 1<<1,
	BatHive = 1<<2,
	ShortcutEnt = 1<<3,
	ShortcutPath = 1<<4,
	RoomEnt = 1<<5,
	LizardDen = 1<<6,
	Rock = 1<<7,
	Spear = 1<<8,
	CrackedTerr = 1<<9,
	ForbidBat = 1<<10,
	GarbageHole = 1<<11,
	Waterfall = 1<<12,
	CreatureShortcut = 1<<13,
	Wormgrass = 1<<14,
	ScavHole = 1<<15
}