using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;


namespace TeamGame
{
    /// <summary>
    /// Interface for an individual puzzle controlled and updated by a PlayerState.
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class IPuzzle : DrawableGameComponent
    {
        public Rectangle drawRegion;
        public Player player;
        public Matrix matrix { get { return Matrix.CreateTranslation(drawRegion.Location.X, drawRegion.Location.Y, 0); } }

        SoundEffectInstance buttonBlip, statusZero;

        bool statusZeroPlayed;


        public Texture2D statusBarTexture;

        public IPuzzle(Game game, Player player)
            : base(game)
        {
            this.drawRegion = player.GetRegion();
            this.player = player;
            statusBarTexture = game.Content.Load<Texture2D>("art/statusBar");

            buttonBlip = Game.Content.Load<SoundEffect>("audio/buttonBeep").CreateInstance();
            buttonBlip.Volume = .5f;
            statusZero = Game.Content.Load<SoundEffect>("audio/statusLow").CreateInstance();
            statusZeroPlayed = false;

            game.Components.Add(this);
        }

        

        public override void Initialize() {}
        public override void Update(GameTime gameTime) { throw new NotImplementedException(); }
        public virtual void Encode(NetOutgoingMessage msg) { throw new NotImplementedException(); }
        public virtual void Decode(NetIncomingMessage msg) { throw new NotImplementedException(); }
        public virtual void PuzzleOver(bool Correct) {}
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            spriteBatch.Begin();
            spriteBatch.Draw(Game.Content.Load<Texture2D>("art/playerBorder"), player.GetRegion().Location.ToVector2() + new Vector2(-4, -4), player.ColorOf());
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, this.matrix);
            spriteBatch.Draw(statusBarTexture, new Vector2(53, 163), new Rectangle(12 * (int) (12 - Game1.pStates[player].status), 0, 144, 12), player.ColorOf());
            spriteBatch.End();

            if (!player.IsLocal())
                return;

            if (Game1.pStates[player].status == 0 && !statusZeroPlayed)
            {
                statusZero.Play();
                statusZeroPlayed = true;
            }
        }
    }
    public static class IPuzzleHelper
    {
        public static Dictionary<Type, byte> map = new Dictionary<Type, byte>(){
            { typeof(Puzzles.Transition), 1},
            { typeof(Puzzles.NumeralSearch), 2},
            { typeof(Puzzles.DragCircleAvoidBlocks), 3},
            { typeof(Puzzles.TextNamesColorOfCircle), 4},
            { typeof(Puzzles.CreateASquare), 5},
            { typeof(Puzzles.MemorizeWhatYouSee), 6},
            { typeof(Puzzles.TeamCirclesInOrder), 10},
            { typeof(Puzzles.TeamFinalTest), 15},
            { typeof(Puzzles.AwaitingParticipants), 20},
            { typeof(Puzzles.StartingGame), 21},
            { typeof(Puzzles.GameOver), 22}
            };

        /// <summary>
        /// Returns the unique ID for the type of puzzle
        /// </summary>
        /// <param name="puzzle">The puzzle</param>
        /// <returns>0 if none, 1-255 if valid</returns>
        public static byte ID(this IPuzzle puzzle)
        {
            byte b = map[puzzle.GetType()];
            return map[puzzle.GetType()];
        }
        public static IPuzzle CreateFromID(this byte puzzleID, Game game, Player player)
        {
            return (IPuzzle)Activator.CreateInstance(map.FirstOrDefault(pair => pair.Value == puzzleID).Key, new object[] { game, player });
        }
    }
}
