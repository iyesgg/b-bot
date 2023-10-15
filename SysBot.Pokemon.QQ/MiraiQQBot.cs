﻿using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Modules;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SysBot.Pokemon.QQ
{
    public class MiraiQQBot<T> where T : PKM, new()
    {
        private static PokeTradeHub<T> Hub = default!;

        internal static TradeQueueInfo<T> Info => Hub.Queues.Info;
        private readonly MiraiBot Client;
        internal static QQSettings Settings = default!;   

        /// <summary>
        /// QQBot初始化
        /// </summary>
        public MiraiQQBot(QQSettings settings, PokeTradeHub<T> hub)
        {
            Settings = settings;
            Hub = hub;
            Client = new MiraiBot
            {
                Address = settings.Address,
                QQ = settings.QQ,
                VerifyKey = settings.VerifyKey
            };

            var modules = new List<IModule>()
            {
                new AliveModule<T>(),
                new CommandModule<T>(),
                new FileModule<T>(),
                new DetectionModule<T>(),
                new PsModule<T>()
            };
            Client.MessageReceived.SubscribeGroupMessage(receiver =>
            {           
                if (receiver.GroupId == Settings.GroupId)
                    modules.Raise(receiver);
            });

            var GroupId = Settings.GroupId;
            Task.Run(async () =>
            {
                try
                {
                    await Client.LaunchAsync();

                    if (!string.IsNullOrWhiteSpace(Settings.MessageStart))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, Settings.MessageStart);
                        await Task.Delay(1_000).ConfigureAwait(false);
                    }

                    if (typeof(T) == typeof(PK8))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, "当前版本为剑盾");
                    }
                    else if (typeof(T) == typeof(PB8))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, "当前版本为晶灿钻石明亮珍珠");
                    }
                    else if (typeof(T) == typeof(PA8))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, "当前版本为阿尔宙斯");
                    }
                    else if (typeof(T) == typeof(PK9))
                    {
                        await MessageManager.SendGroupMessageAsync(GroupId, "当前版本为朱紫");
                    }

                    await Task.Delay(1_000).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    LogUtil.LogError(ex.Message, nameof(MiraiQQBot<T>));
                }
            });
        }

        /// <summary>
        /// 发送群组消息
        /// </summary>
        /// <param name="mc"></param>
        public async static void SendGroupMessage(MessageChain mc)
        {
            if (string.IsNullOrEmpty(Settings.GroupId)) return;
            await MessageManager.SendGroupMessageAsync(Settings.GroupId, mc);
        }

        /// <summary>
        /// 发送好友消息
        /// </summary>
        /// <returns></returns>
        public async static void SendFriendMessage(string friendId, MessageChain mc)
        {
            if (string.IsNullOrEmpty(friendId) || string.IsNullOrEmpty(Settings.GroupId)) return;
            await MessageManager.SendFriendMessageAsync(friendId, mc);
        }

        /// <summary>
        /// 发送临时会话
        /// </summary>
        public async static void SendTempMessage(string tempId, MessageChain mc)
        {
            if (string.IsNullOrEmpty(tempId) || string.IsNullOrEmpty(Settings.GroupId)) return;
            await MessageManager.SendTempMessageAsync(tempId, Settings.GroupId, mc);
        }
    }
}
