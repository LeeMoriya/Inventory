using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;

public class Backpack : CosmeticSprite
{
    public Player player;
    public float heightAdjust = 0.5f;
    public Backpack()
    {

    }

    public override void Update(bool eu)
    {
        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        this.player = (rCam.game.Players.Count <= 0) ? null : (rCam.game.Players[0].realizedCreature as Player);
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("KrakenHead0", true);
        sLeaser.sprites[0].scaleY = 0.7f;
        sLeaser.sprites[0].scaleX = 0.85f;
        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (this.player != null)
        {
            float rot = Custom.AimFromOneVectorToAnother(player.bodyChunks[1].pos, player.bodyChunks[0].pos);
            float lastRot = Custom.AimFromOneVectorToAnother(player.bodyChunks[1].lastPos, player.bodyChunks[0].lastPos);
            if (Input.GetKey(KeyCode.KeypadPlus))
            {
                heightAdjust += 0.05f;
            }
            if (Input.GetKey(KeyCode.KeypadMinus))
            {
                heightAdjust -= 0.05f;
            }
            Vector2 backpackPos = Vector2.Lerp(Vector2.Lerp(player.bodyChunks[1].lastPos, player.bodyChunks[1].pos, timeStacker), Vector2.Lerp(player.bodyChunks[0].lastPos, player.bodyChunks[0].pos, timeStacker), heightAdjust);
            float offset = Mathf.Lerp(0f, 15f, Mathf.Lerp(player.bodyChunks[0].pos.y, player.bodyChunks[1].pos.y, backpackPos.y));
            sLeaser.sprites[0].x = backpackPos.x - camPos.x;
            sLeaser.sprites[0].y = backpackPos.y + offset - camPos.y;
            sLeaser.sprites[0].rotation = Mathf.Lerp(lastRot, rot, timeStacker);
        }
        else
        {
            sLeaser.CleanSpritesAndRemove();
            this.Destroy();
        }
        if (player.inShortcut)
        {
            sLeaser.sprites[0].alpha = 0f;
        }
        else
        {
            sLeaser.sprites[0].alpha = 1f;
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = new Color(0.6f, 0.4f, 0.3f);
        base.ApplyPalette(sLeaser, rCam, palette);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
    }
}

