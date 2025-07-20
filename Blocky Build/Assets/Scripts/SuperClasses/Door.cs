using Godot;
using System;

public partial class Door : Block {
    private bool open = false;

    float offset = 0.436f;
    public override void OnClick() {
        open = !open;
        if (open == false) {
            Rotate(Vector3.Up, Mathf.DegToRad(90));
            Position += Basis.X * offset;
        }
        else {
            Position -= Basis.X * offset;
            Rotate(Vector3.Up, Mathf.DegToRad(-90));
        }
    }
}