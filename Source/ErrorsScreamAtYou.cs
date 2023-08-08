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


	[HarmonyPatch(typeof(Verse.Log), nameof(Verse.Log.Error), new Type[] { typeof(string)})]
	public static class ScreamAtYou
	{
		// Not a defof so this might work while loading before DefOfs are bound
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

		private static string _text;
		private static int _tick;
		public static bool needsErrorScream;
		public static void Postfix(string text)
		{
			try
			{
				// catch recursive errors
				if (text == _text && (Current.Game?.tickManager?.TicksAbs ?? _tick) == _tick)
				{
					Log.Message($"TD_Scream skipping Error {text}");
					return;
				}
				_tick = Current.Game?.tickManager?.TicksAbs ?? 0;
				_text = text;


				if (!StaticConstructorOnStartupUtility.coreStaticAssetsLoaded)
				{
					Log.Message($"Needs Error Scream later (!StaticConstructorOnStartupUtility.coreStaticAssetsLoaded)");
					needsErrorScream = true;
					return;
				}


				if (TD_Scream == null)
				{
					Log.Message($"Needs Error Scream later (TD_Scream == null)");
					needsErrorScream = true;
					return;
				}


				if (!TD_Scream.subSounds.Any(ss=> ss.Duration.max > 0))
				{
					Log.Message($"Needs Error Scream later (subSounds duration 0)");
					needsErrorScream = true;
					return;
				}



				// Okay we should be good.
				Log.Message($"Playing TD_Scream ({TD_Scream})");
				TD_Scream?.PlayOneShotOnCamera();


				if (!Find.SoundRoot.oneShotManager.PlayingOneShots.Any(o => o?.subDef?.parentDef == TD_Scream))
				{
					Log.Message($"Needs Error Scream later (not playing)");
					needsErrorScream = true;
				}
			}
			catch(Exception e)
			{
				Log.Message($"Needs Error Scream later (exception {e})");
				needsErrorScream = true;
			}
		}
	}

	[StaticConstructorOnStartup]
	public static class ErrorOnStartup
	{

		static ErrorOnStartup()
		{
			Log.Message($"ErrorOnStartup: {ScreamAtYou.needsErrorScream}");
			LongEventHandler.QueueLongEvent(delegate
			{
				Log.Message($"Doing Error Scream later: {ScreamAtYou.needsErrorScream}");
				if (ScreamAtYou.needsErrorScream)
				{
					Log.Message($"Playing TD_Scream (later, TD_Scream = {ScreamAtYou.TD_Scream})");
					ScreamAtYou.TD_Scream?.PlayOneShotOnCamera();
				}
			}, null, false, null);
		}
	}
}