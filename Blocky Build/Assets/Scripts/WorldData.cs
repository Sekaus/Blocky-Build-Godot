using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
public partial class WorldData : Node {
    Dictionary<Vector3I, Chunk> chunks = new Dictionary<Vector3I, Chunk>();
    public int ChunkCount {
        get {
            return chunks.Count;
        }
    }

    WorldType worldType;

    public void addBlock() {

    }

    public enum WorldType {
        Flat
    }

    public struct WorldLayer {
        public string blockName;
        public int height;
    }

    public WorldLayer[][] worldTypeLayers = new WorldLayer[4][];

    public WorldType Type = WorldType.Flat;

    Register register;
    public override void _Ready() {
        register = GetParent().GetNode<Register>("%Register");
        
        worldTypeLayers[(int)WorldType.Flat] = new[] {
            new WorldLayer() { blockName = "Bedrock", height = 1 },
            new WorldLayer() { blockName = "Stone", height = 4 },
            new WorldLayer() { blockName = "Dirt", height = 1 },
            new WorldLayer() { blockName = "GrassBlock", height = 1 },
        };
    }

    public void GenChunk(Vector3I chunkPosition) {
        // Genarate world
        Chunk chunk = new Chunk(chunkPosition, worldType, worldTypeLayers);
        chunk.Thread.Join();
        chunks.Add(chunkPosition, chunk);
    }

    public void LoadChunk(Vector3I chunkPosition) {
        // Load genarate blocks
        while (chunks[chunkPosition].Blocks.TryDequeue(out var block)) {
            // Safe scene‑tree addition on main thread
            CallDeferred("add_child", block);
        }
    }
}