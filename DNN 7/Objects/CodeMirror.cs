/* CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
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
    /// CodeMirror Plugin Options
    /// </summary>
    public class CodeMirror
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeMirror" /> class.
        /// </summary>
        public CodeMirror()
        {
            this.AutoCloseBrackets = true;
            this.AutoCloseTags = false;
            this.AutoFormatOnStart = false;
            this.AutoFormatOnUncomment = true;
            this.ContinueComments = true;
            this.EnableCodeFolding = true;
            this.EnableCodeFormatting = true;
            this.EnableSearchTools = true;
            this.HighlightMatches = true;
            this.IndentWithTabs = false;
            this.LineNumbers = true;
            this.LineWrapping = true;
            this.Mode = "htmlmixed";
            this.MatchBrackets = true;
            this.MatchTags = true;
            this.ShowAutoCompleteButton = true;
            this.ShowCommentButton = true;
            this.ShowFormatButton = true;
            this.ShowSearchButton = true;
            this.ShowTrailingSpace = true;
            this.ShowUncommentButton = true;
            this.StyleActiveLine = true;
            this.Theme = "default";
            this.useBeautifyOnStart = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic close brackets].
        /// </summary>
        /// <value>
        /// <c>true</c> if [automatic close brackets]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoCloseBrackets")]
        [Description("Whether or not you want Brackets to automatically close themselves")]
        public bool AutoCloseBrackets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto close tags].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto close tags]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoCloseTags")]
        [Description("Whether or not you want tags to automatically close themselves")]
        public bool AutoCloseTags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto format on start].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto format on start]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoFormatOnStart")]
        [Description("Whether or not to automatically format code should be done every time the source view is opened")]
        public bool AutoFormatOnStart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto format on uncomment].
        /// </summary>
        /// <value>
        /// <c>true</c> if [auto format on uncomment]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("autoFormatOnUncomment")]
        [Description("Whether or not to automatically format code which has just been uncommented")]
        public bool AutoFormatOnUncomment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [continue comments].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [continue comments]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("continueComments")]
        [Description("Whether or not to continue a comment when you press Enter inside a comment block")]
        public bool ContinueComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable code folding].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable code folding]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("enableCodeFolding")]
        [Description("Whether or not you wish to enable code folding (requires 'lineNumbers' to be set to 'true')")]
        public bool EnableCodeFolding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable code formatting].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable code formatting]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("enableCodeFormatting")]
        [Description("Whether or not to enable code formatting")]
        public bool EnableCodeFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable search tools].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable search tools]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("enableSearchTools")]
        [Description(
            "Whether or not to enable search tools, CTRL+F (Find), CTRL+SHIFT+F (Replace), CTRL+SHIFT+R (Replace All), CTRL+G (Find Next), CTRL+SHIFT+G (Find Previous)")]
        public bool EnableSearchTools { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [highlight matches].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [highlight matches]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("highlightMatches")]
        [Description("Whether or not to highlight all matches of current word/selection")]
        public bool HighlightMatches { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [indent with tabs].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [indent with tabs]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("indentWithTabs")]
        [Description("Whether, when indenting, the first N*tabSize spaces should be replaced by N tabs")]
        public bool IndentWithTabs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [line numbers].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [line numbers]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("lineNumbers")]
        [Description("Whether or not you want to show line numbers")]
        public bool LineNumbers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [line wrapping].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [line wrapping]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("lineWrapping")]
        [Description("Whether or not you want to use line wrapping")]
        public bool LineWrapping { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        [XmlAttribute("mode")]
        [Description("Defines the language specific mode")]
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [match brackets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [match brackets]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("matchBrackets")]
        [Description("Whether or not you want to highlight matching braces")]
        public bool MatchBrackets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [match tags].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [match tags]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("matchTags")]
        [Description("Whether or not you want to highlight matching tags")]
        public bool MatchTags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show automatic complete button].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show automatic complete button]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showAutoCompleteButton")]
        [Description("Whether or not to show the auto complete button on the toolbar")]
        public bool ShowAutoCompleteButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show comment button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show comment button]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showCommentButton")]
        [Description("Whether or not to show the comment button on the toolbar")]
        public bool ShowCommentButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show format button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show format button]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showFormatButton")]
        [Description("Whether or not to show the format button on the toolbar")]
        public bool ShowFormatButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show search button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show search button]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showSearchButton")]
        [Description("Whether or not to show the search button on the toolbar")]
        public bool ShowSearchButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show trailing space].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show trailing space]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showTrailingSpace")]
        [Description(
            "Whether or not to add the CSS class cm-trailingspace to stretches of whitespace at the end of lines.")]
        public bool ShowTrailingSpace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show uncomment button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show uncomment button]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("showUncommentButton")]
        [Description("Whether or not to show the uncomment button on the toolbar")]
        public bool ShowUncommentButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [style active line].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [style active line]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("highlightActiveLine")]
        [Description("Whether or not to highlight the currently active line")]
        public bool StyleActiveLine { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        [XmlAttribute("theme")]
        [Description("Set this to the theme you wish to use (codemirror themes)")]
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use beautify].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use beautify]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("useBeautifyOnStart")]
        [Description("Whether or not to use Beautify for auto formatting")]
        public bool useBeautifyOnStart { get; set; }
    }
}