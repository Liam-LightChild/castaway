using System;
using System.Text;
using System.Xml;

namespace Castaway.Assets
{
    public class XMLAssetType : IAssetType
    {
        public T To<T>(Asset a)
        {
            if (typeof(T) == typeof(string))
                return (T) (dynamic) Encoding.UTF8.GetString(a.GetBytes());
            if (typeof(T) == typeof(XmlDocument))
            {
                var d = new XmlDocument();
                d.LoadXml(To<string>(a));
                return (T) (dynamic) d;
            } 
            throw new InvalidOperationException($"Cannot convert XMLAssetType to {typeof(T).FullName}");
        }
    }
}