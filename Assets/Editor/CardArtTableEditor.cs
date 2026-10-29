using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CardArtTable))]
public class CardArtTableEditor : Editor
{
    private string folderPath = "Assets/Art/Card Deck/Standard/";

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        CardArtTable cardArtTable = (CardArtTable)target;
        
        GUILayout.Space(15);
        folderPath = EditorGUILayout.TextField("Source Folder Path", folderPath);
        
        if (GUILayout.Button("Populate Card Art Table from Folder", GUILayout.Height(30))) {
            PopulateTable(cardArtTable);
        }
    }
    
    private void PopulateTable(CardArtTable cardArtTable) {
        if (!Directory.Exists(folderPath)) {
            Debug.LogError("Folder path does not exist: " + folderPath);
            return;
        }
        
        Undo.RecordObject(cardArtTable, "Populate Card Art Table");
        
        cardArtTable.table.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        
        Debug.Log("Found " + guids.Length + " card art textures.");

        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null) {
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in allAssets) {
                    if (asset is Sprite) {
                        sprite = asset as Sprite;
                        break;
                    }
                }
            }
            
            if (sprite != null) {
                cardArtTable.table.Add(new CardArtTable.CardArtEntry() {
                        cardName = Path.GetFileNameWithoutExtension(path),
                        cardArt = sprite
                    });
            }
        }

        EditorUtility.SetDirty(cardArtTable);
        AssetDatabase.SaveAssets();
        Debug.Log("Card Art Table populated with " + cardArtTable.table.Count + " entries.");
    }
}
