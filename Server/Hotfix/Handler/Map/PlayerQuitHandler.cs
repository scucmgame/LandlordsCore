﻿using System.Threading.Tasks;
using Model;

namespace Hotfix
{
    [ActorMessageHandler(AppType.Map)]
    public class MapPlayerQuitHandler : AMActorHandler<Room, PlayerQuit>
    {
        protected override Task Run(Room entity, PlayerQuit message)
        {
            Gamer gamer = entity.Get(message.PlayerID);
            if (gamer != null)
            {
                if (entity.State == RoomState.Game)
                {
                    //玩家操作设置为自动
                    Log.Info($"游戏中，玩家{message.PlayerID}退出房间，切换为自动模式");
                    gamer.isOffline = true;
                    if (gamer.GetComponent<AutoPlayCardsComponent>() == null)
                    {
                        gamer.AddComponent<AutoPlayCardsComponent, Room>(entity);
                    }
                }
                else
                {
                    //房间移除玩家
                    entity.Remove(gamer.Id);

                    //同步匹配服务器移除玩家
                    MapHelper.SendMessage(new GamerQuitRoom() { PlayerID = message.PlayerID, RoomID = entity.Id });

                    //消息广播给其他人
                    entity.Broadcast(new GamerOut() { PlayerID = message.PlayerID });
                    Log.Info($"准备中，玩家{message.PlayerID}退出房间");
                }
            }

            return Task.CompletedTask;
        }
    }
}
