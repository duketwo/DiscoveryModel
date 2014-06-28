using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveModel
{
	public class EveObject : IDisposable
	{
		/// <summary>
		/// Holds the cache of objects
		/// </summary>
		Dictionary<string, EveObject> _attributes;
		string _name;
		bool _addedToCache;
		
		internal EveObject()
		{
			_attributes = new Dictionary<string, EveObject>();
		}
		/// <summary>
		/// Constructs an EVE object
		/// </summary>
		/// <param name="ptr">Address of object</param>
		/// <param name="name">Name used in caching</param>
		/// <param name="addToGlobalCache">Whether or not this object should be cached, defaults to true</param>
		public EveObject(IntPtr ptr, string name, bool addToGlobalCache = true)
		{
			PointerToObject = ptr;
			_attributes = new Dictionary<string, EveObject>();
			_name = name ?? PointerToObject.ToString();
			if(name == null && IsValid) {
//				Frame.Log("[EveObject] DEBUG: NAME == NULL");
				if (IsValid && addToGlobalCache && !Frame.Client.Objects.ContainsKey(ptr.ToString())) {
					Frame.Client.Objects.Add(ptr.ToString(), this);
					_addedToCache = addToGlobalCache;
				}
				
			} else {
				if (IsValid && addToGlobalCache && !Frame.Client.Objects.ContainsKey(_name)) {
					
					Frame.Client.Objects.Add(_name, this);
					_addedToCache = addToGlobalCache;
				}
			}
			
		}
		
		public virtual IntPtr PointerToObject { get; set; }
		
		PyType? _objectType;
		/// <summary>
		/// Returns the object type see <see cref="PyType"/>
		/// </summary>
		public PyType ObjectType
		{
			get
			{
				if (!_objectType.HasValue)
				{
					//EveModel.Frame.Log("[ObjectType]" + " PointerToObject "  + this.PointerToObject.ToString());
					_objectType = PyCall.GetEveObjectType(PointerToObject, false);
				}
				return _objectType.Value;
			}
		}
		
		EveObject ParentObject { get; set; }
		
		
		
		/// <summary>
		/// Gets or sets a child property of the current object
		/// </summary>
		/// <param name="attributeName">Name used in caching, will get prefaced with parentname</param>
		/// <param name="addToCache">Whether or not this object should be cached, defaults to true</param>
		/// <returns></returns>
		public 	EveObject this[string attributeName, bool addToCache = true]
		{
			get
			{
				if (!this.IsValid)
					return null;
				EveObject returnObject;
				string attName = (this._name ?? this.PointerToObject.ToString()) + "." + attributeName;
				if (!_attributes.TryGetValue(attName, out returnObject))
				{
					
					bool valid = (PyCall.PyObject_HasAttrString(this.PointerToObject, attributeName) == 1) ? true : false;
					
					if(valid) {
						//Frame.Log("isValid: " + valid.ToString() + " attributeName: "  + attributeName + " attName: " + attName);
						returnObject = new EveObject(PyCall.PyObject_GetAttrString(this.PointerToObject, attributeName), attName, addToCache);
						_attributes.Add(attName, returnObject);
					} else {
						
						Frame.Log("[EveObject-this] valid: " + valid.ToString() + " attributeName: "  + attributeName + " attName: " + attName);
						returnObject = new EveObject(IntPtr.Zero,null,false);
					}
					
					if (PyCall.PyErr_Occurred() != IntPtr.Zero) {
						Frame.Log("[EveObject-this] PyCall.PyErr_Occurred() != IntPtr.Zero -> PyCall.PyErr_Clear(); attributeName: " + attributeName + " attName: " + attName);
						PyCall.PyErr_Clear();
					}
					
					
					
				}
				return returnObject;
			}
			set
			{
				string attName = (this._name ?? this.PointerToObject.ToString()) + "." + attributeName;
				if (_attributes.ContainsKey(attName))
					_attributes[attName] = value;
				else
					_attributes.Add(attName, value);
			}
		}
		
		
		
		
		
		/// <summary>
		/// Returns the value of the object or an 0object reference
		/// </summary>
		/// <typeparam name="T">Type of value you expect the object to hold, eg. string, double, int etc.</typeparam>
		/// <returns></returns>
		public T GetValueAs<T>()
		{
			object val = null;
			if (!this.IsValid)
			{
				return default(T);
			}
			if (typeof(T) == typeof(double))
			{
				val = PyCall.PyFloat_AsDouble(PointerToObject);
			}
			if (typeof(T) == typeof(flag))
			{
				val = (flag)PyCall.PyLong_AsLong(PointerToObject);
			}
			if (typeof(T) == typeof(DateTime))
			{
				DateTime time = new DateTime(0x641, 1, 1);
				val = time.AddMilliseconds(((double)PyCall.PyLong_AsLongLong(PointerToObject)) / 10000.0);
			}
			if (typeof(T) == typeof(int))
			{
				val = (int)PyCall.PyLong_AsLong(PointerToObject);
			}
			if (typeof(T) == typeof(long))
			{
				val = (long)PyCall.PyLong_AsLongLong(PointerToObject);
				if ((long)val == -1)
					PyCall.PyErr_Clear();
			}
			if (typeof(T) == typeof(bool))
			{
				val = (PyCall.PyLong_AsLong(PointerToObject) == 1);
			}
			if (typeof(T) == typeof(string))
			{
				if (ObjectType == PyType.UnicodeType)
				{
					int len = PyCall.PyUnicodeUCS2_GetSize(PointerToObject);
					if (len <= 0)
					{
						val = null;
					}
					else
					{
						IntPtr strPtr = PyCall.PyUnicodeUCS2_AsUnicode(PointerToObject);
						if (strPtr == IntPtr.Zero)
						{
							val = null;
						}
						else
						{
							val = System.Runtime.InteropServices.Marshal.PtrToStringUni(strPtr, len);
						}
					}
				}
				else
				{
					int len = PyCall.PyString_Size(PointerToObject);
					if (len <= 0)
					{
						val = null;
					}
					else
					{
						IntPtr strPtr = PyCall.PyString_AsString(PointerToObject);
						if (strPtr == IntPtr.Zero)
						{
							val = null;
						}
						else
						{
							val = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(strPtr, len);
						}
					}
				}
			}
			if (typeof(T) == typeof(EveObject))
			{
				val = new EveObject(PointerToObject, null, true);
			}
			if (typeof(T) == typeof(EveOrder)){
				val = new EveOrder(PointerToObject);
			}
			if (PyCall.PyErr_Occurred() != IntPtr.Zero)
			{
				PyCall.PyErr_Clear();
			}
			if (val == null)
				return default(T);
			else
				return (T)val;
		}
		/// <summary>
		/// Gets the current object as a Dictionary where the key is determined by T
		/// </summary>
		/// <typeparam name="T">Type of the key in the Dictionary</typeparam>
		/// <returns></returns>
		public Dictionary<T, EveObject> GetDictionary<T>()
		{
			Dictionary<T, EveObject> dictionary = new Dictionary<T, EveObject>();
			Dictionary<T, EveObject> result;
			if (!this.IsValid)
			{
				result = dictionary;
			}
			else
			{
				EveObject objList = this.CallMethod("keys", new object[0]);
				if (objList != null)
				{
					List<EveObject> list = objList.GetList<EveObject>();
					foreach (EveObject current in list)
					{
						dictionary[current.GetValueAs<T>()] = new EveObject(PyCall.PyDict_GetItem(this.PointerToObject, current.PointerToObject), null, false);
					}
				}
				result = dictionary;
			}
			if (PyCall.PyErr_Occurred() != IntPtr.Zero)
			{
				PyCall.PyErr_Clear();
			}
			return result;
		}
		
		public void Expose()
		{
			IntPtr ptr = PyCall.PyObject_Dir(this.PointerToObject);
			EveObject obj = new EveObject(ptr,null,false);
			if(obj.IsValid){
				List<string> exp = obj.GetList<string>();
				foreach(string e in exp) {
					Frame.Log(e);
				}
				
			}
			if (PyCall.PyErr_Occurred() != IntPtr.Zero)
			{
				Frame.Log("[Expose] - error occured");
				PyCall.PyErr_Clear();
			}
			
		}
		
		
		private List<string> ExposeAsList(){
			
			List<string> exp = new List<string>();
			IntPtr ptr = PyCall.PyObject_Dir(this.PointerToObject);
			EveObject obj = new EveObject(ptr,null,false);
			if(obj.IsValid){
				exp = obj.GetList<string>();
			}
			if (PyCall.PyErr_Occurred() != IntPtr.Zero)
			{
				Frame.Log("[Expose] - error occured");
				PyCall.PyErr_Clear();
			}
			return exp;
		}
		
		public List<string> Expose(string attributes, IntPtr basePointer = default(IntPtr), bool printToLog = true){
			try {
				
				string[] attr = { };
				if(!string.IsNullOrEmpty(attributes))
					attr = attributes.Split('.');
				
				
				EveObject obj =null;
				if(basePointer != IntPtr.Zero){
					obj = new EveObject(basePointer,null,false);
				}
				EveObject ret =null;
				foreach(string att in attr){
					
					//if(obj == null)obj = Import(att);
					if(obj == null)obj = new EveObject(this.PointerToObject,null,false);
					if(obj != null) {
						
						bool valid = (PyCall.PyObject_HasAttrString(obj.PointerToObject, att) == 1) ? true : false;
						if(valid){
							if(printToLog)Frame.Log("isValid => " + att);
							ret = new EveObject(PyCall.PyObject_GetAttrString(obj.PointerToObject,att),null,false);
							if(ret.IsValid) {
								obj = ret;
							}
						}
						
						
					}
				}
				if(printToLog)obj.Expose();
				return obj.ExposeAsList();
				
			} catch (Exception e) {
				
				if (PyCall.PyErr_Occurred() != IntPtr.Zero)
				{
					Frame.Log("[Expose] - error occured");
					PyCall.PyErr_Clear();
				}
				Frame.Log("[Expose] Exception " + e);
				return new List<string>();
				
			}
		}
		
		
		internal EveObject Import(string module){
			EveObject obj;
			if (!_attributes.TryGetValue(module, out obj))
			{
				obj = new EveObject(PyCall.PyImport_ImportModule(module), module);
			}
			return obj;
		}
		
		/// <summary>
		/// Gets the current object as a List of type T
		/// </summary>
		/// <typeparam name="T">The type of objects found in the List</typeparam>
		/// <returns></returns>
		/// 
		
		
		public List<T> GetList<T>()
		{
			List<T> list = new List<T>();
			List<T> result;
			if (!this.IsValid)
			{
				result = list;
			}
			else
			{
				int num = PyCall.PyList_Size(this.PointerToObject);
				for (int i = 0; i < num; i++)
				{
					EveObject item = new EveObject(PyCall.PyList_GetItem(this.PointerToObject, i), null, false);
					object obj = null;
					if (typeof(T) == typeof(flag))
					{
						obj = (flag)item.GetValueAs<int>();
					}
					if (typeof(T) == typeof(int))
					{
						obj = item.GetValueAs<int>();
					}
					else if (typeof(T) == typeof(long))
					{
						obj = item.GetValueAs<long>();
					}
					else if (typeof(T) == typeof(double))
					{
						obj = item.GetValueAs<double>();
					}
					else if (typeof(T) == typeof(string))
					{
						obj = item.GetValueAs<string>();
					}
					else if (typeof(T) == typeof(EveObject))
					{
						obj = item;
					}
					if (obj != null)
					{
						list.Add((T)obj);
					}
				}
				result = list;
			}
			if (PyCall.PyErr_Occurred() != IntPtr.Zero)
			{
				PyCall.PyErr_Clear();
			}
			return result;
		}
		/// <summary>
		/// Gets a List from a Python Typle type
		/// </summary>
		/// <typeparam name="T">The type you expect to find in the Tuple</typeparam>
		/// <returns></returns>
		public List<T> GetListFromTuple<T>()
		{
			List<T> list = new List<T>();
			List<T> result;
			if (!this.IsValid)
			{
				result = list;
			}
			else
			{
				int num = PyCall.PyTuple_Size(this.PointerToObject);
				for (int i = 0; i < num; i++)
				{
					EveObject item = new EveObject(PyCall.PyTuple_GetItem(this.PointerToObject, i), null, false);
					object obj = null;
					if (typeof(T) == typeof(flag))
					{
						obj = (flag)item.GetValueAs<int>();
					}
					if (typeof(T) == typeof(int))
					{
						obj = item.GetValueAs<int>();
					}
					else if (typeof(T) == typeof(long))
					{
						obj = item.GetValueAs<long>();
					}
					else if (typeof(T) == typeof(double))
					{
						obj = item.GetValueAs<double>();
					}
					else if (typeof(T) == typeof(string))
					{
						obj = item.GetValueAs<string>();
					}
					else if (typeof(T) == typeof(EveObject))
					{
						obj = item;
					}
					if (obj != null)
					{
						list.Add((T)obj);
					}
				}
				result = list;
			}
			if (PyCall.PyErr_Occurred() != IntPtr.Zero)
			{
				PyCall.PyErr_Clear();
			}
			return result;
		}
		/// <summary>
		/// Calls a method on the object
		/// </summary>
		/// <param name="name">Name of the method</param>
		/// <param name="parameters">An array of values you want the pass as parameters to the method</param>
		/// <param name="useNewthread">Determines if the method call should be done asynchronous</param>
		/// <param name="args">For passing a Dictionary as paramter to the funtion</param>
		/// <returns></returns>
		public EveObject CallMethod(string name, object[] parameters, bool useNewthread = false, Dictionary<string, object> args = null)
		{
			EveObject cleanupParam, cleanupMethod;
			
			
			if (!this.IsValid || this.IsNone)
			{
				Frame.Log("[ERROR-EveObject-CallMethod] - !this.IsValid || this.IsNone");
				return null;
			}
			
			bool hasAttrString = (PyCall.PyObject_HasAttrString(this.PointerToObject, name) == 1) ? true : false;
			bool callable = (PyCall.PyCallable_Check(this[name].PointerToObject) == 1) ? true : false;
			
			if(!hasAttrString) {
				Frame.Log("[ERROR-EveObject-CallMethod] - PyObject_HasAttrString == false");
				return null;
			}
			
			if(!callable) {
				Frame.Log("[ERROR-EveObject-CallMethod] - PyCallable_Check == false");
				return null;
			}
			
			IntPtr methodPtr = PyCall.PyObject_GetAttrString(this.PointerToObject, name);
			
			
			if (PyCall.PyErr_Occurred() != IntPtr.Zero) {
				Frame.Log("[ERROR-EveObject-CallMethod] PyCall.PyErr_Occurred() != IntPtr.Zero -> PyCall.PyErr_Clear(); ---- name: " + name);
				PyCall.PyErr_Clear();
				foreach(object obj in parameters){
					Frame.Log(obj.ToString());
				}
			}
			if (methodPtr == IntPtr.Zero) {
				Frame.Log("(methodPtr == IntPtr.Zero) => return null");
				return null;
			}
			
			cleanupMethod = new EveObject(methodPtr, null);
			
			if (useNewthread)
			{
				object[] paramConcat = new object[] { methodPtr }.Concat<object>(parameters).ToArray<object>();
				Frame.Client.Builtin["uicore"]["uilib"].CallMethod("RegisterAppEventTime", new object[0]);
				return Frame.Client.UThread.CallMethod("new", paramConcat, false, args);
			}
			
			int paramCount = parameters.Count();
			IntPtr paramPtr = IntPtr.Zero;
			switch (paramCount)
			{
				case 0:
					paramPtr = PyCall.Py_BuildValue_Params0("()");
					break;
				case 1:
					paramPtr = PyCall.Py_BuildValue_Params1("(" + "O" + ")", BuildParam(parameters[0]));
					break;
				case 2:
					paramPtr = PyCall.Py_BuildValue_Params2("(" + "OO" + ")", BuildParam(parameters[0]), BuildParam(parameters[1]));
					break;
				case 3:
					paramPtr = PyCall.Py_BuildValue_Params3("(" + "OOO" + ")", BuildParam(parameters[0]), BuildParam(parameters[1]), BuildParam(parameters[2]));
					break;
				case 4:
					paramPtr = PyCall.Py_BuildValue_Params4("(" + "OOOO" + ")", BuildParam(parameters[0]), BuildParam(parameters[1]), BuildParam(parameters[2]), BuildParam(parameters[3]));
					break;
				case 5:
					paramPtr = PyCall.Py_BuildValue_Params5("(" + "OOOOO" + ")", BuildParam(parameters[0]), BuildParam(parameters[1]), BuildParam(parameters[2]), BuildParam(parameters[3]), BuildParam(parameters[4]));
					break;
				case 6:
					paramPtr = PyCall.Py_BuildValue_Params6("(" + "OOOOOO" + ")", BuildParam(parameters[0]), BuildParam(parameters[1]), BuildParam(parameters[2]), BuildParam(parameters[3]), BuildParam(parameters[4]), BuildParam(parameters[5]));
					break;
			}
			cleanupParam = new EveObject(paramPtr, null);
			
			EveObject pydict;
			if (args == null || args.Count == 0)
				pydict = new EveObject(IntPtr.Zero, null, true);
			else
			{
				pydict = new EveObject(PyCall.PyDict_New(), null, true);
				foreach (var item in args)
				{
					IntPtr paramValue = BuildParam(item.Value);
					EveObject paramObject = new EveObject(paramValue, null);
					PyCall.Py_IncRef(paramValue);
					PyCall.PyDict_SetItem(pydict.PointerToObject, new EveObject(PyCall.PyString_FromString(item.Key), null, true).PointerToObject, paramValue);
				}
			}
			if (PyCall.PyErr_Occurred() != IntPtr.Zero)
			{
				PyCall.PyErr_Clear();
			}
			return new EveObject(PyCall.PyEval_CallObjectWithKeywords(methodPtr, paramPtr, pydict.PointerToObject), null, true);
			
		}
		/// <summary>
		/// Builds the parameters for a method call
		/// </summary>
		/// <param name="obj">The object you wish to pass to the method</param>
		/// <returns>An IntPtr for the param</returns>
		IntPtr BuildParam(object obj)
		{
			EveObject cleanupParam = new EveObject(IntPtr.Zero, null, false);
			if (obj is IEnumerable<object>)
			{
				IEnumerable<object> list = obj as IEnumerable<object>;
				cleanupParam = new EveObject(PyCall.PyList_New(list.Count()), null, false);
				int i = 0;
				foreach (var item in list)
				{
					var listItem = BuildParam(item);
					PyCall.Py_IncRef(listItem);
					PyCall.PyList_SetItem(cleanupParam.PointerToObject, i, listItem);
					i++;
				}
			}
			else if (obj is bool)
			{
				if ((bool)obj)
				{
					cleanupParam = new EveObject(PyCall.PyBool_FromLong(1), null);
				}
				else
				{
					cleanupParam = new EveObject(PyCall.PyBool_FromLong(0), null);
				}
			}
			else if (obj is string)
			{
				cleanupParam = new EveObject(PyCall.PyString_FromString(obj as string), null);
			}
			else if (obj is IntPtr)
			{
				cleanupParam = new EveObject((IntPtr)obj, null);
			}
			else if (obj is long)
			{
				cleanupParam = new EveObject(PyCall.PyLong_FromLongLong((long)obj), null);
			}
			else if (obj is int)
			{
				cleanupParam = new EveObject(PyCall.PyLong_FromLong((int)obj), null);
			}
			else if (obj is double)
			{
				cleanupParam = new EveObject(PyCall.PyFloat_FromDouble((double)obj), null);
			}
			else if (obj is EveObject)
			{
				cleanupParam = ((EveObject)obj);
			}
			return cleanupParam.PointerToObject;
		}
		/// <summary>
		/// Returns true if this Pointer == IntPTr.Zero OR this != null
		/// gotta fix that, this should  include python type_none
		/// </summary>
		public bool IsValid {
			get {
				bool isValid = (this != null && this.PointerToObject != IntPtr.Zero);
				return  isValid;
			}
		}
		
		public bool IsNone {
			get {
				return this.ObjectType == PyType.NoneType;
			}
		}
		public bool HasAttrString(string name)  {
			
			return PyCall.PyObject_HasAttrString(this.PointerToObject, name) == 1 ? true : false;
			
		}
		
		public bool NotValidOrNone {
			get {
				return !this.IsValid || this.IsNone;
			}
		}
		
		
		public bool SetValueAs<T>(string attribute, T value)
		{
			EveObject eveObject = new EveObject(IntPtr.Zero, null, false);
			object obj = value;
			
			if (obj is bool)
			{
				if ((bool)obj)
				{
					eveObject = new EveObject(PyCall.PyBool_FromLong(1), null);
				}
				else
				{
					eveObject = new EveObject(PyCall.PyBool_FromLong(0), null);
				}
			}
			else if (obj is string)
			{
				eveObject = new EveObject(PyCall.PyString_FromString(obj as string), null);
			}
			else if (obj is IntPtr)
			{
				eveObject = new EveObject((IntPtr)obj, null);
			}
			else if (obj is long)
			{
				eveObject = new EveObject(PyCall.PyLong_FromLongLong((long)obj), null);
			}
			else if (obj is int)
			{
				eveObject = new EveObject(PyCall.PyLong_FromLong((int)obj), null);
			}
			else if (obj is double)
			{
				eveObject = new EveObject(PyCall.PyFloat_FromDouble((double)obj), null);
			}
			else if (obj is EveObject)
			{
				eveObject = ((EveObject)obj);
			}
			
			bool result = false;
			try
			{
				if (this.IsValid && eveObject.IsValid)
				{
					result = (PyCall.PyObject_SetAttrString(this.PointerToObject, attribute, eveObject.PointerToObject) != -1);
				}
				else
				{
					result = false;
				}
			} finally {
				
				if (PyCall.PyErr_Occurred() != IntPtr.Zero)
				{
					PyCall.PyErr_Clear();
				}
				
			}
			return result;
		}
		
		bool _isDisposed;
		public void Dispose()
		{
			if (_isDisposed)
				return;
			_isDisposed = true;
			foreach (var item in _attributes)
			{
				item.Value.Dispose();
			}
			_attributes = null;
			if (PointerToObject != IntPtr.Zero)
			{
				PyCall.Py_DecRef(PointerToObject);
			}
			PointerToObject = IntPtr.Zero;
		}
		
		public override string ToString()
		{
			return this._name;
		}
	}
}
