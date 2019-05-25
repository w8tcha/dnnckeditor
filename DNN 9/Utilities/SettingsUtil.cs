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

namespace WatchersNET.CKEditor.Utilities
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
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
        /// <param name="editorHostSettings">The editor host settings.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// Returns if Portal or Page Settings Exists
        /// </returns>
        internal static bool CheckExistsPortalOrPageSettings(List<EditorHostSetting> editorHostSettings, string key)
        {
            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.SKIN}")))
            {
                return !string.IsNullOrEmpty(
                           editorHostSettings.FirstOrDefault(
                               setting => setting.Name.Equals($"{key}{SettingConstants.SKIN}"))?.Value);
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
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);

            if (module == null)
            {
                return false;
            }

            var hshModSet = module.ModuleSettings;
            return hshModSet != null && hshModSet.Keys.Cast<string>().Any(key => key.StartsWith(moduleKey));
        }

        /// <summary>
        /// Checks the exists of the module instance settings.
        /// </summary>
        /// <param name="moduleKey">The module key.</param>
        /// <param name="moduleId">The module id.</param>
        /// <returns>Returns if The Module Settings Exists or not.</returns>
        internal static bool CheckExistsModuleInstanceSettings(string moduleKey, int moduleId)
        {
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);

            if (module == null)
            {
                return false;
            }

            var hshModSet = module.ModuleSettings;

            return hshModSet != null && !string.IsNullOrEmpty((string)hshModSet[$"{moduleKey}skin"]);
        }

        /// <summary>
        /// Loads the portal or page settings.
        /// </summary>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <param name="currentSettings">The current settings.</param>
        /// <param name="editorHostSettings">The editor host settings.</param>
        /// <param name="key">The Portal or Page key.</param>
        /// <param name="portalRoles">The Portal Roles</param>
        /// <returns>
        /// Returns the Filled Settings
        /// </returns>
        internal static EditorProviderSettings LoadPortalOrPageSettings(
            PortalSettings portalSettings,
            EditorProviderSettings currentSettings,
            List<EditorHostSetting> editorHostSettings,
            string key,
            IList<RoleInfo> portalRoles)
        {
            var roleController = new RoleController();

            var roles = new ArrayList();

            // Import all Editor config settings
            foreach (var info in GetEditorConfigProperties())
            {
                var settingValue = string.Empty;
                if (!info.Name.Equals("CodeMirror") && !info.Name.Equals("WordCount") && !info.Name.Equals("AutoSave"))
                {
                    var settingName = $"{key}{info.Name}";
                    if (!editorHostSettings.Any(s => s.Name.Equals(settingName)))
                    {
                        continue;
                    }

                    settingValue = editorHostSettings.FirstOrDefault(setting => setting.Name.Equals(settingName)).Value;

                    if (string.IsNullOrEmpty(settingValue))
                    {
                        continue;
                    }
                }

                switch (info.PropertyType.Name)
                {
                    case "String":
                        info.SetValue(currentSettings.Config, settingValue, null);
                        break;
                    case "Int32":
                        info.SetValue(currentSettings.Config, int.Parse(settingValue), null);
                        break;
                    case "Decimal":
                        info.SetValue(currentSettings.Config, decimal.Parse(settingValue), null);
                        break;
                    case "Boolean":
                        info.SetValue(currentSettings.Config, bool.Parse(settingValue), null);
                        break;
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        info.SetValue(
                            currentSettings.Config,
                            (ToolBarLocation)Enum.Parse(typeof(ToolBarLocation), settingValue),
                            null);
                        break;
                    case "DefaultLinkType":
                        info.SetValue(
                            currentSettings.Config,
                            (LinkType)Enum.Parse(typeof(LinkType), settingValue),
                            null);
                        break;
                    case "EnterMode":
                    case "ShiftEnterMode":
                        info.SetValue(
                            currentSettings.Config,
                            (EnterModus)Enum.Parse(typeof(EnterModus), settingValue),
                            null);
                        break;
                    case "ContentsLangDirection":
                        info.SetValue(
                            currentSettings.Config,
                            (LanguageDirection)Enum.Parse(typeof(LanguageDirection), settingValue),
                            null);
                        break;
                    case "CodeMirror":
                        foreach (var codeMirrorInfo in typeof(CodeMirror).GetProperties()
                            .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                        {
                            var settingName = $"{key}{codeMirrorInfo.Name}";
                            if (!editorHostSettings.Any(s => s.Name.Equals(settingName)))
                            {
                                continue;
                            }

                            settingValue = editorHostSettings
                                .FirstOrDefault(setting => setting.Name.Equals(settingName)).Value;

                            if (string.IsNullOrEmpty(settingValue))
                            {
                                continue;
                            }

                            switch (codeMirrorInfo.PropertyType.Name)
                            {
                                case "String":
                                    codeMirrorInfo.SetValue(currentSettings.Config.CodeMirror, settingValue, null);
                                    break;
                                case "Boolean":
                                    codeMirrorInfo.SetValue(
                                        currentSettings.Config.CodeMirror,
                                        bool.Parse(settingValue),
                                        null);
                                    break;
                            }
                        }

                        break;
                    case "WordCount":
                        foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                        {
                            var settingName = $"{key}{wordCountInfo.Name}";
                            if (!editorHostSettings.Any(s => s.Name.Equals(settingName)))
                            {
                                continue;
                            }

                            settingValue = editorHostSettings
                                .FirstOrDefault(setting => setting.Name.Equals(settingName)).Value;

                            if (string.IsNullOrEmpty(settingValue))
                            {
                                continue;
                            }

                            switch (wordCountInfo.PropertyType.Name)
                            {
                                case "String":
                                    wordCountInfo.SetValue(currentSettings.Config.WordCount, settingValue, null);
                                    break;
                                case "Boolean":
                                    wordCountInfo.SetValue(
                                        currentSettings.Config.WordCount,
                                        bool.Parse(settingValue),
                                        null);
                                    break;
                            }
                        }

                        break;
                    case "AutoSave":
                        foreach (var autoSaveInfo in typeof(AutoSave).GetProperties())
                        {
                            var settingName = $"{key}{autoSaveInfo.Name}";
                            if (!editorHostSettings.Any(s => s.Name.Equals(settingName)))
                            {
                                continue;
                            }

                            settingValue = editorHostSettings
                                .FirstOrDefault(setting => setting.Name.Equals(settingName)).Value;

                            if (string.IsNullOrEmpty(settingValue))
                            {
                                continue;
                            }

                            switch (autoSaveInfo.PropertyType.Name)
                            {
                                case "Int32":
                                    autoSaveInfo.SetValue(
                                        currentSettings.Config.AutoSave,
                                        int.Parse(settingValue),
                                        null);
                                    break;
                                case "String":
                                    autoSaveInfo.SetValue(currentSettings.Config.AutoSave, settingValue, null);
                                    break;
                                case "Boolean":
                                    autoSaveInfo.SetValue(
                                        currentSettings.Config.AutoSave,
                                        bool.Parse(settingValue),
                                        null);
                                    break;
                            }
                        }

                        break;
                }
            }

            /////////////////
            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.SKIN}")))
            {
                var settingValue = editorHostSettings
                    .FirstOrDefault(s => s.Name.Equals($"{key}{SettingConstants.SKIN}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.Skin = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.CODEMIRRORTHEME}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.CODEMIRRORTHEME}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.CodeMirror.Theme = settingValue;
                }
            }

            var listToolbarRoles = (from RoleInfo objRole in portalRoles
                                    where editorHostSettings.Any(
                                        setting => setting.Name.Equals(
                                            string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB)))
                                    where !string.IsNullOrEmpty(
                                              editorHostSettings.FirstOrDefault(
                                                  s => s.Name.Equals(
                                                      string.Format(
                                                          "{0}{2}#{1}",
                                                          key,
                                                          objRole.RoleID,
                                                          SettingConstants.TOOLB)))?.Value)
                                    let sToolbar =
                                        editorHostSettings.FirstOrDefault(
                                            s => s.Name.Equals(
                                                string.Format(
                                                    "{0}{2}#{1}",
                                                    key,
                                                    objRole.RoleID,
                                                    SettingConstants.TOOLB)))?.Value
                                    select new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = sToolbar }).ToList();

            if (editorHostSettings.Any(
                setting => setting.Name.Equals(string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB))))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals(string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    listToolbarRoles.Add(new ToolbarRoles { RoleId = -1, Toolbar = settingValue });
                }
            }

            currentSettings.ToolBarRoles = listToolbarRoles;

            var listUploadSizeRoles = (from RoleInfo objRole in portalRoles
                                       where editorHostSettings.Any(
                                           setting => setting.Name.Equals(
                                               string.Format(
                                                   "{0}{2}#{1}",
                                                   key,
                                                   objRole.RoleID,
                                                   SettingConstants.UPLOADFILELIMITS)))
                                       where !string.IsNullOrEmpty(
                                                 editorHostSettings.FirstOrDefault(
                                                     s => s.Name.Equals(
                                                         string.Format(
                                                             "{0}{2}#{1}",
                                                             key,
                                                             objRole.RoleID,
                                                             SettingConstants.UPLOADFILELIMITS)))?.Value)
                                       let uploadFileLimit =
                                           editorHostSettings.FirstOrDefault(
                                               s => s.Name.Equals(
                                                   string.Format(
                                                       "{0}{2}#{1}",
                                                       key,
                                                       objRole.RoleID,
                                                       SettingConstants.UPLOADFILELIMITS)))?.Value
                                       select new UploadSizeRoles
                                                  {
                                                      RoleId = objRole.RoleID,
                                                      UploadFileLimit = Convert.ToInt32(uploadFileLimit)
                                                  }).ToList();

            if (editorHostSettings.Any(
                setting => setting.Name.Equals(
                    string.Format("{0}{2}#{1}", key, "-1", SettingConstants.UPLOADFILELIMITS))))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{2}#{1}", key, "-1", SettingConstants.UPLOADFILELIMITS)))
                    .Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    listUploadSizeRoles.Add(
                        new UploadSizeRoles { RoleId = -1, UploadFileLimit = Convert.ToInt32(settingValue) });
                }
            }

            currentSettings.UploadSizeRoles = listUploadSizeRoles;

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.ROLES}")))
            {
                var settingValue = editorHostSettings
                    .FirstOrDefault(s => s.Name.Equals($"{key}{SettingConstants.ROLES}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    var browserRoles = settingValue;

                    currentSettings.BrowserRoles = browserRoles;

                    var rolesA = browserRoles.Split(';');

                    foreach (var roleName in rolesA)
                    {
                        if (Utility.IsNumeric(roleName))
                        {
                            var roleInfo = roleController.GetRoleById(portalSettings.PortalId, int.Parse(roleName));

                            if (roleInfo != null)
                            {
                                roles.Add(roleInfo.RoleName);
                            }
                        }
                        else
                        {
                            roles.Add(roleName);
                        }
                    }
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.BROWSER}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.BROWSER}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Browser = settingValue;

                    switch (currentSettings.Browser)
                    {
                        case "ckfinder":
                            foreach (string roleName in roles)
                            {
                                if (PortalSecurity.IsInRoles(roleName))
                                {
                                    currentSettings.BrowserMode = Browser.CKFinder;

                                    break;
                                }

                                currentSettings.BrowserMode = Browser.None;
                            }

                            break;
                        case "standard":
                            foreach (string roleName in roles)
                            {
                                if (PortalSecurity.IsInRoles(roleName))
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

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.INJECTJS}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.INJECTJS}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.InjectSyntaxJs = bResult;
                    }
                }
            }

            if (editorHostSettings.Any(
                setting => setting.Name.Equals($"{key}{SettingConstants.ALLOWEDIMAGEEXTENSIONS}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.ALLOWEDIMAGEEXTENSIONS}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.AllowedImageExtensions = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.WIDTH}")))
            {
                var settingValue = editorHostSettings
                    .FirstOrDefault(s => s.Name.Equals($"{key}{SettingConstants.WIDTH}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.Width = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.HEIGHT}")))
            {
                var settingValue = editorHostSettings
                    .FirstOrDefault(s => s.Name.Equals($"{key}{SettingConstants.HEIGHT}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.Height = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.BLANKTEXT}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.BLANKTEXT}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.BlankText = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.CSS}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(s => s.Name.Equals($"{key}{SettingConstants.CSS}"))
                    .Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.ContentsCss = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.TEMPLATEFILES}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.TEMPLATEFILES}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.Templates_Files = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.CUSTOMJSFILE}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.CUSTOMJSFILE}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.CustomJsFile = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.CONFIG}")))
            {
                var settingValue = editorHostSettings
                    .FirstOrDefault(s => s.Name.Equals($"{key}{SettingConstants.CONFIG}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.CustomConfig = settingValue;
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.FILELISTPAGESIZE}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.FILELISTPAGESIZE}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.FileListPageSize = int.Parse(settingValue);
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.FILELISTVIEWMODE}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.FILELISTVIEWMODE}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.FileListViewMode = (FileListView)Enum.Parse(typeof(FileListView), settingValue);
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.DEFAULTLINKMODE}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.DEFAULTLINKMODE}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.DefaultLinkMode = (LinkMode)Enum.Parse(typeof(LinkMode), settingValue);
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.USEANCHORSELECTOR}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.USEANCHORSELECTOR}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.UseAnchorSelector = bResult;
                    }
                }
            }

            if (editorHostSettings.Any(
                setting => setting.Name.Equals($"{key}{SettingConstants.SHOWPAGELINKSTABFIRST}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.SHOWPAGELINKSTABFIRST}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.ShowPageLinksTabFirst = bResult;
                    }
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.OVERRIDEFILEONUPLOAD}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.OVERRIDEFILEONUPLOAD}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.OverrideFileOnUpload = bResult;
                    }
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.SUBDIRS}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.SUBDIRS}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.SubDirs = bResult;
                    }
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.BROWSERROOTDIRID}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.BROWSERROOTDIRID}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    try
                    {
                        currentSettings.BrowserRootDirId = int.Parse(settingValue);
                    }
                    catch (Exception)
                    {
                        currentSettings.BrowserRootDirId = -1;
                    }
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.UPLOADDIRID}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.UPLOADDIRID}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    try
                    {
                        currentSettings.UploadDirId = int.Parse(settingValue);
                    }
                    catch (Exception)
                    {
                        currentSettings.UploadDirId = -1;
                    }
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.RESIZEWIDTH}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.RESIZEWIDTH}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    try
                    {
                        currentSettings.ResizeWidth = int.Parse(settingValue);
                    }
                    catch (Exception)
                    {
                        currentSettings.ResizeWidth = -1;
                    }
                }
            }

            if (editorHostSettings.Any(setting => setting.Name.Equals($"{key}{SettingConstants.RESIZEHEIGHT}")))
            {
                var settingValue = editorHostSettings.FirstOrDefault(
                    s => s.Name.Equals($"{key}{SettingConstants.RESIZEHEIGHT}")).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    try
                    {
                        currentSettings.ResizeHeight = int.Parse(settingValue);
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
        internal static EditorProviderSettings LoadModuleSettings(
            PortalSettings portalSettings,
            EditorProviderSettings currentSettings,
            string key,
            int moduleId,
            IList<RoleInfo> portalRoles)
        {
            Hashtable hshModSet = null;
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
            if (module != null)
            {
                hshModSet = module.ModuleSettings;
            }

            hshModSet = hshModSet ?? new Hashtable();

            var roleController = new RoleController();

            var roles = new ArrayList();

            // Import all Editor config settings
            foreach (var info in GetEditorConfigProperties().Where(
                info => !string.IsNullOrEmpty((string)hshModSet[$"{key}{info.Name}"])
                /*|| info.Name.Equals("CodeMirror") || info.Name.Equals("WordCount")*/))
            {
                switch (info.PropertyType.Name)
                {
                    case "String":
                        info.SetValue(currentSettings.Config, hshModSet[$"{key}{info.Name}"], null);
                        break;
                    case "Int32":
                        info.SetValue(currentSettings.Config, int.Parse((string)hshModSet[$"{key}{info.Name}"]), null);
                        break;
                    case "Decimal":
                        info.SetValue(
                            currentSettings.Config,
                            decimal.Parse((string)hshModSet[$"{key}{info.Name}"]),
                            null);
                        break;
                    case "Boolean":
                        info.SetValue(currentSettings.Config, bool.Parse((string)hshModSet[$"{key}{info.Name}"]), null);
                        break;
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        info.SetValue(
                            currentSettings.Config,
                            (ToolBarLocation)Enum.Parse(
                                typeof(ToolBarLocation),
                                (string)hshModSet[$"{key}{info.Name}"]),
                            null);
                        break;
                    case "DefaultLinkType":
                        info.SetValue(
                            currentSettings.Config,
                            (LinkType)Enum.Parse(typeof(LinkType), (string)hshModSet[$"{key}{info.Name}"]),
                            null);
                        break;
                    case "EnterMode":
                    case "ShiftEnterMode":
                        info.SetValue(
                            currentSettings.Config,
                            (EnterModus)Enum.Parse(typeof(EnterModus), (string)hshModSet[$"{key}{info.Name}"]),
                            null);
                        break;
                    case "ContentsLangDirection":
                        info.SetValue(
                            currentSettings.Config,
                            (LanguageDirection)Enum.Parse(
                                typeof(LanguageDirection),
                                (string)hshModSet[$"{key}{info.Name}"]),
                            null);
                        break;
                    case "CodeMirror":
                        foreach (var codeMirrorInfo in typeof(CodeMirror).GetProperties()
                            .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                        {
                            switch (codeMirrorInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (hshModSet.ContainsKey($"{key}{codeMirrorInfo.Name}"))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            hshModSet[$"{key}{codeMirrorInfo.Name}"],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (hshModSet.ContainsKey($"{key}{codeMirrorInfo.Name}"))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            bool.Parse((string)hshModSet[$"{key}{codeMirrorInfo.Name}"]),
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
                                    if (hshModSet.ContainsKey($"{key}{wordCountInfo.Name}"))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            hshModSet[$"{key}{wordCountInfo.Name}"],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (hshModSet.ContainsKey($"{key}{wordCountInfo.Name}"))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            bool.Parse((string)hshModSet[$"{key}{wordCountInfo.Name}"]),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                    case "AutoSave":
                        foreach (var wordCountInfo in typeof(AutoSave).GetProperties())
                        {
                            switch (wordCountInfo.PropertyType.Name)
                            {
                                case "Int32":
                                case "String":
                                    if (hshModSet.ContainsKey($"{key}{wordCountInfo.Name}"))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.AutoSave,
                                            hshModSet[$"{key}{wordCountInfo.Name}"],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (hshModSet.ContainsKey($"{key}{wordCountInfo.Name}"))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.AutoSave,
                                            bool.Parse((string)hshModSet[$"{key}{wordCountInfo.Name}"]),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                }
            }

            /////////////////
            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.SKIN}"]))
            {
                currentSettings.Config.Skin = (string)hshModSet[$"{key}{SettingConstants.SKIN}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.CODEMIRRORTHEME}"]))
            {
                currentSettings.Config.CodeMirror.Theme = (string)hshModSet[$"{key}{SettingConstants.CODEMIRRORTHEME}"];
            }

            var listToolbarRoles = (from RoleInfo objRole in portalRoles
                                    where !string.IsNullOrEmpty(
                                              (string)hshModSet[string.Format(
                                                  "{0}{2}#{1}",
                                                  key,
                                                  objRole.RoleID,
                                                  SettingConstants.TOOLB)])
                                    let sToolbar =
                                        (string)hshModSet[string.Format(
                                            "{0}{2}#{1}",
                                            key,
                                            objRole.RoleID,
                                            SettingConstants.TOOLB)]
                                    select new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = sToolbar }).ToList();

            if (!string.IsNullOrEmpty(
                    (string)hshModSet[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)]))
            {
                var sToolbar = (string)hshModSet[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)];

                listToolbarRoles.Add(new ToolbarRoles { RoleId = -1, Toolbar = sToolbar });
            }

            currentSettings.ToolBarRoles = listToolbarRoles;

            var listUploadSizeRoles = (from RoleInfo objRole in portalRoles
                                       where !string.IsNullOrEmpty(
                                                 (string)hshModSet[string.Format(
                                                     "{0}{2}#{1}",
                                                     key,
                                                     objRole.RoleID,
                                                     SettingConstants.UPLOADFILELIMITS)])
                                       let uploadFileLimit =
                                           (string)hshModSet[string.Format(
                                               "{0}{2}#{1}",
                                               key,
                                               objRole.RoleID,
                                               SettingConstants.UPLOADFILELIMITS)]
                                       select new UploadSizeRoles
                                                  {
                                                      RoleId = objRole.RoleID,
                                                      UploadFileLimit = Convert.ToInt32(uploadFileLimit)
                                                  }).ToList();

            if (!string.IsNullOrEmpty(
                    (string)hshModSet[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.UPLOADFILELIMITS)]))
            {
                listUploadSizeRoles.Add(
                    new UploadSizeRoles
                        {
                            RoleId = -1,
                            UploadFileLimit = Convert.ToInt32(
                                hshModSet[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.UPLOADFILELIMITS)])
                        });
            }

            currentSettings.UploadSizeRoles = listUploadSizeRoles;

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.ROLES}"]))
            {
                var sRoles = (string)hshModSet[$"{key}{SettingConstants.ROLES}"];

                currentSettings.BrowserRoles = sRoles;

                var rolesA = sRoles.Split(';');

                foreach (var sRoleName in rolesA)
                {
                    if (Utility.IsNumeric(sRoleName))
                    {
                        var roleInfo = roleController.GetRoleById(portalSettings.PortalId, int.Parse(sRoleName));

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

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.BROWSER}"]))
            {
                currentSettings.Browser = (string)hshModSet[$"{key}{SettingConstants.BROWSER}"];

                switch (currentSettings.Browser)
                {
                    case "ckfinder":
                        foreach (string roleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(roleName))
                            {
                                currentSettings.BrowserMode = Browser.CKFinder;

                                break;
                            }

                            currentSettings.BrowserMode = Browser.None;
                        }

                        break;
                    case "standard":
                        foreach (string roleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(roleName))
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

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.INJECTJS}"]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[$"{key}{SettingConstants.INJECTJS}"], out bResult))
                {
                    currentSettings.InjectSyntaxJs = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.ALLOWEDIMAGEEXTENSIONS}"]))
            {
                currentSettings.AllowedImageExtensions = (string)hshModSet[
                    $"{key}{SettingConstants.ALLOWEDIMAGEEXTENSIONS}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.WIDTH}"]))
            {
                currentSettings.Config.Width = (string)hshModSet[$"{key}{SettingConstants.WIDTH}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.HEIGHT}"]))
            {
                currentSettings.Config.Height = (string)hshModSet[$"{key}{SettingConstants.HEIGHT}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.BLANKTEXT}"]))
            {
                currentSettings.BlankText = (string)hshModSet[$"{key}{SettingConstants.BLANKTEXT}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.CSS}"]))
            {
                currentSettings.Config.ContentsCss = (string)hshModSet[$"{key}{SettingConstants.CSS}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.TEMPLATEFILES}"]))
            {
                currentSettings.Config.Templates_Files = (string)hshModSet[$"{key}{SettingConstants.TEMPLATEFILES}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.CUSTOMJSFILE}"]))
            {
                currentSettings.CustomJsFile = (string)hshModSet[$"{key}{SettingConstants.CUSTOMJSFILE}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.CONFIG}"]))
            {
                currentSettings.Config.CustomConfig = (string)hshModSet[$"{key}{SettingConstants.CONFIG}"];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.FILELISTPAGESIZE}"]))
            {
                currentSettings.FileListPageSize = int.Parse(
                    (string)hshModSet[$"{key}{SettingConstants.FILELISTPAGESIZE}"]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.FILELISTVIEWMODE}"]))
            {
                currentSettings.FileListViewMode = (FileListView)Enum.Parse(
                    typeof(FileListView),
                    (string)hshModSet[$"{key}{SettingConstants.FILELISTVIEWMODE}"]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.DEFAULTLINKMODE}"]))
            {
                currentSettings.DefaultLinkMode = (LinkMode)Enum.Parse(
                    typeof(LinkMode),
                    (string)hshModSet[$"{key}{SettingConstants.DEFAULTLINKMODE}"]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.USEANCHORSELECTOR}"]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[$"{key}{SettingConstants.USEANCHORSELECTOR}"], out bResult))
                {
                    currentSettings.UseAnchorSelector = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.SHOWPAGELINKSTABFIRST}"]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[$"{key}{SettingConstants.SHOWPAGELINKSTABFIRST}"], out bResult))
                {
                    currentSettings.ShowPageLinksTabFirst = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.OVERRIDEFILEONUPLOAD}"]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[$"{key}{SettingConstants.OVERRIDEFILEONUPLOAD}"], out bResult))
                {
                    currentSettings.OverrideFileOnUpload = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.SUBDIRS}"]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[$"{key}{SettingConstants.SUBDIRS}"], out bResult))
                {
                    currentSettings.SubDirs = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.BROWSERROOTDIRID}"]))
            {
                try
                {
                    currentSettings.BrowserRootDirId = int.Parse(
                        (string)hshModSet[$"{key}{SettingConstants.BROWSERROOTDIRID}"]);
                }
                catch (Exception)
                {
                    currentSettings.BrowserRootDirId = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.UPLOADDIRID}"]))
            {
                try
                {
                    currentSettings.UploadDirId = int.Parse((string)hshModSet[$"{key}{SettingConstants.UPLOADDIRID}"]);
                }
                catch (Exception)
                {
                    currentSettings.UploadDirId = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.RESIZEWIDTH}"]))
            {
                try
                {
                    currentSettings.ResizeWidth = int.Parse((string)hshModSet[$"{key}{SettingConstants.RESIZEWIDTH}"]);
                }
                catch (Exception)
                {
                    currentSettings.ResizeWidth = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[$"{key}{SettingConstants.RESIZEHEIGHT}"]))
            {
                try
                {
                    currentSettings.ResizeHeight = int.Parse(
                        (string)hshModSet[$"{key}{SettingConstants.RESIZEHEIGHT}"]);
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
        internal static EditorProviderSettings GetDefaultSettings(
            PortalSettings portalSettings,
            string homeDirPath,
            string alternateSubFolder,
            IList<RoleInfo> portalRoles)
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

            if (!File.Exists(Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName)))
            {
                if (!File.Exists(Path.Combine(Globals.HostMapPath, SettingConstants.XmlDefaultFileName)))
                {
                    CreateDefaultSettingsFile();
                }

                File.Copy(
                    Path.Combine(Globals.HostMapPath, SettingConstants.XmlDefaultFileName),
                    Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName));
            }

            var serializer = new XmlSerializer(typeof(EditorProviderSettings));
            var reader = new StreamReader(
                new FileStream(
                    Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read));

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
                    var rolesA = rolesString.Split(';');

                    foreach (var roleName in rolesA)
                    {
                        if (Utility.IsNumeric(roleName))
                        {
                            var roleInfo = roleController.GetRoleById(portalSettings.PortalId, int.Parse(roleName));

                            if (roleInfo != null)
                            {
                                roles.Add(roleInfo.RoleName);
                            }
                        }
                        else
                        {
                            roles.Add(roleName);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(settings.Browser))
            {
                switch (settings.Browser)
                {
                    case "ckfinder":
                        foreach (string roleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(roleName))
                            {
                                settings.BrowserMode = Browser.CKFinder;

                                break;
                            }

                            settings.BrowserMode = Browser.None;
                        }

                        break;
                    case "standard":
                        foreach (string roleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(roleName))
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

            var textWriter = new StreamWriter(
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
            return typeof(EditorConfig).GetProperties().Where(
                info => !info.Name.Equals("Magicline_KeystrokeNext") && !info.Name.Equals("Magicline_KeystrokePrevious")
                                                                     && !info.Name.Equals("Plugins")
                                                                     && !info.Name.Equals("Codemirror_Theme")
                                                                     && !info.Name.Equals("Width")
                                                                     && !info.Name.Equals("Height")
                                                                     && !info.Name.Equals("ContentsCss")
                                                                     && !info.Name.Equals("Templates_Files")
                                                                     && !info.Name.Equals("CustomConfig")
                                                                     && !info.Name.Equals("Skin")
                                                                     && !info.Name.Equals("Templates_Files")
                                                                     && !info.Name.Equals("Toolbar")
                                                                     && !info.Name.Equals("Language")
                                                                     && !info.Name.Equals("FileBrowserWindowWidth")
                                                                     && !info.Name.Equals("FileBrowserWindowHeight")
                                                                     && !info.Name.Equals("FileBrowserWindowWidth")
                                                                     && !info.Name.Equals("FileBrowserWindowHeight")
                                                                     && !info.Name.Equals("FileBrowserUploadUrl")
                                                                     && !info.Name.Equals("FileBrowserImageUploadUrl")
                                                                     && !info.Name.Equals(
                                                                         "FilebrowserImageBrowseLinkUrl")
                                                                     && !info.Name.Equals("FileBrowserImageBrowseUrl")
                                                                     && !info.Name.Equals("FileBrowserFlashUploadUrl")
                                                                     && !info.Name.Equals("FileBrowserFlashBrowseUrl")
                                                                     && !info.Name.Equals("FileBrowserBrowseUrl"));
        }

        /// <summary>
        /// Gets the size of the current user upload.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns>Returns the MAX. upload file size for the current user</returns>
        internal static int GetCurrentUserUploadSize(
            EditorProviderSettings settings,
            PortalSettings portalSettings,
            HttpRequest httpRequest)
        {
            var uploadFileLimitForPortal = Convert.ToInt32(Utility.GetMaxUploadSize());

            var roleController = new RoleController();

            if (settings.UploadSizeRoles.Count <= 0)
            {
                return uploadFileLimitForPortal;
            }

            var listUserUploadFileSizes = new List<ToolbarSet>();

            foreach (var roleUploadSize in settings.UploadSizeRoles)
            {
                if (roleUploadSize.RoleId.Equals(-1) && !httpRequest.IsAuthenticated)
                {
                    return roleUploadSize.UploadFileLimit;
                }

                if (roleUploadSize.RoleId.Equals(-1))
                {
                    continue;
                }

                // Role
                var role = roleController.GetRoleById(portalSettings.PortalId, roleUploadSize.RoleId);

                if (role == null)
                {
                    continue;
                }

                if (!PortalSecurity.IsInRole(role.RoleName))
                {
                    continue;
                }

                var toolbar = new ToolbarSet(role.RoleName, roleUploadSize.UploadFileLimit);

                listUserUploadFileSizes.Add(toolbar);
            }

            if (listUserUploadFileSizes.Count <= 0)
            {
                return uploadFileLimitForPortal;
            }

            // Compare The User Toolbars if the User is more then One Role, and apply the Toolbar with the Highest Priority
            var highestPrio = listUserUploadFileSizes.Max(toolb => toolb.Priority);

            return listUserUploadFileSizes.Find(toolbarSel => toolbarSel.Priority.Equals(highestPrio)).Priority;
        }

        #endregion
    }
}