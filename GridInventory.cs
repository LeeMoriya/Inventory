using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HUD;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;


    //The main Inventory HUD piece that contains individual Inventory slots
    //Controls and displaying of the Inventory are done in here

    public class GridInventory : HudPart
    {
        public IntVector2 invSize = InventoryData.invSize;
        public InventorySlot[] inventorySlots;
        public bool showMap = false;
        public Vector2 pos = new Vector2(400f, 200f);
        public float slotSize = 50f;
        public float slotOffset = 60f;
        public bool isShown;
        public IntVector2 cursorPos = new IntVector2(0, 0);
        public int inputDelay = 0;
        public float fade = 0f;
        public GridInventory(HUD.HUD hud) : base(hud)
        {
            this.hud = hud;
            this.pos = hud.owner.MapOwnerInRoomPosition;
            //Generate inventory slots based on the invSize IntVector
            List<InventorySlot> slots = new List<InventorySlot>();
            for (int x = 0; x < invSize.x; x++)
            {
                for (int y = 0; y < invSize.y; y++)
                {
                    slots.Add(new InventorySlot(this, this.hud, new IntVector2(x, y)));
                }
            }
            this.inventorySlots = slots.ToArray();
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                this.hud.AddPart(inventorySlots[i]);
                if (InventoryData.storedObjects != null)
                {
                    for (int o = 0; o < InventoryData.storedObjects.Count; o++)
                    {
                        if (InventoryData.storedObjects[o].index == i)
                        {
                            inventorySlots[i].storedItem = InventoryData.storedObjects[o];
                        }
                    }
                }
            }
            //this.hud.AddPart(new CapacityGauge(this, this.hud));
        }

        //Store and Retrieve items from the selected slot
        public void StoreItem()
        {
            //Get highlighted slot
            InventorySlot activeSlot = null;
            int index = -1;
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].slotNum == cursorPos)
                {
                    activeSlot = inventorySlots[i];
                    index = i;
                }
            }
            if (activeSlot != null)
            {
                Player player = this.hud.owner as Player;
                //If this slot has no item...
                if (activeSlot.storedItem == null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (player.grasps[i] != null)
                        {
                            AbstractPhysicalObject apo = player.grasps[i].grabbed.abstractPhysicalObject;
                            //Carried object is a creature
                            if (apo.type == AbstractPhysicalObject.AbstractObjectType.Creature)
                            {
                                activeSlot.storedItem = InventoryData.NewStoredObject(null, apo as AbstractCreature, index);
                                player.room.RemoveObject((apo as AbstractCreature).realizedCreature);
                            }
                            //Carried object is an item
                            else
                            {
                                activeSlot.storedItem = InventoryData.NewStoredObject(apo, null, index);
                                apo.destroyOnAbstraction = true;
                                apo.Abstractize(apo.pos);
                            }
                            //Release grasp and assign slot's sprite and color
                            player.grasps[i].Release();
                            activeSlot.itemSprite.SetElementByName(activeSlot.storedItem.spriteName);
                            activeSlot.itemSprite.color = activeSlot.storedItem.spriteColor;
                            break;
                        }
                    }
                    (this.hud.owner as Player).room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, (this.hud.owner as Player).mainBodyChunk, false, 2f, 0.9f);
                }
                else
                {
                    //Check for free grasps
                    int freeGrasp = 2;
                    for (int i = 0; i < player.grasps.Length; i++)
                    {
                        if (player.grasps[i] != null)
                        {
                            freeGrasp--;
                        }
                    }
                    if (freeGrasp == 0)
                    {
                        this.hud.PlaySound(SoundID.MENU_Error_Ping);
                        return;
                    }
                    AbstractPhysicalObject apo;
                    //Re-create creature from string
                    if (activeSlot.storedItem.type == AbstractPhysicalObject.AbstractObjectType.Creature)
                    {
                        string[] array = Regex.Split(activeSlot.storedItem.data, "<cA>");
                        EntityID id = EntityID.FromString(array[1]);
                        apo = new AbstractCreature(player.room.world, StaticWorld.GetCreatureTemplate(activeSlot.storedItem.critType), null, player.coord, id);
                        (apo as AbstractCreature).state.LoadFromString(Regex.Split(array[3], "<cB>"));
                    }
                    //Re-create item from string
                    else
                    {
                        apo = SaveState.AbstractPhysicalObjectFromString(player.room.world, activeSlot.storedItem.data);
                    }
                    //Place the object in the room and make Slugcat grab it with a free hand
                    player.room.abstractRoom.AddEntity(apo);
                    apo.pos = player.abstractCreature.pos;
                    apo.RealizeInRoom();
                    if (apo.realizedObject != null)
                    {
                        apo.realizedObject.firstChunk.HardSetPosition(player.mainBodyChunk.pos);
                        for (int i = 0; i < 2; i++)
                        {
                            if (player.grasps[i] == null)
                            {
                                if (player.CanIPickThisUp(apo.realizedObject))
                                {
                                    player.SlugcatGrab(apo.realizedObject, i);
                                }
                            }
                        }
                    }
                    //Empty slot after retrieving item
                    this.hud.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back);
                    InventoryData.RemoveStoredObject(activeSlot.storedItem.index);
                    activeSlot.storedItem = null;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                Debug.Log("Inventory contains:");
                for (int i = 0; i < InventoryData.storedObjects.Count; i++)
                {
                    if (InventoryData.storedObjects[i] != null)
                    {
                        InventoryData.StoredObject obj = InventoryData.storedObjects[i];
                        string text = "";
                        text += obj.index.ToString() + " - ";
                        if (obj.critType != default)
                        {
                            text += obj.critType.ToString();
                        }
                        if (obj.type != default)
                        {
                            text += obj.type.ToString();
                        }
                        Debug.Log(text);
                    }
                }
                string saveTest = InventoryData.SaveString();
                Debug.Log(saveTest);
                InventoryData.test = saveTest;
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                InventoryData.LoadString(InventoryData.test);
            }
            Player player = (this.hud.owner as Player);
            if (player != null && player.room != null)
            {
                if (InventoryMod.invInput[0].mp && !showMap)
                {
                    this.isShown = true;
                }
                else
                {
                    this.isShown = false;
                }
                if (!InventoryMod.invInput[0].mp)
                {
                    this.showMap = false;
                    this.isShown = false;
                }
                //Position
                if (isShown)
                {
                    this.pos.x = player.mainBodyChunk.pos.x - player.room.game.cameras[0].pos.x;
                    this.pos.y = player.mainBodyChunk.pos.y - player.room.game.cameras[0].pos.y;
                    this.pos.x -= (this.slotOffset * (this.invSize.x * 0.5f)) - this.slotSize * 0.5f;
                    this.pos.y += 65f;
                    this.pos.x += 5f;
                    this.pos.x = Mathf.RoundToInt(this.pos.x);
                    this.pos.y = Mathf.RoundToInt(this.pos.y);
                    if (this.fade < 1f)
                    {
                        this.fade += 5f * Time.deltaTime;
                    }
                }
                else
                {
                    if (this.fade > 0f)
                    {
                        this.fade -= 5.5f * Time.deltaTime;
                    }
                }

                //Tick down input delay
                if (inputDelay > 0)
                {
                    inputDelay--;
                }
                if (InventoryMod.invInput[0].mp)
                {
                    //Toggle map
                    if (InventoryMod.invInput[0].pckp && inputDelay <= 0)
                    {
                        if (showMap)
                        {
                            showMap = false;
                        }
                        else
                        {
                            showMap = true;
                            this.hud.PlaySound(SoundID.Slugcat_Ghost_Appear);
                        }
                        inputDelay = 12;
                    }
                    //Player cannot manage inventory unless it has fully appeared on-screen.
                    //This prevents the player accidently triggering an action the moment they open the menu.
                    //This doesn't apply to opening the map however.
                    if (showMap || this.fade < 1f)
                    {
                        return;
                    }
                    //Select slot
                    if (InventoryMod.invInput[0].jmp && !InventoryMod.invInput[1].jmp)
                    {
                        inputDelay = 15;
                        StoreItem();
                    }
                    //Left
                    if ((InventoryMod.invInput[0].x < 0f && InventoryMod.invInput[1].x == 0f) || (InventoryMod.invInput[0].x < 0f && inputDelay <= 0))
                    {
                        if (cursorPos.x == 0)
                        {
                            cursorPos.x = invSize.x - 1;
                        }
                        else
                        {
                            cursorPos.x--;
                        }
                        inputDelay = 18;
                        this.hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    }
                    //Right
                    if ((InventoryMod.invInput[0].x > 0f && InventoryMod.invInput[1].x == 0f) || (InventoryMod.invInput[0].x > 0f && inputDelay <= 0))
                    {
                        if (cursorPos.x == invSize.x - 1)
                        {
                            cursorPos.x = 0;
                        }
                        else
                        {
                            cursorPos.x++;
                        }
                        inputDelay = 18;
                        this.hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    }
                    //Up
                    if ((InventoryMod.invInput[0].y > 0f && InventoryMod.invInput[1].y == 0f) || (InventoryMod.invInput[0].y > 0f && inputDelay <= 0))
                    {
                        if (cursorPos.y == invSize.y - 1)
                        {
                            cursorPos.y = 0;
                        }
                        else
                        {
                            cursorPos.y++;
                        }
                        inputDelay = 18;
                        this.hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    }
                    //Down
                    if ((InventoryMod.invInput[0].y < 0f && InventoryMod.invInput[1].y == 0f) || (InventoryMod.invInput[0].y < 0f && inputDelay <= 0))
                    {
                        if (cursorPos.y == 0)
                        {
                            cursorPos.y = invSize.y - 1;
                        }
                        else
                        {
                            cursorPos.y--;
                        }
                        inputDelay = 18;
                        this.hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    }
                }
            }
            for (int i = 0; i < this.inventorySlots.Length; i++)
            {
                if (this.cursorPos == this.inventorySlots[i].slotNum)
                {
                    this.inventorySlots[i].isSelected = true;
                }
                else
                {
                    this.inventorySlots[i].isSelected = false;
                }
            }
        }
    }

    public class CapacityGauge : HudPart
    {
        public GridInventory inventory;
        public FSprite bar;
        public float capacity;
        public CapacityGauge(GridInventory inventory, HUD.HUD hud) : base(hud)
        {
            this.inventory = inventory;
            this.InitiateSprites();
        }

        public override void Update()
        {
            base.Update();
            float filled = 0f;
            for (int i = 0; i < this.inventory.inventorySlots.Length; i++)
            {
                if (this.inventory.inventorySlots[i].storedItem != null)
                {
                    filled += 1f;
                }
            }
            this.capacity = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, (float)this.inventory.inventorySlots.Length, filled));
            if (Input.GetKey(KeyCode.Alpha5))
            {
                Debug.Log(this.inventory.inventorySlots.Length.ToString() + " " + filled.ToString());
            }
        }

        public void InitiateSprites()
        {
            this.bar = new FSprite("Futile_White", true);
            this.bar.scaleY = 50f;
            this.bar.scaleX = 50f;
            this.bar.shader = this.hud.rainWorld.Shaders["HoldButtonCircle"];
            this.bar.rotation = 90f;
            this.hud.fContainers[1].AddChild(this.bar);
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            Vector2 invPos = new Vector2(this.inventory.pos.x + (inventory.slotSize * inventory.invSize.x / 2), this.inventory.pos.y + 200f);
            this.bar.x = invPos.x;
            this.bar.y = invPos.y;
            this.bar.scaleY = Mathf.Lerp(0f, 10f, this.inventory.fade);
            this.bar.scaleX = Mathf.Lerp(0f, 10f, this.inventory.fade);
            this.bar.alpha = this.capacity;
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                this.bar.rotation += 10f;
                Debug.Log(this.bar.rotation);
            }
        }
    }

    public class InventorySlot : HudPart
    {
        public GridInventory inventory;
        public InventoryData.StoredObject storedItem;
        public IntVector2 slotNum;
        public Vector2 pos;
        public FSprite[] sprites;
        public FSprite itemSprite;
        public FLabel label;
        public bool isSelected = false;
        public float fade = 1f;
        public InventorySlot(GridInventory inventory, HUD.HUD hud, IntVector2 slotNum) : base(hud)
        {
            this.inventory = inventory;
            this.slotNum = slotNum;
            this.pos = SlotPos(this.inventory.pos);
            InitiateSprites();
        }

        public override void Update()
        {
            base.Update();
            this.pos = SlotPos(this.inventory.pos);
            if (this.isSelected)
            {
                this.fade = 1f;
            }
            else
            {
                if (this.fade > 0f)
                {
                    this.fade -= 3f * Time.deltaTime;
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

