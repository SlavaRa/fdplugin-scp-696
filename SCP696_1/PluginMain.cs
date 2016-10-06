using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using System.Windows.Forms;

namespace SCP696_1
{
    public class PluginMain : IPlugin, IDisposable
	{
        #region Required Properties

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public int Api => 1;

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public string Name => nameof(SCP696_1);

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public string Guid => "bcb06fe2-90ed-4876-9052-68f299c6b6b1";

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public string Author => "SlavaRa";

        /// <summary>
        /// Description of the plugin
        /// </summary>
        public string Description => $"{Name} plugin";

        /// <summary>
        /// Web address for help
        /// </summary>
        public string Help => "";

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public object Settings { get; private set; }

        #endregion

        string settingFilename;
        Timer timer;
        readonly HashSet<Form> formIsProcessed = new HashSet<Form>();

        #region Required Methods

        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
		{
            InitBasics();
            LoadSettings();
            AddEventHandlers();
        }
		
		/// <summary>
		/// Disposes the plugin
		/// </summary>
		public void Dispose()
	    {
            timer?.Dispose();
	        SaveSettings();
	    }

	    /// <summary>
		/// Handles the incoming events
		/// </summary>
		public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
	    {
	        if (e.Type != EventType.UIStarted || timer != null) return;
	        timer = new Timer {Interval = PluginBase.MainForm.Settings.DisplayDelay};
	        timer.Tick += (o, args) =>
	        {
	            var forms = Application.OpenForms;
	            for (var i = 0; i < forms.Count; i++)
	            {
	                var form = forms[i];
                    if (!formIsProcessed.Contains(form)) Add(form);
                }
	        };
            timer.Start();
	    }

	    #endregion
        
        #region Custom Methods
       
        /// <summary>
        /// Initializes important variables
        /// </summary>
        void InitBasics()
        {
            var path = Path.Combine(PathHelper.DataDir, Name);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            settingFilename = Path.Combine(path, $"{nameof(Settings)}.fdb");
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        void LoadSettings()
        {
            Settings = new Settings();
            if (!File.Exists(settingFilename)) SaveSettings();
            else Settings = (Settings) ObjectSerializer.Deserialize(settingFilename, Settings);
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary>
        void AddEventHandlers() => EventManager.AddEventHandler(this, EventType.UIStarted);

	    /// <summary>
        /// Saves the plugin settings
        /// </summary>
        void SaveSettings() => ObjectSerializer.Serialize(settingFilename, Settings);

        #endregion

        void Add(Form form)
        {
            form.Closed += (sender, args) => formIsProcessed.Remove((Form)sender);
            formIsProcessed.Add(form);
            Process(form);
        }

        static void Process(Control control)
        {
            var controls = control.Controls;
            for (var i = 0; i < controls.Count; i++)
            {
                control = controls[i];
                if (control is TextBoxBase) control.KeyDown += OnKeyDown;
                else if (control is ComboBox) control.KeyDown += OnKeyDown;
                else Process(control);
            }
        }

        static void OnKeyDown(object sender, KeyEventArgs e)
        {
            var shortcutId = PluginBase.MainForm.GetShortcutItemId(e.KeyData);
            if (string.IsNullOrEmpty(shortcutId)) return;
            if (sender is TextBoxBase) OnTextBoxKeyDown((TextBoxBase)sender, shortcutId);
            else if (sender is ComboBox) OnComboBoxKeyDown((ComboBox)sender, shortcutId);
        }

        static void OnTextBoxKeyDown(TextBoxBase sender, string shortcutId)
        {
            switch (shortcutId)
            {
                case "EditMenu.ToLowercase":
                case "EditMenu.ToUppercase":
                    var selectedText = sender.SelectedText;
                    if (string.IsNullOrEmpty(selectedText)) break;
                    var selectionStart = sender.SelectionStart;
                    var selectionLength = sender.SelectionLength;
                    sender.SelectedText = shortcutId == "EditMenu.ToLowercase" ? selectedText.ToLower() : selectedText.ToUpper();
                    sender.Select(selectionStart, selectionLength);
                    break;
            }
        }

        static void OnComboBoxKeyDown(ComboBox sender, string shortcutId)
        {
            switch (shortcutId)
            {
                case "EditMenu.ToLowercase":
                case "EditMenu.ToUppercase":
                    var selectedText = sender.SelectedText;
                    if (string.IsNullOrEmpty(selectedText)) break;
                    var selectionStart = sender.SelectionStart;
                    var selectionLength = sender.SelectionLength;
                    sender.SelectedText = shortcutId == "EditMenu.ToLowercase" ? selectedText.ToLower() : selectedText.ToUpper();
                    sender.Select(selectionStart, selectionLength);
                    break;
            }
        }
    }
}