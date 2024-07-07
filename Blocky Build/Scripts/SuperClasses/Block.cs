using Godot;
using System;

public partial class Block : StaticBody3D {
    [Export]
    public Texture particle;
    [Export]
    public AudioEffect[] breakAudioEffects;
    [Export]
    public AudioEffect[] placeAndStapAudioEffects;
    public int id;

    // when a player steps on this block
    void onPlayerStepOn() {}

    // when a player steps off this block
    void onPlayerStepOff() {}

    // when a player place this block
    void onPlayerPlace() {}

    // when a player remove this block
    void onPlayerDestroy() {}
}