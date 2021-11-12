using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using PsCon;

namespace FileManager
{
    class FilePanel
    {
        //========================== Static ==========================

        #region Parameters

        public static int PANEL_HEIGHT = 18;
        public static int PANEL_WIDTH = 120;

        #endregion

        //========================== Fields ==========================
        // Все наши поля.
        
        #region Panel location

        private int top;
        public int Top
        {
            get
            {
                return this.top;
            }
            set
            {
                this.top = value;
            }
        }

        private int left;
        public int Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
            }
        }

        private int height = FilePanel.PANEL_HEIGHT;
        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        private int width = FilePanel.PANEL_WIDTH;
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
        #endregion

        #region Panel state

        private string path;
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        private int activeObjectIndex = 0;
        private int firstObjectIndex = 0;
        private int displayedObjectsAmount = PANEL_HEIGHT - 2;
        private bool active;
        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                this.active = value;
            }
        }
        private bool discs;
        public bool isDiscs
        {
            get
            {
                return this.discs;
            }
        }

        #endregion

        private List<FileSystemInfo> fsObjects = new List<FileSystemInfo>();

        //========================== Methods ==========================

        #region Ctor

        public FilePanel()
        {
            // Метод чтобы чтобы установить наши диски.

            this.SetDiscs();
        }

        #endregion

        public FileSystemInfo GetActiveObject()
        {
            // Метод чтобы получить активный файл.

            return this.fsObjects[this.activeObjectIndex];
        }

        #region Navigations

        public void KeyboardProcessing(ConsoleKeyInfo key)
        {
            // Методы чтобы установить какие клавиши существуют.

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    this.ScrollUp();
                    break;
                case ConsoleKey.DownArrow:
                    this.ScrollDown();
                    break;
                case ConsoleKey.Home:
                    this.GoBegin();
                    break;
                case ConsoleKey.End:
                    this.GoEnd();
                    break;
                case ConsoleKey.PageUp:
                    this.PageUp();
                    break;
                case ConsoleKey.PageDown:
                    this.PageDown();
                    break;
                default:
                    break;
            }
        }

        private void ScrollDown()
        {
            // Метод чтобы прокрутить страницу вниз.

            if (this.activeObjectIndex >= this.firstObjectIndex + this.displayedObjectsAmount - 1)
            {
                this.firstObjectIndex += 1;
                if (this.firstObjectIndex + this.displayedObjectsAmount >= this.fsObjects.Count)
                {
                    this.firstObjectIndex = this.fsObjects.Count - this.displayedObjectsAmount;
                }
                this.activeObjectIndex = this.firstObjectIndex + this.displayedObjectsAmount - 1;
                this.UpdateContent(false);
            }

            else
            {
                if (this.activeObjectIndex >= this.fsObjects.Count - 1)
                {
                    return;
                }
                this.DeactivateObject(this.activeObjectIndex);
                this.activeObjectIndex++;
                this.ActivateObject(this.activeObjectIndex);
            }
        }

        private void ScrollUp()
        {
            // Метод чтобы прокрутить страницу верх.

            if (this.activeObjectIndex <= this.firstObjectIndex)
            {
                this.firstObjectIndex -= 1;
                if (this.firstObjectIndex < 0)
                {
                    this.firstObjectIndex = 0;
                }
                this.activeObjectIndex = firstObjectIndex;
                this.UpdateContent(false);
            }
            else
            {
                this.DeactivateObject(this.activeObjectIndex);
                this.activeObjectIndex--;
                this.ActivateObject(this.activeObjectIndex);
            }
        }

        private void GoEnd()
        {
            // Метод чтобы пойти в конец.

            if (this.firstObjectIndex + this.displayedObjectsAmount < this.fsObjects.Count)
            {
                this.firstObjectIndex = this.fsObjects.Count - this.displayedObjectsAmount;
            }
            this.activeObjectIndex = this.fsObjects.Count - 1;
            this.UpdateContent(false);
        }

        private void GoBegin()
        {
            // Метод чтобы пойти в начало.

            this.firstObjectIndex = 0;
            this.activeObjectIndex = 0;
            this.UpdateContent(false);
        }

        private void PageDown()
        {
            // Метод чтобы пролистать вниз страницу.

            if (this.activeObjectIndex + this.displayedObjectsAmount < this.fsObjects.Count)
            {
                this.firstObjectIndex += this.displayedObjectsAmount;
                this.activeObjectIndex += this.displayedObjectsAmount;
            }
            else
            {
                this.activeObjectIndex = this.fsObjects.Count - 1;
            }
            this.UpdateContent(false);
        }

        private void PageUp()
        {
            // Метод чтобы пролистать вверх страницу.

            if (this.activeObjectIndex > this.displayedObjectsAmount)
            {
                this.firstObjectIndex -= this.displayedObjectsAmount;
                if (this.firstObjectIndex < 0)
                {
                    this.firstObjectIndex = 0;
                }

                this.activeObjectIndex -= this.displayedObjectsAmount;

                if (this.activeObjectIndex < 0)
                {
                    this.activeObjectIndex = 0;
                }
            }
            else
            {
                this.firstObjectIndex = 0;
                this.activeObjectIndex = 0;
            }
            this.UpdateContent(false);
        }

        #endregion

        #region Fill panels

        public void SetLists()
        {
            // Метод чтобы указать лист директорий и файлов.

            if (this.fsObjects.Count != 0)
            {
                this.fsObjects.Clear();
            }

            this.discs = false;

            DirectoryInfo levelUpDirectory = null;
            this.fsObjects.Add(levelUpDirectory);

            // Директории.

            string[] directories = Directory.GetDirectories(this.path);
            foreach (string directory in directories)
            {
                DirectoryInfo di = new DirectoryInfo(directory);
                this.fsObjects.Add(di);
            }

            // Файлы.

            string[] files = Directory.GetFiles(this.path);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                this.fsObjects.Add(fi);
            }
        }

        public void SetDiscs()
        {
            // Метод чтобы указать наши диски.

            if (this.fsObjects.Count != 0)
            {
                this.fsObjects.Clear();
            }

            this.discs = true;

            DriveInfo[] discs = DriveInfo.GetDrives();
            foreach (DriveInfo disc in discs)
            {
                if (disc.IsReady)
                {
                    DirectoryInfo di = new DirectoryInfo(disc.Name);
                    this.fsObjects.Add(di);
                }
            }
        }

        #endregion

        #region Display methods

        public void Show()
        {
            // Метод чтобы показать содержимое панели.

            this.Clear();

            PsCon.PseudoConsole.PrintFrameDoubleLine(this.left, this.top, this.width, this.height, ConsoleColor.White, ConsoleColor.Black);

            StringBuilder caption = new StringBuilder();
            if (this.discs)
            {
                caption.Append(' ').Append("Disks").Append(' ');
            }
            else
            {
                caption.Append(' ').Append(this.path).Append(' ');
            }
            PsCon.PseudoConsole.PrintString(caption.ToString(), this.left + this.width / 2 - caption.ToString().Length / 2, this.top, ConsoleColor.White, ConsoleColor.Black);

            this.PrintContent();
        }

        public void Clear()
        {
            // Метод чтобы очистить все внутри панели.

            for (int i = 0; i < this.height; i++)
            {
                string space = new String(' ', this.width);
                Console.SetCursorPosition(this.left, this.top + i);
                Console.Write(space);
            }
        }

        private void PrintContent()
        {
            // Метод написать контент.

            if (this.fsObjects.Count == 0)
            {
                return;
            }
            int count = 0;

            int lastElement = this.firstObjectIndex + this.displayedObjectsAmount;
            if (lastElement > this.fsObjects.Count)
            {
                lastElement = this.fsObjects.Count;
            }


            if (this.activeObjectIndex >= this.fsObjects.Count)
            {
                activeObjectIndex = 0;
            }

            for (int i = this.firstObjectIndex; i < lastElement; i++)
            {
                Console.SetCursorPosition(this.left + 1, this.top + count + 1);

                if (i == this.activeObjectIndex && this.active == true)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                }
                this.PrintObject(i);
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                count++;
            }
        }

        private void ClearContent()
        {
            // Метод чтобы очистить контент.

            for (int i = 1; i < this.height - 1; i++)
            {
                string space = new String(' ', this.width - 2);
                Console.SetCursorPosition(this.left + 1, this.top + i);
                Console.Write(space);
            }
        }

        private void PrintObject(int index)
        {
            // Метод чтобы вывести на экран наш объект.

            int currentCursorTopPosition = Console.CursorTop;
            int currentCursorLeftPosition = Console.CursorLeft;

            if (!this.discs && index == 0)
            {
                Console.Write("..");
                return;
            }

            Console.Write("{0}", fsObjects[index].Name);
            Console.SetCursorPosition(currentCursorLeftPosition + this.width / 2, currentCursorTopPosition);
            if (fsObjects[index] is DirectoryInfo)
            {
                Console.Write("{0}", ((DirectoryInfo)fsObjects[index]).LastWriteTime);
            }
            else
            {
                Console.Write("{0}", ((FileInfo)fsObjects[index]).Length);
            }
        }

        public void UpdatePanel()
        {
            // Метод чтобы обновить нашу панель.

            this.firstObjectIndex = 0;
            this.activeObjectIndex = 0;
            this.Show();
        }

        public void UpdateContent(bool updateList)
        {
            // Метод чтобы обновить контент.

            if (updateList)
            {
                this.SetLists();
            }
            this.ClearContent();
            this.PrintContent();
        }

        private void ActivateObject(int index)
        {
            // Метод чтобы активировать наш объект.

            int offsetY = this.activeObjectIndex - this.firstObjectIndex;
            Console.SetCursorPosition(this.left + 1, this.top + offsetY + 1);

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            this.PrintObject(index);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private void DeactivateObject(int index)
        {
            // Метод чтобы деактивирорвать наш объект.

            int offsetY = this.activeObjectIndex - this.firstObjectIndex;
            Console.SetCursorPosition(this.left + 1, this.top + offsetY + 1);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            this.PrintObject(index);
        }

        #endregion
    }
}