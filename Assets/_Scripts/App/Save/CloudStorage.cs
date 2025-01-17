using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

public class CloudStorage
{
    // Save data to the cloud
    public async Task SaveDataAsync(string key, string value)
    {
        try
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { key, value }
            };

            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
            Debug.Log($"Data saved successfully: {key} = {value}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    // Load data from the cloud
    public async Task<string> LoadDataAsync(string key)
    {
        try
        {
            var data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { key });
            if (data.ContainsKey(key))
            {
                string value = data[key] as string;
                Debug.Log($"Data loaded successfully: {key} = {value}");
                return value;
            }
            else
            {
                Debug.LogWarning($"Key '{key}' not found in cloud data.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data: {e.Message}");
            return null;
        }
    }
}


