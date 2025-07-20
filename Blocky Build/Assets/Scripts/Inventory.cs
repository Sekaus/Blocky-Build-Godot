using Godot;
using System;

public partial class Inventory : Node {
    public Item[] Slots;

    public int Width, Height;

    public Inventory(int Width, int Height = 1) {
        this.Slots = new Item[Width * Height];
        this.Width = Width;
        this.Height = Height;
    }
}