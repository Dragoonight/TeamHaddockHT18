﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TeamHaddock
{
    public static class UserInterface
    {
        private static Texture2D healthBar;
        private static Vector2 healthBarPosition = new Vector2(4f);
        private static Rectangle heathBarBackgroundSource = new Rectangle(0,0,141,31);
        private static Rectangle healthBarFillerSource = new Rectangle(4, 35, 132, 23);
        public static void LoadContent(ContentManager content)
        {
            healthBar = content.Load<Texture2D>(@"Textures/HealthBar");
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            // Draw health bar background
            spriteBatch.Draw(healthBar, healthBarPosition, heathBarBackgroundSource, Color.White);
            // Draw Healthbar filler
            spriteBatch.Draw(healthBar, new Vector2(healthBarPosition.X + healthBarFillerSource.X, healthBarPosition.Y + healthBarFillerSource.Y), new Rectangle(healthBarFillerSource.X, healthBarFillerSource.Y, healthBarFillerSource.Width * (InGame.player.Health / Player.maxHealth), healthBarFillerSource.Height) , Color.White);
            // Draw wave number
            spriteBatch.End();
        }
    }
}
