using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class NET_SV_Movable : MonoBehaviour {

    public List<NET_ActorState.Message> buffer = new List<NET_ActorState.Message>();

    public void AddStates(List<NET_ActorState.Message> states)
    {
        buffer.AddRange(states);
    }

    public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        int numMsgs = buffer.Count;
        if (0 < numMsgs)
        {
            stream.Serialize(ref numMsgs);
            foreach (NET_ActorState.Message msg in buffer)
            {
                NET_ActorState.Message.Serialize(stream, msg);
            }
            buffer.Clear();
        }
    }
	
}
