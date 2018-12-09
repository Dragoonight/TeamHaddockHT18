﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// Class created by Alexander 11-07
namespace TeamHaddock
{
    /// <summary>
    ///     Class responsible for Player movement, drawing etc.
    /// </summary>
    public class Player
    {
        private KeyboardState keyboard;
        public CollidableObject collidableObject;
        private Texture2D NormalMap;

        private const float baseWalkingSpeed = 0.1f, baseJumpStrength = -0.08f;
        private readonly Vector2 maxMovementSpeed = new Vector2(0.5f, 10f);
        private Vector2 velocity;
        private Point direction = new Point(1, 1);
        private const int maxJumpTime = 200;
        private int jumpTime;
        private bool jumpComplete, onGround, walking;
        public const int maxHealth = 1000000;
        public int Health { get; private set; } = maxHealth;

        List<Animation> animations = new List<Animation>();

        private Animation CurrentAnimation
        {
            get
            {
                // Attacking
                if (attacking)
                {
                    // X
                    switch (attackDirection.X)
                    {
                        // Left
                        case -1:
                            // Y
                            switch (attackDirection.Y)
                            {
                                // Jumping
                                case -1:
                                    foreach (Animation animation in animations) {if (animation.name == "attackJumpingLeft") { return animation; } } 
                                    throw new ArgumentOutOfRangeException();
                                // On ground
                                case 0:
                                    foreach (Animation animation in animations) { if (animation.name == "attackGroundedLeft") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // Falling
                                case 1:
                                    foreach (Animation animation in animations) { if (animation.name == "attackFallingLeft") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // Error
                                default: throw new ArgumentOutOfRangeException();
                            }
                        // Right
                        case 1:
                            // Y
                            switch (attackDirection.Y)
                            {
                                // Jumping
                                case -1:
                                    foreach (Animation animation in animations) { if (animation.name == "attackJumpingRight") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // On ground
                                case 0:
                                    foreach (Animation animation in animations) { if (animation.name == "attackGroundedRight") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // Falling
                                case 1:
                                    foreach (Animation animation in animations) { if (animation.name == "attackFallingRight") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // Error
                                default: throw new ArgumentOutOfRangeException();
                            }
                        // Error
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                // Not attacking
                else
                {
                    // X
                    switch (direction.X)
                    {
                        // Left
                        case -1:
                            // Y
                            switch (direction.Y)
                            {
                                // Jumping
                                case -1:
                                    foreach (Animation animation in animations) { if (animation.name == "jumpingLeft") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // On ground
                                case 0:
                                    if (walking)
                                    {
                                        foreach (Animation animation in animations) { if (animation.name == "walkLeft") { return animation; } }
                                    }
                                    else
                                    {
                                        foreach (Animation animation in animations) { if (animation.name == "idleLeft") { return animation; } }
                                    }
                                    throw new ArgumentOutOfRangeException();
                                // Falling
                                case 1:
                                    foreach (Animation animation in animations) { if (animation.name == "fallingLeft") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // Error
                                default: throw new ArgumentOutOfRangeException();
                            }
                        // Right
                        case 1:
                            // Y
                            switch (direction.Y)
                            {
                                // Jumping
                                case -1:
                                    foreach (Animation animation in animations) { if (animation.name == "jumpingRight") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // On ground
                                case 0:
                                    if (walking)
                                    {
                                        foreach (Animation animation in animations) { if (animation.name == "walkRight") { return animation; } }
                                    }
                                    else
                                    {
                                        foreach (Animation animation in animations) { if (animation.name == "idleRight") { return animation; } }
                                    }
                                    throw new ArgumentOutOfRangeException();
                                // Falling
                                case 1:
                                    foreach (Animation animation in animations) { if (animation.name == "fallingRight") { return animation; } }
                                    throw new ArgumentOutOfRangeException();
                                // Error
                                default: throw new ArgumentOutOfRangeException();
                            }
                        // Error
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }


        private CollidableObject attackCollidableObject;
        private bool attacking;
        private int timeAttacking;
        private const int attackDamage = 10;
        private Point attackDirection;

        

        /// <summary>
        /// The base damage for the enemies pistol 
        /// </summary>
        private const int basePistolDamage = 8;

        /// <summary>
        /// Called upon to load player textures etc.
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            // Create a new collidableObject
            collidableObject = new CollidableObject(
                content.Load<Texture2D>(@"Textures/Player"), // The texture
                new Vector2(250), // The spawning position
                new Rectangle(0, 0, 79, 104), // Initial size and position of source rectangle
                0f // The rotation
                );

            // Create a new collidable object for attack collision map
            attackCollidableObject = new CollidableObject(content.Load<Texture2D>(@"Textures/PlayerCollisionMap"),
                collidableObject.Position,
                collidableObject.SourceRectangle,
                collidableObject.Rotation);

            // Load normal map texture
            NormalMap = content.Load<Texture2D>(@"Textures/PlayerNormalMap");

            LoadAnimations();
        }

        private void LoadAnimations()
        {
            // The constant animation time
            int walkingFrameTime = 125;

            // Load all frames into their animations

            animations.Add(new Animation("idleRight", new List<Frame>
            {
                new Frame(new Rectangle(80, 0, 75, 104), walkingFrameTime), // TODO
            }));
            
            animations.Add( new Animation("idleLeft", new List<Frame>
            {
                new Frame(new Rectangle(80, 0, 75, 104), walkingFrameTime), // TODO
            }));

            animations.Add( new Animation("walkRight", new List<Frame>
            {
                //new Frame(new Rectangle(0, 0, 79, 104), walkingFrameTime),
                new Frame(new Rectangle(80, 0, 75, 104), walkingFrameTime),
                new Frame(new Rectangle(156, 0, 74, 104), walkingFrameTime),
                new Frame(new Rectangle(80, 0, 75, 104), walkingFrameTime),
                //new Frame(new Rectangle(0, 0, 79, 104), walkingFrameTime),
                new Frame(new Rectangle(231, 0, 80, 104), walkingFrameTime),
                new Frame(new Rectangle(311, 0, 78, 104), walkingFrameTime),
                new Frame(new Rectangle(231, 0, 80, 104), walkingFrameTime),
            }));

            animations.Add( new Animation("walkLeft", new List<Frame>
            {
                //new Frame(new Rectangle(0, 104, 79, 104), walkingFrameTime),
                new Frame(new Rectangle(80, 104, 75, 104), walkingFrameTime),
                new Frame(new Rectangle(156, 104, 74, 104), walkingFrameTime),
                new Frame(new Rectangle(80, 104, 75, 104), walkingFrameTime),
                //new Frame(new Rectangle(0, 104, 100, 104), walkingFrameTime),
                new Frame(new Rectangle(231, 104, 80, 104), walkingFrameTime),
                new Frame(new Rectangle(311, 104, 78, 104), walkingFrameTime),
                new Frame(new Rectangle(231, 104, 80, 104), walkingFrameTime),
            }));

            animations.Add( new Animation("jumpingRight", new List<Frame>
            {
                new Frame(new Rectangle(0, 209, 72, 105), walkingFrameTime)
            }));

            animations.Add( new Animation("jumpingLeft", new List<Frame>
            {
                new Frame(new Rectangle(0, 324, 72, 105), walkingFrameTime)
            }));

            animations.Add( new Animation("fallingRight", new List<Frame>
            {
                new Frame(new Rectangle(0, 440, 68, 107), walkingFrameTime)
            }));

            //
            animations.Add( new Animation("fallingLeft", new List<Frame>
            {
                new Frame(new Rectangle(0, 547, 68, 107), walkingFrameTime)
            }));

            // 
            animations.Add( new Animation("attackJumpingRight", new List<Frame>
            {
                new Frame(new Rectangle(72, 209, 62, 113), walkingFrameTime),
                new Frame(new Rectangle(135, 209, 62, 113), walkingFrameTime)
            }));

            animations.Add(new Animation("attackJumpingLeft", new List<Frame>
            {
                new Frame(new Rectangle(72, 324, 62, 113), walkingFrameTime),
                new Frame(new Rectangle(135, 324, 62, 113), walkingFrameTime)
            }));

            animations.Add( new Animation("attackGroundedRight", new List<Frame>
            {
                new Frame(new Rectangle(80, 0, 75, 104), walkingFrameTime), // TODO
            }));
            animations.Add(new Animation("attackGroundedLeft", new List<Frame>
            {
                new Frame(new Rectangle(80, 0, 75, 104), walkingFrameTime), // TODO
            }));
            animations.Add(new Animation("attackFallingRight",
                new List<Frame>
                {
                    new Frame(new Rectangle(70, 440, 61, 108), walkingFrameTime),
                    new Frame(new Rectangle(133, 440, 146, 108), walkingFrameTime)
                }));
            animations.Add(new Animation("attackFallingLeft",
                new List<Frame>
                {
                    new Frame(new Rectangle(70, 546, 61, 108), walkingFrameTime),
                    new Frame(new Rectangle(132, 546, 146, 108), walkingFrameTime)
                }));
        }

        /// <summary>
        /// Updates player logic
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            UpdateControls(gameTime);
            UpdateAnimation(gameTime);
            UpdatePosition(gameTime);
            if (attacking) { UpdateAttack(gameTime);}
            UpdateVelocity(gameTime);
            HealthDepletion(gameTime);
        }


        // Created by Noble 11-07, edited by Alexander 12-06
        private void UpdateControls(GameTime gameTime)
        {
            // Update keyboard
            keyboard = Keyboard.GetState();

            // Reset walking
            walking = false;

            // if player hits the ground //or the top of a platform
            if (collidableObject.Position.Y >= Game1.ScreenBounds.Y - collidableObject.Origin.Y)
            {
                onGround = true;
                direction.Y = 0;
            }
            else
            {
                onGround = false;
            }
            
            
            // If W or Up arrow key is pressed down And jump is not complete TODO: Fix jumping
            if ((keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) && !jumpComplete)
            {
                    // Continue jump
                    // Jump has already started
                    if (jumpTime > 0)
                    {
                        Jump(gameTime);
                    }

                    // Start jump
                    // If jumpTime is reset and is on ground
                    if (jumpTime == 0 && onGround)
                    {
                        Jump(gameTime);
                    }
            }
            else
            {
                // key was released, therefore set jump to complete
                jumpComplete = true;
                // if both keys are up and player is on ground
                if (keyboard.IsKeyUp(Keys.W) && keyboard.IsKeyUp(Keys.Up) && onGround)
                {
                    // Reset jump
                    jumpTime = 0;
                    jumpComplete = false;
                }
                // Fall
                Fall(gameTime);
            }

            // If A or Left arrow key is pressed down AND right keys are not down as to prevent pressing both directions at once
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left) && !(keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)))
            {
                // Move left
                MoveLeft();
            }

            // If D or Right arrow key is pressed down AND left keys are not down as to prevent pressing both directions at once
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right) && !(keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)))
            {
                // Move Right
                MoveRight();
            }

            // If Space or Z key are pressed down
            if ((UtilityClass.SingleActivationKey(Keys.Space) && !UtilityClass.SingleActivationKey(Keys.Z)) || (UtilityClass.SingleActivationKey(Keys.Z) && !UtilityClass.SingleActivationKey(Keys.Space)))
            {
                // If not already attacking
                if (!attacking)
                {
                    // Start a new attack
                    StartAttack();
                }
            }

            #region Debug controls
            #if DEBUG
                // If Q key is pressed down then rotate counter-clockwise
                if (keyboard.IsKeyDown(Keys.Q))
                {
                    collidableObject.Rotation -= MathHelper.TwoPi / 1000 * gameTime.ElapsedGameTime.Milliseconds;
                }
                // If E key is pressed down then rotate clockwise
                if (keyboard.IsKeyDown(Keys.E))
                {
                    collidableObject.Rotation += MathHelper.TwoPi / 1000 * gameTime.ElapsedGameTime.Milliseconds;
                }
            #endif
            #endregion 
        }


        // Created by Noble 11-21, Edited by Alexander 11-22
        private void MoveLeft()
        {
            // Set direction to left
            direction.X = -1;
            walking = true;
            AddForce(new Vector2(-baseWalkingSpeed, 0));
        }

        // Created by Noble 11-21, Edited by Alexander 11-22
        private void MoveRight()
        {
            // Set direction to Right
            direction.X = 1;
            walking = true;
            AddForce(new Vector2(baseWalkingSpeed, 0));
        }


        // Created by Noble 11-21, Edited by Noble 11-28, Edited by Alexander 12-06
        private void Jump(GameTime gameTime)
        {
            // Add elapsed time to timer
            jumpTime += gameTime.ElapsedGameTime.Milliseconds;
            // update direction to up
            direction.Y = -1;
            // if timer has not expired
            if (jumpTime < maxJumpTime)
            {
                // set velocity to jump
                velocity.Y = baseJumpStrength * gameTime.ElapsedGameTime.Milliseconds;
            }
            // Else timer has expired
            else
            {
                // Complete jump
                jumpComplete = true;
            }
        }

        /// <summary>
        /// Adds gravity to velocity if not onGround
        /// </summary>
        /// <param name="gameTime"></param>
        private void Fall(GameTime gameTime)
        {
            if (!onGround)
            {
                direction.Y = 1;
                // Add gravity
                AddForce(new Vector2(0, 0.04f));
            }
        }

        /// <summary>
        /// Adds a force to velocity while clamping velocity to maxMovementSpeed
        /// </summary>
        /// <param name="force"></param>
        private void AddForce(Vector2 force)
        {
            velocity.X = MathHelper.Clamp(velocity.X + force.X, -maxMovementSpeed.X, maxMovementSpeed.X);
            velocity.Y = MathHelper.Clamp(velocity.Y + force.Y, -maxMovementSpeed.Y, maxMovementSpeed.Y);
        }


        private void StartAttack()
        {
            // Set attacking to active
            attacking = true;
            // Set attackDirection to current direction
            attackDirection = direction;
        }

        private void UpdateAttack(GameTime gameTime)
        {
            // Update attack collidable
            attackCollidableObject.Position = collidableObject.Position;
            attackCollidableObject.SourceRectangle = collidableObject.SourceRectangle;
            attackCollidableObject.Rotation = collidableObject.Rotation;

            // Check attack collision to every enemy
            foreach (IEnemy enemy in InGame.enemies)
            {
                if (enemy.CollidableObject.IsColliding(attackCollidableObject))
                {
                    enemy.TakeDamage(attackDamage);
                }
            }

            // Add elapsed time to timeAttacking
            timeAttacking += gameTime.ElapsedGameTime.Milliseconds;

            // Ends attack if it has been active for one animation loop
            if (timeAttacking >= CurrentAnimation.TotalFrameTime)
            {
                EndAttack();
            }
        }

        private void EndAttack()
        {
            // End attack
            attacking = false;
        }

        /// <summary>
        /// Depletes health over time
        /// </summary>
        /// <param name="gameTime"></param>
        private void HealthDepletion(GameTime gameTime)
        {
            Health -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void TakeDamage(InGame.DamageTypes damageType)
        {
            switch (damageType)
            {
                case InGame.DamageTypes.Pistol:
                    Health -= basePistolDamage * WaveManager.CurrentWave; // TODO: REDO
                    break;

                case InGame.DamageTypes.Melee:
                    Health -= 100; // TODO: Change this
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null);
            }
        }


        private void UpdateAnimation(GameTime gameTime)
        {
            CurrentAnimation.Animate(ref collidableObject.SourceRectangle, gameTime);

            // Reset all other animations except from the CurrentAnimation
            foreach (Animation animation in animations)
            {
                if (ReferenceEquals(animation, CurrentAnimation)) { return;}
                animation.Reset();
            }
        }

        /// <summary>
        /// Updates position while keeping player within the screen bounds.
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdatePosition(GameTime gameTime)
        {
            // Clamp X position + velocity
            collidableObject.Position.X = MathHelper.Clamp(
                collidableObject.Position.X + (velocity.X * gameTime.ElapsedGameTime.Milliseconds),
                0 + collidableObject.Origin.X,
                Game1.ScreenBounds.X - collidableObject.Origin.X);

            // Clamp Y position + velocity
            collidableObject.Position.Y = MathHelper.Clamp(
                collidableObject.Position.Y + (velocity.Y * gameTime.ElapsedGameTime.Milliseconds),
                0 + collidableObject.Origin.Y,
                Game1.ScreenBounds.Y - collidableObject.Origin.Y);
        }

        /// <summary>
        /// Prepares velocity for next update
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateVelocity(GameTime gameTime)
        {
            // Reduce velocity when player is not doing anything
            if (!walking)
            {
                velocity.X *= 0.055f * gameTime.ElapsedGameTime.Milliseconds;               
            }

            // Truncate velocity
            velocity.X = velocity.X.Truncate(3);
            velocity.Y = velocity.Y.Truncate(3);
        }


        /// <summary>
        ///     Draw player color map
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawColorMap(SpriteBatch spriteBatch)
        {
            // Draw player
            spriteBatch.Draw(collidableObject.Texture,
                collidableObject.Position,
                collidableObject.SourceRectangle,
                Color.White,
                collidableObject.Rotation,
                collidableObject.Origin,
                1.0f,
                SpriteEffects.None,
                0.0f);
        }


        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            // Draw player normal map
            spriteBatch.Draw(NormalMap,
                collidableObject.Position,
                collidableObject.SourceRectangle,
                Color.White,
                collidableObject.Rotation,
                collidableObject.Origin,
                1.0f,
                SpriteEffects.None,
                0.0f);
        }
    }
}