using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;
using System.IO;


public class InventorySave
{
    public static void SaveHooks()
    {
        On.PlayerProgression.WipeAll += PlayerProgression_WipeAll;
        On.PlayerProgression.WipeSaveState += PlayerProgression_WipeSaveState;
    }

    private static void PlayerProgression_WipeSaveState(On.PlayerProgression.orig_WipeSaveState orig, PlayerProgression self, int saveStateNumber)
    {
        orig.Invoke(self, saveStateNumber);
        WipeSave(self.rainWorld.options.saveSlot, saveStateNumber);
    }

    private static void PlayerProgression_WipeAll(On.PlayerProgression.orig_WipeAll orig, PlayerProgression self)
    {
        orig.Invoke(self);
        WipeAll(self.rainWorld.options.saveSlot);
    }

    public static void WipeAll(int saveSlot)
    {
        string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "Inventory";
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                if(files[i].StartsWith(path + Path.DirectorySeparatorChar + "Inventory" + saveSlot))
                {
                    File.Delete(files[i]);
                }
            }
        }
    }
    public static void WipeSave(int saveSlot, int slugcat)
    {
        string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "Inventory";
        if (Directory.Exists(path))
        {
            string save = path + Path.DirectorySeparatorChar + "Inventory" + saveSlot.ToString() + slugcat.ToString() + ".txt";
            if (File.Exists(save))
            {
                File.Delete(save);
            }
        }
    }

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
            InventoryData.invSize = InventoryConfig.invSize;
        }
    }
}

