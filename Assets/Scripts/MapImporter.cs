using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class MapImporter : MonoBehaviour
{
    private string filePath;
    private Random rng = new Random();

    private void Awake()
    {
        string path = EditorUtility.OpenFilePanel("Load YAML file...", "", "yml");

        if (path.Length != 0)
        {
            StringReader input = new StringReader(File.ReadAllText(path));

            IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .WithTagMapping("!ul", typeof(ulong))
                .WithTagMapping("!u", typeof(long))
                .Build();

            dynamic order = deserializer.Deserialize<dynamic>(input);

            foreach (dynamic actor in order["Actors"])
            {
                string actorName = (string)actor["Gyaml"];
                GameObject plane = new GameObject();

                GameObject modelFile = (GameObject)Resources.Load("Models" + "/" + actorName + "/" + actorName, typeof(GameObject));

                ActorClass actorClass;

                if (Type.GetType(actorName) != null)
                {
                    actorClass = (ActorClass)plane.AddComponent(Type.GetType(actorName));
                }
                else
                {
                    actorClass = plane.AddComponent<ActorClass>();
                }

                actorClass.ActorData = actor;

                if (modelFile != null)
                {
                    List<Mesh> meshes = new List<Mesh>();

                    foreach (SkinnedMeshRenderer meshRenderer in modelFile.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        meshes.Add(meshRenderer.sharedMesh);
                    }

                    actorClass.ActorMeshes = meshes;
                }

                plane.transform.position = new Vector3(float.Parse(actor["Translate"][0]), float.Parse(actor["Translate"][1]), float.Parse(actor["Translate"][2]));

                if (actor.ContainsKey("Rotate"))
                {
                    actorClass.Rotate.x = Mathf.Rad2Deg * float.Parse(actorClass.ActorData["Rotate"][0]);
                    actorClass.Rotate.y = Mathf.Rad2Deg * float.Parse(actorClass.ActorData["Rotate"][1]);
                    actorClass.Rotate.z = Mathf.Rad2Deg * float.Parse(actorClass.ActorData["Rotate"][2]);
                }

                plane.name = actorClass.ActorData["Gyaml"];
            }

            filePath = order["FilePath"];
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveActors();
        }
    }

    private void SaveActors()
    {
        ActorClass[] actors = FindObjectsOfType<ActorClass>();
        Dictionary<string, dynamic> yamlFile = new Dictionary<string, dynamic>();

        List<dynamic> allActors = new List<dynamic>();

        foreach (ActorClass actorClass in actors)
        {
            Vector3 position = actorClass.transform.position;
            actorClass.ActorData["Translate"][0] = position.x;
            actorClass.ActorData["Translate"][1] = position.y;
            actorClass.ActorData["Translate"][2] = position.z;

            if (actorClass.ActorData.ContainsKey("Rotate"))
            {
                actorClass.ActorData["Rotate"][0] = Mathf.Deg2Rad * actorClass.Rotate.x;
                actorClass.ActorData["Rotate"][1] = Mathf.Deg2Rad * actorClass.Rotate.y;
                actorClass.ActorData["Rotate"][2] = Mathf.Deg2Rad * actorClass.Rotate.z;
            }

            actorClass.ActorData["Gyaml"] = actorClass.gameObject.name;

            allActors.Add(actorClass.ActorData);
        }

        allActors.Reverse();

        yamlFile.Add("Actors", allActors);
        yamlFile.Add("FilePath", filePath);

        ISerializer serializer = new SerializerBuilder()
            .WithTypeConverter(new LongConverter())
            .WithTypeConverter(new ULongConverter())
            .WithTypeConverter(new DoubleConverter())
            .WithTypeConverter(new FloatConverter())
            .Build();

        string yaml = serializer.Serialize(yamlFile);

        string finalFilePath = Application.dataPath + "/converted.yml";
        
        File.WriteAllText(finalFilePath, yaml);
        
        Process.Start(finalFilePath);

        Debug.Log("Wrote converted.yml!");
    }
}

public class LongConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(Int64);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        Scalar scalar = (Scalar)parser.Current;
        Debug.Log(scalar.Value);
        parser.MoveNext();
        return double.Parse(scalar.Value);
    }

    void IYamlTypeConverter.WriteYaml(IEmitter emitter, object value, Type type)
    {
        long bytes = (Int64)value;
        emitter.Emit(new Scalar(
            null,
            "!u",
            "0x" + bytes.ToString("X"),
            ScalarStyle.Plain,
            false,
            false
        ));
    }
}

public class ULongConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(UInt64);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        Scalar scalar = (Scalar)parser.Current;
        byte[] bytes = Convert.FromBase64String(scalar.Value);
        parser.MoveNext();
        return bytes;
    }

    void IYamlTypeConverter.WriteYaml(IEmitter emitter, object value, Type type)
    {
        ulong bytes = (UInt64)value;
        emitter.Emit(new Scalar(
            null,
            "!ul",
            bytes.ToString(),
            ScalarStyle.Plain,
            false,
            false
        ));
    }
}

public class FloatConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(float);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        Scalar scalar = (Scalar)parser.Current;
        Debug.Log(scalar.Value);
        parser.MoveNext();
        return float.Parse(scalar.Value);
    }

    void IYamlTypeConverter.WriteYaml(IEmitter emitter, object value, Type type)
    {
        float bytes = (float)value;

        emitter.Emit(new Scalar(
            null,
            null,
            bytes.ToString().Contains(".") ? bytes.ToString() : bytes + ".0",
            ScalarStyle.Plain,
            true,
            true
        ));
    }
}

public class DoubleConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(double);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        Scalar scalar = (Scalar)parser.Current;
        Debug.Log(scalar.Value);
        parser.MoveNext();
        return double.Parse(scalar.Value);
    }

    void IYamlTypeConverter.WriteYaml(IEmitter emitter, object value, Type type)
    {
        double bytes = (double)value;

        emitter.Emit(new Scalar(
            null,
            null,
            bytes.ToString().Contains(".") ? bytes.ToString() : bytes + ".0",
            ScalarStyle.Plain,
            true,
            true
        ));
    }
}