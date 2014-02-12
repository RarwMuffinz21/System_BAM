function RTBIC_RoomSession::onAdd(%this)
{
   %window = %this.window = new GuiWindowCtrl()
   {
      profile = GuiWindowProfile;
      position = "0 0";
      extent = "500 342";
      minExtent = "450 250";
      text = %this.name;
      title = %this.name;
      resizeWidth = true;
      resizeHeight = true;
      canMove = true;
      canClose = true;
      canMinimize = false;
      canMaximize = false;

      closeCommand = %this @ ".delete();";
      overlayCloseCommand = %this @ ".delete();";
      
      new GuiBitmapBorderCtrl()
      {
         profile = RTB_ContentBorderProfile;
         horizSizing = "width";
         vertSizing = "height";
         position = "7 30";
         extent = "334 280";
         
         new GuiSwatchCtrl()
         {
            profile = GuiDefaultProfile;
            horizSizing = "width";
            vertSizing = "height";
            position = "3 3";
            extent = "328 274";
            color = "255 255 255 255";

            new GuiScrollCtrl()
            {            
               profile = RTB_ScrollProfile;
               horizSizing = "width";
               vertSizing = "height";
               position = "1 1";
               extent = "326 271";
               hScrollBar = "alwaysOff";
               
               dSet = "window";
               dName = "scroll";
                  
               new GuiMLTextCtrl()
               {
                  profile = RTB_MLEditProfile;
                  horizSizing = "width";
                  vertSizing = "height";
                  position = "1 1";
                  extent = "312 12";
                  
                  dSet = "window";
                  dName = "display";
               };
            };
         };
      };
      
      new GuiBitmapBorderCtrl()
      {
         profile = RTB_ContentBorderProfile;
         horizSizing = "left";
         vertSizing = "height";
         position = "344 30";
         extent = "150 280";
         
         new GuiSwatchCtrl()
         {
            profile = GuiDefaultProfile;
            horizSizing = "width";
            vertSizing = "height";
            position = "3 3";
            extent = "144 274";
            color = "255 255 255 255";
            
            dSet = "window";
            dName = "userPane";

            new GuiScrollCtrl()
            {            
               profile = RTB_ScrollProfile;
               horizSizing = "width";
               vertSizing = "height";
               position = "1 1";
               extent = "142 272";
               hScrollBar = "alwaysOff";
               
               dSet = "window";
               dName = "userScroll";
               
               new GuiSwatchCtrl()
               {
                  profile = GuiDefaultProfile;
                  horizSizing = "right";
                  vertSizing = "bottom";
                  position = "1 1";
                  extent = "140 0";
                  minExtent = "140 0";
                  color = "255 255 255 255";
                  
                  dSet = "window";
                  dName = "userSwatch";
               };
            };
         };
      };
      
      new GuiBitmapBorderCtrl()
      {
         profile = RTB_ContentBorderProfile;
         horizSizing = "width";
         vertSizing = "top";
         position = "7 313";
         extent = "487 22";
         
         dSet = "window";
         dName = "inputContainer";
         
         new GuiSwatchCtrl()
         {
            profile = GuiDefaultProfile;
            horizSizing = "width";
            vertSizing = "height";
            position = "3 3";
            extent = "481 16";
            color = "255 255 255 255";
         };
         
         new GuiBitmapCtrl()
         {
            profile = GuiDefaultProfile;
            horizSizing = "left";
            vertSizing = "bottom";
            position = "468 2";
            extent = "16 16";
            bitmap = $RTB::Path@"images/icons/bullet_go";
         };
         
         new GuiBitmapButtonCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "left";
            vertSizing = "bottom";
            position = "468 2";
            extent = "16 16";
            command = %this@".send();";
            text = " ";
         };
      };
   };

   BAM_Overlay.add(%window);

   %window.session = %this;
   %this.registerPointers(%window);

   %input = BAM_InputRecycler.acquire();
   %input.setProfile(RTB_TextEditProfile);

   %input.horizSizing = "width";
   %input.vertSizing = "bottom";
   %input.position = "1 3";
   %input.extent = "465 16";
   %input.command = %this@".focus();";
   %input.altCommand = %this@".send();";

   %window.input = %input;

   %window.getObject(2).add(%input);
   %window.userPane.setVisible(true);

   %offset = 0;
   %position = "0 0";

   while(%free !$= true)
   {
      %free = true;
      for(%i=0;%i<RTBIC_RoomSessionManager.getCount();%i++)
      {
         %session = RTBIC_RoomSessionManager.getObject(%i);
         if(%session $= %this)
            continue;

         if(%session.window.position $= %position)
         {
            %free = false;
            break;
         }
      }
      
      if(%free !$= true)
      {
         %offset += 40;
         %position = %offset SPC %offset;
      }
   }

   %this.window.position = %position;
}

function RTBIC_RoomSession::onRemove(%this)
{
   if (isObject(%this.window.input))
   {
      RTBIC_InputRecycler.reclaim(%this.window.input);
      %this.window.input = "";
   }

   if (isObject(%this.window))
   {
      %this.window.delete();
   }
}

function RTBIC_RoomSession::send(%this)
{
   %text = %this.window.input.getValue();

   if (%text $= "")
   {
      return;
   }

   RTBIC_RoomSessionManager.lastFocus = %this;
   RTBIC_RoomSessionManager.lastFocusTime = getSimTime();

   %this.window.input.setValue("");

   if (getSubStr(%text, 0, 1) $= "/")
   {
      %first = firstWord(%text);
      %args = restWords(%text);

      if (%first $= "/me" || %first $= "/action")
      {
         %this.writeAction(BAM_IRC.nick, parseLinks(stripMLControlChars(%args)));
         BAM_IRC.sendMessage(%this.name, "ACTION", %args);
      }
      else if (%first $= "/msg")
      {
         %name = firstWord(%args);
         %text = restWords(%args);

         BAM_IRC.sendMessage(%name, %text);

         %session = RTBIC_SessionManager.getSession(%name);
         %session.writeMessage(BAM_IRC.nick, parseLinks(stripMLControlChars(%text)));
      }
      else
      {
         BAM_IRC.sendCommand(strUpr(getSubStr(%first, 1, strLen(%first))), %args);
      }
   }
   else
   {
      if (%this.manifest.getByID(BAM_IRC.nick).rank > 0)
      {
         %this.writeRank("<color:FF6600>" @ BAM_IRC.nick, parseLinks(stripMLControlChars(%text)));
      }
      else
      {
         %this.writeMessage("<color:FF6600>" @ BAM_IRC.nick, parseLinks(stripMLControlChars(%text)));
      }
   
      BAM_IRC.sendMessage(%this.name, %text);
   }

   %this.focus();
   %this.window.scroll.scrollToBottom();
}

function RTBIC_RoomSession::receive(%this, %name, %text)
{
   %user = %this.manifest.getByID(%name);

   if (firstWord(%text) $= "ACTION")
   {
      %this.writeAction(%name, restWords(%text));
   }
   else if (%user && %user.rank > 0)
   {
      %this.writeRank(%name, %text);
   }
   else
   {
      %this.writeMessage(%name, %text);
   }
}

function RTBIC_RoomSession::writeMessage(%this, %sender, %message)
{
   %message = "<font:Verdana Bold:12>"@%sender@"<font:Verdana:12><color:444444>: "@%message;
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
      
   %this.write(%message);
}

function RTBIC_RoomSession::writeRank(%this,%sender,%message)
{
   %message = "<font:Verdana Bold:12><color:0099FF>"@%sender@"<font:Verdana:12>: "@%message@"<color:444444>";
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
      
   %this.write(%message);
}

function RTBIC_RoomSession::writeAction(%this,%sender,%message)
{
   %message = "<font:Verdana Bold:12><color:CC00CC>* "@%sender@" <font:Verdana:12>"@%message@"<color:444444>";
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
   
   %this.write(%message);
}

function RTBIC_RoomSession::writeInfo(%this,%message)
{
   %message = "<font:Verdana:12><color:00AA00>* "@%message@"<color:444444>";
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
   
   %this.write(%message);
}

function RTBIC_RoomSession::writeColor(%this,%message,%color)
{
   %message = "<font:Verdana:12><color:"@%color@">* "@%message@"<color:444444>";
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
   
   %this.write(%message);
}

function RTBIC_RoomSession::writeNotice(%this,%message)
{
   %message = "<font:Verdana:12><color:666666>* "@%message@"<color:444444>";
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
   
   %this.write(%message);
}

function RTBIC_RoomSession::writeError(%this,%message)
{
   %message = "<color:FF0000>* "@%message@"<color:444444>";
   
   %this.write(%message);
}

function RTBIC_RoomSession::write(%this,%line)
{
   %scroll = %this.window.scroll;
   %display = %this.window.display;
   
   %position = getWord(%display.position,1);
   if((getWord(%display.extent,1)+getWord(%display.position,1)) <= (getWord(%scroll.extent,1)-1))
      %atBottom = true;
   
   if(%display.getValue() $= "")
      %display.setValue(%line);
   else
      %display.setValue(%display.getValue()@"\n"@%line);
      
   if(BAM_Overlay.isAwake())
      %display.forceReflow();
      
   %display.setCursorPosition(strLen(%display.getValue()));
   
   if(%atBottom)
      %scroll.scrollToBottom();
   else
      %display.resize(getWord(%display.position,0),%position,getWord(%display.extent,0),getWord(%display.extent,1));
}

function RTBIC_RoomSession::focus(%this)
{
   BAM_Overlay.pushToBack(%this.window);
   %this.window.input.makeFirstResponder(1);
}

function RTBIC_RoomSession::registerPointers(%this, %parent)
{
   for(%i=0;%i<%parent.getCount();%i++)
   {
      %ctrl = %parent.getObject(%i);
      
      if(%ctrl.dSet $= "parent")
      {
         %target = %ctrl.getGroup();
         if(%ctrl.dOffset > 0)
            for(%j=0;%j<%ctrl.dOffset;%j++)
               %target = %target.getGroup();
            
         eval(%target@"."@%ctrl.dName@" = "@%ctrl@";");
      }
      else if(%ctrl.dSet $= "window")
      {
         eval(%this.window@"."@%ctrl.dName@" = "@%ctrl@";");
      }

      %ctrl.dSet = "";
      %ctrl.dName = "";

      if(%ctrl.getCount() > 0)
         %this.registerPointers(%ctrl);
   }
}