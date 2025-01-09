using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModdedModesAPI.ModesAPI
{
	/// <summary>
	/// This class is an extra feature from this mod, to help creating custom modes into the game. It contains a few methods that are useful when, for example, you want to modify a <see cref="BaseGameManager"/> object.
	/// </summary>
	public static class ReflectionExtensions
	{
		static bool IsInheritFromType<T, T2>() =>
			typeof(T).IsSubclassOf(typeof(T2)) || typeof(T) == typeof(T2);
		/// <summary>
		/// Gathers all the fields and properties values from <typeparamref name="C"/> and insert into <typeparamref name="T"/>.
		/// <para><strong><typeparamref name="T"/> must inherit from <typeparamref name="C"/> or be type <typeparamref name="C"/> itself.</strong></para>
		/// </summary>
		/// <typeparam name="T">A type that must inherits from <see cref="MonoBehaviour"/>.</typeparam>
		/// <typeparam name="C">A type that must inherits from <see cref="MonoBehaviour"/>.</typeparam>
		/// <param name="original">The instance to get the copied values.</param>
		/// <param name="toCopyFrom">The instance that'll send the copy the values.</param>
		/// <returns>The same instance of type <typeparamref name="T"/>.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static T GetACopyFromFields<T, C>(this T original, C toCopyFrom) where T : MonoBehaviour where C : MonoBehaviour
		{
			if (!IsInheritFromType<T, C>())
			{
				throw new ArgumentException($"Type T ({typeof(T).FullName}) does not inherit from type C ({typeof(C).FullName})");
			}

			List<Type> typesToFollow = [typeof(C)];
			Type t = typeof(C);

			while (true)
			{
				t = t.BaseType;

				if (t == null || t == typeof(MonoBehaviour))
				{
					break;
				}

				typesToFollow.Add(t);
			}

			foreach (var ty in typesToFollow)
			{
				foreach (FieldInfo fieldInfo in AccessTools.GetDeclaredFields(ty))
				{
					fieldInfo.SetValue(original, fieldInfo.GetValue(toCopyFrom));
				}
			}

			return original;
		}
		/// <summary>
		/// This method will literally replace a component with another, <strong>as long as <typeparamref name="T"/> inherits from <typeparamref name="C"/> or it's of type <typeparamref name="C"/></strong>.
		/// </summary>
		/// <typeparam name="T">A type that must inherits from <see cref="MonoBehaviour"/>.</typeparam>
		/// <typeparam name="C">A type that must inherits from <see cref="MonoBehaviour"/>.</typeparam>
		/// <param name="toReplace">The instance to be replaced with.</param>
		/// <returns>A new instance of type <typeparamref name="T"/>.</returns>
		public static T ReplaceComponent<T, C>(this C toReplace) where T : MonoBehaviour where C : MonoBehaviour
		{
			var toExist = toReplace.gameObject.AddComponent<T>();
			toExist.GetACopyFromFields(toReplace);
			UnityEngine.Object.Destroy(toReplace);
			return toExist;
		}
	}
}
