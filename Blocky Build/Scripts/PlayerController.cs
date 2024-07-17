using Godot;
using System;

public partial class PlayerController : RigidBody3D {
	[Export]
	public float WalkSpeed = 100f;
	[Export]
	public float JumpPower = 300f;
	[Export]
	public float MouseSensitivity = 2f;
	public Inventory Hotbar = new Inventory(6, 1);
	public Godot.Collections.Dictionary<int, Control> HotBarGUISlots = new Godot.Collections.Dictionary<int, Control>();

	Camera3D playerCamera;
	RayCast3D groundRayCast;
	RayCast3D playerRayCast;

	Game game;
	WorldData worldData;
	Control inventoryGUI;

	// Add item and item display to inventory
	public void AddItemToHotbar(Item item, int count = 1, int toSlot = -1) {
		// Find the nearest slot containing the specified item
		if (toSlot == -1) {
			for (int i = 0; i < Hotbar.Slots.Length; i++) {
				if (Hotbar.Slots[i] != null && Hotbar.Slots[i].ItemName == item.ItemName) {
					toSlot = i;
					break;
				}
			}
			// If nothing is found then choose nearest available slot
			if (toSlot == -1) {
				for (int i = 0; i < Hotbar.Slots.Length; i++) {
					if (Hotbar.Slots[i] == null) {
						toSlot = i;
						break;
					}
				}
			}
		}

		// If the item is found in the hotbar
		if (toSlot != -1) {
			Node selectedHotBarGUISlotItem = HotBarGUISlots[toSlot].GetNode("%Item");

			if (Hotbar.Slots[toSlot] != item) {
				// If there's an existing item in the slot, remove its GUI node
				if (Hotbar.Slots[toSlot] != null) {
					if (selectedHotBarGUISlotItem.GetChildCount() > 0) {
						Node oldItemNode = selectedHotBarGUISlotItem.GetChild(0);
						selectedHotBarGUISlotItem.RemoveChild(oldItemNode);
						oldItemNode.QueueFree();
					}
				}

				// Assign the new item to the slot and set its count
				Hotbar.Slots[toSlot] = item;
				Hotbar.Slots[toSlot].Count = count;

				// Add the new item's GUI node
				HotBarGUISlots[toSlot].GetNode("%Item").AddChild(item);
			}
			// Adjust the count
			else if (count > 1) {
				Hotbar.Slots[toSlot].Count += count;
			}

			// Update the item count label
			HotBarGUISlots[toSlot].GetNode<Label>("%ItemCount").Text = (Hotbar.Slots[toSlot].Count <= 0 ? "" : Hotbar.Slots[toSlot].Count.ToString());
		}
	}


	// Remove item and item display from hotbar
	public void RemoveItemFromHotbar(Item item, int count = 0, int toSlot = -1) {
		// Find the nearest slot containing the specified item
		if (toSlot == -1) {
			for (int i = 0; i < Hotbar.Slots.Length; i++) {
				if (Hotbar.Slots[i] != null && Hotbar.Slots[i].ItemName == item.ItemName) {
					toSlot = i;
					break;
				}
			}
		}

		// If the item is found in the hotbar
		if (toSlot != -1) {
			Node selectedHotBarGUISlotItem = HotBarGUISlots[toSlot].GetNode("%Item");

			// Adjust the count
			if (count > 0)
				Hotbar.Slots[toSlot].Count -= count;

			// If the count is zero or less, remove the item from the slot
			if (item.Count <= 0 || count <= 0) {
				if (selectedHotBarGUISlotItem.GetChildCount() > 0) {
					Node oldItemNode = selectedHotBarGUISlotItem.GetChild(0);
					selectedHotBarGUISlotItem.RemoveChild(oldItemNode);
					oldItemNode.QueueFree();
				}

				// Clear the slot in the Hotbar
				Hotbar.Slots[toSlot] = null;
			}

			// Update the item count label
			HotBarGUISlots[toSlot].GetNode<Label>("%ItemCount").Text = (Hotbar.Slots[toSlot]?.Count > 0 ? Hotbar.Slots[toSlot].Count.ToString() : "");
		}
	}



	public override void _Ready() {
		game = GetParent<Game>();
		worldData = game.worldData; // this will be used later...
		playerCamera = GetNode<Camera3D>("%PlayerCamera");
		playerRayCast = GetNode<RayCast3D>("%PlayerRayCast");
		inventoryGUI = GetNode<Control>("%InventoryGUI");

		// Setup inventory GUI

		for (int i = 0; i < Hotbar.Slots.Length; i++) {
			Control newItemSlot = Register.GUIElements["ItemSlotGUI"].Instantiate<Control>();
			HotBarGUISlots.Add(i, newItemSlot);

			if (i < Hotbar.Height)
                inventoryGUI.GetNode("ItemContainerGUIH").GetNode("ItemContainerGUIV").AddChild(newItemSlot);
			else
				inventoryGUI.GetNode("ItemContainerGUIH").AddChild(newItemSlot);
		}

		AddItemToHotbar(Register.Items["Dirt"].Instantiate<Item>(), 100);
		AddItemToHotbar(Register.Items["GrassBlock"].Instantiate<Item>(), 100);
		AddItemToHotbar(Register.Items["OkePlanks"].Instantiate<Item>(), 100);
		AddItemToHotbar(Register.Items["Stone"].Instantiate<Item>(), 100);
		AddItemToHotbar(Register.Items["StoneFence"].Instantiate<Item>(), 100);
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
			velocity += Transform.Basis.Y * JumpPower * ((float)delta);
		}

		velocity = new Vector3(velocity.X * WalkSpeed * ((float)delta), velocity.Y, velocity.Z * WalkSpeed * ((float)delta));

		LinearVelocity = velocity;

		// rotate player
		playerRotateX *= ((float)delta) * MouseSensitivity;
		playerCameraRotateionX = Mathf.Clamp(playerCameraRotateionX + playerRotateX, -maxPlayerCameraRotateionX, maxPlayerCameraRotateionX);
		if (playerCameraRotateionX < maxPlayerCameraRotateionX && playerCameraRotateionX > -maxPlayerCameraRotateionX)
			playerCamera.RotateX(playerRotateX);
		RotateY(playerRotateY * ((float)delta) * MouseSensitivity);

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
				foreach (string groupe in node.GetGroups()) {
					switch (groupe) {
						case "block":
							Block block = (Block)node;
							if (Input.IsActionJustPressed("hit_and_remove")) {
								if(block.BlockName != "Bedrock")
									game.RemoveBlock((int)((Block)node).Position.X, (int)((Block)node).Position.Y, (int)((Block)node).Position.Z);
							}
							else if(Input.IsActionJustPressed("integrate_and_place")) {
								Vector3 newBlockPose = block.Position + playerRayCast.GetCollisionNormal();
								game.SetBlock("StoneFence", Mathf.RoundToInt(newBlockPose.X), Mathf.RoundToInt(newBlockPose.Y), Mathf.RoundToInt(newBlockPose.Z));
                            }
							break;
					}
				}
			}
		}

		// shortcouts

		// hotbar
		/*if (Input.IsActionJustPressed("hotbar_slot_1"))

		else if (Input.IsActionJustPressed("hotbar_slot_2")

		else if (Input.IsActionJustPressed("hotbar_slot_3")

		else if (Input.IsActionJustPressed("hotbar_slot_4")

		else if (Input.IsActionJustPressed("hotbar_slot_5")

		else if (Input.IsActionJustPressed("hotbar_slot_6")

		else if (Input.IsActionJustPressed("hotbar_slot_7")

		else if (Input.IsActionJustPressed("hotbar_slot_8")

		else if (Input.IsActionJustPressed("hotbar_slot_9")

		else if (Input.IsActionJustPressed("hotbar_slot_0")*/

	}
}