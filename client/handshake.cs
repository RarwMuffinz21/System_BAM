function clientCmdBAM_SendRevision(%revision)
{
    if (isObject(ServerConnection) && !ServerConnection.bamServer)
    {
        ServerConnection.bamServer = 1;
        ServerConnection.bamRevision = mFloor(%revision);

        echo("Server is running BAM r" @ ServerConnection.bamRevision);
        
        commandToServer('BAM_ClientProp', "steamId", getSteamId());
        commandToServer('BAM_ClientProp', "downloadMusic", $Pref::Net::DownloadMusic);
        commandToServer('BAM_ClientProp', "downloadSounds", $Pref::Net::DownloadSounds);
        commandToServer('BAM_ClientProp', "downloadTextures", $Pref::Net::DownloadTextures);
        commandToServer('BAM_ClientProp', "defaultFov", $Pref::Player::DefaultFov);
        commandToServer('BAM_ClientProp', "shaderQuality", $Pref::ShaderQuality);
    }
}

package BAMC_HandshakePackage
{
    function GameConnection::setConnectArgs(%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p)
    {
        Parent::setConnectArgs(%a,%b,%c,%d,%e,%f,%g,%h,$BAM::Revision,%j,%k,%l,%m,%n,%o,%p);
    }

    function OptionsDlg::updateFov(%this)
    {
        %old = $Pref::Player::DefaultFov;
        Parent::updateFov(%this);

        if (ServerConnection.bamServer && $Pref::Player::DefaultFov != %old)
        {
            if (isEventPending(%this.sendDefaultFov))
            {
                cancel(%this.sendDefaultFov);
            }

            %this.sendDefaultFov = schedule(
                250, 0, commandToServer,
                'BAM_ClientProp', "defaultFov",
                $Pref::Player::DefaultFov
            );
        }
    }

    function OptionsDlg::setShaderQuality(%this, %quality)
    {
        %old = $Pref::ShaderQuality;
        Parent::setShaderQuality(%this, %quality);

        // Not using %quality because it may not be available
        if (ServerConnection.bamServer && %old != $Pref::ShaderQuality)
        {
            commandToServer('BAM_ClientProp', "shaderQuality", $Pref::ShaderQuality);
        }
    }
};

activatePackage("BAMC_HandshakePackage");