$RTB::Path = "Add-Ons/System_BAM/client/transitional/";

// Ripped and rewritten to support IRC sessions
function RTBCO_getPref(%varName)
{
    return $RTB::Options["::" @ %varName];
}