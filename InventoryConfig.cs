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
    public IntVector2 invSize = new IntVector2(3, 2);

    //Preview Window
    public OpRect previewWindow;
    public OpLabel windowLabel;
    public OpRect[] previewSlots;
    public OpLabel sizeLabel;
    public OpLabel totalLabel;

    //Sliders
    public OpSliderSubtle widthSlider;
    public OpSliderSubtle heightSlider;


    public InventoryConfig() : base(plugin: InventoryMod.instance)
    {

    }

    public override void Initialize()
    {
        this.Tabs = new OpTab[1];
        this.Tabs[0] = new OpTab("Config");

        //Preview Window
        this.previewWindow = new OpRect(new Vector2(40f, 325f), new Vector2(525f, 225f), 0.3f);
        this.windowLabel = new OpLabel(45f, 555f, "Inventory Size", true);
        this.previewSlots = new OpRect[50];
        for (int i = 0; i < this.previewSlots.Length; i++)
        {
            previewSlots[i] = new OpRect(new Vector2(0f, 0f), new Vector2(30f, 30f), 0.3f);
            this.Tabs[0].AddItems(previewSlots[i]);
        }
        this.sizeLabel = new OpLabel(50f, 330f, "Size", false);
        this.totalLabel = new OpLabel(510f, 330f, "Total", false);
        this.totalLabel.alignment = FLabelAlignment.Right;
        this.Tabs[0].AddItems(this.previewWindow, this.windowLabel, this.sizeLabel, this.totalLabel);

        //Sliders
        this.widthSlider = new OpSliderSubtle(new Vector2(45f, 285f), "xsize", new IntVector2(1, 10), 520, false, invSize.x);
        this.heightSlider = new OpSliderSubtle(new Vector2(0f, 325f), "ysize", new IntVector2(1, 5), 220, true, invSize.y);
        this.Tabs[0].AddItems(this.widthSlider, this.heightSlider);
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
                this.previewSlots[i].pos = new Vector2(300f, 2000f);
            }
        }


        invSize.x = widthSlider.valueInt;
        invSize.y = heightSlider.valueInt;

        this.sizeLabel.text = "Width: " + invSize.x + " | Height: " + invSize.y;
        this.totalLabel.text = "Total: " + (invSize.x * invSize.y).ToString();
    }

    public override void ConfigOnChange()
    {
        base.ConfigOnChange();
        InventoryData.invSize = invSize;
    }
}

