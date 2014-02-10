new GuiSwatchCtrl(BAM_Overlay)
{
    visible = 1;
    extent = "640 480";
    color = "0 0 0 0";
};

function BAM_Overlay::onWake(%this)
{
    GlobalActionMap.bindCmd("keyboard", "escape", "BAM_Overlay.escapeOverlay();", "");

    %this.fadeTick(0.75);
    %this.invokeMethod("onWake");

    %res = getRes();
    %count = %this.getCount();

    %resX = getWord(%res, 0);
    %resY = getWord(%res, 1);

    for (%i = 0; %i < %count; %i++)
    {
        %control = %this.getObject(%i);

        if (%control.getClassName() !$= "GuiWindowCtrl")
        {
            continue;
        }

        %extX = mClamp(getWord(%control.extent, 0), 0, %resX);
        %extY = mClamp(getWord(%control.extent, 1), 0, %resY);
        %posX = mClamp(getWord(%control.position, 0), 0, %resX - %extX);
        %posY = mClamp(getWord(%control.position, 1), 0, %resY - %extY);

        %control.resize(%posX, %posY, %extX, %extY);
    }
}

function BAM_Overlay::onSleep(%this)
{
    %this.setColor("0 0 0 0");
    %this.invokeMethod("onSleep");

    cancel(%this.fadeTick);
    GlobalActionMap.unbind("keyboard", "escape");
}

function BAM_Overlay::fadeTick(%this, %target)
{
    cancel(%this.fadeTick);

    %speed = 0.06;
    %target = mClampF(%target, 0, 1);

    %opacity = getWord(%this.getColor(), 3);
    %opacity += mClampF(%target - %opacity, -%speed, %speed);

    %this.setColor("0 0 0" SPC %opacity);

    if (%opacity <= 0)
    {
        Canvas.popDialog(%this);
        return;
    }

    %this.fadeTick = %this.schedule(16, "fadeTick", %target);
}

function BAM_Overlay::toggleOverlay(%this)
{
    if (%this.isAwake())
    {
        %this.fadeTick(0);
    }
    else
    {
        Canvas.pushDialog(%this);
    }
}

function BAM_Overlay::escapeOverlay(%this)
{
    %this.fadeTick(0);
}

function BAM_Overlay::invokeMethod(%this, %method)
{
    %count = %this.getCount();

    for (%i = 0; %i < %count; %i++)
    {
        %control = %this.getObject(%i);
        %name = %control.getName();

        if (%name !$= "" && isFunction(%name, %method))
        {
            eval("%control." @ %method @ "();");
        }
    }
}

function BAM_Overlay::toggleControl(%this, %control)
{
    if (!isObject(%control))
    {
        error("ERROR: Control does not exist!");
        return 0;
    }

    if (%this.isMember(%control))
    {
        %this.remove(%control);
        return 0;
    }

    %this.add(%control);
    return 1;
}

GlobalActionMap.bindCmd("keyboard", "ctrl tab", "BAM_Overlay.toggleOverlay();", "");