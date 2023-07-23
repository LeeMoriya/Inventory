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
        if (isSelected)
        {
            fade = 1f;
        }
        else
        {
            if (InventoryConfig.slowBool.Value)
            {
                fade -= 20f * Time.deltaTime;
            }
            else
            {
                fade -= 8f * Time.deltaTime;
            }
        }
    }

    public Vector2 SlotPos(Vector2 pos)
    {
        float offset = inventory.slotOffset;
        Vector2 slotPos = new Vector2(pos.x + (offset * slotNum.x), pos.y + (offset * slotNum.y));
        Vector2 invCenter = new Vector2(pos.x + (inventory.slotSize * inventory.invSize.x / 2) - (inventory.slotOffset - inventory.slotSize), pos.y);
        Vector2 zoomPos = Vector2.Lerp(invCenter, slotPos, inventory.fade);
        return zoomPos;
    }

    public void InitiateSprites()
    {
        sprites = new FSprite[17];
        for (int i = 0; i < 4; i++)
        {
            sprites[SideSprite(i)] = new FSprite("pixel", true);
            sprites[SideSprite(i)].scaleY = 2f;
            sprites[SideSprite(i)].scaleX = 2f;
            sprites[CornerSprite(i)] = new FSprite("UIroundedCorner", true);
            sprites[FillSideSprite(i)] = new FSprite("pixel", true);
            sprites[FillSideSprite(i)].scaleY = 6f;
            sprites[FillSideSprite(i)].scaleX = 6f;
            sprites[FillCornerSprite(i)] = new FSprite("UIroundedCornerInside", true);
        }
        sprites[SideSprite(0)].anchorY = 0f;
        sprites[SideSprite(2)].anchorY = 0f;
        sprites[SideSprite(1)].anchorX = 0f;
        sprites[SideSprite(3)].anchorX = 0f;
        sprites[CornerSprite(0)].scaleY = -1f;
        sprites[CornerSprite(2)].scaleX = -1f;
        sprites[CornerSprite(3)].scaleY = -1f;
        sprites[CornerSprite(3)].scaleX = -1f;
        sprites[MainFillSprite] = new FSprite("pixel", true);
        sprites[MainFillSprite].anchorY = 0f;
        sprites[MainFillSprite].anchorX = 0f;
        sprites[FillSideSprite(0)].anchorY = 0f;
        sprites[FillSideSprite(2)].anchorY = 0f;
        sprites[FillSideSprite(1)].anchorX = 0f;
        sprites[FillSideSprite(3)].anchorX = 0f;
        sprites[FillCornerSprite(0)].scaleY = -1f;
        sprites[FillCornerSprite(2)].scaleX = -1f;
        sprites[FillCornerSprite(3)].scaleY = -1f;
        sprites[FillCornerSprite(3)].scaleX = -1f;
        for (int j = 0; j < 9; j++)
        {
            sprites[j].color = new Color(0f, 0f, 0f);
            sprites[j].alpha = 0.35f;
        }
        itemSprite = new FSprite("Futile_White", true);
        itemSprite.color = new Color(0f, 1f, 0f);
        for (int k = 0; k < sprites.Length; k++)
        {
            hud.fContainers[1].AddChild(sprites[k]);
        }
        hud.fContainers[1].AddChild(itemSprite);
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
        Vector2 vector = DrawPos(timeStacker);
        Vector2 vector2 = new Vector2(inventory.slotSize, inventory.slotSize);
        vector.x -= 0.33333334f;
        vector.y -= 0.33333334f;
        vector.x -= vector2.x / 2f;
        vector.y -= vector2.y / 2f;
        sprites[SideSprite(0)].x = vector.x + 1f;
        sprites[SideSprite(0)].y = vector.y + 6f;
        sprites[SideSprite(0)].scaleY = vector2.y - 12f;
        sprites[SideSprite(1)].x = vector.x + 6f;
        sprites[SideSprite(1)].y = vector.y + vector2.y - 1f;
        sprites[SideSprite(1)].scaleX = vector2.x - 12f;
        sprites[SideSprite(2)].x = vector.x + vector2.x - 1f;
        sprites[SideSprite(2)].y = vector.y + 6f;
        sprites[SideSprite(2)].scaleY = vector2.y - 12f;
        sprites[SideSprite(3)].x = vector.x + 6f;
        sprites[SideSprite(3)].y = vector.y + 1f;
        sprites[SideSprite(3)].scaleX = vector2.x - 12f;
        sprites[CornerSprite(0)].x = vector.x + 3.5f;
        sprites[CornerSprite(0)].y = vector.y + 3.5f;
        sprites[CornerSprite(1)].x = vector.x + 3.5f;
        sprites[CornerSprite(1)].y = vector.y + vector2.y - 3.5f;
        sprites[CornerSprite(2)].x = vector.x + vector2.x - 3.5f;
        sprites[CornerSprite(2)].y = vector.y + vector2.y - 3.5f;
        sprites[CornerSprite(3)].x = vector.x + vector2.x - 3.5f;
        sprites[CornerSprite(3)].y = vector.y + 3.5f;
        Color color = InventoryConfig.cursorColorConfig.Value;
        if (InventoryConfig.rainbowBool.Value)
        {
            color = Custom.HSL2RGB(Mathf.Lerp(0f, 1f, Mathf.PingPong(Time.time * 0.7f, 1)), 0.85f, 0.65f);
        }
        for (int j = 0; j < 4; j++)
        {
            sprites[SideSprite(j)].color = color;
            sprites[CornerSprite(j)].color = color;
        }
        sprites[FillSideSprite(0)].x = vector.x + 4f;
        sprites[FillSideSprite(0)].y = vector.y + 7f;
        sprites[FillSideSprite(0)].scaleY = vector2.y - 14f;
        sprites[FillSideSprite(1)].x = vector.x + 7f;
        sprites[FillSideSprite(1)].y = vector.y + vector2.y - 4f;
        sprites[FillSideSprite(1)].scaleX = vector2.x - 14f;
        sprites[FillSideSprite(2)].x = vector.x + vector2.x - 4f;
        sprites[FillSideSprite(2)].y = vector.y + 7f;
        sprites[FillSideSprite(2)].scaleY = vector2.y - 14f;
        sprites[FillSideSprite(3)].x = vector.x + 7f;
        sprites[FillSideSprite(3)].y = vector.y + 4f;
        sprites[FillSideSprite(3)].scaleX = vector2.x - 14f;
        sprites[FillCornerSprite(0)].x = vector.x + 3.5f;
        sprites[FillCornerSprite(0)].y = vector.y + 3.5f;
        sprites[FillCornerSprite(1)].x = vector.x + 3.5f;
        sprites[FillCornerSprite(1)].y = vector.y + vector2.y - 3.5f;
        sprites[FillCornerSprite(2)].x = vector.x + vector2.x - 3.5f;
        sprites[FillCornerSprite(2)].y = vector.y + vector2.y - 3.5f;
        sprites[FillCornerSprite(3)].x = vector.x + vector2.x - 3.5f;
        sprites[FillCornerSprite(3)].y = vector.y + 3.5f;
        sprites[MainFillSprite].x = vector.x + 7f;
        sprites[MainFillSprite].y = vector.y + 7f;
        sprites[MainFillSprite].scaleX = vector2.x - 14f;
        sprites[MainFillSprite].scaleY = vector2.y - 14f;
        itemSprite.x = vector.x + inventory.slotSize / 2;
        itemSprite.y = vector.y + inventory.slotSize / 2;
        //Fading
        if (inventory.isShown)
        {
            if (inventory.fade > 0.4f)
            {
                //Edge sprites
                if(storedItem != null && !isSelected)
                {
                    sprites[SideSprite(0)].alpha = 0.4f;
                    sprites[SideSprite(2)].alpha = 0.4f;
                    sprites[SideSprite(1)].alpha = 0.4f;
                    sprites[SideSprite(3)].alpha = 0.4f;
                    sprites[CornerSprite(0)].alpha = 0.4f;
                    sprites[CornerSprite(2)].alpha = 0.4f;
                    sprites[CornerSprite(1)].alpha = 0.4f;
                    sprites[CornerSprite(3)].alpha = 0.4f;
                }
                else
                {
                    sprites[SideSprite(0)].alpha = fade;
                    sprites[SideSprite(2)].alpha = fade;
                    sprites[SideSprite(1)].alpha = fade;
                    sprites[SideSprite(3)].alpha = fade;
                    sprites[CornerSprite(0)].alpha = fade;
                    sprites[CornerSprite(2)].alpha = fade;
                    sprites[CornerSprite(1)].alpha = fade;
                    sprites[CornerSprite(3)].alpha = fade;
                }
                if (storedItem != null)
                {
                    if (isSelected && InventoryConfig.rainbowBool.Value)
                    {
                        sprites[SideSprite(0)].color = color;
                        sprites[SideSprite(2)].color = color;
                        sprites[SideSprite(1)].color = color;
                        sprites[SideSprite(3)].color = color;
                        sprites[CornerSprite(0)].color = color;
                        sprites[CornerSprite(2)].color = color;
                        sprites[CornerSprite(1)].color = color;
                        sprites[CornerSprite(3)].color = color;
                    }
                    else
                    {
                        sprites[SideSprite(0)].color = Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.8f);
                        sprites[SideSprite(2)].color = Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.8f);
                        sprites[SideSprite(1)].color = Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.8f);
                        sprites[SideSprite(3)].color = Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.8f);
                        sprites[CornerSprite(0)].color = Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.8f);
                        sprites[CornerSprite(2)].color = Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.8f);
                        sprites[CornerSprite(1)].color = Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.8f);
                        sprites[CornerSprite(3)].color = Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.8f);
                    }
                }
                else
                {
                    sprites[SideSprite(0)].color = color;
                    sprites[SideSprite(2)].color = color;
                    sprites[SideSprite(1)].color = color;
                    sprites[SideSprite(3)].color = color;
                    sprites[CornerSprite(0)].color = color;
                    sprites[CornerSprite(2)].color = color;
                    sprites[CornerSprite(1)].color = color;
                    sprites[CornerSprite(3)].color = color;
                }
                //Fill sprites
                for (int j = 0; j < 9; j++)
                {
                    if (storedItem != null)
                    {
                        if (InventoryConfig.rainbowBool.Value && isSelected)
                        {
                            sprites[j].color = Color.Lerp(Color.Lerp(new Color(0f, 0f, 0f), color, 0.1f), color, fade * 0.5f);
                        }
                        else
                        {
                            sprites[j].color = Color.Lerp(Color.Lerp(new Color(0f, 0f, 0f), storedItem.spriteColor, 0.1f), storedItem.spriteColor, fade * 0.5f);
                        }
                        if (isSelected)
                        {
                            sprites[j].alpha = Mathf.Lerp(0f, 0.45f, fade);
                        }
                        else
                        {
                            sprites[j].alpha = Mathf.Lerp(0f, 0.45f, inventory.fade);
                        }
                    }
                    else
                    {
                        sprites[j].color = new Color(0f, 0f, 0f);
                        sprites[j].alpha = Mathf.Lerp(0f, 0.35f, inventory.fade);
                    }
                }
                //Item sprite
                if (storedItem != null)
                {
                    itemSprite.alpha = Mathf.Lerp(0.74f, 1f, fade);
                }
                else
                {
                    itemSprite.alpha = 0f;
                }
            }
        }
        else
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i].alpha > 0f)
                {
                    sprites[i].alpha -= 3f * Time.deltaTime;
                }
            }
            itemSprite.alpha -= 3f * Time.deltaTime;
        }
    }

    public Vector2 DrawPos(float timeStacker)
    {
        return pos;
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
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].RemoveFromContainer();
        }
        itemSprite.RemoveFromContainer();
    }
}