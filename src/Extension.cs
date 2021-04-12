using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using MessagePack;
using ParadoxNotion.Serialization;

using HarmonyLib;

namespace MovUrAcc
{
	public static partial class Extension
	{
		internal static string GetFullname(this ChaControl _chaCtrl) => _chaCtrl?.chaFile?.parameter?.fullname?.Trim();

		public static object RefTryGetValue(this object _self, object _key)
		{
			if (_self == null) return null;

			MethodInfo tryMethod = AccessTools.Method(_self.GetType(), "TryGetValue");
			object[] parameters = new object[] { _key, null };
			tryMethod.Invoke(_self, parameters);
			return parameters[1];
		}

		public static object RefElementAt(this object _self, int _key)
		{
			if (_self == null)
				return null;
			if (_key > (Traverse.Create(_self).Property("Count").GetValue<int>() - 1))
				return null;

			return Traverse.Create(_self).Method("get_Item", new object[] { _key }).GetValue();
		}

		public static object JsonClone(this object _self)
		{
			if (_self == null)
				return null;
			string json = JSONSerializer.Serialize(_self.GetType(), _self);
			return JSONSerializer.Deserialize(_self.GetType(), json);
		}

		public static T MessagepackClone<T>(T _self)
		{
			byte[] _byte = MessagePackSerializer.Serialize(_self);
			return MessagePackSerializer.Deserialize<T>(_byte);
		}
	}
}
