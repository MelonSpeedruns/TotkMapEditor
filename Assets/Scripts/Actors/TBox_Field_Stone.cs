using System.Collections.Generic;
using UnityEngine;

public class TBox_Field_Stone : ActorClass
{
    [SerializeField] private string contents;

    public override void Start()
    {
        base.Start();

        if (ActorData.ContainsKey("Dynamic"))
        {
            contents = ActorData["Dynamic"]["Drop__DropActor"];
        }
        else
        {
            ActorData.Add("Dynamic", new Dictionary<dynamic, dynamic>());
            ActorData["Dynamic"].Add("Drop__DropActor", contents);
        }
    }

    public override void Update()
    {
        ActorData["Dynamic"]["Drop__DropActor"] = contents;
        base.Update();
    }
}