using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ActorClass : MonoBehaviour
{
    public dynamic ActorData;

    public Vector3 Rotate;
    
    private Random rng = new Random();

    [HideInInspector] public List<Mesh> ActorMeshes = new List<Mesh>();

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        if (gameObject.name.StartsWith("Enemy_"))
        {
            Gizmos.color = Color.red;
        }
        else if (gameObject.name.StartsWith("Item_Mushroom"))
        {
            Gizmos.color = new Color(1f, 0.6f, 0f, 1f);
        }
        else if (gameObject.name.StartsWith("Obj_Tree"))
        {
            Gizmos.color = Color.green;
        }
        else if (gameObject.name.StartsWith("Animal_"))
        {
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 1f);
        }
        else if (gameObject.name.StartsWith("TBox_"))
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.white;
        }

        if (ActorMeshes.Count > 0)
        {
            foreach (Mesh actorMesh in ActorMeshes)
            {
                Gizmos.DrawMesh(actorMesh, Vector3.zero, Quaternion.identity, transform.localScale);
            }
        }
        else
        {
            Gizmos.DrawCube(Vector3.zero, transform.localScale);
        }
    }
    
    private int NextRandomNumber()
    {
        return rng.Next(0, int.MaxValue);
    }

    public virtual void Start()
    {
        if (ActorData == null)
        {
            ActorData = new Dictionary<dynamic, dynamic>();

            Vector3 position = transform.position;
            ActorData.Add("Translate", new List<float>());
            ActorData["Translate"].Add(position.x);
            ActorData["Translate"].Add(position.y);
            ActorData["Translate"].Add(position.z);

            ActorData.Add("Phive", new Dictionary<dynamic, dynamic>());
            ActorData["Phive"].Add("Placement", new Dictionary<dynamic, dynamic>());

            ulong hash = Convert.ToUInt64(NextRandomNumber());
            long srtHash = NextRandomNumber();

            ActorData["Phive"]["Placement"].Add("ID", hash);

            ActorData.Add("Rotate", new List<float>());
            ActorData["Rotate"].Add(Mathf.Deg2Rad * Rotate.x);
            ActorData["Rotate"].Add(Mathf.Deg2Rad * Rotate.y);
            ActorData["Rotate"].Add(Mathf.Deg2Rad * Rotate.z);

            ActorData.Add("Gyaml", gameObject.name);
            ActorData.Add("SRTHash", srtHash);
            ActorData.Add("Hash", hash);
        }
    }

    public virtual void Update()
    {
        ActorData["Gyaml"] = gameObject.name;
        transform.rotation = Quaternion.Euler(Rotate);
    }
}