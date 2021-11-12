using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FileManager
{
    public delegate void OnKey(ConsoleKeyInfo key);

    class Program
    {
        static void Main(string[] args)
        {
            // Здесь все начинается. Юхуу :)

            FileManager manager = new FileManager();
            manager.Explore();
        }
    }
}