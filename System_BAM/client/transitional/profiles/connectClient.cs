//#############################################################################
//#
//#   Return to Blockland - Version 4
//#
//#   -------------------------------------------------------------------------
//#
//#      $Rev: 187 $
//#      $Date: 2010-01-21 23:51:47 +0000 (Thu, 21 Jan 2010) $
//#      $Author: Ephialtes $
//#      $URL: http://svn.returntoblockland.com/code/branches/4000/interface/profiles/generic.cs $
//#
//#      $Id: generic.cs 187 2010-01-21 23:51:47Z Ephialtes $
//#
//#   -------------------------------------------------------------------------
//#
//#   Interface / Profiles / Connect Client GUI Profiles
//#
//#############################################################################
//Register that this module has been loaded
$RTB::Interface::Profiles::ConnectClient = 1;

// Connect Client Content Border
if(!isObject(RTB_ContentBorderProfile)) new GuiControlProfile(RTB_ContentBorderProfile : GuiBitmapBorderProfile)
{
   bitmap = $RTB::Path@"images/ui/contentArray";
};

// RTB Text Edit
if(!isObject(RTB_CCGroupEditProfile)) new GuiControlProfile(RTB_CCGroupEditProfile : RTB_TextEditProfile)
{
   fillColor = "255 255 255 255";
   borderColor = "188 191 193 255";
   fontSize = 12;
   fontType = "Verdana Bold";
   fontColor = "51 51 51 255";
   fontColors[0] = "51 51 51 255";
   fontColors[1] = "0 0 0";
};