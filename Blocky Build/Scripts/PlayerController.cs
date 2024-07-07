using Godot;
using System;

public partial class PlayerController : RigidBody3D {
	WorldData worldData;
	[Export]
	public float walkSpeed = 100f;
	[Export]
	public float jumpPower = 300f;
	[Export]
	public float mouseSensitivity = 2f;
	Game game;
	Camera3D playerCamera;
	RayCast3D groundRayCast;
	RayCast3D playerRayCast;
	public override void _Ready() {
		game = GetParent().GetParent<Game>();
		worldData = GetParent<WorldData>();
		playerCamera = GetNode<Camera3D>("%PlayerCamera");
		playerRayCast = GetNode<RayCast3D>("%PlayerRayCast");
	}

	// move player
	Vector3 velocity;
	float maxLinearVelocityYForJump = 0.1f;
	float playerRotateX;
	float playerRotateY;
	float playerCameraRotateionX;
	float maxPlayerCameraRotateionX = 1.5870861f;
	public override void _PhysicsProcess(double delta) {
		velocity = new Vector3(0.0f, LinearVelocity.Y, 0.0f);

		if (Input.IsActionPressed("walk_forword")) {
			velocity -= Transform.Basis.Z;
		}
		if (Input.IsActionPressed("walk_back")) {
			velocity += Transform.Basis.Z;
		}
		if (Input.IsActionPressed("walk_left")) {
			velocity -= Transform.Basis.X;
		}
		if (Input.IsActionPressed("walk_right")) {
			velocity += Transform.Basis.X;
		}

		if (Input.IsActionJustPressed("jump") && (LinearVelocity.Y < maxLinearVelocityYForJump && LinearVelocity.Y > -maxLinearVelocityYForJump)) {
			velocity += Transform.Basis.Y * jumpPower * ((float)delta);
		}

		//Vector3 normalizedVelocity = velocity.Normalized();
		velocity = new Vector3(velocity.X * walkSpeed * ((float)delta), velocity.Y, velocity.Z * walkSpeed * ((float)delta));

		LinearVelocity = velocity;

		// rotate player
		playerRotateX *= ((float)delta) * mouseSensitivity;
		playerCameraRotateionX = Mathf.Clamp(playerCameraRotateionX + playerRotateX, -maxPlayerCameraRotateionX, maxPlayerCameraRotateionX);
		if (playerCameraRotateionX < maxPlayerCameraRotateionX && playerCameraRotateionX > -maxPlayerCameraRotateionX)
			playerCamera.RotateX(playerRotateX);
		RotateY(playerRotateY * ((float)delta) * mouseSensitivity);

		playerRotateX = 0f;
		playerRotateY = 0f;
	}

    public override void _Input(InputEvent @event) {
		if(@event is InputEventMouseMotion mouseMotion) {
			// rotate player and player camera
			playerRotateX = Mathf.DegToRad(-mouseMotion.Relative.Y);
			playerRotateY = Mathf.DegToRad(-mouseMotion.Relative.X);
		}

		// rayCast Hit
		if(playerRayCast.CollideWithBodies) {
			var collider = playerRayCast.GetCollider();
			if (collider is Node node) {
				//distanceToCollectedNode = GlobalTransform.Origin.DistanceSquaredTo(playerLookRayCast.GetCollisionPoint());
				foreach (string groupe in node.GetGroups()) {
					switch (groupe) {
						case "block":
							if (Input.IsActionJustPressed("hit_and_remove")) {
								node.QueueFree();
							}
							else if(Input.IsActionJustPressed("integrate_and_place")) {
								Vector3 collisionPoint = playerRayCast.GetCollisionPoint();
								Vector3 distanceToCollisionPoint = playerCamera.GlobalPosition - collisionPoint;
								StaticBody3D block = (StaticBody3D)node;
								Vector3 newBlockPose = distanceToCollisionPoint.Round().Normalized() + block.Position;
								game.setBlock(0, Mathf.RoundToInt(newBlockPose.X), Mathf.RoundToInt(newBlockPose.Y), Mathf.RoundToInt(newBlockPose.Z));
                            }
							break;
					}
				}
			}
		}
	}
}