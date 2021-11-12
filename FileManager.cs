using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using PsCon;
using System.Transactions;

namespace FileManager
{
    class FileManager
    {
        //========================== Static ==========================

        public static int HEIGHT_KEYS = 3;
        public static int BOTTOM_OFFSET = 2;

        //========================== Fields ==========================

        public event OnKey KeyPress;
        List<FilePanel> panels = new List<FilePanel>();
        private int activePanelIndex;

        //========================== Methods ==========================

        #region Ctor

        static FileManager()
        {
            // Метод где указано как выглядить консоль.

            Console.CursorVisible = false;
            Console.SetWindowSize(120, 41);
            Console.SetBufferSize(120, 41);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public FileManager()
        {
            // Метод чтобы вывести на экран наши панели.

            FilePanel filePanel = new FilePanel();
            filePanel.Top = 0;
            filePanel.Left = 0;
            this.panels.Add(filePanel);

            filePanel = new FilePanel();
            filePanel.Top = FilePanel.PANEL_HEIGHT;
            filePanel.Left = 0;
            this.panels.Add(filePanel);

            activePanelIndex = 0;

            this.panels[this.activePanelIndex].Active = true;
            KeyPress += this.panels[this.activePanelIndex].KeyboardProcessing;

            foreach (FilePanel fp in panels)
            {
                fp.Show();
            }

            this.ShowKeys();
        }

        #endregion

        public void Explore()
        {
            // Метод чтобы указать какие горячие клавиши существует.

            bool exit = false;
            while (!exit)
            {
                if (Console.KeyAvailable)
                {
                    this.ClearMessage();

                    ConsoleKeyInfo userKey = Console.ReadKey(true);
                    switch (userKey.Key)
                    {
                        case ConsoleKey.Tab:
                            this.ChangeActivePanel();
                            break;
                        case ConsoleKey.Enter:
                            this.ChangeDirectoryOrRunProcess();
                            break;
                        case ConsoleKey.F1:
                            this.ViewInstructions();
                            break;
                        case ConsoleKey.F2:
                            this.ViewFile();
                            break;
                        case ConsoleKey.F3:
                            this.AppendTextFiles();
                            break;
                        case ConsoleKey.F4:
                            this.CreateTextFile();
                            break;
                        case ConsoleKey.F5:
                            this.Copy();
                            break;
                        case ConsoleKey.F6:
                            this.Move();
                            break;
                        case ConsoleKey.F7:
                            this.CreateDirectory();
                            break;
                        case ConsoleKey.F8:
                            this.Rename();
                            break;
                        case ConsoleKey.F9:
                            this.Delete();
                            break;
                        case ConsoleKey.F10:
                            exit = true;
                            Console.ResetColor();
                            Console.Clear();
                            break;
                        case ConsoleKey.DownArrow:
                            goto case ConsoleKey.PageUp;
                        case ConsoleKey.UpArrow:
                            goto case ConsoleKey.PageUp;
                        case ConsoleKey.End:
                            goto case ConsoleKey.PageUp;
                        case ConsoleKey.Home:
                            goto case ConsoleKey.PageUp;
                        case ConsoleKey.PageDown:
                            goto case ConsoleKey.PageUp;
                        case ConsoleKey.PageUp:
                            this.KeyPress(userKey);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private string AskName(string message)
        {
            // Метод чтобы спросить название файла или директории.

            string name;
            Console.CursorVisible = true;
            do
            {
                this.ClearMessage();
                this.ShowMessage(message);
                name = Console.ReadLine();
            } while (name.Length == 0);
            Console.CursorVisible = false;
            this.ClearMessage();
            return name;
        }

        private int AskEncoding(string message)
        {
            // Метод чтобы спросить кодировку текстового файла.

            string name;
            int method;
            Console.CursorVisible = true;
            do
            {
                this.ClearMessage();
                this.ShowMessage(message);
                name = Console.ReadLine();
            } while (!int.TryParse(name, out method) || method < 1 || method > 3);
            Console.CursorVisible = false;
            this.ClearMessage();
            return method;
        }

        #region FileOperation

        private void Copy()
        {
            // Метод чтобы копировать файл или директорию.

            foreach (FilePanel panel in panels)
            {
                if (panel.isDiscs)
                {
                    return;
                }
            }

            if (this.panels[0].Path == this.panels[1].Path)
            {
                return;
            }

            try
            {
                string destPath = this.activePanelIndex == 0 ? this.panels[1].Path : this.panels[0].Path;

                FileSystemInfo fileObject = this.panels[this.activePanelIndex].GetActiveObject();
                FileInfo currentFile = fileObject as FileInfo;

                if (currentFile != null)
                {
                    string fileName = currentFile.Name;
                    string destName = Path.Combine(destPath, fileName);
                    File.Copy(currentFile.FullName, destName, true);
                }
                else
                {
                    string currentDir = ((DirectoryInfo)fileObject).FullName;
                    string destDir = Path.Combine(destPath, ((DirectoryInfo)fileObject).Name);
                    CopyDirectory(currentDir, destDir);
                }

                this.RefreshPannels();
            }
            catch (Exception e)
            {
                this.ShowMessage(e.Message);
                return;
            }
        }

        private void CopyDirectory(string sourceDirName, string destDirName)
        {
            // Метод копировать директорию.

            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
        }

        private void Delete()
        {
            // Метод чтобы удалить файл или директорию.

            if (this.panels[this.activePanelIndex].isDiscs)
            {
                return;
            }

            FileSystemInfo fileObject = this.panels[this.activePanelIndex].GetActiveObject();
            try
            {
                if (fileObject is DirectoryInfo)
                {
                    ((DirectoryInfo)fileObject).Delete(true);
                }
                else
                {
                    ((FileInfo)fileObject).Delete();
                }
                this.RefreshPannels();
            }
            catch (Exception e)
            {
                this.ShowMessage(e.Message);
                return;
            }
        }

        private void CreateDirectory()
        {
            // Метод чтобы создать директорию.

            if (this.panels[this.activePanelIndex].isDiscs)
            {
                return;
            }

            string destPath = this.panels[this.activePanelIndex].Path;
            string dirName = this.AskName("Enter name of folder : ");

            try
            {
                string dirFullName = Path.Combine(destPath, dirName);
                DirectoryInfo dir = new DirectoryInfo(dirFullName);
                if (!dir.Exists)
                {
                    dir.Create();
                }
                else
                {
                    this.ShowMessage("Folder with that name already exists");
                }
                this.RefreshPannels();
            }
            catch (Exception e)
            {
                this.ShowMessage(e.Message);
            }
        }

        private void CreateTextFile()
        {
            // Метод чтобы создать тектовый файл.

            if (this.panels[this.activePanelIndex].isDiscs)
            {
                return;
            }

            string destPath = this.panels[this.activePanelIndex].Path;
            string dirName = this.AskName("Enter name of text file (ex. textfile): ");

            int method = AskEncoding("Choose type of encoding. 1 if UTF8, 2 if ASCII, 3 if Unicode : ");
            var typeOfEncoding = Encoding.UTF8;

            if (method == 2)
            {
                typeOfEncoding = Encoding.ASCII;
            }
            else if (method == 3)
            {
                typeOfEncoding = Encoding.Unicode;
            }

            try
            {
                string dirFullName = Path.Combine(destPath, dirName + ".txt");
                if (!File.Exists(dirFullName))
                {
                    var myFile = File.Create(dirFullName);
                    myFile.Close();
                    File.WriteAllText(dirFullName, "", typeOfEncoding);
                }
                else
                {
                    this.ShowMessage("Text file with that name already exists");
                }
                this.RefreshPannels();
            }
            catch (Exception e)
            {
                this.ShowMessage(e.Message);
            }
        }

        private void Move()
        {
            // Метод чтобы переместить файл или директорию.

            foreach (FilePanel panel in panels)
            {
                if (panel.isDiscs)
                {
                    return;
                }
            }

            if (this.panels[0].Path == this.panels[1].Path)
            {
                return;
            }

            try
            {
                string destPath = this.activePanelIndex == 0 ? this.panels[1].Path : this.panels[0].Path;
                FileSystemInfo fileObject = this.panels[this.activePanelIndex].GetActiveObject();

                string objectName = fileObject.Name;
                string destName = Path.Combine(destPath, objectName);

                if (fileObject is FileInfo)
                {
                    ((FileInfo)fileObject).MoveTo(destName);
                }
                else
                {
                    ((DirectoryInfo)fileObject).MoveTo(destName);
                }

                this.RefreshPannels();
            }
            catch (Exception e)
            {
                this.ShowMessage(e.Message);
                return;
            }

        }

        private void Rename()
        {
            // Метод чтобы переименовать файл или директорию.

            if (this.panels[this.activePanelIndex].isDiscs)
            {
                return;
            }

            FileSystemInfo fileObject = this.panels[this.activePanelIndex].GetActiveObject();
            string currentPath = this.panels[this.activePanelIndex].Path;

            string newName = this.AskName("Enter new name : ");
            string newFullName = Path.Combine(currentPath, newName);

            try
            {
                if (fileObject is FileInfo)
                {
                    ((FileInfo)fileObject).MoveTo(newFullName);
                }
                else
                {
                    ((DirectoryInfo)fileObject).MoveTo(newFullName);
                }
                this.RefreshPannels();
            }
            catch (Exception e)
            {
                this.ShowMessage(e.Message);
            }
        }

        private void AppendTextFiles()
        {
            // Метод чтобы конкатеновать (объядинить) текстовые файлы.

            foreach (FilePanel panel in panels)
            {
                if (panel.isDiscs)
                {
                    return;
                }
            }

            try
            {
                string destPath1 = this.panels[this.activePanelIndex].Path;
                string destPath2 = this.activePanelIndex == 0 ? this.panels[1].Path : this.panels[0].Path;

                FileSystemInfo fileObject = this.panels[this.activePanelIndex].GetActiveObject();
                FileInfo currentFile = fileObject as FileInfo;

                if (currentFile != null)
                {
                    string firstFileName = currentFile.Name;
                    string[] parts = firstFileName.Split('.');
                    if (parts.Length != 2 || parts[1] != "txt")
                    {
                        return;
                    }

                    string dirFirstFullName = Path.Combine(destPath1, firstFileName);
                    string contentOfFirstFile = ReadFileToString(dirFirstFullName, Encoding.UTF8);

                    string secondFileName = AskName("Enter name of second text file which is in another panel (ex. file.txt) : ");
                    string[] parts2 = firstFileName.Split('.');
                    if (parts2.Length != 2 || parts2[1] != "txt")
                    {
                        return;
                    }
                    string dirSecondFullName = Path.Combine(destPath2, secondFileName);
                    FileInfo secondFile = new FileInfo(dirSecondFullName);

                    if (secondFile.Exists)
                    {
                        string contentOfSecondFile = ReadFileToString(dirSecondFullName, Encoding.UTF8);
                        string newFileName = AskName("Enter new name for text file (ex. textFile) : ");

                        string newFileFullName = Path.Combine(destPath1, newFileName + ".txt");
                        if (!File.Exists(newFileFullName))
                        {
                            var myNewFile = File.Create(newFileFullName);
                            myNewFile.Close();
                            File.WriteAllText(newFileFullName, contentOfFirstFile + '\n' + contentOfSecondFile, Encoding.UTF8);
                        }
                        else
                        {
                            this.ShowMessage("Text file with that name already exists");
                        }
                    }
                    else
                    {
                        this.ShowMessage("There is no text file with that name");
                    }
                }

                this.RefreshPannels();
            }
            catch (Exception e)
            {
                this.ShowMessage(e.Message);
                return;
            }
        }

        #endregion

        #region View files

        private void ViewFile()
        {
            // Метод чтобы прочитать (увидеть) файл.

            if (this.panels[this.activePanelIndex].isDiscs)
            {
                return;
            }

            FileSystemInfo fileObject = this.panels[this.activePanelIndex].GetActiveObject();
            if (fileObject is DirectoryInfo || fileObject == null)
            {
                return;
            }

            if (((FileInfo)fileObject).Length == 0)
            {
                this.ShowMessage("File is empty");
                return;
            }

            if (((FileInfo)fileObject).Length > 100000000)
            {
                this.ShowMessage("The file is too large to view");
                return;
            }

            int method = AskEncoding("Choose type of encoding. 1 if UTF8, 2 if ASCII, 3 if Unicode : ");
            var typeOfEncoding = Encoding.UTF8;

            if (method == 2)
            {
                typeOfEncoding = Encoding.ASCII;
            }
            else if (method == 3)
            {
                typeOfEncoding = Encoding.Unicode;
            }

            string fileContent = this.ReadFileToString(fileObject.FullName, typeOfEncoding);

            if (fileContent.Length == 0)
            {
                this.ShowMessage("File is empty");
                return;
            }

            this.DrawViewFileFrame(fileObject.Name);

            int beginPosition = 0;
            int symbolCount = 0;
            bool endOfFile = false;
            bool beginFile = true;
            Stack<int> printSymbols = new Stack<int>();

            symbolCount = this.PrintStringFrame(fileContent, beginPosition);
            printSymbols.Push(symbolCount);
            this.PrintProgress(beginPosition + symbolCount, fileContent.Length);

            bool exit = false;
            while (!exit)
            {
                endOfFile = (beginPosition + symbolCount) >= fileContent.Length;
                beginFile = (beginPosition <= 0);

                ConsoleKeyInfo userKey = Console.ReadKey(true);
                switch (userKey.Key)
                {
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                    case ConsoleKey.PageDown:
                        if (!endOfFile)
                        {
                            beginPosition += symbolCount;
                            symbolCount = this.PrintStringFrame(fileContent, beginPosition);
                            printSymbols.Push(symbolCount);
                            this.PrintProgress(beginPosition + symbolCount, fileContent.Length);
                        }
                        break;
                    case ConsoleKey.PageUp:
                        if (!beginFile)
                        {
                            if (printSymbols.Count != 0)
                            {
                                beginPosition -= printSymbols.Pop();
                                if (beginPosition < 0)
                                {
                                    beginPosition = 0;
                                }
                            }
                            else
                            {
                                beginPosition = 0;
                            }
                            symbolCount = this.PrintStringFrame(fileContent, beginPosition);
                            this.PrintProgress(beginPosition + symbolCount, fileContent.Length);
                        }
                        break;
                }
            }

            Console.Clear();
            foreach (FilePanel fp in panels)
            {
                fp.Show();
            }
            this.ShowKeys();
        }

        private void ViewInstructions()
        {
            // Метод чтобы увидеть инструкцию.

            this.DrawViewFileFrame("Instructions");
            string fileContent = File.ReadAllText("Instruction.txt", Encoding.UTF8);

            int beginPosition = 0;
            int symbolCount = 0;
            bool endOfFile = false;
            bool beginFile = true;
            Stack<int> printSymbols = new Stack<int>();

            symbolCount = this.PrintStringFrame(fileContent, beginPosition);
            printSymbols.Push(symbolCount);
            this.PrintProgress(beginPosition + symbolCount, fileContent.Length);

            bool exit = false;
            while (!exit)
            {
                endOfFile = (beginPosition + symbolCount) >= fileContent.Length;
                beginFile = (beginPosition <= 0);

                ConsoleKeyInfo userKey = Console.ReadKey(true);
                switch (userKey.Key)
                {
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                    case ConsoleKey.PageDown:
                        if (!endOfFile)
                        {
                            beginPosition += symbolCount;
                            symbolCount = this.PrintStringFrame(fileContent, beginPosition);
                            printSymbols.Push(symbolCount);
                            this.PrintProgress(beginPosition + symbolCount, fileContent.Length);
                        }
                        break;
                    case ConsoleKey.PageUp:
                        if (!beginFile)
                        {
                            if (printSymbols.Count != 0)
                            {
                                beginPosition -= printSymbols.Pop();
                                if (beginPosition < 0)
                                {
                                    beginPosition = 0;
                                }
                            }
                            else
                            {
                                beginPosition = 0;
                            }
                            symbolCount = this.PrintStringFrame(fileContent, beginPosition);
                            this.PrintProgress(beginPosition + symbolCount, fileContent.Length);
                        }
                        break;
                }
            }

            Console.Clear();
            foreach (FilePanel fp in panels)
            {
                fp.Show();
            }
            this.ShowKeys();
        }

        private void DrawViewFileFrame(string file)
        {
            // Метод чтобы нарисовать рамку.

            Console.Clear();
            PsCon.PseudoConsole.PrintFrameDoubleLine(0, 0, Console.WindowWidth, Console.WindowHeight - 5, ConsoleColor.DarkYellow, ConsoleColor.Black);
            string fileName = String.Format(" {0} ", file);
            PsCon.PseudoConsole.PrintString(fileName, (Console.WindowWidth - fileName.Length) / 2, 0, ConsoleColor.Yellow, ConsoleColor.Black);
            PsCon.PseudoConsole.PrintFrameDoubleLine(0, Console.WindowHeight - 5, Console.WindowWidth, 4, ConsoleColor.DarkYellow, ConsoleColor.Black);
            PsCon.PseudoConsole.PrintString("PageDown / PageUp - navigation, Esc - exit", 1, Console.WindowHeight - 4, ConsoleColor.White, ConsoleColor.Black);
        }

        private void PrintProgress(int position, int length)
        {
            // Метод чтобы написать прогресс прочтения файла.

            string pageMessage = String.Format("Position: {0}%", (100 * position) / length);
            PsCon.PseudoConsole.PrintString(new String(' ', Console.WindowWidth / 2 - 1), Console.WindowWidth / 2, Console.WindowHeight - 4, ConsoleColor.White, ConsoleColor.Black);
            PsCon.PseudoConsole.PrintString(pageMessage, Console.WindowWidth - pageMessage.Length - 2, Console.WindowHeight - 4, ConsoleColor.White, ConsoleColor.Black);
        }

        private string ReadFileToString(string fullFileName, Encoding encoding)
        {
            // Метод чтобы прочесть содержимое файла.

            StreamReader SR = new StreamReader(fullFileName, encoding);
            string fileContent = SR.ReadToEnd();
            fileContent = fileContent.Replace("\a", " ").Replace("\b", " ").Replace("\f", " ").Replace("\r", " ").Replace("\v", " ");
            SR.Close();
            return fileContent;
        }

        private int PrintStringFrame(string text, int begin)
        {
            // Метод чтобы написать текст внутри рамки.

            this.ClearFileViewFrame();

            int lastTopCursorPosition = Console.WindowHeight - 7;
            int lastLeftCursorPosition = Console.WindowWidth - 2;

            Console.SetCursorPosition(1, 1);

            int currentTopPosition = Console.CursorTop;
            int currentLeftPosition = Console.CursorLeft;

            int index = begin;
            while (true)
            {
                if (index >= text.Length)
                {
                    break;
                }

                Console.Write(text[index]);
                currentTopPosition = Console.CursorTop;
                currentLeftPosition = Console.CursorLeft;

                if (currentLeftPosition == 0 || currentLeftPosition == lastLeftCursorPosition)
                {
                    Console.CursorLeft = 1;
                }

                if (currentTopPosition == lastTopCursorPosition)
                {
                    break;
                }

                index++;
            }
            return index - begin;
        }
        
        private void ClearFileViewFrame()
        {
            // Метод чтобы очистить содержимое внутри рамки.

            int lastTopCursorPosition = Console.WindowHeight - 7;
            int lastLeftCursorPosition = Console.WindowWidth - 2;

            for (int row = 1; row < lastTopCursorPosition; row++)
            {
                Console.SetCursorPosition(1, row);
                string space = new String(' ', lastLeftCursorPosition);
                Console.Write(space);
            }
        }

        #endregion

        private void RefreshPannels()
        {
            // Метод чтобы обновить наши панели после изменения.

            if (this.panels == null || this.panels.Count == 0)
            {
                return;
            }

            foreach (FilePanel panel in panels)
            {
                if (!panel.isDiscs)
                {
                    panel.UpdateContent(true);
                }
            }
        }

        private void ChangeActivePanel()
        {
            // Метод чтобы изменить активную панель.

            this.panels[this.activePanelIndex].Active = false;
            KeyPress -= this.panels[this.activePanelIndex].KeyboardProcessing;
            this.panels[this.activePanelIndex].UpdateContent(false);

            this.activePanelIndex++;
            if (this.activePanelIndex >= this.panels.Count)
            {
                this.activePanelIndex = 0;
            }

            this.panels[this.activePanelIndex].Active = true;
            KeyPress += this.panels[this.activePanelIndex].KeyboardProcessing;
            this.panels[this.activePanelIndex].UpdateContent(false);
        }

        private void ChangeDirectoryOrRunProcess()
        {
            // Метод чтобы открывать директории и диски.

            FileSystemInfo fsInfo = this.panels[this.activePanelIndex].GetActiveObject();
            if (fsInfo != null)
            {
                if (fsInfo is DirectoryInfo)
                {
                    try
                    {
                        Directory.GetDirectories(fsInfo.FullName);
                    }
                    catch
                    {
                        return;
                    }

                    this.panels[this.activePanelIndex].Path = fsInfo.FullName;
                    this.panels[this.activePanelIndex].SetLists();
                    this.panels[this.activePanelIndex].UpdatePanel();
                }
            }
            else
            {
                string currentPath = this.panels[this.activePanelIndex].Path;
                DirectoryInfo currentDirectory = new DirectoryInfo(currentPath);
                DirectoryInfo upLevelDirectory = currentDirectory.Parent;

                if (upLevelDirectory != null)
                {
                    this.panels[this.activePanelIndex].Path = upLevelDirectory.FullName;
                    this.panels[this.activePanelIndex].SetLists();
                    this.panels[this.activePanelIndex].UpdatePanel();
                }

                else
                {
                    this.panels[this.activePanelIndex].SetDiscs();
                    this.panels[this.activePanelIndex].UpdatePanel();
                }
            }
        }

        private void ShowKeys()
        {
            // Метод чтобы вывести на экран горячие клавиши.

            string[] menu = { "F1 Instruc", "F2 View", "F3 Append", "F4 TextFile", "F5 Copy", "F6 Move", "F7 Folder", "F8 Rename", "F9 Delete", "F10 Exit"};

            int cellLeft = this.panels[0].Left;
            int cellTop = FilePanel.PANEL_HEIGHT * this.panels.Count;
            int cellWidth = FilePanel.PANEL_WIDTH / menu.Length;
            int cellHeight = FileManager.HEIGHT_KEYS;

            for (int i = 0; i < menu.Length; i++)
            {
                PsCon.PseudoConsole.PrintFrameDoubleLine(cellLeft + i * cellWidth, cellTop, cellWidth, cellHeight, ConsoleColor.White, ConsoleColor.Black);
                PsCon.PseudoConsole.PrintString(menu[i], cellLeft + i * cellWidth + 1, cellTop + 1, ConsoleColor.White, ConsoleColor.Black);
            }
        }

        public void ShowMessage(string message)
        {
            // Метод вывести на экран сообщение.

            PsCon.PseudoConsole.PrintString(message, 0, Console.WindowHeight - BOTTOM_OFFSET, ConsoleColor.White, ConsoleColor.Black);
        }

        private void ClearMessage()
        {
            // Метод чтобы очистить строку сообщений.

            PsCon.PseudoConsole.PrintString(new String(' ', Console.WindowWidth), 0, Console.WindowHeight - BOTTOM_OFFSET, ConsoleColor.White, ConsoleColor.Black);
        }
    }
}