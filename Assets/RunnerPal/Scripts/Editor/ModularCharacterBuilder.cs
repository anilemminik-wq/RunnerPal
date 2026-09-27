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
// humanoid Animator with Quaternius Universal Animation Library clips (run, jump start/air/land, roll, hit, death).
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

    // ---------- Animation: Quaternius Universal Animation Library (CC0, humanoid) ----------

    public const string UalPath = "Assets/RunnerPal/ThirdParty/Quaternius_UAL/UAL1_Standard.fbx";


    static AnimationClip Clip(string name) =>
        AssetDatabase.LoadAllAssetsAtPath(UalPath).OfType<AnimationClip>().FirstOrDefault(c => c.name == "Armature|" + name);

    // Humanoid import as in Quaternius' Unity setup: bake axis conversion, loop the *_Loop clips, and keep every
    // clip in place (the runner moves by code, not root motion).
    static void SetupClipImport()
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(UalPath);
        importer.bakeAxisConversion = true;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        var clips = importer.defaultClipAnimations;
        foreach (var c in clips)
        {
            c.loopTime = c.name.EndsWith("_Loop");
            c.lockRootRotation = true;
            c.lockRootHeightY = true;
            c.lockRootPositionXZ = true;
            c.keepOriginalOrientation = true;
            c.keepOriginalPositionY = true;
            c.keepOriginalPositionXZ = true;
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    public static AnimatorController BuildController()
    {
        if (!AssetDatabase.IsValidFolder("Assets/RunnerPal/Animation")) AssetDatabase.CreateFolder("Assets/RunnerPal", "Animation");
        SetupClipImport();
        var idle = Clip("Idle_Loop");
        var run = Clip("Sprint_Loop");
        var jumpStart = Clip("Jump_Start");
        var jumpLoop = Clip("Jump_Loop");
        var jumpLand = Clip("Jump_Land");
        var roll = Clip("Roll");
        var hit = Clip("Hit_Chest");
        var death = Clip("Death01");
        if (run == null || jumpStart == null) throw new System.Exception("UAL clips not found in " + UalPath);
        AssetDatabase.DeleteAsset(AnimDir + "Jump.anim"); // the old held-pose jump

        string path = AnimDir + "Runner.controller";
        AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        // PlayerController's parameters.
        ctrl.AddParameter("Running", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Slide", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        // HitFeedback: out of lives -> the runner falls down and stays down.
        ctrl.AddParameter("Fall", AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;
        var sIdle = sm.AddState("Idle"); sIdle.motion = idle;
        var sRun = sm.AddState("Run"); sRun.motion = run;
        // Jump in three parts, timed to PlayerController's ~0.66 s airtime: take-off, in the air, landing.
        var sJumpStart = sm.AddState("JumpStart"); sJumpStart.motion = jumpStart; sJumpStart.speed = jumpStart.length / 0.12f;
        var sJumpAir = sm.AddState("JumpAir"); sJumpAir.motion = jumpLoop;
        var sJumpLand = sm.AddState("JumpLand"); sJumpLand.motion = jumpLand; sJumpLand.speed = jumpLand.length / 0.22f;
        var sSlide = sm.AddState("Slide"); sSlide.motion = roll;
        sSlide.speed = roll.length / 0.7f; // PlayerController.slideDuration
        var sHit = sm.AddState("Hit"); sHit.motion = hit; sHit.speed = 1.4f;
        var sFall = sm.AddState("Fall"); sFall.motion = death;
        sm.defaultState = sIdle;

        AnimatorStateTransition T(AnimatorState from, AnimatorState to, float duration, float exitTime = -1f)
        {
            var t = from.AddTransition(to);
            t.duration = duration;
            t.hasExitTime = exitTime >= 0f;
            if (exitTime >= 0f) t.exitTime = exitTime;
            return t;
        }
        T(sIdle, sRun, 0.15f).AddCondition(AnimatorConditionMode.If, 0, "Running");
        T(sRun, sIdle, 0.2f).AddCondition(AnimatorConditionMode.IfNot, 0, "Running");
        T(sJumpStart, sJumpAir, 0.05f, 0.95f);
        // JumpAir is a loop; leave it after ~0.42 s of air, just before touching down (normalized time = seconds / clip length).
        T(sJumpAir, sJumpLand, 0.06f, 0.42f / Mathf.Max(0.01f, jumpLoop.length));
        T(sJumpLand, sRun, 0.1f, 0.85f);
        T(sSlide, sRun, 0.12f, 0.9f);
        T(sHit, sRun, 0.12f, 0.8f);

        foreach (var (state, trigger) in new[] { (sJumpStart, "Jump"), (sSlide, "Slide"), (sHit, "Hit"), (sFall, "Fall") })
        {
            var any = sm.AddAnyStateTransition(state);
            any.AddCondition(AnimatorConditionMode.If, 0, trigger);
            any.duration = 0.08f;
            any.hasExitTime = false;
            any.canTransitionToSelf = false;
        }
        foreach (var state in new[] { sJumpStart, sJumpAir, sJumpLand, sSlide, sHit })
            T(state, sIdle, 0.15f).AddCondition(AnimatorConditionMode.IfNot, 0, "Running");
        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    // The pack's feet hang off the root (Blender IK); Unity's humanoid wants them under the lower legs.
    // Moving them keeps their world pose, so the skinning (bind poses) is unchanged.
    static void ParentFeetToLegs(Transform armature)
    {
        var bones = armature.GetComponentsInChildren<Transform>().ToDictionary(t => t.name, t => t);
        bones["Foot.L"].SetParent(bones["LowerLeg.L"], true);
        bones["Foot.R"].SetParent(bones["LowerLeg.R"], true);
    }

    // Belly: the abdomen bone widens (bones run along their local Y, so X/Z are girth); the chest takes back most
    // of it so the arms and head keep their size.
    static void Shape(Transform armature, Look look)
    {
        if (look.belly <= 0f || Mathf.Approximately(look.belly, 1f)) return;
        var bones = armature.GetComponentsInChildren<Transform>().ToDictionary(t => t.name, t => t);
        bones["Abdomen"].localScale = new Vector3(look.belly, 1f, look.belly);
        float back = 1.1f / look.belly;
        bones["Torso"].localScale = new Vector3(back, 1f, back);
        float legs = Mathf.Lerp(1f, look.belly, 0.4f);
        bones["UpperLeg.L"].localScale = bones["UpperLeg.R"].localScale = new Vector3(legs, 1f, legs);
    }

    // A humanoid avatar per character (saved next to the controller), mapped by hand because the bone names and the
    // IK feet defeat Unity's automapper. The pack's "Hips" bone only carries the upper body (legs hang off "Body"),
    // so "Body" is the humanoid hips.
    static Avatar BuildAvatar(GameObject root, string id)
    {
        var map = new Dictionary<string, string>
        {
            { "Hips", "Body" }, { "Spine", "Abdomen" }, { "Chest", "Torso" }, { "UpperChest", "Chest" },
            { "Neck", "Neck" }, { "Head", "Head" },
            { "LeftShoulder", "Shoulder.L" }, { "LeftUpperArm", "UpperArm.L" }, { "LeftLowerArm", "LowerArm.L" }, { "LeftHand", "Wrist.L" },
            { "RightShoulder", "Shoulder.R" }, { "RightUpperArm", "UpperArm.R" }, { "RightLowerArm", "LowerArm.R" }, { "RightHand", "Wrist.R" },
            { "LeftUpperLeg", "UpperLeg.L" }, { "LeftLowerLeg", "LowerLeg.L" }, { "LeftFoot", "Foot.L" },
            { "RightUpperLeg", "UpperLeg.R" }, { "RightLowerLeg", "LowerLeg.R" }, { "RightFoot", "Foot.R" },
        };
        var desc = new HumanDescription
        {
            human = map.Select(kv => new HumanBone { humanName = kv.Key, boneName = kv.Value, limit = new HumanLimit { useDefaultValues = true } }).ToArray(),
            skeleton = root.GetComponentsInChildren<Transform>().Select(t => new SkeletonBone
            {
                name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale,
            }).ToArray(),
            upperArmTwist = 0.5f, lowerArmTwist = 0.5f, upperLegTwist = 0.5f, lowerLegTwist = 0.5f,
            armStretch = 0.05f, legStretch = 0.05f, feetSpacing = 0f, hasTranslationDoF = false,
        };
        var avatar = AvatarBuilder.BuildHumanAvatar(root, desc);
        avatar.name = "Avatar_" + id;
        string avatarPath = AnimDir + "Avatar_" + id + ".asset";
        if (!avatar.isValid || !avatar.isHuman) throw new System.Exception("RunnerAvatar is not a valid humanoid");
        AssetDatabase.DeleteAsset(avatarPath);
        AssetDatabase.CreateAsset(avatar, avatarPath);
        return avatar;
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

    static Mesh propMesh;
    static GameObject Prop(string name, Transform bone, Vector3 localPos, Vector3 size, Material mat)
    {
        if (!propMesh) propMesh = ProceduralMesh.RoundedBox(Vector3.one, 0.22f, 3);
        var go = new GameObject(name);
        go.AddComponent<MeshFilter>().sharedMesh = propMesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        // Bones sit under a x100 armature; props are sized in world meters.
        go.transform.SetParent(bone, false);
        float s = 1f / bone.lossyScale.x;
        go.transform.localPosition = localPos * s;
        go.transform.localScale = size * s;
        return go;
    }

    static int Slot(SkinnedMeshRenderer smr, string materialName) =>
        System.Array.FindIndex(smr.sharedMaterials, m => m != null && m.name == materialName);

    // id: character id; head: part name of its head (e.g. "Suit_Head"); skin: skin tone.
    // Colors that make each shop character look different (the outfit rules stay the same).
    public struct Look
    {
        public Color skin, shorts, tankTop, suit, tie;
        // Body shape: whole-body scale (x = width, y = height; zero = normal) and belly size (1 = none).
        public Vector3 bodyScale;
        public float belly;
    }

    public static GameObject Build(string id, string head, Look look, AnimatorController controller)
    {
        var smrs = new Dictionary<string, SkinnedMeshRenderer>();
        var go = Assemble("Model_" + id, new[] { "Beach_Body", "Beach_Legs", "Beach_Feet", head, "Suit_Body", "Suit_Legs", "Suit_Feet" }, smrs);

        // Skin tone on every "Skin" slot; tank top = white instead of the beach shirt's color.
        var skinMat = Lit("CharSkin_" + id, look.skin, 0.2f);
        var shortsMat = Lit("CharShorts_" + id, look.shorts, 0.15f);
        var suitMat = Lit("CharSuit_" + id, look.suit, 0.3f);
        var tieMat = Lit("CharTie_" + id, look.tie, 0.4f);
        var tank = Lit("CharTankTop_" + id, look.tankTop, 0.1f);
        foreach (var smr in smrs.Values)
        {
            var mats = smr.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                if (mats[i].name == "Skin") mats[i] = skinMat;
                else if (smr.name == "Beach_Body" && mats[i].name == "LightBrown") mats[i] = tank;
                else if (mats[i].name == "Red_Dark") mats[i] = shortsMat; // shorts and flip-flop straps
                else if (mats[i].name == "Suit") mats[i] = suitMat;
                else if (mats[i].name == "Tie") mats[i] = tieMat;
            }
            smr.sharedMaterials = mats;
        }

        var armature = go.transform.Find("CharacterArmature");
        ParentFeetToLegs(armature);
        Shape(armature, look);
        // Humanoid so the Universal Animation Library clips retarget onto this skeleton.
        var animator = go.AddComponent<Animator>();
        animator.avatar = BuildAvatar(go, id);
        // Whole-body width / height after the avatar, so the humanoid skeleton stays normal-sized.
        if (look.bodyScale != Vector3.zero) go.transform.localScale = look.bodyScale;
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
        torso.jacketSlot = Slot(suit, suitMat.name);
        torso.shirtSlot = Slot(suit, "White");
        torso.tieSlot = Slot(suit, tieMat.name);
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
