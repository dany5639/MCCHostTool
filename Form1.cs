using Microsoft.VisualBasic.Logging;
using System.Collections;
using System.IO;
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
        public string rootPath = "";
        public string gameTitle = "";
        public string absoluteVariantsPath = "";

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
            public string gameTitle_____;
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
            gameTitle_____,
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

#if DEBUG
            comboBox2.SelectedIndex = 3;
#endif

            // TODO: never used
            toolStripStatusLabel1.Text = "";

            dataGridView1.Rows.Clear();
            dataGridView1.Columns[(int)cell.gameTitle_____].Width = 90;
            dataGridView1.Columns[(int)cell.map_variant___].Width = 90;
            dataGridView1.Columns[(int)cell.map_variant_de].Width = 700;
            dataGridView1.Columns[(int)cell.game_variant__].Width = 90;
            dataGridView1.Columns[(int)cell.game_variant_d].Width = 700;
            dataGridView1.Columns[(int)cell.enabled_______].Width = 90;
            dataGridView1.Columns[(int)cell.playerCount___].Width = 90;
            dataGridView1.Columns[(int)cell.comment_______].Width = 90;
            dataGridView1.Columns[(int)cell.quality_______].Width = 90;

            ApplyColors();
        }
        private void button3_Click_locateMCC(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = rootPath;
            folderBrowserDialog1.ShowDialog();

            rootPath = folderBrowserDialog1.SelectedPath;

            if (rootPath.Length == 0)
            {
                toolStripStatusLabel1.Text = "WARNING: MCC path not selected. Use text box to enter MCC path.";
                rootPath = @"E:\_DLD_Large\Games\DigitalRetail\SteamLibrary\steamapps\common\Halo The Master Chief Collection\";
            }

            if (!Directory.Exists(rootPath))
            {
                toolStripStatusLabel1.Text = "WARNING: MCC path incorrect.";
                return;
            }

            dataGridView1.Rows.Clear();

            comboBox1_gameVariantOverrides.Items.Clear();

            mapVariantsCollection = new Dictionary<string, item>();
            gameVariantsCollection = new Dictionary<string, item>();
            gameVariantOverrides_list = new List<string>();


            readBungieFiles("map_variants");

            readBungieFiles("map_variants_library");

            readBungieFiles("game_variants");

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
                if (a.Value.gameTitle_____ != gameTitle)
                    continue;

                i++;
                dataGridView1.Rows.Add(a.Key);
                dataGridView1.Rows[i].Cells[(int)cell.gameTitle_____].Value = a.Value.gameTitle_____;
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
            if (!File.Exists(logFile))
                return;

            var lines = ReadCsv(logFile);

            foreach (var a in lines)
            {
                var line = a.Split(";".ToCharArray()[0]);

                string gameTitle_____ = line[(int)cell.gameTitle_____];
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
                    // clog($"WARNING: missing settings in the config file for: {map_variant___}");
                    continue;
                }

                mapVariantsCollection[map_variant___].gameTitle_____ = gameTitle_____;
                mapVariantsCollection[map_variant___].map_variant___ = map_variant___;
                // mapVariantsCollection[map_variant___].map_variant_de = map_variant_de;
                mapVariantsCollection[map_variant___].game_variant__ = game_variant__;
                // mapVariantsCollection[map_variant___].game_variant_d = game_variant_d;
                mapVariantsCollection[map_variant___].enabled_______ = enabled_______;
                mapVariantsCollection[map_variant___].playerCount___ = playerCount___;
                mapVariantsCollection[map_variant___].comment_______ = comment_______;
                mapVariantsCollection[map_variant___].quality_______ = quality_______;

            }

        }
        public void readBungieFiles(string variantsFolder)
        {

            gameTitle = comboBox2.SelectedItem as string;
            rootPath = folderBrowserDialog1.SelectedPath;

            absoluteVariantsPath = $"{rootPath}\\{gameTitle}\\{variantsFolder}";


            if (!Directory.Exists(absoluteVariantsPath))
            {
                toolStripStatusLabel1.Text = $"WARNING: missing {variantsFolder} folder: {absoluteVariantsPath}.";
                return;
            }

            var variantsFiles = Directory.EnumerateFiles($"{absoluteVariantsPath}");

            if (variantsFiles.Count() == 0)
            {
                toolStripStatusLabel1.Text = $"{absoluteVariantsPath}: {variantsFiles.Count()} files.";
                return;
            }

            var variantsList = new List<string>();

            foreach (var a in variantsFiles)
                variantsList.Add(new FileInfo(a).Name);

            //  clog($"path1: {path1}");
            // clog($"{path1} count: {variant.Count()}");

            // foreach (var a in variant)
            //     clog($"{path1}: {a}");


            clog($"readBungieFiles({variantsFolder})\n" +
                $"rootPath: {rootPath}\n" +
                $"gameTitle: {gameTitle}\n" +
                $"absoluteVariantsPath: {absoluteVariantsPath}\n" +
                $"variantsFiles.Count(): {variantsFiles.Count()}\n" +
                $"");

            foreach (var variant in variantsList)
            {
                if (variant.ToLower().Contains("legacy_arena_foundry_kentuckyTango"))
                    ;

                var variantDescription = "";

                switch (gameTitle)
                {
                    case "halo3":
                        switch (variantsFolder)
                        {
                            case "map_variants":
                            case "map_variants_library":
                                variantDescription = $"{readH3mvar($"{absoluteVariantsPath}\\{variant}")}";
                                goto done1;
                            case "game_variants":
                            case "game_variants_library":
                                variantDescription = $"{readH3gameVar($"{absoluteVariantsPath}\\{variant}")}";
                                goto done1;
                            default:
                                throw new Exception();
                        }
                    case "haloreach":
                        // switch (variantsFolder)
                        // {
                        //     case "map_variants":
                        //     case "map_variants_library":
                        //         variantDescription = $"{readH3mvar($"{absoluteVariantsPath}\\{variant}")}";
                        //         goto done1;
                        //     case "game_variants":
                        //     case "game_variants_library":
                        //         variantDescription = $"{readH3gameVar($"{absoluteVariantsPath}\\{variant}")}";
                        //         goto done1;
                        //     default:
                        //         throw new Exception();
                        // }

                        variantDescription = $"{readHRmvar($"{absoluteVariantsPath}\\{variant}")}";
                        break;
                    default:
                        throw new Exception();

                }

            done1:
                ;

                // clog($"{path1}: {a}; {map_variant_description}");

                var newVariant = new item
                {
                    gameTitle_____ = $"{gameTitle}",
                    playerCount___ = $"",
                    comment_______ = $"",
                    quality_______ = $"",
                };

                switch (variantsFolder)
                {
                    case "map_variants":
                        newVariant.enabled_______ = "1";
                        goto r_map_variants_library;
                    case "map_variants_library":
                    r_map_variants_library:
                        newVariant.map_variant___ = variant;
                        newVariant.map_variant_de = variantDescription;
                        if (!mapVariantsCollection.ContainsKey(variant))
                            mapVariantsCollection.Add(variant, newVariant);
                        goto done2;
                    case "game_variants":
                        newVariant.enabled_______ = "0";
                        goto r_game_variants_library;
                    case "game_variants_library":
                    r_game_variants_library:
                        newVariant.game_variant__ = variant;
                        newVariant.game_variant_d = variantDescription;
                        if (!gameVariantsCollection.ContainsKey(variant))
                            gameVariantsCollection.Add(variant, newVariant);
                        goto done2;
                    default:
                        throw new Exception();
                }

            done2:
                ;

            }

            switch (variantsFolder)
            {
                case "game_variants":
                case "game_variants_library":
                    fillComboboxWithGameVariantOverrides();
                    goto done3;
                default:
                    goto done3;
            }

        done3:
            ;

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
        public string readH3mvar(string filepath)
        {
            string output = "";
            string mapname = "";
            string mapDescription = "";
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(filepath)))
                {
                    // H3 is big endian
                    // reach are small endian
                    // or the other way

                    var offset = 0x94;
                    var buffer = reader.ReadBytes((int)reader.BaseStream.Length);

                    var outputLine = new List<string>();

                    var s = "";
                    var i = 0x94;
                    while (true)
                    {
                        if (buffer[i] == 0 && buffer[i + 1] == 0)
                        {
                            mapname = s;
                            i++;
                            i++;
                            goto gotMapName;
                        }

                        s = $"{s}{(char)buffer[i + 1]}";

                        i++;
                        i++;

                        // if (b < 0x1F || b > 0x7E)
                        // {
                        //     i++;
                        //     i++;
                        //     goto skip;
                        // }

                    }

                gotMapName:
                    s = "";
                    while (true)
                    {
                        if (buffer[i] == 0 && buffer[i + 1] == 0)
                        {
                            mapDescription = s;
                            output = mapDescription;
                            s = "";
                            goto lbDone;
                        }

                        s = $"{s}{(char)buffer[i]}";

                        i++;

                        // if (b < 0x1F || b > 0x7E)
                        // {
                        //     i++;
                        //     i++;
                        //     goto skip;
                        // }

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
        public string readHRmvar(string filepath)
        {
            string output = "";
            string mapname = "";
            string mapDescription = "";
            var mapnameOffset = 0xC0;
            var descriptionOffset = 0x1C0;
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(filepath)))
                {
                    var buffer = reader.ReadBytes((int)reader.BaseStream.Length);

                    var outputLine = new List<string>();

                    var s = "";
                    var i = mapnameOffset;
                    while (true)
                    {
                        if (buffer[i] == 0 && buffer[i + 1] == 0)
                        {
                            mapname = s;
                            i++;
                            i++;
                            goto gotMapName;
                        }

                        s = $"{s}{(char)buffer[i]}";

                        i++;
                        i++;

                        // if (b < 0x1F || b > 0x7E)
                        // {
                        //     i++;
                        //     i++;
                        //     goto skip;
                        // }

                    }

                gotMapName:
                    s = "";
                    i = descriptionOffset;
                    while (true)
                    {
                        if (buffer[i] == 0 && buffer[i + 1] == 0)
                        {
                            mapDescription = s;
                            output = mapDescription;
                            s = "";
                            goto lbDone;
                        }

                        s = $"{s}{(char)buffer[i]}";

                        i++;
                        i++;

                        // if (b < 0x1F || b > 0x7E)
                        // {
                        //     i++;
                        //     i++;
                        //     goto skip;
                        // }

                    }

                }

            lbDone:

                return mapDescription;
            }
            catch (Exception e)
            {
                clog(e.Message);
                return "";
            }

        }
        public string readH3gameVar(string filepath)
        {
            var mapNameOffset = 0x48;
            var mapDescriptionOffset = 0x68;
            string output = "";
            string mapname = "";
            string mapDescription = "";
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(filepath)))
                {
                    var buffer = reader.ReadBytes((int)reader.BaseStream.Length);

                    var outputLine = new List<string>();

                    var s = "";
                    var i = mapNameOffset;
                    while (true)
                    {
                        if (buffer[i] == 0 && buffer[i + 1] == 0)
                        {
                            mapname = s;
                            i++;
                            i++;
                            goto gotMapName;
                        }

                        s = $"{s}{(char)buffer[i + 1]}";

                        i++;
                        i++;

                        // if (b < 0x1F || b > 0x7E)
                        // {
                        //     i++;
                        //     i++;
                        //     goto skip;
                        // }

                    }

                gotMapName:
                    s = "";
                    i = mapDescriptionOffset;
                    while (true)
                    {
                        if ((buffer[i] == 0 && buffer[i + 1] == 0) || i > mapNameOffset + 0x80)
                        {
                            mapDescription = s;
                            output = mapDescription;
                            s = "";
                            goto lbDone;
                        }

                        s = $"{s}{(char)buffer[i]}";

                        i++;

                        // if (b < 0x1F || b > 0x7E)
                        // {
                        //     i++;
                        //     i++;
                        //     goto skip;
                        // }

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
        public string readVariantFile(string filepath)
        {
            string output = "";

            try
            {
                output = LoadMapInfo(filepath);
            }
            catch (Exception ex) { }

            return output;
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
            var getDataTest0 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.gameTitle_____].Value as string;
            var getDataTest1 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.map_variant___].Value as string;
            var getDataTest2 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.map_variant_de].Value as string;
            var getDataTest3 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.game_variant__].Value as string;
            var getDataTest4 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.game_variant_d].Value as string;
            var getDataTest5 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.enabled_______].Value as string;
            var getDataTest6 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.playerCount___].Value as string;
            var getDataTest7 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.comment_______].Value as string;
            var getDataTest8 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.quality_______].Value as string;

            mapVariantsCollection[getDataTest1].gameTitle_____ = getDataTest0;
            mapVariantsCollection[getDataTest1].map_variant___ = getDataTest1;
            mapVariantsCollection[getDataTest1].map_variant_de = getDataTest2;
            mapVariantsCollection[getDataTest1].game_variant__ = getDataTest3;
            mapVariantsCollection[getDataTest1].game_variant_d = getDataTest4;
            mapVariantsCollection[getDataTest1].enabled_______ = getDataTest5;
            mapVariantsCollection[getDataTest1].playerCount___ = getDataTest6;
            mapVariantsCollection[getDataTest1].comment_______ = getDataTest7;
            mapVariantsCollection[getDataTest1].quality_______ = getDataTest8;

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
            // clog($"dumpSettings");

            var output = new List<string>();

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

                if (row.Cells[(int)cell.gameTitle_____].Value == null) row.Cells[(int)cell.gameTitle_____].Value = "";
                if (row.Cells[(int)cell.map_variant___].Value == null) row.Cells[(int)cell.map_variant___].Value = "";
                if (row.Cells[(int)cell.map_variant_de].Value == null) row.Cells[(int)cell.map_variant_de].Value = "";
                if (row.Cells[(int)cell.game_variant__].Value == null) row.Cells[(int)cell.game_variant__].Value = "";
                if (row.Cells[(int)cell.game_variant_d].Value == null) row.Cells[(int)cell.game_variant_d].Value = "";
                if (row.Cells[(int)cell.enabled_______].Value == null) row.Cells[(int)cell.enabled_______].Value = "";
                if (row.Cells[(int)cell.playerCount___].Value == null) row.Cells[(int)cell.playerCount___].Value = "";
                if (row.Cells[(int)cell.comment_______].Value == null) row.Cells[(int)cell.comment_______].Value = "";
                if (row.Cells[(int)cell.quality_______].Value == null) row.Cells[(int)cell.quality_______].Value = "";

                name__________ = row.Cells[(int)cell.gameTitle_____].Value.ToString();
                map_variant___ = row.Cells[(int)cell.map_variant___].Value.ToString();
                // map_variant_de = row.Cells[(int)cell.map_variant_de].Value.ToString();
                game_variant__ = row.Cells[(int)cell.game_variant__].Value.ToString();
                // game_variant_d = row.Cells[(int)cell.game_variant_d].Value.ToString();
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
        private void button1_Click_moveFiles(object sender, EventArgs e)
        {
            var itemss = mapVariantsCollection;

            foreach (var a in mapVariantsCollection)
            {
                var map_variant___ = a.Value.map_variant___;
                var game_variant__ = a.Value.game_variant__;

                var path1 = $"{rootPath}\\{gameTitle}\\map_variants\\{map_variant___}";
                var path2 = $"{rootPath}\\{gameTitle}\\map_variants_library\\{map_variant___}";
                var path3 = $"{rootPath}\\{gameTitle}\\game_variants\\{game_variant__}";
                var path4 = $"{rootPath}\\{gameTitle}\\game_variants_library\\{game_variant__}";

                // current problem: won't move game variants back if they are not used by any map

                var enabled = a.Value.enabled_______;

                if (enabled == "0")
                {
                    // move map variants
                    if (File.Exists(path1))
                    {
                        if (!File.Exists(path2))
                        {
                            // clog($"moveStuf: File.Move({path1}, {path2})");
                            try
                            {
                                File.Move(path1, path2);
                            }
                            catch (Exception ex)
                            {
                                clog($"moveStuf: File.Move({path1}, {path2}) exceptio: {ex.Message}");
                            }
                        }
                        else
                        {
                            // clog($"moveStuf: File.Delete({path1})");
                            File.Delete(path1);
                        }
                    }

                    // move map game_variants
                    if (File.Exists(path3))
                    {
                        if (!File.Exists(path4))
                        {
                            // clog($"moveStuf: File.Move({path3}, {path4})");
                            try
                            {
                                File.Move(path3, path4);
                            }
                            catch (Exception ex)
                            {
                                clog($"moveStuf: File.Move({path3}, {path4}) exceptio: {ex.Message}");
                            }
                        }
                        else
                        {
                            // clog($"moveStuf: File.Delete({path3})");
                            File.Delete(path3);
                        }
                    }

                }
                else if (enabled == "1")
                {
                    clog($"moveStuf: enabled: File.Move({path2}, {path1})");
                    // move map variants
                    if (File.Exists(path2))
                    {
                        if (!File.Exists(path1))
                        {
                            // clog($"moveStuf: File.Move({path2}, {path1})");
                            try
                            {
                                File.Move(path2, path1);
                            }
                            catch (Exception ex)
                            {
                                clog($"moveStuf: File.Move({path2}, {path1}) exceptio: {ex.Message}");
                            }
                        }
                        else
                        {
                            // clog($"WARNING: moveStuf: File.Exist({path1})");
                        }
                    }
                    else
                    {
                        // handle wether it was moved by another map variant
                        // clog($"WARNING: moveStuf: !File.Exists({path2})");
                    }

                    // move map game_variants
                    if (File.Exists(path4))
                    {
                        if (!File.Exists(path3))
                        {
                            // clog($"moveStuf: File.Move({path4}, {path3})");
                            try
                            {
                                File.Move(path4, path3);
                            }
                            catch (Exception ex)
                            {
                                clog($"moveStuf: File.Move({path4}, {path3}) exceptio: {ex.Message}");
                            }
                        }
                        else
                        {
                            // clog($"WARNING: moveStuf: File.Exist({path3})");
                        }
                    }
                    else
                    {
                        // handle wether it was moved by another map variant
                        // clog($"WARNING: moveStuf: !File.Exists({path4})");
                    }
                }
                else
                {
                    // clog($"WARNING: {enabled} for enabled or not should be a 0 or 1.");
                }


            }

        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void comboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            gameTitle = comboBox2.SelectedItem as string;
        }

        [Flags]
        public enum LevelFlags
        {
            None = 0,
            Unknown0 = 1 << 0,
            Unknown1 = 1 << 1,
            Visible = 1 << 2,
            GeneratesFilm = 1 << 3,
            IsMainMenu = 1 << 4,
            IsCampaign = 1 << 5,
            IsMultiplayer = 1 << 6,
            IsDLC = 1 << 7,
            Unknown8 = 1 << 8,
            Unknown9 = 1 << 9,
            IsFirefight = 1 << 10,
            IsCinematic = 1 << 11,
            IsForgeOnly = 1 << 12,
            Unknown13 = 1 << 13,
            Unknown14 = 1 << 14,
            Unknown15 = 1 << 15,
        }

        private MaplevlInfo _mapInformation;
        private BinaryReader _stream;
        private const int MapNamesOffset = 0x44;
        private int _mapDescriptionsOffset;
        private int _physicalNameOffset;
        private int _internalNameOffset;
        private int _mapIndexOffset;
        private int _maxTeamsOffset;
        private int _mpObjectsOffset;
        private int _insertionOffset;
        private int _defaultAuthorOffset;

        private void UpdateOffsets()
        {
            _mapDescriptionsOffset = (1 * 0x40) + MapNamesOffset;
            _physicalNameOffset = (1 * 0x100) + _mapDescriptionsOffset;
            _internalNameOffset = _physicalNameOffset + 0x100;
            _mapIndexOffset = _internalNameOffset + 0x100;
            _maxTeamsOffset = _mapIndexOffset + 0xA;
            _mpObjectsOffset = _mapIndexOffset + 0x14;
            // _insertionOffset = Engine.MultiplayerObjectCollection != null ? _mapIndexOffset + 0x114 : _mapIndexOffset + 0x1C; 
            _insertionOffset = _mapIndexOffset + 0x114;
            _defaultAuthorOffset = _insertionOffset + (1 * 0);
        }


        #region Loading Code

        public string LoadMapInfo(string filepath)
        {
            _stream = new BinaryReader(File.OpenRead(filepath));

            _mapInformation = new MaplevlInfo();

            // Find out which engine the file uses
            _stream.BaseStream.Position = (0x34);
            var size = _stream.ReadInt32();
            _stream.BaseStream.Position = (0x38);
            var version = _stream.ReadUInt16();

            // Update offsets based on engine info
            UpdateOffsets();

            // Load Map ID
            _stream.BaseStream.Position = (0x3C);
            _mapInformation.MapID = _stream.ReadInt32();

            // Load Flags
            _stream.BaseStream.Position = (0x42);
            _mapInformation.Flags = (LevelFlags)_stream.ReadInt16();

            // Load Map Names and Descriptions
            LoadMapNames(MapNamesOffset);
            LoadMapDescriptions(_mapDescriptionsOffset);

            // Load Map Physical Name
            var a = _stream.ReadChars(0x64);
            var b = "";
            foreach (var c in a)
                b = $"{b}{c}";

            _stream.BaseStream.Position = (_physicalNameOffset);
            _mapInformation.PhysicalName = b; // WHAT SIZE

            // Load Map Internal Name
            var a1 = _stream.ReadChars(0x64);
            var b1 = "";
            foreach (var c1 in a1)
                b1 = $"{b1}{c1}";
            _stream.BaseStream.Position = (_internalNameOffset);
            _mapInformation.InternalName = b1; // WHAT SIZE

            // Load Map Index
            _stream.BaseStream.Position = (_mapIndexOffset);
            _mapInformation.MapIndex = _stream.ReadInt32();

            // Load Max Teams ?
            // if (Engine.MaxTeamCollection != null)
            //     LoadMapMaxTeams(_maxTeamsOffset);

            // Load Multiplayer Object Table
            // if (Engine.MultiplayerObjectCollection != null)
            //     LoadMPObjectTable(_mpObjectsOffset);

            // Load Insertion Points
            LoadInsertionPoints(_insertionOffset);

            // Load Default Author Name
            // might fail because the previous 3 got disabled
            _stream.BaseStream.Position = (_defaultAuthorOffset);
            _mapInformation.DefaultAuthor = _stream.ReadChars(0x64).ToString(); // WHAT SIZE

            return _mapInformation.PhysicalName;
        }

        private void LoadMapNames(int baseOffset)
        {
            _mapInformation.MapNames.Clear();

            for (int i = 0; i < 1; i++) // language count
            {
                _stream.BaseStream.Position = (baseOffset + (i * 0x40));
                _mapInformation.MapNames.Add(_stream.ReadChars(0x64).ToString()); // WHAT SIZE
            }
        }

        private void LoadMapDescriptions(int baseOffset)
        {
            _mapInformation.MapDescriptions.Clear();

            for (int i = 0; i < 1; i++) // lg
            {
                _stream.BaseStream.Position = (baseOffset + (i * 0x100));
                _mapInformation.MapDescriptions.Add(_stream.ReadChars(0x64).ToString()); // WHAT SIZE
            }
        }

        private void LoadInsertionPoints(int baseOffset)
        {
            _mapInformation.MapCheckpoints.Clear();
        }

        #endregion

        public class MaplevlInfo
        {
            public int MapID { get; set; }
            public LevelFlags Flags { get; set; }
            public IList<string> MapNames = new List<string>();
            public IList<string> MapDescriptions = new List<string>();
            public string InternalName { get; set; }
            public string PhysicalName { get; set; }
            public int MapIndex { get; set; }
            public IList<byte> MaxTeamCounts = new List<byte>();
            public BitArray ObjectTable { get; set; }
            public IList<Checkpoint> MapCheckpoints = new List<Checkpoint>();
            public string DefaultAuthor { get; set; }
        }

        public class Checkpoint
        {
            public bool IsVisible { get; set; }
            public bool IsUsed { get; set; }
            public byte ZoneIndex { get; set; }
            public string ZoneName { get; set; }
            public IList<string> CheckpointNames = new List<string>();
            public IList<string> CheckpointDescriptions = new List<string>();
        }

        public void ApplyColors()
        {
            var BackColor = Color.White;
            var ForeColor = Color.Black;
            this.BackColor = BackColor;
            this.ForeColor = ForeColor;
            // zcomboBox1_gameVariantOverrides.BackColor = BackColor;
            // zcomboBox1_gameVariantOverrides.ForeColor = ForeColor;
            // zcomboBox2.BackColor = BackColor;
            // zcomboBox2.ForeColor = ForeColor;
            // ztextBox1.BackColor = BackColor;
            // ztextBox1.ForeColor = ForeColor;
            // ztextBox2.BackColor = BackColor;
            // ztextBox2.ForeColor = ForeColor;
            // zdataGridView1.ForeColor = BackColor;
            // zdataGridView1.BackColor = ForeColor;
        }

        private void comboBox1_gameVariantOverrides_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}