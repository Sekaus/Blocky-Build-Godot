using Godot;
using System;

public partial class Block : StaticBody3D {
    [Export]
    public Texture Particle;
    [Export]
    public AudioEffect[] BreakAudioEffects;
    [Export]
    public AudioEffect[] PlaceAndStapAudioEffects;
    [Export]
    public BlockType Type = BlockType.Normal;
    [Export]
    public string VariationOfBlock = "";
    [Export]
    public string[] Tags;
    [Export]
    public bool canBeConnected = true;

    public bool Unbreakable = false;
    public FacingDirections FacingDirection = FacingDirections.Forward;
    public bool UpsideDown = false;

    private string blockName = "";
    public string BlockName { 
        get {
            if (blockName != "")
                return blockName;
            else
                return Name;
        } 
        set { 
            blockName = value; 
        } 
    }
    
    public enum BlockType {
        Normal,
        Fence,
        Roof,
        Stairs,
        Door
    }
    public enum FacingDirections { 
        Forward, 
        Backward,
        Right,
        Left,
        None
    }

    public Block() { }

    public Block(bool noFacing) {
        FacingDirection = FacingDirections.None;
    }
    
    // Event handlers

    public virtual void OnClick() { }
}