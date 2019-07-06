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

namespace WatchersNET.CKEditor.Controls
{
    #region

    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;

    #endregion

    /// <summary>
    /// The html generic self closing.
    /// </summary>
    [ToolboxData("<{0}:Pager runat=server></{0}:Pager>")]
    public class Pager : WebControl, IPostBackEventHandler
    {
        #region Events

        /// <summary>
        /// Occurs when [page changed].
        /// </summary>
        public event EventHandler PageChanged;

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets Language Code
        /// </summary>
        public string LanguageCode
        {
            get => this.ViewState["LanguageCode"] != null ? (string)this.ViewState["LanguageCode"] : "en";

            set => this.ViewState["LanguageCode"] = value;
        }

        /// <summary>
        ///   Gets or sets Resource File
        /// </summary>
        public string RessourceFile
        {
            get => (string)this.ViewState["RessourceFile"];

            set => this.ViewState["RessourceFile"] = value;
        }

        /// <summary>
        ///   Gets or sets Page Count.
        /// </summary>
        public int PageCount
        {
            get => (int?)this.ViewState["PageCount"] ?? 0;

            set => this.ViewState["PageCount"] = value;
        }

        /// <summary>
        ///   Gets or sets Current Page Index.
        /// </summary>
        public int CurrentPageIndex
        {
            get => (int)(this.ViewState["CurrentPageIndex"] ?? 0);

            set => this.ViewState["CurrentPageIndex"] = value;
        }

        #endregion

        #region IPostBackEventHandler Members

        /// <summary>
        /// Enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            this.CurrentPageIndex = int.Parse(eventArgument.Replace("Page_", string.Empty));
            this.OnPageChanged(new EventArgs());
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="PageChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void OnPageChanged(EventArgs e)
        {
            this.PageChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            this.GeneratePagerLinks(writer);
        }

        /// <summary>
        /// Generates the pager links.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void GeneratePagerLinks(HtmlTextWriter writer)
        {
            var mainTable = new Table { CssClass = "PagerTable" };

            var mainTableRow = new TableRow();

            mainTable.Rows.Add(mainTableRow);

            var previousColumn = new TableCell { CssClass = "PagerFirstColumn" };

            var start = this.CurrentPageIndex - 2;
            var end = this.CurrentPageIndex + 3;

            if (start < 0)
            {
                start = 0;
            }

            if (end > this.PageCount)
            {
                end = this.PageCount;
            }

            var firstElement = new HtmlGenericControl("ul");

            firstElement.Attributes.Add("class", "FilesPager");

            // First Page
            if (start > 0)
            {
                var element = new HtmlGenericControl("li");

                element.Attributes.Add("class", "FirstPage");

                var firstPageLink = new HyperLink
                {
                    ID = "FirstPageLink",
                    ToolTip =
                        $"{Localization.GetString("GoTo.Text", this.RessourceFile, this.LanguageCode)}{Localization.GetString("FirstPage.Text", this.RessourceFile, this.LanguageCode)}",
                    Text = $"&laquo; {Localization.GetString("FirstPage.Text", this.RessourceFile, this.LanguageCode)}",
                    NavigateUrl =
                        this.Page.ClientScript.GetPostBackClientHyperlink(this, $"Page_{0}", false)
                };

                element.Controls.Add(firstPageLink);

                firstElement.Controls.Add(element);
            }

            // Previous Page
            if (this.CurrentPageIndex > start)
            {
                var prevElement = new HtmlGenericControl("li");

                prevElement.Attributes.Add("class", "PreviousPage");

                var lastPrevLink = new HyperLink
                                       {
                                           ID = "PreviousPageLink",
                                           ToolTip =
                                               $"{Localization.GetString("GoTo.Text", this.RessourceFile, this.LanguageCode)}{Localization.GetString("PreviousPage.Text", this.RessourceFile, this.LanguageCode)}",
                                           Text =
                                               $"&lt; {Localization.GetString("PreviousPage.Text", this.RessourceFile, this.LanguageCode)}",
                                           NavigateUrl = this.Page.ClientScript.GetPostBackClientHyperlink(
                                               this,
                                               $"Page_{this.CurrentPageIndex - 1}",
                                               false)
                                       };

                prevElement.Controls.Add(lastPrevLink);

                firstElement.Controls.Add(prevElement);
            }

            // Add Column
            previousColumn.Controls.Add(firstElement);
            mainTableRow.Cells.Add(previousColumn);

            // Second Page Numbers Column
            var secondElement = new HtmlGenericControl("ul");

            var pageNumbersColumn = new TableCell { CssClass = "PagerNumbersColumn", HorizontalAlign = HorizontalAlign.Center };

            secondElement.Attributes.Add("class", "FilesPager");

            for (var i = start; i < end; i++)
            {
                var element = new HtmlGenericControl("li");

                element.Attributes.Add("class", i.Equals(this.CurrentPageIndex) ? "ActivePage" : "NormalPage");

                var page = (i + 1).ToString();

                var pageLink = new HyperLink
                {
                    ID = $"NextPageLink{page}",
                    ToolTip = $"{Localization.GetString("GoTo.Text", this.RessourceFile, this.LanguageCode)}: {page}",
                    Text = page,
                    NavigateUrl =
                        this.Page.ClientScript.GetPostBackClientHyperlink(this, $"Page_{i}", false)
                };

                element.Controls.Add(pageLink);

                secondElement.Controls.Add(element);
            }

            // Add Column
            pageNumbersColumn.Controls.Add(secondElement);
            mainTableRow.Cells.Add(pageNumbersColumn);

            // Last Page Column
            var thirdElement = new HtmlGenericControl("ul");

            thirdElement.Attributes.Add("class", "FilesPager");

            var lastColumn = new TableCell { CssClass = "PagerLastColumn" };

            // Next Page
            if (this.CurrentPageIndex < this.PageCount - 1)
            {
                var nextElement = new HtmlGenericControl("li");

                nextElement.Attributes.Add("class", "NextPage");

                var lastNextLink = new HyperLink
                {
                    ID = "NextPageLink",
                    ToolTip =
                        $"{Localization.GetString("GoTo.Text", this.RessourceFile, this.LanguageCode)}{Localization.GetString("NextPage.Text", this.RessourceFile, this.LanguageCode)}",
                    Text = $"{Localization.GetString("NextPage.Text", this.RessourceFile, this.LanguageCode)} &gt;",
                    NavigateUrl =
                        this.Page.ClientScript.GetPostBackClientHyperlink(
                            this,
                            $"Page_{this.CurrentPageIndex + 2 - 1}", false)
                };

                nextElement.Controls.Add(lastNextLink);

                thirdElement.Controls.Add(nextElement);
            }

            if (end < this.PageCount)
            {
                var lastElement = new HtmlGenericControl("li");

                lastElement.Attributes.Add("class", "LastPage");

                var lastPageLink = new HyperLink
                {
                    ID = "LastPageLink",
                    ToolTip =
                        $"{Localization.GetString("GoTo.Text", this.RessourceFile, this.LanguageCode)}{Localization.GetString("LastPage.Text", this.RessourceFile, this.LanguageCode)}",
                    Text = $"{Localization.GetString("LastPage.Text", this.RessourceFile, this.LanguageCode)} &raquo;",
                    NavigateUrl =
                        this.Page.ClientScript.GetPostBackClientHyperlink(
                            this,
                            $"Page_{this.PageCount - 1}", false)
                };

                lastElement.Controls.Add(lastPageLink);

                thirdElement.Controls.Add(lastElement);
            }

            // Add Column
            lastColumn.Controls.Add(thirdElement);
            mainTableRow.Cells.Add(lastColumn);

            // Render Complete Control
            mainTable.RenderControl(writer);
        }
    }
}