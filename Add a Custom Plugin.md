If you want to add an Plugin that is not already inlcuded, For Example from The [Plugins Repository](http://ckeditor.com/addons/plugins/all) you can do that by doing the following

## Download and Install the Plugin
# Download The Plugin Zip File (if not already done)
# Extract the Zip and copy the plugin folder (the folder should have the same name as the plugin) to the CKEditor Plugins folder (...\Providers\HtmlEditorProviders\CKEditor\plugins)
# Then you need to add the plugin to the extraPlugins list in the Editor Config Settings
# If the Plugin doesnt add a Button to the toolbar, the plugin will loaded with editor

## Making the Plugin visible in the Toolbar Set Editor
# First you need to copy the toolbar icon file from the plugin folder to the ..\Providers\HtmlEditorProviders\CKEditor\icons folder and make sure the file has the same name as the image
# Then you need to modify the CKToolbarButtons.xml in the portal home folder (the standard location is ..\Portals\0) and add a new list item for the plugin

{code:xml}
<ToolbarButton>
    <ToolbarName>pluginName</ToolbarName>
    <ToolbarIcon>pluginName.png</ToolbarIcon>
  </ToolbarButton>
{code:xml}

# Now you should be able to add the new Toolbar to any Toolbar Set you want, inside the Toolbar Editor.



