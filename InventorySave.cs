using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;
using System.IO;


public class InventorySave
{
    public static void Save()
    {
        string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "Inventory";
        //Create Inventory save folder if it doesn't exist
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        //Create data string from current inventory and save it to a file
        if (InventoryData.storedObjects == null || (InventoryData.storedObjects != null && InventoryData.storedObjects.Count == 0))
        {
            return;
        }
        string save = path + Path.DirectorySeparatorChar + "Inventory.txt";
        string data = InventoryData.SaveString();
        File.WriteAllText(save, data);
        Debug.Log("Saving Inventory");
    }

    public static void Load()
    {
        string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "Inventory";
        //Create Inventory save folder if it doesn't exist
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        //Load data string from file and load it into InventoryData
        string save = path + Path.DirectorySeparatorChar + "Inventory.txt";
        if (File.Exists(save))
        {
            string data = File.ReadAllText(save);
            InventoryData.LoadString(data);
            Debug.Log("Loading Inventory");
        }
    }
}

