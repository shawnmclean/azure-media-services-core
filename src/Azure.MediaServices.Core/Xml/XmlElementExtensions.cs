using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Azure.MediaServices.Core.Xml
{
  internal static class XmlElementExtensions
  {
    private const long DefaultLongAttributeValue = 0L;

    private static readonly TimeSpan DefaultTimeSpanAttributeValue = TimeSpan.Zero;

    internal static string GetAttributeOrDefault(this XElement element, XName name)
    {
      string attributeValue = null;

      var attribute = element.Attribute(name);
      if (attribute != null)
      {
        attributeValue = attribute.Value;
      }

      return attributeValue;
    }

    internal static long GetAttributeAsLongOrDefault(this XElement element, XName name)
    {
      string attributeValueString = element.GetAttributeOrDefault(name);

      if (!long.TryParse(attributeValueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var attributeValue))
      {
        attributeValue = DefaultLongAttributeValue;
      }

      return attributeValue;
    }

    internal static TimeSpan GetAttributeAsTimeSpanOrDefault(this XElement element, XName name)
    {
      var attributeValueString = element.GetAttributeOrDefault(name);

      TimeSpan attributeValue;
      try
      {
        attributeValue = XmlConvert.ToTimeSpan(attributeValueString);
      }
      catch (FormatException)
      {
        attributeValue = DefaultTimeSpanAttributeValue;
      }

      return attributeValue;
    }
  }
}
