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

    public bool unbreakable = false;
    public FacingDirections FacingDirection = FacingDirections.Forward;
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
        Fence
    }
    public enum FacingDirections { 
        Forward, 
        Backward,
        Right,
        Left
    }
}