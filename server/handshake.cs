function serverCmdBAM_ClientProp(%client, %property, %value)
{
    %client.bamProperty[%property] = %value;
    %client.bamSentProperty[%property] = 1;

    %client.onClientPropChanged(%property, %value);
}

function GameConnection::setDefaultClientProps(%this)
{
    %this.bamProperty["defaultFov"] = 90;
}

function GameConnection::onClientPropChanged(%this, %property, %value)
{
    // Callback for add-ons to package.
}

package BAMS_HandshakePackage
{
    function GameConnection::onConnectRequest(%this,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p)
    {
        %this.setDefaultClientProps();

        if (%i !$= "")
        {
            %this.bamClient = 1;
            %this.bamRevision = mFloor(%i);
        }

        Parent::onConnectRequest(%this,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p);
    }

    function GameConnection::autoAdminCheck(%this)
    {
        %parent = Parent::autoAdminCheck(%this);

        if (%this.bamClient)
        {
            echo(" +- running BAM r" @ %this.bamRevision);
            commandToClient(%this, 'BAM_SendRevision', $BAM::Revision);
        }

        return %parent;
    }
};

activatePackage("BAMS_HandshakePackage");