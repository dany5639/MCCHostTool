using Microsoft.VisualBasic.Logging;
using System.Xml;
using System.Xml.Linq;

namespace MCCHostTool
{
    public partial class Form1 : Form
    {
        private static string logFile = $"{Directory.GetParent(Environment.ProcessPath)}\\MCCHostTool_settings.csv";
        private Dictionary<string, item> mapVariantsCollection;
        private Dictionary<string, item> gameVariantsCollection;
        public List<string> gameVariantOverrides_list = new List<string>();

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
            public string name__________;
            public string map_variant___;
            public string map_variant_de;
            public string game_variant__;
            public string game_variant_d;
            public string enabled_______;
            public string playerCount___;
            public string comment_______;
            public string quality_______;
            // public string description; // obsolete
        }

        private enum cell
        {
            name__________,
            map_variant___,
            map_variant_de,
            game_variant__,
            game_variant_d,
            enabled_______,
            playerCount___,
            comment_______,
            quality_______,
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

            // TODO handle cases where not all titles are installed
            foreach (var a in MCCtitles)
                comboBox2.Items.Add(a);

            comboBox2.SelectedIndex = comboBox2.Items.Count - 1;

            toolStripStatusLabel1.Text = "";

            dataGridView1.Rows.Clear();
            dataGridView1.Columns[(int)cell.name__________].Width = 90;
            dataGridView1.Columns[(int)cell.map_variant___].Width = 90;
            dataGridView1.Columns[(int)cell.map_variant_de].Width = 700;
            dataGridView1.Columns[(int)cell.game_variant__].Width = 90;
            dataGridView1.Columns[(int)cell.game_variant_d].Width = 90;
            dataGridView1.Columns[(int)cell.enabled_______].Width = 90;
            dataGridView1.Columns[(int)cell.playerCount___].Width = 90;
            dataGridView1.Columns[(int)cell.comment_______].Width = 90;
            dataGridView1.Columns[(int)cell.quality_______].Width = 90;
        }
        private void button1_Click_moveFiles(object sender, EventArgs e)
        {
            var itemss = mapVariantsCollection;

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
                MCCpath = @"E:\_DLD_Large\Games\DigitalRetail\SteamLibrary\steamapps\common\Halo The Master Chief Collection\";
            }

            if (!Directory.Exists(MCCpath))
            {
                toolStripStatusLabel1.Text = "WARNING: MCC path incorrect.";
                return;
            }

            var selectedTitle = comboBox2.SelectedItem as string;

            mapVariantsCollection = new Dictionary<string, item>();
            gameVariantsCollection = new Dictionary<string, item>();


            // clog($"readBungieFiles(map_variants)");
            readBungieFiles("map_variants");

            // clog($"readBungieFiles(map_variants_library)");
            readBungieFiles("map_variants_library");

            // clog($"readBungieFiles(game_variants)");
            readBungieFiles("game_variants");

            // clog($"readBungieFiles(game_variants_library)");
            readBungieFiles("game_variants_library");

            // read from file first, then somehow merge with actual files
            // would be good to checksum files to make sure it's the same version
            readItemsList();

            updateDataGridView1();
        }
        public void updateDataGridView1()
        {
            int i = -1;
            foreach (var a in mapVariantsCollection)
            {
                i++;
                dataGridView1.Rows.Add(a.Key);
                dataGridView1.Rows[i].Cells[(int)cell.name__________].Value = a.Value.name__________;
                dataGridView1.Rows[i].Cells[(int)cell.map_variant___].Value = a.Value.map_variant___;
                dataGridView1.Rows[i].Cells[(int)cell.map_variant_de].Value = a.Value.map_variant_de;
                dataGridView1.Rows[i].Cells[(int)cell.game_variant__].Value = a.Value.game_variant__;
                dataGridView1.Rows[i].Cells[(int)cell.game_variant_d].Value = a.Value.game_variant_d;
                dataGridView1.Rows[i].Cells[(int)cell.enabled_______].Value = a.Value.enabled_______;
                dataGridView1.Rows[i].Cells[(int)cell.playerCount___].Value = a.Value.playerCount___;
                dataGridView1.Rows[i].Cells[(int)cell.comment_______].Value = a.Value.comment_______;
                dataGridView1.Rows[i].Cells[(int)cell.quality_______].Value = a.Value.quality_______;

            }
        }
        private void readItemsList()
        {
            var lines = ReadCsv(logFile);

            foreach (var a in lines)
            {
                var line = a.Split(";".ToCharArray()[0]);

                string name__________ = line[(int)cell.name__________];
                string map_variant___ = line[(int)cell.map_variant___];
                string map_variant_de = line[(int)cell.map_variant_de];
                string game_variant__ = line[(int)cell.game_variant__];
                string game_variant_d = line[(int)cell.game_variant_d];
                string enabled_______ = line[(int)cell.enabled_______];
                string playerCount___ = line[(int)cell.playerCount___];
                string comment_______ = line[(int)cell.comment_______];
                string quality_______ = line[(int)cell.quality_______];

                // TODO WARNING loss of data possible, if file doesn't exist
                if (!mapVariantsCollection.ContainsKey(map_variant___))
                {
                    clog($"WARNING: missing settings in the config file for: {map_variant___}");
                    continue;
                }

                mapVariantsCollection[map_variant___].game_variant__ = game_variant__;
                mapVariantsCollection[map_variant___].game_variant_d = game_variant_d;

                mapVariantsCollection[map_variant___].playerCount___ = playerCount___;
                mapVariantsCollection[map_variant___].comment_______ = comment_______;
                mapVariantsCollection[map_variant___].quality_______ = quality_______;

            }

        }
        public void readBungieFiles(string path1)
        {
            var selectedTitle = comboBox2.SelectedItem as string;
            var MCCpath = folderBrowserDialog1.SelectedPath;

            var MCCtitleFolder = $"{MCCpath}\\{selectedTitle}\\{path1}";

            if (!Directory.Exists(MCCtitleFolder))
            {
                toolStripStatusLabel1.Text = $"WARNING: missing {path1} folder: {MCCtitleFolder}";
                return;
            }

            var filesFolder = Directory.EnumerateFiles($"{MCCtitleFolder}");

            if (filesFolder.Count() == 0)
            {
                toolStripStatusLabel1.Text = $"{MCCtitleFolder} {filesFolder.Count()}";
                return;
            }

            var variant = new List<string>();

            foreach (var a in filesFolder)
                variant.Add(new FileInfo(a).Name);

            //  clog($"path1: {path1}");
            // clog($"{path1} count: {variant.Count()}");

            // foreach (var a in variant)
            //     clog($"{path1}: {a}");

            if (path1 == "map_variants")
            {
                foreach (var a in variant)
                {
                    var map_variant_description = $"{getMvarDescription($"{MCCtitleFolder}\\{a}")}";

                    // clog($"{path1}: {a}; {map_variant_description}");

                    mapVariantsCollection.Add(a, new item
                    {
                        name__________ = $"",
                        map_variant___ = $"{a}",
                        map_variant_de = map_variant_description,
                        game_variant__ = $"",
                        game_variant_d = $"",
                        enabled_______ = $"1",
                        playerCount___ = $"",
                        comment_______ = $"",
                        quality_______ = $"",
                    });
                }
            }

            if (path1 == "map_variants_library")
            {
                foreach (var a in variant)
                {
                    var map_variant_description = $"{getMvarDescription($"{MCCtitleFolder}\\{a}")}";

                    // clog($"{path1}: {a}; {map_variant_description}");

                    if (!mapVariantsCollection.ContainsKey(a))
                        mapVariantsCollection.Add(a, new item
                        {
                            name__________ = $"",
                            map_variant___ = $"{a}",
                            map_variant_de = map_variant_description,
                            game_variant__ = $"",
                            game_variant_d = $"",
                            enabled_______ = $"0",
                            playerCount___ = $"",
                            comment_______ = $"",
                            quality_______ = $"",
                        });
                }
            }

            if (path1 == "game_variants")
            {
                foreach (var a in variant)
                {
                    var game_variant_description = $"{getMvarDescription($"{MCCtitleFolder}\\{a}")}";

                    // clog($"{path1}: {a}; {game_variant_description}");

                    gameVariantsCollection.Add(a, new item
                    {
                        name__________ = $"",
                        map_variant___ = $"",
                        map_variant_de = $"",
                        game_variant__ = $"{a}",
                        game_variant_d = game_variant_description,
                        enabled_______ = $"1",
                        playerCount___ = $"",
                        comment_______ = $"",
                        quality_______ = $"",
                    });
                }
            }

            if (path1 == "game_variants_library")
            {
                foreach (var a in variant)
                {
                    var game_variant_description = $"{getMvarDescription($"{MCCtitleFolder}\\{a}")}";

                    // clog($"{path1}: {a}; {game_variant_description}");

                    if (!gameVariantsCollection.ContainsKey(a))
                        gameVariantsCollection.Add(a, new item
                        {
                            name__________ = $"",
                            map_variant___ = $"",
                            map_variant_de = $"",
                            game_variant__ = $"{a}",
                            game_variant_d = game_variant_description,
                            enabled_______ = $"0",
                            playerCount___ = $"",
                            comment_______ = $"",
                            quality_______ = $"",
                        });
                }
            }

            fillComboboxWithGameVariantOverrides();

        }
        private void fillComboboxWithGameVariantOverrides()
        {

            // add the extra variants from the other folder to the combo box
            foreach (var a in gameVariantsCollection)
                gameVariantOverrides_list.Add(a.Key);

            comboBox1_gameVariantOverrides.Items.Clear();
            foreach (var a in gameVariantsCollection)
                comboBox1_gameVariantOverrides.Items.Add($"{a.Key}: {a.Value.game_variant_d}");

        }
        private string getMvarDescription(string filepath)
        {
            // Rech;
            // TODO: it's different from other titles

            string output = "";

            try
            {
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
            catch (Exception e)
            {
                clog(e.Message);
                return output;
            }

        }
        private void ComboBox1_OverrideGameVariant_changed()
        {
            // last: override game variants combobox is empty.
            // use variant name for lookup.

            // beaver_creek_cl_031.mvar;These forgotten structures were once the site of many bitter battles but have since been reclaimed by nature.  2-8 players
            if (comboBox1_gameVariantOverrides.SelectedItem == null)
                return;

            var gamevariantOverride = (comboBox1_gameVariantOverrides.SelectedItem).ToString();

            var newGameVariant = new item();

            // clog($"gamevariantOverride {comboBox1_gameVariantOverrides.SelectedIndex} {gamevariantOverride}");

            if (gamevariantOverride == null)
                return;

            int hits = 0;
            foreach (var f in gameVariantsCollection)
            {
                if (gamevariantOverride.StartsWith(f.Key))
                {
                    hits++;
                    newGameVariant = f.Value;
                }
            }

            var d = dataGridView1.SelectedCells;
            if (d.Count == 0)
                return;

            foreach (var e in d)
            {
                var f = (DataGridViewCell)e;
                var ci = f.ColumnIndex;
                var ri = f.RowIndex;

                dataGridView1.Rows[ri].Cells[(int)cell.game_variant__].Value = newGameVariant.game_variant__;
                dataGridView1.Rows[ri].Cells[(int)cell.game_variant_d].Value = newGameVariant.game_variant_d;
            }
        }
        /// <summary>
        /// When a cell is modified, dump all contents to the settings file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dumpSettings();

        }
        private void textBox2_TextChanged_overrideFilter(object sender, EventArgs e)
        {
            comboBox1_gameVariantOverrides.Items.Clear();
            foreach (var a in gameVariantOverrides_list)
            {
                string b = a.ToLower();
                if (b.Contains(textBox2.Text))
                    comboBox1_gameVariantOverrides.Items.Add(a);
            }


        }
        private void dumpSettings()
        {
            clog($"dumpSettings");

            List<string> output = new List<string>();

            var getDataTest0 = dataGridView1.Rows[0].Cells[(int)cell.name__________].Value as string;
            var getDataTest1 = dataGridView1.Rows[0].Cells[(int)cell.map_variant___].Value as string;
            var getDataTest2 = dataGridView1.Rows[0].Cells[(int)cell.map_variant_de].Value as string;
            var getDataTest3 = dataGridView1.Rows[0].Cells[(int)cell.game_variant__].Value as string;
            var getDataTest4 = dataGridView1.Rows[0].Cells[(int)cell.game_variant_d].Value as string;
            var getDataTest5 = dataGridView1.Rows[0].Cells[(int)cell.enabled_______].Value as string;
            var getDataTest6 = dataGridView1.Rows[0].Cells[(int)cell.playerCount___].Value as string;
            var getDataTest7 = dataGridView1.Rows[0].Cells[(int)cell.comment_______].Value as string;
            var getDataTest8 = dataGridView1.Rows[0].Cells[(int)cell.quality_______].Value as string;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                var row = dataGridView1.Rows[i];

                if (row.Cells[0].Value == null)
                    continue;
                string name__________ = "";
                string map_variant___ = "";
                string map_variant_de = "";
                string game_variant__ = "";
                string game_variant_d = "";
                string enabled_______ = "";
                string playerCount___ = "";
                string comment_______ = "";
                string quality_______ = "";

                if (row.Cells[(int)cell.name__________].Value == null) row.Cells[(int)cell.name__________].Value = "";
                if (row.Cells[(int)cell.map_variant___].Value == null) row.Cells[(int)cell.map_variant___].Value = "";
                if (row.Cells[(int)cell.map_variant_de].Value == null) row.Cells[(int)cell.map_variant_de].Value = "";
                if (row.Cells[(int)cell.game_variant__].Value == null) row.Cells[(int)cell.game_variant__].Value = "";
                if (row.Cells[(int)cell.game_variant_d].Value == null) row.Cells[(int)cell.game_variant_d].Value = "";
                if (row.Cells[(int)cell.enabled_______].Value == null) row.Cells[(int)cell.enabled_______].Value = "";
                if (row.Cells[(int)cell.playerCount___].Value == null) row.Cells[(int)cell.playerCount___].Value = "";
                if (row.Cells[(int)cell.comment_______].Value == null) row.Cells[(int)cell.comment_______].Value = "";
                if (row.Cells[(int)cell.quality_______].Value == null) row.Cells[(int)cell.quality_______].Value = "";

                name__________ = row.Cells[(int)cell.name__________].Value.ToString();
                map_variant___ = row.Cells[(int)cell.map_variant___].Value.ToString();
                map_variant_de = row.Cells[(int)cell.map_variant_de].Value.ToString();
                game_variant__ = row.Cells[(int)cell.game_variant__].Value.ToString();
                game_variant_d = row.Cells[(int)cell.game_variant_d].Value.ToString();
                enabled_______ = row.Cells[(int)cell.enabled_______].Value.ToString();
                playerCount___ = row.Cells[(int)cell.playerCount___].Value.ToString();
                comment_______ = row.Cells[(int)cell.comment_______].Value.ToString();
                quality_______ = row.Cells[(int)cell.quality_______].Value.ToString();

                // i need to learn a compact way to do this

                string outputline =
                    $"{name__________};" +
                    $"{map_variant___};" +
                    $"{map_variant_de};" +
                    $"{game_variant__};" +
                    $"{game_variant_d};" +
                    $"{enabled_______};" +
                    $"{playerCount___};" +
                    $"{comment_______};" +
                    $"{quality_______};";

                output.Add(outputline);
                // $"{description}";
            }

            WriteCsv(output, logFile);
        }

        private void comboBox1_gameVariantOverrides_SelectedValueChanged(object sender, EventArgs e)
        {
            // clog($"comboBox1_gameVariantOverrides_SelectedValueChanged().dumpSettings()");
            ComboBox1_OverrideGameVariant_changed();
            dumpSettings();
        }
    }
}