﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.GameData;

namespace RidgesideVillage.Questing
{
    internal class RSVSpecialOrderBoard : SpecialOrdersBoard
    {

		const string NINJABOARDNAME = "RSVNinjaSO";
		const string RSVBOARDNAME = "RSVTownSO";
		int timestampOpened;
		static int safetyTimer = 500;



		internal RSVSpecialOrderBoard(string boardType = "") : base(boardType)
		{
			timestampOpened = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			Texture2D billboardTexture;
            if (boardType.Equals(NINJABOARDNAME)){
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\RSVNinjaSOBoard");

			}else if (boardType.Equals(RSVBOARDNAME)){
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\RSVTownSO");
            }
            else
            {
				billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SpecialOrdersBoard");
			}
			//Helper.Reflection.GetField<Texture2D>(this, "billboardTexture").SetValue(billboardTexture); //throws NRE; check later

		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (this.timestampOpened + safetyTimer < Game1.currentGameTime.TotalGameTime.TotalMilliseconds)
			{
				base.receiveRightClick(x, y, playSound);
			}
			return;
		}


		//need this public for DailySpecialOrders :p
		//mostly copied from the decompile
		//randomly chooses 2 SOs for each RSV board
		public static void UpdateAvailableRSVSpecialOrders(bool force_refresh)
		{
			if (Game1.player.team.availableSpecialOrders is not null)
			{
				foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
				{
					if ((order.questDuration.Value == SpecialOrder.QuestDuration.TwoDays || order.questDuration.Value == SpecialOrder.QuestDuration.ThreeDays) && !Game1.player.team.acceptedSpecialOrderTypes.Contains(order.orderType.Value))
					{
						order.SetDuration(order.questDuration.Value);
					}
				}
			}
			if (Game1.player.team.availableSpecialOrders.Count > 0 && !force_refresh)
			{
				return;
			}

			Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
			List<string> keys = new List<string>(order_data.Keys);

			for (int k = 0; k < keys.Count; k++)
			{
				string key = keys[k];
				bool invalid = false;
				if (!invalid && order_data[key].Repeatable != "True" && Game1.MasterPlayer.team.completedSpecialOrders.ContainsKey(key))
				{
					invalid = true;
				}
				if (Game1.dayOfMonth >= 16 && order_data[key].Duration == "Month")
				{
					invalid = true;
				}
				if (!invalid && !SpecialOrder.CheckTags(order_data[key].RequiredTags))
				{
					invalid = true;
				}
				if (!invalid)
				{
					foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
					{
						if ((string)specialOrder.questKey.Value == key)
						{
							invalid = true;
							break;
						}
					}
				}
				Log.Trace($"Order {keys} is valid: {!invalid}");
				if (invalid)
				{
					keys.RemoveAt(k);
					k--;
				}
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)((float)Game1.stats.DaysPlayed * 1.3f));
			string[] array = new string[2] { NINJABOARDNAME, RSVBOARDNAME };
			foreach (string type_to_find in array)
			{
				List<string> typed_keys = new List<string>();
				foreach (string key3 in keys)
				{
					if (order_data[key3].OrderType == type_to_find)
					{
						typed_keys.Add(key3);
					}
				}
				List<string> all_keys = new List<string>(typed_keys);

				for (int j = 0; j < typed_keys.Count; j++)
				{
					if (Game1.player.team.completedSpecialOrders.ContainsKey(typed_keys[j]))
					{
						typed_keys.RemoveAt(j);
						j--;
					}
				}

				for (int i = 0; i < 2; i++)
				{
					if (typed_keys.Count == 0)
					{
						if (all_keys.Count == 0)
						{
							break;
						}
						typed_keys = new List<string>(all_keys);
					}
					int index = r.Next(typed_keys.Count);
					string key2 = typed_keys[index];
					Game1.player.team.availableSpecialOrders.Add(SpecialOrder.GetSpecialOrder(key2, r.Next()));
					typed_keys.Remove(key2);
					all_keys.Remove(key2);
				}
			}

			Log.Trace("Refreshed RSV SpecialOders");
			foreach (var SO in Game1.player.team.availableSpecialOrders)
			{
				Log.Trace($"{SO.questKey.Value}, {SO.orderType.Value}");
			}

		}

	}
}
