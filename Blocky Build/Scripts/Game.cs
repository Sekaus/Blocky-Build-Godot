using Godot;
using System;
public partial class Game : Node {
    [Export]
    public Node map;
    [Export]
    public WorldData worldData;
    private Register register;

    //Set block in world at xyz
    public void SetBlock(string blockName, int x, int y, int z, bool runBlockUpdates = true) {
        if (!IsThereABlockAt(x, y, z)) {
            Block newBlock = register.Blocks[blockName].Instantiate<Block>();

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

    //Replace block in world at xyz
    public void ReplaceBlock(int x, int y, int z, string blockName, bool runBlockUpdates = true) {
        if (IsThereABlockAt(x, y, z)) {
            RemoveBlock(x, y, z);
            SetBlock(blockName, x, y, z, runBlockUpdates);
        }
    }

    //Remove block in world at xyz
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

    //Run at block single update
    public Block UpdateBlock(int atX, int atY, int atZ, Block blockThatExecute) {
        if (blockThatExecute.Type == Block.BlockType.Fence)
            blockThatExecute = UpdateConnectedBlock(atX, atY, atZ, blockThatExecute);

        return blockThatExecute;
    }

    //Collect all connected blocks
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

    //Run at multiple block updates
    public void UpdateBlocks(Godot.Collections.Dictionary<string, Block> blocksToUpdate) {
        if (blocksToUpdate != null || blocksToUpdate.Count > 0) {
            foreach (var block in blocksToUpdate) {
                if (block.Value.Type == Block.BlockType.Fence) {
                    Block updatedBlock = register.Blocks[block.Value.BlockName].Instantiate<Block>();

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

    //Run at single connected block update
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
            blockThatExecute = register.Blocks["StoneFenceConnectedC"].Instantiate<Block>();
        }
        else if (connectionToBlockRight && connectionToBlockLeft && connectionToBlockForward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedB"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(-90));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Forward;
        }
        else if (connectionToBlockRight && connectionToBlockLeft && connectionToBlockBackward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedB"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(90));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Backward;
        }
        else if (connectionToBlockRight && connectionToBlockForward && connectionToBlockBackward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedB"].Instantiate<Block>();
            blockThatExecute.FacingDirection = Block.FacingDirections.Right;
        }
        else if (connectionToBlockLeft && connectionToBlockForward && connectionToBlockBackward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedB"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(180));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Left;
        }
        else if (connectionToBlockForward && connectionToBlockBackward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedMeddel"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(90));
            blockThatExecute.Rotated = true;
        }
        else if (connectionToBlockRight && connectionToBlockLeft) {
            blockThatExecute = register.Blocks["StoneFenceConnectedMeddel"].Instantiate<Block>();
        }
        else if (connectionToBlockLeft && connectionToBlockForward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedD"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(-90));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Left;
        }
        else if (connectionToBlockRight && connectionToBlockForward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedD"].Instantiate<Block>();
            blockThatExecute.FacingDirection = Block.FacingDirections.Forward;
        }
        else if (connectionToBlockLeft && connectionToBlockBackward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedD"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(180));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Backward;
        }
        else if (connectionToBlockRight && connectionToBlockBackward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedD"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(90));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Right;
        }
        else if (connectionToBlockForward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedA"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(-90));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Forward;
        }
        else if (connectionToBlockBackward) {
            blockThatExecute = register.Blocks["StoneFenceConnectedA"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(90));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Backward;
        }
        else if (connectionToBlockRight) {
            blockThatExecute = register.Blocks["StoneFenceConnectedA"].Instantiate<Block>();
            blockThatExecute.FacingDirection = Block.FacingDirections.Right;
        }
        else if (connectionToBlockLeft) {
            blockThatExecute = register.Blocks["StoneFenceConnectedA"].Instantiate<Block>();
            blockThatExecute.Rotate(Vector3.Up, Mathf.DegToRad(180));
            blockThatExecute.Rotated = true;
            blockThatExecute.FacingDirection = Block.FacingDirections.Left;
        }

        blockThatExecute.BlockName = newBlock;

        return blockThatExecute;
    }

    //Test if there is a block at xyz
    public bool IsThereABlockAt(int x, int y, int z) {
        string blockName = "block_" + x + "_" + y + "_" + z;
        return map.GetNodeOrNull(blockName) is not null;
    }

    //Get block as object at xyz
    public Block getBlockAsObject(int x, int y, int z) {
        string blockName = "block_" + x + "_" + y + "_" + z;
        if(IsThereABlockAt(x, y, z))
            return map.GetNode<Block>(blockName);
        else
            return new Block();
    }

    private int worldSize = 16, worldHeight = 0;
    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        register = GetNode<Register>((NodePath)"%Register");

        //Genarate world
        switch (worldData.worldType) {
            case WorldData.WorldType.Flat:
                foreach (WorldData.WorldLayer worldLayer in worldData.worldTypeLayers[(int)WorldData.WorldType.Flat]) {
                    for (int y = worldHeight; y < worldHeight + worldLayer.height; y++) {
                        for (int x = -worldSize; x <= worldSize; x++) {
                            for (int z = -worldSize; z <= worldSize; z++) {
                                SetBlock(worldLayer.blockName, x, y, z, false);
                            }
                        }
                    }
                    worldHeight += worldLayer.height;
                }
                break;
        }

        foreach (var kay in register.Blocks) {
            GD.Print(kay);
        }
    }

    public override void _Process(double delta) {
        if (Input.IsActionPressed("exit"))
            GetTree().Quit();
    }
}