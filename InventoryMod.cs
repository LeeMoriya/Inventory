using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using BepInEx;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

[BepInPlugin("LeeMoriya.Inventory", "Inventory", "1.2")]
public class InventoryMod : BaseUnityPlugin
{
    public static string versionNumber = "v1.2";
    public static BaseUnityPlugin instance;
    public static Inventory inventory;
    public static bool init = false;
    public static bool _lastKey = false;
    private Hook mapHook;
    public InventoryConfig config;
    public InventoryMod()
    {
        instance = this;

    }

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig.Invoke(self);
        if (!init)
        {
            InventorySave.SaveHooks();
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
            On.Player.checkInput += Player_checkInput;
            On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
            On.SaveState.SessionEnded += SaveState_SessionEnded;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate;
            //On.Player.Update += Player_Update;

            if (mapHook == null)
            {
                mapHook = new Hook(typeof(Player).GetProperty("RevealMap", propFlags).GetGetMethod(), typeof(InventoryMod).GetMethod("Player_get_RevealMap", myMethodFlags));
            }
            else
            {
                mapHook.Apply();
            }

            init = true;
        }
        //Setup Config
        config = new InventoryConfig();
        MachineConnector.SetRegisteredOI("leemoriya.inventory", config);
    }


    public delegate bool orig_RevealMap(Player self);
    BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
    BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;
    public static bool Player_get_RevealMap(orig_RevealMap orig, Player self)
    {
        int index = self.playerState.playerNumber;
        KeyCode key = KeyCode.None;
        switch (index)
        {
            case 0:
                if (InventoryConfig.invKey1.Value != KeyCode.None)
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

        bool showMapRequired = key == KeyCode.None;
        //Player is not using custom keybinds
        if (showMapRequired)
        {
            return (!ModManager.CoopAvailable || !self.jollyButtonDown) && self.input[0].mp && inventory.showMap && !self.inVoidSea;
        }
        else
        {
            return (!ModManager.CoopAvailable || !self.jollyButtonDown) && self.input[0].mp && !self.inVoidSea;
        }
    }


    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig.Invoke(self, eu);
        bool key = Input.GetKey(KeyCode.LeftBracket);
        if (key)
        {
            foreach (string str in DataPearl.AbstractDataPearl.DataPearlType.values.entries)
            {
                DataPearl.AbstractDataPearl.DataPearlType dataPearlType = new DataPearl.AbstractDataPearl.DataPearlType(str, false);
                DataPearl.AbstractDataPearl abstractDataPearl = new DataPearl.AbstractDataPearl(self.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, self.coord, self.room.game.GetNewID(), -1, -1, null, dataPearlType);
                abstractDataPearl.RealizeInRoom();
                abstractDataPearl.realizedObject.firstChunk.HardSetPosition(self.mainBodyChunk.pos);
            }
        }
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

        int index = self.playerState.playerNumber;
        KeyCode key = KeyCode.None;
        switch (index)
        {
            case 0:
                if (InventoryConfig.invKey1.Value != KeyCode.None)
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
        if ((key != KeyCode.None && Input.GetKey(key)) || self.input[0].mp)
        {
            //Make player stand still
            self.input[0].x = 0;
            self.input[0].y = 0;
            Player.InputPackage[] input = self.input;
            int num2 = 0;
            input[num2].analogueDir = input[num2].analogueDir * 0f;
            self.input[0].jmp = false;
            self.input[0].thrw = false;
            self.input[0].pckp = false;
        }
    }

    //Add inventory type to player HUD
    private void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig.Invoke(self, cam);
        inventory = new GridInventory(self, cam.game);
        self.AddPart(inventory);
    }
}

