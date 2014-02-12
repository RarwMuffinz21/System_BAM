function BAM_IRCSessionGroup::get(%this, %name, %create)
{
    %count = %this.getCount();

    for (%i = 0; %i < %count; %i++)
    {
        %session = %this.getObject(%i);

        if (%session.name $= %name)
        {
            return %session;
        }
    }

    if (!%create)
    {
        return 0;
    }

    %session = new ScriptObject()
    {
        class = BAM_IRCSession;
        name = %name;
    };

    %this.add(%session);
    return %session;
}

if (!isObject(BAM_IRCSessionGroup))
{
    new ScriptGroup(BAM_IRCSessionGroup);
}

function BAM_IRCSession::onAdd(%this)
{
    %window = %this.createWindow();
    %input = BAM_InputRecycler.acquire();

    // We may have run out of input controls
    if (%input)
    {
        %window.getObject(1).add(%input);
        %window.input = %input;

        // Replace this!
        %input.setProfile(RTB_TextEditProfile);
        %input.altCommand = %this @ ".sendMessage();";

        %input.position = "1 3";
        %input.extent = "264 16";

        %input.horizSizing = "width";
        %input.vertSizing = "bottom";
    }
    else
    {
        // Warn the user about the missing input
        %text = "<font:Verdana Bold:12><color:FF0000>";
        %text = %text @ "ERROR: BAM has run out of input controls.";
        %text = %text @ " Close a few sessions.";

        %this.schedule(0, addLine, %text);
    }

    // Find controls
    %window.scroll = %window.getObject(0).getObject(0).getObject(0);
    %window.text = %window.scroll.getObject(0);

    // Determine the best place to put the window
    %count = BAM_IRCSessionGroup.getCount();

    for (%i = 0; %i < 50; %i++)
    {
        %position = %i * 40 SPC %i * 40;

        for (%j = 0; %j < %count; %j++)
        {
            %session = BAM_IRCSessionGroup.getObject(%j);

            if (%session != %this && %session.window.position $= %position)
            {
                break;
            }
        }

        if (%j == %count)
        {
            break;
        }
    }

    %this.window.position = %position;
}

function BAM_IRCSession::onRemove(%this)
{
    if (isObject(%this.window.input))
    {
        // Make sure to give back the input we acquired
        BAM_InputRecycler.release(%this.window.input);
    }

    if (isObject(%this.window))
    {
        %this.window.delete();
    }
}

function BAM_IRCSession::addLine(%this, %line)
{
    %scroll = %this.window.scroll;
    %text = %this.window.text;

    // This needs to be persisted
    %y = getWord(%text.position, 1);

    // Determine this now so future modifications can be done
    %bottom = getWord(%text.extent, 1) + %y <= getWord(%scroll.extent, 1) - 1;
    %value = %text.getValue();

    // Append with \n if text present, set otherwise
    %text.setValue(%value @ (%value $= "" ? "" : "\n") @ %line);

    // ::forceReflow errors for non-awake controls
    if (BAM_Overlay.isAwake())
    {
        %text.forceReflow();
    }

    // Required for ::scrollToBottom to respond correctly
    %text.setCursorPosition(strLen(%text.getValue()));

    if (%bottom)
    {
        %scroll.scrollToBottom();
    }
    else
    {
        %text.resize(
            getWord(%text.position, 0),
            %y,
            getWord(%text.extent, 0),
            getWord(%text.extent, 1)
        );
    }
}

function BAM_IRCSession::sendMessage(%this)
{
    %text = %this.window.input.getValue();

    if (%text $= "")
    {
        return;
    }

    BAM_IRC.sendMessage(%this.name, %text);

    %this.window.input.setValue("");
    %this.window.input.makeFirstResponder(1);

    %this.window.scroll.scrollToBottom();
}

function BAM_IRCSession::createWindow(%this)
{
    // Replace this!
    %this.window = new GuiWindowCtrl()
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
                    extent = "279 132";
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
                command = %this @ ".sendMessage();";
                text = " ";
            };
        };
    };

    BAM_Overlay.add(%this.window);
    return %this.window;
}