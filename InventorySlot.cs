using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HUD;
using UnityEngine;
using RWCustom;

public class InventorySlot : HudPart
{
    public Inventory inventory;
    public InventoryData.StoredObject storedItem;
    public IntVector2 slotNum;
    public int slotIndex;
    public Vector2 pos;
    public FSprite[] sprites;
    public FSprite itemSprite;
    public FLabel label;
    public bool isSelected = false;
    public float fade = 1f;
    public InventorySlot(Inventory inventory, HUD.HUD hud, IntVector2 slotNum) : base(hud)
    {
        this.inventory = inventory;
        this.slotNum = slotNum;
        InitiateSprites();
    }

    public override void Update()
    {
        base.Update();
        if (this.isSelected)
        {
            this.fade = 1f;
        }
        else
        {
            if (this.fade > 0f)
            {
                this.fade -= 8f * Time.deltaTime;
            }
        }
    }

    public Vector2 SlotPos(Vector2 pos)
    {
        float offset = this.inventory.slotOffset;
        Vector2 slotPos = new Vector2(pos.x + (offset * this.slotNum.x), pos.y + (offset * this.slotNum.y));
        Vector2 invCenter = new Vector2(pos.x + (inventory.slotSize * inventory.invSize.x / 2) - (this.inventory.slotOffset - this.inventory.slotSize), pos.y);
        Vector2 zoomPos = Vector2.Lerp(invCenter, slotPos, inventory.fade);
        return zoomPos;
    }

    public void InitiateSprites()
    {
        this.sprites = new FSprite[17];
        for (int i = 0; i < 4; i++)
        {
            this.sprites[this.SideSprite(i)] = new FSprite("pixel", true);
            this.sprites[this.SideSprite(i)].scaleY = 2f;
            this.sprites[this.SideSprite(i)].scaleX = 2f;
            this.sprites[this.CornerSprite(i)] = new FSprite("UIroundedCorner", true);
            this.sprites[this.FillSideSprite(i)] = new FSprite("pixel", true);
            this.sprites[this.FillSideSprite(i)].scaleY = 6f;
            this.sprites[this.FillSideSprite(i)].scaleX = 6f;
            this.sprites[this.FillCornerSprite(i)] = new FSprite("UIroundedCornerInside", true);
        }
        this.sprites[this.SideSprite(0)].anchorY = 0f;
        this.sprites[this.SideSprite(2)].anchorY = 0f;
        this.sprites[this.SideSprite(1)].anchorX = 0f;
        this.sprites[this.SideSprite(3)].anchorX = 0f;
        this.sprites[this.CornerSprite(0)].scaleY = -1f;
        this.sprites[this.CornerSprite(2)].scaleX = -1f;
        this.sprites[this.CornerSprite(3)].scaleY = -1f;
        this.sprites[this.CornerSprite(3)].scaleX = -1f;
        this.sprites[this.MainFillSprite] = new FSprite("pixel", true);
        this.sprites[this.MainFillSprite].anchorY = 0f;
        this.sprites[this.MainFillSprite].anchorX = 0f;
        this.sprites[this.FillSideSprite(0)].anchorY = 0f;
        this.sprites[this.FillSideSprite(2)].anchorY = 0f;
        this.sprites[this.FillSideSprite(1)].anchorX = 0f;
        this.sprites[this.FillSideSprite(3)].anchorX = 0f;
        this.sprites[this.FillCornerSprite(0)].scaleY = -1f;
        this.sprites[this.FillCornerSprite(2)].scaleX = -1f;
        this.sprites[this.FillCornerSprite(3)].scaleY = -1f;
        this.sprites[this.FillCornerSprite(3)].scaleX = -1f;
        for (int j = 0; j < 9; j++)
        {
            this.sprites[j].color = new Color(0f, 0f, 0f);
            this.sprites[j].alpha = 0.35f;
        }
        this.itemSprite = new FSprite("Futile_White", true);
        this.itemSprite.color = new Color(0f, 1f, 0f);
        for (int k = 0; k < this.sprites.Length; k++)
        {
            this.hud.fContainers[1].AddChild(this.sprites[k]);
        }
        this.hud.fContainers[1].AddChild(this.itemSprite);
    }

    public override void Draw(float timeStacker)
    {
        base.Draw(timeStacker);
        if (storedItem != null)
        {
            //Assign sprite and color from loaded object
            if (itemSprite.element.name == "Futile_White")
            {
                itemSprite.SetElementByName(storedItem.spriteName);
                itemSprite.color = storedItem.spriteColor;
            }
        }
        Vector2 vector = this.DrawPos(timeStacker);
        Vector2 vector2 = new Vector2(this.inventory.slotSize, this.inventory.slotSize);
        vector.x -= 0.33333334f;
        vector.y -= 0.33333334f;
        vector.x -= vector2.x / 2f;
        vector.y -= vector2.y / 2f;
        this.sprites[this.SideSprite(0)].x = vector.x + 1f;
        this.sprites[this.SideSprite(0)].y = vector.y + 6f;
        this.sprites[this.SideSprite(0)].scaleY = vector2.y - 12f;
        this.sprites[this.SideSprite(1)].x = vector.x + 6f;
        this.sprites[this.SideSprite(1)].y = vector.y + vector2.y - 1f;
        this.sprites[this.SideSprite(1)].scaleX = vector2.x - 12f;
        this.sprites[this.SideSprite(2)].x = vector.x + vector2.x - 1f;
        this.sprites[this.SideSprite(2)].y = vector.y + 6f;
        this.sprites[this.SideSprite(2)].scaleY = vector2.y - 12f;
        this.sprites[this.SideSprite(3)].x = vector.x + 6f;
        this.sprites[this.SideSprite(3)].y = vector.y + 1f;
        this.sprites[this.SideSprite(3)].scaleX = vector2.x - 12f;
        this.sprites[this.CornerSprite(0)].x = vector.x + 3.5f;
        this.sprites[this.CornerSprite(0)].y = vector.y + 3.5f;
        this.sprites[this.CornerSprite(1)].x = vector.x + 3.5f;
        this.sprites[this.CornerSprite(1)].y = vector.y + vector2.y - 3.5f;
        this.sprites[this.CornerSprite(2)].x = vector.x + vector2.x - 3.5f;
        this.sprites[this.CornerSprite(2)].y = vector.y + vector2.y - 3.5f;
        this.sprites[this.CornerSprite(3)].x = vector.x + vector2.x - 3.5f;
        this.sprites[this.CornerSprite(3)].y = vector.y + 3.5f;
        Color color = new Color(1f, 1f, 1f);
        for (int j = 0; j < 4; j++)
        {
            this.sprites[this.SideSprite(j)].color = color;
            this.sprites[this.CornerSprite(j)].color = color;
        }
        this.sprites[this.FillSideSprite(0)].x = vector.x + 4f;
        this.sprites[this.FillSideSprite(0)].y = vector.y + 7f;
        this.sprites[this.FillSideSprite(0)].scaleY = vector2.y - 14f;
        this.sprites[this.FillSideSprite(1)].x = vector.x + 7f;
        this.sprites[this.FillSideSprite(1)].y = vector.y + vector2.y - 4f;
        this.sprites[this.FillSideSprite(1)].scaleX = vector2.x - 14f;
        this.sprites[this.FillSideSprite(2)].x = vector.x + vector2.x - 4f;
        this.sprites[this.FillSideSprite(2)].y = vector.y + 7f;
        this.sprites[this.FillSideSprite(2)].scaleY = vector2.y - 14f;
        this.sprites[this.FillSideSprite(3)].x = vector.x + 7f;
        this.sprites[this.FillSideSprite(3)].y = vector.y + 4f;
        this.sprites[this.FillSideSprite(3)].scaleX = vector2.x - 14f;
        this.sprites[this.FillCornerSprite(0)].x = vector.x + 3.5f;
        this.sprites[this.FillCornerSprite(0)].y = vector.y + 3.5f;
        this.sprites[this.FillCornerSprite(1)].x = vector.x + 3.5f;
        this.sprites[this.FillCornerSprite(1)].y = vector.y + vector2.y - 3.5f;
        this.sprites[this.FillCornerSprite(2)].x = vector.x + vector2.x - 3.5f;
        this.sprites[this.FillCornerSprite(2)].y = vector.y + vector2.y - 3.5f;
        this.sprites[this.FillCornerSprite(3)].x = vector.x + vector2.x - 3.5f;
        this.sprites[this.FillCornerSprite(3)].y = vector.y + 3.5f;
        this.sprites[this.MainFillSprite].x = vector.x + 7f;
        this.sprites[this.MainFillSprite].y = vector.y + 7f;
        this.sprites[this.MainFillSprite].scaleX = vector2.x - 14f;
        this.sprites[this.MainFillSprite].scaleY = vector2.y - 14f;
        this.itemSprite.x = vector.x + inventory.slotSize / 2;
        this.itemSprite.y = vector.y + inventory.slotSize / 2;
        //Fading
        if (inventory.isShown)
        {
            if (inventory.fade > 0.4f)
            {
                //Edge sprites
                this.sprites[this.SideSprite(0)].alpha = this.fade;
                this.sprites[this.SideSprite(2)].alpha = this.fade;
                this.sprites[this.SideSprite(1)].alpha = this.fade;
                this.sprites[this.SideSprite(3)].alpha = this.fade;
                this.sprites[this.CornerSprite(0)].alpha = this.fade;
                this.sprites[this.CornerSprite(2)].alpha = this.fade;
                this.sprites[this.CornerSprite(1)].alpha = this.fade;
                this.sprites[this.CornerSprite(3)].alpha = this.fade;
                if (this.storedItem != null)
                {
                    this.sprites[this.SideSprite(0)].color = Color.Lerp(new Color(1f, 1f, 1f), this.storedItem.spriteColor, this.fade * 0.8f);
                    this.sprites[this.SideSprite(2)].color = Color.Lerp(new Color(1f, 1f, 1f), this.storedItem.spriteColor, this.fade * 0.8f);
                    this.sprites[this.SideSprite(1)].color = Color.Lerp(new Color(1f, 1f, 1f), this.storedItem.spriteColor, this.fade * 0.8f);
                    this.sprites[this.SideSprite(3)].color = Color.Lerp(new Color(1f, 1f, 1f), this.storedItem.spriteColor, this.fade * 0.8f);
                    this.sprites[this.CornerSprite(0)].color = Color.Lerp(new Color(1f, 1f, 1f), this.storedItem.spriteColor, this.fade * 0.8f);
                    this.sprites[this.CornerSprite(2)].color = Color.Lerp(new Color(1f, 1f, 1f), this.storedItem.spriteColor, this.fade * 0.8f);
                    this.sprites[this.CornerSprite(1)].color = Color.Lerp(new Color(1f, 1f, 1f), this.storedItem.spriteColor, this.fade * 0.8f);
                    this.sprites[this.CornerSprite(3)].color = Color.Lerp(new Color(1f, 1f, 1f), this.storedItem.spriteColor, this.fade * 0.8f);
                }
                else
                {
                    this.sprites[this.SideSprite(0)].color = new Color(0.8f, 0.8f, 0.8f);
                    this.sprites[this.SideSprite(2)].color = new Color(0.8f, 0.8f, 0.8f);
                    this.sprites[this.SideSprite(1)].color = new Color(0.8f, 0.8f, 0.8f);
                    this.sprites[this.SideSprite(3)].color = new Color(0.8f, 0.8f, 0.8f);
                    this.sprites[this.CornerSprite(0)].color = new Color(0.8f, 0.8f, 0.8f);
                    this.sprites[this.CornerSprite(2)].color = new Color(0.8f, 0.8f, 0.8f);
                    this.sprites[this.CornerSprite(1)].color = new Color(0.8f, 0.8f, 0.8f);
                    this.sprites[this.CornerSprite(3)].color = new Color(0.8f, 0.8f, 0.8f);
                }
                //Fill sprites
                for (int j = 0; j < 9; j++)
                {
                    if (this.storedItem != null)
                    {
                        this.sprites[j].color = Color.Lerp(Color.Lerp(new Color(0f, 0f, 0f), this.storedItem.spriteColor, 0.1f), this.storedItem.spriteColor, this.fade * 0.5f);
                        if (this.isSelected)
                        {
                            this.sprites[j].alpha = Mathf.Lerp(0f, 0.45f, this.fade);
                        }
                        else
                        {
                            this.sprites[j].alpha = Mathf.Lerp(0f, 0.45f, inventory.fade);
                        }
                    }
                    else
                    {
                        this.sprites[j].color = new Color(0f, 0f, 0f);
                        this.sprites[j].alpha = Mathf.Lerp(0f, 0.35f, inventory.fade);
                    }
                    //if (this.sprites[j].alpha < 0.35f)
                    //{
                    //    this.sprites[j].alpha += 0.75f * Time.deltaTime;
                    //}
                }
                //Item sprite
                if (this.storedItem != null)
                {
                    this.itemSprite.alpha = Mathf.Lerp(0.74f, 1f, this.fade);
                }
                else
                {
                    this.itemSprite.alpha = 0f;
                }
            }
        }
        else
        {
            for (int i = 0; i < this.sprites.Length; i++)
            {
                if (this.sprites[i].alpha > 0f)
                {
                    this.sprites[i].alpha -= 3f * Time.deltaTime;
                }
            }
            this.itemSprite.alpha -= 3f * Time.deltaTime;
        }
    }

    public Vector2 DrawPos(float timeStacker)
    {
        return this.pos;
    }

    public int SideSprite(int side)
    {
        return 9 + side;
    }

    public int CornerSprite(int corner)
    {
        return 13 + corner;
    }

    public int FillSideSprite(int side)
    {
        return side;
    }

    public int FillCornerSprite(int corner)
    {
        return 4 + corner;
    }

    public int MainFillSprite
    {
        get
        {
            return 8;
        }
    }
    public override void ClearSprites()
    {
        base.ClearSprites();
        for (int i = 0; i < this.sprites.Length; i++)
        {
            this.sprites[i].RemoveFromContainer();
        }
        this.itemSprite.RemoveFromContainer();
    }
}