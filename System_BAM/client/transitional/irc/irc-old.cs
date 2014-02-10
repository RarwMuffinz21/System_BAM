exec("./InputRecycler.cs");
exec("./RoomSession.cs");
exec("./RoomSessionManager.cs");
exec("./RoomSessionManifest.cs");
exec("./Session.cs");
exec("./SessionManager.cs");

if (!isObject(RTBIC_InputRecycler))
{
   RTBIC_createInputRecycler();
}

if (!isObject(RTBIC_RoomSessionManager))
{
   RTBIC_createRoomSessionManager();
}

if (!isObject(RTBIC_SessionManager))
{
   RTBIC_createSessionManager();
}

$RTB::CIRCClient::RetryCount = "5";

function RTBIC_SC::sendLine(%this,%line)
{
   %this.send(%line @ "\r\n");
}

function RTBIC_SC::addHandle(%this,%handle,%routine)
{
   %this.dispatch[%handle] = %routine;
   return 1;
}

function RTBIC_Connect()
{
   RTBIC_SC.connect(RTBIC_SC.site@":"@RTBIC_SC.port);
   RTBIC_serverSession().writeNotice("Connecting...");
}

function RTBIC_Disconnect(%cleanup)
{
   if(!%cleanup)
      RTBIC_SC.disconnect();
      
   RTBIC_SC.connected = 0;

   cancel(RTBIC_SC.retrySchedule);
   RTBIC_serverSession().writeNotice("Disconnected.");

   RTBIC_RoomSessionManager.deleteAll();
   RTBIC_SessionManager.deleteAll();
}

function RTBIC_SC::onConnected(%this)
{
   RTBIC_serverSession().writeNotice("Connected.");

   %this.connected = 1;
	%guestName = "Blockhead"@getNumKeyID();
	%this.sendLine("NICK "@%guestName);

	if($Pref::Player::NetName !$= "" && isUnlocked())
	   %userName = filterString(strReplace($Pref::Player::NetName," ","_"),"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789[]-");
   else if($Pref::Player::LANName !$= "")
      %userName = filterString(strReplace($Pref::Player::LANName," ","_"),"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789[]-");

	%this.sendLine("USER "@%guestName@" 0 * :"@%userName@"-"@getNumKeyID());

	$RTB::CIRCClient::Cache::NickName = %guestName;
	$RTB::CIRCClient::Cache::DesNickName = %userName;
}

function RTBIC_SC::onLine(%this,%line)
{
	if(getSubStr(%line,0,1) $= ":")
		%line = nextToken(getSubStr(%line,1,strLen(%line)),prefix," ");
		
   %line = nextToken(%line,command," :");
   %line = nextToken(%line,params,"");

   %prefix = strReplace(%prefix,"\"","\\\"");
   %params = strReplace(%params,"\"","\\\"");
   if(getsubstr(%params,strlen(%params)-1,1) $= "\\")
      %params = %params @ "\\";
      
   if(%this.dispatch[%command] !$= "")
      eval("RTBIC_SC::"@%this.dispatch[%command]@"("@%this@",\""@%prefix@"\",\""@%params@"\");");
   else
   { 
      if($RTB::Debug)
         error("ERROR: No dispatch route for command: "@%command);
   }
}

function RTBIC_SC::onConnectFailed(%this)
{
   %this.connected = 0;
   
   %this.retryCount++;
   if(%this.retryCount > $RTB::CIRCClient::RetryCount)
      RTBIC_serverSession().writeError("Failed to connect.");
   else
   {
      RTBIC_serverSession().writeError("Failed to connect. Retrying ("@%this.retryCount@"/5)");
      %this.retrySchedule = %this.schedule(1000,"connect",%this.site@":"@%this.port);
   }
}

function RTBIC_SC::onDNSFailed(%this)
{
   %this.connected = 0;
   
   %this.retryCount++;
   if(%this.retryCount > $RTB::CIRCClient::RetryCount)
      RTBIC_serverSession().writeError("Failed to connect.");
   else
   {
      RTBIC_serverSession().writeError("Failed to connect. Retrying ("@%this.retryCount@"/5)");
      %this.retrySchedule = %this.schedule(1000,"connect",%this.site@":"@%this.port);
   }
}

function RTBIC_SC::onDisconnect(%this)
{
   %this.connected = 0;
   RTBIC_Disconnect(1);
}

RTBIC_SC.addHandle("NOTICE","onNotice");
function RTBIC_SC::onNotice(%this,%prefix,%params)
{
   if (strPos(%prefix, "Global") == 0)
   {
      return;
   }

   nextToken(%prefix, sender, "!");

   if (%sender $= "InfoServ" || %sender $= "failnode.voltirc.com")
   {
      return;
   }

   %params = stripMLControlChars(nextToken(%params, params, ":"));

   %session = RTBIC_SessionManager.getSession(%sender);
   %session.writeNotice(%params);
}

RTBIC_SC.addHandle("JOIN","onJoin");
function RTBIC_SC::onJoin(%this,%prefix,%params)
{
   nextToken(%prefix, "nick", "!");
   nextToken(%params, "room", " :");

   %search = %nick !$= $RTB::CIRCClient::Cache::NickName;
   %session = RTBIC_RoomSessionManager.getSession(%room, %search);

   if (!%session)
   {
      return;
   }

   %user = %session.manifest.addUserByNick(%nick);

   if (!%session.manifest.loading)
   {
      %user.render();
      //%session.manifest.render();
   }

   if ($RTB::Options::IC::Filter::ShowConnects && %search)
   {
      %session.writeNotice(%nick @ " has joined the room.");
   }
}

RTBIC_SC.addHandle("PART","onPart");
function RTBIC_SC::onPart(%this,%prefix,%params)
{
	nextToken(%prefix, "nick", "!");

   %room = nextToken(%params, "", ": ");
   %session = RTBIC_RoomSessionManager.getSession(%room, 1);

   if (!%session)
   {
      return;
   }

   if (%nick $= $RTB::CIRCClient::Cache::NickName)
   {
      %session.delete();
   }
   else if ($RTB::Options::IC::Filter::ShowDisconnects)
   {
      %session.writeNotice(%nick @ " has left the room.");
      %session.manifest.removeByID(%nick);
   }
}

RTBIC_SC.addHandle("QUIT","onQuit");
function RTBIC_SC::onQuit(%this,%prefix,%params)
{
   nextToken(%prefix, "nick", "!");
   %count = RTBIC_RoomSessionManager.getCount();

   for (%i = 0; %i < %count; %i++)
   {
      %session = RTBIC_RoomSessionManager.getObject(%i);

      if (%session.manifest.hasUser(%nick))
      {
         %this.onPart(%prefix, "PART :" @ %session.name);
      }
   }
}

RTBIC_SC.addHandle("332","onTopic");
function RTBIC_SC::onTopic(%this,%prefix,%params)
{
   %topic = getWords(%params, 2, getWordCount(%params));
   %topic = getSubStr(%topic, 1, strLen(%topic));

   %room = getWord(%params, 1);
   %session = RTBIC_RoomSessionManager.getSession(%room, 1);

   if (%session)
   {
      %session.writeInfo(%topic);
   }
}

RTBIC_SC.addHandle("TOPIC","onTopicChange");
function RTBIC_SC::onTopicChange(%this, %prefix, %params)
{
   %topic = nextToken(%params, prefix," :");
   %room = getWord(%params, 1);
   %session = RTBIC_RoomSessionManager.getSession(%room, 1);

   if (%session)
   {
      %session.writeInfo(%topic);
   }
}

RTBIC_SC.addHandle("438","onError");
RTBIC_SC.addHandle("431","onError");
RTBIC_SC.addHandle("401","onError");
RTBIC_SC.addHandle("ERROR","onError");
function RTBIC_SC::onError(%this,%prefix,%params)
{
	%message = nextToken(%params,prefix,":");

	if(getWordCount(%prefix) > 1)
   {
      %session = RTBIC_SessionManager.createSession(getWord(%prefix, 1));
      %session.writeError("ERROR: " @ %message);
   }
   else
   {
	   RTBIC_serverSession().writeError("ERROR: " @ %message);
   }
}

RTBIC_SC.addHandle("432","onErroneousNick");
function RTBIC_SC::onErroneousNick(%this,%prefix,%params)
{
	%msg = getWords(%params, 4, getWordCount(%params));
	RTBIC_serverSession().writeError("The name " @ getWord(%params, 1) @ " is not allowed. (" @ %msg @ ")");
}

RTBIC_SC.addHandle("474","onBanned");
function RTBIC_SC::onBanned(%this,%prefix,%params)
{
   RTBIC_serverSession().writeError("You cannot join " @ getWord(%params, 1) @ " because you are banned.");
}

RTBIC_SC.addHandle("NICK", "onNick");
function RTBIC_SC::onNick(%this, %prefix, %params)
{
	%oldName = getSubStr(%prefix, 0, strPos(%prefix, "!"));

	if (%oldName $= $RTB::CIRCClient::Cache::NickName)
	{
	   %text = "You have changed your name to" SPC %params;
	   $RTB::CIRCClient::Cache::NickName = %params;
	}
	else
	{
	   %text = %oldName SPC "changed their name to" SPC %params;
	}

   %session = RTBIC_SessionManager.getSession(%oldName, 1);

   if (%session)
   {
      %session.name = %params;
      %session.window.setValue(%params);
   }

   %count = RTBIC_RoomSessionManager.getCount();

   for (%i = 0; %i < %count; %i++)
   {
      %session = RTBIC_RoomSessionManager.getObject(%i);

      if (%session.manifest.hasUser(%oldName))
      {
         echo(%session);
         %session.writeNotice(%text);

         %session.manifest.removeByID(%oldName);
         %user = %session.manifest.addUserByNick(%params);

         if (!%session.manifest.loading)
         {
            %user.render();
         }
      }
   }
}

RTBIC_SC.addHandle("MODE","onMode");
function RTBIC_SC::onMode(%this,%prefix,%params)
{
	nextToken(%prefix,nick," !");
	%mode = getSubStr(%params, StrLen(getWord(%params,0))+1, StrLen(%params));

	RTBIC_serverSession().writeAction(%nick, "has set mode: "@%mode);
	RTBIC_SC.sendLine("NAMES" SPC getWord(%params, 0));
}

RTBIC_SC.addHandle("PRIVMSG","onMessage");
function RTBIC_SC::onMessage(%this,%prefix,%params)
{
   nextToken(%prefix,username,"!");

   %message = stripMLControlChars(nextToken(%params,destination," :"));
   %message = getSubStr(%params,strPos(%params,":")+1,strLen(%params));
   %message = stripMLControlChars(%message);

   if (%destination $= $RTB::CIRCClient::Cache::NickName)
   {
      if (%username $= "StatServ" || %message $= "VERSION")
      {
         return;
      }
      
      %session = RTBIC_SessionManager.getSession(%username);
      %session.writeMessage(%username, parseLinks(stripMLControlChars(%message)));
   }
   else
   {
      %session = RTBIC_RoomSessionManager.getSession(%destination, 1);
      %session.receive(%username, parseLinks(stripMLControlChars(%message)));
   }
}

RTBIC_SC.addHandle("303","onPresenceQueryReply");
function RTBIC_SC::onPresenceQueryReply(%this,%prefix,%params)
{
   %name = firstWord(nextToken(%params,prefix,":"));
   if(%name $= $RTB::CIRCClient::Cache::NickName)
   {
      if($RTB::Options::IC::CustomUsername && $RTB::Options::IC::CustomUser !$= "")
      {
         %this.sendLine("NICK "@$RTB::Options::IC::CustomUser);
         if($RTB::Options::IC::CustomPass !$= "")
            %this.sendLine("PRIVMSG NickServ IDENTIFY "@$RTB::Options::IC::CustomPass);
      }
      else
      {
         if($RTB::CIRCClient::Cache::DesNickName !$= "")
	         %this.sendLine("NICK "@$RTB::CIRCClient::Cache::DesNickName);
      }

      %this.sendLine("JOIN #rtb2");
   }
}

RTBIC_SC.addHandle("353","onNameList");
function RTBIC_SC::onNameList(%this,%prefix,%params)
{
   %room = nextToken(%params, "", " :");
   nextToken(%room, "room", " =");

   %session = RTBIC_RoomSessionManager.getSession(%room, 1);

   if (!%session)
   {
      return;
   }

   if (!%session.manifest.loading)
   {
      %session.manifest.loading = 1;
      %session.manifest.deleteAll();
   }

   %names = nextToken(%params, "", ":");
   %count = getWordCount(%names);

   for (%i = 0; %i < %count; %i++)
   {
      %name = getWord(%names, %i);
      %session.manifest.addUserByNick(%name);
   }
}

RTBIC_SC.addHandle("311","onWhoisPartA");
function RTBIC_SC::onWhoisPartA(%this,%prefix,%params)
{
   %username = getWord(%params,1);
   %nick = getWord(%params,2);
   %host = getWord(%params,3);
   %realname = nextToken(%params,prefix,":");
   
   RTBIC_serverSession().writeNotice("\c4"@%username@" is "@%nick@"@"@%host@" * "@%realname);
}

RTBIC_SC.addHandle("307","onWhoisPartB");
function RTBIC_SC::onWhoisPartB(%this,%prefix,%params)
{
   %username = getWord(%params,1);
   
   RTBIC_serverSession().writeNotice("\c4"@%username@" is a registered nick");
}

RTBIC_SC.addHandle("319","onWhoisPartC");
function RTBIC_SC::onWhoisPartC(%this,%prefix,%params)
{
   %username = getWord(%params,1);
   %channels = nextToken(%params,%prefix,":");
   
   RTBIC_serverSession().writeNotice("\c4"@%username@" on "@%channels);
}

RTBIC_SC.addHandle("312","onWhoisPartD");
function RTBIC_SC::onWhoisPartD(%this,%prefix,%params)
{
   %username = getWord(%params,1);
   %server = getWord(%params,2);
   %servername = nextToken(%params,%prefix,":");
   
   RTBIC_serverSession().writeNotice("\c4"@%username@" using "@%server@" "@%servername);
}

RTBIC_SC.addHandle("KICK", "onKick");
function RTBIC_SC::onKick(%this, %prefix, %params)
{
	nextToken(%prefix, "kicker", "!");

   %room = getWord(%params, 0);
   %session = RTBIC_RoomSessionManager.getSession(%room, 1);

   if (!%session)
   {
      return;
   }

	%message = nextToken(%params,kickee,":");
	%kickee = getWord(%params,1);
	
	if(%kickee $= $RTB::CIRCClient::Cache::NickName)
	{
      if(%message $= %kicker)
         %text = "You have been kicked by "@%kicker@". (No Reason)";
      else
         %text = "You have been kicked by "@%kicker@". ("@%message@")";
	}
	else
	{
	   if(%message $= %kicker)
   		%text = ""@%kickee@" has been kicked by "@%kicker@". (No Reason)";
	   else
		   %text = ""@%kickee@" has been kicked by "@%kicker@". ("@%message@")";	
	}

   %session.writeError(%text);
}

RTBIC_SC.addHandle("433", "onNickInUse");
function RTBIC_SC::onNickInUse(%this, %prefix, %params)
{
   RTBIC_serverSession().writeError("The name " @ getWord(%params, 1) @ " is already in use.");
}

RTBIC_SC.addHandle("366", "onNameListDone");
function RTBIC_SC::onNameListDone(%this, %prefix, %params)
{
   %room = getWord(%params, 1);

   if (%room $= "*")
   {
      return;
   }

   %session = RTBIC_RoomSessionManager.getSession(%room, 1);

   if (!%session)
   {
      return;
   }

   %session.manifest.loading = 0;
   %session.manifest.render();
}

RTBIC_SC.addHandle("PING", "onPing");
function RTBIC_SC::onPing(%this, %prefix, %params)
{
   %this.sendLine("PONG" SPC %params);
}

RTBIC_SC.addHandle("376","onMOTDEnd");
function RTBIC_SC::onMOTDEnd(%this,%prefix,%params)
{
   %this.sendLine("ISON " @ $RTB::CIRCClient::Cache::NickName);
}

RTBIC_SC.addHandle("422","onMOTDMissing");
function RTBIC_SC::onMOTDMissing(%this,%prefix,%params)
{
   %this.sendLine("ISON "@ $RTB::CIRCClient::Cache::NickName);
}

function RTBIC_serverSession()
{
   return RTBIC_SessionManager.getSession(RTBIC_SC.site);
}

if (!isObject(BAM_IRC))
{
   new TCPObject(BAM_IRC)
   {
      site = "irc.centralchat.net";
      port = 6667;

      connected = 0;
   };

   BAM_IRC.connect();
}