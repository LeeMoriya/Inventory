using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HUD;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;
using System.Reflection;

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
    public Player.InputPackage invInput = new Player.InputPackage();
    public RainWorldGame game;
    public Player player;

    public Inventory(HUD.HUD hud, RainWorldGame game) : base(hud)
    {
        this.game = game;
    }


    public bool CanIStoreThis(InventorySlot activeSlot, int index)
    {
        //Check on hand at a time
        for (int i = 0; i < 2; i++)
        {
            //Player is holding something
            if (player.grasps[i] != null)
            {
                AbstractPhysicalObject apo;
                if (player.grasps[i].grabbed.abstractPhysicalObject != null)
                {
                    apo = player.grasps[i].grabbed.abstractPhysicalObject;
                }
                else
                {
                    Debug.Log("INV: Item has no Abstract version");
                    return false;
                }

                //Carried object is a creature
                if (apo.type == AbstractPhysicalObject.AbstractObjectType.Creature)
                {
                    ////Test that the object is valid
                    //bool invalid = false;
                    //AbstractCreature test = null;
                    //try
                    //{
                    //    test = SaveState.AbstractCreatureFromString(apo.world, apo.ToString(), false);
                    //}
                    //catch { invalid = true; }
                    //if (invalid || test == null)
                    //{
                    //    Debug.LogException(new Exception("INV: Attempted to store invalid object: " + apo.type.value));
                    //    return false;
                    //}

                    //Don't store the creature if the option is disabled, or its a Player
                    if (!InventoryConfig.critBool.Value || (apo as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                    {
                        return false;
                    }
                }
                //Carried object is an item
                else
                {
                    //Test that the object is valid
                    bool invalid = false;
                    AbstractPhysicalObject test = null;
                    try
                    {
                        test = SaveState.AbstractPhysicalObjectFromString(apo.world, apo.ToString());
                    }
                    catch { invalid = true; }
                    if (invalid || test == null)
                    {
                        Debug.LogException(new Exception("INV: Attempted to store invalid object: " + apo.type.value));
                        return false;
                    }

                    if (apo.type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower && !InventoryConfig.karmaBool.Value)
                    {
                        return false;
                    }

                    //Ugly hardcoded SeedCob fix
                    if(apo.type.value == "SeedCob")
                    {
                        Debug.Log("RotundWorld SeedCobs break Inventory, sorry... you can't store this!");
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }


    public void RetrieveItem(InventorySlot activeSlot)
    {
        //Check for free grasps
        int freeGrasp = 2;
        //bool twoHands = false;
        for (int i = 0; i < player.grasps.Length; i++)
        {
            if (player.grasps[i] != null)
            {
                freeGrasp--;
            }
        }
        if (freeGrasp == 0)
        {
            hud.PlaySound(SoundID.MENU_Error_Ping);
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
            Debug.Log("RECREATE FROM SAVE STRING");
            apo = SaveState.AbstractPhysicalObjectFromString(player.room.world, activeSlot.storedItem.data);
            if (apo is AbstractConsumable)
            {
                (apo as AbstractConsumable).Consume();
            }
        }

        //Place the object in the room and make Slugcat grab it with a free hand
        Debug.Log("ADD ENTITY");
        player.room.abstractRoom.AddEntity(apo);
        apo.pos = player.abstractCreature.pos;
        Debug.Log("REALIZE OBJECT");
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
                        //Spear check
                        bool hasSpear = false;
                        for (int s = 0; s < 2; s++)
                        {
                            if (player.grasps[s] != null && player.grasps[s].grabbed != null)
                            {
                                if (player.grasps[s].grabbed is Spear)
                                {
                                    Debug.Log("HAS SPEAR");
                                    hasSpear = true;
                                }
                            }
                        }
                        if (hasSpear && apo.type == AbstractPhysicalObject.AbstractObjectType.Spear && player.spearOnBack != null && !player.spearOnBack.HasASpear)
                        {
                            player.spearOnBack.SpearToBack(apo.realizedObject as Spear);
                        }
                        else
                        {
                            Debug.Log("SLUGCAT GRAB");
                            player.SlugcatGrab(apo.realizedObject, i);
                        }
                    }
                }
            }
        }
        //Empty slot after retrieving item
        hud.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back);
        InventoryData.RemoveStoredObject(activeSlot.storedItem.index);
        activeSlot.storedItem = null;
    }

    public void InteractWithSlot()
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
        }
        if (activeSlot != null)
        {
            //STORE ITEM
            if (activeSlot.storedItem == null)
            {
                if (CanIStoreThis(activeSlot, index))
                {
                    StoreItem(activeSlot, index);
                }
            }
            else
            {
                RetrieveItem(activeSlot);
            }
        }
    }

    public void StoreItem(InventorySlot activeSlot, int index)
    {
        for (int i = 0; i < 2; i++)
        {
            if (player.grasps[i] != null)
            {
                AbstractPhysicalObject apo = player.grasps[i].grabbed.abstractPhysicalObject;

                //Carried object is a creature
                if (apo.type == AbstractPhysicalObject.AbstractObjectType.Creature)
                {
                    //Create a new storedObject for this creature
                    activeSlot.storedItem = InventoryData.NewStoredObject(null, apo as AbstractCreature, index);
                    if (activeSlot.storedItem == null)
                    {
                        Debug.LogException(new Exception("INV: Attempted to store invalid object: " + apo.type.value));
                        hud.PlaySound(SoundID.MENU_Error_Ping);
                        return;
                    }
                    else
                    {
                        Debug.Log("INV: " + (activeSlot.storedItem.type != null ? activeSlot.storedItem.type.value : "Null apo type"));
                        Debug.Log("INV: " + (activeSlot.storedItem.critType != null ? activeSlot.storedItem.critType.value : "Null crit type"));
                    }
                    try
                    {
                        (apo as AbstractCreature).realizedCreature.RemoveFromRoom();
                        apo.Room.RemoveEntity(apo as AbstractCreature);
                        (apo as AbstractCreature).realizedCreature.Destroy();
                        (player.room.game.session as StoryGameSession).saveState.waitRespawnCreatures.Add((apo as AbstractCreature).ID.spawner);
                    }
                    catch
                    {
                        Debug.LogException(new Exception("Failed to remove Creature from world when storing object, it may not support being Abstracized or was not created from a Spawner"));
                        hud.PlaySound(SoundID.MENU_Error_Ping);
                        return;
                    }
                }
                //Carried object is an item
                else
                {
                    try
                    {
                        //Store the object
                        activeSlot.storedItem = InventoryData.NewStoredObject(apo, null, index);
                        if (activeSlot.storedItem == null)
                        {
                            Debug.LogException(new Exception("INV: Attempted to store invalid object: " + apo.type.value));
                            hud.PlaySound(SoundID.MENU_Error_Ping);
                            return;
                        }
                        //Remove it from the room
                        apo.Room.entities.Remove(apo);
                        apo.destroyOnAbstraction = true;
                        apo.Abstractize(apo.pos);

                        ////Try immediately re-adding it
                        //player.room.abstractRoom.AddEntity(apo);
                        //apo.pos = player.abstractCreature.pos;
                        //apo.RealizeInRoom();

                        ////Remove it again
                        //apo.Room.entities.Remove(apo);
                        //apo.destroyOnAbstraction = true;
                        //apo.Abstractize(apo.pos);
                    }
                    catch
                    {
                        //If it fails at any point, don't store the object
                        InventoryData.RemoveStoredObject(index);
                        return;
                    }
                }
                //Release grasp and assign slot's sprite and color
                player.grasps[i].Release();
                activeSlot.itemSprite.SetElementByName(activeSlot.storedItem.spriteName);
                activeSlot.itemSprite.color = activeSlot.storedItem.spriteColor;
                break;
            }
        }
        player.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, player.mainBodyChunk, false, 2f, 0.9f);
    }
}

public class GridInventory : Inventory
{
    public GridInventory(HUD.HUD hud, RainWorldGame game) : base(hud, game)
    {
        this.hud = hud;
        this.game = game;
        pos = hud.owner.MapOwnerInRoomPosition;
        //Generate inventory slots based on the invSize IntVector
        List<InventorySlot> slots = new List<InventorySlot>();
        for (int x = 0; x < invSize.x; x++)
        {
            for (int y = 0; y < invSize.y; y++)
            {
                slots.Add(new InventorySlot(this, hud, new IntVector2(x, y)));
            }
        }
        inventorySlots = slots.ToArray();
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            hud.AddPart(inventorySlots[i]);
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
        //hud.AddPart(new CapacityGauge(this, hud));
    }

    public override void Update()
    {
        base.Update();

        KeyCode key = KeyCode.None;
        for (int i = 0; i < game.AlivePlayers.Count; i++)
        {
            switch (i)
            {
                case 0:
                    if(InventoryConfig.invKey1.Value != KeyCode.None)
                    {
                        key = InventoryConfig.invKey1.Value;
                    }
                    break;
                case 1:
                    if (InventoryConfig.invKey2.Value != KeyCode.None)
                    {
                        key = InventoryConfig.invKey2.Value;
                    }
                    break;
                case 2:
                    if (InventoryConfig.invKey3.Value != KeyCode.None)
                    {
                        key = InventoryConfig.invKey3.Value;
                    }
                    break;
                case 3:
                    if (InventoryConfig.invKey4.Value != KeyCode.None)
                    {
                        key = InventoryConfig.invKey4.Value;
                    }
                    break;
            }

            //If the player is holding map, or if a custom keybind has been set and is being held
            if (game.AlivePlayers[i].realizedCreature != null && ((game.AlivePlayers[i].realizedCreature as Player).mapInput.mp || (key != KeyCode.None && Input.GetKey(key))))
            {
                player = (game.AlivePlayers[i].realizedCreature as Player);
                if(key != KeyCode.None && Input.GetKey(key))
                {
                    showMap = false;
                }
                break;
            }
        }

        if (player != null && player.room != null)
        {
            //Holding Map
            if ((player.mapInput.mp && !showMap && key == KeyCode.None) || !player.mapInput.mp && key != KeyCode.None && Input.GetKey(key))
            {
                isShown = true;
                
            }
            else
            {
                isShown = false;
            }
            //Release Map
            if (!player.mapInput.mp && key == KeyCode.None)
            {
                isShown = false;
            }
            //Toggle map
            if (player.mapInput.mp && player.mapInput.pckp && inputDelay <= 0 && key == KeyCode.None)
            {
                if (showMap)
                {
                    showMap = false;
                }
                else
                {
                    showMap = true;
                    hud.PlaySound(SoundID.Slugcat_Ghost_Appear);
                }
                inputDelay = 12;
            }
            //Position
            if (isShown)
            {
                Vector2 camPos = player.room.game.cameras[0].pos;
                Vector2 playerPos = player.mainBodyChunk.pos;
                //Default position
                pos.x = playerPos.x - camPos.x;
                pos.y = playerPos.y - camPos.y;
                pos.x -= (slotOffset * (invSize.x * 0.5f)) - slotSize * 0.5f;
                pos.y += 65f;
                pos.x += 5f;
                //Clamped position
                pos.y = Mathf.Clamp(pos.y, (slotOffset - slotSize) * 3, 800f - (slotOffset * invSize.y));
                pos.x = Mathf.Clamp(pos.x, (slotOffset - slotSize) * 3, 1400f - (slotOffset * invSize.x));

                pos.x = Mathf.RoundToInt(pos.x);
                pos.y = Mathf.RoundToInt(pos.y);
                for (int i = 0; i < inventorySlots.Length; i++)
                {
                    inventorySlots[i].pos = inventorySlots[i].SlotPos(pos);
                }
                if (fade < 1f)
                {
                    fade += 10f * Time.deltaTime;
                }

                //Player cannot manage inventory unless it has fully appeared on-screen.
                //This prevents the player accidently triggering an action the moment they open the menu.
                //This doesn't apply to opening the map however.
                if (showMap || fade < 0.5f)
                {
                    return;
                }
                //Select slot
                if (player.mapInput.jmp && !invInput.jmp)
                {
                    inputDelay = 15;
                    InteractWithSlot();
                }
                //Left
                if ((player.mapInput.x < 0f && invInput.x == 0f) || (player.mapInput.x < 0f && inputDelay <= 0))
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
                    hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                }
                //Right
                if ((player.mapInput.x > 0f && invInput.x == 0f) || (player.mapInput.x > 0f && inputDelay <= 0))
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
                    hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                }
                //Up
                if ((player.mapInput.y > 0f && invInput.y == 0f) || (player.mapInput.y > 0f && inputDelay <= 0))
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
                    hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                }
                //Down
                if ((player.mapInput.y < 0f && invInput.y == 0f) || (player.mapInput.y < 0f && inputDelay <= 0))
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
                    hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                }
            }
            else
            {
                if (fade > 0f)
                {
                    fade -= 10f * Time.deltaTime;
                }
            }
            //Tick down input delay
            if (inputDelay > 0)
            {
                inputDelay--;
            }
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (cursorPos == inventorySlots[i].slotNum)
            {
                inventorySlots[i].isSelected = true;
            }
            else
            {
                inventorySlots[i].isSelected = false;
            }
        }
        if (player != null)
        {
            invInput = player.mapInput;
        }
    }
}

public class RadialInventory : Inventory
{
    public RadialInventory(HUD.HUD hud, RainWorldGame game) : base(hud, game)
    {
        this.hud = hud;
        this.game = game;
        pos = hud.owner.MapOwnerInRoomPosition;
        //Generate inventory slots based on the invSize IntVector
        inventorySlots = new InventorySlot[invSlots];
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i] = new InventorySlot(this, hud, new IntVector2(i, 0));
            hud.AddPart(inventorySlots[i]);
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
        int pos;
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
        Player player = (hud.owner as Player);
        if (player != null && player.room != null)
        {
            if (player.mapInput.mp && !showMap)
            {
                isShown = true;
            }
            else
            {
                isShown = false;
            }
            if (!player.mapInput.mp)
            {
                showMap = false;
                isShown = false;
            }
            //Position
            if (isShown)
            {
                pos.x = player.mainBodyChunk.pos.x - player.room.game.cameras[0].pos.x;
                pos.y = player.mainBodyChunk.pos.y - player.room.game.cameras[0].pos.y + 50f;
                pos.x = Mathf.RoundToInt(pos.x);
                pos.y = Mathf.RoundToInt(pos.y);

                //Slot positions
                for (int i = 0; i < inventorySlots.Length; i++)
                {
                    inventorySlots[i].pos = RadialPos(i);
                }

                if (fade < 1f)
                {
                    fade += 10f * Time.deltaTime;
                }
            }
            else
            {
                if (fade > 0f)
                {
                    fade -= 10f * Time.deltaTime;
                }
            }
            //Tick down input delay
            if (inputDelay > 0)
            {
                inputDelay--;
            }
            if (player.mapInput.mp)
            {
                //Toggle map
                if (player.mapInput.pckp && inputDelay <= 0)
                {
                    if (showMap)
                    {
                        showMap = false;
                        Debug.Log("Hide Map");
                    }
                    else
                    {
                        showMap = true;
                        Debug.Log("Show Map");
                        hud.PlaySound(SoundID.Slugcat_Ghost_Appear);
                    }
                    inputDelay = 12;
                }
                //Left
                if ((player.mapInput.x < 0f && invInput.x == 0f) || (player.mapInput.x < 0f && inputDelay <= 0))
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
                    hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    Debug.Log("Slot: " + selectedSlot);
                }
                //Right
                if ((player.mapInput.x > 0f && invInput.x == 0f) || (player.mapInput.x > 0f && inputDelay <= 0))
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
                    hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    Debug.Log("Slot: " + selectedSlot);
                }
                //Player cannot manage inventory unless it has fully appeared on-screen.
                //This prevents the player accidently triggering an action the moment they open the menu.
                //This doesn't apply to opening the map however.
                if (showMap || fade < 0.5f)
                {
                    return;
                }
                //Select slot
                if (player.mapInput.jmp && !invInput.jmp)
                {
                    inputDelay = 15;
                    InteractWithSlot();
                }
            }
        }
    }
}
