using Godot;
using System;
public partial class Game : Node {
    [Export]
    public Node map;
    [Export]
    public WorldData worldData;
    
    // Set block in world at xyz
    public void SetBlock(string blockName, int x, int y, int z, bool runBlockUpdates = true) {
        if (!IsThereABlockAt(x, y, z)) {
            Block newBlock = Register.Blocks[blockName].Instantiate<Block>();

            newBlock.BlockName = blockName;

            if (runBlockUpdates)
                newBlock = UpdateBlock(x, y, z, newBlock);

            newBlock.Position = new Vector3I(x, y, z);

            newBlock.Name = "block_" + x + "_" + y + "_" + z;

            map.AddChild(newBlock);


            if (runBlockUpdates) {
                if (newBlock.Type == Block.BlockType.Fence) {
                    Godot.Collections.Dictionary<string, Block> blocksToUpdate = CollectConnnectedBlocks(x, y, z);

                    UpdateBlocks(blocksToUpdate);
                }
            }
        }
    }

    // Replace block in world at xyz
    public void ReplaceBlock(int x, int y, int z, string blockName, bool runBlockUpdates = true) {
        if (IsThereABlockAt(x, y, z)) {
            RemoveBlock(x, y, z);
            SetBlock(blockName, x, y, z, runBlockUpdates);
        }
    }

    // Remove block in world at xyz
    public void RemoveBlock(int x, int y, int z) {
        if (IsThereABlockAt(x, y, z)) {
            string blockName = "block_" + x + "_" + y + "_" + z;
            Block block = getBlockAsObject(x, y, z);

            map.RemoveChild(block);
            block.QueueFree();

            Godot.Collections.Dictionary<string, Block> blocksToUpdate = CollectConnnectedBlocks(x, y, z);

            UpdateBlocks(blocksToUpdate);
        }
    }

    // Run at block single update
    public Block UpdateBlock(int atX, int atY, int atZ, Block blockThatExecute) {
        if (blockThatExecute.Type == Block.BlockType.Fence)
            blockThatExecute = UpdateConnectedBlock(atX, atY, atZ, blockThatExecute);

        return blockThatExecute;
    }

    // Collect all connected Blocks
    public Godot.Collections.Dictionary<string, Block> CollectConnnectedBlocks(int atX, int atY, int atZ) {
        Godot.Collections.Dictionary<string, Block> connectedBlocks = new Godot.Collections.Dictionary<string, Block>();

        Block nextToBlockRight = getBlockAsObject(atX + 1, atY, atZ);
        Block nextToBlockLeft = getBlockAsObject(atX - 1, atY, atZ);
        Block nextToBlockForward = getBlockAsObject(atX, atY, atZ + 1);
        Block nextToBlockBackward = getBlockAsObject(atX, atY, atZ - 1);

        connectedBlocks.Add("Right", nextToBlockRight);
        connectedBlocks.Add("Left", nextToBlockLeft);
        connectedBlocks.Add("Forward", nextToBlockForward);
        connectedBlocks.Add("Backward", nextToBlockBackward);

        return connectedBlocks;
    }

    // Run at multiple block updates
    public void UpdateBlocks(Godot.Collections.Dictionary<string, Block> blocksToUpdate) {
        if (blocksToUpdate != null || blocksToUpdate.Count > 0) {
            foreach (var block in blocksToUpdate) {
                if (block.Value.Type == Block.BlockType.Fence) {
                    Block updatedBlock = Register.Blocks[block.Value.BlockName].Instantiate<Block>();

                    updatedBlock.BlockName = block.Value.BlockName;

                    updatedBlock = UpdateBlock((int)block.Value.Position.X, (int)block.Value.Position.Y, (int)block.Value.Position.Z, updatedBlock);

                    updatedBlock.Position = new Vector3I((int)block.Value.Position.X, (int)block.Value.Position.Y, (int)block.Value.Position.Z);

                    map.RemoveChild(block.Value);
                    block.Value.QueueFree();

                    updatedBlock.Name = "block_" + updatedBlock.Position.X + "_" + updatedBlock.Position.Y + "_" + updatedBlock.Position.Z;

                    map.AddChild(updatedBlock);
                }
            }
        }
    }

    // Run at single connected block update
    public Block UpdateConnectedBlock(int atX, int atY, int atZ, Block blockThatExecute) {
        Godot.Collections.Dictionary<string, Block> blocksToUpdate = CollectConnnectedBlocks(atX, atY, atZ);

        bool connectionToBlockRight = false;
        bool connectionToBlockLeft = false;
        bool connectionToBlockForward = false;
        bool connectionToBlockBackward = false;

        if (blocksToUpdate["Right"].BlockName != "")
            connectionToBlockRight = true;
        if (blocksToUpdate["Left"].BlockName != "")
            connectionToBlockLeft = true;
        if (blocksToUpdate["Forward"].BlockName != "")
            connectionToBlockForward = true;
        if (blocksToUpdate["Backward"].BlockName != "")
            connectionToBlockBackward = true;

        string newBlock = blockThatExecute.BlockName;

        if (connectionToBlockRight && connectionToBlockLeft && connectionToBlockForward && connectionToBlockBackward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedC"].Instantiate<Block>();
        }
        else if (connectionToBlockRight && connectionToBlockLeft && connectionToBlockForward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedB"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(-90));
            blockThatExecute.FacingDirection = Block.FacingDirections.Forward;
        }
        else if (connectionToBlockRight && connectionToBlockLeft && connectionToBlockBackward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedB"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(90));
            blockThatExecute.FacingDirection = Block.FacingDirections.Backward;
        }
        else if (connectionToBlockRight && connectionToBlockForward && connectionToBlockBackward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedB"].Instantiate<Block>();
            blockThatExecute.FacingDirection = Block.FacingDirections.Right;
        }
        else if (connectionToBlockLeft && connectionToBlockForward && connectionToBlockBackward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedB"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(180));
            blockThatExecute.FacingDirection = Block.FacingDirections.Left;
        }
        else if (connectionToBlockForward && connectionToBlockBackward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedMeddel"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(90));
        }
        else if (connectionToBlockRight && connectionToBlockLeft) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedMeddel"].Instantiate<Block>();
        }
        else if (connectionToBlockLeft && connectionToBlockForward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedD"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(-90));
            blockThatExecute.FacingDirection = Block.FacingDirections.Left;
        }
        else if (connectionToBlockRight && connectionToBlockForward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedD"].Instantiate<Block>();
            blockThatExecute.FacingDirection = Block.FacingDirections.Forward;
        }
        else if (connectionToBlockLeft && connectionToBlockBackward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedD"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(180));
            blockThatExecute.FacingDirection = Block.FacingDirections.Backward;
        }
        else if (connectionToBlockRight && connectionToBlockBackward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedD"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(90));
            blockThatExecute.FacingDirection = Block.FacingDirections.Right;
        }
        else if (connectionToBlockForward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedA"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(-90));
            blockThatExecute.FacingDirection = Block.FacingDirections.Forward;
        }
        else if (connectionToBlockBackward) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedA"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(90));
            blockThatExecute.FacingDirection = Block.FacingDirections.Backward;
        }
        else if (connectionToBlockRight) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedA"].Instantiate<Block>();
            blockThatExecute.FacingDirection = Block.FacingDirections.Right;
        }
        else if (connectionToBlockLeft) {
            blockThatExecute = Register.Blocks["StoneFence"]["ConnectedA"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(180));
            blockThatExecute.FacingDirection = Block.FacingDirections.Left;
        }

        blockThatExecute.BlockName = newBlock;

        return blockThatExecute;
    }

    // Test if there is a block at xyz
    public bool IsThereABlockAt(int x, int y, int z) {
        string blockName = "block_" + x + "_" + y + "_" + z;
        return map.GetNodeOrNull(blockName) is not null;
    }

    // Get block as object at xyz
    public Block getBlockAsObject(int x, int y, int z) {
        string blockName = "block_" + x + "_" + y + "_" + z;
        if(IsThereABlockAt(x, y, z))
            return map.GetNode<Block>(blockName);
        else
            return new Block();
    }

    private int chunkRadius = 16, bedrockLevel = 0;
    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        // Genarate world
        switch (worldData.Type) {
            case WorldData.WorldType.Flat:
                foreach (WorldData.WorldLayer worldLayer in worldData.WorldTypeLayers[(int)WorldData.WorldType.Flat]) {
                    for (int y = bedrockLevel; y < bedrockLevel + worldLayer.height; y++) {
                        for (int x = -chunkRadius; x <= chunkRadius; x++) {
                            for (int z = -chunkRadius; z <= chunkRadius; z++) {
                                SetBlock(worldLayer.blockName, x, y, z, false);
                            }
                        }
                    }
                    bedrockLevel += worldLayer.height;
                }
                break;
        }
    }

    public override void _Process(double delta) {
        if (Input.IsActionPressed("exit"))
            GetTree().Quit();
    }
}