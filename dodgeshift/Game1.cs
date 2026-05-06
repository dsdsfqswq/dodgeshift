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
    private Rectangle _playerRectangle;
    private List<Rectangle> _fallingBlocks = new();
    private List<Rectangle> _staticBlocks = new();
    
    private KeyboardState _previousKeyboardState;
    private float _spawnTimer;
    private float _verticalVelocity;
    private bool _isGrounded;
    private Random _randomGenerator = new();

    private const int GroundLevelY = 560;
    private const float GravityForce = 0.8f;
    private const float JumpImpulse = -16f;
    private const int MovementSpeed = 6;
    private const int BlockSize = 40;
    private const float SpawnIntervalSeconds = 0.5f;
    private const int MaxBlocksOnGround = 60;

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

        _playerRectangle = new Rectangle(400, GroundLevelY, BlockSize, BlockSize);
    }

    protected override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var currentKeyboardState = Keyboard.GetState();

        if (currentKeyboardState.IsKeyDown(Keys.A)) _playerRectangle.X -= MovementSpeed;
        if (currentKeyboardState.IsKeyDown(Keys.D)) _playerRectangle.X += MovementSpeed;

        _verticalVelocity += GravityForce;
        _playerRectangle.Y += (int)_verticalVelocity;

        _isGrounded = false;
        if (_playerRectangle.Y >= GroundLevelY)
        {
            _playerRectangle.Y = GroundLevelY;
            _verticalVelocity = 0;
            _isGrounded = true;
        }

        foreach (var staticBlock in _staticBlocks)
        {
            if (_playerRectangle.Intersects(staticBlock))
            {
                if (_verticalVelocity >= 0 && _playerRectangle.Bottom <= staticBlock.Top + (_verticalVelocity + 10))
                {
                    _playerRectangle.Y = staticBlock.Y - _playerRectangle.Height;
                    _verticalVelocity = 0;
                    _isGrounded = true;
                }
            }
        }

        if (currentKeyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space) && _isGrounded)
        {
            _verticalVelocity = JumpImpulse;
        }

        _playerRectangle.X = Math.Clamp(_playerRectangle.X, 0, _graphics.PreferredBackBufferWidth - _playerRectangle.Width);

        _spawnTimer += deltaTime;
        if (_spawnTimer > SpawnIntervalSeconds)
        {
            int gridPositionX = _randomGenerator.Next(0, 20) * BlockSize; 
            _fallingBlocks.Add(new Rectangle(gridPositionX, -BlockSize, BlockSize, BlockSize)); 
            _spawnTimer = 0;
        }

        for (int i = 0; i < _fallingBlocks.Count; i++)
        {
            var fallingBlock = _fallingBlocks[i];
            fallingBlock.Y += 5;
            _fallingBlocks[i] = fallingBlock;

            if (fallingBlock.Intersects(_playerRectangle))
            {
                 if (fallingBlock.Bottom < _playerRectangle.Bottom + 10) Exit(); 
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
        _spriteBatch.Draw(_blankTexture, _playerRectangle, Color.Green);
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