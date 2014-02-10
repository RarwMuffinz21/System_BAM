//#############################################################################
//#
//#   Return to Blockland - Version 4
//#
//#   -------------------------------------------------------------------------
//#
//#      $Rev: 167 $
//#      $Date: 2011-02-27 22:35:06 +0000 (Sun, 27 Feb 2011) $
//#      $Author: Ephialtes $
//#      $URL: http://svn.returntoblockland.com/code/trunk/interface/profiles/generic.cs $
//#
//#      $Id: generic.cs 167 2011-02-27 22:35:06Z Ephialtes $
//#
//#   -------------------------------------------------------------------------
//#
//#   Interface / Profiles / Generic GUI Profiles
//#
//#############################################################################
//Register that this module has been loaded
$RTB::Interface::Profiles::Generic = 1;

// RTB Verdana 12pt Font
if(!isObject(RTB_Verdana12Pt)) new GuiControlProfile(RTB_Verdana12Pt)
{
	fontColor = "30 30 30 255";
	fontSize = 12;
	fontType = "Verdana";
	justify = "Left";
   fontColors[1] = "100 100 100";
   fontColors[2] = "0 255 0";  
   fontColors[3] = "0 0 255"; 
   fontColors[4] = "255 255 0";   
   fontColorLink = "60 60 60 255";
   fontColorLinkHL = "0 0 0 255";
};

// RTB Verdana 12pt Auto Font
if(!isObject(RTB_AutoVerdana12)) new GuiControlProfile(RTB_AutoVerdana12)
{
   fontColor = "30 30 30 255";
   fontSize = 12;
   fontType = "Verdana";
   justify = "Left";
   fontColors[1] = "100 100 100";
   fontColors[2] = "0 255 0";  
   fontColors[3] = "0 0 255"; 
   fontColors[4] = "255 255 0";
   fontColors[5] = "0 0 0";
   autoSizeWidth = true;
   autoSizeHeight = true;
};

// RTB Verdana 12pt Font Centered
if(!isObject(RTB_Verdana12PtCenter)) new GuiControlProfile(RTB_Verdana12PtCenter)
{
	fontColor = "30 30 30 255";
	fontSize = 12;
	fontType = "Verdana";
	justify = "Center";
   fontColors[1] = "100 100 100";
   fontColors[2] = "0 255 0";  
   fontColors[3] = "0 0 255"; 
   fontColors[4] = "255 255 0";   
};

// RTB Header Text
if(!isObject(RTB_HeaderText)) new GuiControlProfile(RTB_HeaderText)
{
   fontColor = "30 30 30 255";
   fontSize = 18;
   fontType = "Impact";
   justify = "Left";
   fontColors[1] = "100 100 100";
   fontColors[2] = "0 255 0";  
   fontColors[3] = "0 0 255"; 
   fontColors[4] = "255 255 0";   
};

// Profile used for version text on main menu
if(!isObject(RTB_VersionProfile)) new GuiControlProfile(RTB_VersionProfile : MM_LeftProfile)
{
   fontOutlineColor = "255 24 24 255";
   justify = "right";
};

// RTB Checkboxes
if(!isObject(RTB_CheckBoxProfile)) new GuiControlProfile(RTB_CheckBoxProfile : GuiCheckBoxProfile)
{
   bitmap = $RTB::Path@"images/ui/checkboxArray";
};

// Bold Checkbox
if(!isObject(RTB_CheckBoxBoldProfile)) new GuiControlProfile(RTB_CheckBoxBoldProfile : GuiCheckBoxProfile)
{
   fontType = "Arial Bold";
};

// RTB Radio Buttons
if(!isObject(RTB_RadioButtonProfile)) new GuiControlProfile(RTB_RadioButtonProfile : GuiCheckBoxProfile)
{
   bitmap = $RTB::Path@"images/ui/radioArray";
};

// RTB Popup Menu
if(!isObject(RTB_PopupProfile)) new GuiControlProfile(RTB_PopupProfile : GuiPopupMenuProfile)
{
   fillColor = "227 228 230 255";
   borderColor = "189 192 194 255";
   fontSize = 12;
   fontType = "Verdana";
   fontColor = "64 64 64 255";
   fontColors[0] = "64 64 64 255";
};

// RTB Text Edit
if(!isObject(RTB_TextEditProfile)) new GuiControlProfile(RTB_TextEditProfile : GuiTextEditProfile)
{
   fillColor = "255 255 255 255";
   borderColor = "188 191 193 255";
   fontSize = 12;
   fontType = "Verdana";
   fontColor = "64 64 64 255";
   fontColors[0] = "64 64 64 255";
   fontColors[1] = "0 0 0";
};

// RTB ML Text Edit
if(!isObject(RTB_MLEditProfile)) new GuiControlProfile(RTB_MLEditProfile : GuiMLTextEditProfile)
{
   fontSize = 12;
   fontType = "Verdana";
   fontColor = "64 64 64 255";
   fontColors[0] = "64 64 64 255";
};

// RTB Scroll
if(!isObject(RTB_ScrollProfile)) new GuiControlProfile(RTB_ScrollProfile)
{
   fontType = "Book Antiqua";
   fontSize = 22;
   justify = center;
   fontColor = "0 0 0";
   fontColorHL = "130 130 130";
   fontColorNA = "255 0 0";
   fontColors[0] = "0 0 0";
   fontColors[1] = "0 255 0";  
   fontColors[2] = "0 0 255"; 
   fontColors[3] = "255 255 0";
   hasBitmapArray = true;
   
   bitmap = $RTB::Path@"images/ui/scrollArray";
};