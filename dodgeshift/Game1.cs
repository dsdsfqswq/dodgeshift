using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace dodgeshift;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _blankTexture;
    private Rectangle _player;
    private List<Rectangle> _fallingBlocks = new();
    private List<Rectangle> _staticBlocks = new();
    
    private const int WindowWidth = 800;
    private const int WindowHeight = 600;
    
    private KeyboardState _previousKeyboardState;
    private float _spawnTimer;
    private float _verticalVelocity;
    private bool _isOnTheFloor;
    private Random _randomGenerator = new();

    private const int GroundLevelY = 560;
    private const float GravityForce = 0.8f;
    private const float JumpImpulse = -16f;
    private const int MovementSpeed = 6;
    private const int BlockSize = 40;
    private const float SpawnIntervalSeconds = 0.3f;
    private const int MaxBlocksOnGround = 60;
    private int _columns =  WindowWidth / BlockSize;
    private const int fallingSpeed = 5;
    
    private const int CollisionTolerance = 10;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _blankTexture = new Texture2D(GraphicsDevice, 1, 1);
        _blankTexture.SetData(new[] { Color.White });

        _player = new Rectangle(WindowWidth / 2, GroundLevelY, BlockSize, BlockSize);
    }

    protected override void Update(GameTime gameTime)
    {
        float timePassed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var currentKeyboardState = Keyboard.GetState();
        var horizontalMove = 0;

        if (currentKeyboardState.IsKeyDown(Keys.A))
        {
            horizontalMove -= MovementSpeed;
        }

        if (currentKeyboardState.IsKeyDown(Keys.D))
        {
            horizontalMove += MovementSpeed;
        }
        
        if (horizontalMove != 0)
        {
            int oldX = _player.X;
            _player.X += horizontalMove;
            
            foreach (var staticBlock in _staticBlocks)
            {
                if (_player.Intersects(staticBlock))
                {
                    _player.X = oldX; 
                    break;
                }
            }
        }
        _player.X = Math.Clamp(_player.X, 0, WindowWidth - _player.Width);
        _verticalVelocity += GravityForce;
        _player.Y += (int)_verticalVelocity;

        _isOnTheFloor = false;
        if (_player.Y >= GroundLevelY)
        {
            _player.Y = GroundLevelY;
            _verticalVelocity = 0;
            _isOnTheFloor = true;
        }

        foreach (var staticBlock in _staticBlocks)
        {
            if (_player.Intersects(staticBlock))
            {
                if (_verticalVelocity >= 0 && _player.Bottom <= staticBlock.Top + (_verticalVelocity + CollisionTolerance))
                {
                    _player.Y = staticBlock.Y - _player.Height;
                    _verticalVelocity = 0;
                    _isOnTheFloor = true;
                }
            }
        }

        if (currentKeyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space) && _isOnTheFloor)
        {
            _verticalVelocity = JumpImpulse;
        }

        _player.X = Math.Clamp(_player.X, 0, _graphics.PreferredBackBufferWidth - _player.Width);

        _spawnTimer += timePassed;
        if (_spawnTimer > SpawnIntervalSeconds)
        {
            int gridPositionX = _randomGenerator.Next(0, _columns) * BlockSize; 
            _fallingBlocks.Add(new Rectangle(gridPositionX, -BlockSize, BlockSize, BlockSize)); 
            _spawnTimer = 0;
        }

        for (int i = 0; i < _fallingBlocks.Count; i++)
        {
            var fallingBlock = _fallingBlocks[i];
            fallingBlock.Y += fallingSpeed;
            _fallingBlocks[i] = fallingBlock;

            if (fallingBlock.Intersects(_player))
            {
                if (fallingBlock.Bottom < _player.Bottom + CollisionTolerance)
                {
                    Exit();
                } 
            }

            bool touchesGround = fallingBlock.Y >= GroundLevelY;
            bool touchesStaticBlock = _staticBlocks.Any(sb => fallingBlock.Intersects(sb));

            if (touchesGround || touchesStaticBlock)
            {
                fallingBlock.Y = (fallingBlock.Y / BlockSize) * BlockSize;
                _staticBlocks.Add(fallingBlock);
                _fallingBlocks.RemoveAt(i);
                i--;

                if (_staticBlocks.Count > MaxBlocksOnGround)
                {
                    _staticBlocks.RemoveAt(0);
                }
            }
        }

        _previousKeyboardState = currentKeyboardState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_blankTexture, _player, Color.Green);
        foreach (var fallingBlock in _fallingBlocks)
        {
            _spriteBatch.Draw(_blankTexture, fallingBlock, Color.Red);
        }

        foreach (var staticBlock in _staticBlocks)
        {
            _spriteBatch.Draw(_blankTexture, staticBlock, Color.Yellow);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}