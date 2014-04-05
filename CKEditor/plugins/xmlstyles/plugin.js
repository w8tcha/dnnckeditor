/*
Copyright (c) 2003-2011, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/
(function()
{
   CKEDITOR.plugins.add( 'xmlstyles', 
   {
      requires : [ 'ajax', 'xml' ],
	  
	 
	 init: function(editor) 
      {
		 if(typeof editor.config.stylesSet == 'string' && editor.config.stylesSet.indexOf(".xml") > 1) 
         {
			CKEDITOR.ajax.loadXml( editor.config.stylesSet.substr(13), function(xml)
            {
               if(!xml ) 
               {
                  alert( 'XML template file not parsed' );
                  return;
               }
         
               var stylesNode = xml.selectSingleNode( '/Styles' );
         
               if(!stylesNode )
               {
                  alert( 'The <Styles> node was not found' );
                  return ;
               }               
               
               var styleNodes = xml.selectNodes( 'Style', stylesNode ),
                  xml_styles = [];
         
               for ( var i = 0 ; i < styleNodes.length ; i++ ) 
               {
                  var styleNode = styleNodes[i],
                     attributeNodes = xml.selectNodes( 'Attribute', styleNode );
                     styleSet = {},
                  
                  styleSet.name       = styleNode.getAttribute('name');
                  styleSet.element    = styleNode.getAttribute('element');
                                    
                  if(attributeNodes.length) 
                  {
                     styleSet.attributes = {};
                     for ( var j = 0 ; j < attributeNodes.length ; j++ ) 
                     {
                        if(attributeNodes[j].getAttribute('name') == 'style') 
                        {
                           styleSet.styles = {};
                           
                           var params = attributeNodes[j].getAttribute('value').split(';');
                           for(var k = 0; k < params.length;k++) {
                              if(params[k].length) {
                                 var pos = params[k].indexOf(':');//using split would break on absolute urls
                                 styleSet.styles[params[k].substr(0,pos)] = params[k].substr(pos+1);
                              }
                           }
                           
                        }
						else if(attributeNodes[j].getAttribute('name') == 'class') 
                        {
                           styleSet.attributes['class'] = attributeNodes[j].getAttribute('value');
                           
                        } else {         
                           styleSet.attributes[attributeNodes[j].getAttribute('name')] = attributeNodes[j].getAttribute('value');
                        }
                     }
                  }
				  
				  xml_styles.push(styleSet);                  
               }
			   if (CKEDITOR.stylesSet.get('CustomStyles') == null) {         
                   CKEDITOR.stylesSet.add('CustomStyles', xml_styles);
			   }
			   
            });
         }
      }
   });
})();