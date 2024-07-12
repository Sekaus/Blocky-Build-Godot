using Godot;
using System;

//This is the register over all content in this game
public partial class Register : Node {
    [Export]
    public PackedScene[] blockScenes;
    public Godot.Collections.Dictionary<string, PackedScene> Blocks = new Godot.Collections.Dictionary<string, PackedScene>();

    public override void _EnterTree() {
        foreach (PackedScene blockScene in blockScenes) {
            Blocks.Add(blockScene.Instantiate().Name, blockScene);
        }
    }
}