using Godot;
using System;
using System.Reflection;

// This is the register over all content in this game
public partial class Register : Node {
    [Export]
    public PackedScene[] BlockScenes;
    public static Godot.Collections.Dictionary<string, RegisterVariant> Blocks  = new Godot.Collections.Dictionary<string, RegisterVariant>();

    [Export]
    public PackedScene[] ItemScenes;
    public static Godot.Collections.Dictionary<string, RegisterVariant> Items = new Godot.Collections.Dictionary<string, RegisterVariant>();

    [Export]
    public PackedScene[] GUIElementScenes;
    public static Godot.Collections.Dictionary<string, PackedScene> GUIElements = new Godot.Collections.Dictionary<string, PackedScene>();

    public partial class RegisterVariant : Node {
        private Variant variant;

        public RegisterVariant(Variant variant) {
            this.variant = variant;
        }

        public PackedScene this[string key] {
            get {
                if (variant.VariantType == Variant.Type.Dictionary) {
                    var dict = (Godot.Collections.Dictionary)variant;
                    if (dict.ContainsKey(key)) {
                        if (dict[key].Obj is Godot.Collections.Dictionary nestedDict && nestedDict.ContainsKey(key)) {
                            if (nestedDict[key].Obj is PackedScene nestedScene)
                                return nestedScene;
                            else
                                throw new Exception($"Key '{key}' exists but is not a PackedScene.");
                        }
                        else if (dict[key].Obj is PackedScene scene)
                            return scene;
                        else
                            throw new Exception($"Key '{key}' exists but is not a PackedScene.");
                    }
                    else
                        throw new Exception($"Key '{key}' not found in the dictionary.");
                }
                else
                    throw new InvalidOperationException($"Variant is not a Godot.Collections.Dictionary.");
            }
            set {
                if (variant.VariantType == Variant.Type.Dictionary) {
                    var dict = (Godot.Collections.Dictionary)variant;
                    dict[key] = value;
                }
                else
                    throw new InvalidOperationException($"Cannot set variant for '{key}' (not a Godot.Collections.Dictionary).");
            }
        }

        public T Instantiate<T>(PackedScene.GenEditState editState = PackedScene.GenEditState.Disabled) where T : Node {
            if (variant.Obj is PackedScene packedScene)
                return packedScene.Instantiate<T>(editState);
            else if (variant.Obj is Godot.Collections.Dictionary dict && dict.ContainsKey("Default") && dict["Default"].Obj is PackedScene defaultScene)
                return defaultScene.Instantiate<T>(editState);
            else if (variant.Obj is T original) {
                T clone = (T)((Node)variant).Duplicate();

                // Ensure the variant is a valid script
                if (variant.Obj is Script script) {
                    clone.SetScript(script);
                }

                // Copy data from the original instance to the clone
                CopyScriptData(original, clone);

                return clone;
            }
            else
                throw new NotImplementedException("Instantiate can only be used on types of class PackedScene and pre-Instantiated assets.");
        }

        private void CopyScriptData<T>(T source, T destination) where T : Node {
            Type type = typeof(T);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                field.SetValue(destination, field.GetValue(source));
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (property.CanWrite) {
                    property.SetValue(destination, property.GetValue(source));
                }
            }
        }

        public void Add(string key, Variant value) {
            if (variant.VariantType == Variant.Type.Dictionary)
                ((Godot.Collections.Dictionary)variant).Add(key, value);
            else
                throw new InvalidOperationException("Cannot use .Add on non-dictionary variants.");
        }

        public System.Collections.Generic.ICollection<string> Keys {
            get {
                if (variant.VariantType == Variant.Type.Dictionary)
                    return ((Godot.Collections.Dictionary<string, PackedScene>)variant).Keys;
                else
                    throw new NotImplementedException("Cannot use .Keys on non-dictionary variants.");
            }
        }

        public System.Collections.Generic.ICollection<PackedScene> Values {
            get {
                if (variant.VariantType == Variant.Type.Dictionary)
                    return ((Godot.Collections.Dictionary<string, PackedScene>)variant).Values;
                else
                    throw new NotImplementedException("Cannot use .Values on non-dictionary variants.");
            }
        }

        public T ToVariant<T>() where T : Node {
            return (T)variant;
        }

        public Variant ToVariant() {
            return variant;
        }
    }

    public override void _EnterTree() {
        // Load in items
        foreach (PackedScene itemScene in ItemScenes) {
            Items.Add(itemScene.Instantiate<Item>().ItemName, new RegisterVariant(itemScene));
        }

        // Load in Blocks
        foreach (PackedScene blockScene in BlockScenes) {
            Block block = blockScene.Instantiate<Block>();

            if (block.VariationOfBlock == "") {
                Blocks.Add(block.BlockName, new RegisterVariant(blockScene));

                // Load in block as item (pre-Instantiated)
                Item newItem = Items["BlockItem"].Instantiate<Item>();
                newItem.AddChild(block);
                
                newItem.ItemName = block.BlockName;
                newItem.Tags = block.Tags;

                Items.Add(block.BlockName, new RegisterVariant(newItem));
            }
            else {
                if (Blocks.ContainsKey(block.VariationOfBlock)) {
                    if (Blocks[block.VariationOfBlock].ToVariant().Obj is PackedScene oldBlockScene) {
                        // Convert the single PackedScene entry to a Godot.Collections.Dictionary entry if not already done
                        Blocks[block.VariationOfBlock] = new RegisterVariant(new Godot.Collections.Dictionary { ["Default"] = oldBlockScene });
                    }
                }

                string keyName = block.BlockName;
                int index = keyName.IndexOf(block.VariationOfBlock);
                keyName = (index < 0) ? keyName : keyName.Remove(index, block.VariationOfBlock.Length);

                if (Blocks[block.VariationOfBlock].ToVariant().Obj is Godot.Collections.Dictionary dict)
                    dict[keyName] = blockScene;
                else
                    throw new InvalidOperationException($"Expected dictionary but found {Blocks[block.VariationOfBlock].ToVariant().Obj.GetType().Name}");
            }
        }

        // Load in GUI elements
        foreach (PackedScene GUIElementScene in GUIElementScenes) {
            GUIElements.Add(GUIElementScene.Instantiate().Name, GUIElementScene);
        }
    }
}