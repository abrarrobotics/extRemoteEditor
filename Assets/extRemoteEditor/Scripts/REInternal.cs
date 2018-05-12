﻿/* Copyright (c) 2018 ExT (V.Sigalkin) */

using UnityEngine;

using System;
using System.Reflection;
using System.Collections.Generic;

using extOSC;
using extOSC.Serialization;

namespace extRemoteEditor
{
    public class REItem
    {
        #region Public Vars

        public string Name;

        public int InstanceId;

        public REObject Parent;

        #endregion
    }

    public class REObject : REItem
    {
        #region Public Vars

        public GameObject Target;

        public List<REObject> Childs = new List<REObject>();

        public List<REComponent> Components = new List<REComponent>();

        #endregion
    }

    public class REComponent : REItem
    {
        #region Public Vars

        public Component Target;

        public List<REField> Fields = new List<REField>();

        #endregion
    }

    public class REField
    {
        #region Public Vars

        public string FieldName;

        public REComponent Parent;

        public OSCMatchPattern MatchPattern;

        public List<OSCValue> Values
        {
            get
            {
                if (_server)
                {
                    return OSCSerializer.Pack(ReflectionValue);
                }

                return _values;
            }
            set
            {
                if (_server)
                {
                    ReflectionValue = OSCSerializer.Unpack(value, _valueType);
                }
                else
                {
                    _values = value;
                }
            }
        }

        public object ReflectionValue
        {
            get 
            {
                if (_fieldInfo == null && _propertyInfo == null)
                    throw new Exception("You can use Value only with OSCFieldRemote generated by OSCRemoteEditorClient.");

                if (_fieldInfo != null)
                    return _fieldInfo.GetValue(Parent.Target);

                return _getMethod.Invoke(Parent.Target, null);
            }
            set
            {
                if (_fieldInfo == null && _propertyInfo == null)
                    throw new Exception("You can use Value only with OSCFieldRemote generated by OSCRemoteEditorClient.");

                if (_fieldInfo != null)
                    _fieldInfo.SetValue(Parent.Target, value);
                else
                    _setMethod.Invoke(Parent.Target, new object[] { value });
            }
        }

        #endregion

        #region Private Vars

        private List<OSCValue> _values = new List<OSCValue>();

        private FieldInfo _fieldInfo;

        private PropertyInfo _propertyInfo;

        private MethodInfo _setMethod;

        private MethodInfo _getMethod;

        private Type _valueType;

        private bool _server;

        #endregion

        #region Public Methods

        // CLIENT
        public REField()
        { }

        // SERVER
        public REField(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
            _valueType = fieldInfo.FieldType;
            _server = true;
        }

        public REField(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
            _valueType = propertyInfo.PropertyType;
            _server = true;

            _setMethod = propertyInfo.GetSetMethod();
            _getMethod = propertyInfo.GetGetMethod();
        }

        #endregion
    }
}