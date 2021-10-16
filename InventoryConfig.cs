using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompletelyOptional;
using OptionalUI;
using UnityEngine;
using RWCustom;

public class InventoryConfig : OptionInterface
{
    //Configuration
    public static IntVector2 invSize = new IntVector2(3, 2);
    public static bool creatureStorage = false;
    public static bool karmaStorage = false;
    public static bool slowMenu = false;
    public static bool rainbowCursor = false;
    public static Color customCursorColor = new Color(1f, 1f, 1f);

    //Preview Window
    public OpRect previewWindow;
    public OpLabel windowLabel;
    public OpRect[] previewSlots;
    public OpLabel sizeLabel;
    public OpLabel totalLabel;
    public OpLabel descLabel;

    //Sliders
    public OpSliderSubtle widthSlider;
    public OpSliderSubtle heightSlider;

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

    public InventoryConfig() : base(plugin: InventoryMod.instance)
    {

    }

    public override void Initialize()
    {
        this.Tabs = new OpTab[1];
        this.Tabs[0] = new OpTab("Config");

        //Preview Window
        this.previewWindow = new OpRect(new Vector2(40f, 325f), new Vector2(525f, 225f), 0.3f);
        this.windowLabel = new OpLabel(46f, 555f, "Inventory Size", true);
        this.previewSlots = new OpRect[50];
        for (int i = 0; i < this.previewSlots.Length; i++)
        {
            previewSlots[i] = new OpRect(new Vector2(0f, 0f), new Vector2(30f, 30f), 0.3f);
            this.Tabs[0].AddItems(previewSlots[i]);
        }
        this.sizeLabel = new OpLabel(50f, 330f, "Size", false);
        this.totalLabel = new OpLabel(new Vector2(510f, 330f), new Vector2(), "Total Slots", FLabelAlignment.Right, false);
        this.descLabel = new OpLabel(191f, 557f, "-    New inventory size will take affect upon starting a new game", false);
        this.Tabs[0].AddItems(this.previewWindow, this.windowLabel, this.sizeLabel, this.totalLabel, this.descLabel);

        //Sliders
        this.widthSlider = new OpSliderSubtle(new Vector2(45f, 285f), "xsize", new IntVector2(1, 10), 520, false, invSize.x);
        this.heightSlider = new OpSliderSubtle(new Vector2(0f, 325f), "ysize", new IntVector2(1, 5), 220, true, invSize.y);
        this.Tabs[0].AddItems(this.widthSlider, this.heightSlider);

        //Checkboxes
        this.settingsLabel = new OpLabel(45f, 230f, "Additional Settings", true);
        this.descLabel = new OpLabel(45f, 210f, "Hover over each option for more information", false);
        this.creatureCheck = new OpCheckBox(45f, 170f, "critStore", false);
        this.creatureCheck.description = "Allows you to store any creature, living or dead that Slugcat can grab";
        this.creatureLabel = new OpLabel(75f, 172f, "Creature Storage", false);
        this.karmaCheck = new OpCheckBox(45f, 130f, "karmaStore", false);
        this.karmaCheck.description = "Allows you to store Karma Flowers in the inventory - extremely unbalanced";
        this.karmaLabel = new OpLabel(75f, 132f, "Karma Flower Storage", false);
        this.slowCheck = new OpCheckBox(45f, 90f, "slowMenu", false);
        this.slowCheck.description = "Slows down time when the inventory is open";
        this.slowLabel = new OpLabel(75f, 92f, "Slow Motion Inventory", false);
        this.rainbowCheck = new OpCheckBox(45f, 50f, "rainCheck", false);
        this.rainbowCheck.description = "Changes the color of the inventory cursor to a rainbow - this setting overrides custom cursor color";
        this.rainbowLabel = new OpLabel(75f, 52f, "Rainbow Cursor", false);
        this.Tabs[0].AddItems(this.settingsLabel, this.descLabel, this.creatureCheck, this.karmaCheck, this.creatureLabel, this.karmaLabel, this.slowCheck, this.slowLabel, this.rainbowCheck, this.rainbowLabel);

        //Color Picker
        this.cursorColor = new OpColorPicker(new Vector2(400f, 50f), "cursorColor", "FFFFFF");
        this.cursorColor.description = "Pick a custom color for the inventory cursor";
        this.colorLabel = new OpLabel(404f, 210f, "Pick a custom cursor color", false);
        this.Tabs[0].AddItems(this.cursorColor, this.colorLabel);

        //Mod Info
        this.infoLabel = new OpLabel(new Vector2(300f,-5f), new Vector2(), "Inventory " + InventoryMod.versionNumber + " by LeeMoriya", FLabelAlignment.Center,false);
        this.Tabs[0].AddItems(this.infoLabel);
    }

    public override void Update(float dt)
    {
        base.Update(dt);
        int totalSlots = invSize.x * invSize.y;
        float xAnchor = Mathf.Lerp(290f, 125f, Mathf.InverseLerp(1, 10, invSize.x));
        float yAnchor = Mathf.Lerp(425f, 360f, Mathf.InverseLerp(1, 5, invSize.y));
        float offset = 35f;
        int xPos = 0;
        int yPos = 0;

        for (int i = 0; i < this.previewSlots.Length; i++)
        {
            if(i < totalSlots)
            {
                this.previewSlots[i].pos = new Vector2(xAnchor + (offset * xPos), yAnchor + (offset * yPos));
                this.previewSlots[i].Show();

                xPos++;
                if(xPos == invSize.x)
                {
                    xPos = 0;
                    yPos++;
                }
            }
            else
            {
                this.previewSlots[i].Hide();
                this.previewSlots[i].pos = new Vector2(300f, 20000f);
            }
        }

        invSize.x = widthSlider.valueInt;
        invSize.y = heightSlider.valueInt;

        this.sizeLabel.text = "Width: " + invSize.x + " | Height: " + invSize.y;
        this.totalLabel.text = "Total Slots: " + (invSize.x * invSize.y).ToString();

        if (this.rainbowCheck.valueBool)
        {
            this.cursorColor.greyedOut = true;
        }
        else
        {
            this.cursorColor.greyedOut = false;
        }
    }

    public override void ConfigOnChange()
    {
        base.ConfigOnChange();
        InventoryData.invSize = invSize;
        creatureStorage = this.creatureCheck.valueBool;
        karmaStorage = this.karmaCheck.valueBool;
        slowMenu = this.slowCheck.valueBool;
        rainbowCursor = this.rainbowCheck.valueBool;
        customCursorColor = this.cursorColor.valueColor;
        if (config.ContainsKey("critStore"))
        {
            if (config["critStore"] == "true")
            {
                creatureStorage = true;
            }
            else
            {
                creatureStorage = false;
            }
        }
        if (config.ContainsKey("karmaStore"))
        {
            if (config["karmaStore"] == "true")
            {
                karmaStorage = true;
            }
            else
            {
                karmaStorage = false;
            }
        }
        if (config.ContainsKey("slowMenu"))
        {
            if (config["slowMenu"] == "true")
            {
                slowMenu = true;
            }
            else
            {
                slowMenu = false;
            }
        }
        if (config.ContainsKey("rainCheck"))
        {
            if (config["rainCheck"] == "true")
            {
                rainbowCursor = true;
            }
            else
            {
                rainbowCursor = false;
            }
        }
        if (config.ContainsKey("cursorColor"))
        {
            customCursorColor = OpColorPicker.HexToColor(config["cursorColor"]);
        }
    }
}

