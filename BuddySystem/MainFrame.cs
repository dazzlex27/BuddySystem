using System;
using System.Drawing;
using System.Windows.Forms;
// Программа эмулирует распределение и очистку памяти по методу близнецов
namespace BuddySystem
{
    public partial class MainFrame : Form
    {
        // Память
        int[] memory = null; // массив памяти
        int freeMemory = 0; // Свободная память
        int totalMemory = 0; // Общий объём памяти
        // Блоки
        int blockSizeMax; // Максимальный размер блока
        int blockSizeRequired; // Требуемый под файл размер блока
        int blockSizeCurrent; // Текущий размер блока
        // Вспомогательные
        int fileCounter = 0; // Счётчик количества файлов
        bool deleteCheck = false; // Проверка повторного нажатия для удаления файла
        bool addCheck = false; // Проверка повторного нажатия для добавления файла
        bool sizeCheck = false; // Проверка повторного нажатия для выделения памяти
        int[] fileExists = null; // Массив индексов существования файлов
        bool[] fileUsed = null; // массив счётчиков использования файлов
        int[] tree = null; // Дерево
        int outputCounter; // Счётчик для алгоритма разбиения блоков
        Bitmap bitmap; // Битмап для графики

        public MainFrame()
        {
            InitializeComponent();
        }

        void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.BackColor = Color.Gray;
            numericUpDown1.Visible = false;
            choose.Visible = false;
            erase.Enabled = false;
            addFile.Enabled = false;
            deleteFile.Enabled = false;
            numericUpDown1.Increment = numericUpDown1.Value;
            fileLabel.Visible = false;
            numericUpDown2.Visible = false;
            toolStripStatusLabel1.Text = "Выделите память";
        }

        void MaxSize()
        {
            int exponent = (int)(Math.Log10(totalMemory) / Math.Log10(2)), intCounter = 0, pos = 0, check = 0;
            blockSizeMax = 0;
            // Проверка на полную занятость.
            for (int counter = 0; counter < exponent; counter++)
            {
                check = 0;
                for (int counter1 = pos; counter1 < pos+(int)Math.Pow(2, counter); counter1++)
                    if (tree[counter1] > 0)
                        check++;
                if (check == (int)Math.Pow(2, counter))
                {
                    blockSizeMax = 0;
                    return;
                }
                    pos += (int)Math.Pow(2, counter);
            } 
            // Поиск максимального свободного блока.
            check = 1;
            while (true)
            {
                for (int counter = 0; counter < tree.Length; counter++)
                    try
                    {
                        if (tree[counter] == 0)
                        {
                            while (true)
                            {
                                if (counter <= check)
                                {
                                    blockSizeMax = (int)Math.Pow(2, exponent);
                                    return;
                                }
                                intCounter++;
                                check += (int)Math.Pow(2, intCounter);
                                exponent--;
                            }
                        }
                    }
                    catch { return; };
            }
        } 
        
        // Поиск максимального размера доступного блока
        void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Increment = numericUpDown1.Value;
        } 
        
        // Задание размера
        void RefreshData()
        {
            total.Text = "Объём памяти: " + totalMemory + " кБ";
            file.Text = "Количество файлов: " + fileCounter + " шт.";
            free.Text = "Свободная память: " + freeMemory + " кБ";
            // Вывод графики
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics gr = Graphics.FromImage(bitmap);
            field.Text = "";
            treeBox.Text = "";
            for (int counter = 0; counter < totalMemory; counter++)
            {
                field.Text += memory[counter];
                if (memory[counter] == 0)
                    gr.FillRectangle(Brushes.Green, counter * (512 / totalMemory), 0, (512 / totalMemory), 70);
                else
                    gr.FillRectangle(Brushes.Red, counter * (512 / totalMemory), 0, (512 / totalMemory), 70);
            }
            for (int counter = 0; counter < tree.Length-1; counter++)
                treeBox.Text += tree[counter];
            pictureBox1.Image = bitmap;
        } 
        
        // Обновление визуальных счётчиков данных и графики
        bool Position(int treeNode)
        {
            while (treeNode != 0)
            {
                if (treeNode % 2 == 0)
                    treeNode = (treeNode - 1) / 2;
                else
                    treeNode /= 2;
                if (tree[treeNode] > 0)
                    return false;
            }
            return true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        } 
        
        private void fieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!fieldToolStripMenuItem.Checked)
                field.Visible = false;
            else
                field.Visible = true;
        }

        private void treeBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!treeToolStripMenuItem.Checked)
                treeBox.Visible = false;
            else
                treeBox.Visible = true;
        }  
        
        void create_Click(object sender, EventArgs e)
        {
            if (!sizeCheck)
            {
                numericUpDown1.Visible = true;
                choose.Visible = true;
                sizeCheck = true;
            }
            else
            {
                // Обновление счётчиков
                fileExists = new int[1];
                fileExists[0] = 0;
                totalMemory = (int)numericUpDown1.Value;
                freeMemory = totalMemory;
                memory = new int[totalMemory];
                tree = new int[totalMemory * 2];
                blockSizeMax = totalMemory;
                numericUpDown2.Maximum = totalMemory;
                numericUpDown2.Maximum = (int)numericUpDown1.Value;
                // Вывод текста и графики
                RefreshData();
                toolStripStatusLabel1.Text = "Заполните память файлами";
                // Обновление кнопок
                numericUpDown1.Visible = false;
                choose.Visible = false;
                sizeCheck = false;
                erase.Enabled = true;
                create.Enabled = false;
                addFile.Enabled = true;
            }
        }

        void addFile_Click(object sender, EventArgs e)
        {
            if (!addCheck)
            {
                /// <первичное нажатие кнопки>
                numericUpDown2.Maximum = totalMemory;
                fileLabel.Text = "Введите размер файла";
                numericUpDown2.Visible = true;
                fileLabel.Visible = true;
                addCheck = true;
                deleteFile.Enabled = false;
                erase.Enabled = false;
                /// </первичное нажатие кнопки>
            }
            else
            {
                /// <вторичное нажатие кнопки>
                blockSizeRequired = (int)numericUpDown2.Value;
                fileLabel.Visible = false;
                numericUpDown2.Visible = false;
                int[] fileExists1 = fileExists;
                if (blockSizeRequired > freeMemory)
                    MessageBox.Show("Недостаточно памяти", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    if (blockSizeRequired > blockSizeMax)
                        MessageBox.Show("Нет свободных блоков нужного размера", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else
                    {
                        int exponent = 0;
                        fileCounter++;
                        blockSizeCurrent = totalMemory;
                        for (; ; )
                        {
                            if ((blockSizeRequired <= blockSizeCurrent) && (blockSizeRequired > (blockSizeCurrent / 2)))
                                break;
                            else
                            {
                                exponent++;
                                blockSizeCurrent /= 2;
                            }
                        }
                        int checkCounter;
                        for (checkCounter = (int)Math.Pow(2, exponent) - 1; checkCounter <= (int)Math.Pow(2, exponent + 1) - 2; checkCounter++)
                        {
                            if (tree[checkCounter] == 0 && Position(checkCounter) == true)
                            {
                                tree[checkCounter] = blockSizeRequired;
                                Add(checkCounter);
                                break;
                            }
                        }
                        fileUsed = new bool[fileCounter];
                        fileExists = new int[fileCounter];
                        for (int counter = 0; counter < fileCounter - 1; counter++)
                        {
                            fileExists[counter] = fileExists1[counter];
                            fileUsed[counter] = false;
                        }
                        fileExists[fileCounter - 1] = blockSizeRequired;
                        fileUsed[fileCounter - 1] = false;
                        outputCounter = 0;
                        MaxSize();
                        freeMemory -= blockSizeRequired;
                        // вывод графики
                        Output(totalMemory, 0);
                        toolStripStatusLabel1.Text = "Файл размером " + numericUpDown2.Value + " добавлен";
                        // Обновление кнопок
                        deleteFile.Enabled = true;

                    }
                }
                RefreshData();
                if (freeMemory == 0)
                    addFile.Enabled = false;
                addCheck = false;
                deleteFile.Enabled = true;
                erase.Enabled = true;
                /// </вторичное нажатие кнопки>
            }
        } 
        
        void Add(int treeNode)
        {
            {
                while (treeNode != 0)
                {
                    if (treeNode % 2 == 0)
                        treeNode = (treeNode - 1) / 2;
                    else
                    {
                        treeNode /= 2;
                        tree[treeNode] = -1;
                    }
                }
            }
        } 
        
        void Output(int totalMemory, int node)
        {
            /// <запрос на необходимость выполнения>
            bool permission = false;
            int lowerLimit=0, upperLimit=0, exponent=0;
            if (node == 0)
                permission = true;
            else
                if (node % 2 == 0)
                    permission = tree[(node - 1) / 2] == -1 ? true : false;
                else permission = tree[node / 2] == -1 ? true : false;
            /// </запрос на необходимость выполнения>
            if (permission)
            {
                while (true)
                {
                    if (node >= lowerLimit && node <= upperLimit)
                        break;
                    else
                    {
                        exponent++;
                        lowerLimit = upperLimit + 1;
                        upperLimit = 2 * upperLimit + 2;
                    }
                }
                /// <процедура поиска и занесения файла в массив памяти>
                if (tree[node] > 0)
                {
                    bool ok = false;
                    for (int counter = 0; counter < fileExists.Length; counter++)
                        if ((fileExists[counter] == tree[node]) && (fileUsed[counter] == false))
                        {
                            memory[outputCounter] = counter + 2;
                            fileUsed[counter] = true;
                            ok = true;
                            break;
                        }
                    if (ok)
                    {
                        for (int counter = 1; counter < tree[node]; counter++)
                            memory[outputCounter + counter] = 1;
                        outputCounter += totalMemory / (int)Math.Pow(2, exponent);
                    }
                    else
                        outputCounter += totalMemory / (int)Math.Pow(2, exponent);
                }
                else
                    if (tree[node] == 0)
                        outputCounter += totalMemory / (int)Math.Pow(2, exponent);
                /// </процедура поиска и занесения файла в массив памяти>
                Output(totalMemory, 2 * node + 1);
                Output(totalMemory, 2 * node + 2);
            }
        }

        void deleteFile_Click(object sender, EventArgs e)
        {
            if (!deleteCheck)
            {
                numericUpDown2.Maximum = fileCounter;
                numericUpDown2.Value = numericUpDown2.Maximum;
                fileLabel.Text = "Введите номер файла";
                numericUpDown2.Visible = true;
                fileLabel.Visible = true;
                deleteCheck = true;
                addFile.Enabled = false;
                erase.Enabled = false;
            }
            else
            {
                // Обновление счётчиков
                toolStripStatusLabel1.Text = "Файл №" + numericUpDown2.Value + " удалён";
                int intCounter = 0;
                /// <поиск нужного файла>
                for (int counter = 0; counter < totalMemory; counter++)
                    if (memory[counter] == (int)numericUpDown2.Value + 1)
                    {
                        memory[counter] = 0;
                        freeMemory++;
                        intCounter = counter + 1;
                        break;
                    }
                // поиск нужного файла
                /// <удаление файла из памяти>
                try
                {
                    while (memory[intCounter] == 1)
                    {
                        memory[intCounter] = 0;
                        freeMemory++;
                        intCounter++;
                    }
                }
                catch { };
                /// </удаление файла из памяти>
                Delete(fileExists[(int)numericUpDown2.Value - 1]); // объединение блоков
                /// <обновление номеров файлов>
                int[] fileExists1 = fileExists;
                fileExists = new int[fileCounter - 1];
                if (fileCounter > 0)
                {
                    /// <смещение первой части>
                    for (int counter = 0; counter < (int)numericUpDown2.Value; counter++)
                    {
                        try { fileExists[counter] = fileExists1[counter]; }
                        catch { };
                    }
                    /// <смещение второй части>
                    for (int counter = (int)numericUpDown2.Value - 1; counter < fileExists.Length; counter++)
                        fileExists[counter] = fileExists1[counter + 1];
                    /// <уменьшение номеров в массиве>
                    for (int counter = 0; counter < memory.Length; counter++)
                        if (memory[counter] > (int)numericUpDown2.Value)
                            memory[counter]--;
                }
                /// </обновление номеров файлов>
                // Обновление счётчиков
                fileCounter--;
                MaxSize();
                // Вывод текста и графики
                Output(totalMemory, 0);
                RefreshData();
                // обновление кнопок
                deleteFile.Enabled = true;
                if (fileCounter == 0)
                    deleteFile.Enabled = false;
                if (freeMemory != 0)
                    addFile.Enabled = true;
                numericUpDown2.Visible = false;
                fileLabel.Visible = false;
                deleteCheck = false;
                addFile.Enabled = true;
                erase.Enabled = true;
            }
        }

        void Delete(int numRequired)
        {
            int node = 0;
            while (true)
            {
                if (tree[node] == numRequired)
                    break;
                else
                    node++;
            }
            tree[node] = 0;
            while (node != 0)
            {
                if (tree[node % 2 == 0 ? node - 1 : node + 1] == 0 && tree[node] == 0)
                {
                    tree[node % 2 == 0 ? (node - 1) / 2 : node / 2] = 0;
                    node = node % 2 == 0 ? (node - 1) / 2 : node / 2;
                }
                else
                    break;
            }
        }

        void erase_Click(object sender, EventArgs e)
        {
            // Обновление счётчиков
            totalMemory = 0;
            freeMemory = 0;
            RefreshData();
            fileCounter = 0;
            memory = null;
            tree = null;
            // Вывод текста и графики
            field.Text = "";
            treeBox.Text = "";
            pictureBox1.Image = null;
            // Обновление кнопок
            create.Enabled = true;
            addFile.Enabled = false;
            deleteFile.Enabled = false;
            erase.Enabled = false;
        }
    }
}