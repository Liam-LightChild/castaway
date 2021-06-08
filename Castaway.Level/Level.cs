#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Castaway.Assets;
using Castaway.Math;

namespace Castaway.Level
{
    public class Level
    {
        private readonly List<LevelObject> _objects = new();
        
        public Level() {}

        public Level(Asset asset)
        {
            var doc = asset.Type.To<XmlDocument>(asset);
            var root = doc.DocumentElement;

            const string api = "OpenGL"; // TODO

            var node = root!.FirstChild;
            do
            {
                if(node == null) break;

                switch (node.Name)
                {
                    case "Object":
                    {
                        _objects.Add(ParseObject((node as XmlElement)!, api));
                        break;
                    }
                }
            }
            while ((node = node!.NextSibling) != null);
        }

        private LevelObject ParseObject(XmlElement e, string? api, LevelObject? parent = null)
        {
            LevelObject o = new();
            var subs = e.GetElementsByTagName("Object");
            var conts = e["Controllers"]?.ChildNodes;
            for (var i = 0; i < (conts?.Count ?? 0); i++)
                o.Controllers.Add(ParseController((conts![i] as XmlElement)!, api));
            o.Name = e["Name"]?.InnerText ?? throw new InvalidOperationException("All objects need unique names.");
            if (_objects.Any(obj => obj.Name == o.Name))
                throw new InvalidOperationException("All objects need *unique* names.");
            o.Position = (Vector3) Load(typeof(Vector3), e["Position"]?.InnerText ?? "0,0,0");
            o.Scale = (Vector3) Load(typeof(Vector3), e["Scale"]?.InnerText ?? "1,1,1");
            o.Parent = parent;
            for (var i = 0; i < subs.Count; i++) 
                o.Subobjects.Add(ParseObject((subs[i] as XmlElement)!, api, o));
            return o;
        }

        private EmptyController ParseController(XmlElement e, string? api)
        {
            var t = ((((Type.GetType(e.Name) 
                        ?? Type.GetType($"Castaway.Level.{e.Name}Controller"))
                       ?? Type.GetType($"Castaway.Level.{e.Name}"))
                      ?? Type.GetType($"Castaway.Level.{api ?? "<ERROR>"}.{e.Name}Controller")) 
                     ?? Type.GetType($"Castaway.Level.{api ?? "<ERROR>"}.{e.Name}"))
                    ?? throw new InvalidOperationException($"Couldn't find controller {e.Name}.");
            var inst = t!.GetConstructor(Array.Empty<Type>())!.Invoke(null);

            foreach (var on in e.ChildNodes)
            {
                var n = on as XmlNode;
                var f = t.GetFields().Single(field =>
                {
                    var a = field.GetCustomAttribute<LevelSerializedAttribute>();
                    if (a == null) return false;
                    return a.Name == n!.Name;
                });
                f.SetValue(inst, Load(f.FieldType, n!.InnerText));
            }
            
            return (inst as EmptyController)!;
        }

        private static object Load(Type t, string v)
        {
            if (t == typeof(int)) return int.Parse(v);
            if (t == typeof(uint)) return uint.Parse(v);
            if (t == typeof(long)) return long.Parse(v);
            if (t == typeof(ulong)) return ulong.Parse(v);
            if (t == typeof(byte)) return byte.Parse(v);
            if (t == typeof(sbyte)) return sbyte.Parse(v);
            if (t == typeof(short)) return short.Parse(v);
            if (t == typeof(ushort)) return ushort.Parse(v);
            if (t == typeof(float)) return float.Parse(v);
            if (t == typeof(double)) return double.Parse(v);
            if (t == typeof(string)) return v;
            if (t == typeof(Vector2))
            {
                var p = v.Split(',');
                return new Vector2(
                    float.Parse(p[0]), 
                    float.Parse(p[1]));
            }
            if (t == typeof(Vector3))
            {
                var p = v.Split(',');
                return new Vector3(
                    float.Parse(p[0]), 
                    float.Parse(p[1]),
                    float.Parse(p[2]));
            }
            if (t == typeof(Vector4))
            {
                var p = v.Split(',');
                return new Vector4(
                    float.Parse(p[0]), 
                    float.Parse(p[1]),
                    float.Parse(p[2]),
                    float.Parse(p[3]));
            }
            if (t == typeof(Asset)) return AssetLoader.Loader!.GetAssetByName(v);
            if (t.IsSubclassOf(typeof(Enum))) return Enum.Parse(t, v);
            
            throw new InvalidOperationException($"Cannot load {t.FullName} from levels.");
        }

        public void Start()
        {
            foreach(var o in _objects) o.OnInit();
        }

        public void Render()
        {
            foreach(var o in _objects) o.OnRender();
        }

        public void Update()
        {
            foreach(var o in _objects) o.OnUpdate();
        }

        public void End()
        {
            foreach(var o in _objects) o.OnDestroy();
        }

        public void Add(LevelObject obj) => _objects.Add(obj);
        public LevelObject Get(string name) => _objects.Single(o => o.Name == name);
        public LevelObject this[string i] => Get(i);
    }
}