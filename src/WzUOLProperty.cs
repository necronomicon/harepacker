﻿#define UOLRES

using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

public class WzUOLProperty : WzExtended {
  #region Fields
  internal string name, val;
  internal WzObject parent;
  //internal WzImage imgParent;
  internal WzObject linkVal;
  #endregion

  #region Inherited Members
      public override void SetValue(object value)
      {
          val = (string)value;
      }

      public override WzImageProperty DeepClone()
      {
          WzUOLProperty clone = new WzUOLProperty(name, val);
          clone.linkVal = null;
          return clone;
      }

  public override object WzValue
      {
          get
          {
#if UOLRES
              return LinkValue;
#else
              return this;
#endif
          }
      }
  /// <summary>
  /// The parent of the object
  /// </summary>
  public override WzObject Parent { get { return parent; } internal set { parent = value; } }

  /*/// <summary>
  /// The image that this property is contained in
  /// </summary>
  public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/

  /// <summary>
  /// The name of the property
  /// </summary>
  public override string Name { get { return name; } set { name = value; } }

#if UOLRES
  public override List<WzImageProperty> WzProperties
  {
    get
    {
             return LinkValue is WzImageProperty ? ((WzImageProperty)LinkValue).WzProperties : null;
    }
  }


      public override WzImageProperty this[string name]
  {
    get
    {

              return LinkValue is WzImageProperty ? ((WzImageProperty)LinkValue)[name] : LinkValue is WzImage ? ((WzImage)LinkValue)[name] : null;
    }
  }

  public override WzImageProperty GetFromPath(string path)
  {
          return LinkValue is WzImageProperty ? ((WzImageProperty)LinkValue).GetFromPath(path) : LinkValue is WzImage ? ((WzImage)LinkValue).GetFromPath(path) : null;
  }
#endif

  /// <summary>
  /// The WzPropertyType of the property
  /// </summary>
  public override WzPropertyType PropertyType { get { return WzPropertyType.UOL; } }

  public override void WriteValue(WzBinaryWriter writer)
  {
    writer.WriteStringValue("UOL", 0x73, 0x1B);
    writer.Write((byte)0);
    writer.WriteStringValue(Value, 0, 1);
  }

  public override void ExportXml(StreamWriter writer, int level)
  {
    writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedValuePair("WzUOL", this.Name, this.Value));
  }

  /// <summary>
  /// Disposes the object
  /// </summary>
  public override void Dispose()
  {
    name = null;
    val = null;
  }
  #endregion

  #region Custom Members
  /// <summary>
  /// The value of the property
  /// </summary>
  public string Value { get { return val; } set { val = value; } }

#if UOLRES
      public WzObject LinkValue
  {
    get
    {
      if (linkVal == null)
      {
        string[] paths = val.Split('/');
                  linkVal = (WzObject)this.parent;
                  string asdf = parent.FullPath;
        foreach (string path in paths)
        {
          if (path == "..")
          {
                          linkVal = (WzObject)linkVal.Parent;
          }
          else
          {
                          if (linkVal is WzImageProperty)
                              linkVal = ((WzImageProperty)linkVal)[path];
                          else if (linkVal is WzImage)
                              linkVal = ((WzImage)linkVal)[path];
                          else if (linkVal is WzDirectory)
                              linkVal = ((WzDirectory)linkVal)[path];
                          else
                          {
                              Console.WriteLine("UOL got nexon'd at property: " + this.FullPath);
                              return null;
                          }
          }
        }
      }
      return linkVal;
    }
  }
#endif

  /// <summary>
  /// Creates a blank WzUOLProperty
  /// </summary>
  public WzUOLProperty() { }

  /// <summary>
  /// Creates a WzUOLProperty with the specified name
  /// </summary>
  /// <param name="name">The name of the property</param>
  public WzUOLProperty(string name)
  {
    this.name = name;
  }

  /// <summary>
  /// Creates a WzUOLProperty with the specified name and value
  /// </summary>
  /// <param name="name">The name of the property</param>
  /// <param name="value">The value of the property</param>
  public WzUOLProperty(string name, string value)
  {
    this.name = name;
    this.val = value;
  }
  #endregion

      #region Cast Values
#if UOLRES
      public override int GetInt()
      {
          return LinkValue.GetInt();
      }

      public override short GetShort()
      {
          return LinkValue.GetShort();
      }

      public override long GetLong()
      {
          return LinkValue.GetLong();
      }

      public override float GetFloat()
      {
          return LinkValue.GetFloat();
      }

      public override double GetDouble()
      {
          return LinkValue.GetDouble();
      }

      public override string GetString()
      {
          return LinkValue.GetString();
      }

      public override Point GetPoint()
      {
          return LinkValue.GetPoint();
      }

      public override Bitmap GetBitmap()
      {
          return LinkValue.GetBitmap();
      }

      public override byte[] GetBytes()
      {
          return LinkValue.GetBytes();
      }
#else
      public override string GetString()
      {
          return val;
      }
#endif
      public override string ToString()
      {
          return val;
      }
      #endregion
  }
