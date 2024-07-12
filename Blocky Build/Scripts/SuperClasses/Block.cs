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

    public FacingDirections FacingDirection = FacingDirections.Forward;
    public string BlockName = "";
    public bool Rotated = false;
    
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