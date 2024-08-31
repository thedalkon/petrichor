using System;
using Godot;
using Godot.Collections;

namespace Petrichor.scripts.Geometry;

public partial class GeoToolButton : TextureButton
{
	[Export]
	public int ToolNumber;

	private Dictionary<int, String> _toolNames = new()
	{
		{ 0, "Solid" },
		{ 1, "Air" },
		{ 2, "Slope" },
		{ 3, "Vertical Pole" },
		{ 4, "Horizontal Pole"},
		{ 5, "Rect Fill"},
		{ 6, "Rect Erase"},
		{ 7, "Erase All" },
		{ 8, "Shortcut Entrance"},
		{ 9, "Shortcut Path" },
		{ 10, "Room Entrance"},
		{ 11, "Creature Den" },
		{ 12, "Place Spear"},
		{ 13, "Place Rock" },
		{ 14, "Cracked Terrain" },
		{ 15, "Forbid Batfly Chain" },
		{ 16, "Batfly Hive" },
		{ 17, "Garbage Worm Hole" },
		{ 18, "Waterfall" },
		{ 19, "Creature Shortcut" },
		{ 20, "Wormgrass" },
		{ 21, "Scavenger Den" },
		{ 22, "Platform" },
		{ 23, "Selection Rect" },
	};
	
	public override void _EnterTree()
	{
		Pressed += PressedButton;
		GeometryEditor.ChangedTool += ToolChanged;
	}

	private void PressedButton()
	{
		if (Input.IsActionJustReleased("mouse_left"))
		{
			if (GeometryEditor.CurrentSecondaryTool == ToolNumber) return;
			GeometryEditor.CurrentTool = ToolNumber;
			GetNode<Label>("%ToolLabel").Text = "Primary Tool: " + _toolNames[ToolNumber];
		}
		else if (Input.IsActionJustReleased("mouse_right"))
		{
			if (GeometryEditor.CurrentTool == ToolNumber) return;
			GeometryEditor.CurrentSecondaryTool = ToolNumber;
			GetNode<Label>("%SecondaryToolLabel").Text = "Secondary Tool: " + _toolNames[ToolNumber];
		}
		
		GeometryEditor.ToolChanged();
	}

	private void ToolChanged(object sender, EventArgs args)
	{
		if (GeometryEditor.CurrentTool == ToolNumber) SelfModulate = Colors.Red;
		else if (GeometryEditor.CurrentSecondaryTool == ToolNumber) SelfModulate = Colors.Blue;
		else SelfModulate = Colors.Black;
	}
}