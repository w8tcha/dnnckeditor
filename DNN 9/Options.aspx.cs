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

namespace WatchersNET.CKEditor
{
    #region

    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI.HtmlControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using Microsoft.VisualBasic;

    using WatchersNET.CKEditor.Controls;

    using Globals = DotNetNuke.Common.Globals;

    #endregion

    /// <summary>
    /// The options page.
    /// </summary>
    public partial class Options : PageBase
    {
        #region Constants and Fields

        /// <summary>
        /// The request.
        /// </summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

        /// <summary>
        /// The _portal settings.
        /// </summary>
        private PortalSettings curPortalSettings;

        /// <summary>
        ///   Gets Current Language from Url
        /// </summary>
        protected string LangCode => this.request.QueryString["langCode"];

        /// <summary>
        ///   Gets the Name for the Current Resource file name
        /// </summary>
        protected string ResXFile
        {
            get
            {
                var page = this.Request.ServerVariables["SCRIPT_NAME"].Split('/');

                var fileRoot = string.Format(
                    "{0}/{1}/{2}.resx",
                    this.TemplateSourceDirectory,
                    Localization.LocalResourceDirectory,
                    page[page.GetUpperBound(0)]);

                return fileRoot;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Register the java scripts and CSS
        /// </summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        protected override void OnPreRender(EventArgs e)
        {
            var jqueryScriptLink = new HtmlGenericControl("script");

            jqueryScriptLink.Attributes["type"] = "text/javascript";
            jqueryScriptLink.Attributes["src"] = this.ResolveUrl("Scripts/jquery-3.4.1.min.js");

            this.favicon.Controls.Add(jqueryScriptLink);

            var jqueryUiScriptLink = new HtmlGenericControl("script");

            jqueryUiScriptLink.Attributes["type"] = "text/javascript";
            jqueryUiScriptLink.Attributes["src"] = this.ResolveUrl("Scripts/jquery-ui-1.12.1.min.js");

            this.favicon.Controls.Add(jqueryUiScriptLink);

            var notificationScriptLink = new HtmlGenericControl("script");

            notificationScriptLink.Attributes["type"] = "text/javascript";
            notificationScriptLink.Attributes["src"] = this.ResolveUrl("Scripts/jquery.notification.js");

            this.favicon.Controls.Add(notificationScriptLink);

            var optionsScriptLink = new HtmlGenericControl("script");

            optionsScriptLink.Attributes["type"] = "text/javascript";
            optionsScriptLink.Attributes["src"] = this.ResolveUrl("Scripts/Options.js");

            this.favicon.Controls.Add(optionsScriptLink);

            var objCssLink = new HtmlGenericSelfClosing("link");

            objCssLink.Attributes["rel"] = "stylesheet";
            objCssLink.Attributes["type"] = "text/css";
            objCssLink.Attributes["href"] = this.ResolveUrl("Content/themes/base/jquery-ui.min.css");

            this.favicon.Controls.Add(objCssLink);

            var notificationCssLink = new HtmlGenericSelfClosing("link");

            notificationCssLink.Attributes["rel"] = "stylesheet";
            notificationCssLink.Attributes["type"] = "text/css";
            notificationCssLink.Attributes["href"] = this.ResolveUrl("Content/jquery.notification.css");

            this.favicon.Controls.Add(notificationCssLink);

            var optionsCssLink = new HtmlGenericSelfClosing("link");

            optionsCssLink.Attributes["rel"] = "stylesheet";
            optionsCssLink.Attributes["type"] = "text/css";
            optionsCssLink.Attributes["href"] = this.ResolveUrl("Content/Options.css");

            this.favicon.Controls.Add(optionsCssLink);

            base.OnPreRender(e);
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            this.InitializeComponent();
            base.OnInit(e);

            // Favicon
            this.LoadFavIcon();
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            var moduleId = -1;
            var tabId = -1;

            ModuleInfo modInfo = null;
            var db = new ModuleController();

            try
            {
                // Get ModuleID from Url
                if (this.request.QueryString["mid"] != null && Information.IsNumeric(this.Request.QueryString["mid"]))
                {
                    moduleId = Convert.ToInt32(this.request.QueryString["mid"]);
                }

                // Get TabId from Url
                if (this.request.QueryString["tid"] != null && Information.IsNumeric(this.Request.QueryString["tid"]))
                {
                    tabId = Convert.ToInt32(this.request.QueryString["tid"]);
                }

                if (moduleId != -1 && tabId != -1)
                {
                    modInfo = db.GetModule(moduleId, tabId, false);
                }
                else
                {
                    this.ClosePage();
                }
            }
            catch (Exception exception)
            {
                Exceptions.ProcessPageLoadException(exception);

                this.ClosePage();
            }

            try
            {
                // Get ModuleID from Url
                var editorOptions = (CKEditorOptions)this.Page.LoadControl("CKEditorOptions.ascx");

                editorOptions.ID = "CKEditor_Options";
                editorOptions.ModuleConfiguration = modInfo;

                this.phControls.Controls.Add(editorOptions);
            }
            catch (Exception exception)
            {
                Exceptions.ProcessPageLoadException(exception);

                this.ClosePage();
            }
        }

        /// <summary>
        /// Closes the page.
        /// </summary>
        private void ClosePage()
        {
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), "closeScript", "javascript:self.close();", true);
        }

        /// <summary>
        /// Gets the portal settings.
        /// </summary>
        /// <returns>
        /// The Portal Settings
        /// </returns>
        private PortalSettings GetPortalSettings()
        {
            int tabId = 0, portalId = 0;

            PortalSettings portalSettings;

            try
            {
                if (this.request.QueryString["tabid"] != null)
                {
                    tabId = int.Parse(this.request.QueryString["tabid"]);
                }

                if (this.request.QueryString["PortalID"] != null)
                {
                    portalId = int.Parse(this.request.QueryString["PortalID"]);
                }

                var domainName = Globals.GetDomainName(this.Request, true);

                var portalAlias = PortalAliasController.GetPortalAliasByPortal(portalId, domainName);

                var objPortalAliasInfo = PortalAliasController.Instance.GetPortalAlias(portalAlias);

                portalSettings = new PortalSettings(tabId, objPortalAliasInfo);
            }
            catch (Exception)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return portalSettings;
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.curPortalSettings = this.GetPortalSettings();
        }

        /// <summary>
        /// Load Favicon from Current Portal Home Directory
        /// </summary>
        private void LoadFavIcon()
        {
            if (!File.Exists(Path.Combine(this.curPortalSettings.HomeDirectoryMapPath, "favicon.ico")))
            {
                return;
            }

            var faviconUrl = Path.Combine(this.curPortalSettings.HomeDirectory, "favicon.ico");

            var objLink = new HtmlGenericSelfClosing("link");

            objLink.Attributes["rel"] = "shortcut icon";
            objLink.Attributes["href"] = faviconUrl;

            this.favicon.Controls.Add(objLink);
        }

        #endregion
    }
}