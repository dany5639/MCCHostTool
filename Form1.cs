using Microsoft.VisualBasic.Logging;
using System.Xml;
using System.Xml.Linq;

namespace MCCHostTool
{
    public partial class Form1 : Form
    {
        private static string logFile = $"{Directory.GetParent(Environment.ProcessPath)}\\MCCHostTool_settings.csv";
        private Dictionary<string, item> items;
        public List<string> gameVariantOverrides = new List<string>();

        #region utilities
        private static void clog(string in_)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("hhmmss:fff")}] {in_}");
        }
        private void log(string message)
        {
            toolStripStatusLabel1.Text = $"{DateTime.Now.ToString("yyyyMMdd hhmmss")}{message}";
            clog(message);
        }
        private static List<string> ReadCsv(string filename, int lineCount = -1)
        {
            clog("WriteCsv");
            var output = new List<string>();

            using (var reader = new StreamReader(filename))
            {
                var line = "";
                int j = -1;
                while (line != null)
                {
                    j++;
                    if (lineCount != -1)
                        if (j == lineCount)
                            return output;

                    line = reader.ReadLine();
                    output.Add(line);
                }

                if (output.Last() == null)
                    output.RemoveAt(output.Count - 1);
            }

            Console.WriteLine($"Read {output.Count} lines.");

            return output;
        }
        private static void WriteCsv(List<string> in_, string file)
        {
            clog("WriteCsv");

            var fileOut = new FileInfo(file);
            if (File.Exists(file))
                File.Delete(file);

            Console.WriteLine($"Writing {in_.Count} lines to {file}.");

            int i = -1;
            try
            {
                using (var csvStream = fileOut.OpenWrite())
                using (var csvWriter = new StreamWriter(csvStream))
                {
                    foreach (var a in in_)
                    {
                        csvStream.Position = csvStream.Length;
                        csvWriter.WriteLine(a);
                        i++;
                    }
                }
            }
            catch
            { }
        }
        #endregion
   
        #region classes
        private class item
        {
            public string name;
            public string map_variant;
            public string map_variant_description;
            public string game_variant;
            public string game_variant_description;
            public string enabled;
            public string playerCount;
            public string comment;
            public string quality;
            // public string description; // obsolete
        }

        public List<string> MCCtitles = new List<string> {
            "groundhog",
            "halo1",
            "halo2",
            "halo3",
            "halo3odst",
            "halo4",
            "haloreach"
        };
        #endregion

        public Form1()
        {
            InitializeComponent();

            foreach (var a in MCCtitles)
                comboBox2.Items.Add(a);

            comboBox2.SelectedIndex = comboBox2.Items.Count - 1;

            toolStripStatusLabel1.Text = "";
        }
        private void readItemsList()
        {
            var lines = ReadCsv(logFile);

            foreach (var a in lines)
            {
                // TODO

            }

        }
        private void button1_Click_moveFiles(object sender, EventArgs e)
        {
            var itemss = items;

        }
        private void button3_Click_locateMCC(object sender, EventArgs e)
        {
            string MCCpath = "";

            folderBrowserDialog1.SelectedPath = MCCpath;
            folderBrowserDialog1.ShowDialog();

            MCCpath = folderBrowserDialog1.SelectedPath;

            if (MCCpath.Length == 0)
            {
                toolStripStatusLabel1.Text = "WARNING: MCC path not selected. Use text box to enter MCC path.";
                MCCpath = @"D:\DATA\REDLD\VG\Steam\steamapps\common\Halo The Master Chief Collection\";
            }

            if (!Directory.Exists(MCCpath))
            {
                toolStripStatusLabel1.Text = "WARNING: MCC path incorrect.";
                return;
            }

            var selectedTitle = comboBox2.SelectedItem as string;

            items = new Dictionary<string, item>();

            clog($"readBungieFiles(map_variants)");
            readBungieFiles("map_variants");

            clog($"readBungieFiles(map_variants_library)");
            readBungieFiles("map_variants_library");

            clog($"readBungieFiles(game_variants)");
            readBungieFiles("game_variants");

            clog($"readBungieFiles(game_variants_library)");
            readBungieFiles("game_variants_library");

            dataGridView1.Rows.Clear();
            dataGridView1.Columns[0].Width = 90;
            dataGridView1.Columns[1].Width = 90;
            dataGridView1.Columns[2].Width = 700;
            dataGridView1.Columns[3].Width = 90;
            dataGridView1.Columns[4].Width = 90;
            dataGridView1.Columns[5].Width = 90;
            dataGridView1.Columns[6].Width = 90;
            dataGridView1.Columns[7].Width = 90;
            // dataGridView1.Columns[8].Width = 90;

            int i = -1;
            foreach (var a in items)
            {
                i++;
                dataGridView1.Rows.Add(a.Key);
                dataGridView1.Rows[i].Cells[0].Value = a.Value.name;
                dataGridView1.Rows[i].Cells[1].Value = a.Value.map_variant;
                dataGridView1.Rows[i].Cells[2].Value = a.Value.map_variant_description;
                dataGridView1.Rows[i].Cells[3].Value = a.Value.game_variant;
                dataGridView1.Rows[i].Cells[4].Value = a.Value.game_variant_description;
                dataGridView1.Rows[i].Cells[5].Value = a.Value.enabled;
                dataGridView1.Rows[i].Cells[6].Value = a.Value.playerCount;
                dataGridView1.Rows[i].Cells[7].Value = a.Value.comment;
                dataGridView1.Rows[i].Cells[8].Value = a.Value.quality;
                // dataGridView1.Rows[i].Cells[9].Value = a.Value.description;

            }

        }
        public void readBungieFiles(string path1)
        {
            var selectedTitle = comboBox2.SelectedItem as string;
            var MCCpath = folderBrowserDialog1.SelectedPath;

            var mvarPath = $"{MCCpath}\\{selectedTitle}\\{path1}";

            if (!Directory.Exists(mvarPath))
            {
                toolStripStatusLabel1.Text = $"WARNING: missing {path1} folder: {mvarPath}";
                return;
            }

            var map_variantsFolder = Directory.EnumerateFiles($"{mvarPath}");

            if (map_variantsFolder.Count() == 0)
            {
                toolStripStatusLabel1.Text = $"{mvarPath} {map_variantsFolder.Count()}";
                return;
            }   
            
            var variant = new List<string>();

            foreach (var a in map_variantsFolder)
                variant.Add(new FileInfo(a).Name);

            clog($"{path1}: {variant.Count()}");

            // foreach (var a in variant)
            //     clog($"{path1}: {a}");

            if (path1 == "game_variants_library")
            {
                clog("game_variants_library");
                foreach (var a in variant)
                    gameVariantOverrides.Add(a);
            }

            if (path1 == "map_variants")
            {
                clog("map_variants");

                foreach (var a in variant)
                {
                    var map_variant_description = $"{getMvarDescription($"{mvarPath}\\{a}")}";

                    items.Add(a, new item
                    {
                        name = $"{a}",
                        map_variant = $"{a}",
                        map_variant_description = map_variant_description,
                        game_variant = $"",
                        game_variant_description = $"",
                        enabled = $"1",
                        playerCount = $"",
                        comment = $"",
                        quality = $"",
                        // description = $""
                    });
                }
            }

            if (path1 == "map_variants_library")
            {
                clog("map_variants_library");

                foreach (var a in variant)
                {
                    var map_variant_description = $"{getMvarDescription($"{mvarPath}\\{a}")}";

                    if (!items.ContainsKey(a))
                    items.Add(a, new item
                    {
                        name = $"{a}",
                        map_variant = $"{a}",
                        map_variant_description = map_variant_description,
                        game_variant = $"",
                        game_variant_description = $"",
                        enabled = $"0",
                        playerCount = $"",
                        comment = $"",
                        quality = $"",
                        // description = $""
                    });
                }
            }

        }
        private string getMvarDescription(string filepath)
        {
            string output = "";

            using (var reader = new BinaryReader(File.OpenRead(filepath)))
            {
                var descriptionLength = 0x0130;
                var descriptionOffset = 0x01C0;
                var buffer = reader.ReadBytes(descriptionLength + descriptionOffset);

                var outputLine = new List<string>();

                var s = "";
                // pass 2, find unicode strings
                var i = 0;
                while (true)
                {
                    var b = buffer[i + descriptionOffset];
                    if (b == 0)
                    {
                        output = s;
                        goto lbDone;
                    }

                    if (b < 0x1F || b > 0x7E)
                    {
                        i++;
                        i++;
                        goto fuckingcuntsusingbadchar;
                    }

                    s = $"{s}{(char)b}";
                    i++;
                    i++;

                    if (i == descriptionLength)
                    {
                        output = s;
                        goto lbDone;
                    }

                fuckingcuntsusingbadchar:
                    ;
                }
            }
        lbDone:

            return output;
        }

        private void comboBox1_SelectedIndexChanged_overrideGameVariant(object sender, EventArgs e)
        {
            var combobox1_selectedIndex = 0;
            var datagrid1_selectedIndex = 0;

            var gamevariantOverride = (comboBox1_gameVariantOverrides.SelectedItem).ToString();
            clog($"gamevariantOverride {comboBox1_gameVariantOverrides.SelectedIndex} {gamevariantOverride}");

        }
        /// <summary>
        /// When a cell is modified, dump all contents to the settings file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            List<string> output = new List<string>();

            var getDataTest0 = dataGridView1.Rows[0].Cells[0].Value as string;
            var getDataTest1 = dataGridView1.Rows[0].Cells[1].Value as string;
            var getDataTest2 = dataGridView1.Rows[0].Cells[2].Value as string;
            var getDataTest3 = dataGridView1.Rows[0].Cells[3].Value as string;
            var getDataTest4 = dataGridView1.Rows[0].Cells[4].Value as string;
            var getDataTest5 = dataGridView1.Rows[0].Cells[5].Value as string;
            var getDataTest6 = dataGridView1.Rows[0].Cells[6].Value as string;
            var getDataTest7 = dataGridView1.Rows[0].Cells[7].Value as string;
            var getDataTest8 = dataGridView1.Rows[0].Cells[8].Value as string;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                var row = dataGridView1.Rows[i];

                if (row.Cells[0].Value == null)
                    continue;

                string name_ = row.Cells[0].Value.ToString();
                string map_variant = row.Cells[1].Value.ToString();
                string map_variant_description = row.Cells[2].Value.ToString();
                string game_variant = row.Cells[3].Value.ToString();
                string game_variant_description = row.Cells[4].Value.ToString();
                string enabled = row.Cells[5].Value.ToString();
                string playerCount = row.Cells[6].Value.ToString();
                string comment = row.Cells[7].Value.ToString();
                string quality = row.Cells[8].Value.ToString();
                // string description = row.Cells[7].Value.ToString();
                string outputline =
                    $"{name_};" +
                    $"{map_variant};" +
                    $"{map_variant_description};" +
                    $"{game_variant};" +
                    $"{game_variant_description};" +
                    $"{enabled};" +
                    $"{playerCount};" +
                    $"{comment};" +
                    $"{quality};";

                output.Add(outputline);
                // $"{description}";
            }

            WriteCsv(output, logFile);

        }
        private void textBox2_TextChanged_overrideFilter(object sender, EventArgs e)
        {
            comboBox1_gameVariantOverrides.Items.Clear();
            foreach (var a in gameVariantOverrides)
            {
                string b = a.ToLower();
                if (b.Contains(textBox2.Text))
                    comboBox1_gameVariantOverrides.Items.Add(a);
            }
        }

    }
}