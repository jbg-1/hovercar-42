using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public enum AuthorityMode
{
    Server,
    Client
}

public class ClientNetworkTransform : NetworkTransform
{
    public AuthorityMode authorityMode = AuthorityMode.Server;

    protected override bool OnIsServerAuthoritative()
    {
        return authorityMode == AuthorityMode.Server;
    }
}
