using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;
using System.IO;


public class InventorySave
{
    public static void Save(int saveSlot, int slugcat)
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
        //Get current save slot
        string save = path + Path.DirectorySeparatorChar + "Inventory" + saveSlot.ToString() + slugcat.ToString() + ".txt";
        string data = InventoryData.SaveString();
        File.WriteAllText(save, data);
        Debug.Log("Saving Inventory");
    }

    public static void Load(int saveSlot, int slugcat)
    {
        string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "Inventory";
        //Create Inventory save folder if it doesn't exist
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        //Load data string from file and load it into InventoryData
        string save = path + Path.DirectorySeparatorChar + "Inventory" + saveSlot.ToString() + slugcat.ToString()+ ".txt";
        if (File.Exists(save))
        {
            string data = File.ReadAllText(save);
            InventoryData.LoadString(data);
            Debug.Log("Loading Inventory");
        }
        //No saved inventory, wipe contents
        else
        {
            InventoryData.storedObjects = new List<InventoryData.StoredObject>();
        }
    }
}

