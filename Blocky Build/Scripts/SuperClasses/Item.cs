using Godot;
using System;

public partial class Item : Node3D {
    [Export]
    public int MaxCount = 100;
    [Export]
    public string[] Tags;

    public int Count = 1;
    private string itemName = "";
    public string ItemName {
        get {
            if (itemName != "")
                return itemName;
            else
                return Name;
        }
        set {
            itemName = value;
        }
    }
}