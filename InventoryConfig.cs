using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu.Remix.MixedUI;
using Menu.Remix;
using UnityEngine;
using RWCustom;

public class InventoryConfig : OptionInterface
{
    //Configuration
    public static IntVector2 invSize = new IntVector2(3, 2);
    public static Color customCursorColor = new Color(1f, 1f, 1f);

    public static Configurable<bool> critBool;
    public static Configurable<bool> karmaBool;
    public static Configurable<bool> slowBool;
    public static Configurable<bool> rainbowBool;
    public static Configurable<Color> cursorColorConfig;


    //Preview Window
    public OpRect previewWindow;
    public OpLabel windowLabel;
    public OpRect[] previewSlots;
    public OpLabel sizeLabel;
    public OpLabel totalLabel;
    public OpLabel descLabel;

    //Sliders
    //public OpSliderSubtle widthSlider;
    //public OpSliderSubtle heightSlider;

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

    public override void Initialize()
    {
        base.Initialize();

        config = new ConfigHolder(this);

        Tabs = new OpTab[1];
        Tabs[0] = new OpTab(this, "Config");

        //Preview Window
        previewWindow = new OpRect(new Vector2(40f, 325f), new Vector2(525f, 225f), 0.3f);
        windowLabel = new OpLabel(46f, 555f, "Inventory Size", true);
        previewSlots = new OpRect[50];
        for (int i = 0; i < previewSlots.Length; i++)
        {
            previewSlots[i] = new OpRect(new Vector2(0f, 0f), new Vector2(30f, 30f), 0.3f);
            Tabs[0].AddItems(previewSlots[i]);
        }
        sizeLabel = new OpLabel(50f, 330f, "Size", false);
        totalLabel = new OpLabel(new Vector2(510f, 330f), new Vector2(), "Total Slots", FLabelAlignment.Right, false);
        descLabel = new OpLabel(191f, 557f, "-    New inventory size will take affect upon starting a new game", false);
        Tabs[0].AddItems(previewWindow, windowLabel, sizeLabel, totalLabel, descLabel);

        //Sliders
        //widthSlider = new OpSliderSubtle(new Vector2(45f, 285f), "xsize", new IntVector2(1, 10), 520, false, invSize.x);
        //heightSlider = new OpSliderSubtle(new Vector2(0f, 325f), "ysize", new IntVector2(1, 5), 220, true, invSize.y);
        //Tabs[0].AddItems(widthSlider, heightSlider);

        //Checkboxes
        settingsLabel = new OpLabel(45f, 230f, "Additional Settings", true);
        descLabel = new OpLabel(45f, 210f, "Hover over each option for more information", false);
        creatureCheck = new OpCheckBox(critBool ,new Vector2(45f, 170f));
        //creatureCheck.description = "Allows you to store any creature, living or dead that Slugcat can grab";
        creatureLabel = new OpLabel(75f, 172f, "Creature Storage", false);
        karmaCheck = new OpCheckBox(karmaBool, new Vector2(45f, 130f));
        //karmaCheck.description = "Allows you to store Karma Flowers in the inventory - extremely unbalanced";
        karmaLabel = new OpLabel(75f, 132f, "Karma Flower Storage", false);
        slowCheck = new OpCheckBox(slowBool, new Vector2(45f, 90f));
        //slowCheck.description = "Slows down time when the inventory is open";
        slowLabel = new OpLabel(75f, 92f, "Slow Motion Inventory", false);
        rainbowCheck = new OpCheckBox(rainbowBool, new Vector2(45f, 50f));
        //rainbowCheck.description = "Changes the color of the inventory cursor to a rainbow - this setting overrides custom cursor color";
        rainbowLabel = new OpLabel(75f, 52f, "Rainbow Cursor", false);
        Tabs[0].AddItems(settingsLabel, descLabel, creatureCheck, karmaCheck, creatureLabel, karmaLabel, slowCheck, slowLabel, rainbowCheck, rainbowLabel);

        //Color Picker
        cursorColor = new OpColorPicker(cursorColorConfig, new Vector2(400f, 50f));
        cursorColor.description = "Pick a custom color for the inventory cursor";
        colorLabel = new OpLabel(404f, 210f, "Pick a custom cursor color", false);
        Tabs[0].AddItems(cursorColor, colorLabel);

        //Mod Info
        infoLabel = new OpLabel(new Vector2(300f,-5f), new Vector2(), "Inventory " + InventoryMod.versionNumber + " by LeeMoriya", FLabelAlignment.Center,false);
        Tabs[0].AddItems(infoLabel);

        critBool = config.Bind<bool>("critBool", false, new ConfigurableInfo("Allows you to store any creature, living or dead that Slugcat can grab", null, ""));
        karmaBool = config.Bind<bool>("karmaBool", false, new ConfigurableInfo("Allows you to store Karma Flowers in the inventory - extremely unbalanced", null, ""));
        slowBool = config.Bind<bool>("slowBool", false, new ConfigurableInfo("Slows down time when the inventory is open", null, ""));
        rainbowBool = config.Bind<bool>("rainbowBool", false, new ConfigurableInfo("Changes the color of the inventory cursor to a rainbow - this setting overrides custom cursor color", null, ""));
        cursorColorConfig = config.Bind<Color>("cursorColorConfig", new Color(1f, 1f, 1f), new ConfigurableInfo("Set a custom color for the inventory cursor", null, "", ""));
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

        for (int i = 0; i < previewSlots.Length; i++)
        {
            if(i < totalSlots)
            {
                //previewSlots[i].pos = new Vector2(xAnchor + (offset * xPos), yAnchor + (offset * yPos));
                previewSlots[i].Show();

                xPos++;
                if(xPos == invSize.x)
                {
                    xPos = 0;
                    yPos++;
                }
            }
            else
            {
                previewSlots[i].Hide();
                //previewSlots[i].pos = new Vector2(300f, 20000f);
            }
        }

        //invSize.x = widthSlider.valueInt;
        //invSize.y = heightSlider.valueInt;

        sizeLabel.text = "Width: " + invSize.x + " | Height: " + invSize.y;
        totalLabel.text = "Total Slots: " + (invSize.x * invSize.y).ToString();

        //if (rainbowCheck.valueBool)
        //{
        //    cursorColor.greyedOut = true;
        //}
        //else
        //{
        //    cursorColor.greyedOut = false;
        //}
    }

    //public override void ConfigOnChange()
    //{
    //    base.ConfigOnChange();
    //    InventoryData.invSize = invSize;
    //    creatureStorage = creatureCheck.valueBool;
    //    karmaStorage = karmaCheck.valueBool;
    //    slowMenu = slowCheck.valueBool;
    //    rainbowCursor = rainbowCheck.valueBool;
    //    customCursorColor = cursorColor.valueColor;
    //    if (config.ContainsKey("critStore"))
    //    {
    //        if (config["critStore"] == "true")
    //        {
    //            creatureStorage = true;
    //        }
    //        else
    //        {
    //            creatureStorage = false;
    //        }
    //    }
    //    if (config.ContainsKey("karmaStore"))
    //    {
    //        if (config["karmaStore"] == "true")
    //        {
    //            karmaStorage = true;
    //        }
    //        else
    //        {
    //            karmaStorage = false;
    //        }
    //    }
    //    if (config.ContainsKey("slowMenu"))
    //    {
    //        if (config["slowMenu"] == "true")
    //        {
    //            slowMenu = true;
    //        }
    //        else
    //        {
    //            slowMenu = false;
    //        }
    //    }
    //    if (config.ContainsKey("rainCheck"))
    //    {
    //        if (config["rainCheck"] == "true")
    //        {
    //            rainbowCursor = true;
    //        }
    //        else
    //        {
    //            rainbowCursor = false;
    //        }
    //    }
    //    if (config.ContainsKey("cursorColor"))
    //    {
    //        customCursorColor = OpColorPicker.HexToColor(config["cursorColor"]);
    //    }
    //}
}

