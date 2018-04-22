/* CKEditor Html Editor Provider for DNN
 * ========
 * https://github.com/w8tcha/dnnckeditor
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor.Objects
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// the Autosave Plugin Options
    /// </summary>
    public class AutoSave
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSave" /> class.
        /// </summary>
        public AutoSave()
        {
            this.Delay = 10;
            this.MessageType = "notification";
            this.SaveDetectionSelectors = "a[href^='javascript:__doPostBack'][id*='Save'],a[id*='Cancel']";
            this.NotOlderThen = 1440;
            this.DiffType = "sideBySide";
            this.AutoLoad = false;
        }

        /// <summary>
        /// Gets or sets the delay.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        [XmlAttribute("delay")]
        [Description("Delay")]
        public int Delay { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        [XmlAttribute("messageType")]
        [Description("Notification Type - Setting to set the if you want to show the 'Auto Saved' message, and if yes you can show as Notification or as Message in the Status bar (Default is : 'notification')")]
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets the save detection selectors.
        /// </summary>
        /// <value>
        /// The save detection selectors.
        /// </value>
        [XmlAttribute("saveDetectionSelectors")]
        [Description("Setting to set the Save button to inform the plugin when the content is saved by the user and doesn't need to be stored temporary")]
        public string SaveDetectionSelectors { get; set; }

        /// <summary>
        /// Gets or sets the not older then.
        /// </summary>
        /// <value>
        /// The not older then.
        /// </value>
        [XmlAttribute("NotOlderThen")]
        [Description("The Default Minutes (Default is 1440 which is one day) after the auto saved content is ignored can be overidden")]
        public int NotOlderThen { get; set; }

        /// <summary>
        /// Gets or sets the type of the difference.
        /// </summary>
        /// <value>
        /// The type of the difference.
        /// </value>
        [XmlAttribute("diffType")]
        [Description("The Default Diff Type for the Compare Dialog, you can choose between 'sideBySide' or 'inline'. Default is 'sideBySide'")]
        public string DiffType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic load].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic load]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoLoad")]
        [Description("autoLoad when enabled it directly loads the saved content")]
        public bool AutoLoad { get; set; }
    }
}