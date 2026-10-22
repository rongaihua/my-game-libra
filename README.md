# my-game-libra

局域网游戏的共用管理器。纯 C#，不依赖 Unity，客户端和房间进程都可以引用。

| 类 | 职责 |
|---|---|
| `App` | 创建并驱动各个管理器 |
| `Module` | 创建、初始化、每帧更新、销毁 |
| `EventMgr` | 按消息类型监听和派发 |
| `TimerMgr` | 毫秒计时，可重复、可取消 |
| `ObjectMgr` | 按类型取出和归还对象 |
| `RoomMgr` | 房间内玩家名单和位置 |
| `NetMgr` | 局域网 TCP 收发 |

```csharp
var app = new App();
app.Init();
app.Net.StartServer(29000);
app.Room.SetLocalPlayer(1, "host");

// 每帧
app.Update(16);
```
