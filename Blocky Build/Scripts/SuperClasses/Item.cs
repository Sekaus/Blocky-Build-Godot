using Godot;
using System;

public partial class Item : Node {
    public Node Scene;
    public int MaxCount;
    public int Count;

    public Item(Node scene, int maxCount = 99, int count = 1) {
        this.Scene = scene;
        this.MaxCount = maxCount;
        this.Count = count;
    }
}