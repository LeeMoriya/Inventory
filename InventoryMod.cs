using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using System.Reflection;
using MonoMod.RuntimeDetour;


[BepInPlugin("LeeMoriya.Inventory", "Inventory", "0.1")]
public class InventoryMod : BaseUnityPlugin
{
    public static Player.InputPackage[] invInput = new Player.InputPackage[2];
    public static Inventory inventory;
    private Hook mapHook;
    public InventoryMod()
    {

    }

    public void OnEnable()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        On.Player.checkInput += Player_checkInput;
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        On.SaveState.SessionEnded += SaveState_SessionEnded;
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        if (mapHook == null)
        {
            mapHook = new Hook(typeof(Player).GetProperty("RevealMap", propFlags).GetGetMethod(), typeof(InventoryMod).GetMethod("Player_get_RevealMap", myMethodFlags));
        }
        else
        {
            mapHook.Apply();
        }
    }

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
        if(survived == true && !newMalnourished)
        {
            InventorySave.Save(saveSlot, slugcat);
        }
        else if(!newMalnourished)
        {
            InventorySave.Load(saveSlot, slugcat);
        }
    }

    //Load inventory contents at cycle start
    private void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        orig.Invoke(self, player);
        InventorySave.Load(self.game.rainWorld.options.saveSlot, self.saveStateNumber);
    }

    public delegate bool orig_RevealMap(Player self);
    BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
    BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;
    public static bool Player_get_RevealMap(orig_RevealMap orig, Player self)
    {
        return invInput[0].mp && inventory.showMap;
    }

    //Inventory controls
    private void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        for (int i = self.input.Length - 1; i > 0; i--)
        {
            self.input[i] = self.input[i - 1];
        }
        invInput[1] = invInput[0];
        if (self.stun == 0 && !self.dead)
        {
            if (self.controller != null)
            {
                self.input[0] = self.controller.GetInput();
            }
            else
            {
                self.input[0] = RWInput.PlayerInput(self.playerState.playerNumber, self.room.game.rainWorld.options, self.room.game.setupValues);
            }
        }
        else
        {
            self.input[0] = new Player.InputPackage(self.room.game.rainWorld.options.controls[self.playerState.playerNumber].gamePad, 0, 0, false, false, false, false, false);
        }
        self.mapInput = self.input[0];
        invInput[0] = self.input[0];
        if (inventory.isShown && self.input[0].mp)
        {
            self.input[0].x = 0;
            self.input[0].y = 0;
            self.input[0].analogueDir = self.input[0].analogueDir * 0f;
            self.input[0].jmp = false;
            self.input[0].thrw = false;
            self.input[0].pckp = false;
            return;
        }
        if ((self.standStillOnMapButton && self.input[0].mp) || self.Sleeping)
        {
            self.input[0].x = 0;
            self.input[0].y = 0;
            Player.InputPackage[] input = self.input;
            int num = 0;
            input[num].analogueDir = input[num].analogueDir * 0f;
            self.input[0].jmp = false;
            self.input[0].thrw = false;
            self.input[0].pckp = false;
            self.Blink(5);
        }
        if (self.superLaunchJump > 10 && self.input[0].jmp && self.input[1].jmp && self.input[2].jmp && self.input[0].y < 1)
        {
            self.input[0].x = 0;
        }
        if (self.animation == Player.AnimationIndex.Roll && self.input[0].x == 0 && self.input[0].downDiagonal != 0)
        {
            self.input[0].x = self.input[0].downDiagonal;
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

