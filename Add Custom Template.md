# Add Custom Template

This is a Quick How to Define Custom Templates for the Editor

# Create locally a file called **MyTemplates.xml** or **MyTemplates.js** (XML is the Recommended format).
# And add the Content bellow...
# The "imagesBasePath" attribute relative Path of Your Portal Home directory where you store the Template image files which can be defined for every template. The Image, must be uploaded manually to the folder.
# Now you can customize the templates as you like
# When done that must be uploaded to the Portal Home directory and selected in "Editor Templates File" in the Editor Options under Editor Templates.

## XML Sample

{code:xml}
<?xml version="1.0" encoding="utf-8" ?>
<Templates imagesBasePath="/DotNetNuke/Portals/0/" templateName="default">
	<Template title="Image and Title" image="template1.gif">
		<Description>One main image with a title and text that surround the image1.</Description>
		<Html>
			<![CDATA[
				<img style="MARGIN-RIGHT: 10px" height="100" alt="" width="100" align="left"/>
				<h3>Type the title here</h3>
				Type the text here
			]]>
		</Html>
	</Template>
	<Template title="Strange Template" image="template2.gif">
		<Description>A template that defines two colums, each one with a title, and some text.</Description>
		<Html>
			<![CDATA[
				<table cellspacing="0" cellpadding="0" width="100%" border="0">
					<tbody>
						<tr>
							<td width="50%">
							<h3>Title 1</h3>
							</td>
							<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </td>
							<td width="50%">
							<h3>Title 2</h3>
							</td>
						</tr>
						<tr>
							<td>Text 1</td>
							<td>&nbsp;</td>
							<td>Text 2</td>
						</tr>
					</tbody>
				</table>
				More text goes here.
			]]>
		</Html>
	</Template>
	<Template title="Text and Table" image="template3.gif">
		<Description>A title with some text and a table.</Description>
		<Html>
			<![CDATA[
				<table align="left" width="80%" border="0" cellspacing="0" cellpadding="0"><tr><td>
					<h3>Title goes here</h3>
					<p>
					<table style="FLOAT: right" cellspacing="0" cellpadding="0" width="150" border="1">
						<tbody>
							<tr>
								<td align="center" colspan="3"><strong>Table title</strong></td>
							</tr>
							<tr>
								<td>&nbsp;</td>
								<td>&nbsp;</td>
								<td>&nbsp;</td>
							</tr>
							<tr>
								<td>&nbsp;</td>
								<td>&nbsp;</td>
								<td>&nbsp;</td>
							</tr>
							<tr>
								<td>&nbsp;</td>
								<td>&nbsp;</td>
								<td>&nbsp;</td>
							</tr>
							<tr>
								<td>&nbsp;</td>
								<td>&nbsp;</td>
								<td>&nbsp;</td>
							</tr>
						</tbody>
					</table>
					Type the text here</p>
				</td></tr></table>
			]]>
		</Html>
	</Template>
</Templates>
{code:xml}

## Javascript Sample

{code:javascript}
/*Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

// Register a templates definition set named "default".
CKEDITOR.addTemplates( 'default',
{
	// The name of sub folder which hold the shortcut preview images of the
imagesPath : CKEDITOR.getUrl( '/DotNetNuke/Portals/0/' ),

	// The templates definitions.
	templates :
		[
			{
				title: 'Image and Title',
				image: 'template1.gif',
				description: 'One main image with a title and text that surround the image.',
				html:
					'<h3>' +
						'<img style="margin-right: 10px" height="100" width="100" align="left"/>' +
						'Type the title here'+
					'</h3>' +
					'<p>' +
						'Type the text here' +
					'<p></p>'
			},
			{
				title: 'Strange Template',
				image: 'template2.gif',
				description: 'A template that defines two colums, each one with a title, and some text.',
				html:
					'<table cellspacing="0" cellpadding="0" style="width:100%" border="0">' +
						'<tr>' +
							'<td style="width:50%">' +
								'<h3>Title 1</h3>' +
							'</td>' +
							'<td></td>' +
							'<td style="width:50%">' +
								'<h3>Title 2</h3>' +
							'</td>' +
						'</tr>' +
						'<tr>' +
							'<td>' +
								'Text 1' +
							'</td>' +
							'<td></td>' +
							'<td>' +
								'Text 2' +
							'</td>' +
						'</tr>' +
					'</table>' +
					'<p>' +
						'More text goes here.' +
					'<p></p>'
			},
			{
				title: 'Text and Table',
				image: 'template3.gif',
				description: 'A title with some text and a table.',
				html:
					'<div style="width: 80%">' +
						'<h3>' +
							'Title goes here' +
						'</h3>' +
						'<table style="float: right" cellspacing="0" cellpadding="0" border="1">' +
							'<caption style="border:solid 1px black">' +
								'<strong>Table title</strong>' +
							'</caption>' +
							'' +
							'<tr>' +
								'<td>&nbsp;</td>' +
								'<td>&nbsp;</td>' +
								'<td>&nbsp;</td>' +
							'</tr>' +
							'<tr>' +
								'<td>&nbsp;</td>' +
								'<td>&nbsp;</td>' +
								'<td>&nbsp;</td>' +
							'</tr>' +
							'<tr>' +
								'<td>&nbsp;</td>' +
								'<td>&nbsp;</td>' +
								'<td>&nbsp;</td>' +
							'</tr>' +
						'</table>' +
						'<p>' +
							'Type the text here' +
						'<p></p>' +
					'</div>'
			}
		]
});
{code:javascript}

