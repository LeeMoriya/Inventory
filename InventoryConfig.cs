using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu.Remix.MixedUI;
using Menu.Remix;
using UnityEngine;
using RWCustom;
using System.IO;
using System.Text.RegularExpressions;
using static InventoryData;

public class InventoryConfig : OptionInterface
{
    //Configuration
    public static IntVector2 invSize = new IntVector2(3, 2);

    public static Configurable<bool> critBool;
    public static Configurable<bool> karmaBool;
    public static Configurable<bool> slowBool;
    public static Configurable<bool> rainbowBool;
    public static Configurable<Color> cursorColorConfig;
    public static Configurable<int> widthConfig;
    public static Configurable<int> heightConfig;
    public static Configurable<string> saveName;
    public static Configurable<KeyCode> invKey1, invKey2, invKey3, invKey4;

    //Preview Window
    public OpRect previewWindow;
    public OpLabel windowLabel;
    public OpRect[] previewSlots;
    public OpLabel sizeLabel;
    public OpLabel totalLabel;
    public OpLabel descLabel;

    //Sliders
    public OpSlider widthSlider;
    public OpSlider heightSlider;

    //Checkboxes
    public OpLabel settingsLabel;
    public OpLabel settingsDesc;
    public OpCheckBox creatureCheck;
    public OpCheckBox karmaCheck;
    public OpCheckBox slowCheck;
    public OpCheckBox rainbowCheck;
    public OpLabel creatureLabel;
    public OpLabel karmaLabel;
    public OpLabel slowLabel;
    public OpLabel rainbowLabel;

    //Color Picker
    public OpColorPicker cursorColor;
    public OpLabel colorLabel;

    //Mod Info
    public OpLabel infoLabel;

    //Save switcher
    public OpComboBox saveSwitcher;
    public IntVector2 defaultInvSize = new IntVector2(3, 2);
    public int itemsStored;
    public OpLabel sizeWarning;
    public string currentSave = "Default";
    public Dictionary<string,string> slugcatNames = new Dictionary<string,string>();

    //Keybind
    public OpKeyBinder keyBinder1, keyBinder2, keyBinder3, keyBinder4;

    public int resetCounter;

    public InventoryConfig()
    {
        critBool = config.Bind<bool>("critBool", false, new ConfigurableInfo("Allows you to store any creature, living or dead that Slugcat can grab", null));
        karmaBool = config.Bind<bool>("karmaBool", false, new ConfigurableInfo("Allows you to store Karma Flowers in the inventory - extremely unbalanced", null));
        slowBool = config.Bind<bool>("slowBool", false, new ConfigurableInfo("Slows down time when the inventory is open", null));
        rainbowBool = config.Bind<bool>("rainbowBool", false, new ConfigurableInfo("Changes the color of the inventory cursor to a rainbow - this setting overrides custom cursor color", null));
        cursorColorConfig = config.Bind<Color>("cursorColorConfig", new Color(1f, 1f, 1f), new ConfigurableInfo("Set a custom color for the inventory cursor", null));
        widthConfig = config.Bind<int>("invWidth", 3, new ConfigAcceptableRange<int>(1, 8));
        heightConfig = config.Bind<int>("invHeight", 2, new ConfigAcceptableRange<int>(1, 4));
        saveName = config.Bind<string>("saveName", "Default");
        invKey1 = config.Bind<KeyCode>("keyBind1", KeyCode.None);
        invKey2 = config.Bind<KeyCode>("keyBind2", KeyCode.None);
        invKey3 = config.Bind<KeyCode>("keyBind3", KeyCode.None);
        invKey4 = config.Bind<KeyCode>("keyBind4", KeyCode.None);

        invSize = new IntVector2(widthConfig.Value, heightConfig.Value);
        defaultInvSize = invSize;
    }

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[1];
        Tabs[0] = new OpTab(this, "Config");

        //Preview Window
        previewWindow = new OpRect(new Vector2(40f, 325f), new Vector2(525f, 225f), 0.3f);
        windowLabel = new OpLabel(46f, 555f, "Inventory Size", true);
        previewSlots = new OpRect[32];
        for (int i = 0; i < previewSlots.Length; i++)
        {
            previewSlots[i] = new OpRect(new Vector2(0f, 0f), new Vector2(30f, 30f), 0.3f);
            Tabs[0].AddItems(previewSlots[i]);
        }
        sizeLabel = new OpLabel(50f, 330f, "Size", false);
        totalLabel = new OpLabel(new Vector2(510f, 330f), new Vector2(), "Total Slots", FLabelAlignment.Right, false);

        List<ListItem> existingSaves = ReturnExistingSaves();
        saveSwitcher = new OpComboBox(new Configurable<string>("Default"), new Vector2(412f, 557f), 150f, existingSaves);
        saveSwitcher.description = "Select slugcats with existing inventories to resize them";
        saveSwitcher.OnValueChanged += SaveSwitcher_OnValueChanged;
        saveSwitcher.OnListClose += SaveSwitcher_OnListClose;
        sizeWarning = new OpLabel(300f, 355f, "Cannot make inventory smaller than number of items stored!");
        sizeWarning.color = new Color(1f, 0.1f, 0.1f);
        sizeWarning.label.alignment = FLabelAlignment.Center;
        sizeWarning.alpha = 0f;

        Tabs[0].AddItems(previewWindow, windowLabel, sizeLabel, totalLabel, saveSwitcher, sizeWarning);

        //Sliders
        widthSlider = new OpSlider(widthConfig, new Vector2(45f, 285f), 72f, false);
        widthSlider.OnValueChanged += WidthSlider_OnValueChanged;
        widthSlider.OnValueUpdate += WidthSlider_OnValueUpdate;
        heightSlider = new OpSlider(heightConfig, new Vector2(0f, 325f), 70f, true);
        heightSlider.OnValueChanged += HeightSlider_OnValueChanged;
        heightSlider.OnValueUpdate += HeightSlider_OnValueUpdate;
        Tabs[0].AddItems(widthSlider, heightSlider);

        //Checkboxes
        settingsLabel = new OpLabel(45f, 250f, "Additional Settings", true);
        descLabel = new OpLabel(45f, 230f, "Hover over each option for more information", false);

        creatureCheck = new OpCheckBox(critBool ,new Vector2(45f, 190f));
        creatureCheck.description = "Allows you to store any creature, living or dead that Slugcat can grab";
        creatureLabel = new OpLabel(75f, 192f, "Creature Storage", false);

        karmaCheck = new OpCheckBox(karmaBool, new Vector2(45f, 150f));
        karmaCheck.description = "Allows you to store Karma Flowers in the inventory - extremely unbalanced";
        karmaLabel = new OpLabel(75f, 152f, "Karma Flower Storage", false);

        slowCheck = new OpCheckBox(slowBool, new Vector2(45f, 110f));
        slowCheck.description = "Slows down time when the inventory is open";
        slowLabel = new OpLabel(75f, 112f, "Slow Motion Inventory", false);

        rainbowCheck = new OpCheckBox(rainbowBool, new Vector2(45f, 70f));
        rainbowCheck.description = "Changes the color of the inventory cursor to a rainbow - this setting overrides custom cursor color";
        rainbowLabel = new OpLabel(75f, 72f, "Rainbow Cursor", false);
        Tabs[0].AddItems(settingsLabel, descLabel, creatureCheck, karmaCheck, creatureLabel, karmaLabel, slowCheck, slowLabel, rainbowCheck, rainbowLabel);

        //Color Picker
        cursorColor = new OpColorPicker(cursorColorConfig, new Vector2(400f, 70f));
        colorLabel = new OpLabel(404f, 230f, "Pick a custom cursor color", false);
        Tabs[0].AddItems(cursorColor, colorLabel);

        //Keypicker
        OpLabel keyBindLabel = new OpLabel(300f, 40f, "Custom inventory keybinds for players 1-4", false);
        keyBindLabel.label.alignment = FLabelAlignment.Center;
        keyBinder1 = new OpKeyBinder(invKey1, new Vector2(110f, 0f), new Vector2(80f, 25f), true, OpKeyBinder.BindController.AnyController);
        keyBinder1.description = "Set a custom keybind to open the inventory for Player 1, defaults to MAP if nothing is assigned";
        keyBinder2 = new OpKeyBinder(invKey2, new Vector2(210f, 0f), new Vector2(80f, 25f), true, OpKeyBinder.BindController.AnyController);
        keyBinder2.description = "Set a custom keybind to open the inventory for Player 2, defaults to MAP if nothing is assigned";
        keyBinder3 = new OpKeyBinder(invKey3, new Vector2(310f, 0f), new Vector2(80f, 25f), true, OpKeyBinder.BindController.AnyController);
        keyBinder3.description = "Set a custom keybind to open the inventory for Player 3, defaults to MAP if nothing is assigned";
        keyBinder4 = new OpKeyBinder(invKey4, new Vector2(410f, 0f), new Vector2(80f, 25f), true, OpKeyBinder.BindController.AnyController);
        keyBinder4.description = "Set a custom keybind to open the inventory for Player 4, defaults to MAP if nothing is assigned";


        Tabs[0].AddItems(keyBinder1, keyBinder2, keyBinder3, keyBinder4, keyBindLabel);

        OnConfigChanged += InventoryConfig_OnConfigChanged;
        OnConfigReset += InventoryConfig_OnConfigReset;
    }

    private void SaveSwitcher_OnListClose(UIfocusable trigger)
    {
        currentSave = saveSwitcher.value;
    }

    private void HeightSlider_OnValueUpdate(UIconfig config, string value, string oldValue)
    {
        ConfigContainer.instance._history.Push(new ConfigContainer.ConfigHistory());
    }

    private void WidthSlider_OnValueUpdate(UIconfig config, string value, string oldValue)
    {
        ConfigContainer.instance._history.Push(new ConfigContainer.ConfigHistory());
    }

    private void SaveSwitcher_OnValueChanged(UIconfig config, string value, string oldValue)
    {
        LoadAndDisplay(value);
    }

    private void InventoryConfig_OnConfigReset()
    {
        for (int i = 0; i < previewSlots.Length; i++)
        {
            previewSlots[i].Hide();
        }
        resetCounter = 10;
    }

    private void HeightSlider_OnValueChanged(UIconfig config, string value, string oldValue)
    {
        if(saveSwitcher.value != "Default")
        {
            int width = int.Parse(widthSlider.value);
            int height = int.Parse(heightSlider.value);

            while(width * height < itemsStored)
            {
                height++;
                heightSlider.value = height.ToString();
                sizeWarning.alpha = 3f;
            }
            invSize = new IntVector2(width, height);
        }
        else
        {
            invSize = new IntVector2(int.Parse(widthSlider.value), int.Parse(heightSlider.value));
            defaultInvSize = invSize;
        }
    }

    private void WidthSlider_OnValueChanged(UIconfig config, string value, string oldValue)
    {
        if (saveSwitcher.value != "Default")
        {
            int width = int.Parse(widthSlider.value);
            int height = int.Parse(heightSlider.value);

            while (width * height < itemsStored)
            {
                width++;
                widthSlider.value = width.ToString();
                sizeWarning.alpha = 3f;
            }
            invSize = new IntVector2(width, height);
        }
        else
        {
            invSize = new IntVector2(int.Parse(widthSlider.value), int.Parse(heightSlider.value));
            defaultInvSize = invSize;
        }
    }

    private void InventoryConfig_OnConfigChanged()
    {
        invSize = new IntVector2(widthConfig.Value, heightConfig.Value);
        //A slugcat with an existing inventory is selected, update its size
        if(currentSave != "Default")
        {
            ModifyInventorySize(currentSave);
            currentSave = "Default";
        }
    }

    public override void Update()
    {
        base.Update();
        int totalSlots = invSize.x * invSize.y;
        float xAnchor = Mathf.Lerp(290f, 125f, Mathf.InverseLerp(1, 10, invSize.x));
        float yAnchor = Mathf.Lerp(425f, 360f, Mathf.InverseLerp(1, 5, invSize.y));
        float offset = 35f;
        int xPos = 0;
        int yPos = 0;

        if (resetCounter <= 0)
        {
            for (int i = 0; i < previewSlots.Length; i++)
            {
                if (i < totalSlots)
                {
                    previewSlots[i].SetPos(new Vector2(xAnchor + (offset * xPos), yAnchor + (offset * yPos)));
                    previewSlots[i].Show();

                    xPos++;
                    if (xPos == invSize.x)
                    {
                        xPos = 0;
                        yPos++;
                    }
                }
                else
                {
                    previewSlots[i].Hide();
                    previewSlots[i].SetPos(new Vector2(300f, 20000f));
                }
            }
        }
        else
        {
            resetCounter--;
        }

        sizeLabel.text = "Width: " + invSize.x + " | Height: " + invSize.y;
        totalLabel.text = "Total Slots: " + (invSize.x * invSize.y).ToString();

        if (rainbowBool.Value)
        {
            cursorColor.greyedOut = true;
        }
        else
        {
            cursorColor.greyedOut = false;
        }

        sizeWarning.alpha -= 0.02f;
        sizeWarning.alpha = Mathf.Clamp(sizeWarning.alpha, 0f, 3f);
    }

    public List<ListItem> ReturnExistingSaves()
    {
        List<ListItem> saves = new List<ListItem>()
        {
            new ListItem("Default")
        };

        int slotNum = Custom.rainWorld.options.saveSlot;
        string savePath = $"{Application.persistentDataPath}\\Inventory";
        if (Directory.Exists(savePath))
        {
            string[] fileList = Directory.GetFiles(savePath);
            for (int i = 0; i < fileList.Length; i++)
            {
                //This file matches the current save slot
                if (fileList[i].Contains(slotNum.ToString()) && fileList[i].EndsWith(".txt"))
                {
                    string slugcat = Regex.Split(fileList[i], slotNum.ToString())[1];
                    string name = Regex.Split(slugcat, "\\.")[0];
                    ListItem item = new ListItem(name);
                    item.displayName = SlugcatStats.getSlugcatName(new SlugcatStats.Name(name,false));
                    saves.Add(item);
                }
            }
        }
        return saves;
    }

    public void LoadAndDisplay(string slugcat)
    {
        string[] saveData = null;
        int slotNum = Custom.rainWorld.options.saveSlot;
        string savePath = $"{Application.persistentDataPath}\\Inventory";
        if (Directory.Exists(savePath))
        {
            string[] fileList = Directory.GetFiles(savePath);
            for (int i = 0; i < fileList.Length; i++)
            {
                if (fileList[i].Contains(slotNum.ToString()) && fileList[i].Contains(slugcat) && fileList[i].EndsWith(".txt"))
                {
                    saveData = File.ReadAllLines(fileList[i]);
                }
            }
            if(saveData != null)
            {
                string[] invData = Regex.Split(saveData[0], "<V>");
                int width;
                int height;

                width = int.Parse(invData[1]);
                height = int.Parse(invData[2]);

                string[] objectData = Regex.Split(invData[3], "<SObj>");
                itemsStored = objectData.Length -1;
                Debug.Log($"{slugcat} has {itemsStored} items stored");

                invSize.x = width; 
                invSize.y = height;
                widthSlider.value = invSize.x.ToString();
                heightSlider.value = invSize.y.ToString();

                for (int i = 0; i < previewSlots.Length; i++)
                {
                    if (i < itemsStored)
                    {
                        previewSlots[i].colorFill = Menu.MenuColorEffect.rgbMediumGrey;
                        previewSlots[i].fillAlpha = 1f;
                    }
                    else
                    {
                        previewSlots[i].fillAlpha = 0f;
                    }
                }
            }
            else
            {
                invSize.x = defaultInvSize.x;
                invSize.y = defaultInvSize.y;
                widthSlider.value = invSize.x.ToString();
                heightSlider.value = invSize.y.ToString();

                for (int i = 0; i < previewSlots.Length; i++)
                {
                    previewSlots[i].fillAlpha = 0f;
                }
            }
            return;
        }
        if(slugcat == "Default")
        {
            invSize.x = defaultInvSize.x;
            invSize.y = defaultInvSize.y;
            widthSlider.value = invSize.x.ToString();
            heightSlider.value = invSize.y.ToString();

            for (int i = 0; i < previewSlots.Length; i++)
            {
                previewSlots[i].fillAlpha = 0f;
            }
        }
    }

    public void ModifyInventorySize(string slugcat)
    {
        //Open the save file for this slugcat and update the inventory size
        //Move each stored object's index to a new slot
        string saveData = "";
        int slotNum = Custom.rainWorld.options.saveSlot;
        string savePath = $"{Application.persistentDataPath}\\Inventory";
        string saveFile = "";
        if (Directory.Exists(savePath))
        {
            string[] fileList = Directory.GetFiles(savePath);
            for (int i = 0; i < fileList.Length; i++)
            {
                if (fileList[i].Contains(slotNum.ToString()) && fileList[i].Contains(slugcat) && fileList[i].EndsWith(".txt"))
                {
                    saveFile = fileList[i];
                    saveData = File.ReadAllLines(fileList[i])[0];
                }
            }
        }

        if(saveData != "")
        {
            string newSize = $"0<V>{invSize.x}<V>{invSize.y}<V><SObj>";
            List<string> updatedObjects = new List<string>();

            string[] objects = Regex.Split(saveData, "<SObj>");
            for (int i = 1; i < objects.Length; i++)
            {
                string[] objData = Regex.Split(objects[i], "<X>");
                objData[2] = (i - 1).ToString();
                updatedObjects.Add(string.Join("<X>",objData));
            }

            string updatedSave = $"{newSize}{string.Join("<SObj>", updatedObjects.ToArray())}";

            File.WriteAllText(saveFile,updatedSave);
        }
    }
}

