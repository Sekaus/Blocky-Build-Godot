using Godot;
using System;
public partial class WorldData : Node {
    public enum WorldType {
        Flat
    }

    public struct WorldLayer {
        public string blockName;
        public int height;
    }

    public WorldLayer[][] WorldTypeLayers = new WorldLayer[4][];

    public WorldType Type = WorldType.Flat;

    Register register;
    public override void _Ready() {
        register = GetParent().GetNode<Register>("%Register");
        
        WorldTypeLayers[(int)WorldType.Flat] = new[] {
            new WorldLayer() { blockName = "Bedrock", height = 1 },
            new WorldLayer() { blockName = "Stone", height = 4 },
            new WorldLayer() { blockName = "Dirt", height = 1 },
            new WorldLayer() { blockName = "GrassBlock", height = 1 },
        };
    }
}