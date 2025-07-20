using Godot;
using System;
using System.Threading;
using System.Collections.Concurrent;

public class Chunk {
    private Thread _thread;
    public Thread Thread { 
        get { 
            return _thread; 
        } 
    }

    private ConcurrentQueue<Block> _queue = new();
    public ConcurrentQueue<Block> Blocks { 
        get { 
            return _queue; 
        } 
    }

    public Chunk(Vector3I chunkPosition, WorldData.WorldType worldType, WorldData.WorldLayer[][] worldLayers) {
        _thread = new Thread(() => GenWorld(chunkPosition, worldType, worldLayers));
        _thread.Start();
    }

    public void GenWorld(Vector3I atChunk, WorldData.WorldType worldType, WorldData.WorldLayer[][] worldLayers) {
        switch (worldType) {
            case WorldData.WorldType.Flat:
                int atLayer = GameSettings.DefaultBedrockLevel;

                Vector3I offset = atChunk * GameSettings.ChunkRadius;

                foreach (WorldData.WorldLayer worldLayer in worldLayers[(int)WorldData.WorldType.Flat]) {
                    for (int y = atLayer; y < atLayer + worldLayer.height; y++) {
                        for (int x = -GameSettings.ChunkRadius + offset.X; x <= GameSettings.ChunkRadius + offset.X; x++) {
                            for (int z = -GameSettings.ChunkRadius + offset.Z; z <= GameSettings.ChunkRadius + offset.Z; z++) {
                                Block newBlock = Register.Blocks[worldLayer.blockName]?.Instantiate<Block>();
                                newBlock.Translate(new Vector3I(x, y, z));
                                _queue.Enqueue(newBlock);
                            }
                        }
                    }

                    atLayer += worldLayer.height;
                }
                break;
        }
    }
}