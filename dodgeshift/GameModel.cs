using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace dodgeshift;

public class GameModel
{
    public Rectangle Player;
    public List<Rectangle> FallingBlocks { get; } = new();
    public List<Rectangle> StaticBlocks { get; } = new();
    public int WindowWidth { get; } = 800;
    public int WindowHeight { get; } = 600;
    public int BlockSize { get; } = 40;
    public int GroundLevelY { get; } = 560;
    
    private const float GravityForce = 0.8f;
    private const float JumpImpulse = -16f;
    private const int MovementSpeed = 6;
    private const float SpawnIntervalSeconds = 0.3f;
    private const int MaxBlocksOnGround = 60;
    private const int FallingSpeed = 5;
    
    private const int CollisionTolerance = 10;
    
    private float _verticalVelocity;
    private bool _isOnTheFloor;
    private float _spawnTimer;
    private readonly Random _randomGenerator = new();
    private readonly int _columns;

    public bool IsGameOver { get; private set; }

    public GameModel()
    {
        _columns = WindowWidth / BlockSize;
        Reset();
    }

    public void Reset()
    {
        Player = new Rectangle(WindowWidth / 2, GroundLevelY, BlockSize, BlockSize);
        FallingBlocks.Clear();
        StaticBlocks.Clear();
        _verticalVelocity = 0;
        _isOnTheFloor = true;
        _spawnTimer = 0;
        IsGameOver = false;
    }
    
    public void MovePlayer(int direction)
    {
        if (direction == 0)
        {
            return;
        }

        int oldX = Player.X;
        Player.X += direction * MovementSpeed;

        foreach (var staticBlock in StaticBlocks)
        {
            if (Player.Intersects(staticBlock))
            {
                Player.X = oldX;
                break;
            }
        }
        Player.X = Math.Clamp(Player.X, 0, WindowWidth - Player.Width);
    }

    public void TryJump()
    {
        if (_isOnTheFloor)
        {
            _verticalVelocity = JumpImpulse;
        }
    }

    public void UpdatePhysics(float timePassed)
    {
        _verticalVelocity += GravityForce;
        Player.Y += (int)_verticalVelocity;

        _isOnTheFloor = false;
        if (Player.Y >= GroundLevelY)
        {
            Player.Y = GroundLevelY;
            _verticalVelocity = 0;
            _isOnTheFloor = true;
        }

        foreach (var staticBlock in StaticBlocks)
        {
            if (Player.Intersects(staticBlock))
            {
                if (_verticalVelocity >= 0 &&
                    Player.Bottom <= staticBlock.Top + (_verticalVelocity + CollisionTolerance))
                {
                    Player.Y = staticBlock.Y - Player.Height;
                    _verticalVelocity = 0;
                    _isOnTheFloor = true;
                }
            }
        }
        
        _spawnTimer += timePassed;
        if (_spawnTimer > SpawnIntervalSeconds)
        {
            int gridPositionX = _randomGenerator.Next(0, _columns) * BlockSize;
            FallingBlocks.Add(new Rectangle(gridPositionX, -BlockSize, BlockSize, BlockSize));
            _spawnTimer = 0;
        }
        
        for (int i = 0; i < FallingBlocks.Count; i++)
        {
            var fallingBlock = FallingBlocks[i];
            fallingBlock.Y += FallingSpeed;
            FallingBlocks[i] = fallingBlock;

            if (fallingBlock.Intersects(Player))
            {
                if (fallingBlock.Bottom < Player.Bottom + CollisionTolerance)
                {
                    IsGameOver = true;
                }
            }

            bool touchesGround = fallingBlock.Y >= GroundLevelY;
            bool touchesStaticBlock = StaticBlocks.Any(sb => fallingBlock.Intersects(sb));

            if (touchesGround || touchesStaticBlock)
            {
                fallingBlock.Y = (fallingBlock.Y / BlockSize) * BlockSize;
                StaticBlocks.Add(fallingBlock);
                FallingBlocks.RemoveAt(i);
                i--;

                if (StaticBlocks.Count > MaxBlocksOnGround)
                {
                    StaticBlocks.RemoveAt(0);
                }
            }
        }
    }
}