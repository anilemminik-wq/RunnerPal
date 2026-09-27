// TMP setup lessons from DirectBall:
// 1) Remove TMP's EmojiOne sample sprites (license needs attribution; the game uses no emoji). They sit in a
//    Resources folder, so they would ship in every build otherwise.
// 2) Turkish letters render from the dynamic "LiberationSans SDF - Fallback" font, which TMP empties on every
//    build by default -> blank ı / ş / ğ on the phone. Bake the Turkish + Latin set in and never clear it.
// Safe to rerun.
var settingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
var settings = UnityEditor.AssetDatabase.LoadMainAssetAtPath(settingsPath);
var sso = new UnityEditor.SerializedObject(settings);
sso.FindProperty("m_defaultSpriteAsset").objectReferenceValue = null;
sso.ApplyModifiedPropertiesWithoutUndo();
foreach (var p in new[] { "Assets/TextMesh Pro/Resources/Sprite Assets", "Assets/TextMesh Pro/Sprites" })
    if (UnityEditor.AssetDatabase.IsValidFolder(p)) UnityEditor.AssetDatabase.DeleteAsset(p);

const string fallbackPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
var fallback = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(fallbackPath);
string chars = " 0123456789+-–·:;.,!?'\"()/%©×x"
    + "abcçdefgğhıijklmnoöprsştuüvyzqwx"
    + "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZQWX";
string missing;
fallback.ClearFontAssetData(true);
fallback.TryAddCharacters(chars, out missing);
var fso = new UnityEditor.SerializedObject(fallback);
fso.FindProperty("m_ClearDynamicDataOnBuild").boolValue = false;
fso.ApplyModifiedPropertiesWithoutUndo();
UnityEditor.EditorUtility.SetDirty(fallback);
foreach (var tex in fallback.atlasTextures) if (tex != null) UnityEditor.EditorUtility.SetDirty(tex);
if (fallback.material != null) UnityEditor.EditorUtility.SetDirty(fallback.material);
UnityEditor.EditorUtility.SetDirty(settings);
UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.AssetDatabase.ImportAsset(fallbackPath, UnityEditor.ImportAssetOptions.ForceUpdate);
return "fonts ok: fallback " + fallback.characterTable.Count + " chars, missing '" + missing + "'";
