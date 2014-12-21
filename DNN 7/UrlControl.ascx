<%@ Control Language="c#" AutoEventWireup="false" Inherits="WatchersNET.CKEditor.Controls.UrlControl" %>

<div>
    <asp:Label ID="FolderLabel" runat="server" EnableViewState="False" resourcekey="Folder" CssClass="NormalBold" />
    <asp:DropDownList ID="Folders" runat="server" AutoPostBack="True" CssClass="DefaultDropDown" Width="300" />
</div>
<div>
    <asp:Label ID="FileLabel" runat="server" EnableViewState="False" resourcekey="File" CssClass="NormalBold" />
    <asp:DropDownList ID="Files" runat="server" DataTextField="Text" DataValueField="Value" CssClass="DefaultDropDown" Width="300" />
</div>