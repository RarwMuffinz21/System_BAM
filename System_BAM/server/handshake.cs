package BAM_ServerHandshakePackage
{
    function GameConnection::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o, %p)
    {
        if (%h !$= "")
        {
            %this.bamClient = 1;
            %this.bamRevision = mFloor(%h);
        }

        Parent::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o, %p);
    }

    function GameConnection::autoAdminCheck(%this)
	{
		%parent = Parent::autoAdminCheck(%this);

		if (%this.bamClient)
		{
			commandToClient(%this, 'BAM_SendRevision', $RTB::Revision);
		}

		return %parent;
	}
};

activatePackage("BAM_ServerHandshakePackage");