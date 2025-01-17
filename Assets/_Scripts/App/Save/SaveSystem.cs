using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;
//using UnityEngine.ProBuilder.Shapes;

public static class SaveSystem
{
    private static CloudStorage _cloudStorage = new CloudStorage();
    private static string DesignFilesListKey = "saved_design_files";
    private static string CustomizeFilesListKey = "saved_customize_files";
    private static string RatingFilesListKey = "saved_rating_files";

    private static string currentFile;
    private static string _lastLobbyName;

    public static bool _loaded = false;

    public static string LastLobbyName { set { _lastLobbyName = value; } }

    public static async Task LoadDesignLobbiesFromCloudAsync()
    {
        // Load all saved file paths from the cloud (Design files)
        string allFilePaths = await _cloudStorage.LoadDataAsync(DesignFilesListKey);

        if (string.IsNullOrEmpty(allFilePaths))
        {
            Debug.LogWarning("No design files found in cloud save.");
            return;
        }

        foreach (string filePath in allFilePaths.Split('\n'))
        {
            if (string.IsNullOrEmpty(filePath)) continue;

            Debug.Log("Filepath: " + filePath);
            // Extract the lobby name from the path
            string lobbyName = ExtractLobbyNameFromPath(filePath);

            Debug.Log("Lobby Name: " + lobbyName);

            if (!string.IsNullOrEmpty(lobbyName))
            {
                // Set the lobby name in the UI and create the lobby
                Debug.Log("Setting Lobby Name: ");
                LobbyListUI.Instance.lobbyName = lobbyName;

                Debug.Log("Lobby Name was set: ");
                Debug.Log($"Creating lobby: {lobbyName} with type: {LobbyListUI.Instance.GetSessionMode()}");
                LobbyListUI.Instance.CreateCustomizeLobbyButton(lobbyName);
            }
        }

        Debug.Log("All design lobbies loaded.");
    }
 
    public static async Task SaveCustomizeDesignAsync(List<RoomData> roomDataList, string playerName, int aptID, string aptName, int[] choices)
    {
        await SaveRoomInfoAsync(playerName, aptID, aptName, choices);
        await SaveRoomDataListAsync(roomDataList, playerName, aptName);
        await SaveCustomizeFileName(playerName, aptName);
    }
    public static async Task SaveCustomizeFileName(string playerName, string aptName)
    {
        string fileName = LobbyManager.Instance.GetLobbbyName();
        string key = $"{fileName}_{aptName}_{playerName}";
        key = SanitizeKey(key);


        await AddFileNameToCustomizeFilesListAsync(key);
    }

    public static async Task SaveRoomInfoAsync(string playerName, int aptId, string aptName, int[] choices)
    {
        RoomInfo roomInfo = new RoomInfo(aptId, choices[0], choices[1]);
        string fileName = LobbyManager.Instance.GetLobbbyName();
        string key = $"{fileName}_{aptName}_{playerName}_RoomInfo";
        key=SanitizeKey(key);        

        string roomInfoJson = JsonUtility.ToJson(roomInfo);
        await _cloudStorage.SaveDataAsync(key, roomInfoJson);

        currentFile = key;
    }

    public static async Task SaveRoomDataListAsync(List<RoomData> roomDataList, string playerName, string aptName)
    {
        string fileName = LobbyManager.Instance.GetLobbbyName();
        string key = $"{fileName}_{aptName}_{playerName}_RoomData";
        key = SanitizeKey(key);

        string roomDataListJson = JsonUtility.ToJson(new RoomDataListWrapper(roomDataList));
        await _cloudStorage.SaveDataAsync(key, roomDataListJson);

        currentFile = key;
    }

    public static async Task SaveDesignRatingAsync(string playerName, string aptName, int[] ratings)
    {
        RatingInfo ratingInfo = new RatingInfo(aptName, ratings);
        string fileName = LobbyManager.Instance.GetLobbbyName();
        string key = $"{fileName}_{aptName}_{playerName}_Ratings";
        key = SanitizeKey(key);

        string ratingInfoJson = JsonUtility.ToJson(ratingInfo);
        await _cloudStorage.SaveDataAsync(key, ratingInfoJson);

        currentFile = key;

        AddFileNameToRatingFilesListAsync(key);

    }
    public static string SanitizeKey(string key)
    {
        // Remove spaces and replace with underscores
        key = key.Replace(" ", "_");

        // Replace any special characters with empty strings or underscores
        key = new string(key.Where(c => Char.IsLetterOrDigit(c) || c == '_').ToArray());

        // Ensure the key does not exceed 255 characters
        if (key.Length > 255)
        {
            key = key.Substring(0, 255);
        }

        return key;
    }
    public static async Task<List<RoomData>> LoadRoomDataFromCloudAsync(string key)
    {
        string json = await _cloudStorage.LoadDataAsync(key);
        if (string.IsNullOrEmpty(json)) return null;

        RoomDataListWrapper wrapper = JsonUtility.FromJson<RoomDataListWrapper>(json);
        return wrapper?.RoomDataList;
    }

    public static async Task<RoomInfo> LoadRoomInfoFromCloudAsync(string key)
    {
        string json = await _cloudStorage.LoadDataAsync(key);
        if (string.IsNullOrEmpty(json)) return null;

        return JsonUtility.FromJson<RoomInfo>(json);
    }

    public static async Task<List<(RoomInfo, List<RoomData>)>> LoadRoomTuplesFromCloudAsync(string selectedModule)
    {
        List<(RoomInfo, List<RoomData>)> roomTuples = new List<(RoomInfo, List<RoomData>)>();

        // Read the file list from the cloud
        string allFiles = await _cloudStorage.LoadDataAsync(CustomizeFilesListKey) ?? "";

        // Get the last lobby name, sanitize it, and remove spaces (convert them to underscores)
        string lastLobbyName = _lastLobbyName.Replace(" ", "_");
        lastLobbyName = new string(lastLobbyName.Where(c => Char.IsLetterOrDigit(c) || c == '_').ToArray());

        // Sanitize the selected module name as well
        string sanitizedSelectedModule = new string(selectedModule.Where(c => Char.IsLetterOrDigit(c) || c == '_').ToArray());

        // Split the list into individual file names (keys)
        string[] fileNames = allFiles.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string fileName in fileNames)
        {
            try
            {
                Debug.Log("FILENAME "+fileName);
                Debug.Log("LAST LOBBY NAME" + lastLobbyName+" SELECTED MODULE "+ sanitizedSelectedModule);

                // Only process files that contain both the sanitized lastLobbyName and sanitized selectedModule string
                if (fileName.Contains(lastLobbyName) && fileName.Contains(sanitizedSelectedModule))
                {
                    // Construct cloud keys for the room info and room data
                    string roomInfoKey = $"{fileName}_RoomInfo";
                    string roomDataKey = $"{fileName}_RoomData";

                    // Load RoomInfo and RoomData from Cloud
                    RoomInfo currRoomInfo = await LoadRoomInfoFromCloudAsync(roomInfoKey);
                    List<RoomData> currRoomData = await LoadRoomDataFromCloudAsync(roomDataKey);

                    // If both are successfully loaded, add them as a tuple
                    if (currRoomInfo != null && currRoomData != null)
                    {
                        roomTuples.Add((currRoomInfo, currRoomData));
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to load data for file: {fileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading room tuple from cloud for file {fileName}: {ex.Message}");
            }
        }

        // Return the list of tuples
        return roomTuples;
    }

    public static async Task SaveDesignFileAsync()
    {

        string fileName = LobbyManager.Instance.GetLobbbyName();
        currentFile = $"{fileName}/Design";
        await AddFileNameToDesignFilesListAsync(currentFile);
    }

    private static async Task AddFileNameToCustomizeFilesListAsync(string fileName)
    {
        // Load the existing list of file names from the cloud
        string allFiles = await _cloudStorage.LoadDataAsync(CustomizeFilesListKey) ?? "";

        // Check if the fileName is not already in the list
        if (!allFiles.Contains(fileName))
        {
            // Append the new fileName to the list
            allFiles += fileName + "\n";

            // Save the updated list back to the cloud
            await _cloudStorage.SaveDataAsync(CustomizeFilesListKey, allFiles);
        }
    }
    private static async Task AddFileNameToRatingFilesListAsync(string fileName)
    {
        // Load the existing list of file names from the cloud
        string allFiles = await _cloudStorage.LoadDataAsync(CustomizeFilesListKey) ?? "";

        // Check if the fileName is not already in the list
        if (!allFiles.Contains(fileName))
        {
            // Append the new fileName to the list
            allFiles += fileName + "\n";

            // Save the updated list back to the cloud
            await _cloudStorage.SaveDataAsync(RatingFilesListKey, allFiles);
        }
    }
    private static async Task AddFileNameToDesignFilesListAsync(string fileName)
    {
        string allFiles = await _cloudStorage.LoadDataAsync(DesignFilesListKey) ?? "";
        if (!allFiles.Contains(fileName))
        {
            allFiles += fileName + "\n";
            await _cloudStorage.SaveDataAsync(DesignFilesListKey, allFiles);
        }
    }

private static string ExtractLobbyNameFromPath(string filePath)
    {
        try
        {
            // Split the file path using both space (' ') and underscore ('_') characters as delimiters
            string[] parts = filePath.Split(new char[] { ' ', '_', '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Check if the first part starts with "Lobby"
            if (parts.Length > 1 && parts[0].StartsWith("Lobby", StringComparison.OrdinalIgnoreCase))
            {
                // Return the lobby part and the next word (if available)
                return parts[0] + "_" + parts[1]; // Combine the "Lobby" part with the next word
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error extracting lobby name: {e.Message}");
        }

        // Return null if no valid lobby name found
        return null;
    }
    public static async Task SaveAllModulesAsync(List<ModuleData> moduleList)
    {
        if (moduleList == null || moduleList.Count == 0)
        {
            Debug.LogError("No modules to save.");
            return;
        }

        string moduleListJson = JsonUtility.ToJson(new ModuleListWrapper(moduleList));
        await _cloudStorage.SaveDataAsync("saved_modules", moduleListJson);

        Debug.Log($"All modules saved. Total: {moduleList.Count}");
    }

    private static async Task AddModuleKeyToSavedModulesListAsync(string moduleKey)
    {
        string allModuleKeys = await _cloudStorage.LoadDataAsync("saved_modules_list") ?? "";
        if (!allModuleKeys.Contains(moduleKey))
        {
            allModuleKeys += moduleKey + "\n";
            await _cloudStorage.SaveDataAsync("saved_modules_list", allModuleKeys);
        }
    }

    public static async Task<List<ModuleData>> LoadAllModulesAsync()
    {
        string modulesJson = await _cloudStorage.LoadDataAsync("saved_modules");
        if (string.IsNullOrEmpty(modulesJson))
        {
            Debug.LogWarning("No modules found in cloud save.");
            return new List<ModuleData>();
        }

        ModuleListWrapper wrapper = JsonUtility.FromJson<ModuleListWrapper>(modulesJson);
        if (wrapper == null || wrapper.ModuleDataList == null)
        {
            Debug.LogWarning("Failed to load module data.");
            return new List<ModuleData>();
        }

        Debug.Log($"Loaded {wrapper.ModuleDataList.Count} modules.");
        return wrapper.ModuleDataList;
    }

    public static async Task LoadCustomizeLobbiesFromCloudAsync()
    {
        // Load all saved file paths from the cloud (Customize files)
        string allFilePaths = await _cloudStorage.LoadDataAsync(CustomizeFilesListKey);

        if (string.IsNullOrEmpty(allFilePaths))
        {
            Debug.LogWarning("No customize files found in cloud save.");
            return;
        }

        foreach (string filePath in allFilePaths.Split('\n'))
        {
            if (string.IsNullOrEmpty(filePath)) continue;

            Debug.Log("Filepath: " + filePath);
            // Extract the lobby name from the path
            string lobbyName = ExtractLobbyNameFromPath(filePath);

            Debug.Log("Lobby Name: " + lobbyName);

            if (!string.IsNullOrEmpty(lobbyName))
            {
                // Set the lobby name in the UI and create the lobby
                Debug.Log("Setting Lobby Name: ");
                LobbyListUI.Instance.lobbyName = lobbyName;

                Debug.Log("Lobby Name was set: ");
                Debug.Log($"Creating lobby: {lobbyName} with type: {LobbyListUI.Instance.GetSessionMode()}");
                LobbyListUI.Instance.CreateCustomizeLobbyButton(lobbyName);
            }
        }

        Debug.Log("All customize lobbies loaded.");
    }
}

[Serializable]
public class RoomDataListWrapper
{
    public List<RoomData> RoomDataList;

    public RoomDataListWrapper(List<RoomData> roomDataList)
    {
        RoomDataList = roomDataList;
    }
}

[Serializable]
public class ModuleListWrapper
{
    public List<ModuleData> ModuleDataList;

    public ModuleListWrapper(List<ModuleData> moduleDataList)
    {
        ModuleDataList = moduleDataList;
    }
}
