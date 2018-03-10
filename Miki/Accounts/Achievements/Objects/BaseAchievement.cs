﻿using Miki.Framework;
using Miki.Common.Interfaces;
using Miki.Accounts.Achievements.Objects;
using Miki.Models;
using System;
using System.Threading.Tasks;
using Miki.Common;

namespace Miki.Accounts.Achievements
{
    public class BaseAchievement
    {
        public string Name { get; set; } = Constants.NotDefined;
        public string ParentName { get; set; } = Constants.NotDefined;

        public string Icon { get; set; } = Constants.NotDefined;
		public int Points { get; set; } = 5;

        public BaseAchievement()
        {
        }
        public BaseAchievement(Action<BaseAchievement> act)
        {
            act.Invoke(this);
        }

        public virtual async Task<bool> CheckAsync(BasePacket packet)
        {
            return true;
        }

        /// <summary>
        /// Unlocks the achievement and if not yet added to the database, It'll add it to the database.
        /// </summary>
        /// <param name="context">sql context</param>
        /// <param name="id">user id</param>
        /// <param name="r">rank set to (optional)</param>
        /// <returns></returns>
        internal async Task UnlockAsync(IDiscordMessageChannel channel, IDiscordUser user, int r = 0)
        {
            long userid = user.Id.ToDbLong();
       
			if (await UnlockIsValid(userid, r))
			{
				Notification.SendAchievement(this, channel, user);
			}
		}
		internal async Task UnlockAsync(IDiscordUser user, int r = 0)
		{
			long userid = user.Id.ToDbLong();

			if (await UnlockIsValid(userid, r))
			{
				Notification.SendAchievement(this, user);
			}
		}

		internal async Task<bool> UnlockIsValid(long userId, int newRank)
		{
			using (var context = new MikiContext())
			{
				var achievement = await context.Achievements.FindAsync(userId, ParentName);

				// If no achievement has been found and want to unlock first
				if (newRank == 0 && achievement == null)
				{
					context.Achievements.Add(new Achievement() { Id = userId, Name = ParentName, Rank = 0 });
				}
				// If achievement we want to unlock is the next achievement
				else if (achievement.Rank == newRank - 1)
				{
					achievement.Rank++;
				}
				else
				{
					return false;
				}

				await context.SaveChangesAsync();
			}
			return true;
		}
	}
}