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

namespace WatchersNET.CKEditor.Objects
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// WordCount Plugin Config.
    /// </summary>
    public class WordCountConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WordCountConfig" /> class.
        /// </summary>
        public WordCountConfig()
        {
            this.ShowParagraphs = true;
            this.ShowWordCount = true;
            this.ShowCharCount = false;
            this.CountSpacesAsChars = false;
            this.CountHTML = false;
            this.MaxCharCount = -1;
            this.MaxWordCount = -1;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show paragraphs].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show paragraphs]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showParagraphs")]
        [Description("Whether or not you want to show the Paragraphs Count.")]
        public bool ShowParagraphs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show char count].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show char count]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showCharCount")]
        [Description("Whether or not you want to show the Word Count.")]
        public bool ShowCharCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show word count].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show word count]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showWordCount")]
        [Description("Whether or not you want to show the Char Count")]
        public bool ShowWordCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [count spaces as chars].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [count spaces as chars]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("countSpacesAsChars")]
        [Description("Whether or not you want to show the Char Count")]
        public bool CountSpacesAsChars { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [count HTML].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [count HTML]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("countHTML")]
        [Description("Whether or not to include Html chars in the Char Count")]
        public bool CountHTML { get; set; }

        /// <summary>
        /// Gets or sets the maximum character count.
        /// </summary>
        /// <value>
        /// The maximum character count.
        /// </value>
        [XmlAttribute("maxCharCount")]
        [Description("Maximum allowed Word Count, -1 is default for unlimited")]
        public int MaxCharCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum word count.
        /// </summary>
        /// <value>
        /// The maximum word count.
        /// </value>
        [XmlAttribute("maxWordCount")]
        [Description("Maximum allowed Char Count, -1 is default for unlimited")]
        public int MaxWordCount { get; set; }
    }
}