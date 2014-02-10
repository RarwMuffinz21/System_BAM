//*********************************************************
//* Room Session Manifest Implementation
//*********************************************************
//- RTBIC_RoomSessionManifest::addUser (adds a user to the room session manifest)
function RTBIC_RoomSessionManifest::addUser(%this,%id,%name,%rank,%icon,%blocked)
{
   if(%this.hasUser(%id))
      return %this.getByID(%id);

   %user = new ScriptObject()
   {
      class = "RTBIC_RoomSessionManifestUser";
      
      id = %id;
      name = %name;
      rank = %rank;
      icon = %icon;
      
      blocked = %blocked;
      
      session = %this.getGroup();
      manifest = %this;
   };
   %this.add(%user);
   
   if(%this.getCount() == 1)
      %this.getGroup().window.setText(%this.getGroup().window.title @ "   (1 user)");
   else
      %this.getGroup().window.setText(%this.getGroup().window.title @ "   (" @ %this.getCount() @ " users)");
   
   return %user;
}

function RTBIC_RoomSessionManifest::addUserByNick(%this, %nick)
{
   %chars = "~&@%+";

   %rank["~"] = 3;
   %rank["&"] = 3;
   %rank["@"] = 2;
   %rank["%"] = 2;
   %rank["+"] = 1;

   %icon["~"] = "shield_gold";
   %icon["&"] = "shield_silver";
   %icon["@"] = "starS";
   %icon["%"] = "star0";
   %icon["+"] = "user";

   %rank = 0;
   %icon = "status_online";

   %first = getSubStr(%nick, 0, 1);

   if (strPos(%chars, %first) != -1)
   {
      %nick = getSubStr(%nick, 1, strLen(%nick));

      if (%rank[%first] !$= "")
      {
         %rank = %rank[%first];
      }

      if (%icon[%first] !$= "")
      {
         %icon = %icon[%first];
      }
   }

   return %this.addUser(%nick, %nick, %rank, %icon, 0);
}

//- RTBIC_RoomSessionManifest::hasUser (checks if manifest contains user)
function RTBIC_RoomSessionManifest::hasUser(%this,%id)
{
   for(%i=0;%i<%this.getCount();%i++)
   {
      if(%this.getObject(%i).id $= %id)
         return true;
   }
   return false;
}

//- RTBIC_RoomSessionManifest::getByID (returns manifest item by id)
function RTBIC_RoomSessionManifest::getByID(%this,%id)
{
   if(!%this.hasUser(%id))
      return false;
      
   for(%i=0;%i<%this.getCount();%i++)
   {
      %user = %this.getObject(%i);
      if(%user.id $= %id)
         return %user;
   }
   return false;
}

//- RTBIC_RoomSessionManifest::removeByID (remove user from session manifest by id)
function RTBIC_RoomSessionManifest::removeByID(%this,%id)
{
   if(!%this.hasUser(%id))
      return false;
      
   %user = %this.getByID(%id);
   %user.unrender();
   %user.delete();
   
   if(%this.getCount() == 1)
      %this.getGroup().window.setText(%this.getGroup().window.title @ "   (1 user)");
   else
      %this.getGroup().window.setText(%this.getGroup().window.title @ "   (" @ %this.getCount() @ " users)");
}

//- RTBIC_RoomSessionManifest::sort (sorts the manifest by rank/name)
function RTBIC_RoomSessionManifest::sort(%this)
{
   if(%this.getCount() <= 0)
      return;
      
   for(%i=3;%i>=0;%i--)
   {
      %sorter = new GuiTextListCtrl();
      for(%j=0;%j<%this.getCount();%j++)
      {
         %user = %this.getObject(%j);
         if(%user.rank $= %i)
            %sorter.addRow(%user,%user.name);
      }
      %sorter.sort(0,1);
      
      for(%j=0;%j<%sorter.rowCount();%j++)
      {
         %this.pushToBack(%sorter.getRowId(%j));
      }
      %sorter.delete();
   }
}

//- RTBIC_RoomSessionManifest::render (renders the manifest)
function RTBIC_RoomSessionManifest::render(%this)
{
   %this.session.window.userSwatch.clear();
   
   %this.sort();
   for(%i=0;%i<%this.getCount();%i++)
   {
      %item = %this.getObject(%i);
      %item.renderInPlace("0" SPC (%i * 22));
   }
   %item.session.window.userSwatch.extent = "0" SPC (%i * 22);
   %item.manifest.reshape();
   
   if(%this.getCount() == 1)
      %this.getGroup().window.setText(%this.getGroup().window.title @ "   (1 user)");
   else
      %this.getGroup().window.setText(%this.getGroup().window.title @ "   (" @ %this.getCount() @ " users)");
}

//- RTBIC_RoomSessionManifest::reshape (reshapes the swatch)
function RTBIC_RoomSessionManifest::reshape(%this)
{
   %swatch = %this.getGroup().window.userSwatch;
   
   %swatch.resize(1,getWord(%swatch.position,1),194,%swatch.getLowestPoint());
}

//*********************************************************
//* Room Session Manifest User Implementation
//*********************************************************
//- RTBIC_RoomSessionManifestUser::isRendered (checks to see if the manifest user is rendered already)
function RTBIC_RoomSessionManifestUser::isRendered(%this)
{
   if(!isObject(%this.gui_container))
      return false;

   //%a = %this.gui_container.getGroup();
   //%b = %this.session.window.userSwatch.getID();

   //if(%a != %b)
   //   return false;
      
   return true;
}

//- RTBIC_RoomSessionManifestUser::render (attempts to render the manifest user)
function RTBIC_RoomSessionManifestUser::render(%this)
{
   if(%this.isRendered())
   {
      %this.gui_userIcon.setBitmap($RTB::Path @ "images/icons/" @ %this.icon);
      %this.gui_userName.setText("<color:444444><font:Verdana:12>" @ %this.name);
   }
   else
   {
      %position = 0;
      %this.getGroup().sort();
      for(%i=0;%i<%this.getGroup().getCount();%i++)
      {
         %user = %this.getGroup().getObject(%i);
         if(!%user.isRendered() && %user !$= %this)
            continue;
            
         if(%this $= %user)
         {
            %this.session.window.userSwatch.conditionalShiftY(%position,22);
            %this.renderInPlace("0" SPC %position);
            %this.session.window.userSwatch.extent = vectorAdd(%this.session.window.userSwatch.extent,"0 22");
            %this.manifest.reshape();
         }
         else
            %position += 22;
      }
   }
}

//- RTBIC_RoomSessionManifestUser::renderInPlace (renders a manifest user taking a position argument)
function RTBIC_RoomSessionManifestUser::renderInPlace(%this,%position)
{
   if(%this.isRendered())
      return;
      
   %swatch = %this.session.window.userSwatch;
   
   %container = new GuiSwatchCtrl()
   {
      position = %position;
      extent = "129 22";
      
      color = "0 0 0 0";
   };
   %swatch.add(%container);
   %this.gui_container = %container;
   
   %selectBox = new GuiBitmapCtrl()
   {
      position = "0 0";
      extent = "129 22";
      
      visible = false;
      bitmap = $RTB::Path @ "images/ui/userListSelect_n";
   };
   %container.add(%selectBox);
   %this.gui_selectBox = %selectBox;
   
   %icon = new GuiBitmapCtrl()
   {
      position = "1 3";
      extent = "16 16";
   };
   %container.add(%icon);
   %this.gui_userIcon = %icon;
   
   %text = new GuiMLTextCtrl()
   {
      position = "19 5";
      extent = "147 12";
      
      selectable = false;
   };
   %container.add(%text);
   %this.gui_userName = %text;
   
   %mouseEvent = new GuiMouseEventCtrl()
   {
      position = "0 0";
      extent = "129 22";
      
      persistent = 1;
      eventType = "UserListSelect";
      eventCallbacks = "1111011";
      
      user = %this;
      select = %selectBox;
      session = %this.getGroup().session;
   };
   %container.add(%mouseEvent);
   %this.gui_mouseEvent = %mouseEvent;
   
   %this.render();
}

//- RTBIC_RoomSessionManifestUser::rerender (rerenders the manifest user if it's already rendered)
function RTBIC_RoomSessionManifestUser::rerender(%this)
{
   if(%this.isRendered())
      %this.unrender();
   %this.render();
}

//- RTBIC_RoomSessionManifestUser::unrender (unrenders the manifest user)
function RTBIC_RoomSessionManifestUser::unrender(%this)
{
   if(!%this.isRendered())
      return;
      
   %this.closeMenu();
   %position = getWord(%this.gui_container.position,1);

   %this.gui_container.delete();

   %this.session.window.userSwatch.conditionalShiftY(%position,-22);
   %this.session.window.userSwatch.extent = vectorSub(%this.session.window.userSwatch.extent,"0 22");
   
   %this.manifest.reshape();
}

//- RTBIC_RoomSessionManifestUser::openMenu (opens a menu of items for the user to select)
function RTBIC_RoomSessionManifestUser::openMenu(%this)
{
   %this.gui_selectBox.setBitmap($RTB::Path @ "images/ui/userListSelect_d");
   
   %top = getWord(%this.gui_selectBox.getPosRelativeTo(%this.session.window.userScroll),1);
   %bottom = %top + getWord(%this.gui_selectBox.extent,1);
   %scrollExt = getWord(%this.session.window.userScroll.extent,1)-2;

   if(%top < 0)
      %this.session.window.userSwatch.resize(1,getWord(%this.session.window.userSwatch.position,1)-(%top-2),140,getWord(%this.session.window.userSwatch.extent,1));
   if(%bottom > %scrollExt)
      %this.session.window.userSwatch.resize(1,getWord(%this.session.window.userSwatch.position,1)-(%bottom-%scrollExt)-2,140,getWord(%this.session.window.userSwatch.extent,1));
      
   
   %menuItems = -1;
   %menuIcon[%menuItems++] = "comment";
   %menuText[%menuItems] = "Chat";
   %menuComm[%menuItems] = %this@".openChatWindow();";
   //%menuIcon[%menuItems++] = "information";
   //%menuText[%menuItems] = "Info";
   //%menuComm[%menuItems] = %this@".info();";
   // if(!RTBIC_Roster.hasID(%this.id) && !RTBIC_InviteRoster.hasID(%this.id))
   // {
   //    %menuIcon[%menuItems++] = "heart_add";
   //    %menuText[%menuItems] = "Friend";
   //    %menuComm[%menuItems] = %this@".addFriend();";
   // }
   if(%this.manifest.getById($RTB::CIRCClient::Cache::NickName).rank >= 1 && %this.rank < %this.manifest.getById($RTB::CIRCClient::Cache::NickName).rank)
   {
      %menuIcon[%menuItems++] = "block";
      %menuText[%menuItems] = "Kick";
      %menuComm[%menuItems] = %this@".kick();";
   }
   if(%this.manifest.getById($RTB::CIRCClient::Cache::NickName).rank >= 2 && %this.rank < %this.manifest.getById($RTB::CIRCClient::Cache::NickName).rank)
   {
      %menuIcon[%menuItems++] = "delete";
      %menuText[%menuItems] = "Ban";
      %menuComm[%menuItems] = %this@".ban();";
      
      %menuIcon[%menuItems++] = "medal_gold_3";
      %menuText[%menuItems] = "Rank";
      %menuComm[%menuItems] = %this@".changeRank();";
   }
   %menuItems++;
   
   %menuSize = (%menuItems * 20) + 4;
   
   %container = BAM_Overlay;
   %position = %this.gui_selectBox.getAbsPosition(%container);
   %menu = new GuiSwatchCtrl()
   {
      position = vectorAdd(%position,"63 22");
      extent = "66" SPC %menuSize;
      color = "0 0 0 0";
      
      user = %this;
      
      new GuiBitmapCtrl()
      {
         position = "0" SPC %menuSize - 4;
         extent = "66 4";
         
         bitmap = $RTB::Path @ "images/ui/buddyListMenuBottom";
      };
   };
   %container.add(%menu);
   %this.gui_menu = %menu;
   %this.session.gui_userMenu = %menu;
   
   for(%i=0;%i<%menuItems;%i++)
   {
      %item = new GuiBitmapCtrl()
      {
         position = "0" SPC (%i * 20);
         extent = "66 20";
         
         bitmap = $RTB::Path @ "images/ui/buddyListMenu_n";
         
         new GuiBitmapCtrl()
         {
            position = "4 2";
            extent = "16 16";
            
            bitmap = $RTB::Path @ "images/icons/" @ %menuIcon[%i];
         };
         
         new GuiMLTextCtrl()
         {
            position = "22 4";
            extent = "54 12";
            
            text = "<color:444444><font:Verdana:12>" @ %menuText[%i];
            
            selectable = false;
         };
      };
      %menu.add(%item);
      
      %mouseEvent = new GuiMouseEventCtrl()
      {
         position = "0" SPC (%i * 20);
         extent = "66 20";
         
         eventType = "UserListMenu";
         eventCallbacks = "1101000";
         
         user = %this;
         item = %item;
         command = %menuComm[%i];
      };
      %menu.add(%mouseEvent);
   }
   %this.gui_mouseEvent.extent = "129" SPC (%menuSize + 30);
   
   if(RTBCO_getPref("CC::EnableSounds"))
      alxPlay(RTBIC_TickSound);
}

//- RTBIC_RoomSessionManifestUser::closeMenu (closes the roster menu)
function RTBIC_RoomSessionManifestUser::closeMenu(%this)
{
   if(isObject(%this.gui_menu))
   {
      %this.session.gui_userMenu = "";
      %this.gui_menu.schedule(1,"delete");
      if(RTBCO_getPref("CC::EnableSounds"))
         alxPlay(RTBIC_TickSound);
   }
   
   %this.gui_mouseEvent.extent = "129 22";
   %this.gui_selectBox.setBitmap($RTB::Path @ "images/ui/userListSelect_n");
}

//- Event_UserListSelect::onMouseEnter (handles entry interaction with roster user item)
function Event_UserListSelect::onMouseEnter(%this)
{
   %this.select.setVisible(true);
}

//- Event_UserListSelect::onMouseLeave (handles leaving interaction with roster user item)
function Event_UserListSelect::onMouseLeave(%this)
{     
   %this.select.setVisible(false);
   
   %this.user.closeMenu();
}

//- Event_UserListSelect::onMouseDown (handles click interaction with roster user item)
function Event_UserListSelect::onMouseDown(%this)
{
   if(%this.user.id $= $RTB::CIRCClient::Cache::NickName)
      return;
      
   if(isObject(%this.session.gui_userMenu) && %this.session.gui_userMenu.user !$= %this.user)
      %this.session.gui_userMenu.user.closeMenu();
      
   if(isObject(%this.user.gui_menu))
      return;
      
   %this.select.setBitmap($RTB::Path @ "images/ui/userListSelect_h");
}

//- Event_UserListSelect::onMouseUp (handles click interaction with roster user item)
function Event_UserListSelect::onMouseUp(%this)
{
   if(%this.user.id $= $RTB::CIRCClient::Cache::NickName)
      return;
      
   if(isObject(%this.session.gui_userMenu) && %this.session.gui_userMenu.user !$= %this.user)
      return;
      
   if(isObject(%this.user.gui_menu))
   {
      %this.user.closeMenu();
      
      if((getSimTime() - %this.lastClickTime) <= 300)
         %this.user.openChatWindow();
         
      return;
   }
      
   %this.user.openMenu();
   
   %this.lastClickTime = getSimTime();
}

//- Event_UserListSelect::onRightMouseDown (handles click interaction with roster user item)
function Event_UserListSelect::onRightMouseDown(%this)
{
   if(%this.user.id $= $RTB::CIRCClient::Cache::NickName)
      return;
      
   if(isObject(%this.session.gui_userMenu) && %this.session.gui_userMenu.user !$= %this.user)
      %this.session.gui_userMenu.user.closeMenu();
      
   if(isObject(%this.user.gui_menu))
      return;
      
   %this.select.setBitmap($RTB::Path @ "images/ui/userListSelect_h");
}

//- Event_UserListSelect::onRightMouseUp (handles click interaction with roster user item)
function Event_UserListSelect::onRightMouseUp(%this)
{
   if(%this.user.id $= $RTB::CIRCClient::Cache::NickName)
      return;
      
   if(isObject(%this.session.gui_userMenu) && %this.session.gui_userMenu.user !$= %this.user)
      return;
      
   if(isObject(%this.user.gui_menu))
   {
      %this.user.closeMenu();
      return;
   }
      
   %this.user.openMenu();
   
   %this.lastClickTime = getSimTime();
}

//- Event_BuddyListMenu::onMouseEnter (handles menu item interaction of the roster interact menu)
function Event_UserListMenu::onMouseEnter(%this)
{
   %this.item.setBitmap($RTB::Path @ "images/ui/buddyListMenu_h");
}

//- Event_BuddyListMenu::onMouseLeave (handles menu item interaction of the roster interact menu)
function Event_UserListMenu::onMouseLeave(%this)
{
   %this.item.setBitmap($RTB::Path @ "images/ui/buddyListMenu_n");
}

//- Event_BuddyListMenu::onMouseUp (handles menu item interaction of the roster interact menu)
function Event_UserListMenu::onMouseUp(%this)
{
   eval(%this.command);
}

//- RTBIC_RoomSessionManifestUser::openChatWindow (opens chat window with user)
function RTBIC_RoomSessionManifestUser::openChatWindow(%this)
{
   RTBIC_SessionManager.getSession(%this.id);
   %this.closeMenu();
}

//- RTBIC_RoomSessionManifestUser::info (gets info on a user)
function RTBIC_RoomSessionManifestUser::info(%this)
{
   //RTBIC_Socket.getUserInfo(%this.id);
   //RTB_ConnectClient.messageBox("Please Wait ...","Getting user details for Blockland ID "@%this.id);

   RTBIC_SC.sendLine("WHOIS" SPC %this.id);
   %this.closeMenu();
}

//- RTBIC_RoomSessionManifestUser::addFriend (adds user as a friend)
function RTBIC_RoomSessionManifestUser::addFriend(%this)
{
   RTBIC_Socket.addToRoster(%this.id,%this.session);
   
   %this.closeMenu();
}

//- RTBIC_RoomSessionManifestUser::kick (tries to kick a user)
function RTBIC_RoomSessionManifestUser::kick(%this)
{
   BAM_IRC.sendCommand("KICK", %this.session.name SPC %this.id);
   %this.closeMenu();
}

//- RTBIC_RoomSessionManifestUser::ban (tries to ban a user)
function RTBIC_RoomSessionManifestUser::ban(%this,%flag)
{  
   %this.closeMenu();

   %modal = %this.session.window.modal_BanUser;   
   
   if(%flag $= "")
   {
      %this.session.setModalWindow("BanUser");
      
      %modal.btn_ban.command = %this@".ban(1);";
      
      %select = %modal.pop_length;
      %select.clear();
      
      %select.add("5 Minutes",60*5);
      %select.add("15 Minutes",60*15);
      %select.add("30 Minutes",60*30);
      %select.add("1 Hour",60*60);
      %select.add("2 Hours",60*60*2);
      %select.add("6 Hours",60*60*6);
      %select.add("12 Hours",60*60*12);
      %select.add("1 Day",60*60*24);
      %select.add("2 Days",60*60*24*2);
      %select.add("5 Days",60*60*24*5);
      %select.add("1 Week",60*60*24*7);
      
      return;
   }
   %this.session.closeModalWindow();
   
   %reason = %modal.txt_reason.getValue();
   %length = (%modal.chk_forever.getValue() $= 1) ? -1 : %modal.pop_length.getSelected();
   
   RTBIC_Socket.banUser(%this.session.name,%this.id,%reason,%length);
}

//- RTBIC_RoomSessionManifestUser::changeRank (opens dialog to change rank, and changes it)
function RTBIC_RoomSessionManifestUser::changeRank(%this,%rank)
{
   %this.closeMenu();
   
   if(%rank $= "")
   {
      %this.session.setModalWindow("ChangeRank");
      
      %modal = %this.session.window.modal_ChangeRank;
      %modal.btn_normal.command = %this@".changeRank(0);";
      %modal.btn_mod.command = %this@".changeRank(1);";
      %modal.btn_admin.command = %this@".changeRank(2);";
      
      return;
   }
   %this.session.closeModalWindow();
   
   RTBIC_Socket.changeUserRank(%this.session.name,%this.id,%rank);
}