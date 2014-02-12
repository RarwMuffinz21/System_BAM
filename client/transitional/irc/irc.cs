function BAM_IRC::connect(%this, %address)
{
    Parent::connect(%this, %this.host @ ":" @ %this.port);
}

// ============================================================================
// TCPObject callback handling
// ============================================================================

function BAM_IRC::onConnected(%this)
{
    %this.connected = 1;
    %this.registered = 0;

    %this.onFlowStart();
}

function BAM_IRC::onDisconnect(%this)
{
    %this.connected = 0;
    %this.registered = 0;
}

function BAM_IRC::onLine(%this, %line)
{
    if (%line $= "")
    {
        return;
    }

    %prefix = %this.host;

    if (getSubStr(%line, 0, 1) $= ":")
    {
        %prefix = firstWord(%line);
        %prefix = getSubStr(%prefix, 1, strLen(%prefix));

        %line = restWords(%line);
    }

    %command = firstWord(%line);
    %params = restWords(%line);

    if (%command $= "PING")
    {
        %this.sendCommand("PONG", %params);

        if (!%this.registered)
        {
            %this.registered = 1;
            %this.onFlowRegister();
        }
    }
    else if (isFunction(%this.getName(), "onCommand" @ %command))
    {
        eval("%this.onCommand" @ %command @ "(%prefix, %params);");
    }
    else if (%this.debug)
    {
        echo("=>" SPC %line);
    }
}

// ============================================================================
// Methods for sending data
// ============================================================================

function BAM_IRC::sendCommand(%this, %command, %params)
{
    if (%params !$= "")
    {
        %params = " " @ %params;
    }

    if (%this.debug)
    {
        echo("<= " @ %command @ %params);
    }

    %this.send(%command @ %params @ "\r\n");
}

// ============================================================================
// General IRC-related functions
// ============================================================================

function BAM_IRC::displayMessage(%this, %session, %from, %text)
{
    echo("::displayMessage from" SPC %from SPC "in session" SPC %session);

    if (%from $= "StatServ" || %from $= "InfoServ")
    {
        return;
    }

    %text = parseLinks(stripMLControlChars(%text));
    %action = firstWord(%text) $= "ACTION" ? restWords(%text) : "";

    if (isRoom(%session))
    {
        %session = RTBIC_RoomSessionManager.getSession(%session, 1);
        %session.receive(%from, %text);
    }
    else
    {
        %session = BAM_IRCSessionGroup.get(%session, 1);

        if (%action $= "")
        {
            %text = "<font:Verdana Bold:12>" @ %from @ "<font:Verdana:12>: " @ %text;
            %session.addLine(%text);
        }
        else
        {
            %text = "<font:Verdana Bold:12><color:CC00CC>* ";
            %text = %text @ %from @ " <font:Verdana:12>";
            %text = %text @ %action @ "<color:444444>";

            %session.addLine(%text);
        }
    }
}

function BAM_IRC::receiveMessage(%this, %type, %text, %from, %to)
{
    echo("::receiveMessage from" SPC %from SPC "to" SPC %to);

    if (isRoom(%to))
    {
        %this.displayMessage(%to, %from, %text);
    }
    else if (%to $= %this.nick)
    {
        %this.displayMessage(%from, %from, %text);
    }
}

function BAM_IRC::sendMessage(%this, %to, %text, %args)
{
    echo("::sendMessage to" SPC %to);

    if (%args $= "" && getSubStr(%text, 0, 1) $= "/")
    {
        %first = firstWord(%text);
        %param = restWords(%text);

        if (%first $= "/me" || %first $= "/action")
        {
            %args = %param;
            %text = "ACTION";
        }
        else if (%first $= "/msg")
        {
            BAM_IRC.sendMessage(firstWord(%param), restWords(%param));
            return;
        }
        else
        {
            BAM_IRC.sendCommand(strUpr(getSubStr(%first, 1, strLen(%first))), %param);
            return;
        }
    }

    // It's a CTCP message
    if (getWordCount(%text) == 1 && %args !$= "")
    {
        %text = "\x01" @ %text SPC %args @ "\x01";
    }

    %this.sendCommand("PRIVMSG", %to SPC ":" @ %text);
    %this.displayMessage(%to, %this.nick, %text);
}

// ============================================================================
// Callbacks for connection flow and IRC commands
// ============================================================================

function BAM_IRC::onFlowStart(%this)
{
    %chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789[]-";

    %temp = "Blockhead" @ getNonsense();
    %real = $Pref::Player::NetName SPC "(BL_ID" SPC getNumKeyID() @ ")";

    %this.nick = %temp;
    %this.nickDesired = filterString(strReplace($Pref::Player::NetName, " ", "_"), %chars);

    %this.sendCommand("NICK", %temp);
    %this.sendCommand("USER", %this.nickDesired SPC "0 * :" @ %real);
}

function BAM_IRC::onFlowRegister(%this)
{
    %this.sendCommand("NICK", %this.nickDesired);
    %this.sendCommand("JOIN", "#rtb2");
}

function BAM_IRC::onCommandNICK(%this, %prefix, %params)
{
    %old = nickFromSource(%prefix);
    %new = longFromParams(%params);

    if (%old $= %this.nick)
    {
        %text = "You have changed your name to" SPC %new;
        %this.nick = %new;
    }
    else
    {
        %text = %old SPC "changed their name to" SPC %new;
    }

    %session = BAM_IRCSessionGroup.getSession(%old);

    if (%session)
    {
        %session.name = %new;
        %session.window.setValue(%new);
    }

    %count = RTBIC_RoomSessionManager.getCount();

    for (%i = 0; %i < %count; %i++)
    {
        %session = RTBIC_RoomSessionManager.getObject(%i);

        if (%session.manifest.hasUser(%old))
        {
            %session.writeNotice(%text);
            %session.manifest.removeByID(%old);

            %user = %session.manifest.addUserByNick(%new);

            if (!%session.manifest.loading)
            {
                %user.render();
            }
        }
    }
}

function BAM_IRC::onCommandJOIN(%this, %prefix, %params)
{
    %nick = nickFromSource(%prefix);
    %room = longFromParams(%params);

    %self = %nick $= %this.nick;
    %session = RTBIC_RoomSessionManager.getSession(%room, !%self);

    if (!%session)
    {
        return;
    }

    %user = %session.manifest.addUserByNick(%nick);

    if (!%session.manifest.loading)
    {
        %user.render();
    }

    if (!%self)
    {
        %session.writeNotice(%nick SPC "has joined the room.");
    }
}

function BAM_IRC::onCommandPART(%this, %prefix, %params)
{
    %nick = nickFromSource(%prefix);
    %room = getWord(%params, 0);

    %session = RTBIC_RoomSessionManager.getSession(%room, 1);

    if (!%session)
    {
        return;
    }

    if (%nick $= %this.nick)
    {
        %session.delete();
    }
    else
    {
        %session.writeNotice(%nick SPC "has left the room.");
        %session.manifest.removeByID(%nick);
    }
}

function BAM_IRC::onCommandQUIT(%this, %prefix, %params)
{
    %nick = nickFromSource(%prefix);
    %count = RTBIC_RoomSessionManager.getCount();

    for (%i = 0; %i < %count; %i++)
    {
        %session = RTBIC_RoomSessionManager.getObject(%i);

        if (%session.manifest.hasUser(%nick))
        {
            %this.onCommandPART(%prefix, "PART :" @ %session.name);
        }
    }
}

function BAM_IRC::onCommandMODE(%this, %prefix, %params)
{
    %nick = nickFromSource(%prefix);

    %subject = firstWord(%params);
    %mode = restWords(%params);

    if (isRoom(%subject))
    {
        %session = RTBIC_RoomSessionManager.getSession(%subject, 1);

        if (%session)
        {
            %session.writeAction(%nick, "set mode" SPC %mode);
        }
    }
    else
    {
        %text = "<font:Verdana Bold:12><color:CC00CC>";
        %text = %text @ %nick @ " <font:Verdana:12>set mode ";
        %text = %text @ %mode @ "<color:444444>";

        %session = BAM_IRCSessionGroup.get(%this.host, 1);
        %session.addLine(%text);
    }

    %this.sendCommand("NAMES", getWord(%params, 0));
}

function BAM_IRC::onCommandPRIVMSG(%this, %prefix, %params)
{
    %from = nickFromSource(%prefix);
    %to = firstWord(%params);

    %text = longFromParams(%params);
    %this.receiveMessage("PRIVMSG", %text, %from, %to);
}

function BAM_IRC::onCommandNOTICE(%this, %prefix, %params)
{
    %from = nickFromSource(%prefix);
    %to = firstWord(%params);

    %text = longFromParams(%params);
    %this.receiveMessage("NOTICE", %text, %from, %to);
}

function BAM_IRC::onCommandERROR(%this, %prefix, %params)
{
    %text = longFromParams(%params);

    if (getSubStr(%params, 0, 1) !$= ":")
    {
        %this.receiveMessage("ERROR", %text, getWord(%params, 0), %this.nick);
    }
    else
    {
        %this.receiveMessage("ERROR", %text, %this.host, %this.nick);
    }
}

function BAM_IRC::onCommand353(%this, %prefix, %params) // RPL_NAMELIST
{
    %room = getWord(%params, 2);
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

    %names = longFromParams(%params);
    %count = getWordCount(%names);

    for (%i = 0; %i < %count; %i++)
    {
        %session.manifest.addUserByNick(getWord(%names, %i));
    }
}

function BAM_IRC::onCommand366(%this, %prefix, %params) // RPL_NAMELISTDONE
{
    %room = getWord(%params, 1);

    if (%room $= "*")
    {
        return;
    }

    %session = RTBIC_RoomSessionManager.getSession(%room, 1);

    if (%session.manifest.loading)
    {
        %session.manifest.loading = 0;
        %session.manifest.render();
    }
}

function BAM_IRC::onCommand433(%this, %prefix, %params) // ERR_NICKINUSE
{
    %text = "<color:FF0000>The nick " @ getWord(%params, 1) @ " is taken.<color:444444>";

    %session = BAM_IRCSessionGroup.get(%this.host, 1);
    %session.addLine(%text);
}

// ============================================================================
// Miscellaneous support functions
// ============================================================================

function nickFromSource(%source)
{
    %index = strPos(%source, "!");

    if (%index == -1)
    {
        return %source;
    }

    return getSubStr(%source, 0, %index);
}

function longFromParams(%params)
{
    %index = strPos(%params, ":");

    if (%index == -1)
    {
        return "";
    }

    return getSubStr(%params, %index + 1, strLen(%params));

    if (getSubStr(%params, 0, 1) $= ":")
    {
        %params = " " @ %params;
    }

    return nextToken(%params, "", " :");
}

function isRoom(%name)
{
    if (%name $= "")
    {
        return 0;
    }

    %first = getSubStr(%name, 0, 1);
    return %first $= "#" || %first $= "&";
}

// ============================================================================
// Runtime instructions
// ============================================================================

exec("./InputRecycler.cs");
exec("./RoomSession.cs");
exec("./RoomSessionManager.cs");
exec("./RoomSessionManifest.cs");
exec("./Session.cs");

if (!isObject(RTBIC_RoomSessionManager))
{
   RTBIC_createRoomSessionManager();
}

if (!isObject(BAM_IRC))
{
    new TCPObject(BAM_IRC)
    {
        host = "irc.voltirc.com";
        port = 6667;

        connected = 0;
        registered = 0;
    };

    BAM_IRC.connect();
}