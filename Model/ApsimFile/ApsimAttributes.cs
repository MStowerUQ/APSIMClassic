﻿using System;


namespace ModelAttributes
{

    /// <summary>
    /// These classes define the attributes used to provide metadata for the 
    /// APSIM Component properties and events.
    /// </summary>

    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandler : Attribute
    {
        private string _EventName;

        public EventHandler()
        {
            _EventName = "";
        }

        public EventHandler(string Name)
        {
            _EventName = Name;
        }
        public string EventName
        {
            get { return _EventName; }
            set { _EventName = EventName; }
        }
    }

    [AttributeUsage(AttributeTargets.Event)]
    public class Event : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class Param : Attribute
    {
        private string _Name = "";
        private bool _Optional = false;
        private double _MinVal = Double.NaN;
        private double _MaxVal = Double.NaN;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public bool IsOptional
        {
            get { return _Optional; }
            set { _Optional = value; }
        }

        public double MinVal
        {
            get { return _MinVal; }
            set { _MinVal = value; }
        }

        public double MaxVal
        {
            get { return _MaxVal; }
            set { _MaxVal = value; }
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class Input : Attribute
    {
        private bool Optional = false;

        public bool IsOptional
        {
            get { return Optional; }
            set { Optional = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class Output : Attribute
    {
        private bool _Immutable = false;
        public String Name;
        public Output()
        {
            Name = "";
        }
        public Output(String sName)
        {
            Name = sName;
        }

        public bool Immutable { get { return _Immutable; } set { _Immutable = value; } }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class Writable : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class Units : Attribute
    {
        private String St;
        public Units(String Units)
        {
            St = Units;
        }
        public override String ToString()
        {
            return St;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class Description : Attribute
    {
        private String St;

        public Description(String Description)
        {
            St = Description;
        }

        public override String ToString()
        {
            return St;
        }
    }



    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class Model : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class Link : Attribute
    {
        private String _Path = null;
        private bool _IsOptional = false;

        //public Link() { }
        //public Link(string NamePath, bool IsOptional) { _Path = NamePath; _IsOptional = IsOptional; }
        public string NamePath
        {
            get { return _Path; }
            set { _Path = value; }
        }
        public bool IsOptional
        {
            get { return _IsOptional; }
            set { _IsOptional = value; }
        }

    }


    /// <summary>
    /// This attribute is used in DotNetProxies.cs to provide a map between the 
    /// proxy class and the component type.
    /// Usage: [ComponentType("Plant2")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ComponentTypeAttribute : Attribute
    {
        public String ComponentClass;
        public ComponentTypeAttribute(String CompClass)
        {
            ComponentClass = CompClass;
        }

    }

    /// <summary>
    /// A user interface attribute to instruct the UI to allow for a large amount of text.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UILargeText : Attribute
    {
    }

    /// <summary>
    /// A user interface attribute to instruct the UI to ignore this field/property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UIIgnore : Attribute
    {
    }

}
