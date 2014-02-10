function RTBIC_Session::onAdd(%this)
{
   %this.window = %window = new GuiWindowCtrl()
   {
      session = %this;
      profile = GuiWindowProfile;
      position = "0 0";
      extent = "300 202";
      minExtent = "300 202";
      text = %this.name;
      resizeWidth = true;
      resizeHeight = true;
      canMove = true;
      canClose = true;
      canMinimize = false;
      canMaximize = false;
      closeCommand = %this @ ".delete();";

      new GuiBitmapBorderCtrl()
      {
         profile = RTB_ContentBorderProfile;
         horizSizing = "width";
         vertSizing = "height";
         position = "7 30";
         extent = "287 140";

         new GuiSwatchCtrl()
         {
            profile = GuiDefaultProfile;
            horizSizing = "width";
            vertSizing = "height";
            position = "3 3";
            extent = "281 134";
            color = "255 255 255 255";

            new GuiScrollCtrl()
            {            
               profile = RTB_ScrollProfile;
               horizSizing = "width";
               vertSizing = "height";
               position = "1 1";
               extent = "279 119";
               hScrollBar = "alwaysOff";

               new GuiMLTextCtrl()
               {
                  profile = RTB_MLEditProfile;
                  horizSizing = "width";
                  vertSizing = "height";
                  position = "1 1";
                  extent = "265 84";
               };
            };
            new GuiMLTextCtrl()
            {
               profile = GuiDefaultProfile;
               horizSizing = "right";
               vertSizing = "top";
               position = "2 120";
               extent = "280 14";

               selectable = false;
            };
         };
      };

      new GuiBitmapBorderCtrl()
      {
         profile = RTB_ContentBorderProfile;
         horizSizing = "width";
         vertSizing = "top";
         position = "7 173";
         extent = "287 22";

         new GuiSwatchCtrl()
         {
            profile = GuiDefaultProfile;
            horizSizing = "width";
            vertSizing = "height";
            position = "3 3";
            extent = "281 16";
            color = "255 255 255 255";
         };

         new GuiBitmapCtrl()
         {
            profile = GuiDefaultProfile;
            horizSizing = "left";
            vertSizing = "bottom";
            position = "268 2";
            extent = "16 16";
            bitmap = $RTB::Path @ "images/icons/bullet_go";
         };

         new GuiBitmapButtonCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "left";
            vertSizing = "bottom";
            position = "268 2";
            extent = "16 16";
            command = %this @ ".send();";
            text = " ";
         };
      };
   };

   BAM_Overlay.add(%window);
   %input = RTBIC_InputRecycler.get();

   %input.horizSizing = "width";
   %input.vertSizing = "bottom";
   %input.position = "1 3";
   %input.extent = "264 16";
   %input.altCommand = %this @ ".send();";

   %window.getObject(1).add(%input);

   %window.scrollContainer = %window.getObject(0);
   %window.scroll = %window.scrollContainer.getObject(0).getObject(0);
   %window.status = %window.scrollContainer.getObject(0).getObject(1);
   %window.display = %window.scroll.getObject(0);
   %window.input = %input;

   %offset = 0;
   %position = "0 0";

   while(%free !$= true)
   {
      %free = true;
      for(%i=0;%i<RTBIC_SessionManager.getCount();%i++)
      {
         %session = RTBIC_SessionManager.getObject(%i);
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

function RTBIC_Session::onRemove(%this)
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

function RTBIC_Session::focus(%this)
{
   BAM_Overlay.pushToBack(%this.window);
   %this.window.input.makeFirstResponder(1);
}

function RTBIC_Session::write(%this, %line)
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

function RTBIC_Session::send(%this)
{
   %text = %this.window.input.getValue();

   if (%text $= "")
   {
      return;
   }

   %this.window.input.setValue("");

   if (getSubStr(%text,0,1) $= "/")
   {
      %first = firstWord(%text);
      %args = restWords(%text);

      if (%first $= "/me" || %first $= "/action")
      {
         %this.writeAction($RTB::CIRCClient::Cache::NickName, parseLinks(stripMLControlChars(%args)));
         RTBIC_SC.sendLine("PRIVMSG" SPC %this.name SPC ":\c0ACTION " @ %args @ "\c0");
      }
      else if (%first $= "/msg")
      {
         RTBIC_SC.sendLine("PRIVMSG" SPC %args);

         %name = firstWord(%args);
         %text = restWords(%args);

         %session = RTBIC_SessionManager.getSession(%name);
         %session.writeMessage($RTB::CIRCClient::Cache::NickName, %text);

         return;
      }
      else if (%first $= "/nick")
      {
         RTBIC_SC.sendLine("NICK" SPC %args);
      }
      else if (%first $= "/join")
      {
         RTBIC_SC.sendLine("JOIN" SPC %args);
      }
   }
   else
   {
      %this.writeMessage($RTB::CIRCClient::Cache::NickName, parseLinks(stripMLControlChars(%text)));
      RTBIC_SC.sendLine("PRIVMSG" SPC %this.name SPC ":" @ %text);
   }

   %this.focus();
   %this.window.scroll.scrollToBottom();
}

function RTBIC_Session::writeMessage(%this,%sender,%message)
{
   %message = "<font:Verdana Bold:12>"@%sender@"<font:Verdana:12>: "@%message;
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
      
   %this.write(%message);
}

function RTBIC_Session::writeAction(%this,%sender,%message)
{
   %message = "<font:Verdana Bold:12><color:CC00CC>* "@%sender@" <font:Verdana:12>"@%message@"<color:444444>";
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
   
   %this.write(%message);
}

function RTBIC_Session::writeNotice(%this,%message)
{
   %message = "<font:Verdana:12><color:666666>* "@%message@"<color:444444>";
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
   
   %this.write(%message);
}

function RTBIC_Session::writeInfo(%this,%message)
{
   %message = "<font:Verdana:12><color:00AA00>* "@%message@"<color:444444>";
   
   if(RTBCO_getPref("CC::ShowTimestamps"))
      %message = "<font:Verdana Bold:12>["@getSubStr(getWord(getDateTime(),1),0,8)@"] " @ %message;
   
   %this.write(%message);
}

function RTBIC_Session::writeError(%this,%message)
{
   %message = "<color:FF0000>* "@%message@"<color:444444>";
   %this.write(%message);
}