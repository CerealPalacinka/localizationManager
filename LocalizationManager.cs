using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class LocalizationManager : MonoBehaviour
{
    // Reference to current instance of LocalizationManager
    public static LocalizationManager instance;
    static Action refreshCallback;

    // Dictionaries containing localized strings
    Dictionary<string, string> localizedText;
    Dictionary<string, DialogueData []> localizedDialogue;

    // Return this string if no dictionary item matches the key
    string missingTextString = " Missing Text";

    // Array of all json files converted to LocalizationData
    LocalizationData [] loadedLocalizationData;

    void Awake ()
    {
        // Assign current LocalizationManager to instance, else destroy this Object so that there is always only one instance in scene
        if (instance == null)
            instance = this;
        else
            Destroy (gameObject);
        
        GetLocalizationFiles ();
    }

    // Load all localization files from Locales folder, if a function is passed it is executed at the end
    public void GetLocalizationFiles (Action function = null)
    {
        // Get all json FileInfos from Locales folder
        DirectoryInfo dir = new DirectoryInfo (Path.Combine (Application.streamingAssetsPath, "Locales"));
        FileInfo [] files = dir.GetFiles ("*.json");

        loadedLocalizationData = new LocalizationData [files.Length];

        // Convert all json files to LocalizedData
        for (int i = 0; i < files.Length; i++)
        {
            string dataAsJson = File.ReadAllText (files [i].FullName);

            // Add data converted from json to array 
            loadedLocalizationData [i] = JsonUtility.FromJson<LocalizationData> (dataAsJson);
        }

        // if there is no Localization Data selected, assign first in the array
        if (localizedText == null)
            RefreshLocalizedText (0);

        if (function != null)
            function ();
    }

    // Convert selected LocalizationData to Dictionary and refresh all LocalizedText instances that are subscribed for refresh
    public void RefreshLocalizedText (int index)
    {
        // Wipe Dictionaries 
        localizedText = new Dictionary<string, string> ();
        localizedDialogue = new Dictionary<string, DialogueData []> ();

        // Convert LocalizationData to Dictionaries
        foreach (LocalizationItem item in loadedLocalizationData [index].items)
        {
            localizedText.Add (item.key, item.value);
        }
        foreach (LocalizationDialogue item in loadedLocalizationData [index].dialogues)
        {
            localizedDialogue.Add (item.key, item.items);
        }

        // Refresh subscribed LocalizationText instances
        if (refreshCallback != null)
            refreshCallback ();
    }

    // Pass display function for Localized Text to refresh every time localized Dictionary changes
    public static void SubscribeToRefresh (Action function)
    {
        // Add the display function if it isn't allready part of Callback
        if (refreshCallback == refreshCallback - function)
            refreshCallback += function;
        
        // Check if there is an LocalizationManager instance and if the dictionary containing localized text is not empty, and call the passed function
        if (instance && instance.localizedText != null)
            function ();
    }

    // Return localized string from dictionary, if it doesn't contain the key return missing string
    public string GetLocalizedValue (string key)
    {
        return localizedText.ContainsKey (key) ? localizedText [key] : missingTextString;
    }

    // Return localized DialogueData from dictionary, if it doesn't contain the key return null
    public DialogueData [] GetLocalizedDialogue (string key)
    {
        return localizedDialogue.ContainsKey (key) ? localizedDialogue [key] : null;
    }

    // Return title names of all json localization data 
    public List<string> GetLocalizationDataNames ()
    {
        List<string> names = new List<string> ();

        // Go trough all loaded localization data
        for (int i = 0; i < loadedLocalizationData.Length; i++)
        {
            // Look for localizatio item with key "name" and add the value to the list
            foreach (LocalizationItem item in loadedLocalizationData [i].items)
            {
                if (item.key == "name")
                {
                    names.Add (item.value);
                    break;
                }
            }
        }
        return names;
    }
}
