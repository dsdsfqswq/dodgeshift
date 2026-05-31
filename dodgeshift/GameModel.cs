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
    
    public int WindowWidth { get; } = 480;
    public int WindowHeight { get; } = 700;
    public int BlockSize { get; } = 40;
    public int GroundLevelY { get; } = 700; 
    
    private const float GravityForce = 45f;       
    private const float JumpImpulse = -14f;       
    private const int MovementSpeed = 350;        
    
    private const float SpawnIntervalSeconds = 0.5f; 
    private const float FallingSpeed = 300f;      
    
    private const int CollisionTolerance = 14;
    private const float CameraSmooth = 0.1f;
    
    private float _playerExactX;
    private float _playerExactY;
    private List<float> _fallingBlocksExactY = new(); 

    private float _verticalVelocity;
    private bool _isOnTheFloor;
    private float _spawnTimer;
    private readonly Random _randomGenerator = new();
    private readonly int _columns;

    private int _blocksScore;
    private int _heightScore;

    public bool IsGameOver { get; private set; }
    public float CameraMovement { get; private set; }
    public int Score => _blocksScore + _heightScore;

    public GameModel()
    {
        _columns = WindowWidth / BlockSize;
        Reset();
    }

    public void Reset()
    {
        CameraMovement = 0;
        _blocksScore = 0;
        _heightScore = 0;
        _playerExactX = WindowWidth / 2f;
        _playerExactY = GroundLevelY - BlockSize;
        Player = new Rectangle((int)_playerExactX, (int)_playerExactY, BlockSize, BlockSize);
        
        FallingBlocks.Clear();
        _fallingBlocksExactY.Clear();
        StaticBlocks.Clear();
        
        _verticalVelocity = 0;
        _isOnTheFloor = true;
        _spawnTimer = 0;
        IsGameOver = false;
    }
    
    public void MovePlayer(int direction, float timePassed)
    {
        if (direction == 0) return;

        float oldX = _playerExactX;
        _playerExactX += direction * MovementSpeed * timePassed;
        Player.X = (int)_playerExactX;

        foreach (var staticBlock in StaticBlocks)
        {
            if (Player.Intersects(staticBlock))
            {
                _playerExactX = oldX;
                Player.X = (int)_playerExactX;
                break;
            }
        }
        _playerExactX = Math.Clamp(_playerExactX, 0, WindowWidth - Player.Width);
        Player.X = (int)_playerExactX;
    }

    public void TryJump()
    {
        if (_isOnTheFloor)
        {
            _verticalVelocity = JumpImpulse;
            _isOnTheFloor = false;
        }
    }

    public void UpdatePhysics(float timePassed)
    {
        _verticalVelocity += GravityForce * timePassed;
        _playerExactY += _verticalVelocity;
        Player.Y = (int)_playerExactY;

        _isOnTheFloor = false;
        if (Player.Bottom >= GroundLevelY)
        {
            _playerExactY = GroundLevelY - Player.Height;
            Player.Y = (int)_playerExactY;
            _verticalVelocity = 0;
            _isOnTheFloor = true;
        }
        
        foreach (var staticBlock in StaticBlocks)
        {
            if (Player.Intersects(staticBlock))
            {
                if (_verticalVelocity >= 0 && Player.Bottom <= staticBlock.Top + CollisionTolerance + 5)
                {
                    _playerExactY = staticBlock.Y - Player.Height;
                    Player.Y = (int)_playerExactY;
                    _verticalVelocity = 0;
                    _isOnTheFloor = true;
                }
            }
        }

        _spawnTimer += timePassed;
        if (_spawnTimer > SpawnIntervalSeconds)
        {
            int gridPositionX = _randomGenerator.Next(0, _columns) * BlockSize;
            int spawnY = ((int)CameraMovement / BlockSize) * BlockSize - BlockSize;
            
            FallingBlocks.Add(new Rectangle(gridPositionX, spawnY, BlockSize, BlockSize));
            _fallingBlocksExactY.Add(spawnY); 
            _spawnTimer = 0;
        }
        
        for (int i = 0; i < FallingBlocks.Count; i++)
        {
            _fallingBlocksExactY[i] += FallingSpeed * timePassed;
            
            var fallingBlock = FallingBlocks[i];
            fallingBlock.Y = (int)_fallingBlocksExactY[i];
            FallingBlocks[i] = fallingBlock;
            
            if (fallingBlock.Intersects(Player))
            {
                if (fallingBlock.Bottom < Player.Bottom + CollisionTolerance)
                {
                    IsGameOver = true;
                }
            }
            
            bool touchesStaticBlock = StaticBlocks.Any(sb => fallingBlock.Intersects(sb));
            bool touchesGround = fallingBlock.Bottom >= GroundLevelY;

            if (touchesGround || touchesStaticBlock)
            {
                fallingBlock.X = (fallingBlock.X / BlockSize) * BlockSize;

                if (touchesStaticBlock)
                {
                    var hitBlock = StaticBlocks.First(sb => fallingBlock.Intersects(sb));
                    fallingBlock.Y = hitBlock.Y - BlockSize;
                }
                else
                {
                    fallingBlock.Y = GroundLevelY - BlockSize;
                }

                StaticBlocks.Add(fallingBlock);
                FallingBlocks.RemoveAt(i);
                _fallingBlocksExactY.RemoveAt(i);
                i--;
                
                _blocksScore += 10;
            }
        }
        
        UpdateCamera(timePassed);
    }

    private void UpdateCamera(float timePassed)
    {
        float screenCenterY = (WindowHeight / 2f) + CameraMovement;
        if (Player.Y < screenCenterY)
        {
            CameraMovement += (Player.Y - screenCenterY) * CameraSmooth;
        }
        
        int currentHeightScore = (int)Math.Max(0, -CameraMovement);
        if (currentHeightScore > _heightScore)
        {
            _heightScore = currentHeightScore;
        }
        
        if (Player.Y > WindowHeight + CameraMovement)
        {
            IsGameOver = true;
        }
    }
}
