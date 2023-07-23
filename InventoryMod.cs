using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using System.Reflection;
using MonoMod.RuntimeDetour;

[BepInPlugin("LeeMoriya.Inventory", "Inventory", "1.0")]
public class InventoryMod : BaseUnityPlugin
{
    public static string versionNumber = "v1.0";
    public static BaseUnityPlugin instance;
    public static Inventory inventory;
    public static bool init = false;
    private Hook mapHook;
    public InventoryConfig config;
    public InventoryMod()
    {
        instance = this;
    }

    public void Awake()
    {
        On.RainWorld.OnModsInit += delegate (On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            if (!init)
            {
                init = true;
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

                //Setup Config
                config = new InventoryConfig();
                MachineConnector.SetRegisteredOI("inventory", config);
            }
        };
    }

    private int Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if(obj is Creature && !(obj is Player))
        {
            return (int)Player.ObjectGrabability.OneHand;
        }
        return (int)orig.Invoke(self, obj);
    }

    private void MainLoopProcess_RawUpdate(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess self, float dt)
    {
        if (inventory != null && InventoryConfig.slowBool.Value && inventory.isShown && inventory.fade >= 1f)
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
        SlugcatStats.Name slugcat = self.saveStateNumber;
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

