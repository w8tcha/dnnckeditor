/*
 * CKEditor Html Editor Provider for DNN
 * ========
 * https://github.com/w8tcha/dnnckeditor
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor.Web
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using WatchersNET.CKEditor.Constants;
    using WatchersNET.CKEditor.Extensions;
    using WatchersNET.CKEditor.Objects;
    using WatchersNET.CKEditor.Utilities;

    #endregion

    /// <summary>
    /// The CKEditor control.
    /// </summary>
    public class CKEditorControl : WebControl, IPostBackDataHandler
    {
        #region Constants and Fields

        /// <summary>
        /// The provider type.
        /// </summary>
        private const string ProviderType = "htmlEditor";

        /// <summary>
        /// Has MS Ajax Installed?
        /// </summary>
        private static bool? hasMsAjax;

        /// <summary>
        /// The portal settings.
        /// </summary>
        private readonly PortalSettings _portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];

        /// <summary>
        /// Check if the Settings Collection
        /// is Merged with all Settings
        /// </summary>
        private bool isMerged;

        /// <summary>
        /// The settings collection.
        /// </summary>
        private NameValueCollection settings;

        /// <summary>
        /// Current Settings Base
        /// </summary>
        private EditorProviderSettings currentSettings = new EditorProviderSettings();

        /// <summary>
        /// The tool bar name override
        /// </summary>
        private string toolBarNameOverride; // EL 20101006

        /// <summary>
        /// The parent module that contains the editor.
        /// </summary>
        private PortalModuleBase myParModule;

        /// <summary>
        /// The Parent Module ID
        /// </summary>
        private int parentModulId;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CKEditorControl"/> class.
        /// </summary>
        public CKEditorControl()
        {
            this.LoadConfigSettings();

            this.Init += this.CKEditorInit;
        }

        #endregion
        #region Properties

        /// <summary>
        /// Gets a value indicating whether IsRendered.
        /// </summary>
        public bool IsRendered { get; private set; }

        /// <summary>
        /// Gets Settings.
        /// </summary>
        public NameValueCollection Settings
        {
            get
            {
                if (this.isMerged)
                {
                    return this.settings;
                }

                // Override local settings with attributes
                foreach (string key in this.Attributes.Keys)
                {
                    this.settings[key] = this.Attributes[key];
                }

                // Inject all Editor Config
                foreach (
                    var info in
                        SettingsUtil.GetEditorConfigProperties())
                {
                    XmlAttributeAttribute xmlAttributeAttribute = null;
                    var settingValue = string.Empty;

                    if (!info.Name.Equals("CodeMirror") && !info.Name.Equals("WordCount"))
                    {
                        if (info.GetValue(this.currentSettings.Config, null) == null)
                        {
                            continue;
                        }

                        var rawValue = info.GetValue(this.currentSettings.Config, null);

                        settingValue = info.PropertyType.Name.Equals("Double")
                                           ? Convert.ToDouble(rawValue)
                                                 .ToString(CultureInfo.InvariantCulture)
                                           : rawValue.ToString();

                        if (string.IsNullOrEmpty(settingValue))
                        {
                            continue;
                        }

                        xmlAttributeAttribute = info.GetCustomAttribute<XmlAttributeAttribute>(true);
                    }

                    if (info.PropertyType.Name == "Boolean")
                    {
                        this.settings[xmlAttributeAttribute.AttributeName] = settingValue.ToLower();
                    }
                    else
                    {
                        switch (info.Name)
                        {
                            case "ToolbarLocation":
                                this.settings[xmlAttributeAttribute.AttributeName] = settingValue.ToLower();
                                break;
                            case "EnterMode":
                            case "ShiftEnterMode":
                                switch (settingValue)
                                {
                                    case "P":
                                        this.settings[xmlAttributeAttribute.AttributeName] = "1";
                                        break;
                                    case "BR":
                                        this.settings[xmlAttributeAttribute.AttributeName] = "2";
                                        break;
                                    case "DIV":
                                        this.settings[xmlAttributeAttribute.AttributeName] = "3";
                                        break;
                                }

                                break;
                            case "ContentsLangDirection":
                                {
                                    switch (settingValue)
                                    {
                                        case "LeftToRight":
                                            this.settings[xmlAttributeAttribute.AttributeName] = "ltr";
                                            break;
                                        case "RightToLeft":
                                            this.settings[xmlAttributeAttribute.AttributeName] = "rtl";
                                            break;
                                        default:
                                            this.settings[xmlAttributeAttribute.AttributeName] = string.Empty;
                                            break;
                                    }
                                }

                                break;
                            case "CodeMirror":
                                {
                                    var codeMirrorArray = new StringBuilder();

                                    foreach (var codeMirrorInfo in typeof(CodeMirror).GetProperties())
                                    {
                                        var xmlAttribute =
                                            codeMirrorInfo.GetCustomAttribute<XmlAttributeAttribute>(true);
                                        var rawSettingValue = codeMirrorInfo.GetValue(
                                            this.currentSettings.Config.CodeMirror,
                                            null);

                                        var codeMirrorSettingValue = rawSettingValue.ToString();

                                        if (string.IsNullOrEmpty(codeMirrorSettingValue))
                                        {
                                            continue;
                                        }

                                        switch (codeMirrorInfo.PropertyType.Name)
                                        {
                                            case "String":
                                                codeMirrorArray.AppendFormat(
                                                    "{0}: '{1}',",
                                                    xmlAttribute.AttributeName,
                                                    codeMirrorSettingValue);
                                                break;
                                            case "Boolean":
                                                codeMirrorArray.AppendFormat(
                                                    "{0}: {1},",
                                                    xmlAttribute.AttributeName,
                                                    codeMirrorSettingValue.ToLower());
                                                break;
                                        }
                                    }

                                    var codemirrorSettings = codeMirrorArray.ToString();

                                    this.settings["codemirror"] =
                                        $"{{ {codemirrorSettings.Remove(codemirrorSettings.Length - 1, 1)} }}";
                                }

                                break;
                            case "WordCount":
                                {
                                    var wordcountArray = new StringBuilder();

                                    foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                                    {
                                        var xmlAttribute =
                                            wordCountInfo.GetCustomAttribute<XmlAttributeAttribute>(true);

                                        var rawSettingValue = wordCountInfo.GetValue(
                                            this.currentSettings.Config.WordCount,
                                            null);

                                        var wordCountSettingValue = rawSettingValue.ToString();

                                        if (string.IsNullOrEmpty(wordCountSettingValue))
                                        {
                                            continue;
                                        }

                                        switch (wordCountInfo.PropertyType.Name)
                                        {
                                            case "String":
                                                wordcountArray.AppendFormat(
                                                    "{0}: '{1}',",
                                                    xmlAttribute.AttributeName,
                                                    wordCountSettingValue);
                                                break;
                                            case "Boolean":
                                                wordcountArray.AppendFormat(
                                                    "{0}: {1},",
                                                    xmlAttribute.AttributeName,
                                                    wordCountSettingValue.ToLower());
                                                break;
                                        }
                                    }

                                    var wordcountSettings = wordcountArray.ToString();

                                    this.settings["wordcount"] =
                                        $"{{ {wordcountSettings.Remove(wordcountSettings.Length - 1, 1)} }}";
                                }

                                break;

                            case "AutoSave":
                                {
                                    var wordcountArray = new StringBuilder();

                                    foreach (var wordCountInfo in typeof(AutoSave).GetProperties())
                                    {
                                        var xmlAttribute =
                                            wordCountInfo.GetCustomAttribute<XmlAttributeAttribute>(true);

                                        var rawSettingValue = wordCountInfo.GetValue(
                                            this.currentSettings.Config.AutoSave,
                                            null);

                                        var wordCountSettingValue = rawSettingValue.ToString();

                                        if (string.IsNullOrEmpty(wordCountSettingValue))
                                        {
                                            continue;
                                        }

                                        switch (wordCountInfo.PropertyType.Name)
                                        {
                                            case "String":
                                                wordcountArray.AppendFormat(
                                                    "{0}: \"{1}\",",
                                                    xmlAttribute.AttributeName,
                                                    wordCountSettingValue);
                                                break;
                                            case "Int32":
                                                wordcountArray.AppendFormat(
                                                    "{0}: {1},",
                                                    xmlAttribute.AttributeName,
                                                    wordCountSettingValue);
                                                break;
                                            case "Boolean":
                                                wordcountArray.AppendFormat(
                                                    "{0}: {1},",
                                                    xmlAttribute.AttributeName,
                                                    wordCountSettingValue.ToLower());
                                                break;
                                        }
                                    }

                                    var wordcountSettings = wordcountArray.ToString();

                                    this.settings["autosave"] =
                                        $"{{ {wordcountSettings.Remove(wordcountSettings.Length - 1, 1)} }}";
                                }

                                break;
                            default:
                                this.settings[xmlAttributeAttribute.AttributeName] = settingValue;
                                break;
                        }
                    }
                }

                try
                {
                    var currentCulture = Thread.CurrentThread.CurrentUICulture;

                    this.settings["language"] = currentCulture.Name.ToLowerInvariant();

                    if (string.IsNullOrEmpty(this.currentSettings.Config.Scayt_sLang))
                    {
                        this.settings["scayt_sLang"] = currentCulture.Name.ToLowerInvariant();
                    }
                }
                catch (Exception)
                {
                    this.settings["language"] = "en";
                }

                this.settings["customConfig"] = string.Empty;

                if (!string.IsNullOrEmpty(this.currentSettings.Config.Skin))
                {
                    if (this.currentSettings.Config.Skin.Equals("office2003")
                        || this.currentSettings.Config.Skin.Equals("BootstrapCK-Skin")
                        || this.currentSettings.Config.Skin.Equals("chris")
                        || this.currentSettings.Config.Skin.Equals("v2"))
                    {
                        this.settings["skin"] = "moono-lisa";
                    }
                    else
                    {
                        this.settings["skin"] = this.currentSettings.Config.Skin;
                    }
                }

                if (!string.IsNullOrEmpty(this.currentSettings.Config.ContentsCss))
                {
                    this.settings["contentsCss"] =
                        $"['{Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/contents.css")}', '{this.FormatUrl(this.currentSettings.Config.ContentsCss)}']";
                }
                else
                {
                    this.settings["contentsCss"] = Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/contents.css");
                }

                if (!string.IsNullOrEmpty(this.currentSettings.Config.Templates_Files))
                {
                    var templateUrl = this.FormatUrl(this.currentSettings.Config.Templates_Files);

                    this.settings["templates_files"] =
                        $"[ '{(templateUrl.EndsWith(".xml") ? $"xml:{templateUrl}" : templateUrl)}' ]";
                }

                if (!string.IsNullOrEmpty(this.toolBarNameOverride))
                {
                    this.settings["toolbar"] = this.toolBarNameOverride;
                }
                else
                {
                    var toolbarName = this.SetUserToolbar(this.settings["configFolder"]);

                    var listToolbarSets = ToolbarUtil.GetToolbars(this._portalSettings.HomeDirectoryMapPath, this.settings["configFolder"]);

                    var toolbarSet = listToolbarSets.FirstOrDefault(toolbar => toolbar.Name.Equals(toolbarName));

                    var toolbarSetString = ToolbarUtil.ConvertToolbarSetToString(toolbarSet, true);

                    this.settings["toolbar"] = $"[{toolbarSetString}]";
                }

                // Editor Width
                if (!string.IsNullOrEmpty(this.currentSettings.Config.Width))
                {
                    this.settings["width"] = this.currentSettings.Config.Width;
                }
                else
                {
                    if (this.Width.Value > 0)
                    {
                        this.settings["width"] = this.Width.ToString();
                    }
                }

                // Editor Height
                if (!string.IsNullOrEmpty(this.currentSettings.Config.Height))
                {
                    this.settings["height"] = this.currentSettings.Config.Height;
                }
                else
                {
                    if (this.Height.Value > 0)
                    {
                        this.settings["height"] = this.Height.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(this.settings["extraPlugins"])
                    && this.settings["extraPlugins"].Contains("syntaxhighlight"))
                {
                    this.settings["extraPlugins"] = this.settings["extraPlugins"].Replace("syntaxhighlight", "codesnippet");
                }

                if (!string.IsNullOrEmpty(this.settings["extraPlugins"])
                    && this.settings["extraPlugins"].Contains("xmlstyles"))
                {
                    this.settings["extraPlugins"] = this.settings["extraPlugins"].Replace(",xmlstyles", string.Empty);
                }

                // fix oembed/embed issue and other bad settings
                if (!string.IsNullOrEmpty(this.settings["extraPlugins"])
                    && this.settings["extraPlugins"].Contains("oembed"))
                {
                    this.settings["extraPlugins"] = this.settings["extraPlugins"].Replace("oembed", "embed");
                }

                if (this.settings["PasteFromWordCleanupFile"] != null
                    && this.settings["PasteFromWordCleanupFile"].Equals("default"))
                {
                    this.settings["PasteFromWordCleanupFile"] = string.Empty;
                }

                if (this.settings["menu_groups"] != null
                    && this.settings["menu_groups"].Equals("clipboard,table,anchor,link,image"))
                {
                    this.settings["menu_groups"] =
                        "clipboard,tablecell,tablecellproperties,tablerow,tablecolumn,table,anchor,link,image,flash,checkbox,radio,textfield,hiddenfield,imagebutton,button,select,textarea,div";
                }

                // Inject maxFileSize
                this.settings["maxFileSize"] = Utility.GetMaxUploadSize().ToString();

                // Inject Tabs for the dnnpages plugin
                this.settings["dnnPagesArray"] = this.GetTabsArray();

                HttpContext.Current.Session["CKDNNtabid"] = this._portalSettings.ActiveTab.TabID;
                HttpContext.Current.Session["CKDNNporid"] = this._portalSettings.PortalId;

                // Add FileBrowser
                switch (this.currentSettings.BrowserMode)
                {
                    case Browser.StandardBrowser:
                        {
                            this.settings["filebrowserBrowseUrl"] =
                                Globals.ResolveUrl(
                                    $"~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Type=Link&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}&mid={this.parentModulId}&ckid={this.ID}&mode={this.currentSettings.SettingMode}&lang={CultureInfo.CurrentCulture.Name}");
                            this.settings["filebrowserImageBrowseUrl"] =
                                Globals.ResolveUrl(
                                    $"~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Type=Image&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}&mid={this.parentModulId}&ckid={this.ID}&mode={this.currentSettings.SettingMode}&lang={CultureInfo.CurrentCulture.Name}");
                            this.settings["filebrowserFlashBrowseUrl"] =
                                Globals.ResolveUrl(
                                    $"~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Type=Flash&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}&mid={this.parentModulId}&ckid={this.ID}&mode={this.currentSettings.SettingMode}&lang={CultureInfo.CurrentCulture.Name}");

                            if (Utility.CheckIfUserHasFolderWriteAccess(this.currentSettings.UploadDirId, this._portalSettings))
                            {
                                this.settings["filebrowserUploadUrl"] =
                                    Globals.ResolveUrl(
                                        $"~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Command=FileUpload&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}&mid={this.parentModulId}&ckid={this.ID}&mode={this.currentSettings.SettingMode}&lang={CultureInfo.CurrentCulture.Name}");
                                this.settings["filebrowserFlashUploadUrl"] =
                                    Globals.ResolveUrl(
                                        $"~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Command=FlashUpload&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}&mid={this.parentModulId}&ckid={this.ID}&mode={this.currentSettings.SettingMode}&lang={CultureInfo.CurrentCulture.Name}");
                                this.settings["filebrowserImageUploadUrl"] =
                                    Globals.ResolveUrl(
                                        $"~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Command=ImageUpload&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}&mid={this.parentModulId}&ckid={this.ID}&mode={this.currentSettings.SettingMode}&lang={CultureInfo.CurrentCulture.Name}");
                                this.settings["imageUploadUrl"] =
                                    Globals.ResolveUrl(
                                        $"~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Command=ImageAutoUpload&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}&mid={this.parentModulId}&ckid={this.ID}&mode={this.currentSettings.SettingMode}&lang={CultureInfo.CurrentCulture.Name}");
                            }

                            this.settings["filebrowserWindowWidth"] = "870";
                            this.settings["filebrowserWindowHeight"] = "800";

                            // Set Browser Authorize
                            const bool CKDNNIsAuthorized = true;

                            HttpContext.Current.Session["CKDNNIsAuthorized"] = CKDNNIsAuthorized;

                            DataCache.SetCache("CKDNNIsAuthorized", CKDNNIsAuthorized);
                        }

                        break;
                    case Browser.CKFinder:
                        {
                            this.settings["filebrowserBrowseUrl"] =
                                Globals.ResolveUrl(
                                    $"~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.html?tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}");
                            this.settings["filebrowserImageBrowseUrl"] =
                                Globals.ResolveUrl(
                                    $"~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.html?type=Images&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}");
                            this.settings["filebrowserFlashBrowseUrl"] =
                                Globals.ResolveUrl(
                                    $"~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.html?type=Flash&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}");

                            if (Utility.CheckIfUserHasFolderWriteAccess(this.currentSettings.UploadDirId, this._portalSettings))
                            {
                                this.settings["filebrowserUploadUrl"] =
                                    Globals.ResolveUrl(
                                        $"~/Providers/HtmlEditorProviders/CKEditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Files&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}");
                                this.settings["filebrowserFlashUploadUrl"] =
                                    Globals.ResolveUrl(
                                        $"~/Providers/HtmlEditorProviders/CKEditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Flash&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}");
                                this.settings["filebrowserImageUploadUrl"] =
                                    Globals.ResolveUrl(
                                        $"~/Providers/HtmlEditorProviders/CKEditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Images&tabid={this._portalSettings.ActiveTab.TabID}&PortalID={this._portalSettings.PortalId}");
                            }

                            HttpContext.Current.Session["CKDNNSubDirs"] = this.currentSettings.SubDirs;

                            HttpContext.Current.Session["CKDNNRootDirId"] = this.currentSettings.BrowserRootDirId;
                            HttpContext.Current.Session["CKDNNUpDirId"] = this.currentSettings.UploadDirId;

                            // Set Browser Authorize
                            const bool CKDNNIsAuthorized = true;

                            HttpContext.Current.Session["CKDNNIsAuthorized"] = CKDNNIsAuthorized;

                            DataCache.SetCache("CKDNNIsAuthorized", CKDNNIsAuthorized);
                        }

                        break;
                }

                this.isMerged = true;

                return this.settings;
            }
        }

        /// <summary>
        ///  Gets or sets The ToolBarName defined in config to override all other Toolbars
        /// </summary>
        public string ToolBarName
        {
            // EL 20101006
            get => this.toolBarNameOverride;

            set => this.toolBarNameOverride = value;
        }

        /// <summary>
        /// Gets or sets Value.
        /// </summary>
        [DefaultValue("")]
        public string Value
        {
            get
            {
                var o = this.ViewState["Value"];

                return o == null ? string.Empty : Convert.ToString(o);
            }

            set => this.ViewState["Value"] = value;
        }

        /// <summary>
        /// Gets a value indicating whether Has Microsoft Ajax is installed.
        /// </summary>
        private static bool HasMsAjax
        {
            get
            {
                if (hasMsAjax != null)
                {
                    return hasMsAjax.Value;
                }

                hasMsAjax = false;

                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var asm in
                    appAssemblies.Where(asm => asm.ManifestModule.Name == "System.Web.Extensions.dll"))
                {
                    try
                    {
                        var scriptManager = asm.GetType("System.Web.UI.ScriptManager");

                        if (scriptManager != null)
                        {
                            hasMsAjax = true;
                        }
                    }
                    catch
                    {
                        hasMsAjax = false;
                    }

                    break;
                }

                return hasMsAjax.Value;
            }
        }

        /// <summary>
        ///   Gets Name for the Current Resource file name
        /// </summary>
        private static string SResXFile => Globals.ResolveUrl(
            $"~/Providers/HtmlEditorProviders/CKEditor/{Localization.LocalResourceDirectory}/Options.aspx.resx");

        /// <summary>
        /// Gets the tabs array.
        /// </summary>
        /// <returns>Returns the Tabs Array as String</returns>
        private string GetTabsArray()
        {
            // Generate Pages Array
            var pagesArray = new StringBuilder();

            pagesArray.Append("[");

            var domainName = $"http://{Globals.GetDomainName(HttpContext.Current.Request, true)}";

            foreach (var tab in TabController.GetPortalTabs(
                this._portalSettings.PortalId, -1, false, null, true, false, true, true, true))
            {
                var tabUrl = PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", this._portalSettings.PortalId, false)
                             && !string.IsNullOrEmpty(tab.CultureCode)
                                 ? Globals.FriendlyUrl(
                                     tab,
                                     $"{Globals.ApplicationURL(tab.TabID)}&language={tab.CultureCode}")
                                 : Globals.FriendlyUrl(tab, Globals.ApplicationURL(tab.TabID));

                tabUrl = Globals.ResolveUrl(Regex.Replace(tabUrl, domainName, "~", RegexOptions.IgnoreCase));

                var tabName = Microsoft.JScript.GlobalObject.escape(tab.TabName);

                if (tab.Level.Equals(0))
                {
                    pagesArray.AppendFormat("new Array('| {0}','{1}'),", tabName, tabUrl);
                }
                else
                {
                    var separator = new StringBuilder();

                    for (var index = 0; index < tab.Level; index++)
                    {
                        separator.Append("--");
                    }

                    pagesArray.AppendFormat("new Array('|{0} {1}','{2}'),", separator, tabName, tabUrl);
                }
            }

            if (pagesArray.ToString().EndsWith(","))
            {
                pagesArray.Remove(pagesArray.Length - 1, 1);
            }

            pagesArray.Append("]");

            return pagesArray.ToString();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Finds the module instance.
        /// </summary>
        /// <param name="editorControl">The editor control.</param>
        /// <returns>
        /// The Instances found
        /// </returns>
        public static Control FindModuleInstance(Control editorControl)
        {
            var ctl = editorControl.Parent;
            Control selectedCtl = null;
            Control possibleCtl = null;

            while (ctl != null)
            {
                if (ctl is PortalModuleBase portalModuleBase)
                {
                    if (portalModuleBase.TabModuleId == Null.NullInteger)
                    {
                        possibleCtl = ctl;
                    }
                    else
                    {
                        selectedCtl = ctl;
                        break;
                    }
                }

                ctl = ctl.Parent;
            }

            if (selectedCtl == null & possibleCtl != null)
            {
                selectedCtl = possibleCtl;
            }

            return selectedCtl;
        }

        /// <summary>
        /// The has rendered text area.
        /// </summary>
        /// <param name="control">
        /// The control.
        /// </param>
        /// <returns>
        /// Returns if it has rendered text area.
        /// </returns>
        public bool HasRenderedTextArea(Control control)
        {
            if (control is CKEditorControl editorControl && editorControl.IsRendered)
            {
                return true;
            }

            return control.Controls.Cast<Control>().Any(this.HasRenderedTextArea);
        }

        #endregion

        #region Implemented Interfaces

        #region IPostBackDataHandler

        /// <summary>
        /// The load post data.
        /// </summary>
        /// <param name="postDataKey">
        /// The post data key.
        /// </param>
        /// <param name="postCollection">
        /// The post collection.
        /// </param>
        /// <returns>
        /// Returns if the PostData are loaded.
        /// </returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            try
            {
                var currentValue = this.Value;
                var postedValue = postCollection[postDataKey];

                if (currentValue == null | !postedValue.Equals(currentValue))
                {
                    if (this.currentSettings.InjectSyntaxJs)
                    {
                        if (postedValue.Contains("<code class=\"language-") && !postedValue.Contains("highlight.pack.js"))
                        {
                            // Add CodeSnipped Plugin JS/CSS
                            postedValue =
                                string.Format(
                                    "<!-- Injected  Highlight.js Code --><script type=\"text/javascript\" src=\"{0}plugins/codesnippet/lib/highlight/highlight.pack.js\"></script><link type=\"text/css\" rel=\"stylesheet\" href=\"{0}plugins/codesnippet/lib/highlight/styles/default.css\"/><script type=\"text/javascript\">window.onload = function() {{var aCodes = document.getElementsByTagName('pre');for (var i=0; i < aCodes.length;i++){{hljs.highlightBlock(aCodes[i]);}} }};</script>{1}",
                                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/"),
                                    postedValue);
                        }

                        if (postedValue.Contains("<span class=\"math-tex\">") && !postedValue.Contains("MathJax.js"))
                        {
                            // Add MathJax Plugin
                            postedValue =
                                $"<!-- Injected MathJax Code --><script type=\"text/javascript\" src=\"//cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.2/MathJax.js?config=TeX-AMS_HTML\"></script>{postedValue}";
                        }
                    }

                    this.Value = postedValue;

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// The raise post data changed event.
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            // Do nothing
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Update the Editor after the Post back
        /// And Create Main Script to Render the Editor
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (HasMsAjax)
            {
                return;
            }

            this.RegisterCKEditorLibrary();

            this.GenerateEditorLoadScript();
        }

        /// <summary>
        /// The render.
        /// </summary>
        /// <param name="outWriter">
        /// The out writer.
        /// </param>
        protected override void Render(HtmlTextWriter outWriter)
        {
            // Render loading div
            outWriter.AddAttribute(
                HtmlTextWriterAttribute.Id,
                $"ckeditorLoading{this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty)}");
            outWriter.RenderBeginTag(HtmlTextWriterTag.Div);

            outWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ckeditorLoader");
            outWriter.RenderBeginTag(HtmlTextWriterTag.Div);
            outWriter.Write(
                "<img src=\"{0}\" alt=\"loader\" width=\"50\" height\"50\" />",
                this.ResolveUrl("~/images/loading.gif"));
            outWriter.RenderEndTag();

            outWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ckeditorLoaderText");
            outWriter.RenderBeginTag(HtmlTextWriterTag.Div);
            outWriter.Write(Localization.GetString("LoadingEditor.Text", SResXFile));
            outWriter.RenderEndTag();

            outWriter.RenderEndTag();

            outWriter.RenderBeginTag(HtmlTextWriterTag.Div);
            outWriter.RenderBeginTag(HtmlTextWriterTag.Noscript);
            outWriter.RenderBeginTag(HtmlTextWriterTag.P);

            outWriter.Write(Localization.GetString("NoJava.Text", SResXFile));

            outWriter.RenderEndTag();
            outWriter.RenderEndTag();
            outWriter.RenderEndTag();

            outWriter.Write(
                "<input type=\"hidden\" name=\"CKDNNporid\" id=\"CKDNNporid\" value=\"{0}\">",
                this._portalSettings.PortalId);

            outWriter.Write(outWriter.NewLine);

            if (!string.IsNullOrEmpty(this.currentSettings.Config.Width))
            {
                outWriter.AddAttribute(
                    HtmlTextWriterAttribute.Style,
                    $"width:{this.currentSettings.Config.Width};");
            }

            outWriter.RenderBeginTag(HtmlTextWriterTag.Div);

            // Write text area
            outWriter.AddAttribute(
                HtmlTextWriterAttribute.Id,
                this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty));
            outWriter.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);

            outWriter.AddAttribute(HtmlTextWriterAttribute.Cols, "80");
            outWriter.AddAttribute(HtmlTextWriterAttribute.Rows, "10");

            outWriter.AddAttribute(HtmlTextWriterAttribute.Class, "editor");
            outWriter.AddAttribute("aria-label", "editor");

            outWriter.AddAttribute(HtmlTextWriterAttribute.Style, "visibility: hidden; display: none;");

            outWriter.RenderBeginTag(HtmlTextWriterTag.Textarea);

            if (string.IsNullOrEmpty(this.Value))
            {
                if (!string.IsNullOrEmpty(this.currentSettings.BlankText))
                {
                    outWriter.Write(this.Context.Server.HtmlEncode(this.currentSettings.BlankText));
                }
            }
            else
            {
                outWriter.Write(this.Context.Server.HtmlEncode(this.Value));
            }

            outWriter.RenderEndTag();

            outWriter.RenderEndTag();

            this.IsRendered = true;

            /////////////////
            if (!this.HasRenderedTextArea(this.Page))
            {
                return;
            }

            if (PortalSecurity.IsInRoles(this._portalSettings.AdministratorRoleName))
            {
                outWriter.AddAttribute(HtmlTextWriterAttribute.Style, "text-align:center;");
                outWriter.RenderBeginTag(HtmlTextWriterTag.P);

                outWriter.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");

                outWriter.AddAttribute(
                    HtmlTextWriterAttribute.Onclick,
                    $"window.open('{Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/Options.aspx")}?mid={this.parentModulId}&tid={this._portalSettings.ActiveTab.TabID}&minc={this.ID}&PortalID={this._portalSettings.PortalId}&langCode={CultureInfo.CurrentCulture.Name}','Options','width=850,height=750,resizable=yes')",
                    true);

                outWriter.AddAttribute(HtmlTextWriterAttribute.Class, "CommandButton");

                outWriter.AddAttribute(
                    HtmlTextWriterAttribute.Id,
                    $"{this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty)}_ckoptions");

                outWriter.RenderBeginTag(HtmlTextWriterTag.A);

                outWriter.Write(Localization.GetString("Options.Text", SResXFile));

                outWriter.RenderEndTag();
                outWriter.RenderEndTag();
            }

            /////////////////
        }

        /// <summary>
        /// Initializes the Editor
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void CKEditorInit(object sender, EventArgs e)
        {
            this.Page?.RegisterRequiresPostBack(this); // Ensures that postback is handled

            this.myParModule = (PortalModuleBase)FindModuleInstance(this);

            if (this.myParModule == null || this.myParModule.ModuleId == -1)
            {
                // Get Parent ModuleID From this ClientID
                var clientId = this.ClientID.Substring(this.ClientID.IndexOf("ctr", StringComparison.Ordinal) + 3);

                clientId = clientId.Remove(this.ClientID.IndexOf("_", StringComparison.Ordinal));

                try
                {
                    this.parentModulId = int.Parse(clientId);
                }
                catch (Exception)
                {
                    // The is no real module, then use the "User Accounts" module (Profile editor)
                    var db = new ModuleController();
                    var objm = db.GetModuleByDefinition(this._portalSettings.PortalId, "User Accounts");

                    this.parentModulId = objm.TabModuleID;
                }
            }
            else
            {
                this.parentModulId = this.myParModule.ModuleId;
            }

            this.CheckFileBrowser();

            this.LoadAllSettings();

            if (!HasMsAjax)
            {
                return;
            }

            this.RegisterCKEditorLibrary();

            this.GenerateEditorLoadScript();
        }

        /// <summary>
        /// The check file browser.
        /// </summary>
        private void CheckFileBrowser()
        {
            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
            var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            if (objProvider == null || string.IsNullOrEmpty(objProvider.Attributes["ck_browser"]))
            {
                return;
            }

            switch (objProvider.Attributes["ck_browser"])
            {
                case "ckfinder":
                    this.currentSettings.BrowserMode = Browser.CKFinder;
                    break;
                case "standard":
                    this.currentSettings.BrowserMode = Browser.StandardBrowser;
                    break;
                case "none":
                    this.currentSettings.BrowserMode = Browser.None;
                    break;
            }
        }

        /// <summary>
        /// Load Portal/Page/Module Settings
        /// </summary>
        private void LoadAllSettings()
        {
            var settingsDictionary = Utility.GetEditorHostSettings();
            var portalRoles = new RoleController().GetRoles(this._portalSettings.PortalId);

            // Load Default Settings
            this.currentSettings = SettingsUtil.GetDefaultSettings(
                this._portalSettings,
                this._portalSettings.HomeDirectoryMapPath,
                this.settings["configFolder"],
                portalRoles);

            // Set Current Mode to Default
            this.currentSettings.SettingMode = SettingsMode.Default;

            var portalKey = $"DNNCKP#{this._portalSettings.PortalId}#";
            var pageKey = $"DNNCKT#{this._portalSettings.ActiveTab.TabID}#";
            var moduleKey = $"DNNCKMI#{this.parentModulId}#INS#{this.ID}#";

            // Load Portal Settings ?!
            if (SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, portalKey))
            {
               /* throw new ApplicationException(settingsDictionary.FirstOrDefault(
                            setting => setting.Name.Equals(string.Format("{0}{1}", portalKey, "StartupMode"))).Value);*/
                this.currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                    this._portalSettings, this.currentSettings, settingsDictionary, portalKey, portalRoles);

                // Set Current Mode to Portal
                this.currentSettings.SettingMode = SettingsMode.Portal;
            }

            // Load Page Settings ?!
            if (SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, pageKey))
            {
                this.currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                    this._portalSettings, this.currentSettings, settingsDictionary, pageKey, portalRoles);

                // Set Current Mode to Page
                this.currentSettings.SettingMode = SettingsMode.Page;
            }

            // Load Module Settings ?!
            if (!SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, this.parentModulId))
            {
                return;
            }

            this.currentSettings = SettingsUtil.LoadModuleSettings(
                this._portalSettings, this.currentSettings, moduleKey, this.parentModulId, portalRoles);

            // Set Current Mode to Module Instance
            this.currentSettings.SettingMode = SettingsMode.ModuleInstance;
        }

        /// <summary>
        /// Format the URL from FileID to File Path URL
        /// </summary>
        /// <param name="inputUrl">
        /// The Input URL.
        /// </param>
        /// <returns>
        /// The formatted URL.
        /// </returns>
        private string FormatUrl(string inputUrl)
        {
            var formattedUrl = string.Empty;

            if (string.IsNullOrEmpty(inputUrl))
            {
                return formattedUrl;
            }

            if (inputUrl.StartsWith("http://"))
            {
                formattedUrl = inputUrl;
            }
            else if (inputUrl.StartsWith("FileID="))
            {
                var fileId = int.Parse(inputUrl.Substring(7));

                var objFileInfo = FileManager.Instance.GetFile(fileId);

                formattedUrl = this._portalSettings.HomeDirectory + objFileInfo.Folder + objFileInfo.FileName;
            }
            else
            {
                formattedUrl = this._portalSettings.HomeDirectory + inputUrl;
            }

            return formattedUrl;
        }

        /// <summary>
        /// Load the Settings from the web.config file
        /// </summary>
        private void LoadConfigSettings()
        {
            this.settings = new NameValueCollection();

            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
            var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            if (objProvider == null)
            {
                return;
            }

            foreach (string key in objProvider.Attributes)
            {
                if (!key.ToLower().StartsWith("ck_"))
                {
                    continue;
                }

                var adjustedKey = key.Substring(3, key.Length - 3).ToLower();

                if (adjustedKey != string.Empty)
                {
                    this.settings[adjustedKey] = objProvider.Attributes[key];
                }
            }
        }

        /// <summary>
        /// This registers a startup JavaScript with compatibility with the Microsoft Ajax
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="addScriptTags">
        /// The add Script Tags.
        /// </param>
        private void RegisterStartupScript(string key, string script, bool addScriptTags)
        {
            if (HasMsAjax)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, addScriptTags);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), key, script, true);
            }
        }

        /// <summary>
        /// Registers the on submit statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        /// <param name="script">The script.</param>
        private void RegisterOnSubmitStatement(Type type, string key, string script)
        {
            if (HasMsAjax)
            {
                ScriptManager.RegisterOnSubmitStatement(this, type, key, script);
            }
            else
            {
                this.Page.ClientScript.RegisterOnSubmitStatement(type, key, script);
            }
        }

        /// <summary>
        /// Set Toolbar based on Current User
        /// </summary>
        /// <param name="alternateConfigSubFolder">
        /// The alternate config sub folder.
        /// </param>
        /// <returns>
        /// Toolbar Name
        /// </returns>
        private string SetUserToolbar(string alternateConfigSubFolder)
        {
            var toolbarName = HttpContext.Current.Request.IsAuthenticated
                                 && PortalSecurity.IsInRoles(this._portalSettings.AdministratorRoleName)
                                     ? "Full"
                                     : "Basic";

            var listToolbarSets = ToolbarUtil.GetToolbars(
                this._portalSettings.HomeDirectoryMapPath, alternateConfigSubFolder);

            var listUserToolbarSets = new List<ToolbarSet>();

            var roleController = new RoleController();

            if (this.currentSettings.ToolBarRoles.Count <= 0)
            {
                return toolbarName;
            }

            foreach (var roleToolbar in this.currentSettings.ToolBarRoles)
            {
                if (roleToolbar.RoleId.Equals(-1) && !HttpContext.Current.Request.IsAuthenticated)
                {
                    return roleToolbar.Toolbar;
                }

                if (roleToolbar.RoleId.Equals(-1))
                {
                    continue;
                }

                // Role
                var role = roleController.GetRoleById(this._portalSettings.PortalId, roleToolbar.RoleId);

                if (role == null)
                {
                    continue;
                }

                if (!PortalSecurity.IsInRole(role.RoleName))
                {
                    continue;
                }

                // Handle Different Roles
                if (!listToolbarSets.Any(toolbarSel => toolbarSel.Name.Equals(roleToolbar.Toolbar)))
                {
                    continue;
                }

                var toolbar = listToolbarSets.Find(toolbarSel => toolbarSel.Name.Equals(roleToolbar.Toolbar));

                listUserToolbarSets.Add(toolbar);

                /*if (roleToolbar.RoleId.Equals(this._portalSettings.AdministratorRoleId) && HttpContext.Current.Request.IsAuthenticated)
                    {
                        if (PortalSecurity.IsInRole(roleName))
                        {
                            return roleToolbar.Toolbar;
                        }
                    }*/
            }

            if (listUserToolbarSets.Count <= 0)
            {
                return toolbarName;
            }

            // Compare The User Toolbars if the User is more then One Role, and apply the Toolbar with the Highest Priority
            var highestPrio = listUserToolbarSets.Max(toolb => toolb.Priority);

            return ToolbarUtil.FindHighestToolbar(listUserToolbarSets, highestPrio).Name;
        }

        /// <summary>
        /// Registers the CKEditor library.
        /// </summary>
        private void RegisterCKEditorLibrary()
        {
            ClientResourceManager.RegisterStyleSheet(
                this.Page,
                Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/editor.css"));

            var cs = this.Page.ClientScript;

            var type = this.GetType();

            const string CsName = "CKEdScript";
            const string CsAdaptName = "CKAdaptScript";
            const string CsFindName = "CKFindScript";

            JavaScript.RequestRegistration(CommonJs.jQuery);

            // Inject jQuery if editor is loaded in a RadWindow
            if (HttpContext.Current.Request.QueryString["rwndrnd"] != null)
            {
                ScriptManager.RegisterClientScriptInclude(
                    this, type, "jquery_registered", "//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js");
            }

            if (File.Exists(this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js"))
                && !cs.IsClientScriptIncludeRegistered(type, CsName))
            {
                cs.RegisterClientScriptInclude(
                    type, CsName, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js"));
            }

            if (
                File.Exists(
                    this.Context.Server.MapPath(
                        "~/Providers/HtmlEditorProviders/CKEditor/js/jquery.ckeditor.adapter.js"))
                && !cs.IsClientScriptIncludeRegistered(type, CsAdaptName))
            {
                cs.RegisterClientScriptInclude(
                    type,
                    CsAdaptName,
                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/js/jquery.ckeditor.adapter.js"));
            }

            if (
                File.Exists(
                    this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.js")) &&
                !cs.IsClientScriptIncludeRegistered(type, CsFindName) && this.currentSettings.BrowserMode.Equals(Browser.CKFinder))
            {
                cs.RegisterClientScriptInclude(
                    type,
                    CsFindName,
                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.js"));
            }

            // Load Custom JS File
            if (!string.IsNullOrEmpty(this.currentSettings.CustomJsFile)
                && !cs.IsClientScriptIncludeRegistered(type, "CKCustomJSFile"))
            {
                cs.RegisterClientScriptInclude(
                    type,
                    "CKCustomJSFile",
                    this.FormatUrl(this.currentSettings.CustomJsFile));
            }
        }

        /// <summary>
        /// Generates the editor load script.
        /// </summary>
        private void GenerateEditorLoadScript()
        {
            var editorVar =
                $"editor{this.ClientID.Substring(this.ClientID.LastIndexOf("_", StringComparison.Ordinal) + 1).Replace("-", string.Empty)}";

            var editorFixedId = this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty);

            if (HasMsAjax)
            {
                var postBackScript =
                    string.Format(
                        @"if (CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement(); CKEDITOR.instances.{0}.destroy(); }}",
                        editorFixedId);

                this.RegisterOnSubmitStatement(this.GetType(), $"CKEditor_OnAjaxSubmit_{editorFixedId}", postBackScript);
            }

            var editorScript = new StringBuilder();

            editorScript.AppendFormat(
                "Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(LoadCKEditorInstance_{0});", editorFixedId);

            editorScript.AppendFormat("function LoadCKEditorInstance_{0}(sender,args) {{", editorFixedId);

            editorScript.AppendFormat(
                @"if (jQuery(""[id*='UpdatePanel']"").length == 0 && CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement();}}",
                editorFixedId);

            editorScript.AppendFormat(
                "if (document.getElementById('{0}') == null){{return;}}",
                editorFixedId);

            // Render EditorConfig
            editorScript.AppendFormat("var editorConfig{0} = {{", editorVar);

            var keysCount = this.Settings.Keys.Count;
            var currentCount = 0;

            // Write options
            foreach (string key in this.Settings.Keys)
            {
                var value = this.Settings[key];

                currentCount++;

                // Is boolean state or string
                if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase)
                    || value.Equals("false", StringComparison.InvariantCultureIgnoreCase) || value.StartsWith("[")
                    || value.StartsWith("{") || Utility.IsNumeric(value))
                {
                    if (value.Equals("True"))
                    {
                        value = "true";
                    }
                    else if (value.Equals("False"))
                    {
                        value = "false";
                    }

                    editorScript.AppendFormat("{0}:{1}", key, value);

                    editorScript.Append(currentCount == keysCount ? "};" : ",");
                }
                else
                {
                    if (key == "browser")
                    {
                        continue;
                    }

                    editorScript.AppendFormat(value.EndsWith("/i") ? "{0}:{1}" : "{0}:\'{1}\'", key, value);

                    editorScript.Append(currentCount == keysCount ? "};" : ",");
                }
            }

            editorScript.AppendFormat(
                "if (CKEDITOR.instances.{0}){{return;}}",
                editorFixedId);

            // Check if we can use jQuery or $, and if both fail use ckeditor without the adapter
            editorScript.Append("if (jQuery().ckeditor) {");

            editorScript.AppendFormat("var {0} = jQuery('#{1}').ckeditor(editorConfig{0});", editorVar, editorFixedId);

            editorScript.Append("} else if ($.ckeditor) {");

            editorScript.AppendFormat("var {0} = $('#{1}').ckeditor(editorConfig{0});", editorVar, editorFixedId);

            editorScript.Append("} else {");

            editorScript.AppendFormat("var {0} = CKEDITOR.replace( '{1}', editorConfig{0});", editorVar, editorFixedId);

            editorScript.Append("}");

            // firefox maximize fix
            editorScript.Append("CKEDITOR.on('instanceReady', function (ev) {");
            editorScript.AppendFormat(
                "document.getElementById('ckeditorLoading{0}').style.display = 'none';",
                this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty));
            editorScript.Append("ev.editor.on('maximize', function () {");
            editorScript.Append("if (ev.editor.commands.maximize.state == 1) {");
            editorScript.Append("var mainDocument = CKEDITOR.document;");
            editorScript.Append("CKEDITOR.env.gecko && mainDocument.getDocumentElement().setStyle( 'position', 'fixed' );");
            editorScript.Append("}");
            editorScript.Append("});");
            editorScript.Append("});");

            // End of LoadScript
            editorScript.Append("}");

            this.RegisterStartupScript($@"{editorFixedId}_CKE_Startup", editorScript.ToString(), true);
        }

        #endregion
    }
}
