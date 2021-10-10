using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HUD;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;

public class Inventory : HudPart
{
    public IntVector2 invSize = InventoryData.invSize;
    public int selectedSlot = 2;
    public int invSlots = InventoryData.invSlots;
    public InventorySlot[] inventorySlots;
    public bool showMap = false;
    public Vector2 pos = new Vector2(400f, 200f);
    public float slotSize = 50f;
    public float slotOffset = 60f;
    public bool isShown;
    public IntVector2 cursorPos = new IntVector2(0, 0);
    public int inputDelay = 0;
    public float fade = 0f;

    public Inventory(HUD.HUD hud) : base(hud)
    {

    }

    public void StoreItem()
    {
        //Get highlighted slot
        InventorySlot activeSlot = null;
        int index = -1;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (this is GridInventory)
            {
                if (inventorySlots[i].slotNum == cursorPos)
                {
                    activeSlot = inventorySlots[i];
                    index = i;
                }
            }
            if(this is RadialInventory)
            {
                activeSlot = inventorySlots[selectedSlot];
                index = selectedSlot;
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
                        Debug.Log("MASS: " + apo.realizedObject.TotalMass.ToString());
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
}

public class GridInventory : Inventory
{
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

    public override void Update()
    {
        base.Update();
        Player player = (this.hud.owner as Player);
        if (player != null && player.room != null)
        {
            //Holding Map
            if (InventoryMod.invInput[0].mp && !showMap)
            {
                this.isShown = true;
            }
            else
            {
                this.isShown = false;
            }
            //Release Map
            if (!InventoryMod.invInput[0].mp)
            {
                this.showMap = false;
                this.isShown = false;
            }
            //Position
            if (isShown)
            {
                Vector2 camPos = player.room.game.cameras[0].pos;
                Vector2 playerPos = player.mainBodyChunk.pos;
                //Default position
                this.pos.x = playerPos.x - camPos.x;
                this.pos.y = playerPos.y - camPos.y;
                this.pos.x -= (this.slotOffset * (this.invSize.x * 0.5f)) - this.slotSize * 0.5f;
                this.pos.y += 65f;
                this.pos.x += 5f;
                //Clamped position
                this.pos.y = Mathf.Clamp(this.pos.y, (this.slotOffset - this.slotSize) * 3, 800f - (this.slotOffset * this.invSize.y));
                this.pos.x = Mathf.Clamp(this.pos.x, (this.slotOffset - this.slotSize) * 3, 1400f - (this.slotOffset * this.invSize.x));

                this.pos.x = Mathf.RoundToInt(this.pos.x);
                this.pos.y = Mathf.RoundToInt(this.pos.y);
                for (int i = 0; i < inventorySlots.Length; i++)
                {
                    inventorySlots[i].pos = inventorySlots[i].SlotPos(this.pos);
                }
                if (this.fade < 1f)
                {
                    this.fade += 10f * Time.deltaTime;
                }
            }
            else
            {
                if (this.fade > 0f)
                {
                    this.fade -= 10f * Time.deltaTime;
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
                if (showMap || this.fade < 0.5f)
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

public class RadialInventory : Inventory
{
    public RadialInventory(HUD.HUD hud) : base(hud)
    {
        this.hud = hud;
        this.pos = hud.owner.MapOwnerInRoomPosition;
        //Generate inventory slots based on the invSize IntVector
        this.inventorySlots = new InventorySlot[invSlots];
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i] = new InventorySlot(this, this.hud, new IntVector2(i, 0));
            this.hud.AddPart(inventorySlots[i]);
            //Load stored objects by index
            if (InventoryData.storedObjects != null)
            {
                for (int o = 0; o < InventoryData.storedObjects.Count; o++)
                {
                    inventorySlots[i].storedItem = InventoryData.storedObjects[o];
                }
            }
        }
    }

    public Vector2 RadialPos(int index)
    {
        int pos = 0;
        float offset = 0f;
        if(index < selectedSlot)
        {
            pos = selectedSlot - index;
            offset = -52f * pos;
        }
        if (index > selectedSlot)
        {
            pos = index - selectedSlot;
            offset = 52f * pos;
        }
        return this.pos + new Vector2(offset, 0f);
    }

    public override void Update()
    {
        base.Update();
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
                this.pos.y = player.mainBodyChunk.pos.y - player.room.game.cameras[0].pos.y + 50f;
                this.pos.x = Mathf.RoundToInt(this.pos.x);
                this.pos.y = Mathf.RoundToInt(this.pos.y);

                //Slot positions
                for (int i = 0; i < inventorySlots.Length; i++)
                {
                    inventorySlots[i].pos = RadialPos(i);
                }

                if (this.fade < 1f)
                {
                    this.fade += 10f * Time.deltaTime;
                }
            }
            else
            {
                if (this.fade > 0f)
                {
                    this.fade -= 10f * Time.deltaTime;
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
                //Left
                if ((InventoryMod.invInput[0].x < 0f && InventoryMod.invInput[1].x == 0f) || (InventoryMod.invInput[0].x < 0f && inputDelay <= 0))
                {
                    selectedSlot--;
                    if(selectedSlot == -1)
                    {
                        selectedSlot = inventorySlots.Length - 1;
                    }
                    for (int i = 0; i < inventorySlots.Length; i++)
                    {
                        if (i == selectedSlot)
                        {
                            inventorySlots[i].isSelected = true;
                        }
                        else
                        {
                            inventorySlots[i].isSelected = false;
                        }
                    }
                    inputDelay = 18;
                    this.hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    Debug.Log("Slot: " + selectedSlot);
                }
                //Right
                if ((InventoryMod.invInput[0].x > 0f && InventoryMod.invInput[1].x == 0f) || (InventoryMod.invInput[0].x > 0f && inputDelay <= 0))
                {
                    selectedSlot++;
                    if (selectedSlot >= inventorySlots.Length)
                    {
                        selectedSlot = 0;
                    }
                    for (int i = 0; i < inventorySlots.Length; i++)
                    {
                        if(i == selectedSlot)
                        {
                            inventorySlots[i].isSelected = true;
                        }
                        else
                        {
                            inventorySlots[i].isSelected = false;
                        }
                    }
                    inputDelay = 18;
                    this.hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    Debug.Log("Slot: " + selectedSlot);
                }
                //Player cannot manage inventory unless it has fully appeared on-screen.
                //This prevents the player accidently triggering an action the moment they open the menu.
                //This doesn't apply to opening the map however.
                if (showMap || this.fade < 0.5f)
                {
                    return;
                }
                //Select slot
                if (InventoryMod.invInput[0].jmp && !InventoryMod.invInput[1].jmp)
                {
                    inputDelay = 15;
                    StoreItem();
                }
            }
        }
    }
}
