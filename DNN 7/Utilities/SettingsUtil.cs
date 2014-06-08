/*
 * CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor.Utilities
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;

    using WatchersNET.CKEditor.Constants;
    using WatchersNET.CKEditor.Objects;

    #endregion

    /// <summary>
    /// Settings Base Helper Class
    /// </summary>
    public class SettingsUtil
    {
       #region Public Methods

        /// <summary>
        /// Checks the exists portal or page settings.
        /// </summary>
        /// <param name="settingsDictionary">The settings dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>Returns if Portal or Page Settings Exists</returns>
        internal static bool CheckExistsPortalOrPageSettings(Dictionary<string, string> settingsDictionary, string key)
        {
            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.SKIN)))
            {
                return !string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.SKIN)]);
            }

            // No Portal/Page Settings Found
            return false;
        }

        /// <summary>
        /// Checks there are any Module Settings
        /// </summary>
        /// <param name="moduleKey">The module key.</param>
        /// <param name="moduleId">The module id.</param>
        /// <returns>Returns if The Module Settings Exists or not.</returns>
        internal static bool CheckExistsModuleSettings(string moduleKey, int moduleId)
        {
            var hshModSet = new ModuleController().GetModuleSettings(moduleId);

            return hshModSet.Keys.Cast<string>().Any(key => key.StartsWith(moduleKey));
        }

        /// <summary>
        /// Checks the exists of the module instance settings.
        /// </summary>
        /// <param name="moduleKey">The module key.</param>
        /// <param name="moduleId">The module id.</param>
        /// <returns>Returns if The Module Settings Exists or not.</returns>
        internal static bool CheckExistsModuleInstanceSettings(string moduleKey, int moduleId)
        {
            var hshModSet = new ModuleController().GetModuleSettings(moduleId);

            return !string.IsNullOrEmpty((string)hshModSet[string.Format("{0}skin", moduleKey)]);
        }

        /// <summary>
        /// Loads the portal or page settings.
        /// </summary>
        /// <param name="portalSettings">The current portal settings. </param>
        /// <param name="currentSettings">The current settings.</param>
        /// <param name="settingsDictionary">The Host settings dictionary.</param>
        /// <param name="key">The Portal or Page key.</param>
        /// <param name="portalRoles">The Portal Roles</param>
        /// <returns>
        /// Returns the Filled Settings
        /// </returns>
        internal static EditorProviderSettings LoadPortalOrPageSettings(PortalSettings portalSettings, EditorProviderSettings currentSettings, Dictionary<string, string> settingsDictionary, string key, ArrayList portalRoles)
        {
            var roleController = new RoleController();

            var roles = new ArrayList();

            // Import all Editor config settings
            foreach (PropertyInfo info in
                GetEditorConfigProperties()
                    .Where(
                        info =>
                        settingsDictionary.ContainsKey(string.Format("{0}{1}", key, info.Name))
                        && !string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, info.Name)])
                        || info.Name.Equals("CodeMirror") || info.Name.Equals("WordCount")))
            {
                switch (info.PropertyType.Name)
                {
                    case "String":
                        info.SetValue(
                            currentSettings.Config, settingsDictionary[string.Format("{0}{1}", key, info.Name)], null);
                        break;
                    case "Int32":
                        info.SetValue(
                            currentSettings.Config,
                            int.Parse(settingsDictionary[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "Decimal":
                        info.SetValue(
                            currentSettings.Config,
                            decimal.Parse(settingsDictionary[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "Boolean":
                        info.SetValue(
                            currentSettings.Config,
                            bool.Parse(settingsDictionary[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        info.SetValue(
                            currentSettings.Config,
                            (ToolBarLocation)
                            Enum.Parse(
                                typeof(ToolBarLocation), settingsDictionary[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "DefaultLinkType":
                        info.SetValue(
                            currentSettings.Config,
                            (LinkType)
                            Enum.Parse(typeof(LinkType), settingsDictionary[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "EnterMode":
                    case "ShiftEnterMode":
                        info.SetValue(
                            currentSettings.Config,
                            (EnterModus)
                            Enum.Parse(typeof(EnterModus), settingsDictionary[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "ContentsLangDirection":
                        info.SetValue(
                            currentSettings.Config,
                            (LanguageDirection)
                            Enum.Parse(
                                typeof(LanguageDirection), settingsDictionary[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "CodeMirror":
                        foreach (var codeMirrorInfo in
                            typeof(CodeMirror).GetProperties()
                                              .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                        {
                            switch (codeMirrorInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, codeMirrorInfo.Name)))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            settingsDictionary[string.Format("{0}{1}", key, codeMirrorInfo.Name)],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, codeMirrorInfo.Name)))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            bool.Parse(
                                                settingsDictionary[string.Format("{0}{1}", key, codeMirrorInfo.Name)]),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                    case "WordCount":
                        foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                        {
                            switch (wordCountInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, wordCountInfo.Name)))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            settingsDictionary[string.Format("{0}{1}", key, wordCountInfo.Name)],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, wordCountInfo.Name)))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            bool.Parse(
                                                settingsDictionary[string.Format("{0}{1}", key, wordCountInfo.Name)]),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                }
            }

            /////////////////

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.SKIN)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.SKIN)]))
                {
                    currentSettings.Config.Skin = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.SKIN)];
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME)]))
                {
                    currentSettings.Config.CodeMirror.Theme = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME)];
                }
            }

            List<ToolbarRoles> listToolbarRoles = (from RoleInfo objRole in portalRoles
                                                   where
                                                       settingsDictionary.ContainsKey(
                                                           string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB))
                                                   where
                                                       !string.IsNullOrEmpty(
                                                           settingsDictionary[string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB)])
                                                   let sToolbar =
                                                       settingsDictionary[string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB)]
                                                   select
                                                       new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = sToolbar })
                .ToList();

            if (settingsDictionary.ContainsKey(string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)]))
                {
                    string sToolbar = settingsDictionary[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)];

                    listToolbarRoles.Add(new ToolbarRoles { RoleId = -1, Toolbar = sToolbar });
                }
            }

            currentSettings.ToolBarRoles = listToolbarRoles;

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.ROLES)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.ROLES)]))
                {
                    string sRoles = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.ROLES)];

                    currentSettings.BrowserRoles = sRoles;

                    string[] rolesA = sRoles.Split(';');

                    foreach (string sRoleName in rolesA)
                    {
                        if (Utility.IsNumeric(sRoleName))
                        {
                            RoleInfo roleInfo = roleController.GetRole(
                                int.Parse(sRoleName), portalSettings.PortalId);

                            if (roleInfo != null)
                            {
                                roles.Add(roleInfo.RoleName);
                            }
                        }
                        else
                        {
                            roles.Add(sRoleName);
                        }
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.BROWSER)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.BROWSER)]))
                {
                    currentSettings.Browser = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.BROWSER)];

                    switch (currentSettings.Browser)
                    {
                        case "ckfinder":
                            foreach (string sRoleName in roles)
                            {
                                if (PortalSecurity.IsInRoles(sRoleName))
                                {
                                    currentSettings.BrowserMode = Browser.CKFinder;

                                    break;
                                }

                                currentSettings.BrowserMode = Browser.None;
                            }

                            break;
                        case "standard":
                            foreach (string sRoleName in roles)
                            {
                                if (PortalSecurity.IsInRoles(sRoleName))
                                {
                                    currentSettings.BrowserMode = Browser.StandardBrowser;

                                    break;
                                }

                                currentSettings.BrowserMode = Browser.None;
                            }

                            break;
                        case "none":
                            currentSettings.BrowserMode = Browser.None;
                            break;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.INJECTJS)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.INJECTJS)]))
                {
                    bool bResult;
                    if (bool.TryParse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.INJECTJS)], out bResult))
                    {
                        currentSettings.InjectSyntaxJs = bResult;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.WIDTH)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.WIDTH)]))
                {
                    currentSettings.Config.Width = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.WIDTH)];
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.HEIGHT)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.HEIGHT)]))
                {
                    currentSettings.Config.Height = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.HEIGHT)];
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.BLANKTEXT)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.BLANKTEXT)]))
                {
                    currentSettings.BlankText = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.BLANKTEXT)];
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.STYLES)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.STYLES)]))
                {
                    currentSettings.Config.StylesSet = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.STYLES)];
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.CSS)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.CSS)]))
                {
                    currentSettings.Config.ContentsCss = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.CSS)];
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES)]))
                {
                    currentSettings.Config.Templates_Files = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES)];
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE)]))
                {
                    currentSettings.CustomJsFile = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE)];
                }
            }

            if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.CONFIG)]))
            {
                currentSettings.Config.CustomConfig = settingsDictionary[string.Format("{0}{1}", key, SettingConstants.CONFIG)];
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE)]))
                {
                    currentSettings.FileListPageSize = int.Parse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE)]);
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE)]))
                {
                    currentSettings.FileListViewMode =
                        (FileListView)
                        Enum.Parse(typeof(FileListView), settingsDictionary[string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE)]);
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE)]))
                {
                    currentSettings.DefaultLinkMode =
                        (LinkMode)
                        Enum.Parse(typeof(LinkMode), settingsDictionary[string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE)]);
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR)]))
                {
                    bool bResult;
                    if (bool.TryParse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR)], out bResult))
                    {
                        currentSettings.UseAnchorSelector = bResult;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST)]))
                {
                    bool bResult;
                    if (bool.TryParse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST)], out bResult))
                    {
                        currentSettings.ShowPageLinksTabFirst = bResult;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD)]))
                {
                    bool bResult;
                    if (bool.TryParse(
                        settingsDictionary[string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD)], out bResult))
                    {
                        currentSettings.OverrideFileOnUpload = bResult;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.SUBDIRS)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.SUBDIRS)]))
                {
                    bool bResult;
                    if (bool.TryParse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.SUBDIRS)], out bResult))
                    {
                        currentSettings.SubDirs = bResult;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID)]))
                {
                    try
                    {
                        currentSettings.BrowserRootDirId =
                            int.Parse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID)]);
                    }
                    catch (Exception)
                    {
                        currentSettings.BrowserRootDirId = -1;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID)]))
                {
                    try
                    {
                        currentSettings.UploadDirId =
                            int.Parse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID)]);
                    }
                    catch (Exception)
                    {
                        currentSettings.UploadDirId = -1;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH)]))
                {
                    try
                    {
                        currentSettings.ResizeWidth =
                            int.Parse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH)]);
                    }
                    catch (Exception)
                    {
                        currentSettings.ResizeWidth = -1;
                    }
                }
            }

            if (settingsDictionary.ContainsKey(string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT)))
            {
                if (!string.IsNullOrEmpty(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT)]))
                {
                    try
                    {
                        currentSettings.ResizeHeight =
                            int.Parse(settingsDictionary[string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT)]);
                    }
                    catch (Exception)
                    {
                        currentSettings.ResizeHeight = -1;
                    }
                }
            }

            return currentSettings;
        }

        /// <summary>
        /// Loads the module settings.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="currentSettings">The current settings.</param>
        /// <param name="key">The module key.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="portalRoles">The portal roles.</param>
        /// <returns>
        /// Returns the filled Module Settings
        /// </returns>
        internal static EditorProviderSettings LoadModuleSettings(PortalSettings portalSettings, EditorProviderSettings currentSettings, string key, int moduleId, ArrayList portalRoles)
        {
            var hshModSet = new ModuleController().GetModuleSettings(moduleId);

            var roleController = new RoleController();

            var roles = new ArrayList();

            // Import all Editor config settings
            foreach (
                PropertyInfo info in
                    GetEditorConfigProperties()
                        .Where(
                            info =>
                            !string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, info.Name)])
                            || info.Name.Equals("CodeMirror") || info.Name.Equals("WordCount")))
            {
                switch (info.PropertyType.Name)
                {
                    case "String":
                        info.SetValue(currentSettings.Config, hshModSet[string.Format("{0}{1}", key, info.Name)], null);
                        break;
                    case "Int32":
                        info.SetValue(
                            currentSettings.Config,
                            int.Parse((string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "Decimal":
                        info.SetValue(
                            currentSettings.Config,
                            decimal.Parse((string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "Boolean":
                        info.SetValue(
                            currentSettings.Config,
                            bool.Parse((string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        info.SetValue(
                            currentSettings.Config,
                            (ToolBarLocation)
                            Enum.Parse(
                                typeof(ToolBarLocation), (string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "DefaultLinkType":
                        info.SetValue(
                            currentSettings.Config,
                            (LinkType)
                            Enum.Parse(typeof(LinkType), (string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "EnterMode":
                    case "ShiftEnterMode":
                        info.SetValue(
                            currentSettings.Config,
                            (EnterModus)
                            Enum.Parse(typeof(EnterModus), (string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "ContentsLangDirection":
                        info.SetValue(
                            currentSettings.Config,
                            (LanguageDirection)
                            Enum.Parse(
                                typeof(LanguageDirection), (string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "CodeMirror":
                        foreach (var codeMirrorInfo in
                            typeof(CodeMirror).GetProperties()
                                              .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                        {
                            switch (codeMirrorInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (hshModSet.ContainsKey(string.Format("{0}{1}", key, codeMirrorInfo.Name)))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            hshModSet[string.Format("{0}{1}", key, codeMirrorInfo.Name)],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (hshModSet.ContainsKey(string.Format("{0}{1}", key, codeMirrorInfo.Name)))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            bool.Parse(
                                                (string)hshModSet[string.Format("{0}{1}", key, codeMirrorInfo.Name)]),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                    case "WordCount":
                        foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                        {
                            switch (wordCountInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (hshModSet.ContainsKey(string.Format("{0}{1}", key, wordCountInfo.Name)))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            hshModSet[string.Format("{0}{1}", key, wordCountInfo.Name)],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (hshModSet.ContainsKey(string.Format("{0}{1}", key, wordCountInfo.Name)))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            bool.Parse(
                                                (string)hshModSet[string.Format("{0}{1}", key, wordCountInfo.Name)]),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                }
            }

            /////////////////

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SKIN)]))
            {
                currentSettings.Config.Skin = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SKIN)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME)]))
            {
                currentSettings.Config.CodeMirror.Theme = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME)];
            }

            List<ToolbarRoles> listToolbarRoles = (from RoleInfo objRole in portalRoles
                                                   where
                                                       !string.IsNullOrEmpty(
                                                           (string)
                                                           hshModSet[string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB)])
                                                   let sToolbar =
                                                       (string)
                                                       hshModSet[string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB)]
                                                   select
                                                       new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = sToolbar })
                .ToList();

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)]))
            {
                string sToolbar = (string)hshModSet[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)];

                listToolbarRoles.Add(new ToolbarRoles { RoleId = -1, Toolbar = sToolbar });
            }

            currentSettings.ToolBarRoles = listToolbarRoles;

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.ROLES)]))
            {
                string sRoles = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.ROLES)];

                currentSettings.BrowserRoles = sRoles;

                string[] rolesA = sRoles.Split(';');

                foreach (string sRoleName in rolesA)
                {
                    if (Utility.IsNumeric(sRoleName))
                    {
                        RoleInfo roleInfo = roleController.GetRole(
                            int.Parse(sRoleName), portalSettings.PortalId);

                        if (roleInfo != null)
                        {
                            roles.Add(roleInfo.RoleName);
                        }
                    }
                    else
                    {
                        roles.Add(sRoleName);
                    }
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BROWSER)]))
            {
                currentSettings.Browser = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BROWSER)];

                switch (currentSettings.Browser)
                {
                    case "ckfinder":
                        foreach (string sRoleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(sRoleName))
                            {
                                currentSettings.BrowserMode = Browser.CKFinder;

                                break;
                            }

                            currentSettings.BrowserMode = Browser.None;
                        }

                        break;
                    case "standard":
                        foreach (string sRoleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(sRoleName))
                            {
                                currentSettings.BrowserMode = Browser.StandardBrowser;

                                break;
                            }

                            currentSettings.BrowserMode = Browser.None;
                        }

                        break;
                    case "none":
                        currentSettings.BrowserMode = Browser.None;
                        break;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.INJECTJS)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.INJECTJS)], out bResult))
                {
                    currentSettings.InjectSyntaxJs = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.WIDTH)]))
            {
                currentSettings.Config.Width = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.WIDTH)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.HEIGHT)]))
            {
                currentSettings.Config.Height = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.HEIGHT)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BLANKTEXT)]))
            {
                currentSettings.BlankText = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BLANKTEXT)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.STYLES)]))
            {
                currentSettings.Config.StylesSet = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.STYLES)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CSS)]))
            {
                currentSettings.Config.ContentsCss = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CSS)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES)]))
            {
                currentSettings.Config.Templates_Files = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE)]))
            {
                currentSettings.CustomJsFile = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CONFIG)]))
            {
                currentSettings.Config.CustomConfig = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CONFIG)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE)]))
            {
                currentSettings.FileListPageSize = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE)]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE)]))
            {
                currentSettings.FileListViewMode =
                    (FileListView)
                    Enum.Parse(typeof(FileListView), (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE)]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE)]))
            {
                currentSettings.DefaultLinkMode =
                    (LinkMode)
                    Enum.Parse(typeof(LinkMode), (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE)]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR)], out bResult))
                {
                    currentSettings.UseAnchorSelector = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST)], out bResult))
                {
                    currentSettings.ShowPageLinksTabFirst = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD)], out bResult))
                {
                    currentSettings.OverrideFileOnUpload = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SUBDIRS)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SUBDIRS)], out bResult))
                {
                    currentSettings.SubDirs = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID)]))
            {
                try
                {
                    currentSettings.BrowserRootDirId = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID)]);
                }
                catch (Exception)
                {
                    currentSettings.BrowserRootDirId = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID)]))
            {
                try
                {
                    currentSettings.UploadDirId = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID)]);
                }
                catch (Exception)
                {
                    currentSettings.UploadDirId = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH)]))
            {
                try
                {
                    currentSettings.ResizeWidth = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH)]);
                }
                catch (Exception)
                {
                    currentSettings.ResizeWidth = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT)]))
            {
                try
                {
                    currentSettings.ResizeHeight = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT)]);
                }
                catch (Exception)
                {
                    currentSettings.ResizeHeight = -1;
                }
            }

            return currentSettings;
        }

        /// <summary>
        /// Gets the default settings.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="homeDirPath">The home folder path.</param>
        /// <param name="alternateSubFolder">The alternate Sub Folder.</param>
        /// <param name="portalRoles">The portal roles.</param>
        /// <returns>
        /// Returns the Default Provider Settings
        /// </returns>
        internal static EditorProviderSettings GetDefaultSettings(PortalSettings portalSettings, string homeDirPath, string alternateSubFolder, ArrayList portalRoles)
        {
            var roles = new ArrayList();

            var roleController = new RoleController();

            if (!string.IsNullOrEmpty(alternateSubFolder))
            {
                var alternatePath = Path.Combine(homeDirPath, alternateSubFolder);

                if (!Directory.Exists(alternatePath))
                {
                    Directory.CreateDirectory(alternatePath);
                }

                homeDirPath = alternatePath;
            }

            // Check if old Settings File Exists
            if (File.Exists(Path.Combine(homeDirPath, SettingConstants.XmlDefaultFileName)))
            {
                // Import Old SettingsBase Xml File
                ImportSettingBaseXml(homeDirPath);
            }

            if (!File.Exists(Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName)))
            {
                if (!File.Exists(Path.Combine(Globals.HostMapPath, SettingConstants.XmlDefaultFileName)))
                {
                    CreateDefaultSettingsFile();
                }
                else
                {
                    // Import Old SettingBase Xml File
                    ImportSettingBaseXml(Globals.HostMapPath, true);
                }

                File.Copy(Path.Combine(Globals.HostMapPath, SettingConstants.XmlDefaultFileName), Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName));
            }

            var serializer = new XmlSerializer(typeof(EditorProviderSettings));
            var reader =
                new StreamReader(
                    new FileStream(
                        Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName), FileMode.Open, FileAccess.Read, FileShare.Read));

            var settings = (EditorProviderSettings)serializer.Deserialize(reader);

            if (!string.IsNullOrEmpty(settings.EditorWidth))
            {
                settings.Config.Width = settings.EditorWidth;
            }

            if (!string.IsNullOrEmpty(settings.EditorHeight))
            {
                settings.Config.Height = settings.EditorHeight;
            }

            // Get Browser Roles
            if (!string.IsNullOrEmpty(settings.BrowserRoles))
            {
                var rolesString = settings.BrowserRoles;

                if (rolesString.Length >= 1 && rolesString.Contains(";"))
                {
                    string[] rolesA = rolesString.Split(';');

                    foreach (string sRoleName in rolesA)
                    {
                        if (Utility.IsNumeric(sRoleName))
                        {
                            RoleInfo roleInfo = roleController.GetRole(int.Parse(sRoleName), portalSettings.PortalId);

                            if (roleInfo != null)
                            {
                                roles.Add(roleInfo.RoleName);
                            }
                        }
                        else
                        {
                            roles.Add(sRoleName);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(settings.Browser))
            {
                switch (settings.Browser)
                {
                    case "ckfinder":
                        foreach (string sRoleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(sRoleName))
                            {
                                settings.BrowserMode = Browser.CKFinder;

                                break;
                            }

                            settings.BrowserMode = Browser.None;
                        }

                        break;
                    case "standard":
                        foreach (string sRoleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(sRoleName))
                            {
                                settings.BrowserMode = Browser.StandardBrowser;

                                break;
                            }

                            settings.BrowserMode = Browser.None;
                        }

                        break;
                    case "none":
                        settings.BrowserMode = Browser.None;
                        break;
                }
            }

            reader.Close();

            return settings;
        }

        /// <summary>
        /// Creates the default settings file.
        /// </summary>
        internal static void CreateDefaultSettingsFile()
        {
            var newSettings = new EditorProviderSettings();

            var serializer = new XmlSerializer(typeof(EditorProviderSettings));

            var textWriter =
                new StreamWriter(
                    new FileStream(
                        Path.Combine(Globals.HostMapPath, SettingConstants.XmlDefaultFileName),
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite));

            serializer.Serialize(textWriter, newSettings);

            textWriter.Close();
        }

        /// <summary>
        /// Gets the editor config properties.
        /// </summary>
        /// <returns>Returns the EditorConfig Properties</returns>
        internal static IEnumerable<PropertyInfo> GetEditorConfigProperties()
        {
            return
                typeof(EditorConfig).GetProperties()
                                    .Where(
                                        info =>
                                        !info.Name.Equals("Magicline_KeystrokeNext")
                                        && !info.Name.Equals("Magicline_KeystrokePrevious")
                                        && !info.Name.Equals("Plugins") && !info.Name.Equals("Codemirror_Theme")
                                        && !info.Name.Equals("Width") && !info.Name.Equals("Height")
                                        && !info.Name.Equals("StylesSet") && !info.Name.Equals("ContentsCss")
                                        && !info.Name.Equals("Templates_Files") && !info.Name.Equals("CustomConfig")
                                        && !info.Name.Equals("Skin") && !info.Name.Equals("Templates_Files")
                                        && !info.Name.Equals("Toolbar") && !info.Name.Equals("Language")
                                        && !info.Name.Equals("FileBrowserWindowWidth")
                                        && !info.Name.Equals("FileBrowserWindowHeight")
                                        && !info.Name.Equals("FileBrowserWindowWidth")
                                        && !info.Name.Equals("FileBrowserWindowHeight")
                                        && !info.Name.Equals("FileBrowserUploadUrl")
                                        && !info.Name.Equals("FileBrowserImageUploadUrl")
                                        && !info.Name.Equals("FilebrowserImageBrowseLinkUrl")
                                        && !info.Name.Equals("FileBrowserImageBrowseUrl")
                                        && !info.Name.Equals("FileBrowserFlashUploadUrl")
                                        && !info.Name.Equals("FileBrowserFlashBrowseUrl")
                                        && !info.Name.Equals("FileBrowserBrowseUrl"));
        }

        /// <summary>
        /// Imports the old SettingsBase Xml File
        /// </summary>
        /// <param name="homeDirPath">The home folder path.</param>
        /// <param name="isDefaultXmlFile">if set to <c>true</c> [is default XML file].</param>
        internal static void ImportSettingBaseXml(string homeDirPath, bool isDefaultXmlFile = false)
        {
            var oldXmlPath = Path.Combine(homeDirPath, SettingConstants.XmlDefaultFileName);
            var oldSerializer = new XmlSerializer(typeof(SettingBase));
            var reader = new XmlTextReader(new FileStream(oldXmlPath, FileMode.Open, FileAccess.Read, FileShare.Read));

            if (!oldSerializer.CanDeserialize(reader))
            {
                reader.Close();

                return;
            }

            var oldDefaultSettings = (SettingBase)oldSerializer.Deserialize(reader);

            reader.Close();

            // Fix for old skins
            if (oldDefaultSettings.sSkin.Equals("office2003")
                            || oldDefaultSettings.sSkin.Equals("BootstrapCK-Skin")
                            || oldDefaultSettings.sSkin.Equals("chris")
                            || oldDefaultSettings.sSkin.Equals("v2"))
            {
                oldDefaultSettings.sSkin = "moono";
            }

            // Migrate Settings
            var importedSettings = new EditorProviderSettings
            {
                FileListPageSize = oldDefaultSettings.FileListPageSize,
                FileListViewMode = oldDefaultSettings.FileListViewMode,
                UseAnchorSelector = oldDefaultSettings.UseAnchorSelector,
                ShowPageLinksTabFirst = oldDefaultSettings.ShowPageLinksTabFirst,
                SubDirs = oldDefaultSettings.bSubDirs,
                InjectSyntaxJs = oldDefaultSettings.injectSyntaxJs,
                BrowserRootDirId = oldDefaultSettings.BrowserRootDirId,
                UploadDirId = oldDefaultSettings.UploadDirId,
                ResizeHeight = oldDefaultSettings.iResizeHeight,
                ResizeWidth = oldDefaultSettings.iResizeWidth,
                ToolBarRoles = oldDefaultSettings.listToolbRoles,
                BlankText = oldDefaultSettings.sBlankText,
                BrowserRoles = oldDefaultSettings.sBrowserRoles,
                Browser = oldDefaultSettings.sBrowser,
                Config =
                {
                    CustomConfig = oldDefaultSettings.sConfig,
                    ContentsCss = oldDefaultSettings.sCss,
                    Skin = oldDefaultSettings.sSkin,
                    StylesSet = oldDefaultSettings.sStyles,
                    Templates_Files = oldDefaultSettings.sTemplates,
                    Height = oldDefaultSettings.BrowserHeight,
                    Width = oldDefaultSettings.BrowserWidth,
                    AutoParagraph = true,
                    AutoUpdateElement = true,
                    BasicEntities = true,
                    BrowserContextMenuOnCtrl = true,
                    ColorButton_EnableMore = true,
                    DisableNativeSpellChecker = true,
                    DisableNativeTableHandles = true,
                    EnableTabKeyTools = true,
                    Entities = true,
                    Entities_Greek = true,
                    Entities_Latin = true,
                    FillEmptyBlocks = true,
                    IgnoreEmptyParagraph = true,
                    Image_RemoveLinkByEmptyURL = true,
                    PasteFromWordRemoveFontStyles = true,
                    PasteFromWordRemoveStyles = true,
                    Resize_Enabled = true,
                    StartupShowBorders = true,
                    ToolbarGroupCycling = true,
                    ToolbarStartupExpanded = true,
                    UseComputedState = true,
                    AutoGrow_BottomSpace = 0,
                    AutoGrow_MaxHeight = 0,
                    AutoGrow_MinHeight = 200,
                    BaseFloatZIndex = 10000,
                    Dialog_MagnetDistance = 20,
                    IndentOffset = 40,
                    Menu_SubMenuDelay = 400,
                    Resize_MaxHeight = 600,
                    Resize_MaxWidth = 3000,
                    Resize_MinHeight = 250,
                    Resize_MinWidth = 750,
                    Scayt_MaxSuggestions = 5,
                    Smiley_columns = 8,
                    SourceAreaTabSize = 20,
                    TabIndex = 0,
                    TabSpaces = 0,
                    UndoStackSize = 20
                }
            };

            // Delete Old File
            File.Delete(oldXmlPath);

            // Save new xml file
            var newSerializer = new XmlSerializer(typeof(EditorProviderSettings));

            using (
                var textWriter =
                    new StreamWriter(
                        new FileStream(
                            Path.Combine(
                                homeDirPath, isDefaultXmlFile ? SettingConstants.XmlDefaultFileName : SettingConstants.XmlSettingsFileName),
                            FileMode.OpenOrCreate,
                            FileAccess.ReadWrite,
                            FileShare.ReadWrite)))
            {
                newSerializer.Serialize(textWriter, importedSettings);

                textWriter.Close();
            }
        }

        #endregion
    }
}