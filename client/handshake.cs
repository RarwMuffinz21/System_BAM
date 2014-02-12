function clientCmdBAM_SendRevision(%revision)
{
    if (isObject(ServerConnection) && !ServerConnection.bamServer)
    {
        ServerConnection.bamServer = 1;
        ServerConnection.bamRevision = mFloor(%revision);

        echo("Server is running BAM r" @ ServerConnection.bamRevision);
    }
}

package BAMC_HandshakePackage
{
    function GameConnection::setConnectArgs(%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p)
    {
        Parent::setConnectArgs(%a,%b,%c,%d,%e,%f,%g,%h,$BAM::Revision,%j,%k,%l,%m,%n,%o,%p);
    }
};

activatePackage("BAMC_HandshakePackage");