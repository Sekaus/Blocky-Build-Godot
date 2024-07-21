using Godot;
using System;

public partial class PlayerController : RigidBody3D {
	[Export]
	public float WalkSpeed = 100f;
	[Export]
	public float JumpPower = 300f;
	[Export]
	public float MouseSensitivity = 2f;
	public Inventory Hotbar = new Inventory(10, 1);
	public Godot.Collections.Dictionary<int, Control> HotBarGUISlots = new Godot.Collections.Dictionary<int, Control>();

	Node3D leftHand;
	Camera3D playerCamera;
	RayCast3D playerRayCast;
	Area3D groundArea;

	Game game;
	WorldData worldData;
	Control inventoryGUI;

	WeakReference<CsgMesh3D> blockInFocusRef = new WeakReference<CsgMesh3D>(null);

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

			if (Hotbar.Slots[toSlot]?.ItemName != item.ItemName) {
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

	// Add item in left hand
	private void AddItemInLeftHand(Item item) {
		// Remove existing item in left hand if necessary
		if (leftHand.GetChildCount() > 0) {
			Item existingItem = leftHand.GetChild<Item>(0);
			if (existingItem != null) {
				leftHand.RemoveChild(existingItem);
				existingItem.QueueFree();
			}
		}

		// Add the new item to the left hand
		if (item != null) {
			leftHand.AddChild(item);
			item.Owner = leftHand; // Ensure proper ownership

			// Center the item to its parent
			if (item is Node3D item3D) {
				item3D.GlobalTransform = leftHand.GlobalTransform;
				item3D.Position = Vector3.Zero;
			}
		}
	}

	// Remove item in left hand
	private void RemoveItemInLeftHand() {
		if (leftHand.GetChildCount() > 0) {
			Node oldItemNode = leftHand.GetChild<Item>(0);
			leftHand.RemoveChild(oldItemNode);
			oldItemNode.QueueFree();
		}
	}

	// Equip item from the hotbar
	private void EquipItemFromHotbar(int slotIndex) {
		if (Hotbar.Slots.Length > slotIndex && Hotbar.Slots[slotIndex] != null) {
			Item newItem = Register.Blocks[Hotbar.Slots[slotIndex].ItemName]?.Instantiate<Item>();
			AddItemInLeftHand(newItem);
		}
		else
			RemoveItemInLeftHand();
	}

	public string GetItemInLeftHand() {
		if (leftHand.GetChildOrNull<Item>(0) != null)
			return leftHand.GetChild<Item>(0).ItemName;
		return "";
	}

	public override void _Ready() {
		game = GetParent<Game>();
		worldData = game.worldData; // this will be used later...
		leftHand = GetNode<Node3D>("%LeftHand");
		playerCamera = GetNode<Camera3D>("%PlayerCamera");
		playerRayCast = GetNode<RayCast3D>("%PlayerRayCast");
		inventoryGUI = GetNode<Control>("%InventoryGUI");
		groundArea = GetNode<Area3D>("%GroundArea");

		// Connect to signels

		groundArea.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));

		// Setup inventory GUI

		for (int i = 0; i < Hotbar.Slots.Length; i++) {
			Control newItemSlot = Register.GUIElements["ItemSlotGUI"].Instantiate<Control>();
			HotBarGUISlots.Add(i, newItemSlot);

			if (i < Hotbar.Height)
				inventoryGUI.GetNode("ItemContainerGUIH").GetNode("ItemContainerGUIV").AddChild(newItemSlot);
			else
				inventoryGUI.GetNode("ItemContainerGUIH").AddChild(newItemSlot);
		}

		AddItemToHotbar(Register.Blocks["Dirt"].Instantiate<Item>());
		AddItemToHotbar(Register.Blocks["GrassBlock"].Instantiate<Item>());
		AddItemToHotbar(Register.Blocks["OkePlanks"].Instantiate<Item>());
		AddItemToHotbar(Register.Blocks["OkeDoorTypeA"].Instantiate<Item>());
        AddItemToHotbar(Register.Blocks["OkeRoof"].Instantiate<Item>());
		AddItemToHotbar(Register.Blocks["Stone"].Instantiate<Item>());
		AddItemToHotbar(Register.Blocks["StoneStairs"].Instantiate<Item>());
		AddItemToHotbar(Register.Blocks["StoneFence"].Instantiate<Item>());
		AddItemToHotbar(Register.Blocks["GlassBlock"].Instantiate<Item>());

		EquipItemFromHotbar(0);
	}

	// Method to handle the collision event of HitBox
	private void OnBodyEntered(Node body) {
		if (body is CollisionObject3D) {
			// Check if the body is a ground-like object by collision layer/mask or other criteria
			onGroundDetect = true;
		}
	}

	// move player
	bool onGroundDetect = false;
	Vector3 velocity;
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

		if (Input.IsActionJustPressed("jump") && onGroundDetect) {
			velocity += Transform.Basis.Y * JumpPower * ((float)delta);
			onGroundDetect = false;
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

	public override void _Input(InputEvent inputEvent) {
		if (inputEvent is InputEventMouseMotion mouseMotion) {
			// Rotate player and player camera
			playerRotateX = Mathf.DegToRad(-mouseMotion.Relative.Y);
			playerRotateY = Mathf.DegToRad(-mouseMotion.Relative.X);
		}

		// RayCast Hit
		if (playerRayCast.CollideWithBodies) {
			var collider = playerRayCast.GetCollider();
			if (collider is Node node) {
				foreach (string group in node.GetGroups()) {
					if (group == "block") {
						Block block = (Block)node;

						// Safely load the material
						Material edgeHighlight = (Material)GD.Load("res://Assets/Shaders/EdgeHighlight.tres");
						if (edgeHighlight == null) {
							GD.PrintErr("Failed to load material");
							return;
						}

						// Get the CsgMesh3D node from the block
						var blockMesh = block.GetNode<CsgMesh3D>("Mesh");

						if (!IsInstanceValid(blockMesh)) {
							GD.PrintErr("CsgMesh3D node is not valid");
							return;
						}

						// Apply highlight
						if (blockInFocusRef.TryGetTarget(out var blockInFocus) && IsInstanceValid(blockInFocus)) {
							// Remove previous highlight if exists
							if (blockInFocus.Material?.NextPass != null)
								blockInFocus.Material.NextPass = null;

							// Set new highlight
							blockInFocusRef.SetTarget(blockMesh);

							if (blockMesh.Material != null)
								blockMesh.Material.NextPass = edgeHighlight;
						}
						else {
							// Set new highlight if blockInFocus was null
							blockInFocusRef.SetTarget(blockMesh);
							if (blockMesh.Material != null)
								blockMesh.Material.NextPass = edgeHighlight;
						}

						// Perform actions based on input

						if (Input.IsActionJustPressed("hit_and_remove")) {
							if (block.BlockName != "Bedrock") {
								game.RemoveBlock(Mathf.RoundToInt(block.Position.X), Mathf.RoundToInt(block.Position.Y), Mathf.RoundToInt(block.Position.Z));
								blockInFocusRef.SetTarget(null);
							}
						}
						else if (Input.IsActionJustPressed("integrate_and_place")) {
							if (GetItemInLeftHand() != "") {
								Vector3 normal = playerRayCast.GetCollisionNormal();
								Vector3 newBlockPose = block.Position + normal;

								Block newBlock = Register.Blocks[GetItemInLeftHand()]?.Instantiate<Block>();

								// Rotate the new block if it use rotation
								Vector3 rotation;
								if (newBlock.Type != Block.BlockType.Roof && newBlock.Type != Block.BlockType.Stairs && newBlock.Type != Block.BlockType.Door)
									rotation = Vector3.Zero;
								else {
									rotation = -Position.DirectionTo(newBlockPose).Round();
									if(newBlock.Type != Block.BlockType.Door)
										rotation.Y -= playerCamera.Basis.Z.Y;
								}

								if (block.BlockName != "Grass")
									game.SetBlock(newBlock, Mathf.RoundToInt(newBlockPose.X), Mathf.RoundToInt(newBlockPose.Y), Mathf.RoundToInt(newBlockPose.Z), true, rotation);
								else {
									if (GetItemInLeftHand() != "") {
										game.RemoveBlock(Mathf.RoundToInt(newBlockPose.X), Mathf.RoundToInt(newBlockPose.Y - 1), Mathf.RoundToInt(newBlockPose.Z));
										game.SetBlock(newBlock, Mathf.RoundToInt(newBlockPose.X), Mathf.RoundToInt(newBlockPose.Y - 1), Mathf.RoundToInt(newBlockPose.Z), true, rotation);
									}
								}
							}
							else if (block is Door)
								block.OnClick();
						}
					}
				}
			}
			else {
				if (blockInFocusRef.TryGetTarget(out var blockInFocus) && IsInstanceValid(blockInFocus)) {
					// Remove previous highlight if exists
					if (blockInFocus.Material?.NextPass != null)
						blockInFocus.Material.NextPass = null;
				}
			}
		}

		// Hotbar shortcuts
		for (int i = 0; i < 10; i++) {
			if (Input.IsActionJustPressed($"hotbar_slot_{i}")) {
				EquipItemFromHotbar(i);
			}
		}
	}
}