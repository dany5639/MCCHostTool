using Microsoft.VisualBasic.Logging;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace MCCHostTool
{
    /*
     * TODO
     * read write properly to config file;
     * handle excess items per line in config file;
     * handle files when they're both in map_variants and map_variants_library
     * identify map files that are actually game variants, and game variants that are actually map files,mvar magic
     * see why changing halo title breaks the config file
     * ignore map variants that haven't had info modified
     * dark mode V2
     * 
     */

    public partial class Form1 : Form
    {
        private static string logFile = $"{Directory.GetParent(Environment.ProcessPath)}\\MCCHostTool_settings.csv";
        private Dictionary<string, item> mapVariantsCollection;
        private Dictionary<string, item> gameVariantsCollection;
        public List<string> gameVariantOverrides_list = new List<string>();
        public string rootPath = @"E:\_DLD_Large\Games\DigitalRetail\SteamLibrary\steamapps\common\Halo The Master Chief Collection\";
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

            return output;
        }
        private static void WriteCsv(List<string> in_, string file)
        {
            var fileOut = new FileInfo(file);
            if (File.Exists(file))
                File.Delete(file);

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
            public string game_variant__;
            public string enabled_______;
            public string comment_______;
            public string quality_______;
            public string map___________;
            public string gameMode______;
            public string playerCount___;
            public string map_var_descrp;
            public string game_var_descr;
            // public string description; // obsolete
        }

        private enum cell
        {
            gameTitle_____,
            map_variant___,
            game_variant__,
            enabled_______,
            comment_______,
            quality_______,
            map___________,
            gameMode______,
            playerCount___,
            map_variant_de,
            game_variant_d,
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

            // TODO: never used
            toolStripStatusLabel1.Text = "";

            // gameTitle;mapVariant;mapVariantDescription;gameVariant;gameVariantDescription;enabled;playerCount;comment;quality;map;gameMode
            // gameTitle;mapVariant;gameVariant;enabled;comment;quality;map;gameMode;playerCount;mapVariantDescription;gameVariantDescription
            dataGridView1.Rows.Clear();
            dataGridView1.Columns[(int)cell.gameTitle_____].Width = 90;
            dataGridView1.Columns[(int)cell.map_variant___].Width = 90;
            dataGridView1.Columns[(int)cell.game_variant__].Width = 90;
            dataGridView1.Columns[(int)cell.enabled_______].Width = 90;
            dataGridView1.Columns[(int)cell.comment_______].Width = 90;
            dataGridView1.Columns[(int)cell.quality_______].Width = 90;
            dataGridView1.Columns[(int)cell.map___________].Width = 90;
            dataGridView1.Columns[(int)cell.gameMode______].Width = 90;
            dataGridView1.Columns[(int)cell.playerCount___].Width = 90;
            dataGridView1.Columns[(int)cell.map_variant_de].Width = 700;
            dataGridView1.Columns[(int)cell.game_variant_d].Width = 700;

            ApplyColors();

            if (File.Exists($"{rootPath}\\mcclauncher.exe"))
            {
                MCClocated();
            }
        }
        private void button3_Click_locateMCC(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = rootPath;
            folderBrowserDialog1.ShowDialog();

            rootPath = folderBrowserDialog1.SelectedPath;

            if (File.Exists($"{rootPath}\\mcclauncher.exe"))
            {
                MCClocated();
                return;
            }

            toolStripStatusLabel1.Text = "WARNING: MCC path incorrect.";

        }
        private void MCClocated()
        {
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
                i++;
                dataGridView1.Rows.Add(a.Key);
                dataGridView1.Rows[i].Cells[(int)cell.gameTitle_____].Value = a.Value.gameTitle_____;
                dataGridView1.Rows[i].Cells[(int)cell.map_variant___].Value = a.Value.map_variant___;
                dataGridView1.Rows[i].Cells[(int)cell.game_variant__].Value = a.Value.game_variant__;
                dataGridView1.Rows[i].Cells[(int)cell.enabled_______].Value = a.Value.enabled_______;
                dataGridView1.Rows[i].Cells[(int)cell.comment_______].Value = a.Value.comment_______;
                dataGridView1.Rows[i].Cells[(int)cell.quality_______].Value = a.Value.quality_______;
                dataGridView1.Rows[i].Cells[(int)cell.map___________].Value = a.Value.map___________;
                dataGridView1.Rows[i].Cells[(int)cell.gameMode______].Value = a.Value.gameMode______;
                dataGridView1.Rows[i].Cells[(int)cell.playerCount___].Value = a.Value.playerCount___;
                dataGridView1.Rows[i].Cells[(int)cell.map_variant_de].Value = a.Value.map_var_descrp;
                dataGridView1.Rows[i].Cells[(int)cell.game_variant_d].Value = a.Value.game_var_descr;

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

                string gameTitle_____ = "";
                string map_variant___ = "";
                string game_variant__ = "";
                string enabled_______ = "";
                string comment_______ = "";
                string quality_______ = "";
                string playerCount___ = "";
                string map_variant_de = "";
                string game_variant_d = "";
                string map___________ = "";
                string gameMode______ = "";

                // extra silly
                if (line.Length > 0)
                    gameTitle_____ = line[(int)cell.gameTitle_____];
                if (line.Length > 1)
                    map_variant___ = line[(int)cell.map_variant___];
                if (line.Length > 2)
                    game_variant__ = line[(int)cell.game_variant__];
                if (line.Length > 3)
                    enabled_______ = line[(int)cell.enabled_______];
                if (line.Length > 4)
                    comment_______ = line[(int)cell.comment_______];
                if (line.Length > 5)
                    quality_______ = line[(int)cell.quality_______];
                if (line.Length > 6)
                    playerCount___ = line[(int)cell.playerCount___];
                if (line.Length > 7)
                    map_variant_de = line[(int)cell.map_variant_de];
                if (line.Length > 8)
                    game_variant_d = line[(int)cell.game_variant_d];
                if (line.Length > 9)
                    map___________ = line[(int)cell.map___________];
                if (line.Length > 10)
                    gameMode______ = line[(int)cell.gameMode______];

                if (!MCCtitles.Contains(gameTitle_____))
                    throw new Exception($"CSV parsing error: gameTitle, expected halo3 or haloreach, got: {gameTitle_____}");

                if (enabled_______ != "1" && enabled_______ != "0")
                    throw new Exception($"CSV parsing error: mapVariant enable toggle: expected 0 or 1, got: {enabled_______}");

                // TODO WARNING loss of data possible, if file doesn't exist
                if (!mapVariantsCollection.ContainsKey(map_variant___))
                {
                    // clog($"WARNING: missing settings in the config file for: {map_variant___}");
                    continue;
                }

                mapVariantsCollection[map_variant___].gameTitle_____ = gameTitle_____;
                mapVariantsCollection[map_variant___].map_variant___ = map_variant___;
                mapVariantsCollection[map_variant___].game_variant__ = game_variant__;
                mapVariantsCollection[map_variant___].enabled_______ = enabled_______;
                mapVariantsCollection[map_variant___].comment_______ = comment_______;
                mapVariantsCollection[map_variant___].quality_______ = quality_______;
                mapVariantsCollection[map_variant___].map___________ = map___________;
                mapVariantsCollection[map_variant___].gameMode______ = gameMode______;
                mapVariantsCollection[map_variant___].playerCount___ = playerCount___;
                // mapVariantsCollection[map_variant___].map_variant_de = map_variant_de;
                // mapVariantsCollection[map_variant___].game_variant_d = game_variant_d;


            }

        }
        public void readBungieFiles(string variantsFolder)
        {
            foreach (var gameTitle in MCCtitles)
            {
                // just skip other titles for now
                switch (gameTitle)
                {
                    case "halo3":
                        clog($"readBungieFiles({variantsFolder}).halo3");
                        break;
                    case "haloreach":
                        clog($"readBungieFiles({variantsFolder}).haloreach");
                        break;
                    default:
                        continue;

                }

                absoluteVariantsPath = $"{rootPath}\\{gameTitle}\\{variantsFolder}";

                if (!Directory.Exists(absoluteVariantsPath))
                {
                    throw new Exception($"WARNING: missing {variantsFolder} folder: {absoluteVariantsPath}");

                    // toolStripStatusLabel1.Text = $"WARNING: missing {variantsFolder} folder: {absoluteVariantsPath}.";
                    // return;
                }

                var variantsFiles = Directory.EnumerateFiles($"{absoluteVariantsPath}");

                toolStripStatusLabel1.Text = $"{absoluteVariantsPath}: {variantsFiles.Count()} files.";

                if (variantsFiles.Count() == 0)
                {
                    clog($"WARNING: no files found in {absoluteVariantsPath}");
                    continue;
                }

                var variantsList = new List<string>();

                foreach (var a in variantsFiles)
                    variantsList.Add(new FileInfo(a).Name);

                foreach (var variant in variantsList)
                {
                    var variantDescription = "";

                    switch (gameTitle)
                    {
                        case "halo3":
                            try
                            {
                                variantDescription = $"{readH3Hexpat($"{absoluteVariantsPath}\\{variant}")}";
                            }
                            catch
                            {
                            }
                            break;
                        case "haloreach":
                            try
                            {
                                variantDescription = $"{readReachHexpat($"{absoluteVariantsPath}\\{variant}")}";
                            }
                            catch
                            {
                            }
                            break;
                        default:
                            continue;

                    }
                    while (true)
                    {
                        if (variantDescription.Length == 0)
                            goto done;

                        if (variantDescription.First() == " ".ToCharArray()[0])
                            variantDescription = variantDescription.Remove(0);

                        else goto done;
                    }
                done:
                    ;

                    var newVariant = new item { gameTitle_____ = $"{gameTitle}" };

                    switch (variantsFolder)
                    {
                        case "map_variants":
                            newVariant.enabled_______ = "1";
                            goto r_map_variants_library;
                        case "map_variants_library":
                        r_map_variants_library:
                            newVariant.map_variant___ = variant;
                            newVariant.map_var_descrp = variantDescription;
                            if (!mapVariantsCollection.ContainsKey(variant))
                                mapVariantsCollection.Add(variant, newVariant);
                            break;
                        case "game_variants":
                            newVariant.enabled_______ = "0";
                            goto r_game_variants_library;
                        case "game_variants_library":
                        r_game_variants_library:
                            newVariant.game_variant__ = variant;
                            newVariant.game_var_descr = variantDescription;
                            if (!gameVariantsCollection.ContainsKey(variant))
                                gameVariantsCollection.Add(variant, newVariant);
                            break;
                        default:
                            throw new Exception();
                    }
                }

                switch (variantsFolder)
                {
                    case "game_variants":
                    case "game_variants_library":
                        fillComboboxWithGameVariantOverrides();
                        break;
                    default:
                        break;
                        // throw new Exception($"Exception: Not handeled: {variantsFolder}");
                }
            }


        }
        private void fillComboboxWithGameVariantOverrides()
        {

            // add the extra variants from the other folder to the combo box
            foreach (var a in gameVariantsCollection)
                gameVariantOverrides_list.Add(a.Key);

            comboBox1_gameVariantOverrides.Items.Clear();
            foreach (var a in gameVariantsCollection)
                comboBox1_gameVariantOverrides.Items.Add($"{a.Value.gameTitle_____} {a.Key}: {a.Value.game_var_descr}");

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
                if (gamevariantOverride.Contains(f.Key))
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
                dataGridView1.Rows[ri].Cells[(int)cell.game_variant_d].Value = newGameVariant.game_var_descr;
            }
        }
        /// <summary>
        /// When a cell is modified, dump all contents to the settings file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var getDataTest0 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.gameTitle_____].Value as string; // getDataTest0
            var getDataTest1 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.map_variant___].Value as string; // getDataTest1
            var getDataTest3 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.game_variant__].Value as string; // getDataTest2
            var getDataTest5 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.enabled_______].Value as string; // getDataTest3
            var getDataTest7 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.comment_______].Value as string; // getDataTest4
            var getDataTest8 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.quality_______].Value as string; // getDataTest5
            var getDataTest9 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.map___________].Value as string; // getDataTest6
            var getDataTestA = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.gameMode______].Value as string; // getDataTest7
            var getDataTest6 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.playerCount___].Value as string; // getDataTest8
            var getDataTest2 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.map_variant_de].Value as string; // getDataTest9
            var getDataTest4 = dataGridView1.Rows[e.RowIndex].Cells[(int)cell.game_variant_d].Value as string; // getDataTestA

            mapVariantsCollection[getDataTest1].gameTitle_____ = getDataTest0;
            mapVariantsCollection[getDataTest1].map_variant___ = getDataTest1;
            mapVariantsCollection[getDataTest1].game_variant__ = getDataTest3;
            mapVariantsCollection[getDataTest1].enabled_______ = getDataTest5;
            mapVariantsCollection[getDataTest1].comment_______ = getDataTest7;
            mapVariantsCollection[getDataTest1].quality_______ = getDataTest8;
            mapVariantsCollection[getDataTest1].map___________ = getDataTest9;
            mapVariantsCollection[getDataTest1].gameMode______ = getDataTestA;
            mapVariantsCollection[getDataTest1].playerCount___ = getDataTest6;
            mapVariantsCollection[getDataTest1].map_var_descrp = getDataTest2;
            mapVariantsCollection[getDataTest1].game_var_descr = getDataTest4;

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

                // don't save descriptions for now, they often contain garbage data;

                string name__________ = "";
                string map_variant___ = "";
                string game_variant__ = "";
                string enabled_______ = "";
                string comment_______ = "";
                string quality_______ = "";
                string map___________ = "";
                string gameMode______ = "";
                string playerCount___ = "";
                // string map_variant_de = "";
                // string game_variant_d = "";

                if (row.Cells[(int)cell.gameTitle_____].Value == null) row.Cells[(int)cell.gameTitle_____].Value = "";
                if (row.Cells[(int)cell.map_variant___].Value == null) row.Cells[(int)cell.map_variant___].Value = "";
                if (row.Cells[(int)cell.game_variant__].Value == null) row.Cells[(int)cell.game_variant__].Value = "";
                if (row.Cells[(int)cell.enabled_______].Value == null) row.Cells[(int)cell.enabled_______].Value = "";
                if (row.Cells[(int)cell.comment_______].Value == null) row.Cells[(int)cell.comment_______].Value = "";
                if (row.Cells[(int)cell.quality_______].Value == null) row.Cells[(int)cell.quality_______].Value = "";
                if (row.Cells[(int)cell.map___________].Value == null) row.Cells[(int)cell.map___________].Value = "";
                if (row.Cells[(int)cell.gameMode______].Value == null) row.Cells[(int)cell.gameMode______].Value = "";
                if (row.Cells[(int)cell.playerCount___].Value == null) row.Cells[(int)cell.playerCount___].Value = "";
                // if (row.Cells[(int)cell.map_variant_de].Value == null) row.Cells[(int)cell.map_variant_de].Value = "";
                // if (row.Cells[(int)cell.game_variant_d].Value == null) row.Cells[(int)cell.game_variant_d].Value = "";


                name__________ = row.Cells[(int)cell.gameTitle_____].Value.ToString();
                map_variant___ = row.Cells[(int)cell.map_variant___].Value.ToString();
                game_variant__ = row.Cells[(int)cell.game_variant__].Value.ToString();
                enabled_______ = row.Cells[(int)cell.enabled_______].Value.ToString();
                comment_______ = row.Cells[(int)cell.comment_______].Value.ToString();
                quality_______ = row.Cells[(int)cell.quality_______].Value.ToString();
                map___________ = row.Cells[(int)cell.map___________].Value.ToString();
                gameMode______ = row.Cells[(int)cell.gameMode______].Value.ToString();
                playerCount___ = row.Cells[(int)cell.playerCount___].Value.ToString();
                // map_variant_de = row.Cells[(int)cell.map_variant_de].Value.ToString();
                // game_variant_d = row.Cells[(int)cell.game_variant_d].Value.ToString();

                // i need to learn a compact way to do this
                if (game_variant__ == "")
                    continue;

                string outputline =
                    $"{name__________};" +
                    $"{map_variant___};" +
                    $"{game_variant__};" +
                    $"{enabled_______};" +
                    $"{comment_______};" +
                    $"{quality_______};" +
                    $"{map___________};" +
                    $"{gameMode______};" +
                    $"{playerCount___};" +
                    "";
                // $"{map_variant_de};" +
                // $"{game_variant_d}";

                output.Add(outputline);
                // $"{description}";
            }

            // WriteCsv(output, logFile);
            clog("TEMPDEBUG");
            clog($"WriteCsv({output.Count}, {logFile})");
            foreach (var a in output)
                clog(a);
            clog("TEMPDEBUG end WriteCsv");

        }
        private void comboBox1_gameVariantOverrides_SelectedValueChanged(object sender, EventArgs e)
        {
            // clog($"comboBox1_gameVariantOverrides_SelectedValueChanged().dumpSettings()");
            ComboBox1_OverrideGameVariant_changed();
            dumpSettings();
        }
        private void button1_Click_moveFiles(object sender, EventArgs e)
        {

            // dirty, just move all files back to library folders
            foreach (var gameTitle in MCCtitles)
            {
                switch (gameTitle)
                {
                    case "halo3":
                    case "haloreach":
                        break;
                    default: // ignore titles other than halo3 and haloreach
                        continue;
                }

                var items11 = Directory.EnumerateFiles($"{rootPath}{gameTitle}\\map_variants");
                var items12 = Directory.EnumerateFiles($"{rootPath}{gameTitle}\\map_variants_library");
                var items21 = Directory.EnumerateFiles($"{rootPath}{gameTitle}\\game_variants");
                var items22 = Directory.EnumerateFiles($"{rootPath}{gameTitle}\\game_variants_library");

                foreach (var item in items11)
                {
                    clog($"TEMPDEBUG: button1_Click_moveFiles.File.Move({items11}\\{item}\", \"{items12}\\{item}\")");
                    try
                    {

                        File.Move($"{items11}\\{item}", $"{items12}\\{item}");

                    }
                    catch (Exception ex)
                    {
                        clog($"Exception: moveStuf: File.Move({items11}\\{item}, {items12}\\{item}) exception: {ex.Message}");

                    }
                }
                foreach (var item in items21)
                {
                    clog($"TEMPDEBUG: button1_Click_moveFiles.File.Move({items21}\\{item}\", \"{items22}\\{item}\")");

                    try
                    {

                        File.Move($"{items21}\\{item}", $"{items22}\\{item}");

                    }
                    catch (Exception ex)
                    {

                        clog($"Exception: moveStuf: File.Move({items21}\\{item}, {items22}\\{item}) exception: {ex.Message}");
                    }
                }

                foreach (var a in mapVariantsCollection)
                {
                    var map_variant___ = a.Value.map_variant___;
                    var game_variant__ = a.Value.game_variant__;

                    var path1 = $"{rootPath}{gameTitle}\\map_variants\\{map_variant___}";
                    var path2 = $"{rootPath}{gameTitle}\\map_variants_library\\{map_variant___}";
                    var path3 = $"{rootPath}{gameTitle}\\game_variants\\{game_variant__}";
                    var path4 = $"{rootPath}{gameTitle}\\game_variants_library\\{game_variant__}";

                    var enabled = a.Value.enabled_______;

                    if (enabled != "1")
                        continue;

                    try
                    {
                        clog($"TEMPDEBUG: button1_Click_moveFiles: File.Move({path2}, {path1})");
                        File.Move(path2, path1);
                    }
                    catch (Exception ex)
                    {
                        clog($"Exception: moveStuf: File.Move({path2}, {path1}) exception: {ex.Message}");
                    }

                    try
                    {
                        clog($"button1_Click_moveFiles: File.Move({path4}, {path3})");
                        File.Move(path4, path3);
                    }
                    catch (Exception ex)
                    {
                        clog($"Exception: moveStuf: File.Move({path4}, {path3}) exceptio: {ex.Message}");
                    }
                }
            }
        }
        public void ApplyColors()
        {
            var BackColor = Color.White;
            var ForeColor = Color.Black;
            this.BackColor = BackColor;
            this.ForeColor = ForeColor;
        }

        public string readH3Hexpat(string filepath)
        {
            var _stream = new BinaryReader2(File.OpenRead(filepath));

            if (filepath.Split("\\").Last() == "INF Hide&Seek.mvar")
                ;

            _stream.BaseStream.Position = (0x0);

            int pos = 0;
            short a;

        start:
            _stream.BaseStream.Position = pos;
            var chunk_type = _stream.ReadChars(4);

            var chunk_type_string = new string(chunk_type);

            var chunk_size = _stream.ReadInt32();
            var major_version = _stream.ReadInt16();
            var minor_version = _stream.ReadInt16();

            switch (chunk_type_string)
            {
                case "_blf":
                    pos = pos + chunk_size;
                    goto start;
                case "chdr":
                    pos = pos + chunk_size;
                    _stream.BaseStream.Position += 0x4 * 3;
                    var array = new List<char>();
                    while (true)
                    {
                        a = _stream.ReadInt16();
                        if (a == 0x0)
                            goto stringdone2;
                        array.Add((char)a);

                    }
                stringdone2:
                    // string str = System.Text.Encoding.Unicode.GetString(array.ToArray());
                    var internalName = new string(array.ToArray());

                    var description = new string(_stream.ReadChars(0x80));

                    return description;
                case "athr":
                    pos = pos + chunk_size;
                    goto start;
                case "mvar":
                    try
                    {
                        _stream.BaseStream.Position += 8;
                        array = new List<char>();
                        while (true)
                        {
                            a = _stream.ReadInt16();
                            if (a == 0x0)
                                goto stringdone;
                            array.Add((char)a);

                        }
                    stringdone:
                        // string str = System.Text.Encoding.Unicode.GetString(array.ToArray());
                        internalName = new string(array.ToArray());

                        description = new string(_stream.ReadChars(0x80));
                        // description = "";

                        return description;
                    }
                    catch
                    { return ""; }

                case "_eof":
                    goto end;
                case "mpvr": // offset is wrong
                    goto end;
                default:
                    goto end;
            }

        end:
            return "";
        }
        public string readReachHexpat(string filepath)
        {
            var _stream = new BinaryReader2(File.OpenRead(filepath));

            if (filepath.Split("\\").Last() == "INF Hide&Seek.mvar")
                ;

            _stream.BaseStream.Position = (0x0);

            int pos = 0;
            short a;

        start:
            _stream.BaseStream.Position = pos;
            var chunk_type = _stream.ReadChars(4);

            var chunk_type_string = new string(chunk_type);

            var chunk_size = _stream.ReadInt32();
            var major_version = _stream.ReadInt16();
            var minor_version = _stream.ReadInt16();

            switch (chunk_type_string)
            {
                case "_blf":
                    pos = pos + chunk_size;
                    goto start;
                case "chdr":
                    pos = pos + chunk_size;
                    // just jump to description
                    _stream.BaseStream.Position += 0x190 - 0x4 * 3;
                    var array = new List<char>();
                    while (true)
                    {
                        a = _stream.ReadInt16();
                        array.Add((char)(a / 0x100));
                        if (a == 0x0)
                            goto stringdone2;

                    }
                stringdone2:
                    // var str = System.Text.Encoding.Unicode.GetString(array.ToArray());
                    var str = new string(array.ToArray());

                    return str;
                case "athr":
                    pos = pos + chunk_size;
                    goto start;
                case "mvar":
                    _stream.BaseStream.Position += 8;
                    array = new List<char>();
                    while (true)
                    {
                        a = _stream.ReadInt16();
                        array.Add((char)(a / 0x100));
                        if (a == 0x0)
                            goto stringdone;

                    }
                stringdone:
                    // str = System.Text.Encoding.Unicode.GetString(array.ToArray());
                    // var description = _stream.ReadChars(0x80).ToString();

                    str = new string(array.ToArray());
                    return str;
                case "_eof":
                    goto end;
                case "mpvr": // offset is wrong
                    goto end;
                default:
                    goto end;
            }

        end:
            return "";
        }

        class BinaryReader2 : BinaryReader
        {
            public BinaryReader2(System.IO.Stream stream) : base(stream) { }

            public override int ReadInt32()
            {
                var data = base.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToInt32(data, 0);
            }

            public Int16 ReadInt16()
            {
                var data = base.ReadBytes(2);
                Array.Reverse(data);
                return BitConverter.ToInt16(data, 0);
            }

            public Int64 ReadInt64()
            {
                var data = base.ReadBytes(8);
                Array.Reverse(data);
                return BitConverter.ToInt64(data, 0);
            }

            public UInt32 ReadUInt32()
            {
                var data = base.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToUInt32(data, 0);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            // clog($"dumpSettings");

            var output = new List<string>();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                var row = dataGridView1.Rows[i];

                if (row.Cells[0].Value == null)
                    continue;

                // don't save descriptions for now, they often contain garbage data;

                string name__________ = "";
                string map_variant___ = "";
                string game_variant__ = "";
                string enabled_______ = "";
                string comment_______ = "";
                string quality_______ = "";
                string map___________ = "";
                string gameMode______ = "";
                string playerCount___ = "";
                // string map_variant_de = "";
                // string game_variant_d = "";

                if (row.Cells[(int)cell.gameTitle_____].Value == null) row.Cells[(int)cell.gameTitle_____].Value = "";
                if (row.Cells[(int)cell.map_variant___].Value == null) row.Cells[(int)cell.map_variant___].Value = "";
                if (row.Cells[(int)cell.game_variant__].Value == null) row.Cells[(int)cell.game_variant__].Value = "";
                if (row.Cells[(int)cell.enabled_______].Value == null) row.Cells[(int)cell.enabled_______].Value = "";
                if (row.Cells[(int)cell.comment_______].Value == null) row.Cells[(int)cell.comment_______].Value = "";
                if (row.Cells[(int)cell.quality_______].Value == null) row.Cells[(int)cell.quality_______].Value = "";
                if (row.Cells[(int)cell.map___________].Value == null) row.Cells[(int)cell.map___________].Value = "";
                if (row.Cells[(int)cell.gameMode______].Value == null) row.Cells[(int)cell.gameMode______].Value = "";
                if (row.Cells[(int)cell.playerCount___].Value == null) row.Cells[(int)cell.playerCount___].Value = "";
                // if (row.Cells[(int)cell.map_variant_de].Value == null) row.Cells[(int)cell.map_variant_de].Value = "";
                // if (row.Cells[(int)cell.game_variant_d].Value == null) row.Cells[(int)cell.game_variant_d].Value = "";

                name__________ = row.Cells[(int)cell.gameTitle_____].Value.ToString();
                map_variant___ = row.Cells[(int)cell.map_variant___].Value.ToString();
                game_variant__ = row.Cells[(int)cell.game_variant__].Value.ToString();
                enabled_______ = row.Cells[(int)cell.enabled_______].Value.ToString();
                comment_______ = row.Cells[(int)cell.comment_______].Value.ToString();
                quality_______ = row.Cells[(int)cell.quality_______].Value.ToString();
                map___________ = row.Cells[(int)cell.map___________].Value.ToString();
                gameMode______ = row.Cells[(int)cell.gameMode______].Value.ToString();
                playerCount___ = row.Cells[(int)cell.playerCount___].Value.ToString();
                // map_variant_de = row.Cells[(int)cell.map_variant_de].Value.ToString();
                // game_variant_d = row.Cells[(int)cell.game_variant_d].Value.ToString();

                // i need to learn a compact way to do this
                if (game_variant__ == "")
                    continue;

                string outputline =
                    $"{name__________};" +
                    $"{map_variant___};" +
                    $"{game_variant__};" +
                    $"{enabled_______};" +
                    $"{comment_______};" +
                    $"{quality_______};" +
                    $"{map___________};" +
                    $"{gameMode______};" +
                    $"{playerCount___};" +
                    "";
                // $"{map_variant_de};" +
                // $"{game_variant_d}";

                output.Add(outputline);
                // $"{description}";
            }

            // WriteCsv(output, logFile);
            clog("TEMPDEBUG");
            clog($"WriteCsv({output.Count}, {logFile})");
            foreach (var a in output)
                clog(a);
            clog("TEMPDEBUG end WriteCsv");
        }
    }
}
