﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace TeamGame.Puzzles
{
    class TextNamesColorOfCircle : IPuzzle
    {
        byte timesDisplayed = 61;
        int numberMissed = 0;
        Rectangle ballPosition;

        MyColor ballColor, textColor, textWord;
        Texture2D ballTexture; // 50x50
        SoundEffectInstance textNames;

        MouseState mouse, prevMouse;

        LeftClick leftClick;

        public TextNamesColorOfCircle(Game game, Player player)
            : base(game, player)
        {
            leftClick = new LeftClick(game, player);
        }

        public override void Initialize()
        {
            ballPosition = new Rectangle(drawRegion.Center.X - 25, drawRegion.Center.Y - 25, 50, 50);
            ballTexture = Game.Content.Load<Texture2D>("art/ball50px");

            if (player == Game1.localPlayer)
            {
                textNames = Game.Content.Load<SoundEffect>("audio/ClickWhenTheTextNames").CreateInstance();
                textNames.Play();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (player != Game1.localPlayer)
                return; // this puzzle only updates by its owner
            if (timesDisplayed > 60)
            {
                ballColor = (MyColor)((Game1.random.Next(1, 4) + (int)ballColor) % 4);
                textColor = (MyColor)((Game1.random.Next(1, 4) + (int)textColor) % 4);
                if (textColor == ballColor)
                    textColor = (MyColor)(((int) textColor + 1) % 4);
                textWord = (MyColor)((Game1.random.Next(1, 4) + (int)textWord) % 4);
                if (textWord == textColor && Game1.random.Next(2) == 1)
                    textWord = ballColor;
                timesDisplayed = 0;
                if (ballColor == textWord)
                    numberMissed++;
                if (numberMissed > 2)
                    PuzzleOver(false);
                return;
            }

            mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                leftClick.Play(gameTime, mouse.Position());

            if (mouse.LeftButton.IsClicked() && ballPosition.Contains(mouse.PPosition()))
            {
                if (ballColor == textWord)
                    PuzzleOver(true); // Correct
                else
                    PuzzleOver(false);
            }
            prevMouse = mouse;
        }

        public override void Draw(GameTime gameTime)
        {
            if (textColor == ballColor) // not yet updated so don't draw that first frame
                return;

            SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteBatch.Begin(/*SpriteSortMode.Deferred, null, null, null, null, null, this.matrix*/);

            spriteBatch.Draw(ballTexture, ballPosition, player == Game1.localPlayer ? ((Player)ballColor + 7).RealColor() : player.ColorOf());
            String text = Enum.GetName(typeof(MyColor), textWord);
            spriteBatch.DrawString(Game1.font, text, ballPosition.Center.ToVector2().Plus(0, 50), player==Game1.localPlayer?((Player)textColor+7).RealColor():player.ColorOf(), 0, Game1.font.MeasureString(text)/2, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();
            base.Draw(gameTime); // draw healthbar

            timesDisplayed += 1;
        }

        public new void PuzzleOver(bool p)
        {
            if (textNames.State == SoundState.Playing)
                textNames.Dispose();
            if (p)
                Game.Content.Load<SoundEffect>("audio/Correct").Play(1.0f, 0.0f, 0.0f);
            else
                Game.Content.Load<SoundEffect>("audio/Incorrect").Play(1.0f, 0.0f, 0.0f);

            Game.Components.Remove(this);


            Game1.pStates[this.player].puzzle = new Puzzles.Transition(Game, player, p);
        }

        public override void Encode(Lidgren.Network.NetOutgoingMessage msg)
        {
            msg.Write((byte) timesDisplayed);
            msg.Write((byte) ballColor);
            msg.Write((byte) textColor);
            msg.Write((byte) textWord);
        }

        public override void Decode(Lidgren.Network.NetIncomingMessage msg)
        {
            timesDisplayed =      msg.ReadByte();
            ballColor = (MyColor) msg.ReadByte();
            textColor = (MyColor) msg.ReadByte();
            textWord =  (MyColor) msg.ReadByte();
        }
    }

    public enum MyColor
    {
        YELLOW, BLUE, RED, GREEN
    }
}
