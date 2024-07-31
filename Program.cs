using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace Pseudo3D
{
    internal class Program
    {
        private const bool shouldUseDeltaTime = false;
        private const double deltaAngle = 0.03;
        private const double deltaMovement = 0.15;

        private static readonly int ScreenWidth = 200;//Console.BufferWidth;
        private static readonly int ScreenHeight = 80;//Console.BufferHeight;

        private static char[] Screen = new char[ScreenWidth * ScreenHeight];

        private const int MapW = 32;
        private const int MapH = 32;

        private static double _playerX = 5;
        private static double _playerY = 5;
        private static double _playerA = 0;

        private static readonly StringBuilder Map = new StringBuilder();

        private const double FOV = Math.PI / 3;
        private const double Depth = 16;

        static void Main(string[] args)
        {
            Console.SetWindowSize(ScreenWidth, ScreenHeight);
            Console.SetBufferSize(ScreenWidth, ScreenHeight);
            Console.SetWindowSize(ScreenWidth, ScreenHeight + 1);

            Console.CursorVisible = false;


            DateTime dateTimeFrom = DateTime.Now;

            while (true)
            {
                InitMap();

                DateTime dateTimeTo = DateTime.Now;

                double deltaTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = DateTime.Now;

                if (Console.KeyAvailable)
                {
                    ConsoleKey consoleKey = Console.ReadKey(true).Key;

                    switch (consoleKey)
                    {
                        case ConsoleKey.W:
                            _playerX += Math.Sin(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                            _playerY += Math.Cos(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);

                            if (Map[(int)_playerY * MapW + (int)_playerX] == '#')
                            {
                                _playerX -= Math.Sin(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                                _playerY -= Math.Cos(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                            }

                            break;

                        case ConsoleKey.S:
                            _playerX -= Math.Sin(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                            _playerY -= Math.Cos(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);

                            if (Map[(int)_playerY * MapW + (int)_playerX] == '#')
                            {
                                _playerX += Math.Sin(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                                _playerY += Math.Cos(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                            }

                            break;

                        case ConsoleKey.D:
                            _playerX -= Math.Cos(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                            _playerY += Math.Sin(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);

                            if (Map[(int)_playerY * MapW + (int)_playerX] == '#')
                            {
                                _playerX += Math.Cos(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                                _playerY -= Math.Sin(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                            }

                            break;

                        case ConsoleKey.A:
                            _playerX += Math.Cos(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                            _playerY -= Math.Sin(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);

                            if (Map[(int)_playerY * MapW + (int)_playerX] == '#')
                            {
                                _playerX -= Math.Cos(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                                _playerY += Math.Sin(_playerA) * ((shouldUseDeltaTime) ? (deltaTime * deltaMovement * 100) : deltaMovement);
                            }

                            break;

                        case ConsoleKey.RightArrow:
                            _playerA -= ((shouldUseDeltaTime) ? (deltaTime * deltaAngle * 100) : deltaAngle);
                            break;
                        case ConsoleKey.LeftArrow:
                            _playerA += ((shouldUseDeltaTime) ? (deltaTime * deltaAngle * 100) : deltaAngle);
                            break;

                    }
                    //if (_playerX >= MapW - 1) _playerX = MapW - 1;
                    //if (_playerX <= 1) _playerX = 1;
                    //if (_playerY >= MapH - 1) _playerY = MapH - 1;
                    //if (_playerY <= 1) _playerY = 1;
                }

                for (int x = 0; x < ScreenWidth; x++)
                {
                    double rayAngle = _playerA + FOV / 2 - x * FOV / ScreenWidth;

                    double rayX = Math.Sin(rayAngle);
                    double rayY = Math.Cos(rayAngle);

                    double distanceToWall = 0;
                    bool hitWall = false;
                    bool isBound = false;

                    while (!hitWall && distanceToWall < Depth)
                    {
                        distanceToWall += 0.1;

                        int testX = (int)(_playerX + rayX * distanceToWall);
                        int testY = (int)(_playerY + rayY * distanceToWall);

                        if (testX < 0 || testX >= Depth + _playerX || testY < 0 || testY >= Depth + _playerY)
                        {
                            hitWall = true;
                            distanceToWall = Depth;
                        }
                        else
                        {
                            char testCell = Map[testY * MapW + testX];
                            if (testCell == '#')
                            {
                                hitWall = true;

                                List<(double module, double cos)> boundsVectorList = new List<(double module, double cos)>();

                                for (int tx = 0; tx < 2; tx++)
                                {
                                    for (int ty = 0; ty < 2; ty++)
                                    {
                                        double vx = testX + tx - _playerX;
                                        double vy = testY + ty - _playerY;

                                        double vectorModule = Math.Sqrt(vx * vx + vy * vy);
                                        double cosAngle = rayX * vx / vectorModule + rayY * vy / vectorModule;

                                        boundsVectorList.Add((vectorModule, cosAngle));
                                    }
                                }

                                boundsVectorList = boundsVectorList.OrderBy(v => v.module).ToList();

                                double boundAngle = 0.03 / distanceToWall;

                                if (Math.Acos(boundsVectorList[0].cos) < boundAngle ||
                                    Math.Acos(boundsVectorList[1].cos) < boundAngle)
                                {
                                    isBound = true;
                                }
                            }
                            else
                            {
                                Map[testY * MapW + testX] = '*';
                            }
                        }
                    }

                    int ceiling = (int)(ScreenHeight / 2d - ScreenHeight * FOV / distanceToWall);
                    int floor = ScreenHeight - ceiling;

                    char wallShade;
                    if (isBound)
                        wallShade = '|';
                    else if (distanceToWall <= Depth / 4d)
                        wallShade = '\u2588';
                    else if (distanceToWall <= Depth / 3d)
                        wallShade = '\u2593';
                    else if (distanceToWall <= Depth / 2d)
                        wallShade = '\u2592';
                    else if (distanceToWall <= Depth)
                        wallShade = '\u2591';
                    else wallShade = ' ';

                    for (int y = 0; y < ScreenHeight; y++)
                    {
                        if (y <= ceiling)
                        {
                            Screen[y * ScreenWidth + x] = ' ';
                        }
                        else if (y > ceiling && y <= floor)
                        {
                            Screen[y * ScreenWidth + x] = wallShade;
                        }
                        else
                        {
                            char floorShade;

                            double b = 1 - (y - ScreenHeight / 2d) / (ScreenHeight / 2d);

                            if (b < 0.25)
                                floorShade = '#';
                            else if (b < 0.5)
                                floorShade = 'x';
                            else if (b < 0.75)
                                floorShade = '-';
                            else if (b < 0.9)
                                floorShade = '.';
                            else floorShade = ' ';

                            Screen[y * ScreenWidth + x] = floorShade;
                        }
                    }

                }

                char[] stats = $"X: {_playerX}, Y: {_playerY}, A: {_playerA}, FPS: {(int)(1 / deltaTime)}".ToCharArray();
                stats.CopyTo(Screen, 0);

                // map
                for (int x = 0; x < MapW; x++)
                {
                    for (int y = 0; y < MapH; y++)
                    {
                        Screen[(y + 1) * ScreenWidth + x] = Map[y * MapW + x];
                    }
                }

                // player
                Screen[(int)(_playerY + 1) * ScreenWidth + (int)(_playerX)] = 'P';

                Console.SetCursorPosition(0, 0);
                Console.Write(Screen);
                //Console.SetCursorPosition(0, 0);

            }
        }

        private static void InitMap()
        {
            Map.Clear();

            Map.Append("################################");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..###.............#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..................#...........#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("########.......................#");
            Map.Append("#..............................#");
            Map.Append("#.......#......................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#......................##......#");
            Map.Append("#......................##......#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("#..............................#");
            Map.Append("################################");
        }

        static void Fill_W_Char(ref char[] A, char c)
        {
            for (int i = 0; i < A.Length; i++)
            {
                A[i] = c;
            }
        }
    }
}
