using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardArtTable", menuName = "ArtAssets/CardArtTable")]
public class CardArtTable : ScriptableObject
{
    [System.Serializable]
    public class CardArtEntry
    {
        public string cardName;
        public Sprite cardArt;
    }
    
    public List<CardArtEntry> table = new List<CardArtEntry>();

    private Dictionary<string, Sprite> _runtimeDictionary;

    public void Initialize() {
        _runtimeDictionary = new Dictionary<string, Sprite>();
        foreach (var entry in table)
        {
            if (entry.cardArt != null && !_runtimeDictionary.ContainsKey(entry.cardName)) {
                _runtimeDictionary.Add(entry.cardName, entry.cardArt);
            }
        }
    }
    
    public Sprite GetCardArt(string cardName) {
        if (_runtimeDictionary == null) {
            Initialize();
        }
        
        if (_runtimeDictionary.TryGetValue(cardName, out Sprite art)) {
            return art;
        }
        
        Debug.LogWarning("Card Art not found: " + cardName);
        return null;
    }
}
