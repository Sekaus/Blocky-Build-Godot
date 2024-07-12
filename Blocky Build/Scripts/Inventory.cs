using Godot;
using System;

public partial class Inventory : Node {
    public int SlotCount;
    public Item[] Slots;
    public Inventory(int slotCount) {
        this.SlotCount = slotCount;
        this.Slots = new Item[slotCount];
    }
}