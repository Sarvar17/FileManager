using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PsCon
{
    struct FrameDoubleLine
    {
        // Структура чтобы нарисовать наши рамки с двойными линиями.

        public string topLeft;
        public string topRight;
        public string bottomLeft;
        public string bottomRight;
        public string lineA;
        public string lineB;

        public FrameDoubleLine(int n)
        {
            topLeft = "╔";
            topRight = "╗";
            bottomLeft = "╚";
            bottomRight = "╝";
            lineA = "═";
            lineB = "║";
        }
    }

    static class PseudoConsole
    {
        public static void PrintFrameDoubleLine(int positionX, int positionY, int sizeX, int sizeY, ConsoleColor text, ConsoleColor background)
        {
            // Метод чтобы нарисовать рамку с двойной линией в нашей консоли.

            int SizeY = positionY + sizeY;
            int SizeX = positionX + sizeX;

            FrameDoubleLine f = new FrameDoubleLine(0);
            Console.ForegroundColor = text;
            Console.BackgroundColor = background;

            for (int y = positionY; y < SizeY; y++)
            {
                for (int x = positionX; x < SizeX; x++)
                {
                    if (y == positionY && x == positionX)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(f.topLeft);
                    }
                    if (y == positionY && x > positionX && x < SizeX - 1)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(f.lineA);
                    }
                    if (y == positionY && x == SizeX - 1)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(f.topRight);
                    }
                    if (y > positionY && y < SizeY - 1 && x == positionX || y > positionY && y < SizeY - 1 && x == SizeX - 1)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(f.lineB);
                    }
                    if (y == SizeY - 1 && x == positionX)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(f.bottomLeft);
                    }
                    if (y == SizeY - 1 && x > positionX && x < SizeX - 1)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(f.lineA);
                    }
                    if (y == SizeY - 1 && x == SizeX - 1)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(f.bottomRight);
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public static void PrintString(string str, int X, int Y, ConsoleColor text, ConsoleColor background)
        {
            // Метод чтобы вывести на экран нашу строку.

            Console.ForegroundColor = text;
            Console.BackgroundColor = background;

            Console.SetCursorPosition(X, Y);
            Console.Write(str);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}