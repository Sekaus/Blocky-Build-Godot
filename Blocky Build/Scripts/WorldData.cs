using Godot;
using System;
public partial class WorldData: Node {
    public enum WorldType {
        flat
    }

    public WorldType worldType = WorldType.flat;
}