using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PortraitBuilder.Engine;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using PortraitBuilder.Model;
using System.Linq;
using SkiaSharp;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PortraitBuilder.UI
{

    /// <summary>
    /// Controller class
    /// </summary>
    public partial class PortraitBuilderForm : Form
    {

        private static readonly ILogger logger = LoggingHelper.CreateLogger<PortraitBuilderForm>();

        private SKImage previewImage;

        private bool started = false;
        public static Random rand = new Random();

        private Boolean nextToogleIsSelectAll = true;

        private GameLoader loader;

        private User _user;

        /// <summary>
        /// List of all available DLCs and Mods, indexed by their corresponding checkbox
        /// </summary>
        private Dictionary<CheckBox, Content> usableContents = new Dictionary<CheckBox, Content>();

        /// <summary>
        /// The portrait being previewed. 
        /// 
        /// This is the primary Model object, whose state is modified by UI inputs, and used to display the output.
        /// </summary>
        private Character character = new Character();

        /// <summary>
        /// ComboBox for vanilla dna, ordered by their dna index.
        /// </summary>
        private Dictionary<Characteristic, ComboBox> dnaComboBoxes = new Dictionary<Characteristic, ComboBox>();

        /// <summary>
        /// ComboBox for vanilla properties, ordered by their properties index.
        /// </summary>
        private Dictionary<Characteristic, ComboBox> propertiesComboBoxes = new Dictionary<Characteristic, ComboBox>();

        /// <summary>
        /// ComboBox for custom mod properties, ordered by their properties index, and dynamically refreshed per portraitType.
        /// </summary>
        private Dictionary<Characteristic, ComboBox> customPropertiesComboBoxes = new Dictionary<Characteristic, ComboBox>();

        private ToolTip toolTip = new ToolTip();

        public PortraitBuilderForm()
        {
            InitializeComponent();

            foreach (var dna in DefaultCharacteristics.DNA)
            {
                registerDisplayCharacteristic(panelDNA, dna);
            }
            foreach (var property in DefaultCharacteristics.PROPERTIES)
            {
                registerDisplayCharacteristic(panelProperties, property);
            }

            initializeForm();
            load(false);
            started = true;
        }

        private void initializeForm()
        {
            logger.LogInformation("Portrait Builder Version " + Application.ProductVersion);
            // Add the version to title
            this.Text += " " + Application.ProductVersion;

            initializeTooltip();

            _user = new User();
            _user.GameDir = readGameDir();
            _user.ModDir = readModDir(_user.GameDir);
            _user.DlcDir = Path.Combine(Environment.CurrentDirectory, "dlc/");
            logger.LogInformation("Configuration: " + _user);
            logger.LogInformation("----------------------------");

            loader = new GameLoader();
        }

        private void initializeTooltip()
        {
            // Set up the delays for the ToolTip.
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip.ShowAlways = true;

            // Set up the ToolTip text for form controls.
            toolTip.SetToolTip(this.btnToogleAll, "Check or uncheck checkboxes in active tab");
            toolTip.SetToolTip(this.btnReload, "Reload all data from folders");
            toolTip.SetToolTip(this.btnImport, "Import DNA and Properties strings");
            toolTip.SetToolTip(this.btnRandom, "Choose random values for dna/properties that are checked");
            toolTip.SetToolTip(this.btnSave, "Save portrait as a .png image");
            toolTip.SetToolTip(this.btnCopy, "Copy DNA & Properties to use for character history");

            toolTip.SetToolTip(this.cbPortraitTypes, "Select portraitType to render as base");
            toolTip.SetToolTip(this.cbCulturePortraitTypes, "Select portraitType to render for clothing override");
            toolTip.SetToolTip(this.cbRank, "Select rank to use for rendering portrait border");
            toolTip.SetToolTip(this.cbGovernment, "Select government to use for rendering. Theocracy and Merchant Republic use special sprites for headgear and clothing.");
        }

        private void load(bool clean)
        {
            logger.LogInformation("----------------------------");
            logger.LogInformation("(Re-)loading data");

            loader.LoadVanilla(_user.GameDir);
            loadDLCs(_user.GameDir, _user.DlcDir, clean);
            loadMods(_user.ModDir);

            loadPortraitTypes();
            fillDisplayCharacteristicComboBoxes();
            randomizeDisplayCharacteristics(true);

            drawPortrait();
        }

        private void loadMods(string modDir)
        {
            List<Mod> mods = loader.LoadMods(modDir);
            panelMods.Controls.Clear();

            foreach (var mod in mods)
            {
                registerContent(panelMods, mod);
            }
        }

        private void loadDLCs(string gameDir, string dlcDir, bool clean)
        {
            var dlcs = loader.LoadDLCs(gameDir, dlcDir, clean);
            panelDLCs.Controls.Clear();

            foreach (var dlc in dlcs.Where(d => d.HasPortraitData))
            {
                registerContent(panelDLCs, dlc);
            }
        }

        private void registerContent(Control container, Content content)
        {
            CheckBox checkbox = new CheckBox();
            checkbox.Text = content.Name;
            checkbox.AutoEllipsis = true;
            checkbox.Width = 190; // Force overflow
            checkbox.CheckedChanged += this.onCheckContent;
            checkbox.Padding = new Padding(0);
            checkbox.Margin = new Padding(0);

            container.Controls.Add(checkbox);
            usableContents.Add(checkbox, content);

            if (content is Mod)
            {
                if (content.Enabled)
                {
                    toolTip.SetToolTip(checkbox, "Toggle activation and file watching of this mod");
                    content.Watcher = createModFilesWatcher(content);
                }
                else
                {
                    // Note: can't use checkbox.Enabled since it disables tooltips as well !
                    checkbox.ForeColor = Color.Gray; // Read-only appearance
                    checkbox.AutoCheck = false; // Read-only behavior
                    toolTip.SetToolTip(checkbox, content.DisabledReason);
                }
            }
            else
            {
                toolTip.SetToolTip(checkbox, "Toggle activation of this DLC");
            }
        }

        private void registerDisplayCharacteristic(Control container, Characteristic characteristic)
        {
            ComboBox combobox = new ComboBox
            {
                Width = 90,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            combobox.SelectedValueChanged += this.onChangeDisplayCharacteristic;

            Label label = new Label
            {
                Text = $"{characteristic}:",
                Width = 90,
                TextAlign = ContentAlignment.MiddleRight
            };

            CheckBox randomizable = new CheckBox
            {
                Width = 20,
                Padding = new Padding(5, 0, 0, 0)
            };
            randomizable.DataBindings.Add("Checked", characteristic, "Randomizable");
            toolTip.SetToolTip(randomizable, "Use this DisplayCharacteristic when randomizing");

            container.Controls.Add(label);
            container.Controls.Add(combobox);
            container.Controls.Add(randomizable);

            if (characteristic.Type == CharacteristicType.DNA)
            {
                dnaComboBoxes.Add(characteristic, combobox);
            }
            else if (characteristic.Custom)
            {
                customPropertiesComboBoxes.Add(characteristic, combobox);
            }
            else
            {
                propertiesComboBoxes.Add(characteristic, combobox);
            }
        }

        private void unregisterCustomProperties()
        {
            for (int i = 0; i <= customPropertiesComboBoxes.Count * 3 - 1; i++)
            {
                // Remove Label + ComboBox + CheckBox for each custom property
                panelProperties.Controls.RemoveAt(panelProperties.Controls.Count - 1);
            }
            customPropertiesComboBoxes.Clear();
        }

        private string readGameDir()
        {
            Stream stream = new FileStream("gamedir.txt", FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            String gameDir = reader.ReadToEnd();
            logger.LogInformation("Read gamedir: " + gameDir);
            return gameDir;
        }

        /// <summary>
        /// Read userdir.txt in Steam directory for the path to mod dir, or default to pre-defined location
        /// </summary>
        private string readModDir(string gameDir)
        {
            string userDir = getDefaultUserDir();
            string userdirFilePath = Path.Combine(gameDir, "userdir.txt");
            if (File.Exists(userdirFilePath))
            {
                logger.LogInformation("Reading userdir.txt to determine the mod directory.");
                Stream stream = new FileStream(userdirFilePath, FileMode.Open);
                StreamReader reader = new StreamReader(stream, Encoding.Default);
                userDir = reader.ReadLine() + Path.DirectorySeparatorChar;
                logger.LogInformation("Found userdir.txt with path: " + userDir);
            }
            return Path.Combine(userDir, "mod");
        }

        private string getDefaultUserDir()
        {
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine(myDocuments, "Paradox Interactive", "Crusader Kings II");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                // "Environment.SpecialFolder.MyDocuments does not add /Documents/ on Mac"
                // JZ: verify if this is true in corefx
                return Path.Combine(myDocuments, "Documents", "Paradox Interactive", "Crusader Kings II");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Path.Combine(myDocuments, ".paradoxinteractive", "Crusader Kings II");
            else
            {
                logger.LogError($"Unknown operating system, cannot lookup user dir. OS: {RuntimeInformation.OSDescription}");
                return null;
            }
        }

        /// <summary>
        /// Entry point for re-drawing based on updated portrait.
        /// </summary>
        private void drawPortrait()
        {
            pbPortrait.Image?.Dispose();

            try
            {
                var steps = PortraitBuilder.Engine.PortraitBuilder.BuildCharacter(character, loader.ActivePortraitData.Sprites)
                    .Where(step => step != null)
                    .Select(step =>
                    {
                        step.Tile = loader.Cache.Get(step.Def)[step.TileIndex];
                        return step;
                    });

                var rendered = PortraitRenderer.DrawPortrait(steps);
                previewImage = SKImage.FromBitmap(rendered);
            }
            catch (Exception e)
            {
                logger.LogError("Error encountered rendering portrait", e);
                return;
            }

            pbPortrait.Image = Image.FromStream(previewImage.Encode().AsStream(true));

            if (!this.Visible)
            {
                return;
            }

            var fullUrl = GetOnlinePortraitUrl();
            logger.LogInformation("Rendering remote portrait: " + fullUrl);
            lnkLblRemotePortrait.Text = fullUrl;
            pbPortraitOnline.ImageLocation = fullUrl;
        }

        private string GetOnlinePortraitUrl()
        {
            var (@base, clothing) = GetSelectedGraphicalCultures();
            string host;
#if DEBUG
            host = "http://localhost:7071";
#else
            host = "https://portraits.ga";
#endif
            var baseUrl = $"{host}/portrait?dna={character.DNA}&properties={character.Properties}&base={@base}";
            var clothingUrl = !string.IsNullOrEmpty(clothing) ? $"&clothing={clothing}" : string.Empty;
            var govtUrl = $"&government={character.Government}";
            var titleUrl = character.Rank != TitleRank.None ? $"&titlerank={character.Rank}" : string.Empty;
            return baseUrl + clothingUrl + govtUrl + titleUrl;
        }

        private void OnRemotePortraitLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!(sender is LinkLabel lnkLbl))
                return;

            string url;
            if (e.Link.LinkData != null)
                url = e.Link.LinkData.ToString();
            else
                url = lnkLbl.Text.Substring(e.Link.Start, e.Link.Length);

            Debug.Assert(url.Contains("://"));

            var si = new ProcessStartInfo(url) { UseShellExecute = true };
            Process.Start(si);
            lnkLbl.LinkVisited = true;
        }

        private string getDisplayCharacteristicsString(Dictionary<Characteristic, ComboBox> characteristics)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ComboBox cb in characteristics.Values)
            {
                char letter = '0';
                if (cb != null)
                {
                    letter = GetLetter(cb);
                }
                sb.Append(letter);
            }
            return sb.ToString();
        }

        // Needs to be called each time portrait object is modified
        private void outputDNA()
        {
            logger.LogDebug(" --Outputting DNA and Property strings.");
            StringBuilder dnaPropOutput = new StringBuilder();

            dnaPropOutput.Append("  dna=\"");
            dnaPropOutput.Append(character.DNA);
            dnaPropOutput.AppendLine("\"");

            dnaPropOutput.Append("  properties=\"");
            dnaPropOutput.Append(character.Properties);
            dnaPropOutput.AppendLine("\"");
            tbDNA.Text = dnaPropOutput.ToString();
        }

        private char GetLetter(ComboBox cb) => Character.GetLetter(cb.SelectedIndex);

        /// <summary>
        /// Some very specific characristics are not randomized: scars, red dots, boils, prisoner, blinded.
        /// </summary>
        /// <param name="doRank"></param>
        private void randomizeDisplayCharacteristics(bool doRank)
        {
            logger.LogDebug("Randomizing UI");
            if (doRank)
            {
                randomizeComboBox(cbGovernment);
                randomizeComboBox(cbRank);
            }

            randomizeDisplayCharacteristics(dnaComboBoxes);
            randomizeDisplayCharacteristics(propertiesComboBoxes);
            randomizeDisplayCharacteristics(customPropertiesComboBoxes);

            updatePortrait(getDisplayCharacteristicsString(dnaComboBoxes), getDisplayCharacteristicsString(propertiesComboBoxes), getDisplayCharacteristicsString(customPropertiesComboBoxes));
        }

        private void randomizeDisplayCharacteristics(Dictionary<Characteristic, ComboBox> cbs)
        {
            foreach (var pair in cbs.Where(kvp => kvp.Key.Randomizable))
            {
                randomizeComboBox(pair.Value);
            }
        }

        private void randomizeComboBox(ComboBox cb)
        {
            if (cb.Items.Count > 0)
            {
                cb.SelectedIndex = rand.Next(cb.Items.Count);
            }
        }

        private void resetComboBox(ComboBox cb)
        {
            if (cb.Items.Count > 0)
            {
                cb.SelectedIndex = 0;
            }
        }

        private void fillComboBox(ComboBox cb, int count)
        {
            for (int i = 0; i < count; i++)
                cb.Items.Add(i);
        }

        private void fillComboBox(ComboBox cb, Characteristic characteristic)
        {
            cb.Items.Clear();
            PortraitType portraitType = character.PortraitType;
            if (portraitType == null)
                return;

            int frameCount = loader.ActivePortraitData.GetFrameCount(portraitType, characteristic);
            if (frameCount > 0)
            {
                logger.LogDebug("Item count for {0} {1} : {2}", portraitType, characteristic, frameCount);
                cb.Enabled = true;
                fillComboBox(cb, frameCount);
                if (frameCount == 1)
                {
                    cb.Enabled = false; // Disable if only 1 frame, as there's no selection to do, for instance head (p2)
                }
            }
            else
            {
                logger.LogWarning("Could not find frame count for {0} and {1}, disabling dropdown.", portraitType, characteristic);
                cb.Enabled = false;
            }
        }

        private (string @base, string clothing) GetSelectedGraphicalCultures()
            => (cbPortraitTypes.SelectedItem?.ToString(), cbCulturePortraitTypes.SelectedItem?.ToString());

        private PortraitType getSelectedPortraitType()
        {
            PortraitType selectedPortraitType = null;
            object selectedItem = cbPortraitTypes.SelectedItem;
            object selectedItem2 = cbCulturePortraitTypes.SelectedItem;
            if (selectedItem != null)
            {
                if (selectedItem2 != null && !selectedItem2.ToString().Equals(""))
                {
                    return loader.GetPortraitType("PORTRAIT_" + selectedItem.ToString(), "PORTRAIT_" + selectedItem2.ToString());
                }
                else
                {
                    return loader.GetPortraitType("PORTRAIT_" + selectedItem.ToString());
                }
            }
            return selectedPortraitType;
        }

        private void fillDisplayCharacteristicComboBoxes()
        {
            fillDisplayCharacteristicComboBoxes(dnaComboBoxes);
            fillDisplayCharacteristicComboBoxes(propertiesComboBoxes);
            fillDisplayCharacteristicComboBoxes(customPropertiesComboBoxes);
        }

        private void fillDisplayCharacteristicComboBoxes(Dictionary<Characteristic, ComboBox> cbs)
        {
            foreach (var pair in cbs.Where(kvp => kvp.Value != null))
            {
                fillComboBox(pair.Value, pair.Key);
            }
        }

        private void loadPortraitTypes()
        {
            object previouslySelectedBasePortrait = null;
            object previouslySelectedOverridePortrait = null;

            if (cbPortraitTypes.SelectedItem != null)
            {
                previouslySelectedBasePortrait = cbPortraitTypes.Items[cbPortraitTypes.SelectedIndex];
            }
            if (cbCulturePortraitTypes.SelectedItem != null)
            {
                previouslySelectedOverridePortrait = cbCulturePortraitTypes.Items[cbCulturePortraitTypes.SelectedIndex];
            }
            cbPortraitTypes.Items.Clear();
            cbCulturePortraitTypes.Items.Clear();

            loader.LoadPortraits();

            if (loader.ActivePortraitData.PortraitTypes.Count == 0)
            {
                logger.LogCritical("No portrait types found.");
                return;
            }

            cbCulturePortraitTypes.Items.Add(""); // Empty = no override
            foreach (KeyValuePair<string, PortraitType> pair in loader.ActivePortraitData.PortraitTypes)
            {
                PortraitType portraitType = pair.Value;
                String portraitName = portraitType.Name.Replace("PORTRAIT_", "");
                if (portraitType.IsBasePortraitType())
                {
                    cbPortraitTypes.Items.Add(portraitName);
                }
                cbCulturePortraitTypes.Items.Add(portraitName);
            }

            if (previouslySelectedBasePortrait != null)
            {
                cbPortraitTypes.SelectedIndex = cbPortraitTypes.Items.IndexOf(previouslySelectedBasePortrait);
            }
            if (previouslySelectedOverridePortrait != null)
            {
                cbCulturePortraitTypes.SelectedIndex = cbCulturePortraitTypes.Items.IndexOf(previouslySelectedOverridePortrait);
            }
            if (cbPortraitTypes.SelectedIndex == -1)
            {
                cbPortraitTypes.SelectedIndex = 0;
            }
            if (cbCulturePortraitTypes.SelectedIndex == -1)
            {
                cbCulturePortraitTypes.SelectedIndex = 0;
            }
            character.PortraitType = getSelectedPortraitType();
        }

        private void updateActiveAdditionalContent()
        {
            List<Content> activeContent = new List<Content>();
            activeContent.AddRange(getSelectedContent(panelDLCs));
            activeContent.AddRange(getSelectedContent(panelMods));
            loader.UpdateActiveAdditionalContent(activeContent);
        }

        private List<Content> getSelectedContent(Panel panel)
        {
            List<Content> selectedContent = new List<Content>();
            foreach (Control control in panel.Controls)
            {
                CheckBox checkbox = (CheckBox)control;
                if (checkbox.Checked)
                {
                    selectedContent.Add(usableContents[checkbox]);
                }
            }
            return selectedContent;
        }

        private void updateSelectedDisplayCharacteristicValues(Character character)
        {
            foreach (var pair in dnaComboBoxes.Where(kvp => kvp.Value != null))
            {
                pair.Value.SelectedIndex = Character.GetIndex(character.DNA[pair.Key.Index], pair.Value.Items.Count);
            }

            foreach (var pair in propertiesComboBoxes.Where(kvp => kvp.Value != null))
            {
                pair.Value.SelectedIndex = Character.GetIndex(character.Properties[pair.Key.Index], pair.Value.Items.Count);
            }
        }

        private void updatePortrait(string dna, string properties, string customProperties)
        {
            character.Import(dna, properties + customProperties);
            outputDNA();
        }

        private FileSystemWatcher createModFilesWatcher(Content content)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Path = content.AbsolutePath;
            watcher.IncludeSubdirectories = true;

            // Do the filtering in event handlers, as watchers do not support Filter such as "*.gfx|*.txt|*.dds"
            watcher.Filter = "*.*";

            watcher.NotifyFilter = NotifyFilters.LastWrite;

            // Have the callbacks execute in Main thread
            watcher.SynchronizingObject = this;
            watcher.Changed += new FileSystemEventHandler(onContentFileChanged);
            watcher.Created += new FileSystemEventHandler(onContentFileChanged);
            watcher.Deleted += new FileSystemEventHandler(onContentFileChanged);
            watcher.Renamed += new RenamedEventHandler(onContentFileRenamed);

            watcher.Error += new ErrorEventHandler(onWatcherError);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private Content getAssociatedContent(FileSystemWatcher watcher)
        {
            Content content = null;

            foreach (KeyValuePair<CheckBox, Content> pair in usableContents)
            {
                if (pair.Value.Watcher == watcher)
                {
                    content = pair.Value;
                    break;
                }
            }
            return content;
        }

        private void updateSelectedContent(List<CheckBox> cbs)
        {
            started = false;
            updateActiveAdditionalContent();
            loadPortraitTypes();
            refreshCustomDisplayCharacteristics();

            fillDisplayCharacteristicComboBoxes();
            // TODO No refresh of DNA/Properties needed (if ComboBox has less options ?)
            started = true;

            foreach (CheckBox cb in cbs)
            {
                Mod content = usableContents[cb] as Mod;
                if (content != null)
                {
                    content.Watcher.EnableRaisingEvents = cb.Checked;
                }
            }

            drawPortrait();
        }

        private void refreshCustomDisplayCharacteristics()
        {
            unregisterCustomProperties();
            foreach (var dc in getSelectedPortraitType().CustomCharacteristics)
            {
                if (!customPropertiesComboBoxes.ContainsKey(dc))
                {
                    registerDisplayCharacteristic(panelProperties, dc);
                }
            }
        }

        ///////////////////
        // Event handlers
        ///////////////////

        private void onChangeDisplayCharacteristic(object sender, EventArgs e)
        {
            if (started)
            {
                // Assumption: customPropertiesComboBoxes are contiguous !
                updatePortrait(getDisplayCharacteristicsString(dnaComboBoxes), getDisplayCharacteristicsString(propertiesComboBoxes), getDisplayCharacteristicsString(customPropertiesComboBoxes));
                drawPortrait();
            }
        }

        private void onChangeRank(object sender, EventArgs e)
        {
            character.Rank = (TitleRank)cbRank.SelectedIndex;
            drawPortrait();
        }

        private void onChangeGovernment(object sender, EventArgs e)
        {
            character.Government = (GovernmentType)cbGovernment.SelectedIndex;
            drawPortrait();
        }

        private void onClickCopy(object sender, EventArgs e)
        {
            Clipboard.SetText(tbDNA.Text);
        }

        private void onClickSave(object sender, EventArgs e)
        {
            string file = Snippets.SaveFileDialog("Save Image", "PNG|*.png", null);
            if (string.IsNullOrEmpty(file)) return;

            using (var fs = File.Create(file))
            {
                previewImage.Encode().SaveTo(fs);
            }
        }

        private void onClickImport(object sender, EventArgs e)
        {
            ImportDialog dialog = new ImportDialog();

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                started = false;

                updatePortrait(dialog.character.DNA, dialog.character.Properties, "");

                // Reflect on dropdown
                updateSelectedDisplayCharacteristicValues(character);

                started = true;

                drawPortrait();
            }
        }

        private void onClickRandomize(object sender, EventArgs e)
        {
            started = false;
            randomizeDisplayCharacteristics(false);
            started = true;

            drawPortrait();
        }

        /// <summary>
        /// Called each time a content CheckBox is ticked/unticked
        /// </summary>
        private void onCheckContent(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            List<CheckBox> cbs = new List<CheckBox>();
            cbs.Add(cb);
            updateSelectedContent(cbs);
        }

        private void onChangePortraitType(object sender, EventArgs e)
        {
            if (!started)
                return;

            started = false;

            PortraitType selectedPortraitType = getSelectedPortraitType();
            character.PortraitType = selectedPortraitType;

            refreshCustomDisplayCharacteristics();

            fillDisplayCharacteristicComboBoxes();
            updateSelectedDisplayCharacteristicValues(character);

            started = true;

            drawPortrait();
        }

        private void onClickReload(object sender, EventArgs e)
        {
            usableContents.Clear();
            load(true);
        }

        private void onClickToogleAll(object sender, EventArgs e)
        {
            TabPage tabPage = tabContent.SelectedTab;
            List<CheckBox> cbs = new List<CheckBox>();
            foreach (CheckBox checkbox in tabPage.Controls[0].Controls)
            {
                // Remove handler so it doesn't trigger
                checkbox.CheckedChanged -= onCheckContent;
                checkbox.Checked = nextToogleIsSelectAll;
                checkbox.CheckedChanged += onCheckContent;
                cbs.Add(checkbox);
            }
            nextToogleIsSelectAll = !nextToogleIsSelectAll;
            updateSelectedContent(cbs);
        }

        private void onContentFileChanged(object sender, FileSystemEventArgs e)
        {
            FileSystemWatcher watcher = sender as FileSystemWatcher;
            onContentChange(watcher, e.FullPath);
        }

        private void onContentFileRenamed(object sender, RenamedEventArgs e)
        {
            FileSystemWatcher watcher = sender as FileSystemWatcher;
            onContentChange(watcher, e.OldFullPath);
        }

        private void onContentChange(FileSystemWatcher watcher, string path)
        {
            // Workaround same change firing multiple events
            watcher.EnableRaisingEvents = false;

            Content content = getAssociatedContent(watcher);

            if (content != null)
            {
                logger.LogInformation(string.Format("Content change for {0} in content {1}", path, content));
                loader.RefreshContent(content);
                loadPortraitTypes();
                fillDisplayCharacteristicComboBoxes();
                drawPortrait();
            }
            else
            {
                logger.LogError(string.Format("No content matched for watcher on file {0}", path));
            }

            watcher.EnableRaisingEvents = true;
        }

        private void onWatcherError(object sender, ErrorEventArgs e)
        {
            logger.LogError("FileSystemWatcher unable to continue", e.GetException());
        }
    }
}
