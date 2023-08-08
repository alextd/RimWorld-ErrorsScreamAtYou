using System;
using System.Reflection;
using System.Linq;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace ErrorsScreamAtYou
{
	public class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content) =>
			new Harmony("Uuugggg.rimworld.ErrorsScreamAtYou.main").PatchAll();
	}


	[HarmonyPatch(typeof(Log), nameof(Log.Error), new Type[] { typeof(string)})]
	public static class ScreamAtYou
	{
		// Not a defof so this works while loading before DefOfs are bound
		public static SoundDef _TD_Scream;
		public static SoundDef TD_Scream
		{
			get
			{
				if (_TD_Scream == null)
					_TD_Scream = DefDatabase<SoundDef>.GetNamed("TD_Scream", false);

				return _TD_Scream;
			}
		}
		public static void Postfix()
		{
			TD_Scream?.PlayOneShotOnCamera();
		}
	}
}