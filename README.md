# iCarus
predict-sync-compensate experiment

TODO
====
2017/5/1
happy worker's day
- [ ] player simple login phase, choose name/server/color etc
- [ ] player add/remove broadcast to every one(player sync)
- [ ] movement(kinematic) predict will cause the avatar to jitter at the end of input sequence, find out why
- [ ] SyncManagerClient.Simulate(Lerping) probably should run in render update, which can result in a more smoothly replay of avatar action
- [ ] deal with player disconnect
- [ ] connect to server with more than 2 clients, test it
- [ ] finish and test reconnect feature
- [ ] looks like message built using flatbuffer is very large in bytes though the information it contains is not too much, maybe i use it wrong, check this problem
- [ ] improve SharpFlatBuffer lib, it just sucks now

2017/4/23
got no idea when can i finish this, can only work on this one day per week
1. finish client-side game state machine
2. keep using ConnectionApproval feature, but do not deny player connection now, add a auth stage to verify player.
3. refactor code mostly
TODO:
~~player manager sync, which only requre event sync, make server.syncmanager to support it~~
- [x] finish avatar creation and connect player to avatar
- [x] basic avatar movement sync

2017/4/16
- [x] finish game state machine
- [x] UdpListener expose onIncommingConnection/onConnectionStatusChanged for it can be modify from outside
- [x] implement server side player state machine
- [x] don't use ConnectionApproval feature, implement a simple verification step in VerifyIdentity phase
- [x] test make sure both state machine works
