using Godot;
using System;
public partial class Game : Node {
    [Export]
    public Node map;
    [Export]
    public WorldData worldData;
    private Register register;

    //Set block in world at xyz
    public void setBlock(int blockID, int x, int y, int z) {
        if (!isThereABlockAt(x, y, z)) {
            StaticBody3D newBlock = register.block[blockID].Instantiate<StaticBody3D>();
            newBlock.Position = new Vector3I(x, y, z);
            newBlock.Name = "block_" + x + "_" + y + "_" + z;
            ((Block)newBlock).id = blockID;
            map.AddChild(newBlock);
        }
    }

    //Remove block in world at xyz
    public void removeBlock(int x, int y, int z) {
        string blockName = "block_" + x + "_" + y + "_" + z;
        map.GetNode(blockName).QueueFree();
    }

    //Test if there is a block at xyz
    public bool isThereABlockAt(int x, int y, int z) {
        string blockName = "block_" + x + "_" + y + "_" + z;
        return map.GetNodeOrNull(blockName) is not null;
    }

    private int worldSizeX = 16, worldSizeY = 16, worldSizeZ = 16;

    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        register = GetNode<Register>((NodePath)"%Register");
        //Genarate world
        switch (worldData.worldType) {
            case WorldData.WorldType.flat:
                for (int x = 0; x <= worldSizeX; x++) {
                    for (int y = 0; y <= worldSizeY; y++) {
                        for (int z = 0; z <= worldSizeZ; z++) {
                            setBlock(0, x, y, z);
                        }
                    }
                }
                break;
        }
    }

    public override void _Process(double delta) {
        if (Input.IsActionPressed("exit"))
            GetTree().Quit();
    }
}