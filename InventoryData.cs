using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;


public static class InventoryData
{
    //Inventory style
    public enum InventoryType
    {
        Grid,
        Cycle
    }
    //Number of available inventory slots
    public static IntVector2 invSize = new IntVector2(4, 2);
    //Inventory type
    public static InventoryType inventoryType = InventoryType.Grid;
    //Array of stored objects
    public static List<StoredObject> storedObjects;
    public static string test = "";

    public static StoredObject NewStoredObject(AbstractPhysicalObject apo, AbstractCreature crit, int index)
    {
        if (storedObjects == null)
        {
            storedObjects = new List<StoredObject>();
            Debug.Log("New StoredObject list created");
        }
        Debug.Log("Current list length: " + storedObjects.Count);
        for (int i = 0; i < storedObjects.Count; i++)
        {
            if (storedObjects[i].index == index)
            {
                Debug.Log("An object already exists with index " + index);
                return storedObjects[i];
            }
        }
        storedObjects.Add(new StoredObject(apo, crit, index));
        for (int i = 0; i < storedObjects.Count; i++)
        {
            if (storedObjects[i].index == index)
            {
                Debug.Log("New stored object created with index " + index);
                return storedObjects[i];
            }
        }
        Debug.Log("Error creating or obtaining stored object with index " + index);
        return null;
    }

    public static void RemoveStoredObject(int index)
    {
        int listIndex = -1;
        for (int i = 0; i < storedObjects.Count; i++)
        {
            if (storedObjects[i].index == index)
            {
                listIndex = i;
                storedObjects[i] = null;
            }
        }
        storedObjects.RemoveAt(listIndex);
        Debug.Log("Removed object from inventory at index " + index);
    }

    //Full inventory data as string for saving
    public static string SaveString()
    {
        //Get stored object data
        string objectData = "";
        for (int i = 0; i < storedObjects.Count; i++)
        {
            objectData += "<SObj>";
            objectData += storedObjects[i].ToDataString();
        }
        int invType = (int)inventoryType;
        return string.Concat(new string[]
        {
                //Inventory type
                invType.ToString(),
                "<V>",
                //Inventory Width
                invSize.x.ToString(),
                "<V>",
                //Inventory Height
                invSize.y.ToString(),
                "<V>",
                //Stored Items
                objectData,
        });
    }

    public static void LoadString(string data)
    {
        string[] invData = Regex.Split(data, "<V>");
        //0 - Inventory type
        inventoryType = (InventoryType)int.Parse(invData[0]);
        //1 - Inventory width
        invSize.x = int.Parse(invData[1]);
        //2 - Inventory height
        invSize.y = int.Parse(invData[2]);
        string[] objectData = Regex.Split(invData[3], "<SObj>");
        storedObjects = new List<StoredObject>();
        //Loop starts at 1 because 0 is empty after the split
        for (int i = 1; i < objectData.Length; i++)
        {
            storedObjects.Add(GenerateStoredObjectFromString(objectData[i]));
        }
    }

    public static StoredObject GenerateStoredObjectFromString(string data)
    {
        StoredObject SObj = new StoredObject(null, null, -1);
        string[] objData = Regex.Split(data, "<X>");
        for (int i = 0; i < objData.Length; i++)
        {
            Debug.Log(objData[i]);
        }
        //0 - type of item
        SObj.type = (AbstractPhysicalObject.AbstractObjectType)int.Parse(objData[0]);
        Debug.Log(SObj.type.ToString());
        //1 - type of creature
        SObj.critType = (CreatureTemplate.Type)int.Parse(objData[1]);
        //2 - index
        SObj.index = int.Parse(objData[2]);
        //3 - sprite name
        SObj.spriteName = objData[3];
        //4 - red value
        SObj.spriteColor.r = float.Parse(objData[4]);
        //5 - green value
        SObj.spriteColor.g = float.Parse(objData[5]);
        //6 - blue value
        SObj.spriteColor.b = float.Parse(objData[6]);
        SObj.spriteColor.a = 1f;
        //7 - object data
        SObj.data = objData[7];
        return SObj;
    }

    public static string ObjectSprite(int index)
    {
        for (int i = 0; i < storedObjects.Count; i++)
        {
            if (storedObjects[i].index == index)
            {
                return storedObjects[i].spriteName;
            }
        }
        return "Futile_White";
    }

    public static Color ObjectColor(int index)
    {
        for (int i = 0; i < storedObjects.Count; i++)
        {
            if (storedObjects[i].index == index)
            {
                return storedObjects[i].spriteColor;
            }
        }
        return new Color(0f, 1f, 0f);
    }

    public class StoredObject
    {
        //Type
        public AbstractPhysicalObject.AbstractObjectType type;
        public CreatureTemplate.Type critType;
        //Data
        public string data;
        public int weight;
        public int index;
        //Sprite
        public string spriteName;
        public Color spriteColor;

        public StoredObject(AbstractPhysicalObject apo, AbstractCreature crit, int index)
        {
            this.index = index;
            //Stored object is an item
            if (apo != null)
            {
                this.type = apo.type;
                this.data = apo.ToString();
                ItemSymbol.IconSymbolData symbol = (ItemSymbol.IconSymbolData)ItemSymbol.SymbolDataFromItem(apo);
                this.spriteName = ItemSymbol.SpriteNameForItem(this.type, symbol.intData);
                this.spriteColor = ItemSymbol.ColorForItem(this.type, symbol.intData);
            }
            //Stored object is a creature
            if (crit != null)
            {
                this.type = crit.type;
                this.critType = crit.creatureTemplate.type;
                this.data = SaveState.AbstractCreatureToString(crit);
                CreatureSymbol.IconSymbolData creature = CreatureSymbol.SymbolDataFromCreature(crit);
                this.spriteName = CreatureSymbol.SpriteNameOfCreature(creature);
                this.spriteColor = CreatureSymbol.ColorOfCreature(creature);
            }
        }

        public string ToDataString()
        {
            int type = (int)this.type;
            int critType = (int)this.critType;
            return string.Concat(new string[]
            {
                    //0 Item Type
                    type.ToString(),
                    "<X>",
                    //1 Creature Type
                    critType.ToString(),
                    "<X>",
                    //2 Index of object in inventory
                    index.ToString(),
                    "<X>",
                    //3 Name of item sprite
                    spriteName,
                    "<X>",
                    //4 Red
                    spriteColor.r.ToString(),
                    "<X>",
                    //5 Green
                    spriteColor.g.ToString(),
                    "<X>",
                    //5 Blue
                    spriteColor.b.ToString(),
                    "<X>",
                    //6 Item or Creature Data
                    data
            });
        }
    }
}

