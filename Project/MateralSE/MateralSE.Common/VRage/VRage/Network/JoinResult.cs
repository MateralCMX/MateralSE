namespace VRage.Network
{
    using System;

    public enum JoinResult
    {
        OK,
        AlreadyJoined,
        TicketInvalid,
        SteamServersOffline,
        NotInGroup,
        GroupIdInvalid,
        ServerFull,
        BannedByAdmins,
        KickedRecently,
        TicketCanceled,
        TicketAlreadyUsed,
        LoggedInElseWhere,
        NoLicenseOrExpired,
        UserNotConnected,
        VACBanned,
        VACCheckTimedOut,
        PasswordRequired,
        WrongPassword,
        ExperimentalMode
    }
}

