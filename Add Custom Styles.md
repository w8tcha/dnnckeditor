# Add Custom Styles
This is a Quick How to Define Custom Styles for the Editor 

## Editor Area CSS
* You need to modify or create the CSS File (Which you need to select under "Editor area CSS" in the Editor Options) you want to use.
* Here is a Sample of a css file i used for my tests, you could also use your skin.css or the portal.css otherwise you also need to have the same css properties in your skin.css or portal.css. Then it will be displayed correctly in the Editor and on the dnn page....

{code:css}
.textHeader,H1 {font-size:14px;color:#003366;font-weight:bold;}
{code:css}

## List of available styles
* Then you create a Style List which you need to enter as Setting in the Settings in the "Editor Config" Tab under StylesSet. You need to create a style Rule for each html element you want to use



{code:javascript}
[
// Block Styles
{ name : 'TextHeader', element : 'p', attributes : { 'class' : 'textHeader' } },

// Inline Styles
{ name : 'TextHeader', element : 'span', attributes : { 'class' : 'textHeader' } },

// Object Styles
{ name : 'TextHeader a', element : 'a', attributes : { 'class' : 'textHeader' } }
]
{code:javascript}

* Then you can select one of the Styles from the DropDown, and you are ready to use it in your dnn Installation.

![Styles DropDown With Custom Styles](Add Custom Styles_http://www.watchersnet.de/Portals/0/screenshots/dnn/StylesDropDownCustom.png)