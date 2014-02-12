package BAMS_HandshakePackage
{
    function GameConnection::onConnectRequest(%this,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p)
    {
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