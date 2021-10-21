using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using System.Reflection;
using MonoMod.RuntimeDetour;
using OptionalUI;

[BepInPlugin("LeeMoriya.Inventory", "Inventory", "0.12")]
public class InventoryMod : BaseUnityPlugin
{
    //AutoUpdate
    public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/4/7";
    public int version = 3;
    public string keyE = "AQAB";
    public string keyN = "lDaM5h0hJUvZcIdiWXH4qfdia/V8UWzikqRIiC9jVGA87jMrafo4EWOTk0MMIQZWHVy+msVzvEAVR3V45wZShFu7ylUndroL5u4zyqHfVeAeDIALfBrM3J4BIM1rMi4wieYdLIF6t2Uj4GVH7iU59AIfobew1vICUILu9Zib/Aw2QY6Nc+0Cz6Lw3xh7DL/trIMaW7yQfYRZUaEZBHelN2JGyUjKkbby4vL6gySfGlVl1OH0hYYhrhNwnQrOow8WXFMIu/WyTA3cY3wqkjd4/WRJ+EvYtMKTwfG+TZiHGst9Bg1ZTFfvEvrTFiPadTf19iUnfyL/QJaTAD8qe+rba5KwirIElovqFpYNH9tAr7SpjixjbT3Igmz+SlqGa9wSbm1QWt/76QqpyAYV/b5G/VzbytoZrhkEVdGuaotD4tXh462AhK5xoigB8PEt+T3nWuPdoZlVo5hRCxoNleH4yxLpVv8C7TpQgQHDqzHMcEX79xjiYiCvigCq7lLEdxUD0fhnxSYVK0O+y7T+NXkk3is/XqJxdesgyYUMT81MSou9Ur/2nv9H8IvA9QeIqso05hK3c496UOaRJS27WJhrxABtU+HHtxo9SifmXjisDj3IV46uTeVp5bivDTu1yBymgnU8qli/xmwWxKvOisi9ZOZsg4vFHaY31gdUBWOz4dU=";

    public static string versionNumber = "v1.0d";
    public static BaseUnityPlugin instance;
    public static Inventory inventory;
    private Hook mapHook;
    public InventoryMod()
    {
        instance = this;
    }

    public void OnEnable()
    {
        InventorySave.SaveHooks();
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        On.Player.checkInput += Player_checkInput;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.SaveState.SessionEnded += SaveState_SessionEnded;
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate;
        //On.Player.Grabability += Player_Grabability;
        if (mapHook == null)
        {
            mapHook = new Hook(typeof(Player).GetProperty("RevealMap", propFlags).GetGetMethod(), typeof(InventoryMod).GetMethod("Player_get_RevealMap", myMethodFlags));
        }
        else
        {
            mapHook.Apply();
        }
    }

    private int Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if(obj is Creature && !(obj is Player))
        {
            return (int)Player.ObjectGrabability.OneHand;
        }
        return orig.Invoke(self, obj);
    }

    private void MainLoopProcess_RawUpdate(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess self, float dt)
    {
        if (inventory != null && InventoryConfig.slowMenu && inventory.isShown && inventory.fade >= 1f)
        {
            if (self.framesPerSecond > 18)
            {
                self.framesPerSecond = 18;
            }
        }
        orig.Invoke(self, dt);
    }

    public static OptionInterface LoadOI() => new InventoryConfig();

    private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        //rCam.room.AddObject(new Backpack());
        orig.Invoke(self, sLeaser, rCam);
    }

    private void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
    {
        orig.Invoke(self, game, survived, newMalnourished);
        int saveSlot = game.rainWorld.options.saveSlot;
        int slugcat = self.saveStateNumber;
        //Survived without starving
        if (survived == true && !newMalnourished)
        {
            InventorySave.Save(saveSlot, slugcat);
        }
        else if (!newMalnourished)
        {
            InventorySave.Load(saveSlot, slugcat);
        }
    }

    //Load inventory contents at cycle start
    private void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        orig.Invoke(self, player);
        if (!self.saveState.malnourished)
        {
            InventorySave.Load(self.game.rainWorld.options.saveSlot, self.saveStateNumber);
        }
    }

    public delegate bool orig_RevealMap(Player self);
    BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
    BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;
    public static bool Player_get_RevealMap(orig_RevealMap orig, Player self)
    {
        return self.mapInput.mp && inventory.showMap;
    }

    //Inventory controls
    private void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        int blink = 0;
        if(self != null && self.graphicsModule != null)
        {
            blink = (self.graphicsModule as PlayerGraphics).blink;
        }
        orig.Invoke(self);
        if (inventory != null && self != null && self.graphicsModule != null && inventory.isShown)
        {
            (self.graphicsModule as PlayerGraphics).blink = blink;
        }
    }

    //Add inventory type to player HUD
    private void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig.Invoke(self, cam);
        inventory = new GridInventory(self);
        self.AddPart(inventory);
    }
}

