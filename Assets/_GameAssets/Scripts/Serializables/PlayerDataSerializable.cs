using System;
using Unity.Netcode;
using UnityEngine;

public struct PlayerDataSerializable : INetworkSerializeByMemcpy, IEquatable<PlayerDataSerializable>
{
    public ulong CliendId;
    public int ColorId;

    public PlayerDataSerializable(ulong cliendId, int colorId)
    {
        CliendId = cliendId;
        ColorId = colorId;
    }

    public bool Equals(PlayerDataSerializable other)
    {
        return CliendId == other.CliendId && ColorId == other.ColorId;
    }
}
