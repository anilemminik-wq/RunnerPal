#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Builds the runner characters from Quaternius "Ultimate Modular Men" (CC0) parts:
// base look = Beach body (tank top, recolored white) + Beach legs (red shorts = "don") + Beach feet, one head per
// character; outfit = Suit legs (Pantolon), Suit feet (Ayakkabı), Suit body (Gömlek + Ceket via SuitTorso), and
// small props on bones for Saat / Telefon / Laptop. All parts are rebound to one armature and driven by one
// Animator (Run / Idle / Roll-as-slide / HitRecieve / a Jump pose made from Run).
public static class ModularCharacterBuilder
{
    public const string PartsDir = "Assets/RunnerPal/ThirdParty/Quaternius_UltimateModularMen/";
    const string AnimDir = "Assets/RunnerPal/Animation/";
    const string ModelDir = "Assets/RunnerPal/Models/";
    const string MatDir = "Assets/RunnerPal/Materials/";

    static Material Lit(string name, Color color, float smooth = 0.25f)
    {
        string path = MatDir + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", smooth);
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject Part(string name) => AssetDatabase.LoadAssetAtPath<GameObject>(PartsDir + name + ".fbx");

    static AnimationClip Clip(string name) =>
        AssetDatabase.LoadAllAssetsAtPath(PartsDir + "Animations.fbx").OfType<AnimationClip>()
            .FirstOrDefault(c => c.name == "CharacterArmature|" + name);

    // ---------- Animation ----------

    // Run / Idle loop; everything else plays once.
    static void SetupClipImport()
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(PartsDir + "Animations.fbx");
        var clips = importer.defaultClipAnimations;
        foreach (var c in clips)
            c.loopTime = c.name.EndsWith("|Run") || c.name.Contains("|Idle");
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    // The pack has no jump, so hold a mid-stride frame of the run (legs apart) as a leap pose.
    static AnimationClip MakeJumpPose(AnimationClip run)
    {
        string path = AnimDir + "Jump.anim";
        var jump = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (jump == null) { jump = new AnimationClip(); AssetDatabase.CreateAsset(jump, path); }
        jump.ClearCurves();
        float t = run.length * 0.25f;
        foreach (var binding in AnimationUtility.GetCurveBindings(run))
        {
            float v = AnimationUtility.GetEditorCurve(run, binding).Evaluate(t);
            AnimationUtility.SetEditorCurve(jump, binding, new AnimationCurve(new Keyframe(0f, v), new Keyframe(0.6f, v)));
        }
        EditorUtility.SetDirty(jump);
        return jump;
    }

    public static AnimatorController BuildController()
    {
        if (!AssetDatabase.IsValidFolder("Assets/RunnerPal/Animation")) AssetDatabase.CreateFolder("Assets/RunnerPal", "Animation");
        SetupClipImport();
        var run = Clip("Run");
        var idle = Clip("Idle");
        var roll = Clip("Roll");
        var hit = Clip("HitRecieve");
        var jump = MakeJumpPose(run);

        string path = AnimDir + "Runner.controller";
        AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        // PlayerController's parameters.
        ctrl.AddParameter("Running", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Slide", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hit", AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;
        var sIdle = sm.AddState("Idle"); sIdle.motion = idle;
        var sRun = sm.AddState("Run"); sRun.motion = run;
        var sJump = sm.AddState("Jump"); sJump.motion = jump;
        var sSlide = sm.AddState("Slide"); sSlide.motion = roll;
        sSlide.speed = roll.length / 0.7f; // PlayerController.slideDuration
        var sHit = sm.AddState("Hit"); sHit.motion = hit;
        sm.defaultState = sIdle;

        var toRun = sIdle.AddTransition(sRun); toRun.AddCondition(AnimatorConditionMode.If, 0, "Running"); toRun.duration = 0.15f; toRun.hasExitTime = false;
        var toIdle = sRun.AddTransition(sIdle); toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Running"); toIdle.duration = 0.2f; toIdle.hasExitTime = false;
        foreach (var (state, trigger) in new[] { (sJump, "Jump"), (sSlide, "Slide"), (sHit, "Hit") })
        {
            var any = sm.AddAnyStateTransition(state);
            any.AddCondition(AnimatorConditionMode.If, 0, trigger);
            any.duration = 0.08f;
            any.hasExitTime = false;
            any.canTransitionToSelf = false;
            var back = state.AddTransition(sRun);
            back.hasExitTime = true;
            back.exitTime = 0.9f;
            back.duration = 0.12f;
            var stop = state.AddTransition(sIdle);
            stop.AddCondition(AnimatorConditionMode.IfNot, 0, "Running");
            stop.hasExitTime = false;
            stop.duration = 0.15f;
        }
        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    // ---------- Assembly ----------

    // Instantiates each part and rebinds its skinned mesh to the first part's armature (bones matched by name).
    static GameObject Assemble(string name, IEnumerable<string> parts, Dictionary<string, SkinnedMeshRenderer> byPart)
    {
        var root = new GameObject(name);
        Transform armature = null;
        Dictionary<string, Transform> bones = null;
        foreach (string partName in parts)
        {
            var instance = (GameObject)Object.Instantiate(Part(partName));
            var smr = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            if (armature == null)
            {
                armature = instance.transform.Find("CharacterArmature");
                armature.SetParent(root.transform, false);
                bones = armature.GetComponentsInChildren<Transform>().ToDictionary(t => t.name, t => t);
            }
            else
            {
                smr.bones = smr.bones.Select(b => bones[b.name]).ToArray();
                smr.rootBone = bones[smr.rootBone.name];
            }
            smr.transform.SetParent(root.transform, false);
            smr.name = partName;
            byPart[partName] = smr;
            Object.DestroyImmediate(instance);
        }
        return root;
    }

    static GameObject Prop(string name, Transform bone, Vector3 localPos, Vector3 size, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.name = name;
        // Bones sit under a x100 armature; props are sized in world meters.
        go.transform.SetParent(bone, false);
        float s = 1f / bone.lossyScale.x;
        go.transform.localPosition = localPos * s;
        go.transform.localScale = size * s;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static int Slot(SkinnedMeshRenderer smr, string materialName) =>
        System.Array.FindIndex(smr.sharedMaterials, m => m != null && m.name == materialName);

    // id: character id; head: part name of its head (e.g. "Suit_Head"); skin: skin tone.
    public static GameObject Build(string id, string head, Color skin, AnimatorController controller)
    {
        var smrs = new Dictionary<string, SkinnedMeshRenderer>();
        var go = Assemble("Model_" + id, new[] { "Beach_Body", "Beach_Legs", "Beach_Feet", head, "Suit_Body", "Suit_Legs", "Suit_Feet" }, smrs);

        // Skin tone on every "Skin" slot; tank top = white instead of the beach shirt's color.
        var skinMat = Lit("CharSkin_" + id, skin, 0.2f);
        var tank = Lit("CharTankTop", new Color(0.96f, 0.96f, 0.96f), 0.1f);
        foreach (var smr in smrs.Values)
        {
            var mats = smr.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                if (mats[i].name == "Skin") mats[i] = skinMat;
                else if (smr.name == "Beach_Body" && mats[i].name == "LightBrown") mats[i] = tank;
            }
            smr.sharedMaterials = mats;
        }

        var armature = go.transform.Find("CharacterArmature");
        var animator = armature.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // Props for the later worlds.
        var bones = armature.GetComponentsInChildren<Transform>().ToDictionary(t => t.name, t => t);
        var watchMat = Lit("Prop_Watch", new Color(0.95f, 0.75f, 0.2f), 0.7f);
        var phoneMat = Lit("Prop_Phone", new Color(0.08f, 0.08f, 0.1f), 0.8f);
        var laptopMat = Lit("Prop_Laptop", new Color(0.72f, 0.75f, 0.8f), 0.6f);
        var watch = Prop("Outfit_Saat", bones["Wrist.L"], new Vector3(0f, 0f, 0f), new Vector3(0.09f, 0.035f, 0.09f), watchMat);
        var phone = Prop("Outfit_Telefon", bones["Wrist.R"], new Vector3(0f, 0.07f, 0f), new Vector3(0.05f, 0.1f, 0.012f), phoneMat);
        var laptop = Prop("Outfit_Laptop", bones["Wrist.L"], new Vector3(0f, 0.12f, 0f), new Vector3(0.3f, 0.02f, 0.22f), laptopMat);

        var outfit = go.AddComponent<PlayerOutfit>();
        GameObject O(string part) => smrs[part].gameObject;
        outfit.visuals = new[]
        {
            new PlayerOutfit.ItemVisual { item = ItemType.Gomlek, visual = O("Suit_Body"), replaces = new[] { O("Beach_Body") } },
            new PlayerOutfit.ItemVisual { item = ItemType.Ceket, visual = O("Suit_Body"), replaces = new[] { O("Beach_Body") } },
            new PlayerOutfit.ItemVisual { item = ItemType.Pantolon, visual = O("Suit_Legs"), replaces = new[] { O("Beach_Legs") } },
            new PlayerOutfit.ItemVisual { item = ItemType.Ayakkabi, visual = O("Suit_Feet"), replaces = new[] { O("Beach_Feet") } },
            new PlayerOutfit.ItemVisual { item = ItemType.Saat, visual = watch },
            new PlayerOutfit.ItemVisual { item = ItemType.Telefon, visual = phone },
            new PlayerOutfit.ItemVisual { item = ItemType.Laptop, visual = laptop },
        };

        var suit = smrs["Suit_Body"];
        var torso = suit.gameObject.AddComponent<SuitTorso>();
        var suitMats = suit.sharedMaterials;
        torso.outfit = outfit;
        torso.jacketSlot = Slot(suit, "Suit");
        torso.shirtSlot = Slot(suit, "White");
        torso.tieSlot = Slot(suit, "Tie");
        torso.jacket = suitMats[torso.jacketSlot];
        torso.shirt = suitMats[torso.shirtSlot];
        torso.tie = suitMats[torso.tieSlot];
        torso.skin = skinMat;

        if (!AssetDatabase.IsValidFolder("Assets/RunnerPal/Models")) AssetDatabase.CreateFolder("Assets/RunnerPal", "Models");
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, ModelDir + "Model_" + id + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }
}
#endif
